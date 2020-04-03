using System;
using System.Collections.Generic;
using PostSharp.Sdk.AspectInfrastructure;
using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.Collections;

namespace PostSharp.Community.Packer.Weaver
{
    internal class CallAttachModuleInitializer : ITypeInitializationClient
    {
        private readonly IMethod attachMethod;

        public CallAttachModuleInitializer(IMethod attachMethod)
        {
            this.attachMethod = attachMethod;
        }
        
        public bool HasEffect(string effect) => false;

        public string GetDisplayName() => "Calls the assembly loader's Attach method in the module initializer.";

        public IList<AspectDependency> Dependencies { get; } = new AspectDependency[0];
        public bool IsCommutative => true;
        public void Emit(InstructionWriter writer, InstructionBlock block, TypeInitializationClientScopes scope)
        {
            writer.EmitInstructionMethod(OpCodeNumber.Call, attachMethod);
        }

        public bool IsBeforeFieldInitSupported => false;
    }
}