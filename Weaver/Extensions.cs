using System.Collections.ObjectModel;
using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.CodeModel.Collections;

namespace PostSharp.Community.Packer.Weaver
{
    public static class Extensions
    {
        public static bool GetSafeBool(this MemberValuePairCollection collection, string name, bool defaultValue)
        {
            return (bool)(collection[name]?.Value.Value ?? defaultValue);
        }
        public static string[] GetSafeStringArray(this MemberValuePairCollection collection, string name)
        {
            return (string[]) (collection[name]?.Value.Value ?? new string[0]);
        }
    }
}