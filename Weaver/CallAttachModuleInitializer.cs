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
        
        public bool HasEffect(string effect) => true;

        public string GetDisplayName() => "Calls the assembly loader's Attach method in the module initializer.";

        public IList<AspectDependency> Dependencies { get; } = new AspectDependency[0];
        public bool IsCommutative => true;
        public void Emit(InstructionWriter writer, InstructionBlock block, TypeInitializationClientScopes scope)
        {
            InstructionSequence sequence = block.AddInstructionSequence();
            writer.AttachInstructionSequence(sequence);
            writer.EmitInstructionMethod(OpCodeNumber.Call, attachMethod);
            writer.DetachInstructionSequence();
        }

        public bool IsBeforeFieldInitSupported => false;
    }
}