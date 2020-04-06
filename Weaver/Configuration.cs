using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using PostSharp.Community.Packer;
using PostSharp.Extensibility;
using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.CodeModel.Collections;

namespace PostSharp.Community.Packer.Weaver
{
    public class Configuration
    {
        public static Packer.PackerAttribute Read(IAnnotationInstance annotation)
        {
            Packer.PackerAttribute config = new Packer.PackerAttribute();
            MemberValuePairCollection namedArguments = annotation.Value.NamedArguments;

            config.DisableCleanup = namedArguments.GetSafeBool(nameof(Packer.PackerAttribute.DisableCleanup), false);
            config.DisableCompression = namedArguments.GetSafeBool(nameof(Packer.PackerAttribute.DisableCompression), false);
            config.IncludeDebugSymbols = namedArguments.GetSafeBool(nameof(Packer.PackerAttribute.IncludeDebugSymbols), true);
            config.LoadAtModuleInit = namedArguments.GetSafeBool(nameof(Packer.PackerAttribute.LoadAtModuleInit), true);
            config.CreateTemporaryAssemblies = namedArguments.GetSafeBool(nameof(Packer.PackerAttribute.CreateTemporaryAssemblies), false);
            config.IgnoreSatelliteAssemblies = namedArguments.GetSafeBool(nameof(Packer.PackerAttribute.IgnoreSatelliteAssemblies), false);
            
            config.IncludeAssemblies = namedArguments.GetSafeStringArray(nameof(Packer.PackerAttribute.IncludeAssemblies));
            config.ExcludeAssemblies = namedArguments.GetSafeStringArray(nameof(Packer.PackerAttribute.ExcludeAssemblies));
            config.PreloadOrder = namedArguments.GetSafeStringArray(nameof(Packer.PackerAttribute.PreloadOrder));
            config.Unmanaged32Assemblies = namedArguments.GetSafeStringArray(nameof(Packer.PackerAttribute.Unmanaged32Assemblies));
            config.Unmanaged64Assemblies = namedArguments.GetSafeStringArray(nameof(Packer.PackerAttribute.Unmanaged64Assemblies));
            
            if (config.IncludeAssemblies != null && config.IncludeAssemblies.Length > 0 &&
                config.ExcludeAssemblies != null && config.ExcludeAssemblies.Length > 0)
            {
                Message.Write(annotation.TargetElement, SeverityType.Error, "JOE01", "Set IncludeAssemblies, or ExcludeAssemblies, but not both.");
            }
            return config;
        }
    }
}