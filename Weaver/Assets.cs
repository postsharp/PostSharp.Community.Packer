using System.Collections.Generic;
using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.Collections;

namespace PostSharp.Community.Packer.Weaver
{
    public class Assets
    {
        public IMethod DictionaryOfStringOfStringAdd { get; }
        public IMethod ListOfStringAdd { get; }

        public Assets(ModuleDeclaration module)
        {
            INamedType dictionary = (INamedType)module.FindType(typeof(Dictionary<,>));
            DictionaryOfStringOfStringAdd = module.FindMethod(dictionary, "Add").GetGenericInstance(
                new GenericMap(module, new List<ITypeSignature>
                {
                    module.Cache.GetIntrinsic(IntrinsicType.String),
                    module.Cache.GetIntrinsic(IntrinsicType.String)
                }));
            
            INamedType list = (INamedType)module.FindType(typeof(List<>));
            ListOfStringAdd = module.FindMethod(list, "Add").GetGenericInstance(
                new GenericMap(module, new List<ITypeSignature>
                {
                    module.Cache.GetIntrinsic(IntrinsicType.String)
                }));
        }
    }
}