using dnlib.DotNet;
using System.Collections.Generic;

namespace Embedix.Core.dnlib.Injection
{
    /// <summary>
    /// Deep-copies <see cref="CustomAttribute"/>s and <see cref="DeclSecurity"/> declarations,
    /// including every constructor argument and named argument.
    /// </summary>
    /// <remarks>
    /// The original implementation only imported <c>ca.Constructor</c> and dropped every
    /// constructor/named argument (<c>new CustomAttribute(mapper.Import(ca.Constructor))</c>).
    /// That silently produced data-less attributes after injection - e.g.
    /// <c>[Obsolete("message")]</c> would lose <c>"message"</c>, and any attribute that relies
    /// on named properties (<c>[SomeAttribute(Foo = 1)]</c>) would lose them entirely. This
    /// class copies the full argument list, including <c>typeof(X)</c> arguments (stored as a
    /// <see cref="TypeSig"/> value) and array arguments (stored as a list of <see cref="CAArgument"/>).
    /// </remarks>
    internal sealed class AttributeCopier
    {
        /// <summary>
        ///     Mapper used to import types and signatures into the target module.
        /// </summary>
        private readonly InjectMapper _mapper;

        /// <summary>
        ///     Initializes a new instance of the <see cref="AttributeCopier"/> class.
        /// </summary>
        /// <param name="mapper">The mapper that imports references into the target module.</param>
        public AttributeCopier(InjectMapper mapper)
        {
            _mapper = mapper;
        }

        /// <summary>
        ///     Copies a custom attribute, including constructor and named arguments.
        /// </summary>
        /// <param name="source">The attribute to copy.</param>
        /// <returns>A new attribute whose references point into the target module.</returns>
        public CustomAttribute CopyCustomAttribute(CustomAttribute source)
        {
            var target = new CustomAttribute(_mapper.ImportCustomAttributeType(source.Constructor));

            foreach (var arg in source.ConstructorArguments)
                target.ConstructorArguments.Add(CopyArgument(arg));

            foreach (var namedArg in source.NamedArguments)
                target.NamedArguments.Add(CopyNamedArgument(namedArg));

            return target;
        }

        /// <summary>
        ///     Copies a declarative security attribute set.
        /// </summary>
        /// <param name="source">The declaration to copy.</param>
        /// <returns>A new declaration with imported attribute types and named arguments.</returns>
        public DeclSecurity CopyDeclSecurity(DeclSecurity source)
        {
            var target = new DeclSecurityUser { Action = source.Action };

            foreach (var secAttr in source.SecurityAttributes)
            {
                var newSecAttr = new SecurityAttribute(_mapper.ImportType(secAttr.AttributeType));

                foreach (var namedArg in secAttr.NamedArguments)
                    newSecAttr.NamedArguments.Add(CopyNamedArgument(namedArg));

                target.SecurityAttributes.Add(newSecAttr);
            }

            return target;
        }

        private CANamedArgument CopyNamedArgument(CANamedArgument source) =>
            new CANamedArgument(source.IsField, _mapper.ImportTypeSig(source.Type), source.Name, CopyArgument(source.Argument));

        private CAArgument CopyArgument(CAArgument source) =>
            new CAArgument(_mapper.ImportTypeSig(source.Type), CopyArgumentValue(source.Value));

        private object CopyArgumentValue(object value)
        {
            switch (value)
            {
                // `typeof(SomeType)` arguments are stored as a TypeSig value that needs
                // redirecting to the injected copy just like any other type reference.
                case TypeSig typeSig:
                    return _mapper.ImportTypeSig(typeSig);

                // Array arguments store their elements as a list of CAArgument.
                case IList<CAArgument> array:
                    var copiedArray = new List<CAArgument>(array.Count);
                    foreach (var element in array)
                        copiedArray.Add(CopyArgument(element));
                    return copiedArray;

                // Primitives (string, numeric types, enum underlying values, null) carry no
                // module-bound token and can be reused as-is.
                default:
                    return value;
            }
        }
    }
}
