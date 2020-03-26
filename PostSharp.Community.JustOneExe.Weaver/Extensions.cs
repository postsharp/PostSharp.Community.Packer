using System.Collections.ObjectModel;
using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.CodeModel.Collections;

namespace PostSharp.Community.JustOneExe.Weaver
{
    public static class Extensions
    {
        public static bool GetSafeBool(this MemberValuePairCollection collection, string name)
        {
            return (bool)(collection[name]?.Value.Value ?? false);
        }
        public static string[] GetSafeStringArray(this MemberValuePairCollection collection, string name)
        {
            return (string[]) (collection[name]?.Value.Value ?? new string[0]);
        }
        
        // public static Collection<TypeReference> GetGenericInstanceArguments(this TypeReference type) => ((GenericInstanceType)type).GenericArguments;
        //
        // public static MethodReference MakeHostInstanceGeneric(this MethodReference self, params TypeReference[] args)
        // {
        //     var reference = new MethodReference(
        //         self.Name,
        //         self.ReturnType,
        //         self.DeclaringType.MakeGenericInstanceType(args))
        //     {
        //         HasThis = self.HasThis,
        //         ExplicitThis = self.ExplicitThis,
        //         CallingConvention = self.CallingConvention
        //     };
        //
        //     foreach (var parameter in self.Parameters)
        //     {
        //         reference.Parameters.Add(new ParameterDefinition(parameter.ParameterType));
        //     }
        //
        //     foreach (var genericParam in self.GenericParameters)
        //     {
        //         reference.GenericParameters.Add(new GenericParameter(genericParam.Name, reference));
        //     }
        //
        //     return reference;
        // }
        //
        // public static void InsertBefore(this Collection<Instruction> instructions, int index, params Instruction[] newInstructions)
        // {
        //     foreach (var item in newInstructions)
        //     {
        //         instructions.Insert(index, item);
        //         index++;
        //     }
        // }
        //
        // public static byte[] FixedGetResourceData(this EmbeddedResource resource)
        // {
        //     // There's a bug in Mono.Cecil so when you access a resources data
        //     // the stream is not reset after use.
        //     var data = resource.GetResourceData();
        //     resource.GetResourceStream().Position = 0;
        //     return data;
        // }
    }
}