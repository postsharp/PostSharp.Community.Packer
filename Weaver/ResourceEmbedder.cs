using PostSharp.Sdk.CodeModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;

namespace PostSharp.Community.Packer.Weaver
{
    public class ResourceEmbedder : IDisposable
    {
        private readonly AssemblyManifestDeclaration _manifest;
        private readonly List<Stream> _streams = new List<Stream>();
        private string _cachePath;

        /// <summary>
        /// Constructs a new instance of
        /// <see cref="T:PostSharp.Community.Packer.Weaver.ResourceEmbedder" /> and returns
        /// a reference to it.
        /// </summary>
        /// <param name="manifest">
        /// (Required.) Reference to an instance of
        /// <see cref="T:PostSharp.Sdk.CodeModel.AssemblyManifestDeclaration" /> that
        /// represents the manifest.
        /// </param>
        public ResourceEmbedder(AssemblyManifestDeclaration manifest)
            => this._manifest = manifest;

        public bool HasUnmanaged { get; private set; }

        public void Dispose()
        {
            if (_streams == null) return;
            foreach (var stream in _streams) stream.Dispose();
        }

        public void EmbedResources(PackerAttribute config,
            IEnumerable<string> referenceCopyLocalPaths, Checksums checksums)
        {
            try
            {
                if (config == null) return;
                if (referenceCopyLocalPaths == null ||
                    !referenceCopyLocalPaths.Any()) return;
                if (checksums?.AllChecksums == null ||
                    !checksums.AllChecksums.Any()) return;

                _cachePath = Path.Combine(
                    Path.GetTempPath(), Path.GetRandomFileName()
                );
                if (string.IsNullOrWhiteSpace(_cachePath))
                    return;

                Directory.CreateDirectory(_cachePath);
                if (!Directory.Exists(_cachePath)) return;

                if (!referenceCopyLocalPaths.Any(IsBinaryPath)) return;

                var onlyBinaries = referenceCopyLocalPaths.Where(IsBinaryPath)
                    .ToArray();

                var disableCompression = config.DisableCompression;
                var createTemporaryAssemblies =
                    config.CreateTemporaryAssemblies;

                foreach (var dependency in GetFilteredReferences(
                             onlyBinaries, config
                         ))
                {
                    var fullPath = Path.GetFullPath(dependency);

                    if (!config.IgnoreSatelliteAssemblies)
                        if (dependency.EndsWith(
                                ".resources.dll",
                                StringComparison.OrdinalIgnoreCase
                            ))
                        {
                            Embed(
                                $"costura.{Path.GetFileName(Path.GetDirectoryName(fullPath))}.",
                                fullPath, !disableCompression,
                                createTemporaryAssemblies,
                                config.DisableCleanup, checksums
                            );
                            continue;
                        }

                    Embed(
                        "costura.", fullPath, !disableCompression,
                        createTemporaryAssemblies, config.DisableCleanup,
                        checksums
                    );

                    if (!config.IncludeDebugSymbols) continue;
                    var pdbFullPath = Path.ChangeExtension(fullPath, "pdb");
                    if (File.Exists(pdbFullPath))
                        Embed(
                            "costura.", pdbFullPath, !disableCompression,
                            createTemporaryAssemblies, config.DisableCleanup,
                            checksums
                        );
                }

                foreach (var dependency in onlyBinaries)
                {
                    var prefix = "";

                    if (config.Unmanaged32Assemblies.Any(
                            x => string.Equals(
                                x, Path.GetFileNameWithoutExtension(dependency),
                                StringComparison.OrdinalIgnoreCase
                            )
                        ))
                    {
                        prefix = "costura32.";
                        HasUnmanaged = true;
                    }

                    if (config.Unmanaged64Assemblies.Any(
                            x => string.Equals(
                                x, Path.GetFileNameWithoutExtension(dependency),
                                StringComparison.OrdinalIgnoreCase
                            )
                        ))
                    {
                        prefix = "costura64.";
                        HasUnmanaged = true;
                    }

                    if (string.IsNullOrEmpty(prefix)) continue;

                    var fullPath = Path.GetFullPath(dependency);
                    Embed(
                        prefix, fullPath, !disableCompression, true,
                        config.DisableCleanup, checksums
                    );

                    if (!config.IncludeDebugSymbols) continue;
                    var pdbFullPath = Path.ChangeExtension(fullPath, "pdb");
                    if (File.Exists(pdbFullPath))
                        Embed(
                            prefix, pdbFullPath, !disableCompression, true,
                            config.DisableCleanup, checksums
                        );
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static MemoryStream BuildMemoryStream(string fullPath,
            bool compress, string cacheFile)
        {
            var newMemoryStream = new MemoryStream();

            try
            {
                if (string.IsNullOrWhiteSpace(fullPath)) return newMemoryStream;
                if (string.IsNullOrWhiteSpace(cacheFile)) return newMemoryStream;

                if (File.Exists(cacheFile))
                {
                    using var fileStream = File.Open(
                        cacheFile, FileMode.Open, FileAccess.Read,
                        FileShare.ReadWrite
                    );
                    fileStream.CopyTo(newMemoryStream);
                }
                else
                {
                    using var cacheFileStream = File.Open(
                        cacheFile, FileMode.CreateNew, FileAccess.Write,
                        FileShare.Read
                    );
                    using (var fileStream = File.Open(
                               fullPath, FileMode.Open, FileAccess.Read,
                               FileShare.ReadWrite
                           ))
                    {
                        if (compress)
                        {
                            using var compressedStream = new DeflateStream(
                                newMemoryStream, CompressionMode.Compress,
                                true
                            );
                            fileStream.CopyTo(compressedStream);
                        }
                        else
                        {
                            fileStream.CopyTo(newMemoryStream);
                        }
                    }

                    newMemoryStream.Position = 0;
                    newMemoryStream.CopyTo(cacheFileStream);
                }

                newMemoryStream.Position = 0;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                newMemoryStream = new MemoryStream();
            }

            return newMemoryStream;
        }

        private static bool IsBinaryPath(string pathname)
        {
            var result = false;

            try
            {
                if (string.IsNullOrWhiteSpace(pathname)) return result;

                result =
                    pathname.EndsWith(
                        ".dll", StringComparison.OrdinalIgnoreCase
                    ) || pathname.EndsWith(
                        ".exe", StringComparison.OrdinalIgnoreCase
                    );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return result;
        }

        private static bool CompareAssemblyName(string matchText, string assemblyName)
        {
            var result = false;

            try
            {
                if (string.IsNullOrWhiteSpace(matchText)) return result;
                if (string.IsNullOrWhiteSpace(assemblyName)) return result;

                result = matchText.EndsWith("*") && matchText.Length > 1
                    ? assemblyName.StartsWith(
                        matchText.Substring(0, matchText.Length - 1),
                        StringComparison.OrdinalIgnoreCase
                    )
                    : matchText.Equals(
                        assemblyName, StringComparison.OrdinalIgnoreCase
                    );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                result = false;
            }

            return result;
        }

        private void Embed(string prefix, string fullPath, bool compress,
            bool addChecksum, bool disableCleanup, Checksums checksums)
        {
            try
            {
                InnerEmbed(
                    prefix, fullPath, compress, addChecksum, disableCleanup,
                    checksums
                );
            }
            catch (Exception exception)
            {
                throw new Exception(
                    innerException: exception, message: $@"Failed to embed.
                        prefix: {prefix}
                        fullPath: {fullPath}
                        compress: {compress}
                        addChecksum: {addChecksum}
                        disableCleanup: {disableCleanup}"
                );
            }
        }

        private static IEnumerable<string> GetFilteredReferences(
            IEnumerable<string> onlyBinaries, PackerAttribute config)
        {
            if (onlyBinaries == null || !onlyBinaries.Any())
                yield break;
            if (config == null) yield break;

            if (config.IncludeAssemblies.Any())
            {
                var skippedAssemblies =
                    new List<string>(config.IncludeAssemblies);

                foreach (var file in onlyBinaries)
                {
                    if (string.IsNullOrWhiteSpace(file)) continue;

                    var assemblyName = Path.GetFileNameWithoutExtension(file);

                    if (config.IncludeAssemblies.Any(
                            x => CompareAssemblyName(x, assemblyName)
                        ) && config.Unmanaged32Assemblies.All(
                            x => !CompareAssemblyName(x, assemblyName)
                        ) && config.Unmanaged64Assemblies.All(
                            x => !CompareAssemblyName(x, assemblyName)
                        ))
                    {
                        skippedAssemblies.Remove(
                            config.IncludeAssemblies.First(
                                x => CompareAssemblyName(x, assemblyName)
                            )
                        );
                        yield return file;
                    }
                }

                if (skippedAssemblies.Count <= 0) 
                    yield break;

                var splittedReferences =
                    Array.Empty<string>(); // References.Split(';');

                var hasErrors = false;

                foreach (var fileName in skippedAssemblies.Select(
                             skippedAssembly
                                 => (from splittedReference in
                                         splittedReferences
                                     where string.Equals(
                                         Path.GetFileNameWithoutExtension(
                                             splittedReference
                                         ), skippedAssembly,
                                         StringComparison.InvariantCulture
                                     )
                                     select splittedReference)
                                 .FirstOrDefault()
                         ))
                {
                    if (string.IsNullOrEmpty(fileName))
                    {
                        hasErrors = true;

                        // TODO  LogError($"Assembly '{skippedAssembly}' cannot be found (not even as CopyLocal='false'), please update the configuration");
                        continue;
                    }

                    yield return fileName;
                }

                if (hasErrors)
                    throw new Exception(
                        "One or more errors occurred, please check the log"
                    );

                yield break;
            }

            if (config.ExcludeAssemblies.Any())
            {
                foreach (var file in onlyBinaries
                                     .Except(config.Unmanaged32Assemblies)
                                     .Except(config.Unmanaged64Assemblies))
                {
                    var assemblyName = Path.GetFileNameWithoutExtension(file);

                    if (config.ExcludeAssemblies.Any(
                            x => CompareAssemblyName(x, assemblyName)
                        ) || config.Unmanaged32Assemblies.Any(
                            x => CompareAssemblyName(x, assemblyName)
                        ) || config.Unmanaged64Assemblies.Any(
                            x => CompareAssemblyName(x, assemblyName)
                        ))
                        continue;
                    yield return file;
                }

                yield break;
            }

            if (config.OptOut)
                foreach (var file in onlyBinaries)
                {
                    var assemblyName = Path.GetFileNameWithoutExtension(file);

                    if (config.Unmanaged32Assemblies.All(
                            x => !CompareAssemblyName(x, assemblyName)
                        ) && config.Unmanaged64Assemblies.All(
                            x => !CompareAssemblyName(x, assemblyName)
                        ))
                        yield return file;
                }
        }

        private void InnerEmbed(string prefix, string fullPath, bool compress,
            bool addChecksum, bool disableCleanup, Checksums checksums)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(prefix)) return;
                if (string.IsNullOrWhiteSpace(fullPath)) return;
                if (!File.Exists(fullPath)) return;
                if (checksums == null || !checksums.AllChecksums.Any()) return;

                if (!disableCleanup)
                {
                    // in any case we can remove this from the copy local paths,
                    // because either it's already embedded, or it will be embedded.
                    // ReferenceCopyLocalPaths.RemoveAll(item => string.Equals(item,
                    // fullPath, StringComparison.OrdinalIgnoreCase));
                }

                var resourceName =
                    $"{prefix}{Path.GetFileName(fullPath).ToLowerInvariant()}";

                if (_manifest.Resources.Any(
                        x => string.Equals(
                            x.Name, resourceName,
                            StringComparison.OrdinalIgnoreCase
                        )
                    ))
                {
                    // - an assembly that is already embedded uncompressed, using
                    // <EmbeddedResource> in the project file
                    // - if compress == false: an assembly that appeared twice in the
                    // ReferenceCopyLocalPaths, e.g. the same library from different nuget
                    // packages (https://github.com/Fody/Costura/issues/332)
                    if (addChecksum && !checksums.ContainsKey(resourceName))
                        checksums.Add(
                            resourceName, Checksums.CalculateChecksum(fullPath)
                        );

                    return;
                }

                if (compress)
                {
                    resourceName += ".compressed";

                    if (_manifest.Resources.Any(
                            x => string.Equals(
                                x.Name, resourceName,
                                StringComparison.OrdinalIgnoreCase
                            )
                        ))

                        // an assembly that appeared twice in the ReferenceCopyLocalPaths, e.g. the same library from different nuget packages (https://github.com/Fody/Costura/issues/332)
                        return;
                }

                var checksum = Checksums.CalculateChecksum(fullPath);
                if (!Directory.Exists(_cachePath))
                    Directory.CreateDirectory(_cachePath);

                var cacheFile = Path.Combine(
                    _cachePath, $"{checksum}.{resourceName}"
                );
                var memoryStream = BuildMemoryStream(
                    fullPath, compress, cacheFile
                );
                if (memoryStream == null || memoryStream.Length == 0) return;

                _streams.Add(memoryStream);
                var resource = new ManifestResourceDeclaration
                {
                    Name = resourceName, IsPublic = false,
                    ContentStreamProvider = () => memoryStream
                };

                _manifest.Resources.Add(resource);

                if (addChecksum) checksums.Add(resourceName, checksum);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}