using System;
using System.Collections.Generic;
using System.Reflection;
using PostSharp.Extensibility;
using PostSharp.Reflection;
using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.CodeModel.TypeSignatures;
using PostSharp.Sdk.Extensibility;
using PostSharp.Sdk.Extensibility.Configuration;
using PostSharp.Sdk.Extensibility.Tasks;

namespace PostSharp.Community.JustOneExe.Weaver
{
    // TODO change "Transform" to "CustomTransform" when it starts working
    [ExportTask(Phase = TaskPhase.Transform, TaskName = nameof(JustOneExeTask))] 
     public partial class JustOneExeTask : Task
     {
         [ImportService]
         private IAnnotationRepositoryService annotationsService;

         public override bool Execute()
        {
            // Find configuration:
            var annotations =
                annotationsService.GetAnnotationsOfType(typeof(JustOneExeAttribute), false, true);
            JustOneExeAttribute config = new JustOneExeAttribute();
            if (annotations.MoveNext())
            {
                config = Configuration.Read(annotations.Current);
            }

            var manifest = this.Project.Module.AssemblyManifest;
            
            FindMsCoreReferences();
            FixResourceCase(manifest);
            ProcessNativeResources(manifest, !config.DisableCompression);
            EmbedResources(config);

            // CalculateHash();
            // ImportAssemblyLoader(config.CreateTemporaryAssemblies);
            // CallAttach(config);
            //
            // AddChecksumsToTemplate();
            // BuildUpNameDictionary(config.CreateTemporaryAssemblies, config.PreloadOrder);
            
            return true;
        }
     }
}