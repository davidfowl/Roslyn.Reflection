# Roslyn.Reflection

These are reflection wrappers over the roslyn APIs. Use familiar APIs to explore the type system.

Here's an example of using these APIs with to find types in a roslyn compilation using the reflection APIs:

```C#
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
```