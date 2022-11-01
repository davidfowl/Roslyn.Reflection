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

            Assert.Equal("Base", baseType.FullName);
            Assert.Equal("IContract", interfaceType.FullName);
            Assert.Null(baseType.Namespace);
            Assert.Null(interfaceType.Namespace);
            Assert.Equal(interfaceType, baseType.GetInterface("IContract"));
            Assert.Equal(interfaceType, baseType.GetInterface("icontract", ignoreCase: true));
            Assert.Null(baseType.GetInterface("icontract", ignoreCase: false));
        }

        [Fact]
        public void GetMethodsWorks()
        {
            var compilation = CreateBasicCompilation(@"
class TypeWithMethods
{
    public void InstanceMethod() { }
    string PrivateMethod() => ""Woah"";
    public static int StaticMethod() => 1;
}

");
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type by name
            var thisType = metadataLoadContext.ResolveType("TypeWithMethods");

            Assert.NotNull(thisType);
            var actualMethods = thisType.GetMethods();
            var expectedMethods = typeof(TypeWithMethods).GetMethods();

            AssertMembers(expectedMethods, actualMethods);
        }

        [Fact]
        public void GetMethodWorks()
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
            var method = thisType.GetMethod("InstanceMethod");

            Assert.NotNull(method);

            Assert.Contains("InstanceMethod", method!.Name);
        }

        [Fact]
        public void GetCtorWorks()
        {
            var compilation = CreateBasicCompilation(@"
class ThisType
{
    public ThisType() { }
    private ThisType(int x, int y) { }
}

");
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type by name
            var thisType = metadataLoadContext.ResolveType("ThisType");

            Assert.NotNull(thisType);
            var method0 = thisType.GetConstructor(BindingFlags.Public | BindingFlags.Instance, Type.EmptyTypes);
            var method1 = thisType.GetConstructor(BindingFlags.NonPublic | BindingFlags.Instance, new[] { typeof(int), typeof(int) });

            Assert.NotNull(method0);
            Assert.NotNull(method1);

            Assert.Empty(method0!.GetParameters());
            Assert.Equal(2, method1!.GetParameters().Length);
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

            AssertMembers(expectedMethods, actualMethods);
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
        public void GetCtorsWithBindingFlags(BindingFlags flags)
        {
            var compilation = CreateBasicCompilation(ThisTypeText);
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type by name
            var thisType0 = metadataLoadContext.ResolveType("ThisType");

            Assert.NotNull(thisType0);
            var actualMethods = thisType0.GetConstructors(flags);
            var expectedMethods = typeof(ThisType).GetConstructors(flags);

            AssertMembers(expectedMethods, actualMethods);
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

            AssertMembers(expectedProperties, actualProperties);
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

            AssertMembers(expectedFields, actualFields);
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

            AssertMembers(expectedMembers, actualMembers);
        }

        [Theory]
        [InlineData(BindingFlags.Public | BindingFlags.Instance)]
        [InlineData(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)]
        [InlineData(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)]
        public void GetInheritedPropertiesWithBindingFlags(BindingFlags flags)
        {
            var compilation = CreateBasicCompilation(ThisTypeText);
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type by name
            var thisType0 = metadataLoadContext.ResolveType("DerivedType");

            Assert.NotNull(thisType0);
            var actualProperties = thisType0.GetProperties(flags);
            var expectedProperties = typeof(DerivedType).GetProperties(flags);

            AssertMembers(expectedProperties, actualProperties);
        }

        [Theory]
        [InlineData(BindingFlags.Public | BindingFlags.Instance)]
        [InlineData(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly)]
        [InlineData(BindingFlags.Public | BindingFlags.Instance | BindingFlags.FlattenHierarchy)]
        public void GetInheritedMethodsWithBindingFlags(BindingFlags flags)
        {
            var compilation = CreateBasicCompilation(ThisTypeText);
            var metadataLoadContext = new MetadataLoadContext(compilation);

            // Resolve the type by name
            var thisType0 = metadataLoadContext.ResolveType("DerivedType");

            Assert.NotNull(thisType0);
            var actualMethods = thisType0.GetMethods(flags);
            var expectedMethods = typeof(DerivedType).GetMethods(flags);

            AssertMembers(expectedMethods, actualMethods);
        }

        private static void AssertMembers(IEnumerable<MemberInfo> expectedMembers, IEnumerable<MemberInfo> actualMembers)
        {
            bool Include(MemberInfo member) => !member.DeclaringType.Equals(typeof(object));

            var actualNames = actualMembers.Where(Include).Select(m => m.Name).OrderBy(m => m).ToArray();
            var expetedNames = expectedMembers.Where(Include).Select(m => m.Name).OrderBy(m => m).ToArray();

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
            var methods = typeWithPointers.GetMethods(BindingFlags.DeclaredOnly | BindingFlags.Public | BindingFlags.Instance);
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
            var tryParseSpanMethod = intType.GetMethod("TryParse", BindingFlags.Public | BindingFlags.Static, new[] { typeof(ReadOnlySpan<char>), intType.MakeByRefType() });

            Assert.NotNull(tryParseMethod);
            Assert.NotNull(tryParseSpanMethod);
        }

        // Is IsPrimitive
        // https://github.com/dotnet/runtime/blob/55e95c80a7d7ec9d7bbbd5ad434604a1dc33e19c/src/libraries/System.Reflection.MetadataLoadContext/src/System/Reflection/TypeLoading/Types/RoType.TypeClassification.cs#L85
        [Theory]
        [InlineData(typeof(Int32), true)]
        [InlineData(typeof(Int16), true)]
        [InlineData(typeof(Int64), true)]
        [InlineData(typeof(Boolean), true)]
        [InlineData(typeof(Char), true)]
        [InlineData(typeof(SByte), true)]
        [InlineData(typeof(Single), true)]
        [InlineData(typeof(Double), true)]
        [InlineData(typeof(IntPtr), true)]
        [InlineData(typeof(UIntPtr), true)]
        [InlineData(typeof(Byte), true)]
        [InlineData(typeof(DateTime), false)]
        public void IsPrimitiveType(Type type, bool isPrimitive)
        {
            var compilation = CreateBasicCompilation("");
            var metadataLoadContext = new MetadataLoadContext(compilation);
            var typeInContext = metadataLoadContext.ResolveType(type);

            Assert.Equal(isPrimitive, typeInContext.IsPrimitive);
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
            public ThisType() { }
            public ThisType(int x) : this(x, 0) { }
            private ThisType(int x, int y) { }

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

            public class PublicNested { }
            private class PrivateNested { }
        }

        class TypeWithMethods
        {
            public void InstanceMethod() { }
            string PrivateMethod() => "Woah";
            public static int StaticMethod() => 1;
        }
        class DerivedType : BaseType
        {

        }

        class BaseType
        {
            public virtual int X { get; }
            public virtual int GetX() => X;
        }

        private const string ThisTypeText = @"
class ThisType
{
    public ThisType() { }
    public ThisType(int x) : this(x, 0) { }
    private ThisType(int x, int y) { }

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

    public class PublicNested { }
    private class PrivateNested { }
}

class DerivedType : BaseType
{
    
}

class BaseType
{
    public virtual int X { get; }
    public virtual int GetX() => X;
}
";
    }
}
