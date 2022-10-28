using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace System.Reflection
{
    internal class RoslynCustomAttributeData : CustomAttributeData
    {
        public RoslynCustomAttributeData(AttributeData a, MetadataLoadContext metadataLoadContext)
        {
            if (a.AttributeConstructor is null)
            {
                throw new InvalidOperationException();
            }

            var namedArguments = new List<CustomAttributeNamedArgument>();
            foreach (var na in a.NamedArguments)
            {
                var member = a.AttributeClass.BaseTypes().SelectMany(t => t.GetMembers(na.Key)).First();

                MemberInfo memberInfo = member switch
                {
                    IPropertySymbol property => property.AsPropertyInfo(metadataLoadContext),
                    IFieldSymbol field => field.AsFieldInfo(metadataLoadContext),
                    _ => new RoslynMemberInfo(member, metadataLoadContext)
                };
                namedArguments.Add(new CustomAttributeNamedArgument(memberInfo, na.Value.Value));
            }

            var constructorArguments = new List<CustomAttributeTypedArgument>();
            foreach (var ca in a.ConstructorArguments)
            {
                if (ca.Kind == TypedConstantKind.Error)
                {
                    continue;
                }

                object value = ca.Kind == TypedConstantKind.Array ? ca.Values : ca.Value;
                constructorArguments.Add(new CustomAttributeTypedArgument(ca.Type.AsType(metadataLoadContext), value));
            }
            Constructor = new RoslynConstructorInfo(a.AttributeConstructor, metadataLoadContext);
            NamedArguments = namedArguments;
            ConstructorArguments = constructorArguments;
        }

        public override ConstructorInfo Constructor { get; }

        public override IList<CustomAttributeNamedArgument> NamedArguments { get; }

        public override IList<CustomAttributeTypedArgument> ConstructorArguments { get; }
    }
}