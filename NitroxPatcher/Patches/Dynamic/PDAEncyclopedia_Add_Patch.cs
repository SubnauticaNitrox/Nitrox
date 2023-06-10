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

    public static void Prefix(string key, out bool __state)
    {
        __state = PDAEncyclopedia.ContainsEntry(key);
    }

    public static void Postfix(string key, bool verbose, bool postNotification, bool __state)
#endif
    {
        if (!Multiplayer.Main || !Multiplayer.Main.InitialSyncCompleted)
        {
            return;
        }

#if SUBNAUTICA
        // Is null when it's a duplicate call
        if (__result != null)
        {
            Resolve<IPacketSender>().Send(new PDAEncyclopediaEntryAdd(key, verbose));
#elif BELOWZERO
        if (!__state)
        {
            Resolve<IPacketSender>().Send(new PDAEncyclopediaEntryAdd(key, verbose, postNotification));
#endif
        }
    }
}
