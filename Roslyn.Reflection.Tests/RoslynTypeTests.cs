using System.Reflection;
using System.Runtime.CompilerServices;
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
        public void GetMethodsWorks()
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
        [InlineData(BindingFlags.NonPublic | BindingFlags.Instance)]
        [InlineData(BindingFlags.Public | BindingFlags.Static)]
        [InlineData(BindingFlags.NonPublic | BindingFlags.Static)]
        [InlineData(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)]
        [InlineData(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)]
        public void GetMethodsWithBindingFlags(BindingFlags flags)
        {
            var compilation = CreateBasicCompilation(ThisTypeText);
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type by name
            var thisType0 = metadataLoadContext.ResolveType("ThisType");

            Assert.NotNull(thisType0);
            var actualMethods = thisType0.GetMethods(flags);
            var expectedMethods = typeof(ThisType).GetMethods(flags);

            AssertMembers(actualMethods, expectedMethods);
        }

        [Theory]
        [InlineData(BindingFlags.Public)]
        [InlineData(BindingFlags.NonPublic)]
        [InlineData(BindingFlags.Instance)]
        [InlineData(BindingFlags.Public | BindingFlags.Instance)]
        [InlineData(BindingFlags.NonPublic | BindingFlags.Instance)]
        [InlineData(BindingFlags.Public | BindingFlags.Static)]
        [InlineData(BindingFlags.NonPublic | BindingFlags.Static)]
        [InlineData(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)]
        [InlineData(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)]
        public void GetPropertiesWithBindingFlags(BindingFlags flags)
        {
            var compilation = CreateBasicCompilation(ThisTypeText);
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type by name
            var thisType0 = metadataLoadContext.ResolveType("ThisType");

            Assert.NotNull(thisType0);
            var actualProperties = thisType0.GetProperties(flags);
            var expectedProperties = typeof(ThisType).GetProperties(flags);

            AssertMembers(actualProperties, expectedProperties);
        }

        [Theory]
        [InlineData(BindingFlags.Public)]
        [InlineData(BindingFlags.NonPublic)]
        [InlineData(BindingFlags.Instance)]
        [InlineData(BindingFlags.Public | BindingFlags.Instance)]
        [InlineData(BindingFlags.NonPublic | BindingFlags.Instance)]
        [InlineData(BindingFlags.Public | BindingFlags.Static)]
        [InlineData(BindingFlags.NonPublic | BindingFlags.Static)]
        [InlineData(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)]
        [InlineData(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)]
        public void GetFieldsWithBindingFlags(BindingFlags flags)
        {
            var compilation = CreateBasicCompilation(ThisTypeText);
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type by name
            var thisType0 = metadataLoadContext.ResolveType("ThisType");

            Assert.NotNull(thisType0);
            var actualFields = thisType0.GetFields(flags);
            var expectedFields = typeof(ThisType).GetFields(flags);

            AssertMembers(actualFields, expectedFields);
        }

        [Fact]
        public void GetFieldWithBindingFlagsFailsIfFlagsDontMatch()
        {
            var compilation = CreateBasicCompilation(ThisTypeText);
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type by name
            var thisType0 = metadataLoadContext.ResolveType("ThisType");

            Assert.NotNull(thisType0);
            var flags = BindingFlags.Public;
            var actualField = thisType0.GetField("publicInstanceField", flags);
            var expectedField = typeof(ThisType).GetField("publicInstanceField", flags);

            Assert.Null(actualField);
            Assert.Null(expectedField);
        }

        [Fact]
        public void GetFieldWithBindingFlags()
        {
            var compilation = CreateBasicCompilation(ThisTypeText);
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type by name
            var thisType0 = metadataLoadContext.ResolveType("ThisType");

            Assert.NotNull(thisType0);
            var flags = BindingFlags.Public | BindingFlags.Instance;
            var actualField = thisType0.GetField("publicInstanceField", flags);
            var expectedField = typeof(ThisType).GetField("publicInstanceField", flags);

            Assert.NotNull(actualField);
            Assert.NotNull(expectedField);

            Assert.Equal(expectedField!.Name, actualField!.Name);
        }

        [Theory]
        [InlineData(BindingFlags.Public)]
        [InlineData(BindingFlags.NonPublic)]
        [InlineData(BindingFlags.Instance)]
        [InlineData(BindingFlags.Public | BindingFlags.Instance)]
        [InlineData(BindingFlags.NonPublic | BindingFlags.Instance)]
        [InlineData(BindingFlags.Public | BindingFlags.Static)]
        [InlineData(BindingFlags.NonPublic | BindingFlags.Static)]
        [InlineData(BindingFlags.Public | BindingFlags.Static | BindingFlags.Instance)]
        [InlineData(BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)]
        public void GetMembersWithBindingFlags(BindingFlags flags)
        {
            var compilation = CreateBasicCompilation(ThisTypeText);
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type by name
            var thisType0 = metadataLoadContext.ResolveType("ThisType");

            Assert.NotNull(thisType0);
            var actualMembers = thisType0.GetMembers(flags);
            var expectedMembers = typeof(ThisType).GetMembers(flags);

            AssertMembers(actualMembers, expectedMembers);
        }

        private void AssertMembers(IEnumerable<MemberInfo> actualMembers, IEnumerable<MemberInfo> expectedMembers)
        {
            var actualNames = actualMembers.Select(m => m.Name).OrderBy(m => m).ToArray();
            // REVIEW: Why do we need to filter object based members?
            var expetedNames = expectedMembers.Where(m => m.DeclaringType != typeof(object)).Select(m => m.Name).OrderBy(m => m).ToArray();

            Assert.Equal(expetedNames, actualNames);
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
  public class Nested { }
  class PrivateNested { }
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
  public class Nested { }
  class PrivateNested { }
}

");
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type by name
            var pluginType = metadataLoadContext.ResolveType("TopLevel");

            Assert.NotNull(pluginType);
            Assert.True(pluginType.IsClass);

            var nestedType = pluginType.GetNestedType("Nested");
            var privateNestedType = pluginType.GetNestedType("Nested", BindingFlags.Public | BindingFlags.Instance);

            Assert.NotNull(nestedType);
            Assert.NotNull(privateNestedType);

            Assert.True(nestedType!.IsNested);
            Assert.Equal("Nested", nestedType!.Name);
        }

        [Fact]
        public void GetMethodUsingBindingFlagsAndParameterTypes()
        {
            var compilation = CreateBasicCompilation("");
            var metadataLoadContext = new MetadataLoadContext(compilation);

            var intType = metadataLoadContext.ResolveType<int>();

            // We're using a type from this context so test that we always resolve types in the MetadataLoadContext before comparison
            var tryParseMethod = intType.GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static, new[] { typeof(string), intType.MakeByRefType() });
            var tryParseSpanMethod = typeof(int).GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static, new[] { typeof(ReadOnlySpan<char>), intType.MakeByRefType() });

            Assert.NotNull(tryParseMethod);
            Assert.NotNull(tryParseSpanMethod);
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
            private readonly int _privateInstanceField;
            public readonly int publicInstanceField;
            private static readonly int privateStaticField;
            public static readonly int publicStaticField;


            public int InstanceProperty { get; set; }
            public static object? StaticProperty { get; set; }
            private int PrivateProperty { get; set; }
            private static object? StaticPrivateProperty { get; set; }


            public void InstanceMethod() { }
            string PrivateMethod() => "Woah";
            static string StaticPrivateMethod() => "Woah";
            public static int StaticMethod() => 1;
        }

        private const string ThisTypeText = @"
class ThisType
{
    private readonly int _privateInstanceField;
    public readonly int publicInstanceField;
    private static readonly int privateStaticField;
    public static readonly int publicStaticField;

    public int InstanceProperty { get; set; }
    public static object? StaticProperty { get; set; }
    private int PrivateProperty { get; set; }
    private static object? StaticPrivateProperty { get; set; }


    public void InstanceMethod() { }
    string PrivateMethod() => ""Woah"";
    static string StaticPrivateMethod() => ""Woah"";
    public static int StaticMethod() => 1;
}
";
    }
}
