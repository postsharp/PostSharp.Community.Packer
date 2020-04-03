using System;
using System.Collections.Generic;
using System.Reflection;
using PostSharp.Community.Packer;
using PostSharp.Extensibility;
using PostSharp.Reflection;
using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.CodeModel.TypeSignatures;
using PostSharp.Sdk.Extensibility;
using PostSharp.Sdk.Extensibility.Configuration;
using PostSharp.Sdk.Extensibility.Tasks;

namespace PostSharp.Community.Packer.Weaver
{
     [ExportTask(Phase = TaskPhase.Transform, TaskName = nameof(PackerTask))] 
     [TaskDependency(TaskNames.AspectInfrastructure, IsRequired = true, Position = DependencyPosition.After)]
     public partial class PackerTask : Task
     {
         [ImportService]
         private IAnnotationRepositoryService annotationsService;

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
            
            manifest = Project.Module.AssemblyManifest;
            
            var assets = new Assets(Project.Module);
            var checksums = new Checksums();
            FixResourceCase(manifest);
            ProcessNativeResources(manifest, !config.DisableCompression, checksums);
            EmbedResources(config, paths, checksums);

            // CalculateHash();
            AssemblyLoaderInfo info = AssemblyLoaderInfo.LoadAssemblyLoader(config.CreateTemporaryAssemblies,
                false /*TODO*/,
                Project.Module);
            new AttachCallSynthesis().SynthesizeCallToAttach(config, Project, info);
            new ResourceNameFinder(info, manifest, assets).BuildUpNameDictionary(config.CreateTemporaryAssemblies,
                config.PreloadOrder, checksums);
            
            return true;
        }
     }
}