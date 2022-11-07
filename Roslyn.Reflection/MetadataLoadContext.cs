using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

#nullable disable
namespace Roslyn.Reflection
{
    public class MetadataLoadContext
    {
        private readonly Compilation _compilation;
        private readonly ConcurrentDictionary<ISymbol, object> _cache = new(SymbolEqualityComparer.Default);

        public MetadataLoadContext(Compilation compilation)
        {
            _compilation = compilation;
        }

        public Assembly Assembly => _compilation.Assembly.AsAssembly(this);

        internal Compilation Compilation => _compilation;

        public Type ResolveType(string fullyQualifiedMetadataName)
        {
            return _compilation.GetTypeByMetadataName(fullyQualifiedMetadataName)?.AsType(this);
        }

        public Type ResolveType<T>() => ResolveType(typeof(T));

        public Type ResolveType(Type type)
        {
            if (type is RoslynType)
            {
                return type;
            }

            var resolvedType = _compilation.GetTypeByMetadataName(type.FullName);

            if (resolvedType is not null)
            {
                return resolvedType.AsType(this);
            }

            if (type.IsArray)
            {
                var typeSymbol = _compilation.GetTypeByMetadataName(type.GetElementType().FullName);
                if (typeSymbol is null)
                {
                    return null;
                }

                return _compilation.CreateArrayTypeSymbol(typeSymbol).AsType(this);
            }

            if (type.IsGenericType)
            {
                var openGenericTypeSymbol = _compilation.GetTypeByMetadataName(type.GetGenericTypeDefinition().FullName);
                if (openGenericTypeSymbol is null)
                {
                    return null;
                }

                return openGenericTypeSymbol.AsType(this).MakeGenericType(type.GetGenericArguments());
            }

            return null;
        }

        public TMember GetOrCreate<TMember>(ISymbol symbol) where TMember : class
        {
            if (symbol is null)
            {
                return null;
            }

            return (TMember)_cache.GetOrAdd(symbol, s => s switch
            {
                ITypeSymbol t => new RoslynType(t, this),
                IFieldSymbol f => new RoslynFieldInfo(f, this),
                IPropertySymbol p => new RoslynPropertyInfo(p, this),
                IMethodSymbol c when c.MethodKind == MethodKind.Constructor => new RoslynConstructorInfo(c, this),
                IMethodSymbol m => new RoslynMethodInfo(m, this),
                IParameterSymbol param => new RoslynParameterInfo(param, this),
                IAssemblySymbol a => new RoslynAssembly(a, this),
                _ => null
            });
        }

        public TMember ResolveMember<TMember>(TMember memberInfo) where TMember : MemberInfo
        {
            return memberInfo switch
            {
                RoslynFieldInfo f => (TMember)(object)f,
                RoslynMethodInfo m => (TMember)(object)m,
                RoslynPropertyInfo p => (TMember)(object)p,
                MethodInfo m => (TMember)(object)ResolveType(m.ReflectedType)?.GetMethod(m.Name, SharedUtilities.ComputeBindingFlags(m), binder: null, types: m.GetParameters().Select(t => t.ParameterType).ToArray(), modifiers: null),
                PropertyInfo p => (TMember)(object)ResolveType(p.ReflectedType)?.GetProperty(p.Name, SharedUtilities.ComputeBindingFlags(p), binder: null, returnType: p.PropertyType, types: p.GetIndexParameters().Select(t => t.ParameterType).ToArray(), modifiers: null),
                FieldInfo f => (TMember)(object)ResolveType(f.ReflectedType)?.GetField(f.Name, SharedUtilities.ComputeBindingFlags(f)),
                _ => null
            };
        }
    }
}
#nullable restore
