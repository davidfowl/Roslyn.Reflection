using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Roslyn.Reflection
{
    public static class RoslynExtensions
    {
        public static Assembly AsAssembly(this IAssemblySymbol assemblySymbol, MetadataLoadContext metadataLoadContext) => assemblySymbol == null ? null : new RoslynAssembly(assemblySymbol, metadataLoadContext);

        public static Type AsType(this ITypeSymbol typeSymbol, MetadataLoadContext metadataLoadContext) => typeSymbol == null ? null : new RoslynType(typeSymbol, metadataLoadContext);

        public static ParameterInfo AsParameterInfo(this IParameterSymbol parameterSymbol, MetadataLoadContext metadataLoadContext) => parameterSymbol == null ? null : new RoslynParameterInfo(parameterSymbol, metadataLoadContext);

        public static MethodInfo AsMethodInfo(this IMethodSymbol methodSymbol, MetadataLoadContext metadataLoadContext) => methodSymbol == null ? null : new RoslynMethodInfo(methodSymbol, metadataLoadContext);

        public static PropertyInfo AsPropertyInfo(this IPropertySymbol propertySymbol, MetadataLoadContext metadataLoadContext) => propertySymbol == null ? null : new RoslynPropertyInfo(propertySymbol, metadataLoadContext);

        public static FieldInfo AsFieldInfo(this IFieldSymbol fieldSymbol, MetadataLoadContext metadataLoadContext) => fieldSymbol == null ? null : new RoslynFieldInfo(fieldSymbol, metadataLoadContext);

        public static IMethodSymbol GetMethodSymbol(this MethodInfo methodInfo) => (methodInfo as RoslynMethodInfo)?.MethodSymbol;

        public static IPropertySymbol GetPropertySymbol(this PropertyInfo property) => (property as RoslynPropertyInfo)?.PropertySymbol;
        public static IFieldSymbol GetFieldSymbol(this FieldInfo field) => (field as RoslynFieldInfo)?.FieldSymbol;

        public static IParameterSymbol GetParameterSymbol(this ParameterInfo parameterInfo) => (parameterInfo as RoslynParameterInfo)?.ParameterSymbol;

        public static ITypeSymbol GetTypeSymbol(this Type type) => (type as RoslynType)?.TypeSymbol;

        public static IEnumerable<ITypeSymbol> BaseTypes(this ITypeSymbol typeSymbol)
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
