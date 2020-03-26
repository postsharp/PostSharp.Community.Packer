using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using PostSharp.Extensibility;
using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.CodeModel.Collections;

namespace PostSharp.Community.JustOneExe.Weaver
{
    public class Configuration
    {
        //
        //     IncludeDebugSymbols = ReadBool(config, "IncludeDebugSymbols", IncludeDebugSymbols);
        //     DisableCompression = ReadBool(config, "DisableCompression", DisableCompression);
        //     DisableCleanup = ReadBool(config, "DisableCleanup", DisableCleanup);
        //     LoadAtModuleInit = ReadBool(config, "LoadAtModuleInit", LoadAtModuleInit);
        //     CreateTemporaryAssemblies = ReadBool(config, "CreateTemporaryAssemblies", CreateTemporaryAssemblies);
        //     IgnoreSatelliteAssemblies = ReadBool(config, "IgnoreSatelliteAssemblies", IgnoreSatelliteAssemblies);
        //
        //     ExcludeAssemblies = ReadList(config, "ExcludeAssemblies");
        //     IncludeAssemblies = ReadList(config, "IncludeAssemblies");
        //     Unmanaged32Assemblies = ReadList(config, "Unmanaged32Assemblies");
        //     Unmanaged64Assemblies = ReadList(config, "Unmanaged64Assemblies");
        //     PreloadOrder = ReadList(config, "PreloadOrder");
        //
        //     if (IncludeAssemblies.Any() && ExcludeAssemblies.Any())
        //     {
        //         throw new Exception("Either configure IncludeAssemblies OR ExcludeAssemblies, not both.");
        //     }
        // }
        public static JustOneExeAttribute Read(IAnnotationInstance annotation)
        {
            JustOneExeAttribute config = new JustOneExeAttribute();
            MemberValuePairCollection namedArguments = annotation.Value.NamedArguments;

            config.DisableCleanup = namedArguments.GetSafeBool(nameof(JustOneExeAttribute.DisableCleanup));
            config.DisableCompression = namedArguments.GetSafeBool(nameof(JustOneExeAttribute.DisableCompression));
            config.IncludeDebugSymbols = namedArguments.GetSafeBool(nameof(JustOneExeAttribute.IncludeDebugSymbols));
            config.LoadAtModuleInit = namedArguments.GetSafeBool(nameof(JustOneExeAttribute.LoadAtModuleInit));
            config.CreateTemporaryAssemblies = namedArguments.GetSafeBool(nameof(JustOneExeAttribute.CreateTemporaryAssemblies));
            config.IgnoreSatelliteAssemblies = namedArguments.GetSafeBool(nameof(JustOneExeAttribute.IgnoreSatelliteAssemblies));
            
            config.IncludeAssemblies = namedArguments.GetSafeStringArray(nameof(JustOneExeAttribute.IncludeAssemblies));
            config.ExcludeAssemblies = namedArguments.GetSafeStringArray(nameof(JustOneExeAttribute.ExcludeAssemblies));
            config.PreloadOrder = namedArguments.GetSafeStringArray(nameof(JustOneExeAttribute.PreloadOrder));
            config.Unmanaged32Assemblies = namedArguments.GetSafeStringArray(nameof(JustOneExeAttribute.Unmanaged32Assemblies));
            config.Unmanaged64Assemblies = namedArguments.GetSafeStringArray(nameof(JustOneExeAttribute.Unmanaged64Assemblies));
            
            if (config.IncludeAssemblies != null && config.IncludeAssemblies.Length > 0 &&
                config.ExcludeAssemblies != null && config.ExcludeAssemblies.Length > 0)
            {
                Message.Write(annotation.TargetElement, SeverityType.Error, "JOE01", "Set IncludeAssemblies, or ExcludeAssemblies, but not both.");
            }
            return config;
        }
    }
}