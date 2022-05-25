using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.MonoBehaviours;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync;

/// <summary>
/// Happens after each initial sync processor finished loading
/// </summary>
public class PostInitialSyncProcessor : InitialSyncProcessor
{
    private IPacketSender packetSender;

    public PostInitialSyncProcessor(IPacketSender packetSender)
    {
        this.packetSender = packetSender;

        IEnumerable<Type> allProcessors = AppDomain.CurrentDomain.GetAssemblies()
            .SelectMany(s => s.GetTypes())
            .Where(type => typeof(InitialSyncProcessor).IsAssignableFrom(type) &&
                           !type.IsAbstract &&
                           type != GetType());
        
        DependentProcessors.AddRange(allProcessors);
    }

    public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
    {
        UpdatePingInstanceVisibility(packet.PingInstancePreferences.HiddenSignalPings);
        waitScreenItem.SetProgress(0.5f);
        yield return null;

        UpdatePingInstanceColors(packet.PingInstancePreferences.ColorPreferences);
        waitScreenItem.SetProgress(1f);
        yield return null;
    }

    private void UpdatePingInstanceVisibility(HashSet<string> hiddenSignalPings)
    {
        foreach (PingInstance pingInstance in GameObject.FindObjectsOfType<PingInstance>())
        {
            if ((pingInstance.TryGetComponent(out SignalPing signalPing) && hiddenSignalPings.Contains(signalPing.descriptionKey)) ||
                (pingInstance.TryGetComponent(out NitroxEntity nitroxEntity) && hiddenSignalPings.Contains(nitroxEntity.Id.ToString())))
            {
                using (packetSender.Suppress<SignalPingPreferenceChanged>())
                {
                    pingInstance.SetVisible(false);
                }
            }
        }
    }

    private void UpdatePingInstanceColors(List<KeyValuePair<string, int>> colorPreferences)
    {
        Dictionary<string, int > preferences = colorPreferences.ToDictionary(pref => pref.Key, pref => pref.Value);
        foreach (PingInstance pingInstance in GameObject.FindObjectsOfType<PingInstance>())
        {
            if ((pingInstance.TryGetComponent(out SignalPing signalPing) && preferences.TryGetValue(signalPing.descriptionKey, out int color)) ||
                (pingInstance.TryGetComponent(out NitroxEntity nitroxEntity) && preferences.TryGetValue(nitroxEntity.Id.ToString(), out color)))
            {
                using (packetSender.Suppress<SignalPingPreferenceChanged>())
                {
                    pingInstance.SetColor(color);
                }
            }
        }
    }
}
