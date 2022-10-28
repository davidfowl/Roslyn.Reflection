using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace System.Reflection
{
    public class RoslynParameter : ParameterInfo
    {
        private readonly IParameterSymbol _parameter;
        private readonly MetadataLoadContext _metadataLoadContext;

        public RoslynParameter(IParameterSymbol parameter, MetadataLoadContext metadataLoadContext)
        {
            _parameter = parameter;
            _metadataLoadContext = metadataLoadContext;
        }

        public IParameterSymbol ParameterSymbol => _parameter;

        public override Type ParameterType => _parameter.Type.AsType(_metadataLoadContext);
        public override string Name => _parameter.Name;

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