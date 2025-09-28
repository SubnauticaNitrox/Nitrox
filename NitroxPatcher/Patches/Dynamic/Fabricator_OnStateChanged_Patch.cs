using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Fabricator_OnStateChanged_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Fabricator t) => t.OnStateChanged(default));

    public static void Postfix(Fabricator __instance, bool crafting)
    {
        Log.Info($"[Fabricator_OnStateChanged_Patch] crafting: {crafting}");

        if (!__instance.TryGetIdOrWarn(out NitroxId id))
        {
            Log.Info("[Fabricator_OnStateChanged_Patch] Failed to get NitroxId");
            return;
        }

        LocalPlayer player = Resolve<LocalPlayer>();
        Resolve<IPacketSender>().Send(new FabricatorStateChanged(id, player.PlayerId.Value, crafting));
    }

}
