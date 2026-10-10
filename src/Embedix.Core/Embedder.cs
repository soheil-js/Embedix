using dnlib.DotNet.Emit;
using dnlib.Loading;
using Embedix.Core.Resolver;
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

            //Class injection
            var typeDef = context.GetTypeDef(typeof(AssemblyResolver));
            var copyTypeDef = context.Merge(typeDef, "AssemblyResolver", "Embedix");
            if (copyTypeDef != null)
            {
                //Call the load method in the <Module> class
                var method = copyTypeDef.Methods.Single(m => m.Name == "Resolve");
                context.GlobalTypeStaticConstructor.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, method));
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
