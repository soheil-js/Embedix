using System;
using System.IO;
using dnlib.DotNet;
using Embedix.Core.dnlib.Loading;

namespace Embedix.Core.dnlib.Resources
{
    /// <summary>
    ///     Adds, replaces and reads embedded resources of an assembly's module.
    /// </summary>
    public class ResourceManagement
    {
        /// <summary>
        ///     The context whose module holds the resources.
        /// </summary>
        private AssemblyContext _context = null;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ResourceManagement"/> class.
        /// </summary>
        /// <param name="context">The context whose module resources are managed.</param>
        public ResourceManagement(AssemblyContext context)
        {
            _context = context;
        }

        /// <summary>
        ///     Adds a public embedded resource from a byte array.
        /// </summary>
        /// <param name="name">The resource name.</param>
        /// <param name="bytes">The resource data.</param>
        public void Add(string name, byte[] bytes)
        {
            EmbeddedResource resource = new EmbeddedResource(name, bytes, ManifestResourceAttributes.Public);
            _context.Module.Resources.Add(resource);
        }

        /// <summary>
        ///     Adds a public embedded resource from a file, named after the file name.
        /// </summary>
        /// <param name="path">Path of the file to embed.</param>
        public void Add(string path)
        {
            string name = Path.GetFileName(path);
            byte[] bytes = File.ReadAllBytes(path);
            EmbeddedResource resource = new EmbeddedResource(name, bytes, ManifestResourceAttributes.Public);
            _context.Module.Resources.Add(resource);
        }

        /// <summary>
        ///     Replaces the first resource with the given name by removing it and appending a new one at the end of the resource list; does nothing if none exists.
        /// </summary>
        /// <param name="name">The name of the resource to replace.</param>
        /// <param name="bytes">The new resource data.</param>
        public void Replace(string name, byte[] bytes)
        {
            for (var i = 0; i < _context.Module.Resources.Count; i++)
            {
                if (_context.Module.Resources[i].Name == name)
                {
                    _context.Module.Resources.RemoveAt(i);
                    EmbeddedResource resource = new EmbeddedResource(name, bytes, ManifestResourceAttributes.Public);
                    _context.Module.Resources.Add(resource);
                    break;
                }
            }
        }

        /// <summary>
        ///     Replaces the resource named after the file name with the file contents (the new resource is appended at the end of the list);
        ///     does nothing if no such resource exists.
        /// </summary>
        /// <param name="path">Path of the file providing the new data.</param>
        public void Replace(string path)
        {
            string name = Path.GetFileName(path);
            byte[] bytes = File.ReadAllBytes(path);
            for (var i = 0; i < _context.Module.Resources.Count; i++)
            {
                if (_context.Module.Resources[i].Name == name)
                {
                    _context.Module.Resources.RemoveAt(i);
                    EmbeddedResource resource = new EmbeddedResource(name, bytes, ManifestResourceAttributes.Public);
                    _context.Module.Resources.Add(resource);
                    break;
                }
            }
        }

        /// <summary>
        ///     Gets the data of an embedded resource as a stream.
        /// </summary>
        /// <param name="name">The resource name.</param>
        /// <returns>A new <see cref="MemoryStream"/> positioned at the start, or <see langword="null"/> if no resource matches.</returns>
        /// <exception cref="InvalidCastException">A resource with that name exists but is not an embedded resource.</exception>
        public MemoryStream GetStreamResource(string name)
        {
            foreach (var resource in _context.Module.Resources)
            {
                if (resource.Name == name)
                {
                    var embeddedResource = (EmbeddedResource)resource;
                    var stream = embeddedResource.CreateReader().AsStream();

                    var bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);

                    var memStream = new MemoryStream();
                    memStream.Write(bytes, 0, bytes.Length);
                    memStream.Position = 0;
                    return memStream;
                }
            }
            return null;
        }

        /// <summary>
        ///     Gets the data of an embedded resource as a byte array.
        /// </summary>
        /// <param name="name">The resource name.</param>
        /// <returns>The resource bytes, or <see langword="null"/> if no resource matches.</returns>
        /// <exception cref="InvalidCastException">A resource with that name exists but is not an embedded resource.</exception>
        public byte[] GetByteResource(string name)
        {
            foreach (var resource in _context.Module.Resources)
            {
                if (resource.Name == name)
                {
                    var embeddedResource = (EmbeddedResource)resource;
                    var stream = embeddedResource.CreateReader().AsStream();
                    var bytes = new byte[stream.Length];
                    stream.Read(bytes, 0, bytes.Length);
                    return bytes;
                }
            }
            return null;
        }
    }
}
