using System;
using System.Collections.Generic;
using System.Globalization;
using System.Reflection;
using Microsoft.CodeAnalysis;

#nullable disable
namespace Roslyn.Reflection
{
    internal class RoslynFieldInfo : FieldInfo
    {
        private readonly IFieldSymbol _field;
        private readonly MetadataLoadContext _metadataLoadContext;
        private FieldAttributes? _attributes;

        public RoslynFieldInfo(IFieldSymbol parameter, MetadataLoadContext metadataLoadContext)
        {
            _field = parameter;
            _metadataLoadContext = metadataLoadContext;
        }

        public IFieldSymbol FieldSymbol => _field;

        public override FieldAttributes Attributes
        {
            get
            {
                if (!_attributes.HasValue)
                {
                    _attributes = default(FieldAttributes);

                    if (_field.IsStatic)
                    {
                        _attributes |= FieldAttributes.Static;
                    }

                    if (_field.IsReadOnly)
                    {
                        _attributes |= FieldAttributes.InitOnly;
                    }

                    switch (_field.DeclaredAccessibility)
                    {
                        case Accessibility.Public:
                            _attributes |= FieldAttributes.Public;
                            break;
                        case Accessibility.Private:
                            _attributes |= FieldAttributes.Private;
                            break;
                        case Accessibility.Protected:
                            _attributes |= FieldAttributes.Family;
                            break;
                    }
                }

                return _attributes.Value;
            }
        }

        public override RuntimeFieldHandle FieldHandle => throw new NotSupportedException();

        public override Type FieldType => _field.Type.AsType(_metadataLoadContext);

        public override Type DeclaringType => _field.ContainingType.AsType(_metadataLoadContext);

        public override string Name => _field.Name;

        public override Type ReflectedType => throw new NotImplementedException();

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotSupportedException();
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotSupportedException();
        }

        public override object GetValue(object obj)
        {
            throw new NotSupportedException();
        }

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return SharedUtilities.GetCustomAttributesData(_field, _metadataLoadContext);
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotSupportedException();
        }

        public override void SetValue(object obj, object value, BindingFlags invokeAttr, Binder binder, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override string ToString() => _field.ToString();
    }
}
#nullable restore
