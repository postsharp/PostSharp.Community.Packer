using PostSharp.Sdk.CodeModel;
using System;

namespace PostSharp.Community.Packer.Weaver
{
    internal static class ResourceCaseFixer
    {
        public static void FixResourceCase(AssemblyManifestDeclaration manifest)
        {
            foreach (var resource in manifest.Resources)
            {
                if (resource.Name.StartsWith("costura.", StringComparison.OrdinalIgnoreCase))
                {
                    resource.Name = resource.Name.ToLowerInvariant();
                }
            }
        }
    }
}