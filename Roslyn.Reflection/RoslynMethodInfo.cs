using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.Reflection;
using Microsoft.CodeAnalysis;

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

            if (method.IsAbstract)
            {
                Attributes |= MethodAttributes.Abstract | MethodAttributes.Virtual;
            }

            if (method.IsStatic)
            {
                Attributes |= MethodAttributes.Static;
            }

            if (method.IsVirtual || method.IsOverride)
            {
                Attributes |= MethodAttributes.Virtual;
            }

            switch (method.DeclaredAccessibility)
            {
                case Accessibility.Public:
                    Attributes |= MethodAttributes.Public;
                    break;
                case Accessibility.Private:
                    Attributes |= MethodAttributes.Private;
                    break;
                case Accessibility.Internal:
                    Attributes |= MethodAttributes.Assembly;
                    break;
            }
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
            var attributes = new List<CustomAttributeData>();
            foreach (var a in _method.GetAttributes())
            {
                attributes.Add(new RoslynCustomAttributeData(a, _metadataLoadContext));
            }
            return attributes;
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
            var typeArguments = new List<Type>();
            foreach (var t in _method.TypeArguments)
            {
                typeArguments.Add(t.AsType(_metadataLoadContext));
            }
            return typeArguments.ToArray();
        }

        public override MethodImplAttributes GetMethodImplementationFlags()
        {
            throw new NotImplementedException();
        }

        public override ParameterInfo[] GetParameters()
        {
            var parameters = new List<ParameterInfo>();
            foreach (var p in _method.Parameters)
            {
                parameters.Add(new RoslynParameterInfo(p, _metadataLoadContext));
            }
            return parameters.ToArray();
        }

        public override object Invoke(object obj, BindingFlags invokeAttr, Binder binder, object[] parameters, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return _method.ToString();
        }
    }
}