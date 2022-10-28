using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace Roslyn.Reflection.Tests
{
    public class UnitTest1
    {
        [Fact]
        public void Test1()
        {
            var compilation = CSharpCompilation.Create("something",
    syntaxTrees: new[] { CSharpSyntaxTree.ParseText(@"
public class NotAPlugin
{
}

public class Plugin1 : IPlugin { }
public class Plugin2 : IPlugin { }

public interface IPlugin { }

") },
    references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
    });
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
    }
}