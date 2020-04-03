using System;
using PostSharp.Extensibility;

namespace PostSharp.Community.Packer
{
    
   
    /// <summary>
    /// Add <c>[assembly: Packer]</c> anywhere in your source code to ensure that all references are packed into
    /// your main output assembly.
    /// </summary>
    [AttributeUsage(AttributeTargets.Assembly)]
    [RequirePostSharp("PostSharp.Community.Packer.Weaver", "PackerTask")]
    public class PackerAttribute : Attribute
    {
        /// <summary>
        /// Returns true if all assemblies except excluded assemblies should be gathered. If this is false,
        /// then only assemblies specifically included should be gathered.
        /// </summary>
        public bool OptOut => IncludeAssemblies == null || IncludeAssemblies.Length == 0;
        public bool IncludeDebugSymbols { get;set; }
        
        /// <summary>
        /// If true, then the assemblies gathered into the main assembly won't be compressed. Default false ("do compress").
        /// </summary>
        public bool DisableCompression { get;set; }
        public bool DisableCleanup { get;set; }
        
        /// <summary>
        /// If true, then Packer will bootstrap itself in your assembly's module initializer and you don't need to
        /// call <see cref="PackerUtility.Initialize"/>. Default true ("load automatically").
        /// </summary>
        public bool LoadAtModuleInit { get;set; }
        public bool CreateTemporaryAssemblies { get; set;  }
        public bool IgnoreSatelliteAssemblies { get; set;  }
        public string[] IncludeAssemblies { get; set; }
        public string[] ExcludeAssemblies { get; set; }
        public string[] Unmanaged32Assemblies { get; set;}
        public string[] Unmanaged64Assemblies { get; set;}
        public string[] PreloadOrder { get; set; }
    }
}