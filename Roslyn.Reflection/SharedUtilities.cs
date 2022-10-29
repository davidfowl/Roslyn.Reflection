using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;
using Microsoft.CodeAnalysis;

namespace Roslyn.Reflection
{
    internal class SharedUtilities
    {
        public static Func<ISymbol, bool> GetPredicateFromBindingFlags(BindingFlags bindingAttr)
        {
            Func<ISymbol, bool> predicate = m => false;
            if ((bindingAttr & BindingFlags.NonPublic) == BindingFlags.NonPublic)
            {
                var previous = predicate;
                predicate = m => previous(m) || (m.DeclaredAccessibility & Accessibility.Private) == Accessibility.Private;
            }

            if ((bindingAttr & BindingFlags.Public) == BindingFlags.Public)
            {
                var previous = predicate;
                predicate = m => previous(m) || (m.DeclaredAccessibility & Accessibility.Public) == Accessibility.Public;
            }

            if ((bindingAttr & BindingFlags.Static) == BindingFlags.Static)
            {
                var previous = predicate;
                predicate = m => previous(m) || m.IsStatic;
            }

            if ((bindingAttr & BindingFlags.Static) == BindingFlags.Instance)
            {
                var previous = predicate;
                predicate = m => previous(m) || !m.IsStatic;
            }

            return predicate;
        }

    }
}
