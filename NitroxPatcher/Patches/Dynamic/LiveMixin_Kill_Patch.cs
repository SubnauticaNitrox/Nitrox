using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class LiveMixin_Kill_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((LiveMixin t) => t.Kill(default));

    public static void Postfix(LiveMixin __instance)
    {
        if (!Multiplayer.Main || !Multiplayer.Main.InitialSyncCompleted)
        {
            return;
        }

        // We don't broadcast if we don't have objectId or if the object is whitelisted,
        // in which case kill broadcast is managed differently
        if (!__instance.TryGetNitroxId(out NitroxId objectId) ||
            Resolve<LiveMixinManager>().IsWhitelistedUpdateType(__instance))
        {
            return;
        }
        
        // Some objects don't have destroyOnDeath but we still need to broadcast the death
        // (because the destruction is managed by another script)
        if (__instance.destroyOnDeath || Resolve<LiveMixinManager>().ShouldBroadcastDeath(__instance))
        {
            Resolve<IPacketSender>().Send(new EntityDestroyed(objectId));
        }
    }
}
