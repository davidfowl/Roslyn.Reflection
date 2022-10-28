using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace System.Reflection
{
    internal class RoslynType : Type
    {
        private readonly ITypeSymbol _typeSymbol;
        private readonly MetadataLoadContext _metadataLoadContext;
        private readonly bool _isByRef;

        public RoslynType(ITypeSymbol typeSymbol, MetadataLoadContext metadataLoadContext, bool isByRef = false)
        {
            _typeSymbol = typeSymbol;
            _metadataLoadContext = metadataLoadContext;
            _isByRef = isByRef;
        }

        public override Assembly Assembly => new RoslynAssembly(_typeSymbol.ContainingAssembly, _metadataLoadContext);

        public override string AssemblyQualifiedName => throw new NotImplementedException();

        public override Type BaseType => _typeSymbol.BaseType.AsType(_metadataLoadContext);

        public override string FullName => Namespace == null || Namespace == "<global namespace>" ? Name : Namespace + "." + Name;

        public override Guid GUID => Guid.Empty;

        public override Module Module => throw new NotImplementedException();

        public override string Namespace => _typeSymbol.ContainingNamespace?.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat.WithGlobalNamespaceStyle(SymbolDisplayGlobalNamespaceStyle.OmittedAsContaining));

        public override Type UnderlyingSystemType => this;

        public override string Name => _typeSymbol.MetadataName;

        public override bool IsGenericType => NamedTypeSymbol?.IsGenericType ?? false;

        private INamedTypeSymbol NamedTypeSymbol => _typeSymbol as INamedTypeSymbol;

        private IArrayTypeSymbol ArrayTypeSymbol => _typeSymbol as IArrayTypeSymbol;

        public override bool IsGenericTypeDefinition => base.IsGenericTypeDefinition;

        public ITypeSymbol TypeSymbol => _typeSymbol;

        public override bool IsEnum => _typeSymbol.TypeKind == TypeKind.Enum;

        public override bool IsConstructedGenericType => NamedTypeSymbol?.IsUnboundGenericType == false;

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
            var attributes = new List<CustomAttributeData>();
            foreach (var a in _typeSymbol.GetAttributes())
            {
                attributes.Add(new RoslynCustomAttributeData(a, _metadataLoadContext));
            }
            return attributes;
        }

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
        {
            var ctors = new List<ConstructorInfo>();
            foreach (var c in NamedTypeSymbol.Constructors)
            {
                if ((bindingAttr & BindingFlags.Static) != BindingFlags.Static && c.IsStatic)
                {
                    continue;
                }

                ctors.Add(new RoslynConstructorInfo(c, _metadataLoadContext));
            }
            return ctors.ToArray();
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
            throw new NotImplementedException();
        }

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override Type GetInterface(string name, bool ignoreCase)
        {
            throw new NotImplementedException();
        }

        public override Type[] GetInterfaces()
        {
            var interfaces = new List<Type>();
            foreach (var i in _typeSymbol.Interfaces)
            {
                interfaces.Add(i.AsType(_metadataLoadContext));
            }
            return interfaces.ToArray();
        }

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
        {
            var methods = new List<MethodInfo>();
            foreach (var m in _typeSymbol.GetMembers())
            {
                // TODO: Efficiency
                if (m is IMethodSymbol method && !NamedTypeSymbol.Constructors.Contains(method))
                {
                    if ((bindingAttr & BindingFlags.Public) == BindingFlags.Public &&
                        (m.DeclaredAccessibility & Accessibility.Public) == Accessibility.Public)
                    {
                        methods.Add(method.AsMethodInfo(_metadataLoadContext));
                    }
                }
            }
            return methods.ToArray();
        }

        public override Type GetNestedType(string name, BindingFlags bindingAttr)
        {
            throw new NotImplementedException();
        }

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
        {
            var nestedTypes = new List<Type>();
            foreach (var type in _typeSymbol.GetTypeMembers())
            {
                nestedTypes.Add(type.AsType(_metadataLoadContext));
            }
            return nestedTypes.ToArray();
        }

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
        {
            var properties = new List<PropertyInfo>();
            foreach (var item in _typeSymbol.GetMembers())
            {
                if (item is IPropertySymbol property)
                {
                    properties.Add(new RoslynProperty(property, _metadataLoadContext));
                }
            }
            return properties.ToArray();
        }

        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            throw new NotSupportedException();
        }

        public override bool IsDefined(Type attributeType, bool inherit)
        {
            throw new NotImplementedException();
        }

        protected override TypeAttributes GetAttributeFlagsImpl()
        {
            TypeAttributes flags = default;

            if (_typeSymbol.TypeKind == TypeKind.Interface)
            {
                flags |= TypeAttributes.Interface;
            }

            return flags;
        }

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
        {
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        protected override bool IsPrimitiveImpl()
        {
            throw new NotImplementedException();
        }

        public override string ToString()
        {
            return _typeSymbol.ToString();
        }

        public override bool IsAssignableFrom(Type c)
        {
            if (c is RoslynType tr)
            {
                return tr._typeSymbol.AllInterfaces.Contains(_typeSymbol) || (tr.NamedTypeSymbol != null && tr.NamedTypeSymbol.BaseTypes().Contains(_typeSymbol));
            }
            else if (_metadataLoadContext.ResolveType(c) is RoslynType trr)
            {
                return trr._typeSymbol.AllInterfaces.Contains(_typeSymbol) || (trr.NamedTypeSymbol != null && trr.NamedTypeSymbol.BaseTypes().Contains(_typeSymbol));
            }
            return false;
        }

        public override int GetHashCode()
        {
            return _typeSymbol.GetHashCode();
        }

        public override bool Equals(object o)
        {
            if (o is RoslynType tw)
            {
                return _typeSymbol.Equals(tw._typeSymbol, SymbolEqualityComparer.Default);
            }
            else if (o is Type t && _metadataLoadContext.ResolveType(t) is RoslynType tww)
            {
                return _typeSymbol.Equals(tww._typeSymbol, SymbolEqualityComparer.Default);
            }
            else if (o is ITypeSymbol ts)
            {
                return _typeSymbol.Equals(ts, SymbolEqualityComparer.Default);
            }

            return false;
        }

        public override bool Equals(Type o)
        {
            if (o is RoslynType tw)
            {
                return _typeSymbol.Equals(tw._typeSymbol, SymbolEqualityComparer.Default);
            }
            else if (_metadataLoadContext.ResolveType(o) is RoslynType tww)
            {
                return _typeSymbol.Equals(tww._typeSymbol, SymbolEqualityComparer.Default);
            }
            return false;
        }
    }
}
