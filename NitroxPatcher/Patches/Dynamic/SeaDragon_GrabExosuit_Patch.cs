using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Broadcasts the exosuit grab by Sea Dragons and temporarily disables exosuit's position sync while they're grabbed.
/// </summary>
public sealed partial class SeaDragon_GrabExosuit_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((SeaDragon t) => t.GrabExosuit(default));

    public static void Prefix(SeaDragon __instance, Exosuit exosuit)
    {
        if (exosuit.TryGetComponent(out RemotelyControlled remotelyControlled))
        {
            remotelyControlled.enabled = false;
        }

        if (__instance.TryGetNitroxId(out NitroxId seaDragonId) && exosuit.TryGetNitroxId(out NitroxId targetId))
        {
            Resolve<IPacketSender>().Send(new SeaDragonGrabExosuit(seaDragonId, targetId));
        }
    }
}
