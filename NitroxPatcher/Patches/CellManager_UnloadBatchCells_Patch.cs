using Harmony;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using System;
using System.Reflection;

namespace NitroxPatcher.Patches
{
    public class CellManager_UnloadBatchCells_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(CellManager);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("UnloadBatchCells", BindingFlags.Public | BindingFlags.Instance);

        public static bool Prefix(CellManager __instance, Int3 index)
        {
            Validate.NotNull(TARGET_CLASS);
            Validate.NotNull(TARGET_METHOD);

            //todo instead use SaveBatchTmp Postfix (may not help but whatever)
            Multiplayer.Logic.Serializer.SendChunkSave(index);
            return true;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            this.PatchPrefix(harmony, TARGET_METHOD);
        }
    }
}