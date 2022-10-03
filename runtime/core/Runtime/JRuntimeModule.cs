using System;
using System.Collections.Generic;
using runtime.core.Containers;
using runtime.core.JIL;

namespace runtime.core.Dynamic
{
    //High Read/Write Speed for Globals
    //High Read for Names
    //Slow Write for Names
    //Low Memory for Names & Globals
    public class JRuntimeModule : JRuntimeExpr, IJModule
    {
        private readonly string _name;
        private readonly JModuleFlags _moduleModifiers;
        private readonly JRuntimeModule _parentModule;
        private readonly Dictionary<string, object> _names = new();

        public IContainer<IJType> DeclaredTypes => throw new NotSupportedException();
        public IContainer<IJModule> DeclaredModules => throw new NotSupportedException();
        public IContainer<IJMethod> DeclaredMethods => throw new NotSupportedException();

        public IJModule ParentModule
        {
            get => _parentModule;
            set => throw new NotSupportedException();
        }

        public string Name
        {
            get => _name;
            set => throw new NotSupportedException();
        }

        public JModuleFlags ModuleModifiers
        {
            get => _moduleModifiers;
            set => throw new NotSupportedException();
        }

        internal JRuntimeModule(JILModule m, JRuntimeModule parentModule)
        {
            _name = name;
            _moduleModifiers = moduleFlags;
            _parentModule = parentModule;
        }


        public bool GetNameV(string name, out object v, bool throwOnError = true)
        {
            if (GetName(name, out IJVar rv, throwOnError))
            {
                v = rv.ObjectValue;
                return true;
            }

            v = default;
            return false;
        }

        public bool GetNameV<T>(string name, out T v, bool throwOnError = true)
        {
            if (GetName(name, out IJVar rv, throwOnError))
            {
                v = ((IJVar<T>)rv).Value;
                return true;
            }

            v = default;
            return false;
        }

        public bool GetName(string name, out IJVar v, bool throwOnError = true)
        {
            if (GetNameImpl(name, out v, throwOnError))
                return true;
            if (throwOnError)
                throw new JuliaException("Name \"" + name + "\" does not exist!");
            return false;
        }

        public bool CreateName(IJField f, out int nameIdx, bool throwOnError = true)
        {
            if (GetName(f.Name, out IJVar r, false))
            {
                if (r.IsConst)
                {
                    if (throwOnError)
                        throw new JuliaException("Cannot Redefine Constant \"" + f.Name + "\"");
                    nameIdx = default;
                    return false;
                }

                nameIdx = r.NameRef;
                return true;
            }

            var var = CreateNameImpl(f, throwOnError);
            if (var != null)
            {
                nameIdx = var.NameRef;
                return true;
            }

            nameIdx = default;

            if (throwOnError)
                throw new JuliaException("Cannot Create Name \"" + f.Name + "\"");

            return false;
        }

        protected abstract bool GetNameImpl(string name, out IJVar v, bool throwOnError);
        protected abstract IJVar CreateNameImpl(IJField f, bool throwOnError);
    }
}