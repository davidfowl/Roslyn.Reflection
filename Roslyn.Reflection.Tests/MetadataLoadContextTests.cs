using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Types.Data;

namespace Roslyn.Reflection.Tests
{
    public class MetadataLoadContextTests
    {
        [Fact]
        public void CanResolveTypeByName()
        {
            var compilation = CreateBasicCompilation(@"
public class NotAPlugin
{
}

public class Plugin1 : IPlugin { }
public class Plugin2 : IPlugin { }

public interface IPlugin { }

");
            var metadataLoadContext = new MetadataLoadContext(compilation);

            Assert.Equal("something", metadataLoadContext.Assembly.FullName);
            // Resolve the type by name
            var pluginType = metadataLoadContext.ResolveType("IPlugin");

            Assert.NotNull(pluginType);
            Assert.True(pluginType.IsInterface);

            var plugin1 = metadataLoadContext.ResolveType("Plugin1");
            Assert.NotNull(plugin1);
            Assert.True(pluginType.IsAssignableFrom(plugin1));

            var plugin2 = metadataLoadContext.ResolveType("Plugin2");
            Assert.NotNull(plugin1);
            Assert.True(pluginType.IsAssignableFrom(plugin1));

            var types = new List<Type>();
            // Find all plugin types
            foreach (var t in metadataLoadContext.Assembly.GetTypes())
            {
                if (!t.Equals(pluginType) && pluginType.IsAssignableFrom(t))
                {
                    types.Add(t);
                }
            }
            Assert.NotNull(types);
            Assert.Equal(types, new[] { plugin1, plugin2 });
        }

        [Fact]
        public void CanResolveType()
        {
            var compilation = CreateBasicCompilation(@"
namespace Types.Data
{
    public class Disposable : System.IDisposable { }
}
");
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type
            var idisposable = metadataLoadContext.ResolveType<IDisposable>();
            Assert.NotNull(idisposable);

            // This is using a type symbol in this context (defined below) to avoid using strings
            var disposable = metadataLoadContext.ResolveType<Types.Data.Disposable>();
            Assert.NotNull(disposable);
            Assert.True(idisposable.IsAssignableFrom(disposable));
            Assert.True(disposable.Equals(typeof(Disposable)));
            Assert.NotEqual(typeof(Disposable), disposable);
        }

        [Fact]
        public void CanResolveGenericTypeByName()
        {
            var compilation = CreateBasicCompilation(@"
public class Generic<T> { }
");
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type
            var genericType = metadataLoadContext.ResolveType("Generic`1");
            Assert.True(genericType.IsGenericType);
            Assert.True(genericType.IsGenericTypeDefinition);
            Assert.True(genericType.GetGenericTypeDefinition().Equals(genericType));
            Assert.NotNull(genericType);
        }

        [Fact]
        public void CanResolveGenericTypeByType()
        {
            var compilation = CreateBasicCompilation(@"
public class Generic<T> { }
");
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type
            var genericType = metadataLoadContext.ResolveType(typeof(Generic<>));
            Assert.True(genericType.IsGenericType);
            Assert.True(genericType.IsGenericTypeDefinition);
            Assert.True(genericType.GetGenericTypeDefinition().Equals(genericType));
            Assert.NotNull(genericType);
        }

        [Fact]
        public void CanCloseOpenGeneric()
        {
            var compilation = CreateBasicCompilation(@"
public class Generic<T> { }
public class ClosedGeneric : Generic<string> { } 
");
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type
            var genericType = metadataLoadContext.ResolveType(typeof(Generic<>));
            var closedGenericType = metadataLoadContext.ResolveType("ClosedGeneric");

            Assert.NotNull(closedGenericType);
            Assert.NotNull(closedGenericType.BaseType);
            Assert.NotNull(genericType);

            Assert.True(genericType.IsGenericType);
            Assert.True(genericType.IsGenericTypeDefinition);
            Assert.True(genericType.MakeGenericType(typeof(string)).Equals(closedGenericType.BaseType));
        }

        [Fact]
        public void CanResolveClosedGeneric()
        {
            var compilation = CreateBasicCompilation(@"
public class Generic<T> { }
");
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type
            var genericType = metadataLoadContext.ResolveType(typeof(Generic<string>));
            var stringType = metadataLoadContext.ResolveType<string>();

            Assert.NotNull(genericType);
            Assert.True(genericType.IsGenericType);
            Assert.False(genericType.IsGenericTypeDefinition);
            Assert.Equal(new[] { stringType }, genericType.GetGenericArguments());
        }

        [Fact]
        public void CanResolveArrayOfType()
        {
            var compilation = CreateBasicCompilation(@"
public class Thing { }
");
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type
            var thingArrayType = metadataLoadContext.ResolveType(typeof(Thing[]));

            Assert.NotNull(thingArrayType);
            Assert.True(thingArrayType.IsArray);
            Assert.Equal(typeof(Thing[]).Name, thingArrayType.Name);
        }

        [Fact]
        public void CanResolveMethodInfo()
        {
            var compilation = CreateBasicCompilation(@"
class TypeWithMembers
{
    public int MyProperty { get; set; }

    public void Foo(int x) { }
    public void Foo(double y) { }
}
");
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type
            var method = typeof(TypeWithMembers).GetMethod("Foo", BindingFlags.Public | BindingFlags.Instance, new[] { typeof(int) });

            Assert.NotNull(method);

            var methodInContext = metadataLoadContext.ResolveMember(method);

            Assert.NotNull(methodInContext);
            Assert.NotNull(methodInContext.GetMethodSymbol());
        }

        [Fact]
        public void CanResolvePropertyInfo()
        {
            var compilation = CreateBasicCompilation(@"
class TypeWithMembers
{
    public int MyProperty { get; set; }

    public void Foo(int x) { }
    public void Foo(double y) { }
}
");
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type
            var propertyInfo = typeof(TypeWithMembers).GetProperty("MyProperty", BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(propertyInfo);

            var propertyInContext = metadataLoadContext.ResolveMember(propertyInfo);

            Assert.NotNull(propertyInContext);
            Assert.NotNull(propertyInContext.GetPropertySymbol());
        }

        [Fact]
        public void CanResolveFieldInfo()
        {
            var compilation = CreateBasicCompilation(@"
class TypeWithMembers
{
    public int MyField;
    public int MyProperty { get; set; }

    public void Foo(int x) { }
    public void Foo(double y) { }
}
");
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type
            var fieldInfo = typeof(TypeWithMembers).GetField("MyField", BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(fieldInfo);

            var fieldInContext = metadataLoadContext.ResolveMember(fieldInfo);

            Assert.NotNull(fieldInContext);
            Assert.NotNull(fieldInContext.GetFieldSymbol());
        }

        private static CSharpCompilation CreateBasicCompilation(string text)
        {
            return CSharpCompilation.Create("something",
                syntaxTrees: new[] { CSharpSyntaxTree.ParseText(text) },
                references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
                });
        }
    }
}

#pragma warning disable CS0649
class TypeWithMembers
{
    public int MyField;
    public int MyProperty { get; set; }

    public void Foo(int x) { }
    public void Foo(double y) { }
}
#pragma warning restore CS0649

class Thing { }
class Generic<T> { }

namespace Types.Data
{
    class Disposable { }
}