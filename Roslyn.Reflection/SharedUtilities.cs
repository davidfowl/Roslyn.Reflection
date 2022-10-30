using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Roslyn.Reflection
{
    internal class SharedUtilities
    {
        public static BindingFlags ComputeBindingFlags(ISymbol symbol)
        {
            var isPublic = (symbol.DeclaredAccessibility & Accessibility.Public) == Accessibility.Public;
            var isStatic = symbol.IsStatic;
            var isInherited = !SymbolEqualityComparer.Default.Equals(symbol.OriginalDefinition.ContainingType, symbol.ContainingType);

            // From https://github.com/dotnet/runtime/blob/9ec7fc21862f3446c6c6f7dcfff275942e3884d3/src/coreclr/System.Private.CoreLib/src/System/RuntimeType.CoreCLR.cs#L2058

            BindingFlags bindingFlags = isPublic ? BindingFlags.Public : BindingFlags.NonPublic;

            if (isInherited)
            {
                // We arrange things so the DeclaredOnly flag means "include inherited members"
                bindingFlags |= BindingFlags.DeclaredOnly;

                if (isStatic)
                {
                    bindingFlags |= BindingFlags.Static | BindingFlags.FlattenHierarchy;
                }
                else
                {
                    bindingFlags |= BindingFlags.Instance;
                }
            }
            else
            {
                if (isStatic)
                {
                    bindingFlags |= BindingFlags.Static;
                }
                else
                {
                    bindingFlags |= BindingFlags.Instance;
                }
            }

            return bindingFlags;
        }

    }
}
