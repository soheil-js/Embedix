using System.Reflection;

namespace Embedix.GUI.Utility
{
    public static class AppVersion
    {
        private static Lazy<string> _version = new Lazy<string>(new Func<string>(CreateVersion));

        private static string CreateVersion()
        {
            return (Assembly.GetExecutingAssembly().GetName().Version ?? new Version()).ToString(3);
        }

        public static string Get()
        {
            return _version.Value;
        }
    }
}
