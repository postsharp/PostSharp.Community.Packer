using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.Collections;
using System;
using System.Linq;

namespace PostSharp.Community.Packer.Weaver
{
    public class ResourceNameFinder
    {
        private readonly Assets assets;
        private readonly AssemblyLoaderInfo info;
        private readonly AssemblyManifestDeclaration manifest;

        public ResourceNameFinder(AssemblyLoaderInfo info, AssemblyManifestDeclaration manifest, Assets assets)
        {
            this.info = info;
            this.manifest = manifest;
            this.assets = assets;
        }

        public void FillInStaticConstructor(bool createTemporaryAssemblies, string[] preloadOrder, string resourcesHash, Checksums checksums)
        {
            var loaderMethod = info.StaticConstructorMethod;
            InstructionReader reader = loaderMethod.MethodBody.CreateInstructionReader();
            reader.EnterInstructionBlock(loaderMethod.MethodBody.RootInstructionBlock);
            reader.EnterInstructionSequence(loaderMethod.MethodBody.RootInstructionBlock.LastInstructionSequence);
            while (reader.ReadInstruction())
            {
                if (reader.CurrentInstruction.OpCodeNumber == OpCodeNumber.Ret)
                {
                    break;
                }
            }
            Console.WriteLine(reader.CurrentInstruction);
            reader.CurrentInstructionSequence.SplitAroundReaderPosition(reader, out _, out _);
            var newSequence =
                reader.CurrentInstructionBlock.AddInstructionSequence(null, NodePosition.Before,
                    reader.CurrentInstructionSequence);

            InstructionWriter writer = InstructionWriter.GetInstance();
            writer.AttachInstructionSequence(newSequence);
            var orderedResources = preloadOrder
                .Join(this.manifest.Resources, p => p.ToLowerInvariant(),
                    r =>
                    {
                        var parts = r.Name.Split('.');
                        GetNameAndExt(parts, out var name, out _);
                        return name;
                    }, (s, r) => r)
                .Union(this.manifest.Resources.OrderBy(r => r.Name))
                .Where(r => r.Name.StartsWith("costura", StringComparison.OrdinalIgnoreCase))
                .Select(r => r.Name);

            foreach (var resource in orderedResources)
            {
                var parts = resource.Split('.');

                GetNameAndExt(parts, out var name, out var ext);

                if (string.Equals(parts[0], "costura", StringComparison.OrdinalIgnoreCase))
                {
                    if (createTemporaryAssemblies)
                    {
                        AddToList(writer, info.PreloadListField, resource);
                    }
                    else
                    {
                        if (string.Equals(ext, "pdb", StringComparison.OrdinalIgnoreCase))
                        {
                            AddToDictionary(writer, info.SymbolNamesField, name, resource);
                        }
                        else
                        {
                            AddToDictionary(writer, info.AssemblyNamesField, name, resource);
                        }
                    }
                }
                else if (string.Equals(parts[0], "costura32", StringComparison.OrdinalIgnoreCase))
                {
                    AddToList(writer, info.Preload32ListField, resource);
                }
                else if (string.Equals(parts[0], "costura64", StringComparison.OrdinalIgnoreCase))
                {
                    AddToList(writer, info.Preload64ListField, resource);
                }
            }

            if (info.ChecksumsField != null)
            {
                foreach (var checksum in checksums.AllChecksums)
                {
                    AddToDictionary(writer, info.ChecksumsField, checksum.Key, checksum.Value);
                }
            }

            if (info.Md5HashField != null)
            {
                writer.EmitInstructionString(OpCodeNumber.Ldstr, resourcesHash);
                writer.EmitInstructionField(OpCodeNumber.Stsfld, info.Md5HashField);
            }

            writer.DetachInstructionSequence();
        }

        private static void GetNameAndExt(string[] parts, out string name, out string ext)
        {
            var isCompressed = string.Equals(parts[parts.Length - 1], "compressed", StringComparison.OrdinalIgnoreCase);

            ext = parts[parts.Length - (isCompressed ? 2 : 1)];

            name = string.Join(".", parts.Skip(1).Take(parts.Length - (isCompressed ? 3 : 2)));
        }

        private void AddToDictionary(InstructionWriter writer, FieldDefDeclaration field, string key, string name)
        {
            writer.EmitInstructionField(OpCodeNumber.Ldsfld, field);
            writer.EmitInstructionString(OpCodeNumber.Ldstr, key);
            writer.EmitInstructionString(OpCodeNumber.Ldstr, name);
            writer.EmitInstructionMethod(OpCodeNumber.Callvirt, assets.DictionaryOfStringOfStringAdd);
        }

        private void AddToList(InstructionWriter writer, FieldDefDeclaration field, string name)
        {
            writer.EmitInstructionField(OpCodeNumber.Ldsfld, field);
            writer.EmitInstructionString(OpCodeNumber.Ldstr, name);
            writer.EmitInstructionMethod(OpCodeNumber.Callvirt, assets.ListOfStringAdd);
        }
    }
}