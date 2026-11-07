using dnlib.DotNet;
using dnlib.DotNet.Writer;
using dnlib.PE;
using System;
using System.IO;

namespace Embedix.Core.dnlib.Loading
{
    /// <summary>
    ///     Loads a .NET module from disk and writes it back, preserving metadata.
    /// </summary>
    public class AssemblyLoader : IDisposable
    {
        /// <summary>
        ///     Gets the loaded module.
        /// </summary>
        public ModuleDefMD ModuleDefMD { get; private set; }

        /// <summary>
        ///     Initializes a new instance of the <see cref="AssemblyLoader"/> class and loads the module.
        /// </summary>
        /// <param name="path">Path to the module file.</param>
        /// <exception cref="FileNotFoundException">The file does not exist or could not be loaded as a module.</exception>
        public AssemblyLoader(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException($"Missing file: {path}");
            }
            try
            {
                ModuleContext modCtx = ModuleDef.CreateModuleContext();
                ModuleDefMD = ModuleDefMD.Load(path, modCtx);
            }
            catch 
            {
                throw new FileNotFoundException($"Error for load module");
            }
        }

        /// <summary>
        ///     Writes the module to disk, using the native writer for mixed-mode modules.
        /// </summary>
        /// <param name="path">Destination file path.</param>
        /// <param name="writePdb"><see langword="true"/> to also write a PDB file; otherwise <see langword="false"/>.</param>
        /// <exception cref="FileNotFoundException">The module could not be saved.</exception>
        public void Write(string path, bool writePdb = true)
        {
            try
            {
                if (ModuleDefMD.IsILOnly)
                {
                    IImageOptionalHeader optionalHeader = ModuleDefMD.Metadata.PEImage.ImageNTHeaders.OptionalHeader;
                    uint numberOfRvaAndSizes = optionalHeader.NumberOfRvaAndSizes;
                    var options = new ModuleWriterOptions(ModuleDefMD)
                    {
                        MetadataOptions = { Flags = MetadataFlags.PreserveAll },
                        PEHeadersOptions = { NumberOfRvaAndSizes = numberOfRvaAndSizes },
                        Logger = DummyLogger.NoThrowInstance,
                        WritePdb = writePdb
                    };

                    ModuleDefMD.Write(path, options);
                }
                else
                {
                    var options = new NativeModuleWriterOptions(ModuleDefMD, true)
                    {
                        MetadataOptions = { Flags = MetadataFlags.PreserveAll },
                        Logger = DummyLogger.NoThrowInstance,
                        WritePdb = writePdb
                    };

                    ModuleDefMD.NativeWrite(path, options);
                }
            }
            catch
            {
                throw new FileNotFoundException($"Fail to save current module");
            }
        }

        /// <summary>
        ///     Creates an <see cref="AssemblyContext"/> for the loaded module.
        /// </summary>
        /// <returns>A new <see cref="AssemblyContext"/>.</returns>
        public AssemblyContext GetAssemblyContext()
        {
            return ModuleDefMD.GetAssemblyContext();
        }

        /// <summary>
        ///     Releases the loaded module.
        /// </summary>
        public void Dispose()
        {
            ModuleDefMD.Dispose();
        }
    }
}
