using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using System.Collections.Generic;
using NitroxClient.GameLogic.InitialSync.Base;
using System;
using NitroxModel.Logger;
using NitroxClient.MonoBehaviours;

namespace NitroxClient.Communication.Packets.Processors
{
    public class InitialPlayerSyncProcessor : ClientPacketProcessor<InitialPlayerSync>
    {
        private HashSet<InitialSyncProcessor> processors;
        private HashSet<Type> alreadyRan = new HashSet<Type>();
        private InitialPlayerSync packet;

        public InitialPlayerSyncProcessor(IEnumerable<InitialSyncProcessor> processors)
        {
            this.processors = processors.ToSet();
        }

        public override void Process(InitialPlayerSync packet)
        {
            this.packet = packet;
            ProcessInitialSyncPacket(this, null);
        }
        
        private void ProcessInitialSyncPacket(object sender, EventArgs eventArgs)
        {
            bool moreProcessorsToRun;

            do
            {
                int processorsRan = 0;
                bool ranAsyncProcessor = false;

                RunPendingProcessors(ref processorsRan, ref ranAsyncProcessor);

                if(ranAsyncProcessor)
                {
                    // If we ran an async process, we'll have to wait for it to finish before we continue
                    return;
                }

                moreProcessorsToRun = (alreadyRan.Count < processors.Count);

                if (moreProcessorsToRun && processorsRan == 0)
                {
                    throw new Exception("Detected circular dependencies in initial packet sync between: " + GetRemainingProcessorsText());
                }
            } while (moreProcessorsToRun);
            
            Multiplayer.Main.InitialSyncCompleted = true;
        }

        private void RunPendingProcessors(ref int processorsRan, ref bool ranAsyncProcessor)
        {
            processorsRan = 0;
            ranAsyncProcessor = false;

            foreach (InitialSyncProcessor processor in processors)
            {
                if (IsWaitingToRun(processor.GetType()) && HasDependenciesSatisfied(processor))
                {
                    Log2.Instance.Log(NLogType.Info, "Running " + processor.GetType());
                    alreadyRan.Add(processor.GetType());
                    processorsRan++;

                    if (processor is AsyncInitialSyncProcessor)
                    {
                        ((AsyncInitialSyncProcessor)processor).Completed += ProcessInitialSyncPacket;
                        processor.Process(packet);
                        ranAsyncProcessor = true;
                        return;
                    }
                    else
                    {
                        processor.Process(packet);
                    }
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
