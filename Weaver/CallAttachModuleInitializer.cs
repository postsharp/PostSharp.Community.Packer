using PostSharp.Sdk.AspectInfrastructure;
using PostSharp.Sdk.CodeModel;
using System;
using System.Collections.Generic;

namespace PostSharp.Community.Packer.Weaver
{
    internal class CallAttachModuleInitializer : ITypeInitializationClient
    {
        private readonly IMethod attachMethod;

        public CallAttachModuleInitializer(IMethod attachMethod)
            => this.attachMethod = attachMethod ??
                                   throw new ArgumentNullException(
                                       nameof(attachMethod)
                                   );

        public IList<AspectDependency> Dependencies { get; } =
            Array.Empty<AspectDependency>();

        public bool IsBeforeFieldInitSupported
            => false;

        public bool IsCommutative
            => true;

        public void Emit(InstructionWriter writer, InstructionBlock block,
            TypeInitializationClientScopes scope)
        {
            if (writer == null) return;
            if (block == null) return;

            var sequence = block.AddInstructionSequence();
            writer.AttachInstructionSequence(sequence);
            writer.EmitInstructionMethod(OpCodeNumber.Call, attachMethod);
            writer.DetachInstructionSequence();
        }

        public string GetDisplayName()
            => "Calls the assembly loader's Attach method in the module initializer.";

        public bool HasEffect(string effect)
            => true;
    }
}