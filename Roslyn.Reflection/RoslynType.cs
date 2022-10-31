using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis;

namespace Roslyn.Reflection
{
    internal class RoslynType : Type
    {
        private readonly ITypeSymbol _typeSymbol;
        private readonly MetadataLoadContext _metadataLoadContext;
        private readonly bool _isByRef;
        private TypeAttributes? _typeAttributes;

        public RoslynType(ITypeSymbol typeSymbol, MetadataLoadContext metadataLoadContext, bool isByRef = false)
        {
            _typeSymbol = typeSymbol;
            _metadataLoadContext = metadataLoadContext;
            _isByRef = isByRef;
        }

        public override Assembly Assembly => new RoslynAssembly(_typeSymbol.ContainingAssembly, _metadataLoadContext);

        public override string AssemblyQualifiedName => throw new NotImplementedException();

        public override Type BaseType => _typeSymbol.BaseType.AsType(_metadataLoadContext);

        public override string FullName => Namespace is null ? Name : Namespace + "." + Name;

        public override Guid GUID => Guid.Empty;

        public override Module Module => throw new NotImplementedException();

        public override string Namespace => _typeSymbol.ContainingNamespace?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.Omitted)) is { Length: > 0 } ns ? ns : null;

        public override Type UnderlyingSystemType => this;

        public override string Name => ArrayTypeSymbol is { } ar ? ar.ElementType.MetadataName + "[]" : _typeSymbol.MetadataName;

        public override bool IsGenericType => NamedTypeSymbol?.IsGenericType ?? false;

        private INamedTypeSymbol NamedTypeSymbol => _typeSymbol as INamedTypeSymbol;

        private IArrayTypeSymbol ArrayTypeSymbol => _typeSymbol as IArrayTypeSymbol;

        public override bool IsGenericTypeDefinition => IsGenericType && SymbolEqualityComparer.Default.Equals(NamedTypeSymbol, NamedTypeSymbol.ConstructedFrom);

        public override bool IsGenericParameter => _typeSymbol.TypeKind == TypeKind.TypeParameter;

        public ITypeSymbol TypeSymbol => _typeSymbol;

        public override bool IsEnum => _typeSymbol.TypeKind == TypeKind.Enum;

        public override bool IsConstructedGenericType => NamedTypeSymbol?.IsUnboundGenericType == false;

        public override Type DeclaringType => _typeSymbol.ContainingType?.AsType(_metadataLoadContext);

        public override int GetArrayRank()
        {
            return ArrayTypeSymbol.Rank;
        }

        public override Type[] GetGenericArguments()
        {
            if (NamedTypeSymbol is null) return Array.Empty<Type>();

            var args = new List<Type>();
            foreach (var item in NamedTypeSymbol.TypeArguments)
            {
                args.Add(item.AsType(_metadataLoadContext));
            }
            return args.ToArray();
        }

        public override Type GetGenericTypeDefinition()
        {
            return NamedTypeSymbol?.ConstructedFrom.AsType(_metadataLoadContext) ?? throw new NotSupportedException();
        }

        public override IList<CustomAttributeData> GetCustomAttributesData()
        {
            return SharedUtilities.GetCustomAttributesData(_typeSymbol, _metadataLoadContext);
        }

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            if (NamedTypeSymbol is null)
            {
                return Array.Empty<ConstructorInfo>();
            }

            List<ConstructorInfo> ctors = default;
            foreach (var c in NamedTypeSymbol.Constructors)
            {
                var flags = SharedUtilities.ComputeBindingFlags(c);

                if ((flags & bindingAttr) != flags)
                {
                    continue;
                }

                ctors ??= new();
                ctors.Add(new RoslynConstructorInfo(c, _metadataLoadContext));
            }
            return ctors?.ToArray() ?? Array.Empty<ConstructorInfo>();
        }

        public override Type MakeByRefType()
        {
            return new RoslynType(_typeSymbol, _metadataLoadContext, isByRef: true);
        }

        public override object[] GetCustomAttributes(bool inherit)
        {
            throw new NotSupportedException();
        }

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
        {
            throw new NotSupportedException();
        }

        public override Type MakeArrayType()
        {
            return _metadataLoadContext.Compilation.CreateArrayTypeSymbol(_typeSymbol).AsType(_metadataLoadContext);
        }

        public override Type MakeGenericType(params Type[] typeArguments)
        {
            if (!IsGenericTypeDefinition)
            {
                throw new NotSupportedException();
            }

            var typeSymbols = new ITypeSymbol[typeArguments.Length];
            for (int i = 0; i < typeArguments.Length; i++)
            {
                typeSymbols[i] = _metadataLoadContext.ResolveType(typeArguments[i]).GetTypeSymbol();
            }

            return NamedTypeSymbol.Construct(typeSymbols).AsType(_metadataLoadContext);
        }

        public override Type GetElementType()
        {
            return ArrayTypeSymbol?.ElementType.AsType(_metadataLoadContext);
        }

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
        {
            foreach (var symbol in _typeSymbol.GetMembers())
            {
                var flags = SharedUtilities.ComputeBindingFlags(symbol);
                if (symbol is not IFieldSymbol fieldSymbol)
                {
                    continue;
                }

                if ((flags & bindingAttr) != flags)
                {
                    continue;
                }

                return fieldSymbol.AsFieldInfo(_metadataLoadContext);
            }

            return null;
        }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            List<FieldInfo> fields = default;

            foreach (var symbol in _typeSymbol.GetMembers())
            {
                if (symbol is not IFieldSymbol fieldSymbol)
                {
                    continue;
                }

                var flags = SharedUtilities.ComputeBindingFlags(symbol);

                if ((flags & bindingAttr) != flags)
                {
                    continue;
                }

                fields ??= new();
                fields.Add(new RoslynFieldInfo(fieldSymbol, _metadataLoadContext));
            }

            return fields?.ToArray() ?? Array.Empty<FieldInfo>();
        }

        public override Type GetInterface(string name, bool ignoreCase)
        {
            var comparison = ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
            foreach (var i in _typeSymbol.Interfaces)
            {
                if (i.Name.Equals(name, comparison))
                {
                    return i.AsType(_metadataLoadContext);
                }
            }
            return null;
        }

        public override Type[] GetInterfaces()
        {
            List<Type> interfaces = default;
            foreach (var i in _typeSymbol.Interfaces)
            {
                interfaces ??= new();
                interfaces.Add(i.AsType(_metadataLoadContext));
            }
            return interfaces?.ToArray() ?? Array.Empty<Type>();
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            List<MemberInfo> members = null;

            foreach (var symbol in _typeSymbol.GetMembers())
            {
                var flags = SharedUtilities.ComputeBindingFlags(symbol);

                if ((flags & bindingAttr) != flags)
                {
                    continue;
                }

                MemberInfo member = symbol switch
                {
                    IFieldSymbol f => f.AsFieldInfo(_metadataLoadContext),
                    IPropertySymbol p => p.AsPropertyInfo(_metadataLoadContext),
                    IMethodSymbol m => m.AsMethodInfo(_metadataLoadContext),
                    _ => null
                };

                if (member is null)
                {
                    continue;
                }

                members ??= new();
                members.Add(member);
            }

            // https://github.com/dotnet/runtime/blob/9ec7fc21862f3446c6c6f7dcfff275942e3884d3/src/coreclr/System.Private.CoreLib/src/System/RuntimeType.CoreCLR.cs#L2693-L2694
            bindingAttr &= ~BindingFlags.Static;
            foreach (var type in GetNestedTypes(bindingAttr))
            {
                if (type.IsInterface)
                {
                    continue;
                }

                members ??= new();
                members.Add(type);
            }

            return members?.ToArray() ?? Array.Empty<MemberInfo>();
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            List<MethodInfo> methods = null;

            foreach (var m in _typeSymbol.GetMembers())
            {
                if (m is not IMethodSymbol method || method.MethodKind == MethodKind.Constructor)
                {
                    // Only methods that are not constructors
                    continue;
                }

                var flags = SharedUtilities.ComputeBindingFlags(m);

                if ((flags & bindingAttr) != flags)
                {
                    continue;
                }

                methods ??= new();
                methods.Add(method.AsMethodInfo(_metadataLoadContext));
            }

            return methods?.ToArray() ?? Array.Empty<MethodInfo>();
        }

        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            foreach (var type in _typeSymbol.GetTypeMembers(name))
            {
                var flags = SharedUtilities.ComputeBindingFlags(type);
                if ((flags & bindingAttr) != flags)
                {
                    continue;
                }

                return type.AsType(_metadataLoadContext);
            }
            return null;
        }

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            List<Type> nestedTypes = default;
            foreach (var type in _typeSymbol.GetTypeMembers())
            {
                var flags = SharedUtilities.ComputeBindingFlags(type);
                if ((flags & bindingAttr) != flags)
                {
                    continue;
                }

                nestedTypes ??= new();
                nestedTypes.Add(type.AsType(_metadataLoadContext));
            }
            return nestedTypes?.ToArray() ?? Array.Empty<Type>();
        }

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            List<PropertyInfo> properties = default;
            foreach (var symbol in _typeSymbol.GetMembers())
            {
                if (symbol is not IPropertySymbol property)
                {
                    continue;
                }

                var flags = SharedUtilities.ComputeBindingFlags(symbol);
                if ((flags & bindingAttr) != flags)
                {
                    continue;
                }

                properties ??= new();
                properties.Add(new RoslynPropertyInfo(property, _metadataLoadContext));
            }
            return properties?.ToArray() ?? Array.Empty<PropertyInfo>();
        }

        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            throw new NotSupportedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotSupportedException();
        }

        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            if (!_typeAttributes.HasValue)
            {
                _typeAttributes = default(TypeAttributes);

                if (_typeSymbol.IsAbstract)
                {
                    _typeAttributes |= TypeAttributes.Abstract;
                }

                if (_typeSymbol.TypeKind == TypeKind.Interface)
                {
                    _typeAttributes |= TypeAttributes.Interface;
                }

                if (_typeSymbol.IsSealed)
                {
                    _typeAttributes |= TypeAttributes.Sealed;
                }

                bool isNested = _typeSymbol.ContainingType != null;

                switch (_typeSymbol.DeclaredAccessibility)
                {
                    case Accessibility.NotApplicable:
                    case Accessibility.Private:
                        _typeAttributes |= isNested ? TypeAttributes.NestedPrivate : TypeAttributes.NotPublic;
                        break;
                    case Accessibility.ProtectedAndInternal:
                        _typeAttributes |= isNested ? TypeAttributes.NestedFamANDAssem : TypeAttributes.NotPublic;
                        break;
                    case Accessibility.Protected:
                        _typeAttributes |= isNested ? TypeAttributes.NestedFamily : TypeAttributes.NotPublic;
                        break;
                    case Accessibility.Internal:
                        _typeAttributes |= isNested ? TypeAttributes.NestedAssembly : TypeAttributes.NotPublic;
                        break;
                    case Accessibility.ProtectedOrInternal:
                        _typeAttributes |= isNested ? TypeAttributes.NestedFamORAssem : TypeAttributes.NotPublic;
                        break;
                    case Accessibility.Public:
                        _typeAttributes |= isNested ? TypeAttributes.NestedPublic : TypeAttributes.Public;
                        break;
                }
            }

            return _typeAttributes.Value;
        }

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            // TODO: Use callConvention and modifiers
            StringComparison comparison = (bindingAttr & BindingFlags.IgnoreCase) == BindingFlags.IgnoreCase
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            foreach (var m in _typeSymbol.GetMembers())
            {
                if (m is not IMethodSymbol method || method.MethodKind != MethodKind.Constructor)
                {
                    // Only methods that are constructors
                    continue;
                }

                var flags = SharedUtilities.ComputeBindingFlags(m);

                if ((flags & bindingAttr) != flags)
                {
                    continue;
                }

                var parameterCount = types?.Length ?? 0;

                // Compare parameter types
                if (parameterCount != method.Parameters.Length)
                {
                    continue;
                }

                var valid = true;
                for (int i = 0; i < parameterCount; i++)
                {
                    var parameterType = types[i];
                    var parameterTypeSymbol = _metadataLoadContext.ResolveType(parameterType)?.GetTypeSymbol();

                    if (parameterTypeSymbol is null)
                    {
                        valid = false;
                        break;
                    }

                    if (!method.Parameters[i].Type.Equals(parameterTypeSymbol, SymbolEqualityComparer.Default))
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                {
                    return new RoslynConstructorInfo(method, _metadataLoadContext);
                }
            }

            return null;
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            // TODO: Use callConvention and modifiers
            StringComparison comparison = (bindingAttr & BindingFlags.IgnoreCase) == BindingFlags.IgnoreCase
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            foreach (var m in _typeSymbol.GetMembers())
            {
                if (m is not IMethodSymbol method || method.MethodKind == MethodKind.Constructor)
                {
                    // Only methods that are not constructors
                    continue;
                }

                var flags = SharedUtilities.ComputeBindingFlags(m);

                if ((flags & bindingAttr) != flags)
                {
                    continue;
                }

                if (!method.Name.Equals(name, comparison))
                {
                    continue;
                }

                var parameterCount = types?.Length ?? 0;

                // Compare parameter types
                if (parameterCount != method.Parameters.Length)
                {
                    continue;
                }

                var valid = true;
                for (int i = 0; i < parameterCount; i++)
                {
                    var parameterType = types[i];
                    var parameterTypeSymbol = _metadataLoadContext.ResolveType(parameterType)?.GetTypeSymbol();

                    if (parameterTypeSymbol is null)
                    {
                        valid = false;
                        break;
                    }

                    if (!method.Parameters[i].Type.Equals(parameterTypeSymbol, SymbolEqualityComparer.Default))
                    {
                        valid = false;
                        break;
                    }
                }

                if (valid)
                {
                    return method.AsMethodInfo(_metadataLoadContext);
                }
            }

            return null;
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            StringComparison comparison = (bindingAttr & BindingFlags.IgnoreCase) == BindingFlags.IgnoreCase
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            foreach (var symbol in _typeSymbol.GetMembers())
            {
                if (symbol is not IPropertySymbol property)
                {
                    continue;
                }

                var flags = SharedUtilities.ComputeBindingFlags(symbol);
                if ((flags & bindingAttr) != flags)
                {
                    continue;
                }

                if (!property.Name.Equals(name, comparison))
                {
                    continue;
                }

                var roslynReturnType = _metadataLoadContext.ResolveType(returnType);

                if (roslynReturnType?.Equals(property.Type) == false)
                {
                    continue;
                }

                var parameterCount = types?.Length ?? 0;

                // Compare parameter types
                if (parameterCount  != property.Parameters.Length)
                {
                    continue;
                }

                // TODO: Use parameters

                return property.AsPropertyInfo(_metadataLoadContext);

            }
            return null;
        }

        protected override bool HasElementTypeImpl()
        {
            return ArrayTypeSymbol is not null;
        }

        protected override bool IsArrayImpl()
        {
            return ArrayTypeSymbol is not null;
        }

        protected override bool IsByRefImpl() => _isByRef;

        protected override bool IsCOMObjectImpl()
        {
            throw new NotImplementedException();
        }

        protected override bool IsPointerImpl()
        {
            return _typeSymbol.Kind == SymbolKind.PointerType;
        }

        protected override bool IsPrimitiveImpl()
        {
            // Is IsPrimitive
            // https://github.com/dotnet/runtime/blob/55e95c80a7d7ec9d7bbbd5ad434604a1dc33e19c/src/libraries/System.Reflection.MetadataLoadContext/src/System/Reflection/TypeLoading/Types/RoType.TypeClassification.cs#L85

            return _typeSymbol.SpecialType switch
            {
                SpecialType.System_Boolean => true,
                SpecialType.System_Char => true,
                SpecialType.System_SByte => true,
                SpecialType.System_Byte => true,
                SpecialType.System_Int16 => true,
                SpecialType.System_UInt16 => true,
                SpecialType.System_Int32 => true,
                SpecialType.System_UInt32 => true,
                SpecialType.System_Int64 => true,
                SpecialType.System_UInt64 => true,
                SpecialType.System_Single => true,
                SpecialType.System_Double => true,
                SpecialType.System_String => true,
                SpecialType.System_IntPtr => true,
                SpecialType.System_UIntPtr => true,
                _ => false
            };
        }

        public override string ToString()
        {
            return _typeSymbol.ToString();
        }

        public override bool IsAssignableFrom(Type c)
        {
            if (c is RoslynType rt)
            {
                return rt._typeSymbol.AllInterfaces.Contains(_typeSymbol, SymbolEqualityComparer.Default) || (rt.NamedTypeSymbol != null && rt.NamedTypeSymbol.BaseTypes().Contains(_typeSymbol, SymbolEqualityComparer.Default));
            }
            else if (_metadataLoadContext.ResolveType(c) is RoslynType rtt)
            {
                return rtt._typeSymbol.AllInterfaces.Contains(_typeSymbol, SymbolEqualityComparer.Default) || (rtt.NamedTypeSymbol != null && rtt.NamedTypeSymbol.BaseTypes().Contains(_typeSymbol, SymbolEqualityComparer.Default));
            }
            return false;
        }

        public override int GetHashCode()
        {
            return SymbolEqualityComparer.Default.GetHashCode(_typeSymbol);
        }

        public override bool Equals(object o)
        {
            if (o is RoslynType rt)
            {
                return _typeSymbol.Equals(rt._typeSymbol, SymbolEqualityComparer.Default);
            }
            else if (o is Type t && _metadataLoadContext.ResolveType(t) is RoslynType rtt)
            {
                return _typeSymbol.Equals(rtt._typeSymbol, SymbolEqualityComparer.Default);
            }
            else if (o is ITypeSymbol ts)
            {
                return _typeSymbol.Equals(ts, SymbolEqualityComparer.Default);
            }

            return false;
        }

        public override bool Equals(Type o)
        {
            if (o is RoslynType rt)
            {
                return _typeSymbol.Equals(rt._typeSymbol, SymbolEqualityComparer.Default);
            }
            else if (_metadataLoadContext.ResolveType(o) is RoslynType rtt)
            {
                return _typeSymbol.Equals(rtt._typeSymbol, SymbolEqualityComparer.Default);
            }
            return false;
        }
    }
}
