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
        private readonly AssemblyManifestDeclaration manifest;
        private readonly List<Stream> streams = new List<Stream>();
        private string cachePath;

        public ResourceEmbedder(AssemblyManifestDeclaration manifest)
        {
            this.manifest = manifest;
        }

        public bool HasUnmanaged { get; private set; }

        public void Dispose()
        {
            if (streams == null)
            {
                return;
            }
            foreach (var stream in streams)
            {
                stream.Dispose();
            }
        }

        public void EmbedResources(PackerAttribute config, string[] referenceCopyLocalPaths, Checksums checksums)
        {
            var tempDirectory = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());

            cachePath = tempDirectory; //  Path.Combine(Path.GetDirectoryName(AssemblyFilePath), "Costura");
            Directory.CreateDirectory(cachePath);

            var onlyBinaries = referenceCopyLocalPaths.Where(x => x.EndsWith(".dll", StringComparison.OrdinalIgnoreCase)
                                                                  || x.EndsWith(".exe", StringComparison.OrdinalIgnoreCase)).ToArray();

            var disableCompression = config.DisableCompression;
            var createTemporaryAssemblies = config.CreateTemporaryAssemblies;

            foreach (var dependency in GetFilteredReferences(onlyBinaries, config))
            {
                var fullPath = Path.GetFullPath(dependency);

                if (!config.IgnoreSatelliteAssemblies)
                {
                    if (dependency.EndsWith(".resources.dll", StringComparison.OrdinalIgnoreCase))
                    {
                        Embed($"costura.{Path.GetFileName(Path.GetDirectoryName(fullPath))}.", fullPath, !disableCompression, createTemporaryAssemblies, config.DisableCleanup, checksums);
                        continue;
                    }
                }

                Embed("costura.", fullPath, !disableCompression, createTemporaryAssemblies, config.DisableCleanup, checksums);

                if (!config.IncludeDebugSymbols)
                {
                    continue;
                }
                var pdbFullPath = Path.ChangeExtension(fullPath, "pdb");
                if (File.Exists(pdbFullPath))
                {
                    Embed("costura.", pdbFullPath, !disableCompression, createTemporaryAssemblies, config.DisableCleanup, checksums);
                }
            }

            foreach (var dependency in onlyBinaries)
            {
                var prefix = "";

                if (config.Unmanaged32Assemblies.Any(x => string.Equals(x, Path.GetFileNameWithoutExtension(dependency), StringComparison.OrdinalIgnoreCase)))
                {
                    prefix = "costura32.";
                    this.HasUnmanaged = true;
                }
                if (config.Unmanaged64Assemblies.Any(x => string.Equals(x, Path.GetFileNameWithoutExtension(dependency), StringComparison.OrdinalIgnoreCase)))
                {
                    prefix = "costura64.";
                    this.HasUnmanaged = true;
                }

                if (string.IsNullOrEmpty(prefix))
                {
                    continue;
                }

                var fullPath = Path.GetFullPath(dependency);
                Embed(prefix, fullPath, !disableCompression, true, config.DisableCleanup, checksums);

                if (!config.IncludeDebugSymbols)
                {
                    continue;
                }
                var pdbFullPath = Path.ChangeExtension(fullPath, "pdb");
                if (File.Exists(pdbFullPath))
                {
                    Embed(prefix, pdbFullPath, !disableCompression, true, config.DisableCleanup, checksums);
                }
            }
        }

        private static MemoryStream BuildMemoryStream(string fullPath, bool compress, string cacheFile)
        {
            var memoryStream = new MemoryStream();

            if (File.Exists(cacheFile))
            {
                using (var fileStream = File.Open(cacheFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fileStream.CopyTo(memoryStream);
                }
            }
            else
            {
                using (var cacheFileStream = File.Open(cacheFile, FileMode.CreateNew, FileAccess.Write, FileShare.Read))
                {
                    using (var fileStream = File.Open(fullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    {
                        if (compress)
                        {
                            using (var compressedStream = new DeflateStream(memoryStream, CompressionMode.Compress, true))
                            {
                                fileStream.CopyTo(compressedStream);
                            }
                        }
                        else
                        {
                            fileStream.CopyTo(memoryStream);
                        }
                    }

                    memoryStream.Position = 0;
                    memoryStream.CopyTo(cacheFileStream);
                }
            }

            memoryStream.Position = 0;
            return memoryStream;
        }

        private bool CompareAssemblyName(string matchText, string assemblyName)
        {
            if (matchText.EndsWith("*") && matchText.Length > 1)
            {
                return assemblyName.StartsWith(matchText.Substring(0, matchText.Length - 1), StringComparison.OrdinalIgnoreCase);
            }

            return matchText.Equals(assemblyName, StringComparison.OrdinalIgnoreCase);
        }

        private void Embed(string prefix, string fullPath, bool compress, bool addChecksum, bool disableCleanup, Checksums checksums)
        {
            try
            {
                InnerEmbed(prefix, fullPath, compress, addChecksum, disableCleanup, checksums);
            }
            catch (Exception exception)
            {
                throw new Exception(
                    innerException: exception,
                    message: $@"Failed to embed.
prefix: {prefix}
fullPath: {fullPath}
compress: {compress}
addChecksum: {addChecksum}
disableCleanup: {disableCleanup}");
            }
        }

        private IEnumerable<string> GetFilteredReferences(IEnumerable<string> onlyBinaries, PackerAttribute config)
        {
            if (config.IncludeAssemblies.Any())
            {
                var skippedAssemblies = new List<string>(config.IncludeAssemblies);

                foreach (var file in onlyBinaries)
                {
                    var assemblyName = Path.GetFileNameWithoutExtension(file);

                    if (config.IncludeAssemblies.Any(x => CompareAssemblyName(x, assemblyName)) &&
                        config.Unmanaged32Assemblies.All(x => !CompareAssemblyName(x, assemblyName)) &&
                        config.Unmanaged64Assemblies.All(x => !CompareAssemblyName(x, assemblyName)))
                    {
                        skippedAssemblies.Remove(config.IncludeAssemblies.First(x => CompareAssemblyName(x, assemblyName)));
                        yield return file;
                    }
                }

                if (skippedAssemblies.Count > 0)
                {
                    var splittedReferences = new string[0];// References.Split(';');

                    var hasErrors = false;

                    foreach (var skippedAssembly in skippedAssemblies)
                    {
                        var fileName = (from splittedReference in splittedReferences
                                        where string.Equals(Path.GetFileNameWithoutExtension(splittedReference), skippedAssembly, StringComparison.InvariantCulture)
                                        select splittedReference).FirstOrDefault();
                        if (string.IsNullOrEmpty(fileName))
                        {
                            hasErrors = true;
                            // TODO  LogError($"Assembly '{skippedAssembly}' cannot be found (not even as CopyLocal='false'), please update the configuration");
                            continue;
                        }

                        yield return fileName;
                    }

                    if (hasErrors)
                    {
                        throw new Exception("One or more errors occurred, please check the log");
                    }
                }

                yield break;
            }
            if (config.ExcludeAssemblies.Any())
            {
                foreach (var file in onlyBinaries.Except(config.Unmanaged32Assemblies).Except(config.Unmanaged64Assemblies))
                {
                    var assemblyName = Path.GetFileNameWithoutExtension(file);

                    if (config.ExcludeAssemblies.Any(x => CompareAssemblyName(x, assemblyName)) ||
                        config.Unmanaged32Assemblies.Any(x => CompareAssemblyName(x, assemblyName)) ||
                        config.Unmanaged64Assemblies.Any(x => CompareAssemblyName(x, assemblyName)))
                    {
                        continue;
                    }
                    yield return file;
                }
                yield break;
            }
            if (config.OptOut)
            {
                foreach (var file in onlyBinaries)
                {
                    var assemblyName = Path.GetFileNameWithoutExtension(file);

                    if (config.Unmanaged32Assemblies.All(x => !CompareAssemblyName(x, assemblyName)) &&
                        config.Unmanaged64Assemblies.All(x => !CompareAssemblyName(x, assemblyName)))
                    {
                        yield return file;
                    }
                }
            }
        }

        private void InnerEmbed(string prefix, string fullPath, bool compress, bool addChecksum, bool disableCleanup, Checksums checksums)
        {
            if (!disableCleanup)
            {
                // in any case we can remove this from the copy local paths, because either it's already embedded, or it will be embedded.
                // ReferenceCopyLocalPaths.RemoveAll(item => string.Equals(item, fullPath, StringComparison.OrdinalIgnoreCase));
            }

            var resourceName = $"{prefix}{Path.GetFileName(fullPath).ToLowerInvariant()}";

            if (manifest.Resources.Any(x => string.Equals(x.Name, resourceName, StringComparison.OrdinalIgnoreCase)))
            {
                // - an assembly that is already embedded uncompressed, using <EmbeddedResource> in the project file
                // - if compress == false: an assembly that appeared twice in the ReferenceCopyLocalPaths, e.g. the same library from different nuget packages (https://github.com/Fody/Costura/issues/332)
                if (addChecksum && !checksums.ContainsKey(resourceName))
                {
                    checksums.Add(resourceName, Checksums.CalculateChecksum(fullPath));
                }

                return;
            }

            if (compress)
            {
                resourceName += ".compressed";

                if (manifest.Resources.Any(x => string.Equals(x.Name, resourceName, StringComparison.OrdinalIgnoreCase)))
                {
                    // an assembly that appeared twice in the ReferenceCopyLocalPaths, e.g. the same library from different nuget packages (https://github.com/Fody/Costura/issues/332)
                    return;
                }
            }

            var checksum = Checksums.CalculateChecksum(fullPath);
            var cacheFile = Path.Combine(cachePath, $"{checksum}.{resourceName}");
            var memoryStream = BuildMemoryStream(fullPath, compress, cacheFile);
            streams.Add(memoryStream);
            var resource = new ManifestResourceDeclaration
            {
                Name = resourceName,
                IsPublic = false
            };

            resource.ContentStreamProvider = () => memoryStream;

            manifest.Resources.Add(resource);

            if (addChecksum)
            {
                checksums.Add(resourceName, checksum);
            }
        }
    }
}