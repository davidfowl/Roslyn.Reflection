using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis;

namespace Roslyn.Reflection.Tests
{
    public class RoslynMethodInfoTests
    {
        [Fact]
        public void GetMethodBase()
        {
            var compilaton = CreateBasicCompilation(@"
public abstract class BaseType
{
    public abstract void Method();
}
public class DerivedType : BaseType
{
    public override void Method() { }
}
");

            var metadataLoadContext = new MetadataLoadContext(compilaton);

            var derivedType = metadataLoadContext.ResolveType("DerivedType");

            Assert.NotNull(derivedType);

            var method = derivedType.GetMethod("Method");

            Assert.NotNull(method);
            Assert.NotNull(derivedType.BaseType);

            Assert.Equal(derivedType!.BaseType, method!.GetBaseDefinition().DeclaringType);
        }


        [Fact]
        public void MakeGenericMethodWorks()
        {
            var compilaton = CreateBasicCompilation(@"
public class TypeWithGenericMethod
{
    public T Identity<T>(T value) => value;
}
");

            var metadataLoadContext = new MetadataLoadContext(compilaton);

            var typeWithGenericMethod = metadataLoadContext.ResolveType("TypeWithGenericMethod");

            Assert.NotNull(typeWithGenericMethod);

            var method = typeWithGenericMethod.GetMethod("Identity");

            Assert.NotNull(method);
            Assert.True(method!.IsGenericMethod);
            Assert.False(method!.IsGenericMethodDefinition);

            var closedGeneric = method!.MakeGenericMethod(typeof(string));
            Assert.NotNull(closedGeneric);

            Assert.Equal("Identity", closedGeneric.Name);
            Assert.Equal(new[] { metadataLoadContext.ResolveType(typeof(string)) }, closedGeneric.GetGenericArguments());
        }

        private static CSharpCompilation CreateBasicCompilation(string text)
        {
            return CSharpCompilation.Create("something",
                syntaxTrees: new[] { CSharpSyntaxTree.ParseText(text) },
                references: new[] { MetadataReference.CreateFromFile(typeof(object).Assembly.Location)
                });
        }
    }
}
