using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

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

// Find all plugin types
foreach (var t in metadataLoadContext.Assembly.GetTypes())
{
    if (!t.Equals(pluginType) && pluginType.IsAssignableFrom(t))
    {
        Console.WriteLine(t);
    }
}
