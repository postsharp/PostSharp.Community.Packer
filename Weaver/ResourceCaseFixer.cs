using System;
using PostSharp.Sdk.CodeModel;

namespace PostSharp.Community.Packer.Weaver
{
    partial class JustOneExeTask
    {
        void FixResourceCase(AssemblyManifestDeclaration manifest)
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