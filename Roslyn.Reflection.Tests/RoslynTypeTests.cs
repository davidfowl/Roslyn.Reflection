using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Types.Data;

namespace Roslyn.Reflection.Tests
{
    public class RoslynTypeTests
    {
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
    }
}
