using PostSharp.Sdk.CodeModel;

namespace PostSharp.Community.Packer.Weaver
{
    public class AssemblyLoaderInfo
    {
        public MethodDefDeclaration AttachMethod { get; private set; }
        public MethodDefDeclaration StaticConstructorMethod { get; private set;}
        public bool HasUnmanaged { get; }
        public FieldDefDeclaration AssemblyNamesField { get; private set;}
        public FieldDefDeclaration SymbolNamesField { get; private set;}
        public FieldDefDeclaration PreloadListField { get; private set;}
        public FieldDefDeclaration Preload32ListField { get;private set; }
        public FieldDefDeclaration Preload64ListField { get;private set; }
        public FieldDefDeclaration ChecksumsField { get;private set; }

        public static AssemblyLoaderInfo LoadAssemblyLoader(bool createTemporaryAssemblies, 
                                                            bool hasUnmanaged, 
                                                            ModuleDeclaration module)
        {
            AssemblyLoaderInfo info = new AssemblyLoaderInfo();
            TypeDefDeclaration sourceType;
            if (createTemporaryAssemblies)
            {
                sourceType = module.FindType("PostSharp.Community.Packer.Templates.ILTemplateWithTempAssembly")
                    .GetTypeDefinition();
            }
            else if (hasUnmanaged)
            {
                sourceType = module.FindType("PostSharp.Community.Packer.Templates.ILTemplateWithUnmanagedHandler")
                    .GetTypeDefinition();
            }
            else
            {
                sourceType = module.FindType("PostSharp.Community.Packer.Templates.ILTemplate")
                    .GetTypeDefinition();
            }
            
            TypeDefDeclaration commonType = 
                module.FindType("PostSharp.Community.Packer.Templates.Common")
                    .GetTypeDefinition();

            info.AttachMethod = module.FindMethod(sourceType, "Attach").GetMethodDefinition();
            info.StaticConstructorMethod = module.FindMethod(sourceType, ".cctor").GetMethodDefinition();

            info.AssemblyNamesField = sourceType.FindField("assemblyNames")?.Field;
            info.SymbolNamesField = sourceType.FindField("symbolNames")?.Field;
            info.PreloadListField = sourceType.FindField("preloadList")?.Field;
            info.Preload32ListField = sourceType.FindField("preload32List")?.Field;
            info.Preload64ListField = sourceType.FindField("preload64List")?.Field;
            info.ChecksumsField = sourceType.FindField("checksums")?.Field;
            return info;
        }
    }
}