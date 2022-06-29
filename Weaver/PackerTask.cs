using PostSharp.Sdk.Extensibility;
using PostSharp.Sdk.Extensibility.Configuration;
using PostSharp.Sdk.Extensibility.Tasks;
using System;
using System.Linq;

namespace PostSharp.Community.Packer.Weaver
{
    [ExportTask(Phase = TaskPhase.Transform, TaskName = nameof(PackerTask))]
    [TaskDependency(
        TaskNames.AspectInfrastructure, IsRequired = true,
        Position = DependencyPosition.After
    )]
    public class PackerTask : Task
    {
        [ImportService] public IAnnotationRepositoryService annotationsService;

        public override bool Execute()
        {
            var result = false;

            try
            {
                // Find configuration:
                var annotations = annotationsService.GetAnnotationsOfType(
                    typeof(PackerAttribute), false, true
                );
                var config = new PackerAttribute();
                if (annotations.MoveNext())
                    config = Configuration.Read(annotations.Current);

                // Find gatherable assemblies:
                var paths = Project.Properties["ReferenceCopyLocalPaths"]
                                   ?.Split('|') ?? Array.Empty<string>();
                if (!paths.Any()) return result;

                var manifest = Project.Module.AssemblyManifest;

                // I have no idea what this is doing:
                ResourceCaseFixer.FixResourceCase(manifest);

                // Embed resources:
                var checksums = new Checksums();
                var unmanagedFromProcessor =
                    NativeResourcesProcessor.ProcessNativeResources(
                        manifest, !config.DisableCompression, checksums
                    );
                var resourceEmbedder = new ResourceEmbedder(manifest);
                resourceEmbedder.EmbedResources(config, paths, checksums);
                var unmanagedFromEmbedder = resourceEmbedder.HasUnmanaged;

                // Load references:
                var assets = new Assets(Project.Module);
                var info = AssemblyLoaderInfo.LoadAssemblyLoader(
                    config.CreateTemporaryAssemblies,
                    unmanagedFromEmbedder || unmanagedFromProcessor, Project.Module
                );

                // Alter code:
                var resourcesHash = ResourceHash.CalculateHash(manifest);
                new AttachCallSynthesis().SynthesizeCallToAttach(
                    config, Project, info
                );
                new ResourceNameFinder(info, manifest, assets)
                    .FillInStaticConstructor(
                        config.CreateTemporaryAssemblies, config.PreloadOrder,
                        resourcesHash, checksums
                    );

                result = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                result = false;
            }

            return result;
        }
    }
}