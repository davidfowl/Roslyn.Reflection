using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.CodeAnalysis;

#nullable disable
namespace Roslyn.Reflection
{
    internal class SharedUtilities
    {
        public static IList<CustomAttributeData> GetCustomAttributesData(ISymbol symbol, MetadataLoadContext metadataLoadContext)
        {
            List<CustomAttributeData> attributes = default;
            foreach (var a in symbol.GetAttributes())
            {
                attributes ??= new();
                attributes.Add(new RoslynCustomAttributeData(a, metadataLoadContext));
            }
            return (IList<CustomAttributeData>)attributes ?? Array.Empty<CustomAttributeData>();
        }

        public static MethodAttributes GetMethodAttributes(IMethodSymbol method)
        {
            MethodAttributes attributes = default;

            if (method.IsAbstract)
            {
                attributes |= MethodAttributes.Abstract | MethodAttributes.Virtual;
            }

            if (method.IsStatic)
            {
                attributes |= MethodAttributes.Static;
            }

            if (method.IsVirtual || method.IsOverride)
            {
                attributes |= MethodAttributes.Virtual;
            }

            switch (method.DeclaredAccessibility)
            {
                case Accessibility.Public:
                    attributes |= MethodAttributes.Public;
                    break;
                case Accessibility.Private:
                    attributes |= MethodAttributes.Private;
                    break;
                case Accessibility.Internal:
                    attributes |= MethodAttributes.Assembly;
                    break;
            }

            if (method.MethodKind != MethodKind.Ordinary)
            {
                attributes |= MethodAttributes.SpecialName;
            }

            return attributes;
        }

        public static bool MatchBindingFlags(BindingFlags bindingFlags, ITypeSymbol thisType, ISymbol symbol)
        {
            var isPublic = (symbol.DeclaredAccessibility & Accessibility.Public) == Accessibility.Public;
            var isNonProtectedInternal = (symbol.DeclaredAccessibility & Accessibility.ProtectedOrInternal) == 0;
            var isStatic = symbol.IsStatic;
            var isInherited = !SymbolEqualityComparer.Default.Equals(thisType, symbol.ContainingType);

            // TODO: REVIEW precomputing binding flags
            // From https://github.com/dotnet/runtime/blob/9ec7fc21862f3446c6c6f7dcfff275942e3884d3/src/coreclr/System.Private.CoreLib/src/System/RuntimeType.CoreCLR.cs#L2058

            //var symbolBindingFlags = ComputeBindingFlags(isPublic, isStatic, isInherited);

            //if (symbol is ITypeSymbol && !isStatic)
            //{
            //    symbolBindingFlags &= ~BindingFlags.Instance;
            //}

            // The below logic is a mishmash of copied logic from the following

            // https://github.com/dotnet/runtime/blob/9ec7fc21862f3446c6c6f7dcfff275942e3884d3/src/coreclr/System.Private.CoreLib/src/System/RuntimeType.CoreCLR.cs#L2261

            // filterFlags ^= BindingFlags.DeclaredOnly;

            // https://github.com/dotnet/runtime/blob/9ec7fc21862f3446c6c6f7dcfff275942e3884d3/src/coreclr/System.Private.CoreLib/src/System/RuntimeType.CoreCLR.cs#L2153

            //if ((filterFlags & symbolBindingFlags) != symbolBindingFlags)
            //{
            //    return false;
            //}

            // Filter by Public & Private
            if (isPublic)
            {
                if ((bindingFlags & BindingFlags.Public) == 0)
                {
                    return false;
                }
            }
            else
            {
                if ((bindingFlags & BindingFlags.NonPublic) == 0)
                {
                    return false;
                }
            }

            // Filter by DeclaredOnly
            if ((bindingFlags & BindingFlags.DeclaredOnly) != 0 && isInherited)
            {
                return false;
            }

            if (symbol is not ITypeSymbol)
            {
                if (isStatic)
                {
                    if ((bindingFlags & BindingFlags.FlattenHierarchy) == 0 && isInherited)
                    {
                        return false;
                    }

                    if ((bindingFlags & BindingFlags.Static) == 0)
                    {
                        return false;
                    }
                }
                else
                {
                    if ((bindingFlags & BindingFlags.Instance) == 0)
                    {
                        return false;
                    }
                }
            }

            // @Asymmetry - Internal, inherited, instance, non -protected, non-virtual, non-abstract members returned
            //              iff BindingFlags !DeclaredOnly, Instance and Public are present except for fields
            if (((bindingFlags & BindingFlags.DeclaredOnly) == 0) &&        // DeclaredOnly not present
                 isInherited &&                                            // Is inherited Member

                isNonProtectedInternal &&                                 // Is non-protected internal member
                ((bindingFlags & BindingFlags.NonPublic) != 0) &&           // BindingFlag.NonPublic present

                (!isStatic) &&                                              // Is instance member
                ((bindingFlags & BindingFlags.Instance) != 0))              // BindingFlag.Instance present
            {
                if (symbol is not IMethodSymbol method)
                {
                    return false;
                }

                if (!method.IsVirtual && !method.IsAbstract)
                {
                    return false;
                }
            }

            return true;
        }

        public static BindingFlags ComputeBindingFlags(MemberInfo member)
        {
            if (member is PropertyInfo p)
            {
                return ComputeBindingFlags(p.GetMethod ?? p.SetMethod);
            }

            var (isPublic, isStatic) = member switch
            {
                FieldInfo f => (f.IsPublic, f.IsStatic),
                MethodInfo m => (m.IsPublic, m.IsStatic),
                _ => throw new NotSupportedException()
            };

            var isInherited = member.ReflectedType != member.DeclaringType;

            return ComputeBindingFlags(isPublic, isStatic, isInherited);
        }

        private static BindingFlags ComputeBindingFlags(bool isPublic, bool isStatic, bool isInherited)
        {
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
#nullable restore
