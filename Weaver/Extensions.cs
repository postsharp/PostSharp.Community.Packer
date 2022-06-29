using PostSharp.Sdk.CodeModel.Collections;
using System;
using System.Linq;

namespace PostSharp.Community.Packer.Weaver
{
    public static class Extensions
    {
        public static bool GetSafeBool(
            this MemberValuePairCollection collection, string name,
            bool defaultValue)
        {
            var result = defaultValue;

            if (collection == null || collection.Count == 0) return result;
            if (string.IsNullOrWhiteSpace(name)) return result;

            try
            {
                result = (bool)(collection[name]
                                ?.Value.Value ?? defaultValue);
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }

            return result;
        }

        public static string[] GetSafeStringArray(
            this MemberValuePairCollection collection, string name)
        {
            var result = Array.Empty<string>();

            try
            {
                if (collection == null || !collection.Any()) return result;
                if (string.IsNullOrWhiteSpace(name)) return result;

                var element = collection[name]
                              ?.Value.Value;

                // Even though only string[] array are legal in
                // attribute properties, the collection provides them to us
                // as object arrays so we need to convert:
                if (!(element is object[] objects))
                    return (string[])(element ?? Array.Empty<string>());
                if (!objects.Any()) return Array.Empty<string>();
                result = new string[objects.Length];
                Array.Copy(objects, result, objects.Length);

                if (result == null)
                    result = Array.Empty<string>();
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);

                result = Array.Empty<string>();
            }

            return result;
        }
    }
}