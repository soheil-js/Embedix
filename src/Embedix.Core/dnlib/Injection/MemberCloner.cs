using dnlib.DotNet;

namespace Embedix.Core.dnlib.Injection
{
    /// <summary>
    /// Creates empty ("skeleton") copies of dnlib definitions. A clone only carries the
    /// structural information needed to add the definition to the target module; bodies,
    /// custom attributes and cross-references are filled in later by <see cref="MemberCopier"/>.
    /// </summary>
    internal sealed class MemberCloner
    {
        private readonly InjectMapper _mapper;

        public MemberCloner(InjectMapper mapper)
        {
            _mapper = mapper;
        }

        /// <summary>
        ///     Creates a skeleton copy of a type with its attributes, base type, class layout and generic parameters.
        /// </summary>
        /// <param name="source">The type to clone.</param>
        /// <returns>The new type.</returns>
        public TypeDefUser CloneType(TypeDef source)
        {
            var typeDef = new TypeDefUser(source.Namespace, source.Name, _mapper.ImportType(source.BaseType))
            {
                Attributes = source.Attributes,
                ClassLayout = source.ClassLayout != null
                    ? new ClassLayoutUser(source.ClassLayout.PackingSize, source.ClassSize)
                    : null
            };

            foreach (GenericParam genericParam in source.GenericParameters)
                typeDef.GenericParameters.Add(CloneGenericParam(genericParam));

            return typeDef;
        }

        /// <summary>
        ///     Creates a skeleton copy of a method with its signature, attributes and generic parameters.
        /// </summary>
        /// <param name="source">The method to clone.</param>
        /// <returns>The new method.</returns>
        public MethodDefUser CloneMethod(MethodDef source)
        {
            var methodDef = new MethodDefUser(
                source.Name,
                _mapper.ImportMethodSig(source.MethodSig),
                source.ImplAttributes,
                source.Attributes);

            foreach (GenericParam genericParam in source.GenericParameters)
                methodDef.GenericParameters.Add(CloneGenericParam(genericParam));

            return methodDef;
        }

        /// <summary>
        ///     Creates a skeleton copy of a property.
        /// </summary>
        /// <param name="source">The property to clone.</param>
        /// <returns>The new property, without accessors.</returns>
        public PropertyDefUser CloneProperty(PropertyDef source) =>
            new PropertyDefUser(source.Name, _mapper.ImportPropertySig(source.PropertySig), source.Attributes);

        /// <summary>
        ///     Creates a skeleton copy of a field.
        /// </summary>
        /// <param name="source">The field to clone.</param>
        /// <returns>The new field.</returns>
        public FieldDefUser CloneField(FieldDef source) =>
            new FieldDefUser(source.Name, _mapper.ImportFieldSig(source.FieldSig), source.Attributes);

        /// <summary>
        ///     Creates a skeleton copy of an event.
        /// </summary>
        /// <param name="source">The event to clone.</param>
        /// <returns>The new event, without accessors.</returns>
        public EventDefUser CloneEvent(EventDef source) =>
            new EventDefUser(source.Name, _mapper.ImportType(source.EventType), source.Attributes);

        /// <summary>
        ///     Creates a copy of a generic parameter without its constraints.
        /// </summary>
        /// <param name="source">The generic parameter to clone.</param>
        /// <returns>The new generic parameter.</returns>
        public GenericParamUser CloneGenericParam(GenericParam source) =>
            new GenericParamUser(source.Number, source.Flags, source.Name);
    }
}
