using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using PostSharp.Sdk.CodeModel;

namespace PostSharp.Community.Packer.Weaver
{
    partial class JustOneExeTask
    {
        string resourcesHash;

        // void CalculateHash(AssemblyManifestDeclaration manifest)
        // {
        //     var data = manifest.Resources
        //         .OrderBy(r => r.Name)
        //         .Where(r => r.Name.StartsWith("costura"))
        //         .Select(r => r.ContentStreamProvider())
        //         .ToArray();
        //
        //     using (var md5 = MD5.Create())
        //     {
        //         var hashBytes = md5.ComputeHash((Stream)data);
        //
        //         var sb = new StringBuilder();
        //         for (var i = 0; i < hashBytes.Length; i++)
        //         {
        //             sb.Append(hashBytes[i].ToString("X2"));
        //         }
        //
        //         resourcesHash = sb.ToString();
        //     }
        // }
    }
}