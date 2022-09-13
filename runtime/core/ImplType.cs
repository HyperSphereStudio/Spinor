using System;
using System.Globalization;
using System.Reflection;

namespace runtime.core
{
    public class ImplType : Type {
        protected const BindingFlags FLAGS = BindingFlags.Instance | BindingFlags.Static | BindingFlags.Public |
                                             BindingFlags.NonPublic;
        
        protected Type _t;

        public ImplType(Type t) => _t = t;

        public override Module Module => _t.Module;
        public override string Namespace => _t.Namespace;
        public override Type UnderlyingSystemType => _t;
        protected override bool IsArrayImpl() => _t.IsArray;
        protected override bool IsByRefImpl() => _t.IsByRef;
        protected override bool IsCOMObjectImpl() => _t.IsCOMObject;
        protected override bool IsPointerImpl() => _t.IsPointer;
        protected override bool IsPrimitiveImpl() => _t.IsPrimitive;
        public override Assembly Assembly => _t.Assembly;
        public override string AssemblyQualifiedName => _t.AssemblyQualifiedName;
        public override Type BaseType => null;
        public override string FullName => _t.FullName;
        protected override bool HasElementTypeImpl() => _t.HasElementType;
        public override Guid GUID => _t.GUID;
        public override string Name => _t.Name;
        
        
        public override object[] GetCustomAttributes(bool inherit) => _t.GetCustomAttributes(inherit);
        public override object[] GetCustomAttributes(Type attributeType, bool inherit) => _t.GetCustomAttributes(attributeType, inherit);
        public override bool IsDefined(Type attributeType, bool inherit) => _t.IsDefined(attributeType, inherit);
        protected override TypeAttributes GetAttributeFlagsImpl() =>  throw new NotImplementedException();

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention,
            Type[] types, ParameterModifier[] modifiers) => throw new NotImplementedException();

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr) => _t.GetConstructors(FLAGS);
        public override Type GetElementType() => _t.GetElementType();
        public override EventInfo GetEvent(string name, BindingFlags bindingAttr) => _t.GetEvent(name, FLAGS);
        public override EventInfo[] GetEvents(BindingFlags bindingAttr) => _t.GetEvents(FLAGS);
        public override FieldInfo GetField(string name, BindingFlags bindingAttr) => _t.GetField(name, FLAGS);
        public override FieldInfo[] GetFields(BindingFlags bindingAttr) => _t.GetFields(FLAGS);
        public override MemberInfo[] GetMembers(BindingFlags bindingAttr) => _t.GetMembers(FLAGS);

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder,
            CallingConventions callConvention,
            Type[] types, ParameterModifier[] modifiers) => _t.GetMethod(name, bindingAttr, binder, callConvention, types ?? EmptyTypes, modifiers);

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr) => _t.GetMethods(FLAGS);
        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr) => _t.GetProperties(FLAGS);

        public override object InvokeMember(string name, 
            BindingFlags invokeAttr, 
            Binder binder, 
            object target,
            object[] args,
            ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            return _t.InvokeMember(name, invokeAttr, binder, target, args, modifiers, culture, namedParameters);
        }

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder,
            Type returnType, Type[] types,
            ParameterModifier[] modifiers) => _t.GetProperty(name, bindingAttr, binder, returnType, types, modifiers);
        public override Type GetNestedType(string name, BindingFlags bindingAttr) => _t.GetNestedType(name, FLAGS);
        public override Type[] GetNestedTypes(BindingFlags bindingAttr) => _t.GetNestedTypes(FLAGS);
        public override Type GetInterface(string name, bool ignoreCase) => _t.GetInterface(name, ignoreCase);
        public override Type[] GetInterfaces() => _t.GetInterfaces();
        public override bool Equals(Type t) {
            if(this != t) 
                return t.Equals(UnderlyingSystemType);
            return true;
        }

        public virtual void SetInternalType(Type t) => _t = t;

    }
}