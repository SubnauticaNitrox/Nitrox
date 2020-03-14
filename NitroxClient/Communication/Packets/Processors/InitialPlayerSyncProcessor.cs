using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using System.Collections.Generic;
using NitroxClient.GameLogic.InitialSync.Base;
using System;
using NitroxModel.Logger;
using NitroxClient.MonoBehaviours;
using System.Collections;

namespace NitroxClient.Communication.Packets.Processors
{
    public class InitialPlayerSyncProcessor : ClientPacketProcessor<InitialPlayerSync>
    {
        private HashSet<InitialSyncProcessor> processors;
        private HashSet<Type> alreadyRan = new HashSet<Type>();
        private InitialPlayerSync packet;

        private WaitScreen.ManualWaitItem initialSyncWaitItem;
        private WaitScreen.ManualWaitItem subWaitScreenItem;

        private int processorsRan;

        public InitialPlayerSyncProcessor(IEnumerable<InitialSyncProcessor> processors)
        {
            this.processors = processors.ToSet();
        }

        public override void Process(InitialPlayerSync packet)
        {
            this.packet = packet;
            initialSyncWaitItem = WaitScreen.Add("Processing Initial Sync");
            Multiplayer.Main.StartCoroutine(ProcessInitialSyncPacket(this, null));
        }
        
        private IEnumerator ProcessInitialSyncPacket(object sender, EventArgs eventArgs)
        {
            bool moreProcessorsToRun;

            do
            {
                yield return Multiplayer.Main.StartCoroutine(RunPendingProcessors());
                
                moreProcessorsToRun = (alreadyRan.Count < processors.Count);

                if (moreProcessorsToRun && processorsRan == 0)
                {
                    throw new Exception("Detected circular dependencies in initial packet sync between: " + GetRemainingProcessorsText());
                }
            } while (moreProcessorsToRun);

            WaitScreen.Remove(initialSyncWaitItem);
            Multiplayer.Main.InitialSyncCompleted = true;
        }

        private IEnumerator RunPendingProcessors()
        {
            processorsRan = 0;

            foreach (InitialSyncProcessor processor in processors)
            {
                if (IsWaitingToRun(processor.GetType()) && HasDependenciesSatisfied(processor))
                {
                    initialSyncWaitItem.SetProgress(processorsRan, processors.Count);

                    Log.Info("Running " + processor.GetType());
                    alreadyRan.Add(processor.GetType());
                    processorsRan++;

                    subWaitScreenItem = WaitScreen.Add("Running " + processor.GetType().Name);
                    yield return Multiplayer.Main.StartCoroutine(processor.Process(packet, subWaitScreenItem));
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

        private bool IsWaitingToRun(Type processor)
        {
            return (alreadyRan.Contains(processor) == false);
        }

        private string GetRemainingProcessorsText()
        {
            string remaining = "";

            foreach (InitialSyncProcessor processor in processors)
            {
                if (IsWaitingToRun(processor.GetType()))
                {
                    remaining += " " + processor.GetType();
                }
            }

            return remaining;
        }
    }
}
