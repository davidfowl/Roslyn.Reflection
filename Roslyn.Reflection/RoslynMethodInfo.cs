using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection;
using Microsoft.CodeAnalysis;

#nullable disable
namespace Roslyn.Reflection
{
    internal class RoslynMethodInfo : MethodInfo
    {
        private readonly IMethodSymbol _method;
        private readonly MetadataLoadContext _metadataLoadContext;

        public RoslynMethodInfo(IMethodSymbol method, MetadataLoadContext metadataLoadContext)
        {
            _method = method;
            _metadataLoadContext = metadataLoadContext;

            Attributes = SharedUtilities.GetMethodAttributes(method);
        }

        public override ICustomAttributeProvider ReturnTypeCustomAttributes => throw new NotImplementedException();

        public override MethodAttributes Attributes { get; }

        public override RuntimeMethodHandle MethodHandle => throw new NotSupportedException();

        public override Type DeclaringType => _method.ContainingType.AsType(_metadataLoadContext);

        public override Type ReturnType => _method.ReturnType.AsType(_metadataLoadContext);

        public override string Name => _method.Name;

        public override bool IsGenericMethod => _method.IsGenericMethod;

        public override Type ReflectedType => throw new NotImplementedException();

        public IMethodSymbol MethodSymbol => _method;

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return SharedUtilities.GetCustomAttributesData(_method, _metadataLoadContext);
        }

        public override MethodInfo GetBaseDefinition()
        {
            var method = _method;

            // Walk until we find the base definition for this method
            while (method.OverriddenMethod is not null)
            {
                method = method.OverriddenMethod;
            }

            if (method.Equals(_method, SymbolEqualityComparer.Default))
            {
                return this;
            }

            return method.AsMethodInfo(_metadataLoadContext);
        }

        public override MethodInfo MakeGenericMethod(params Type[] typeArguments)
        {
            var typeSymbols = new ITypeSymbol[typeArguments.Length];
            for (int i = 0; i < typeSymbols.Length; i++)
            {
                typeSymbols[i] = _metadataLoadContext.ResolveType(typeArguments[i]).GetTypeSymbol();
            }
            return _method.Construct(typeSymbols).AsMethodInfo(_metadataLoadContext);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotSupportedException();
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotSupportedException();
        }

        public override Type[] GetGenericArguments()
        {
            List<Type> typeArguments = default;
            foreach (var t in _method.TypeArguments)
            {
                typeArguments ??= new();
                typeArguments.Add(t.AsType(_metadataLoadContext));
            }
            return typeArguments?.ToArray() ?? Array.Empty<Type>();
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            throw new NotImplementedException();
        }

        public override ParameterInfo[] GetParameters()
        {
            List<ParameterInfo> parameters = default;
            foreach (var p in _method.Parameters)
            {
                parameters ??= new();
                parameters.Add(p.AsParameterInfo(_metadataLoadContext));
            }
            return parameters?.ToArray() ?? Array.Empty<ParameterInfo>();
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotSupportedException();
        }

        public override string ToString() => _method.ToString();
    }
}
#nullable restore
