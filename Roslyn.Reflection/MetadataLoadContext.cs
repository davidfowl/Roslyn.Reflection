using System;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Roslyn.Reflection
{
    public class MetadataLoadContext
    {
        private readonly Compilation _compilation;

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
                if (typeSymbol == null)
                {
                    return null;
                }

                return _compilation.CreateArrayTypeSymbol(typeSymbol).AsType(this);
            }

            return null;
        }
    }
}
