using System;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using dnlib.DotNet;
using dnlib.DotNet.Emit;
using NitroxLauncher.Properties;
using FileAttributes = System.IO.FileAttributes;

namespace NitroxLauncher.Patching
{
    internal class NitroxEntryPatch
    {
        public const string GAME_ASSEMBLY_NAME = "Assembly-CSharp.dll";
        public const string OLD_GAME_ASSEMBLY_NAME = "Assembly-CSharp.dll.old";
        public const string NITROX_ASSEMBLY_NAME = "NitroxPatcher.dll";
        public const string GAME_ASSEMBLY_MODIFIED_NAME = "Assembly-CSharp-Nitrox.dll";

        private const string NITROX_ENTRY_TYPE_NAME = "Main";
        private const string NITROX_ENTRY_METHOD_NAME = "Execute";

        private const string GAME_INPUT_TYPE_NAME = "GameInput";
        private const string GAME_INPUT_METHOD_NAME = "Awake";

        private const string NITROX_EXECUTE_INSTRUCTION = "System.Void NitroxPatcher.Main::Execute()";

        private readonly string subnauticaManagedPath;

        private readonly SHA256 sha256;

        public bool IsApplied => IsPatchApplied();

        public NitroxEntryPatch(string subnauticaBasePath)
        {
            subnauticaManagedPath = Path.Combine(subnauticaBasePath, "Subnautica_Data", "Managed");
            sha256 = SHA256.Create();
        }

        private string GetHash(string file)
        {
            using (FileStream stream = File.OpenRead(file))
            {
                // we check if it's already the modified assembly
                return BitConverter.ToString(sha256.ComputeHash(stream));
            }
        }

        public void Apply()
        {
            string assemblyCSharp = Path.Combine(subnauticaManagedPath, GAME_ASSEMBLY_NAME);
            string defaultAssemblyCSharp = Path.Combine(subnauticaManagedPath, OLD_GAME_ASSEMBLY_NAME);
            string nitroxPatcherPath = Path.Combine(subnauticaManagedPath, NITROX_ASSEMBLY_NAME);
            string modifiedAssemblyCSharp = Path.Combine(subnauticaManagedPath, GAME_ASSEMBLY_MODIFIED_NAME);

            if (GetHash(assemblyCSharp) != Settings.Default.AssemblyCSharpModifiedHash)
            {
                // not the same hash, we need to modify it

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

                Settings.Default.AssemblyCSharpModifiedHash = GetHash(modifiedAssemblyCSharp);
                Settings.Default.Save();

                File.SetAttributes(assemblyCSharp, FileAttributes.Normal);
                // keep a copy of the original
                if (File.Exists(defaultAssemblyCSharp))
                {
                    File.Delete(defaultAssemblyCSharp);
                }
                File.Move(assemblyCSharp, defaultAssemblyCSharp);
                File.Move(modifiedAssemblyCSharp, assemblyCSharp);
            }
        }

        public void Remove()
        {
            string assemblyCSharp = Path.Combine(subnauticaManagedPath, GAME_ASSEMBLY_NAME);
            string defaultAssemblyCSharp = Path.Combine(subnauticaManagedPath, OLD_GAME_ASSEMBLY_NAME);

            if (GetHash(assemblyCSharp) == Settings.Default.AssemblyCSharpModifiedHash && File.Exists(defaultAssemblyCSharp))
            {
                File.Delete(assemblyCSharp);
                File.Move(defaultAssemblyCSharp, assemblyCSharp);

                Settings.Default.AssemblyCSharpModifiedHash = "";
                Settings.Default.Save();
            }
        }

        private bool IsPatchApplied()
        {
            string assemblyCSharp = Path.Combine(subnauticaManagedPath, GAME_ASSEMBLY_NAME);
            string nitroxPatcherPath = Path.Combine(subnauticaManagedPath, NITROX_ASSEMBLY_NAME);

            return GetHash(assemblyCSharp) == Settings.Default.AssemblyCSharpModifiedHash;
        }
    }
}
