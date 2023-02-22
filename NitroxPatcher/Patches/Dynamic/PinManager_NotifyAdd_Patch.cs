using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public class PinManager_NotifyAdd_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((PinManager t) => t.NotifyAdd(default));

    public static void Prefix(TechType techType)
    {
        if (!Multiplayer.Main || !Multiplayer.Main.InitialSyncCompleted)
        {
            return;
        }
        Resolve<IPacketSender>().Send(new RecipePinned((int)techType, true));
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
