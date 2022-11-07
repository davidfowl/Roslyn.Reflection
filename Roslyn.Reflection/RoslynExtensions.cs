using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;

#nullable disable
namespace Roslyn.Reflection
{
    public static class RoslynExtensions
    {
        public static Assembly AsAssembly(this IAssemblySymbol assemblySymbol, MetadataLoadContext metadataLoadContext) => metadataLoadContext.GetOrCreateSymbol<Assembly>(assemblySymbol);

        public static Type AsType(this ITypeSymbol typeSymbol, MetadataLoadContext metadataLoadContext) => metadataLoadContext.GetOrCreateSymbol<Type>(typeSymbol);

        public static ParameterInfo AsParameterInfo(this IParameterSymbol parameterSymbol, MetadataLoadContext metadataLoadContext) => metadataLoadContext.GetOrCreateSymbol<ParameterInfo>(parameterSymbol);

        public static MethodInfo AsMethodInfo(this IMethodSymbol methodSymbol, MetadataLoadContext metadataLoadContext) => metadataLoadContext.GetOrCreateSymbol<MethodInfo>(methodSymbol);

        public static PropertyInfo AsPropertyInfo(this IPropertySymbol propertySymbol, MetadataLoadContext metadataLoadContext) => metadataLoadContext.GetOrCreateSymbol<PropertyInfo>(propertySymbol);

        public static FieldInfo AsFieldInfo(this IFieldSymbol fieldSymbol, MetadataLoadContext metadataLoadContext) => metadataLoadContext.GetOrCreateSymbol<FieldInfo>(fieldSymbol);

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
#nullable restore
