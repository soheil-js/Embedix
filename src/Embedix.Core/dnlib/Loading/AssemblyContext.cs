using dnlib.DotNet;
using Embedix.Core.dnlib.Injection;
using Embedix.Core.dnlib.Resources;
using System;
using System.Collections.Generic;

namespace Embedix.Core.dnlib.Loading
{
	public class AssemblyContext
    {
        private ResourceManagement _resources = null;

        public AssemblyDef Assembly { get; private set; }
        public ModuleDef Module { get; private set; }
        public ModuleDefMD ModuleMD { get; private set; }
        public Importer Importer { get; private set; }
        public TypeDef EntryPointType { get; private set; }
        public MethodDef EntryPointMethod { get; private set; }
        public TypeDef GlobalType { get; private set; }
        public MethodDef GlobalTypeStaticConstructor { get; private set; }
        public Injector Injector { get; private set; }
        public ResourceManagement Resources { get { return _resources; } }

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

        public TypeDef GetTypeDef(Type type)
        {
            var typeModule = ModuleDefMD.Load(type.Module);
            var typeDef = typeModule.ResolveTypeDef(MDToken.ToRID(type.MetadataToken));
            return typeDef;
        }

        public AssemblyRefUser GetAssemblyRefUser(string path)
        {
            var assembly = System.Reflection.Assembly.LoadFile(path);
            var assemblyName = assembly.GetName();
            var assemblyRefUser = new AssemblyRefUser(assemblyName);
            return assemblyRefUser;
        }

        public IEnumerable<AssemblyRef> GetAssemblyRefs()
        {
            return Module.GetAssemblyRefs();
        }

        public TypeDef Merge(Type source)
        {
            TypeDef target;
            var sourceTypeDef = GetTypeDef(source);
            Injector.TryInject(sourceTypeDef, out target);
            return target;
        }

        public TypeDef Merge(TypeDef source)
        {
            TypeDef target;
            Injector.TryInject(source, out target);
            return target;
        }

        public TypeDef Merge(TypeDef source, string name)
        {
            TypeDef target;
            if (Injector.TryInject(source, out target))
            {
                target.Name = name;
            }
            return target;
        }

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
