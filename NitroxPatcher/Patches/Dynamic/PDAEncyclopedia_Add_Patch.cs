using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class PDAEncyclopedia_Add_Patch : NitroxPatch, IDynamicPatch
{
#if SUBNAUTICA
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => PDAEncyclopedia.Add(default(string), default(PDAEncyclopedia.Entry), default(bool)));

    public static void Postfix(string key, bool verbose, PDAEncyclopedia.EntryData __result)
#elif BELOWZERO
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method(() => PDAEncyclopedia.Add(default(string), default(PDAEncyclopedia.Entry), default(bool), default(bool)));

    public static void Postfix(string key, bool verbose, bool postNotification, PDAEncyclopedia.EntryData __result)
#endif
    {
        if (!Multiplayer.Main || !Multiplayer.Main.InitialSyncCompleted)
        {
            return;
        }

        // Is null when it's a duplicate call
        if (__result != null)
        {
#if SUBNAUTICA
            Resolve<IPacketSender>().Send(new PDAEncyclopediaEntryAdd(key, verbose));
#elif BELOWZERO
            Resolve<IPacketSender>().Send(new PDAEncyclopediaEntryAdd(key, verbose, postNotification));
#endif
        }
    }
}
