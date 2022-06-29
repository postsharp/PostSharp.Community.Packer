using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.Collections;
using System;
using System.Collections.Generic;
using System.Linq;

namespace PostSharp.Community.Packer.Weaver
{
    public class ResourceNameFinder
    {
        private readonly AssemblyLoaderInfo _assemblyLoaderInfo;
        private readonly Assets _assets;
        private readonly AssemblyManifestDeclaration _manifest;

        /// <summary>
        /// Constructs a new instance of
        /// <see cref="T:PostSharp.Community.Packer.Weaver.ResourceNameFinder" /> and
        /// returns a reference to it.
        /// </summary>
        public ResourceNameFinder(AssemblyLoaderInfo assemblyLoaderInfo,
            AssemblyManifestDeclaration manifest, Assets assets)
        {
            if (assemblyLoaderInfo != null)
                _assemblyLoaderInfo = assemblyLoaderInfo;
            if (manifest != null) _manifest = manifest;
            if (assets != null) _assets = assets;
        }

        public void FillInStaticConstructor(bool createTemporaryAssemblies,
            IEnumerable<string> preloadOrder, string resourcesHash,
            Checksums checksums)
        {
            try
            {
                if (preloadOrder == null || !preloadOrder.Any()) return;
                if (string.IsNullOrWhiteSpace(resourcesHash)) return;
                if (checksums == null || !checksums.AllChecksums.Any()) return;

                var loaderMethod = _assemblyLoaderInfo.StaticConstructorMethod;
                if (loaderMethod == null) return;

                using var reader =
                    loaderMethod.MethodBody.CreateInstructionReader();
                reader.EnterInstructionBlock(
                    loaderMethod.MethodBody.RootInstructionBlock
                );
                reader.EnterInstructionSequence(
                    loaderMethod.MethodBody.RootInstructionBlock
                                .LastInstructionSequence
                );
                while (reader.ReadInstruction())
                    if (reader.CurrentInstruction.OpCodeNumber ==
                        OpCodeNumber.Ret)
                        break;
                Console.WriteLine(reader.CurrentInstruction);
                reader.CurrentInstructionSequence.SplitAroundReaderPosition(
                    reader, out _, out _
                );
                var newSequence =
                    reader.CurrentInstructionBlock.AddInstructionSequence(
                        null, NodePosition.Before,
                        reader.CurrentInstructionSequence
                    );

                using var writer = InstructionWriter.GetInstance();
                writer.AttachInstructionSequence(newSequence);
                var orderedResources = preloadOrder.Join(
                                                       _manifest.Resources,
                                                       p => p
                                                           .ToLowerInvariant(),
                                                       r =>
                                                       {
                                                           var parts =
                                                               r.Name.Split(
                                                                   '.'
                                                               );
                                                           GetNameAndExt(
                                                               parts,
                                                               out var name,
                                                               out _
                                                           );
                                                           return name;
                                                       }, (s, r) => r
                                                   )
                                                   .Union(
                                                       _manifest.Resources
                                                           .OrderBy(r => r.Name)
                                                   )
                                                   .Where(
                                                       r => r.Name.StartsWith(
                                                           "costura",
                                                           StringComparison
                                                               .OrdinalIgnoreCase
                                                       )
                                                   )
                                                   .Select(r => r.Name);
                if (orderedResources == null || !orderedResources.Any()) return;

                foreach (var resource in orderedResources)
                {
                    var parts = resource.Split('.');
                    if (parts == null || !parts.Any()) continue;

                    GetNameAndExt(parts, out var name, out var ext);

                    if (string.Equals(
                            parts.First(), "costura",
                            StringComparison.OrdinalIgnoreCase
                        ))
                    {
                        if (createTemporaryAssemblies)
                            AddToList(
                                writer, _assemblyLoaderInfo.PreloadListField,
                                resource
                            );
                        else
                            AddToDictionary(
                                writer,
                                string.Equals(
                                    ext, "pdb",
                                    StringComparison.OrdinalIgnoreCase
                                )
                                    ? _assemblyLoaderInfo.SymbolNamesField
                                    : _assemblyLoaderInfo.AssemblyNamesField,
                                name, resource
                            );
                    }
                    else if (string.Equals(
                                 parts.First(), "costura32",
                                 StringComparison.OrdinalIgnoreCase
                             ))
                    {
                        AddToList(
                            writer, _assemblyLoaderInfo.Preload32ListField,
                            resource
                        );
                    }
                    else if (string.Equals(
                                 parts.First(), "costura64",
                                 StringComparison.OrdinalIgnoreCase
                             ))
                    {
                        AddToList(
                            writer, _assemblyLoaderInfo.Preload64ListField,
                            resource
                        );
                    }
                }

                if (_assemblyLoaderInfo?.ChecksumsField != null)
                    foreach (var checksum in checksums.AllChecksums)
                        AddToDictionary(
                            writer, _assemblyLoaderInfo.ChecksumsField,
                            checksum.Key, checksum.Value
                        );

                if (_assemblyLoaderInfo?.Md5HashField != null)
                {
                    writer.EmitInstructionString(
                        OpCodeNumber.Ldstr, resourcesHash
                    );
                    writer.EmitInstructionField(
                        OpCodeNumber.Stsfld, _assemblyLoaderInfo.Md5HashField
                    );
                }

                writer.DetachInstructionSequence();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private static void GetNameAndExt(IReadOnlyList<string> parts,
            out string name, out string ext)
        {
            name = ext = string.Empty;  // default value of out parameters

            if (parts == null || !parts.Any()) return;

            var isCompressed = string.Equals(
                parts[parts.Count - 1], "compressed",
                StringComparison.OrdinalIgnoreCase
            );

            ext = parts[parts.Count - (isCompressed ? 2 : 1)];

            name = string.Join(
                ".", parts.Skip(1)
                          .Take(parts.Count - (isCompressed ? 3 : 2))
            );
        }

        private void AddToDictionary(BaseInstructionWriter writer, IField field,
            string key, string name)

        {
            try
            {
                if (writer == null) return;
                if (field == null) return;
                if (string.IsNullOrWhiteSpace(key)) return;
                if (string.IsNullOrWhiteSpace(name))
                    return;

                writer.EmitInstructionField(OpCodeNumber.Ldsfld, field);
                writer.EmitInstructionString(OpCodeNumber.Ldstr, key);
                writer.EmitInstructionString(OpCodeNumber.Ldstr, name);
                writer.EmitInstructionMethod(
                    OpCodeNumber.Callvirt, _assets.DictionaryOfStringOfStringAdd
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }

        private void AddToList(BaseInstructionWriter writer, IField field,
            string name)
        {
            try
            {
                if (writer == null) return;
                if (field == null) return;
                if (string.IsNullOrWhiteSpace(name)) return;

                writer.EmitInstructionField(OpCodeNumber.Ldsfld, field);
                writer.EmitInstructionString(OpCodeNumber.Ldstr, name);
                writer.EmitInstructionMethod(
                    OpCodeNumber.Callvirt, _assets.ListOfStringAdd
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}