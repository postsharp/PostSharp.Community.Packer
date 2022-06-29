using PostSharp.Sdk.AspectInfrastructure;
using PostSharp.Sdk.CodeModel;
using System.Collections.Generic;

namespace PostSharp.Community.Packer.Weaver
{
    internal class CallAttachModuleInitializer : ITypeInitializationClient
    {
        private readonly IMethod attachMethod;

        public CallAttachModuleInitializer(IMethod attachMethod)
        {
            this.attachMethod = attachMethod;
        }

        public IList<AspectDependency> Dependencies { get; } = new AspectDependency[0];

        public bool IsBeforeFieldInitSupported => false;

        public bool IsCommutative => true;

        public void Emit(InstructionWriter writer, InstructionBlock block, TypeInitializationClientScopes scope)
        {
            var sequence = block.AddInstructionSequence();
            writer.AttachInstructionSequence(sequence);
            writer.EmitInstructionMethod(OpCodeNumber.Call, attachMethod);
            writer.DetachInstructionSequence();
        }

        public string GetDisplayName() => "Calls the assembly loader's Attach method in the module initializer.";

        public bool HasEffect(string effect) => true;
    }
}