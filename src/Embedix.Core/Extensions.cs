using dnlib.DotNet;
using Embedix.Core.dnlib.Injection;
using Embedix.Core.dnlib.Loading;

namespace Embedix.Core
{
    public static class Extensions
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
