using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Metadata;
using System.Reflection.Metadata.Ecma335;

namespace Lokad.ILPack.Metadata
{
    internal partial class AssemblyMetadata : IAssemblyMetadata
    {
        private sealed class AssemblyNameComparer : IEqualityComparer<AssemblyName>
        {
            public bool Equals(AssemblyName x, AssemblyName y) =>
                string.Equals(x?.Name, y?.Name, StringComparison.Ordinal);

            public int GetHashCode(AssemblyName obj) =>
                obj.Name.GetHashCode();
        }

        private const BindingFlags AllMethods =
            BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic |
            BindingFlags.DeclaredOnly | BindingFlags.CreateInstance | BindingFlags.Instance;

        private readonly Dictionary<string, AssemblyReferenceHandle> _assemblyRefHandles;
        private readonly Dictionary<ConstructorInfo, MethodBaseDefinitionMetadata> _ctorDefHandles;
        private readonly Dictionary<ConstructorInfo, MemberReferenceHandle> _ctorRefHandles;
        private readonly Dictionary<FieldInfo, FieldDefinitionMetadata> _fieldDefHandles;
        private readonly Dictionary<int, FieldInfo> _unconstructedFieldDefs;
        private readonly Dictionary<FieldInfo, MemberReferenceHandle> _fieldRefHandles;
        private readonly Dictionary<MethodInfo, MethodBaseDefinitionMetadata> _methodDefHandles;
        private readonly Dictionary<int, MethodInfo> _unconstructedMethodDefs;
        private readonly Dictionary<MethodInfo, MemberReferenceHandle> _methodRefHandles;
        private readonly Dictionary<MethodInfo, MethodSpecificationHandle> _methodSpecHandles;
        private readonly Dictionary<ParameterInfo, ParameterHandle> _parameterHandles;
        private readonly Dictionary<PropertyInfo, PropertyDefinitionMetadata> _propertyHandles;
        private readonly Dictionary<EventInfo, EventDefinitionMetadata> _eventHandles;
        private readonly Dictionary<Type, TypeDefinitionMetadata> _typeDefHandles;
        private readonly Dictionary<Type, TypeReferenceHandle> _typeRefHandles;
        private readonly Dictionary<Type, TypeSpecificationHandle> _typeSpecHandles;

        /// <summary>
        /// Mapping of assembly fullname to assembly for other dynamic assembly
        /// referenced by the serialized assembly.
        /// </summary>
        /// <remarks>
        /// We are forced to use the <see cref="Assembly.FullName"/> instead of
        /// an <see cref="AssemblyName"/>, because the <see cref="AssemblyName"/> of
        /// the referenced dynamic assembly has much more information than the one
        /// from the serialized one, and thus equality fail.
        /// </remarks>
        private readonly IReadOnlyDictionary<string, Assembly> _referencedDynamics;

        public AssemblyMetadata(Assembly sourceAssembly, IEnumerable<Assembly> referencedDynamicAssemblies)
        {
            SourceAssembly = sourceAssembly;
            Builder = new();
            ILBuilder = new();
            MappedFieldDataBuilder = new();

            _assemblyRefHandles = new();
            _ctorDefHandles = new();
            _ctorRefHandles = new();
            _fieldDefHandles = new();
            _unconstructedFieldDefs = new();
            _fieldRefHandles = new();
            _methodDefHandles = new();
            _unconstructedMethodDefs = new();
            _methodRefHandles = new();
            _methodSpecHandles = new();
            _parameterHandles = new();
            _propertyHandles = new();
            _eventHandles = new();
            _typeDefHandles = new();
            _typeRefHandles = new();
            _typeSpecHandles = new();
            _referencedDynamics = referencedDynamicAssemblies.ToDictionary(a => a.FullName, a => a);

            var assemblies = new HashSet<AssemblyName>(sourceAssembly.GetReferencedAssemblies(), new AssemblyNameComparer());

            var netstandardAssemblyName = Assembly.Load("System.Runtime").GetName();
            
            if (!assemblies.Contains(netstandardAssemblyName)) {
                // HACK: [vermorel] 2019-07-25. 'GetReferencedAssemblies()' does not capture all assemblies,
                // only those that are explicitly referenced. Thus, we end-up  manually adding the assembly.
                assemblies.Add(Assembly.GetAssembly(typeof(object)).GetName());
            }

            if (assemblies.Remove(Assembly.Load("System.Private.CoreLib").GetName())) {
                // Replace any reference to the private core library implementation with netstandard
                assemblies.Add(netstandardAssemblyName);
            }

            CreateReferencedAssemblies(assemblies);
        }

        public Assembly SourceAssembly { get; }
        public MetadataBuilder Builder { get; }
        public BlobBuilder ILBuilder { get; }
        public BlobBuilder MappedFieldDataBuilder { get; }

        public UserStringHandle GetOrAddUserString(string value) => value != null ? Builder.GetOrAddUserString(value) : default;
        public BlobHandle GetOrAddBlob(byte[] value) => value != null ? Builder.GetOrAddBlob(value) : default;
        public BlobHandle GetOrAddBlob(BlobBuilder value) => value != null ? Builder.GetOrAddBlob(value) : default;
        public GuidHandle GetOrAddGuid(Guid value) => Builder.GetOrAddGuid(value);
        public StringHandle GetOrAddString(string value) => value != null ? Builder.GetOrAddString(value) : default;

        private SignatureTypeEncoder AddCustomModifiers(SignatureTypeEncoder encoder, FieldInfo info)
        {
            this.AddCustomModifiers(encoder.CustomModifiers(),
                info.GetRequiredCustomModifiers(), info.GetOptionalCustomModifiers());
            return encoder;
        }
        
        private void AddCustomModifiers(ParameterTypeEncoder encoder, ParameterInfo info) =>
            this.AddCustomModifiers(encoder.CustomModifiers(),
                info.GetRequiredCustomModifiers(), info.GetOptionalCustomModifiers());
    }
}
