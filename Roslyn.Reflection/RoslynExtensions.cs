using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace System.Reflection
{
    public static class RoslynExtensions
    {
        public static Assembly AsAssembly(this IAssemblySymbol assemblySymbol, MetadataLoadContext metadataLoadContext) => assemblySymbol == null ? null : new RoslynAssembly(assemblySymbol, metadataLoadContext);

        public static Type AsType(this ITypeSymbol typeSymbol, MetadataLoadContext metadataLoadContext) => typeSymbol == null ? null : new RoslynType(typeSymbol, metadataLoadContext);

        public static ParameterInfo AsParameterInfo(this IParameterSymbol parameterSymbol, MetadataLoadContext metadataLoadContext) => parameterSymbol == null ? null : new RoslynParameter(parameterSymbol, metadataLoadContext);

        public static MethodInfo AsMethodInfo(this IMethodSymbol methodSymbol, MetadataLoadContext metadataLoadContext) => methodSymbol == null ? null : new RoslynMethodInfo(methodSymbol, metadataLoadContext);

        public static IMethodSymbol GetMethodSymbol(this MethodInfo methodInfo) => (methodInfo as RoslynMethodInfo)?.MethodSymbol;

        public static IPropertySymbol GetPropertySymbol(this PropertyInfo property) => (property as RoslynProperty)?.PropertySymbol;

        public static IParameterSymbol GetParameterSymbol(this ParameterInfo parameterInfo) => (parameterInfo as RoslynParameter)?.ParameterSymbol;

        public static ITypeSymbol GetTypeSymbol(this Type type) => (type as RoslynType)?.TypeSymbol;

        public static IEnumerable<INamedTypeSymbol> BaseTypes(this INamedTypeSymbol typeSymbol)
        {
            var t = typeSymbol;
            while (t != null)
            {
                yield return t;
                t = t.BaseType;
            }
        }
    }
}
