using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Notice the server of a Ping visibility change
/// </summary>
public class PingInstance_SetVisible_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((PingInstance t) => t.SetVisible(default));

    public static void Prefix(PingInstance __instance, bool visible)
    {
        // Only notice server if there's a change on client-side
        if (__instance.visible == visible)
        {
            return;
        }

        if (PingInstance_SetColor_Patch.TryGetPingKey(__instance.gameObject, out string pingKey))
        {
            Resolve<IPacketSender>().Send(new SignalPingPreferenceChanged(pingKey, visible));
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD, nameof(Prefix));
    }
}
