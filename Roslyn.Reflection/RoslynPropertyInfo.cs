using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Microsoft.CodeAnalysis;

#nullable disable
namespace Roslyn.Reflection
{
    internal class RoslynPropertyInfo : PropertyInfo
    {
        private readonly IPropertySymbol _property;
        private readonly MetadataLoadContext _metadataLoadContext;

        public RoslynPropertyInfo(IPropertySymbol property, MetadataLoadContext metadataLoadContext)
        {
            _property = property;
            _metadataLoadContext = metadataLoadContext;
        }

        public IPropertySymbol PropertySymbol => _property;

        public override PropertyAttributes Attributes => throw new NotImplementedException();

        public override bool CanRead => _property.GetMethod != null;

        public override bool CanWrite => _property.SetMethod != null;

        public override Type PropertyType => _property.Type.AsType(_metadataLoadContext);

        public override Type DeclaringType => _property.ContainingType.AsType(_metadataLoadContext);

        public override string Name => _property.Name;

        public override Type ReflectedType => throw new NotImplementedException();

        public override MethodInfo[] GetAccessors(bool nonPublic)
        {
            throw new NotImplementedException();
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotSupportedException();
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotSupportedException();
        }

        public override MethodInfo GetGetMethod(bool nonPublic)
        {
            return _property.GetMethod.AsMethodInfo(_metadataLoadContext);
        }

        public override ParameterInfo[] GetIndexParameters()
        {
            List<ParameterInfo> parameters = default;
            foreach (var p in _property.Parameters)
            {
                parameters ??= new();
                parameters.Add(p.AsParameterInfo(_metadataLoadContext));
            }
            return parameters?.ToArray() ?? Array.Empty<ParameterInfo>();
        }

        public override MethodInfo GetSetMethod(bool nonPublic)
        {
            return _property.SetMethod.AsMethodInfo(_metadataLoadContext);
        }

        public override object GetValue(object obj, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, object[] index, CultureInfo culture)
        {
            throw new NotSupportedException();
        }
    }
}
#nullable restore
