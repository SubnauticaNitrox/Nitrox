using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using NitroxModel.Logger;
using NitroxModel.Platforms.OS.Shared;

namespace Nitrox.Launcher.Models.Utils;

public static class NitroxEntryPatch
{
    public const string GAME_ASSEMBLY_NAME = "Assembly-CSharp.dll";
    public const string NITROX_ASSEMBLY_NAME = "NitroxPatcher.dll";
    public const string GAME_ASSEMBLY_MODIFIED_NAME = "Assembly-CSharp-Nitrox.dll";

    private const string NITROX_ENTRY_TYPE_NAME = "Main";
    private const string NITROX_ENTRY_METHOD_NAME = "Execute";

    private const string GAME_INPUT_TYPE_NAME = "GameInput";
    private const string GAME_INPUT_METHOD_NAME = "Awake";

    private const string NITROX_EXECUTE_INSTRUCTION = "System.Void NitroxPatcher.Main::Execute()";

    /// <summary>
    /// Inject Nitrox entry point into Subnautica's Assembly-CSharp.dll
    /// </summary>
    public static void Apply(string subnauticaBasePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(subnauticaBasePath, nameof(subnauticaBasePath));

        Log.Debug("Adding Nitrox entry point to Subnautica");

        string subnauticaManagedPath = Path.Combine(subnauticaBasePath, GameInfo.Subnautica.DataFolder, "Managed");
        string assemblyCSharp = Path.Combine(subnauticaManagedPath, GAME_ASSEMBLY_NAME);
        string nitroxPatcherPath = Path.Combine(subnauticaManagedPath, NITROX_ASSEMBLY_NAME);
        string modifiedAssemblyCSharp = Path.Combine(subnauticaManagedPath, GAME_ASSEMBLY_MODIFIED_NAME);

        if (File.Exists(modifiedAssemblyCSharp))
        {
            // Avoid the case where AssemblyCSharp.dll get wiped and the only file left is AssemblyCSharp-Nitrox.dll
            if (!File.Exists(assemblyCSharp))
            {
                Log.Error($"Invalid state, {GAME_ASSEMBLY_NAME} not found, but {GAME_ASSEMBLY_MODIFIED_NAME} exists. Please verify your installation.");
                FileSystem.Instance.ReplaceFile(modifiedAssemblyCSharp, assemblyCSharp);
            }
            else
            {
                Log.Debug($"{GAME_ASSEMBLY_MODIFIED_NAME} already exists, removing it");
                Exception copyError = RetryWait(() => File.Delete(modifiedAssemblyCSharp), 100, 5);
                if (copyError != null)
                {
                    throw copyError;
                }
            }
        }

        /*
          	private void Awake()
	        {
		        NitroxPatcher.Main.Execute(); <----------- Insert this line inside subnautica's code
		        if (GameInput.instance != null)
		        {
			        global::UnityEngine.Object.Destroy(base.gameObject);
			        return;
		        }
		        GameInput.instance = this;
		        GameInput.instance.Initialize();
		        for (int i = 0; i < GameInput.numDevices; i++)
		        {
			        GameInput.SetupDefaultBindings((GameInput.Device)i);
		        }
		        DevConsole.RegisterConsoleCommand(this, "debuginput", false, false);
	        }
        */
        // TODO: Find a better way to inject Nitrox entrypoint instead of using file swapping
        using (ModuleDefMD module = ModuleDefMD.Load(assemblyCSharp))
        using (ModuleDefMD nitroxPatcherAssembly = ModuleDefMD.Load(nitroxPatcherPath))
        {
            TypeDef nitroxMainDefinition = nitroxPatcherAssembly.GetTypes().FirstOrDefault(x => x.Name == NITROX_ENTRY_TYPE_NAME);
            MethodDef executeMethodDefinition = nitroxMainDefinition.Methods.FirstOrDefault(x => x.Name == NITROX_ENTRY_METHOD_NAME);

            MemberRef executeMethodReference = module.Import(executeMethodDefinition);

            TypeDef gameInputType = module.GetTypes().First(x => x.FullName == GAME_INPUT_TYPE_NAME);
            MethodDef awakeMethod = gameInputType.Methods.First(x => x.Name == GAME_INPUT_METHOD_NAME);

            Instruction callNitroxExecuteInstruction = OpCodes.Call.ToInstruction(executeMethodReference);

            if (awakeMethod.Body.Instructions[0].Operand == callNitroxExecuteInstruction.Operand)
            {
                Log.Warn("Nitrox entry point already patched.");
                return;
            }

            awakeMethod.Body.Instructions.Insert(0, callNitroxExecuteInstruction);
            module.Write(modifiedAssemblyCSharp);

            Log.Debug($"Writing assembly to {GAME_ASSEMBLY_MODIFIED_NAME}");
            File.SetAttributes(assemblyCSharp, System.IO.FileAttributes.Normal);
        }

        // The assembly might be used by other code or some other program might work in it. Retry to be on the safe side.
        Log.Debug($"Deleting {GAME_ASSEMBLY_NAME}");
        Exception error = RetryWait(() => File.Delete(assemblyCSharp), 100, 5);
        if (error != null)
        {
            throw error;
        }

        FileSystem.Instance.ReplaceFile(modifiedAssemblyCSharp, assemblyCSharp);
        Log.Debug("Added Nitrox entry point to Subnautica");
    }

    /// <summary>
    /// Remote Nitrox entry point from Subnautica's Assembly-CSharp.dll
    /// </summary>
    public static void Remove(string subnauticaBasePath)
    {
        ArgumentException.ThrowIfNullOrEmpty(subnauticaBasePath, nameof(subnauticaBasePath));

        Log.Debug("Removing Nitrox entry point from Subnautica");

        string subnauticaManagedPath = Path.Combine(subnauticaBasePath, GameInfo.Subnautica.DataFolder, "Managed");
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
                Log.Debug($"Nitrox entry point not found in {GAME_INPUT_TYPE_NAME}:{GAME_INPUT_METHOD_NAME}");
                return;
            }

            methodInstructions.RemoveAt(nitroxExecuteInstructionIndex);
            module.Write(modifiedAssemblyCSharp);

            File.SetAttributes(assemblyCSharp, System.IO.FileAttributes.Normal);
        }

        FileSystem.Instance.ReplaceFile(modifiedAssemblyCSharp, assemblyCSharp);
        Log.Debug("Removed Nitrox entry point from Subnautica");
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

    private static Exception RetryWait(Action action, int interval, int retries = 0)
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

    public static bool IsPatchApplied(string subnauticaBasePath)
    {
        string subnauticaManagedPath = Path.Combine(subnauticaBasePath, GameInfo.Subnautica.DataFolder, "Managed");
        string gameInputPath = Path.Combine(subnauticaManagedPath, GAME_ASSEMBLY_NAME);

        using (ModuleDefMD module = ModuleDefMD.Load(gameInputPath))
        {
            TypeDef gameInputType = module.GetTypes().First(x => x.FullName == GAME_INPUT_TYPE_NAME);
            MethodDef awakeMethod = gameInputType.Methods.First(x => x.Name == GAME_INPUT_METHOD_NAME);

            return awakeMethod.Body.Instructions[0]?.ToString() == NITROX_EXECUTE_INSTRUCTION;
        }
    }
}
