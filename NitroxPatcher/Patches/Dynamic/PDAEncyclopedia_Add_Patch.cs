using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class PDAEncyclopedia_Add_Patch : NitroxPatch, IDynamicPatch
{
    public override MethodInfo targetMethod { get; } = Reflect.Method(() => PDAEncyclopedia.Add(default, default, default));

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
}
