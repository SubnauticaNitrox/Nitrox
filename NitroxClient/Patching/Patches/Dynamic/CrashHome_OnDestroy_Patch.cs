using System;
using System.Reflection;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic;

namespace NitroxClient.Patching.Patches.Dynamic;

internal sealed partial class CrashHome_OnDestroy_Patch : NitroxPatch, IDynamicPatch
{
    private static SimulationOwnership simulationOwnership;
    private static IPacketSender packetSender;

    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((CrashHome t) => t.OnDestroy());

    public CrashHome_OnDestroy_Patch(SimulationOwnership so, IPacketSender ps)
    {
        simulationOwnership = so ?? throw new ArgumentNullException(nameof(so));
        packetSender = ps ?? throw new ArgumentNullException(nameof(ps));
    }

    public static void Prefix(CrashHome __instance)
    {
        if (!__instance.TryGetNitroxId(out NitroxId crashHomeId) ||
            !simulationOwnership.HasAnyLockType(crashHomeId) ||
            !__instance.crash ||
            !__instance.crash.TryGetNitroxId(out NitroxId crashId))
        {
            return;
        }
        packetSender.Send(new EntityDestroyed(crashId));
    }
}
