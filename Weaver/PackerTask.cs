using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.Extensibility;
using PostSharp.Sdk.Extensibility.Configuration;
using PostSharp.Sdk.Extensibility.Tasks;

namespace PostSharp.Community.Packer.Weaver
{
    [ExportTask(Phase = TaskPhase.Transform, TaskName = nameof(PackerTask))]
    [TaskDependency(TaskNames.AspectInfrastructure, IsRequired = true, Position = DependencyPosition.After)]
    public class PackerTask : Task
    {
        [ImportService] private IAnnotationRepositoryService annotationsService;

        public override bool Execute()
        {
            // Find configuration:
            var annotations =
                annotationsService.GetAnnotationsOfType(typeof(PackerAttribute), false, true);
            PackerAttribute config = new PackerAttribute();
            if (annotations.MoveNext())
            {
                config = Configuration.Read(annotations.Current);
            }

            // Find gatherable assemblies:
            string[] paths = Project.Properties["ReferenceCopyLocalPaths"]?.Split('|') ?? new string[0];

            AssemblyManifestDeclaration manifest = Project.Module.AssemblyManifest;

            // I have no idea what this is doing:
            ResourceCaseFixer.FixResourceCase(manifest);

            // Embed resources:
            var checksums = new Checksums();
            bool unmanagedFromProcessor = NativeResourcesProcessor.ProcessNativeResources(manifest, !config.DisableCompression, checksums);
            var resourceEmbedder = new ResourceEmbedder(manifest);
            resourceEmbedder.EmbedResources(config, paths, checksums);
            bool unmanagedFromEmbedder = resourceEmbedder.HasUnmanaged;

            // Load references:
            var assets = new Assets(Project.Module);
            AssemblyLoaderInfo info = AssemblyLoaderInfo.LoadAssemblyLoader(config.CreateTemporaryAssemblies,
                unmanagedFromEmbedder || unmanagedFromProcessor,
                Project.Module);

            // Alter code:
            string resourcesHash = ResourceHash.CalculateHash(manifest);
            new AttachCallSynthesis().SynthesizeCallToAttach(config, Project, info);
            new ResourceNameFinder(info, manifest, assets).FillInStaticConstructor(
                config.CreateTemporaryAssemblies,
                config.PreloadOrder,
                resourcesHash,
                checksums);

            return true;
        }
    }
}