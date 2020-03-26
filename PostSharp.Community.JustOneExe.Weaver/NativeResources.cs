using System;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;
using PostSharp.Sdk.CodeModel;

namespace PostSharp.Community.JustOneExe.Weaver
{
    partial class JustOneExeTask
    {
        void ProcessNativeResources(AssemblyManifestDeclaration manifest, bool compress)
        {
            var unprocessedNameMatch = new Regex(@"^(.*\.)?costura(32|64)\.", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);
            var processedNameMatch = new Regex(@"^costura(32|64)\.", RegexOptions.CultureInvariant | RegexOptions.IgnoreCase);

            foreach (var resource in manifest.Resources)
            {
                var match = unprocessedNameMatch.Match(resource.Name);
                if (match.Success)
                {
                    resource.Name = resource.Name.Substring(match.Groups[1].Length).ToLowerInvariant();
                    hasUnmanaged = true;
                }

                if (processedNameMatch.IsMatch(resource.Name))
                {
                    using (var stream = resource.ContentStreamProvider())
                    {
                        if (compress && resource.Name.EndsWith(".compressed",StringComparison.OrdinalIgnoreCase))
                        {
                            using (var compressStream = new DeflateStream(stream, CompressionMode.Decompress))
                            {
                                checksums.Add(resource.Name, CalculateChecksum(compressStream));
                            }
                        }
                        else
                        {
                            checksums.Add(resource.Name, CalculateChecksum(stream));
                        }
                    }
                }
            }
        }
    }
}