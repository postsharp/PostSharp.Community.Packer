using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.CodeWeaver;

namespace PostSharp.Community.Packer.Weaver
{
    internal class ReplacePackerUtilityInitializeAdvice : IAdvice
    {
        private IMethod _methodToCall;

        public ReplacePackerUtilityInitializeAdvice(IMethod methodToCall)
        {
            _methodToCall = methodToCall;
        }
        
        public bool RequiresWeave(WeavingContext context)
        {
            return true;
        }

        public void Weave(WeavingContext context, InstructionBlock block)
        {
            InstructionWriter iw = context.InstructionWriter;
            iw.AttachInstructionSequence(block.AddInstructionSequence());
            iw.EmitInstructionMethod(OpCodeNumber.Call, _methodToCall);
            iw.DetachInstructionSequence();
            ReplacedAtLeastOneCall = true;
        }

        public int Priority => 0;
        public bool ReplacedAtLeastOneCall { get; set; }
    }
}