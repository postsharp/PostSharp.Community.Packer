using PostSharp.Sdk.CodeModel;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace PostSharp.Community.Packer.Weaver
{
    public static class ResourceHash
    {
        public static string CalculateHash(AssemblyManifestDeclaration manifest)
        {
            var result = string.Empty;

            try
            {
                if (manifest == null || !manifest.Resources.Any())
                    return result;

                var data = manifest.Resources.OrderBy(r => r.Name)
                                   .Where(r => r.Name.StartsWith("costura"))
                                   .Select(r => r.ContentStreamProvider())
                                   .ToArray();
                if (data == null || data.Length == 0) return result;

                var allStreams = new ConcatenatedStream(data);
                if (allStreams == null) return result;

                using var md5 = MD5.Create();
                var hashBytes = md5.ComputeHash(allStreams);

                var sb = new StringBuilder();
                foreach (var @byte in hashBytes)
                    sb.Append(
                        @byte
                            .ToString("X2")
                    );
                
                allStreams.ResetAllToZero();
                result = sb.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                result = string.Empty;
            }

            return result;
        }
    }
}