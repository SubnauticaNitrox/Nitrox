﻿using System.Reflection;
using NitroxClient.Communication.Abstract;
using Nitrox.Model.DataStructures;

namespace NitroxPatcher.Patches.Dynamic;

public sealed partial class Seaglide_onLightsToggled_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((Seaglide t) => t.onLightsToggled(default(bool)));

    public static void Postfix(Seaglide __instance, bool active)
    {
        if (__instance.TryGetIdOrWarn(out NitroxId id))
        {
            Resolve<IPacketSender>().Send(new Nitrox.Model.Subnautica.Packets.ToggleLights(id, active));
        }
    }
}
