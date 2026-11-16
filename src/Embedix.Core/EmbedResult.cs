
using System.Collections.Generic;
using System.Linq;

namespace Embedix.Core
{
    /// <summary>
    ///     Represents the result of an assembly embedding operation.
    /// </summary>
    public sealed class EmbedResult
    {
        /// <summary>
        ///     Gets the names of the libraries successfully embedded.
        /// </summary>
        public IEnumerable<string> EmbeddedLibraries { get; private set; }

        /// <summary>
        ///     Gets the total size of the original library files in bytes.
        /// </summary>
        public long OriginalSize { get; private set; }

        /// <summary>
        ///     Gets the total size of the compressed library files in bytes.
        /// </summary>
        public long CompressedSize { get; private set; }

        /// <summary>
        ///     Gets the total size reduction achieved by compression in bytes.
        /// </summary>
        public long SavedSize
        {
            get { return OriginalSize - CompressedSize; }
        }

        /// <summary>
        ///     Gets the compression ratio as a percentage.
        /// </summary>
        public double CompressionRatio
        {
            get
            {
                return OriginalSize == 0
                    ? 0
                    : (double)CompressedSize / OriginalSize * 100;
            }
        }

        /// <summary>
        ///     Initializes a new instance of the <see cref="EmbedResult"/> class.
        /// </summary>
        /// <param name="embeddedLibraries">The names of the successfully embedded libraries.</param>
        /// <param name="originalSize">The total size of the original library files in bytes.</param>
        /// <param name="compressedSize">The total size of the compressed library files in bytes.</param>
        public EmbedResult(
            IEnumerable<string> embeddedLibraries,
            long originalSize,
            long compressedSize,
            IEnumerable<string> warnings)
        {
            EmbeddedLibraries = (embeddedLibraries ?? Enumerable.Empty<string>()).ToArray();
            OriginalSize = originalSize;
            CompressedSize = compressedSize;
        }
    }
}
