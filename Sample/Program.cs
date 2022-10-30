using System.Reflection;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Roslyn.Reflection;

var compilation = CSharpCompilation.Create("something",
    syntaxTrees: new[] { CSharpSyntaxTree.ParseText(@"
using Microsoft.AspNetCore.Mvc;

public class NotAPlugin
{
}

public class Plugin1 : IPlugin { }
public class Plugin2 : IPlugin { }

public interface IPlugin { }

public class MyController : ControllerBase
{
    [HttpGet(""/hello/{name}"")]
    public IActionResult Get(string name) => Ok(""Hello World"");
}

[Authorize]
public class AuthController : ControllerBase  { }

public class GenericThing<T> { }

public class AnotherThing : GenericThing<string> { }

namespace Microsoft.AspNetCore.Mvc
{
    public class ControllerBase { }

    public class AuthorizeAttribute : System.Attribute { }
    public class HttpGetAttribute : System.Attribute 
    { 
       public HttpGetAttribute(string path) { }
    }
}
") },
    references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
    });

var metadataLoadContext = new MetadataLoadContext(compilation);

// Resolve the type by name
var pluginType = metadataLoadContext.ResolveType("IPlugin");
var controllerType = metadataLoadContext.ResolveType("Microsoft.AspNetCore.Mvc.ControllerBase");

Console.WriteLine("Plugins");
Console.WriteLine();

// Find all types with a base class or interface

foreach (var t in metadataLoadContext.Assembly.GetTypes())
{
    if (!t.Equals(pluginType) && pluginType.IsAssignableFrom(t))
    {
        Console.WriteLine($"- {t}");
    }
}

Console.WriteLine();

Console.WriteLine("Controllers");
Console.WriteLine();


foreach (var t in metadataLoadContext.Assembly.GetTypes())
{

    if (!t.Equals(controllerType) && controllerType.IsAssignableFrom(t))
    {
        Console.WriteLine($"- {t}");

        foreach (var m in t.GetMethods())
        {
            Console.WriteLine($"    {m}");
        }
    }
}
Console.WriteLine();

var attribute = metadataLoadContext.ResolveType("Microsoft.AspNetCore.Mvc.AuthorizeAttribute");

Console.WriteLine("Types with authorize attribute");

foreach (var t in metadataLoadContext.Assembly.GetTypes())
{
    if (t.CustomAttributes.Any(c => c.AttributeType.Equals(attribute)))
    {
        Console.WriteLine(@$" - {t}");
    }
}

Console.WriteLine();

// Get back the type roslyn symbol and find where it is declared in source
ITypeSymbol controllerTypeSymbol = controllerType.GetTypeSymbol();

foreach (var syntaxReference in controllerTypeSymbol.DeclaringSyntaxReferences)
{
    var syntax = syntaxReference.GetSyntax();

    var span = syntax.SyntaxTree.GetLocation(syntax.Span);
    var lineNumber = span.GetLineSpan().StartLinePosition.Line;

    Console.WriteLine($"{controllerTypeSymbol}  was declared on line {lineNumber}");
}