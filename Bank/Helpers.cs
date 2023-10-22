using System.IO;
using System.Reflection;

namespace LightPath.Bank
{
    public static class Helpers
    {
        public static string AsString(this byte[] source) => System.Text.Encoding.Default.GetString(source);

        public static byte[] GetEmbeddedBytes(Assembly assembly, string nameSpace, string fileName)
        {
            using var resourceStream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{nameSpace}.{fileName}");
            using var memoryStream = resourceStream == null ? null : new MemoryStream();
            
            resourceStream?.CopyTo(memoryStream);
            
            var output = memoryStream?.ToArray();

            resourceStream?.Dispose();
            memoryStream?.Dispose();

            return output;
        }

        public static string GetEmbeddedString(Assembly assembly, string nameSpace, string fileName)
        {
            using var stream = assembly.GetManifestResourceStream($"{assembly.GetName().Name}.{nameSpace}.{fileName}");
            using var reader = stream == null ? null : new StreamReader(stream);

            var output = reader?.ReadToEnd();

            return output;
        }
    }
}
