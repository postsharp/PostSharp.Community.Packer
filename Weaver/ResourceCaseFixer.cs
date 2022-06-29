using PostSharp.Sdk.CodeModel;
using System;
using System.Linq;

namespace PostSharp.Community.Packer.Weaver
{
    public static class ResourceCaseFixer
    {
        public static void FixResourceCase(AssemblyManifestDeclaration manifest)
        {
            if (manifest?.Resources == null || !manifest.Resources.Any()) return;

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