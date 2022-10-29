using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Types.Data;

namespace Roslyn.Reflection.Tests
{
    public class RoslynTypeTests
    {
        [Fact]
        public void InheritanceTypeProperties()
        {
            var compilation = CreateBasicCompilation(@"
sealed class Derived : Base { }

abstract class Base : IContract { }

interface IContract { }

");
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type by name
            var derivedType = metadataLoadContext.ResolveType("Derived");
            var baseType = metadataLoadContext.ResolveType("Base");
            var interfaceType = metadataLoadContext.ResolveType("IContract");

            Assert.NotNull(derivedType);
            Assert.True(derivedType.IsSealed);
            Assert.NotNull(baseType);
            Assert.True(baseType.IsAbstract);
            Assert.NotNull(interfaceType);
            Assert.True(interfaceType.IsInterface);

            Assert.Equal(derivedType.BaseType, baseType);
            Assert.Contains(interfaceType, baseType.GetInterfaces());
        }

        [Fact]
        public void GetInterfaceByName()
        {
            var compilation = CreateBasicCompilation(@"
sealed class Derived : Base { }

abstract class Base : IContract { }

interface IContract { }

");
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type by name
            var baseType = metadataLoadContext.ResolveType("Base");
            var interfaceType = metadataLoadContext.ResolveType("IContract");

            Assert.NotNull(baseType);
            Assert.NotNull(interfaceType);

            Assert.Equal(interfaceType, baseType.GetInterface("IContract"));
            Assert.Equal(interfaceType, baseType.GetInterface("icontract", ignoreCase: true));
            Assert.Null(baseType.GetInterface("icontract", ignoreCase: false));
        }

        [Fact]
        public void GetMethods()
        {
            var compilation = CreateBasicCompilation(@"
class ThisType
{
    public void InstanceMethod() { }
    string PrivateMethod() => ""Woah"";
    public static int StaticMethod() => 1;
}

");
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type by name
            var thisType = metadataLoadContext.ResolveType("ThisType");

            Assert.NotNull(thisType);
            var methods = thisType.GetMethods();

            // Private methods don't show up by default
            Assert.Equal(2, methods.Length);

            Assert.Contains(methods, m => m.Name == "InstanceMethod");
            Assert.Contains(methods, m => m.Name == "StaticMethod");
        }

        [Theory]
        [InlineData(BindingFlags.Public)]
        [InlineData(BindingFlags.NonPublic)]
        [InlineData(BindingFlags.Instance)]
        [InlineData(BindingFlags.Public | BindingFlags.Instance)]
        [InlineData(BindingFlags.Public | BindingFlags.Static)]
        [InlineData(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)]
        public void BindingFlagsTest(BindingFlags flags)
        {
            var compilation = CreateBasicCompilation(@"
class ThisType
{
    public void InstanceMethod() { }
    string PrivateMethod() => ""Woah"";
    static string StaticPrivateMethod() => ""Woah"";
    public static int StaticMethod() => 1;
}

");
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type by name
            var thisType0 = metadataLoadContext.ResolveType("ThisType");
            var thisType1 = typeof(ThisType);

            Assert.NotNull(thisType0);
            var methods0 = thisType0.GetMethods(flags);
            // TODO: Should this return types from object?
            var methods1 = thisType1.GetMethods(flags).Where(m => m.DeclaringType != typeof(object));

            Assert.Equal(methods1.Select(m => m.Name), methods0.Select(m => m.Name));
        }

        [Fact]
        public void IsPointer()
        {
            var compilation = CreateBasicCompilation(@"
class TypeWithPointers
{
    public unsafe void Parse(byte* p) { }
}

");
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type by name
            var typeWithPointers = metadataLoadContext.ResolveType("TypeWithPointers");

            Assert.NotNull(typeWithPointers);
            var methods = typeWithPointers.GetMethods();
            var method = Assert.Single(methods);

            Assert.Equal("Parse", method.Name);

            var parameter = Assert.Single(method.GetParameters());

            Assert.Equal("p", parameter.Name);
            Assert.True(parameter.ParameterType.IsPointer);
        }

        [Fact]
        public void CanResolveNestedTypes()
        {
            var compilation = CreateBasicCompilation(@"
class TopLevel
{
  class Nested { }
}

");
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type by name
            var pluginType = metadataLoadContext.ResolveType("TopLevel");

            Assert.NotNull(pluginType);
            Assert.True(pluginType.IsClass);

            var nestedTypes = pluginType.GetNestedTypes();
            Assert.NotEmpty(nestedTypes);

            Assert.Equal("Nested", nestedTypes[0].Name);
        }

        [Fact]
        public void CanResolveNestedTypeByName()
        {
            var compilation = CreateBasicCompilation(@"
class TopLevel
{
  class Nested { }
}

");
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type by name
            var pluginType = metadataLoadContext.ResolveType("TopLevel");

            Assert.NotNull(pluginType);
            Assert.True(pluginType.IsClass);

            var nestedType = pluginType.GetNestedType("Nested");
            Assert.NotNull(nestedType);
            Assert.True(nestedType!.IsNested);
            Assert.Equal("Nested", nestedType!.Name);
        }

        private static CSharpCompilation CreateBasicCompilation(string text)
        {
            return CSharpCompilation.Create("something",
                syntaxTrees: new[] { CSharpSyntaxTree.ParseText(text) },
                references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
                });
        }


        // Keep this in sync with the tests that mirror this type
        class ThisType
        {
            public void InstanceMethod() { }
            string PrivateMethod() => "Woah";
            static string StaticPrivateMethod() => "Woah";
            public static int StaticMethod() => 1;
        }
    }
}
