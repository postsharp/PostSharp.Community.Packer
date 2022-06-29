using PostSharp.Sdk.CodeModel;
using System.Collections.Generic;

namespace PostSharp.Community.Packer.Weaver
{
    public class Assets
    {
        public Assets(ModuleDeclaration module)
        {
            var dictionary = (INamedType)module.FindType(typeof(Dictionary<,>));
            DictionaryOfStringOfStringAdd = module.FindMethod(dictionary, "Add").GetGenericInstance(
                new GenericMap(module, new List<ITypeSignature>
                {
                    module.Cache.GetIntrinsic(IntrinsicType.String),
                    module.Cache.GetIntrinsic(IntrinsicType.String)
                }));

            var list = (INamedType)module.FindType(typeof(List<>));
            ListOfStringAdd = module.FindMethod(list, "Add").GetGenericInstance(
                new GenericMap(module, new List<ITypeSignature>
                {
                    module.Cache.GetIntrinsic(IntrinsicType.String)
                }));
        }

        public IMethod DictionaryOfStringOfStringAdd { get; }
        public IMethod ListOfStringAdd { get; }
    }
}