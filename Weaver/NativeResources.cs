using PostSharp.Sdk.CodeModel;
using System;
using System.IO.Compression;
using System.Linq;
using System.Text.RegularExpressions;

namespace PostSharp.Community.Packer.Weaver
{
    public static class NativeResourcesProcessor
    {
        /// <summary>
        /// Calculates checksums for Costura-processed unmanaged resources of the assembly,
        /// and returns true if any unprocessed resources exist.
        /// </summary>
        public static bool ProcessNativeResources(
            AssemblyManifestDeclaration manifest, bool compress,
            Checksums checksums)
        {
            var hasUnmanaged = false;

            if (manifest == null) return hasUnmanaged;
            if (checksums == null) return hasUnmanaged;
            if (manifest.Resources == null || !manifest.Resources.Any())
                return hasUnmanaged;

            try
            {
                var unprocessedNameMatch = new Regex(
                    @"^(.*\.)?costura(32|64)\.",
                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase
                );
                var processedNameMatch = new Regex(
                    @"^costura(32|64)\.",
                    RegexOptions.CultureInvariant | RegexOptions.IgnoreCase
                );

                foreach (var resource in manifest.Resources)
                {
                    var match = unprocessedNameMatch.Match(resource.Name);
                    if (match.Success)
                    {
                        resource.Name = resource.Name.Substring(
                                                    match.Groups[1]
                                                        .Length
                                                )
                                                .ToLowerInvariant();
                        hasUnmanaged = true;
                    }

                    if (string.IsNullOrWhiteSpace(resource.Name)) continue;
                    if (!processedNameMatch.IsMatch(resource.Name)) 
                        continue;

                    using var stream = resource.ContentStreamProvider();
                    if (compress && resource.Name.EndsWith(
                            ".compressed",
                            StringComparison.OrdinalIgnoreCase
                        ))
                    {
                        using var compressStream = new DeflateStream(
                            stream, CompressionMode.Decompress
                        );
                        checksums.Add(
                            resource.Name,
                            Checksums.CalculateChecksum(
                                compressStream
                            )
                        );
                    }
                    else
                    {
                        checksums.Add(
                            resource.Name,
                            Checksums.CalculateChecksum(stream)
                        );
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                hasUnmanaged = false;
            }

            return hasUnmanaged;
        }
    }
}