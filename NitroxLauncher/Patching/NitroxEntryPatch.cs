using System.Collections.Generic;
using System.IO;
using System.Linq;
using dnlib.DotNet;
using dnlib.DotNet.Emit;

namespace NitroxLauncher.Patching
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

        private readonly string subnauticaManagedPath;

        public NitroxEntryPatch(string subnauticaBasePath)
        {
            subnauticaManagedPath = Path.Combine(subnauticaBasePath, "Subnautica_Data", "Managed");
        }

        public bool IsApplied => IsPatchApplied();

        private bool IsPatchApplied()
        {
            string gameInputPath = Path.Combine(subnauticaManagedPath, GAME_ASSEMBLY_NAME);
            string nitroxPatcherPath = Path.Combine(subnauticaManagedPath, NITROX_ASSEMBLY_NAME);

            using (ModuleDefMD module = ModuleDefMD.Load(gameInputPath))
            {
                TypeDef gameInputType = module.GetTypes().First(x => x.FullName == GAME_INPUT_TYPE_NAME);
                MethodDef awakeMethod = gameInputType.Methods.First(x => x.Name == GAME_INPUT_METHOD_NAME);

                return awakeMethod.Body.Instructions.Any(instruction => instruction.Operand?.ToString() == NITROX_EXECUTE_INSTRUCTION);
            }
        }

        public void Apply()
        {
            string assemblyCSharp = Path.Combine(subnauticaManagedPath, GAME_ASSEMBLY_NAME);
            string nitroxPatcherPath = Path.Combine(subnauticaManagedPath, NITROX_ASSEMBLY_NAME);
            string modifiedAssemblyCSharp = Path.Combine(subnauticaManagedPath, GAME_ASSEMBLY_MODIFIED_NAME);

            if(File.Exists(modifiedAssemblyCSharp))
            {
                File.Delete(modifiedAssemblyCSharp);
            }

            using (ModuleDefMD module = ModuleDefMD.Load(assemblyCSharp))
            using (ModuleDefMD nitroxPatcherAssembly = ModuleDefMD.Load(nitroxPatcherPath))
            {
                TypeDef nitroxMainDefinition = nitroxPatcherAssembly.GetTypes().FirstOrDefault(x => x.Name == NITROX_ENTRY_TYPE_NAME);
                MethodDef executeMethodDefinition = nitroxMainDefinition.Methods.FirstOrDefault(x => x.Name == NITROX_ENTRY_METHOD_NAME);

                MemberRef executeMethodReference = module.Import(executeMethodDefinition);

                TypeDef gameInputType = module.GetTypes().First(x => x.FullName == GAME_INPUT_TYPE_NAME);
                MethodDef awakeMethod = gameInputType.Methods.First(x => x.Name == GAME_INPUT_METHOD_NAME);

                Instruction callNitroxExecuteInstruction = OpCodes.Call.ToInstruction(executeMethodReference);

                awakeMethod.Body.Instructions.Insert(0, callNitroxExecuteInstruction);
                module.Write(modifiedAssemblyCSharp);
            }

            File.SetAttributes(assemblyCSharp, System.IO.FileAttributes.Normal);
            File.Delete(assemblyCSharp);
            File.Move(modifiedAssemblyCSharp, assemblyCSharp);
        }

        public void Remove()
        {
            string assemblyCSharp = Path.Combine(subnauticaManagedPath, GAME_ASSEMBLY_NAME);
            string modifiedAssemblyCSharp = Path.Combine(subnauticaManagedPath, GAME_ASSEMBLY_MODIFIED_NAME);

            using (ModuleDefMD module = ModuleDefMD.Load(assemblyCSharp))
            {
                TypeDef gameInputType = module.GetTypes().First(x => x.FullName == GAME_INPUT_TYPE_NAME);
                MethodDef awakeMethod = gameInputType.Methods.First(x => x.Name == GAME_INPUT_METHOD_NAME);

                IList<Instruction> methodInstructions = awakeMethod.Body.Instructions;
                int nitroxExecuteInstructionIndex = FindNitroxExecuteInstructionIndex(methodInstructions);

                if(nitroxExecuteInstructionIndex != -1)
                {
                    methodInstructions.RemoveAt(nitroxExecuteInstructionIndex);
                    module.Write(modifiedAssemblyCSharp);
                    File.SetAttributes(assemblyCSharp, System.IO.FileAttributes.Normal);
                }
            }

            if(File.Exists(modifiedAssemblyCSharp))
            {
                File.Delete(assemblyCSharp);
                File.Move(modifiedAssemblyCSharp, assemblyCSharp);
            }
        }

        private static int FindNitroxExecuteInstructionIndex(IList<Instruction> methodInstructions)
        {
            for (int instructionIndex = 0; instructionIndex < methodInstructions.Count; instructionIndex++)
            {
                string instruction = methodInstructions[instructionIndex].Operand?.ToString();

                if (instruction == NITROX_EXECUTE_INSTRUCTION)
                {
                    return instructionIndex;
                }
            }

            return -1;
        }
    }
}
