using System.Reflection;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.InitialSync;
using NitroxModel.Helper;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Notice the server of a Ping color change
/// </summary>
public class PingInstance_Set_Patches : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD_COLOR = Reflect.Method((PingInstance t) => t.SetColor(default));
    private static readonly MethodInfo TARGET_METHOD_VISIBLE = Reflect.Method((PingInstance t) => t.SetVisible(default));

    public static void PrefixColor(PingInstance __instance, int index)
    {
        // Only notice server if there's a change on client-side
        if (__instance.colorIndex == index)
        {
            return;
        }
        
        if (PlayerPreferencesInitialSyncProcessor.TryGetKeyForPingInstance(__instance, out string pingKey, out bool _))
        {
            Resolve<IPacketSender>().Send(new SignalPingPreferenceChanged(pingKey, __instance.visible, index));
        }
    }

    public static void PrefixVisible(PingInstance __instance, bool visible)
    {
        // Only notice server if there's a change on client-side
        if (__instance.visible == visible)
        {
            return;
        }

        if (PlayerPreferencesInitialSyncProcessor.TryGetKeyForPingInstance(__instance, out string pingKey, out bool _))
        {
            Resolve<IPacketSender>().Send(new SignalPingPreferenceChanged(pingKey, visible, __instance.colorIndex));
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD_COLOR, nameof(PrefixColor));
        PatchPrefix(harmony, TARGET_METHOD_VISIBLE, nameof(PrefixVisible));
    }
}
