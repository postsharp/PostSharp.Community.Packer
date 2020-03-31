using System;

namespace PostSharp.Community.Packer.Weaver
{
    partial class JustOneExeTask
    {
        void CallAttach(Packer.PackerAttribute config)
        {
            var initialized = FindInitializeCalls();

            if (config.LoadAtModuleInit)
            {
                AddModuleInitializerCall();
            }
            else if (!initialized)
            {
                throw new Exception(
                    "The add-in was not initialized. Make sure LoadAtModuleInit=true or call PackerUtility.Initialize().");
            }
        }

        bool FindInitializeCalls()
        {
            var found = false;

            // foreach (var type in this.Project.Module.Types)
            // {
            //     foreach (var method in type.Methods)
            //     {
            //         if (!method.HasBody)
            //         {
            //             continue;
            //         }
            //
            //         var instructions = method.Body.Instructions;
            //         for (var i = 0; i < instructions.Count; i++)
            //         {
            //             if (instructions[i].OpCode == OpCodes.Call &&
            //                 instructions[i].Operand is MethodReference callMethod &&
            //                 callMethod.FullName == "System.Void PackerUtility::Initialize()")
            //             {
            //                 found = true;
            //                 instructions[i] = Instruction.Create(OpCodes.Call, attachMethod);
            //             }
            //         }
            //     }
            // }

            return found;
        }

        void AddModuleInitializerCall()
        {
            // const MethodAttributes attributes = MethodAttributes.Private
            //                                     | MethodAttributes.HideBySig
            //                                     | MethodAttributes.Static
            //                                     | MethodAttributes.SpecialName
            //                                     | MethodAttributes.RTSpecialName;
            //
            // var moduleClass = ModuleDefinition.Types.FirstOrDefault(x => x.Name == "<Module>");
            // if (moduleClass == null)
            // {
            //     throw new WeavingException("Found no module class!");
            // }
            //
            // var cctor = moduleClass.Methods.FirstOrDefault(x => x.Name == ".cctor");
            // if (cctor == null)
            // {
            //     cctor = new MethodDefinition(".cctor", attributes, TypeSystem.VoidReference);
            //     cctor.Body.Instructions.Add(Instruction.Create(OpCodes.Ret));
            //     moduleClass.Methods.Add(cctor);
            // }
            //
            // cctor.Body.Instructions.Insert(0, Instruction.Create(OpCodes.Call, attachMethod));
        }
    }
}