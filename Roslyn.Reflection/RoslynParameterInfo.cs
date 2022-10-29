using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Roslyn.Reflection
{
    public class RoslynParameterInfo : ParameterInfo
    {
        private readonly IParameterSymbol _parameter;
        private readonly MetadataLoadContext _metadataLoadContext;

        public RoslynParameterInfo(IParameterSymbol parameter, MetadataLoadContext metadataLoadContext)
        {
            _parameter = parameter;
            _metadataLoadContext = metadataLoadContext;
        }

        public IParameterSymbol ParameterSymbol => _parameter;

        public override Type ParameterType => _parameter.Type.AsType(_metadataLoadContext);
        public override string Name => _parameter.Name;
        public override bool HasDefaultValue => _parameter.HasExplicitDefaultValue;

        public override object DefaultValue => HasDefaultValue ? _parameter.ExplicitDefaultValue : null;

        public override int Position => _parameter.Ordinal;

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            var attributes = new List<CustomAttributeData>();
            foreach (var a in _parameter.GetAttributes())
            {
                attributes.Add(new RoslynCustomAttributeData(a, _metadataLoadContext));
            }
            return attributes;
        }
    }
}