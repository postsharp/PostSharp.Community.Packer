using PostSharp.Sdk.CodeModel;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PostSharp.Community.Packer.Weaver
{
    public static class ResourceHash
    {
        public static string CalculateHash(AssemblyManifestDeclaration manifest)
        {
            var data = manifest.Resources
                .OrderBy(r => r.Name)
                .Where(r => r.Name.StartsWith("costura"))
                .Select(r => r.ContentStreamProvider())
                .ToArray();
            var allStream = new ConcatenatedStream(data);

            using (var md5 = MD5.Create())
            {
                var hashBytes = md5.ComputeHash(allStream);

                var sb = new StringBuilder();
                for (var i = 0; i < hashBytes.Length; i++)
                {
                    sb.Append(hashBytes[i].ToString("X2"));
                }

                allStream.ResetAllToZero();
                return sb.ToString();
            }
        }
    }
}