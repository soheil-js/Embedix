using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Embedix.Core.dnlib.Loading;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Embedix.Core
{
    /// <summary>
    ///     Embeds library files into a target assembly as compressed resources and merges the
    ///     runtime payload (an assembly resolver) that loads them on demand.
    /// </summary>
    public class Embedder
    {
        /// <summary>
        ///     Embeds the given libraries into the target assembly and wires up the runtime resolver.
        /// </summary>
        /// <param name="context">The context of the assembly that receives the embedded libraries.</param>
        /// <param name="librarys">Paths of the library files to embed. Each is deflate-compressed and stored as a resource named <c>Embedded.&lt;file name&gt;</c>.</param>
        /// <remarks>
        ///     After the resources are added, the payload module matching the target's core library is
        ///     loaded, its <c>AssemblyResolver</c> type is merged into the target, and a call to its
        ///     <c>Resolve</c> method is inserted at the start of the module static constructor. If the merge does not produce a copy (for example because the type was already injected), no call is inserted.
        /// </remarks>
        public static void Execute(AssemblyContext context, string[] librarys)
        {
            //Embedding libraries into resources for dynamic usage
            foreach (var path in librarys)
            {
                string name = Path.GetFileName(path);
                byte[] file = File.ReadAllBytes(path);
                byte[] zip = Compress(file);
                context.Resources.Add($"Embedded.{name}", zip);
            }

            ModuleDefMD payloadModule = null;
            try
            {
                string resourceName = SelectPayloadResource(context.Module);
                payloadModule = LoadPayloadModule(resourceName);

                var typeDef = payloadModule.Find("Embedix.Core.Payload.AssemblyResolver", false);

                var copyTypeDef = context.Merge(typeDef, "AssemblyResolver", "Embedix");
                if (copyTypeDef != null)
                {
                    var method = copyTypeDef.Methods.Single(m => m.Name == "Resolve");
                    context.GlobalTypeStaticConstructor.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, method));
                }
            }
            finally
            {
                payloadModule?.Dispose();
            }
        }

        /// <summary>
        ///     Chooses the embedded payload assembly that matches the target module's core library.
        /// </summary>
        /// <param name="targetModule">The module being processed.</param>
        /// <returns>The payload resource name: <c>net45</c> when the core library is <c>mscorlib</c> (or a name starting with it), <c>netstandard2.0</c> for <c>netstandard</c>, and <c>net6.0</c> for <c>System.Private.CoreLib</c>, <c>System.Runtime</c>, a missing name or any unrecognized core library.</returns>
        private static string SelectPayloadResource(ModuleDef targetModule)
        {
            var corLibAssemblyRef = targetModule.CorLibTypes.AssemblyRef;
            string corLibName = corLibAssemblyRef?.Name.String;

            if (string.IsNullOrEmpty(corLibName))
                return "Payload.net6.0.dll";

            if (string.Equals(corLibName, "mscorlib", StringComparison.OrdinalIgnoreCase))
                return "Payload.net45.dll";

            if (string.Equals(corLibName, "System.Private.CoreLib", StringComparison.OrdinalIgnoreCase) || string.Equals(corLibName, "System.Runtime", StringComparison.OrdinalIgnoreCase))
                return "Payload.net6.0.dll";

            if (string.Equals(corLibName, "netstandard", StringComparison.OrdinalIgnoreCase))
                return "Payload.netstandard2.0.dll";

            if (corLibName.StartsWith("mscorlib", StringComparison.OrdinalIgnoreCase))
                return "Payload.net45.dll";

            return "Payload.net6.0.dll";
        }

        /// <summary>
        ///     Loads a payload assembly from this assembly's manifest resources.
        /// </summary>
        /// <param name="resourceName">The name of the embedded payload resource.</param>
        /// <returns>The loaded payload module.</returns>
        /// <exception cref="InvalidOperationException">The resource does not exist.</exception>
        private static ModuleDefMD LoadPayloadModule(string resourceName)
        {
            using (var stream = typeof(Embedder).Assembly.GetManifestResourceStream(resourceName))
            {
                if (stream == null)
                    throw new InvalidOperationException($"Resource '{resourceName}' not found.");

                var buffer = new byte[stream.Length];
                stream.Read(buffer, 0, buffer.Length);
                return ModuleDefMD.Load(buffer, ModuleDef.CreateModuleContext());
            }
        }

        //https://stackoverflow.com/questions/10599596/compress-and-decompress-a-stream-with-compression-deflatestream
        /// <summary>
        ///     Compresses data using the Deflate algorithm.
        /// </summary>
        /// <param name="data">The bytes to compress.</param>
        /// <returns>The compressed bytes, or <see langword="null"/> if compression fails.</returns>
        private static byte[] Compress(byte[] data)
        {
            try
            {
                using (MemoryStream memoryStream = new MemoryStream())
                {
                    using (DeflateStream deflateStream = new DeflateStream(memoryStream, CompressionMode.Compress))
                    {
                        deflateStream.Write(data, 0, data.Length);
                    }
                    return memoryStream.ToArray();
                }
            }
            catch
            {
                return null;
            }
        }
    }
}
