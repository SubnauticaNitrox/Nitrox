using System;
using HarmonyLib;
using NitroxClient.Communication.Abstract;
using NitroxModel.Helper;
using Story;
using System.Linq;
using System.Reflection;
using NitroxModel.Packets;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Watches for any modification to the sunbeam, and if any, notifies the server of it
/// </summary>
public class StoryGoalCustomEventHandler_NotifyGoalComplete_Patch : NitroxPatch, IDynamicPatch
{
    private static readonly MethodInfo TARGET_METHOD = Reflect.Method((StoryGoalCustomEventHandler t) => t.NotifyGoalComplete(default));

    /// <summary>
    /// In the case that either countdownActive or countdownStartingTime was modified, notify the server accordingly
    /// </summary>
    public static void Prefix(StoryGoalCustomEventHandler __instance, string key)
    {
        switch (key)
        {
            case "Goal_Disable_Gun":
                if (StoryGoalManager.main.pendingRadioMessages.Any(message => string.Equals(message, "RadioSunbeam4", StringComparison.OrdinalIgnoreCase)))
                {
                    Resolve<IPacketSender>().Send(new SunbeamUpdate(-1));
                }
                break;
            case "OnPlayRadioSunbeam4":
                Resolve<IPacketSender>().Send(new SunbeamUpdate(DayNightCycle.main.timePassedAsFloat));
                break;
            case "SunbeamCheckPlayerRange":
                Resolve<IPacketSender>().Send(new SunbeamUpdate(-1));
                break;
        }
        // In the case the event is just a sunbeam event, we need to verify if we ever trigger the sunbeamCancel
        if (__instance.sunbeamGoals.Any(goal => string.Equals(key, goal.trigger, StringComparison.OrdinalIgnoreCase)) && __instance.gunDisabled)
        {
            Resolve<IPacketSender>().Send(new SunbeamUpdate(-1));
        }
    }

    public override void Patch(Harmony harmony)
    {
        PatchPrefix(harmony, TARGET_METHOD);
    }
}
