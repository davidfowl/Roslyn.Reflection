using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;

#nullable disable
namespace Roslyn.Reflection
{
    public static class RoslynExtensions
    {
        public static IMethodSymbol GetMethodSymbol(this MethodInfo methodInfo) => (methodInfo as RoslynMethodInfo)?.MethodSymbol;

        public static IPropertySymbol GetPropertySymbol(this PropertyInfo property) => (property as RoslynPropertyInfo)?.PropertySymbol;
        public static IFieldSymbol GetFieldSymbol(this FieldInfo field) => (field as RoslynFieldInfo)?.FieldSymbol;

        public static IParameterSymbol GetParameterSymbol(this ParameterInfo parameterInfo) => (parameterInfo as RoslynParameterInfo)?.ParameterSymbol;

        public static ITypeSymbol GetTypeSymbol(this Type type) => (type as RoslynType)?.TypeSymbol;
    }

    internal static class RoslynInternalExtensions
    {
        public static Assembly AsAssembly(this IAssemblySymbol assemblySymbol, MetadataLoadContext metadataLoadContext) => metadataLoadContext.GetOrCreate<Assembly>(assemblySymbol);

        public static Type AsType(this ITypeSymbol typeSymbol, MetadataLoadContext metadataLoadContext) => metadataLoadContext.GetOrCreate<Type>(typeSymbol);

        public static ParameterInfo AsParameterInfo(this IParameterSymbol parameterSymbol, MetadataLoadContext metadataLoadContext) => metadataLoadContext.GetOrCreate<ParameterInfo>(parameterSymbol);

        public static ConstructorInfo AsConstructorInfo(this IMethodSymbol methodSymbol, MetadataLoadContext metadataLoadContext) => metadataLoadContext.GetOrCreate<ConstructorInfo>(methodSymbol);

        public static MethodInfo AsMethodInfo(this IMethodSymbol methodSymbol, MetadataLoadContext metadataLoadContext) => metadataLoadContext.GetOrCreate<MethodInfo>(methodSymbol);

        public static PropertyInfo AsPropertyInfo(this IPropertySymbol propertySymbol, MetadataLoadContext metadataLoadContext) => metadataLoadContext.GetOrCreate<PropertyInfo>(propertySymbol);

        public static FieldInfo AsFieldInfo(this IFieldSymbol fieldSymbol, MetadataLoadContext metadataLoadContext) => metadataLoadContext.GetOrCreate<FieldInfo>(fieldSymbol);

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
