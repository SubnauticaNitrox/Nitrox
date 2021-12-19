using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using NitroxModel.Platforms.OS.Shared;
using FileAttributes = System.IO.FileAttributes;

namespace NitroxLauncher.Models.Patching
{
    internal sealed class NitroxEntryPatch
    {
        public const string GAME_ASSEMBLY_NAME = "Assembly-CSharp.dll";
        public const string NITROX_ASSEMBLY_NAME = "NitroxPatcher.dll";
        public const string GAME_ASSEMBLY_MODIFIED_NAME = "Assembly-CSharp-Nitrox.dll";

        private const string NITROX_ENTRY_TYPE_NAME = "Main";
        private const string NITROX_ENTRY_METHOD_NAME = "Execute";

        private const string GAME_INPUT_TYPE_NAME = "GameInput";
        private const string GAME_INPUT_METHOD_NAME = "Awake";

        private const string NITROX_EXECUTE_INSTRUCTION = "System.Void NitroxPatcher.Main::Execute()";

        private readonly Func<string> subnauticaBasePathFunc;
        private string subnauticaManagedPath => Path.Combine(subnauticaBasePathFunc(), "Subnautica_Data", "Managed");

        public bool IsApplied => IsPatchApplied();

        public NitroxEntryPatch(Func<string> subnauticaBasePathFunc)
        {
            this.subnauticaBasePathFunc = subnauticaBasePathFunc;
        }

        public void Apply()
        {
            string assemblyCSharp = Path.Combine(subnauticaManagedPath, GAME_ASSEMBLY_NAME);
            string nitroxPatcherPath = Path.Combine(subnauticaManagedPath, NITROX_ASSEMBLY_NAME);
            string modifiedAssemblyCSharp = Path.Combine(subnauticaManagedPath, GAME_ASSEMBLY_MODIFIED_NAME);

            if (File.Exists(modifiedAssemblyCSharp))
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

            // The assembly might be used by other code or some other program might work in it. Retry to be on the safe side.
            Exception error = RetryWait(() => File.Delete(assemblyCSharp), 100, 5);
            if (error != null)
            {
                throw error;
            }
            FileSystem.Instance.ReplaceFile(modifiedAssemblyCSharp, assemblyCSharp);
        }

        private Exception RetryWait(Action action, int interval, int retries = 0)
        {
            Exception lastException = null;
            while (retries >= 0)
            {
                try
                {
                    retries--;
                    action();
                    return null;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    Task.Delay(interval).Wait();
                }
            }
            return lastException;
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

                if (nitroxExecuteInstructionIndex == -1)
                {
                    return;
                }

                methodInstructions.RemoveAt(nitroxExecuteInstructionIndex);
                module.Write(modifiedAssemblyCSharp);

                File.SetAttributes(assemblyCSharp, FileAttributes.Normal);
            }

            FileSystem.Instance.ReplaceFile(modifiedAssemblyCSharp, assemblyCSharp);
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

        private bool IsPatchApplied()
        {
            string gameInputPath = Path.Combine(subnauticaManagedPath, GAME_ASSEMBLY_NAME);

            using (ModuleDefMD module = ModuleDefMD.Load(gameInputPath))
            {
                TypeDef gameInputType = module.GetTypes().First(x => x.FullName == GAME_INPUT_TYPE_NAME);
                MethodDef awakeMethod = gameInputType.Methods.First(x => x.Name == GAME_INPUT_METHOD_NAME);

                return awakeMethod.Body.Instructions.Any(instruction => instruction.Operand?.ToString() == NITROX_EXECUTE_INSTRUCTION);
            }
        }
    }
}
