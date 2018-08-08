using System.Collections.Generic;
using System.IO;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace InstallerActions.Patches
{
    internal class NitroxEntryPatch
    {
        public const string GAME_ASSEMBLY_NAME = "Assembly-CSharp.dll";
        public const string NITROX_ASSEMBLY_NAME = "NitroxPatcher.dll";
        public const string GAME_ASSEMBLY_BACKUP_NAME = "Assembly-CSharp.dll.bak";
        public const string GAME_ASSEMBLY_MODIFIED_NAME = "Assembly-CSharp-Nitrox.dll";

        private const string NITROX_ENTRY_TYPE_NAME = "Main";
        private const string NITROX_ENTRY_METHOD_NAME = "Execute";

        private const string GAME_INPUT_TYPE_NAME = "GameInput";
        private const string GAME_INPUT_METHOD_NAME = "Awake";

        private const string NITROX_EXECUTE_INSTRUCTION = "System.Void NitroxPatcher.Main::Execute()";

        private readonly string subnauticaBasePath;

        public NitroxEntryPatch(string subnauticaBasePath)
        {
            this.subnauticaBasePath = subnauticaBasePath;
        }

        public bool IsApplied => IsPatchApplied();

        private bool IsPatchApplied()
        {
            string gameInputPath = subnauticaBasePath + GAME_ASSEMBLY_NAME;
            string nitroxPatcherPath = subnauticaBasePath + NITROX_ASSEMBLY_NAME;

            using (ModuleDefMD module = ModuleDefMD.Load(gameInputPath))
            {
                TypeDef gameInputType = module.GetTypes().First(x => x.FullName == GAME_INPUT_TYPE_NAME);
                MethodDef awakeMethod = gameInputType.Methods.First(x => x.Name == GAME_INPUT_METHOD_NAME);

                return awakeMethod.Body.Instructions.Any(instruction => instruction.Operand?.ToString() == NITROX_EXECUTE_INSTRUCTION);
            }
        }

        public void Apply()
        {
            string gameInputPath = subnauticaBasePath + GAME_ASSEMBLY_NAME;
            string nitroxPatcherPath = subnauticaBasePath + NITROX_ASSEMBLY_NAME;
            string modifiedAssemblyPath = subnauticaBasePath + GAME_ASSEMBLY_MODIFIED_NAME;

            using (ModuleDefMD module = ModuleDefMD.Load(gameInputPath))
            using (ModuleDefMD nitroxPatcherAssembly = ModuleDefMD.Load(nitroxPatcherPath))
            {
                TypeDef nitroxMainDefinition = nitroxPatcherAssembly.GetTypes().FirstOrDefault(x => x.Name == NITROX_ENTRY_TYPE_NAME);
                MethodDef executeMethodDefinition = nitroxMainDefinition.Methods.FirstOrDefault(x => x.Name == NITROX_ENTRY_METHOD_NAME);

                MemberRef executeMethodReference = module.Import(executeMethodDefinition);

                TypeDef gameInputType = module.GetTypes().First(x => x.FullName == GAME_INPUT_TYPE_NAME);
                MethodDef awakeMethod = gameInputType.Methods.First(x => x.Name == GAME_INPUT_METHOD_NAME);

                Instruction callNitroxExecuteInstruction = OpCodes.Call.ToInstruction(executeMethodReference);

                awakeMethod.Body.Instructions.Insert(0, callNitroxExecuteInstruction);
                module.Write(modifiedAssemblyPath);
            }

            string backuupAssemblyPath = subnauticaBasePath + GAME_ASSEMBLY_BACKUP_NAME;

            File.Replace(modifiedAssemblyPath, gameInputPath, backuupAssemblyPath);
        }

        public void Remove()
        {
            string gameInputPath = subnauticaBasePath + GAME_ASSEMBLY_NAME;
            string modifiedAssemblyPath = subnauticaBasePath + GAME_ASSEMBLY_MODIFIED_NAME;

            using (ModuleDefMD module = ModuleDefMD.Load(gameInputPath))
            {
                TypeDef gameInputType = module.GetTypes().First(x => x.FullName == GAME_INPUT_TYPE_NAME);
                MethodDef awakeMethod = gameInputType.Methods.First(x => x.Name == GAME_INPUT_METHOD_NAME);

                IList<Instruction> methodInstructions = awakeMethod.Body.Instructions;
                int nitroxExecuteInstructionIndex = FindNitroxExecuteInstructionIndex(methodInstructions);

                methodInstructions.RemoveAt(nitroxExecuteInstructionIndex);
                module.Write(modifiedAssemblyPath);
            }

            if (File.Exists(gameInputPath))
            {
                File.Delete(gameInputPath);
            }
            
            File.Move(modifiedAssemblyPath, gameInputPath);
        }

        private static int FindNitroxExecuteInstructionIndex(IList<Instruction> methodInstructions)
        {
            int nitroxExecuteInstructionIndex = 0;

            for (int instructionIndex = 0; instructionIndex < methodInstructions.Count; instructionIndex++)
            {
                string instruction = methodInstructions[instructionIndex].Operand?.ToString();

                if (instruction == NITROX_EXECUTE_INSTRUCTION)
                {
                    nitroxExecuteInstructionIndex = instructionIndex;
                }
            }

            return nitroxExecuteInstructionIndex;
        }
    }
}
