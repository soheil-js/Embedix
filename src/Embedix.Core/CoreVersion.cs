using System;
using System.Reflection;

namespace Embedix.Core
{
    /// <summary>
    ///     Provides the version of the Embedix core assembly.
    /// </summary>
    public static class CoreVersion
    {
        /// <summary>
        ///     Cached version string, initialized once by the static field initializer.
        /// </summary>
        private static readonly string _version = CreateVersion();

        /// <summary>
        ///     Builds a <c>Major.Minor.Build</c> version string from the executing assembly,
        ///     falling back to <c>0.0.0</c> when no version is available.
        /// </summary>
        /// <returns>The formatted version string.</returns>
        private static string CreateVersion()
        {
            Version version = Assembly.GetExecutingAssembly().GetName().Version;

            if (version == null)
                version = new Version(0, 0, 0);

            return string.Format(
                "{0}.{1}.{2}",
                version.Major,
                version.Minor,
                version.Build < 0 ? 0 : version.Build
            );
        }

        /// <summary>
        ///     Gets the version of the Embedix core assembly.
        /// </summary>
        /// <returns>The version formatted as <c>Major.Minor.Build</c>; a negative build number is reported as <c>0</c>.</returns>
        public static string Get()
        {
            return _version;
        }
    }
}
