using System;
using runtime.core.Containers;
using runtime.core.JIL;

namespace runtime.core.Dynamic
{

    public class JRuntimeVar<T> : IJVar<T>
    {
        private readonly IJField _field;
        private T _v;
        private JNameRef _nameRef;

        public JRuntimeVar(IJField field, JNameRef nameRef, T v = default) {
            _field = field;
            _v = v;
            _nameRef = nameRef;
        }

        public T Value { get => _v; set => _v = value; }
        public object ObjectValue { get => _v; set => _v = (T) value; }
        public string Name { get => _field.Name; set => _field.Name = value; }
        public IJType Type { get => _field.Type; set => _field.Type = value; }
        public JFieldFlags Modifiers { get => _field.Modifiers; set => _field.Modifiers = value; }
        public JNameRef NameRef => _nameRef;

        public IJType GetJType() => throw new NotImplementedException();
    }
    
    public class JRuntimeType : IJType{
        public readonly JRuntimeModule RuntimeModule;

        public string Name { get; set; }
        public JTypeType Type { get; set; }
        public IContainer<IJField> Fields { get; }
        public IContainer<IJMethod> Constructors { get; }
    }
}