using dnlib.DotNet;
using Embedix.Core.dnlib.Injection;
using Embedix.Core.dnlib.Loading;

namespace Embedix.Core
{
    /// <summary>
    ///     Extension methods that simplify creating Embedix helpers from dnlib modules.
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        ///     Creates an <see cref="AssemblyContext"/> for the specified module.
        /// </summary>
        /// <param name="module">The loaded module to wrap.</param>
        /// <returns>A new <see cref="AssemblyContext"/> bound to <paramref name="module"/>.</returns>
        public static AssemblyContext GetAssemblyContext(this ModuleDefMD module)
        {
            return new AssemblyContext(module);
        }

        /// <summary>
        ///     Creates an <see cref="Injector"/> that injects types into the manifest module of the
        ///     specified module's assembly.
        /// </summary>
        /// <param name="module">The loaded module whose assembly manifest module is the injection target.</param>
        /// <returns>A new <see cref="Injector"/> targeting the assembly's manifest module.</returns>
        public static Injector GetInjector(this ModuleDefMD module)
        {
            return new Injector(module.Assembly.ManifestModule);
        }

        /// <summary>
        ///     Creates an <see cref="Injector"/> that injects types into the specified module.
        /// </summary>
        /// <param name="module">The module that receives the injected types.</param>
        /// <returns>A new <see cref="Injector"/> targeting <paramref name="module"/>.</returns>
        public static Injector GetInjector(this ModuleDef module)
        {
            return new Injector(module);
        }
    }
}
