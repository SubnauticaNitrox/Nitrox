using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Text;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.MonoBehaviours;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors
{
    public class InitialPlayerSyncProcessor : ClientPacketProcessor<InitialPlayerSync>
    {
        private readonly IPacketSender packetSender;
        private readonly HashSet<InitialSyncProcessor> processors;
        private readonly HashSet<Type> alreadyRan = new HashSet<Type>();
        private InitialPlayerSync packet;

        private WaitScreen.ManualWaitItem loadingMultiplayerWaitItem;

        private int cumulativeProcessorsRan;
        private int processorsRanLastCycle;

        public InitialPlayerSyncProcessor(IPacketSender packetSender, IEnumerable<InitialSyncProcessor> processors)
        {
            this.packetSender = packetSender;
            this.processors = processors.ToSet();
        }

        public override void Process(InitialPlayerSync packet)
        {
            this.packet = packet;
            loadingMultiplayerWaitItem = WaitScreen.Add("Syncing Multiplayer World");
            cumulativeProcessorsRan = 0;
            Multiplayer.Instance.StartCoroutine(ProcessInitialSyncPacket());
        }

        private IEnumerator ProcessInitialSyncPacket()
        {
            // Some packets should not fire during game session join but only afterwards so that initialized/spawned game objects don't trigger packet sending again. 
            using (packetSender.Suppress<PingRenamed>())
            {
                bool moreProcessorsToRun;
                do
                {
                    yield return Multiplayer.Instance.StartCoroutine(RunPendingProcessors());

                    moreProcessorsToRun = alreadyRan.Count < processors.Count;
                    if (moreProcessorsToRun && processorsRanLastCycle == 0)
                    {
                        throw new InvalidDataException($"Detected circular dependencies in initial packet sync between: {GetRemainingProcessorsText()}");
                    }
                } while (moreProcessorsToRun);
            }

            WaitScreen.Remove(loadingMultiplayerWaitItem);
            Multiplayer.Instance.InitialSyncCompleted = true;
        }

        private IEnumerator RunPendingProcessors()
        {
            processorsRanLastCycle = 0;

            foreach (InitialSyncProcessor processor in processors)
            {
                if (IsWaitingToRun(processor.GetType()) && HasDependenciesSatisfied(processor))
                {
                    loadingMultiplayerWaitItem.SetProgress(cumulativeProcessorsRan, processors.Count);

                    Log.Info("Running " + processor.GetType());
                    alreadyRan.Add(processor.GetType());
                    processorsRanLastCycle++;
                    cumulativeProcessorsRan++;

                    WaitScreen.ManualWaitItem subWaitScreenItem = WaitScreen.Add("Running " + processor.GetType().Name);
                    yield return Multiplayer.Instance.StartCoroutine(processor.Process(packet, subWaitScreenItem));
                    WaitScreen.Remove(subWaitScreenItem);
                }
            }
        }

        private bool HasDependenciesSatisfied(InitialSyncProcessor processor)
        {
            foreach (Type dependentType in processor.DependentProcessors)
            {
                if (IsWaitingToRun(dependentType))
                {
                    return false;
                }
            }

            return true;
        }

        private bool IsWaitingToRun(Type processor) => !alreadyRan.Contains(processor);

        private string GetRemainingProcessorsText()
        {
            StringBuilder remaining = new StringBuilder("");

            foreach (InitialSyncProcessor processor in processors)
            {
                if (IsWaitingToRun(processor.GetType()))
                {
                    remaining.Append(processor.GetType());
                    remaining.Append(", ");
                }
            }

            return remaining.ToString();
        }
    }
}
