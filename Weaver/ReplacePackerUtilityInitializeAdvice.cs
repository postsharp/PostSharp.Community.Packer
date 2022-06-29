using PostSharp.Sdk.CodeModel;
using PostSharp.Sdk.CodeWeaver;
using System;

namespace PostSharp.Community.Packer.Weaver
{
    public class ReplacePackerUtilityInitializeAdvice : IAdvice
    {
        /// <summary>
        /// Reference to an instance of an object that implements the
        /// <see cref="T:PostSharp.Sdk.CodeModel.IMethod" /> interface
        /// that plays the role of the method to call.
        /// </summary>
        private readonly IMethod methodToCall;

        /// <summary>
        /// Constructs a new instance of
        /// <see
        ///     cref="T:PostSharp.Community.Packer.Weaver.ReplacePackerUtilityInitializeAdvice" />
        /// and returns a reference to it.
        /// </summary>
        public ReplacePackerUtilityInitializeAdvice(IMethod methodToCall)
            => this.methodToCall = methodToCall ??
                                   throw new ArgumentNullException(
                                       nameof(methodToCall)
                                   );

        public int Priority
            => 0;

        public bool ReplacedAtLeastOneCall { get; private set; }

        public bool RequiresWeave(WeavingContext context)
            => true;

        public void Weave(WeavingContext context, InstructionBlock block)
        {
            try
            {
                if (context == null) return;
                if (context.InstructionWriter == null) return;
                if (block == null) return;

                using var iw = context.InstructionWriter;
                iw.AttachInstructionSequence(block.AddInstructionSequence());
                iw.EmitInstructionMethod(OpCodeNumber.Call, methodToCall);
                iw.DetachInstructionSequence();

                ReplacedAtLeastOneCall = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex);
            }
        }
    }
}