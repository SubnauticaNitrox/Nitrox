using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours;
using NitroxModel_Subnautica.Packets;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic
{
    internal class CyclopsDecoyLaunchButton_OnClick_Patch : NitroxPatch, IDynamicPatch
    {
        public static readonly MethodInfo TARGET_METHOD = Reflect.Method((CyclopsDecoyLaunchButton t) => t.OnClick());

        public static void Postfix(CyclopsHornButton __instance)
        {
            NitroxId id = NitroxEntity.GetId(__instance.subRoot.gameObject);
            SendPacket<CyclopsDecoyLaunch>(new(id));
        }

        public override void Patch(Harmony harmony)
        {
            PatchPostfix(harmony, TARGET_METHOD);
        }
    }
}
