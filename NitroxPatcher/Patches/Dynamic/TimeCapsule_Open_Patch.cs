using NitroxModel.Packets;
using System.Reflection;
using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.Helper;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Ensures that when a player opens a time capsule, it is permanently destroyed on the server,
/// preventing it from respawning.
/// </summary>
public sealed partial class TimeCapsule_Open_Patch : NitroxPatch, IDynamicPatch
{
    internal static readonly MethodInfo TARGET_METHOD = Reflect.Method((TimeCapsule t) => t.Open());
    
    public static void Postfix(TimeCapsule __instance)
    {
        if (!__instance.TryGetIdOrWarn(out NitroxId timeCapsuleId))
        {
            return;
        }
        
        EntityDestroyed packet = new(timeCapsuleId);
        Resolve<IPacketSender>().Send(packet);
    }
}
