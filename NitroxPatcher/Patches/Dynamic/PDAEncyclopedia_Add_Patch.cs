using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public class PDAEncyclopedia_Add_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => PDAEncyclopedia.Add(default, default, default));

    public static void Postfix(string key, bool verbose, PDAEncyclopedia.EntryData __result)
    {
        if (!Multiplayer.Main || !Multiplayer.Main.InitialSyncCompleted)
        {
            return;
        }

        // Is null when it's a duplicate call
        if (__result != null)
        {
            Resolve<IPacketSender>().Send(new PDAEncyclopediaEntryAdd(key, verbose));
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, TARGET_METHOD);
    }
}
