using PostSharp.Sdk.CodeModel.Collections;
using System;

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
            var array = collection[name]?.Value.Value;

            // Even though only string[] array are legal in attribute properties, the collection provides them to us
            // as object arrays so we need to convert:
            if (array is object[] objects)
            {
                var converted = new string[objects.Length];
                Array.Copy(objects, converted, objects.Length);
                array = converted;
            }

            return (string[])(array ?? new string[0]);
        }
    }
}