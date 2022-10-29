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

        private static CSharpCompilation CreateBasicCompilation(string text)
        {
            return CSharpCompilation.Create("something",
                syntaxTrees: new[] { CSharpSyntaxTree.ParseText(text) },
                references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
                });
        }
    }
}

class Generic<T> { }

namespace Types.Data
{
    class Disposable { }
}