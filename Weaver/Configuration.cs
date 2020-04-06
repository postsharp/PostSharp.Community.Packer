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
    public static class Configuration
    {
        public static PackerAttribute Read(IAnnotationInstance annotation)
        {
            PackerAttribute config = new PackerAttribute();
            MemberValuePairCollection namedArguments = annotation.Value.NamedArguments;

            config.DisableCleanup = namedArguments.GetSafeBool(nameof(PackerAttribute.DisableCleanup), false);
            config.DisableCompression = namedArguments.GetSafeBool(nameof(PackerAttribute.DisableCompression), false);
            config.IncludeDebugSymbols = namedArguments.GetSafeBool(nameof(PackerAttribute.IncludeDebugSymbols), true);
            config.LoadAtModuleInit = namedArguments.GetSafeBool(nameof(PackerAttribute.LoadAtModuleInit), true);
            config.CreateTemporaryAssemblies = namedArguments.GetSafeBool(nameof(PackerAttribute.CreateTemporaryAssemblies), false);
            config.IgnoreSatelliteAssemblies = namedArguments.GetSafeBool(nameof(PackerAttribute.IgnoreSatelliteAssemblies), false);
            
            config.IncludeAssemblies = namedArguments.GetSafeStringArray(nameof(PackerAttribute.IncludeAssemblies));
            config.ExcludeAssemblies = namedArguments.GetSafeStringArray(nameof(PackerAttribute.ExcludeAssemblies));
            config.PreloadOrder = namedArguments.GetSafeStringArray(nameof(PackerAttribute.PreloadOrder));
            config.Unmanaged32Assemblies = namedArguments.GetSafeStringArray(nameof(PackerAttribute.Unmanaged32Assemblies));
            config.Unmanaged64Assemblies = namedArguments.GetSafeStringArray(nameof(PackerAttribute.Unmanaged64Assemblies));
            
            if (config.IncludeAssemblies != null && config.IncludeAssemblies.Length > 0 &&
                config.ExcludeAssemblies != null && config.ExcludeAssemblies.Length > 0)
            {
                Message.Write(annotation.TargetElement, SeverityType.Error, "JOE01", "Set IncludeAssemblies, or ExcludeAssemblies, but not both.");
            }
            return config;
        }
    }
}