using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.CodeWeaver;

namespace PostSharp.Community.Packer.Weaver
{
    internal class ReplacePackerUtilityInitializeAdvice : IAdvice
    {
        private readonly IMethod methodToCall;

        public ReplacePackerUtilityInitializeAdvice(IMethod methodToCall)
        {
            this.methodToCall = methodToCall;
        }

        public int Priority => 0;

        public bool ReplacedAtLeastOneCall { get; private set; }

        public bool RequiresWeave(WeavingContext context)
        {
            return true;
        }

        public void Weave(WeavingContext context, InstructionBlock block)
        {
            var iw = context.InstructionWriter;
            iw.AttachInstructionSequence(block.AddInstructionSequence());
            iw.EmitInstructionMethod(OpCodeNumber.Call, methodToCall);
            iw.DetachInstructionSequence();
            ReplacedAtLeastOneCall = true;
        }
    }
}