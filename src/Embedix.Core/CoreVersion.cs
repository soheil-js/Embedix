using System;
using System.Reflection;

namespace Embedix.Core
{
    public static class CoreVersion
    {
        private static readonly string _version = CreateVersion();

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

        public static string Get()
        {
            return _version;
        }
    }
}
