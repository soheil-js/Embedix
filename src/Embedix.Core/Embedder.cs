using dnlib.DotNet;
using dnlib.DotNet.Emit;
using Embedix.Core.dnlib.Loading;
using System;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace Embedix.Core
{
    public class Embedder
    {
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
