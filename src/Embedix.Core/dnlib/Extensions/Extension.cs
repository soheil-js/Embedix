using dnlib.DotNet;
using dnlib.Injection;
using dnlib.Loading;

namespace dnlib.Extensions
{
    public static class Extension
    {
        public static AssemblyContext GetAssemblyContext(this ModuleDefMD module)
        {
            return new AssemblyContext(module);
        }

        public static Injector GetInjector(this ModuleDefMD module)
        {
            return new Injector(module.Assembly.ManifestModule);
        }

        public static Injector GetInjector(this ModuleDef module)
        {
            return new Injector(module);
        }
    }
}
