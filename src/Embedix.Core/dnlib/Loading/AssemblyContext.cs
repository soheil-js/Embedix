using dnlib.DotNet;
using Embedix.Core.dnlib.Injection;
using Embedix.Core.dnlib.Resources;
using System;
using System.Collections.Generic;

namespace Embedix.Core.dnlib.Loading
{
    /// <summary>
	///     Bundles a loaded assembly with the dnlib helpers needed to modify it: importer,
	///     injector, entry point information, module static constructor and resources.
	/// </summary>
	public class AssemblyContext
    {
        private ResourceManagement _resources = null;

        /// <summary>
        ///     Gets the assembly that owns the module.
        /// </summary>
        public AssemblyDef Assembly { get; private set; }
        /// <summary>
        ///     Gets the manifest module of <see cref="Assembly"/>.
        /// </summary>
        public ModuleDef Module { get; private set; }
        /// <summary>
        ///     Gets the module this context was created from.
        /// </summary>
        public ModuleDefMD ModuleMD { get; private set; }
        /// <summary>
        ///     Gets the importer that brings foreign references into <see cref="Module"/>.
        /// </summary>
        public Importer Importer { get; private set; }
        /// <summary>
        ///     Gets the type declaring the entry point, or <see langword="null"/> if the module has none.
        /// </summary>
        public TypeDef EntryPointType { get; private set; }
        /// <summary>
        ///     Gets the entry point method, or <see langword="null"/> if the module has none.
        /// </summary>
        public MethodDef EntryPointMethod { get; private set; }
        /// <summary>
        ///     Gets the global <c>&lt;Module&gt;</c> type of the module.
        /// </summary>
        public TypeDef GlobalType { get; private set; }
        /// <summary>
        ///     Gets the static constructor of <see cref="GlobalType"/>, created if it did not exist.
        /// </summary>
        public MethodDef GlobalTypeStaticConstructor { get; private set; }
        /// <summary>
        ///     Gets the injector that copies types into <see cref="Module"/>.
        /// </summary>
        public Injector Injector { get; private set; }
        /// <summary>
        ///     Gets the helper used to add, replace and read embedded resources of the module.
        /// </summary>
        public ResourceManagement Resources { get { return _resources; } }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AssemblyContext"/> class for the given module.
        /// </summary>
        /// <param name="module">The loaded module to build the context for.</param>
        public AssemblyContext(ModuleDefMD module)
		{
            ModuleMD = module;

            Assembly = module.Assembly;

            Module = Assembly.ManifestModule;

            Importer = new Importer(Module);

            EntryPointMethod = Module.EntryPoint;
            if(EntryPointMethod != null)
                EntryPointType = Module.EntryPoint.DeclaringType;

            GlobalType = Module.GlobalType;
            GlobalTypeStaticConstructor = GlobalType.FindOrCreateStaticConstructor();

            Injector = Module.GetInjector();
            _resources = new ResourceManagement(this);
		}

        /// <summary>
        ///     Gets the dnlib definition of a runtime type.
        /// </summary>
        /// <param name="type">The runtime type to look up.</param>
        /// <returns>The <see cref="TypeDef"/> describing <paramref name="type"/>.</returns>
        public TypeDef GetTypeDef(Type type)
        {
            var typeModule = ModuleDefMD.Load(type.Module);
            var typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(type.MetadataToken));
            return typeDef;
        }

        /// <summary>
        ///     Creates an assembly reference for the assembly located at the given path.
        /// </summary>
        /// <param name="path">Path to the assembly file. The assembly is loaded into the current process to read its name.</param>
        /// <returns>A new <see cref="AssemblyRefUser"/> for that assembly.</returns>
        public AssemblyRefUser GetAssemblyRefUser(string path)
        {
            var assembly = System.Reflection.Assembly.LoadFile(path);
            var assemblyName = assembly.GetName();
            var assemblyRefUser = new AssemblyRefUser(assemblyName);
            return assemblyRefUser;
        }

        /// <summary>
        ///     Gets the assemblies referenced by the module.
        /// </summary>
        /// <returns>The module's assembly references.</returns>
        public IEnumerable<AssemblyRef> GetAssemblyRefs()
        {
            return Module.GetAssemblyRefs();
        }

        /// <summary>
        ///     Injects a runtime type into the module.
        /// </summary>
        /// <param name="source">The runtime type to inject.</param>
        /// <returns>The injected type, or <see langword="null"/> if injection failed or the type was already injected.</returns>
        public TypeDef Merge(Type source)
        {
            TypeDef target;
            var sourceTypeDef = GetTypeDef(source);
            Injector.TryInject(sourceTypeDef, out target);
            return target;
        }

        /// <summary>
        ///     Injects a type definition into the module.
        /// </summary>
        /// <param name="source">The type to inject.</param>
        /// <returns>The injected type, or <see langword="null"/> if injection failed or the type was already injected.</returns>
        public TypeDef Merge(TypeDef source)
        {
            TypeDef target;
            Injector.TryInject(source, out target);
            return target;
        }

        /// <summary>
        ///     Injects a type definition into the module and renames the copy.
        /// </summary>
        /// <param name="source">The type to inject.</param>
        /// <param name="name">The name given to the injected type.</param>
        /// <returns>The injected type, or <see langword="null"/> if injection failed or the type was already injected.</returns>
        public TypeDef Merge(TypeDef source, string name)
        {
            TypeDef target;
            if (Injector.TryInject(source, out target))
            {
                target.Name = name;
            }
            return target;
        }

        /// <summary>
        ///     Injects a type definition into the module and renames the copy and its namespace.
        /// </summary>
        /// <param name="source">The type to inject.</param>
        /// <param name="name">The name given to the injected type.</param>
        /// <param name="nameSpace">The namespace given to the injected type.</param>
        /// <returns>The injected type, or <see langword="null"/> if injection failed or the type was already injected.</returns>
        public TypeDef Merge(TypeDef source, string name, string nameSpace)
        {
            TypeDef target;
            if (Injector.TryInject(source, out target))
            {
                target.Name = name;
                target.Namespace = nameSpace;
            }
            return target;
        }
    }
}
