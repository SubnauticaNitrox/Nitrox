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
        UpdateSignalPingsVisibility(packet.HiddenSignalPings);
        waitScreenItem.SetProgress(1f);
        yield return null;
    }

    private void UpdateSignalPingsVisibility(HashSet<string> hiddenSignalPings)
    {
        foreach (PingInstance pingInstance in GameObject.FindObjectsOfType<PingInstance>())
        {
            bool hide = (pingInstance.TryGetComponent(out SignalPing signalPing) && hiddenSignalPings.Contains(signalPing.descriptionKey)) ||
                        (pingInstance.TryGetComponent(out NitroxEntity nitroxEntity) && hiddenSignalPings.Contains(nitroxEntity.Id.ToString()));
            
            if (hide)
            {
                using (packetSender.Suppress<SignalPingVisibilityChanged>())
                {
                    pingInstance.SetVisible(false);
                }
            }
        }
    }
}
