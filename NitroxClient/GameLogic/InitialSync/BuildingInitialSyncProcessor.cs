using System;
using System.Collections;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxModel.GameLogic;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic.InitialSync.Base;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.GameLogic.InitialSync
{
    public class BuildingInitialSyncProcessor : InitialSyncProcessor
    {
        private readonly IPacketSender packetSender;
        private readonly BuildThrottlingQueue buildEventQueue;

        private bool completed;
        
        public BuildingInitialSyncProcessor(IPacketSender packetSender, BuildThrottlingQueue buildEventQueue)
        {
            this.packetSender = packetSender;
            this.buildEventQueue = buildEventQueue;

            DependentProcessors.Add(typeof(CyclopsInitialAsyncProcessor));
        }

        public override IEnumerator Process(InitialPlayerSync packet, WaitScreen.ManualWaitItem waitScreenItem)
        {
            completed = false;
            NitroxServiceLocator.LocateService<IBuilding>().InitialSyncActive = true;

            List<BasePiece> basePieces = packet.BasePieces;
            Log.Info("Received initial sync packet with " + basePieces.Count + " base pieces");

#if TRACE && BUILDING
            foreach (BasePiece item in basePieces)
            {
                NitroxModel.Logger.Log.Debug("BuildingInitialSyncProcessor - unordered basePiece: " + item);
            }
#endif

            if (basePieces.Count == 0)
            {
                completed = true;
                NitroxServiceLocator.LocateService<IBuilding>().InitialSyncActive = false;
            }
            else
            {
                List<BasePiece> orderedbasePieces = GenerateBaseLayouts(basePieces);

#if TRACE && BUILDING
                foreach (BasePiece item in orderedbasePieces)
                {
                    NitroxModel.Logger.Log.Debug("BuildingInitialSyncProcessor - ordered basePiece: " + item);
                }
#endif

                ThrottledBuilder.main.WaitItem = waitScreenItem;
                ThrottledBuilder.main.Count = orderedbasePieces.Count;

                QueueUpPieces(orderedbasePieces);
                ThrottledBuilder.main.QueueDrained += FinishedCompletedBuildings;
            }

            yield return new WaitUntil(() => completed);
        }

        private void QueueUpPieces(List<BasePiece> basePieces)
        {
            using (packetSender.Suppress<BaseConstructionAmountChanged>())
            using (packetSender.Suppress<BaseConstructionCompleted>())
            using (packetSender.Suppress<BaseConstructionBegin>())
            using (packetSender.Suppress<BaseDeconstructionBegin>())
            using (packetSender.Suppress<BaseDeconstructionCompleted>())
            {
                foreach (BasePiece basePiece in basePieces)
                {
                    buildEventQueue.EnqueueConstructionBegin(basePiece);

                    if (basePiece.ConstructionCompleted)
                    {
                        buildEventQueue.EnqueueConstructionCompleted(basePiece.Id);
                    }
                    else
                    {
                        buildEventQueue.EnqueueAmountChanged(basePiece.Id, basePiece.ConstructionAmount);
                    }
                }
            }
        }

        private void FinishedCompletedBuildings(object sender, EventArgs eventArgs)
        {
            ThrottledBuilder.main.QueueDrained -= FinishedCompletedBuildings;
            completed = true;
            NitroxServiceLocator.LocateService<IBuilding>().InitialSyncActive = false;
        }

        // Subnautica changes the Layout of Bases when new BasePieces are added or removed. A simple replay of all the saved BasePieces is not enough, because 
        // deconstructig BasePieces (like Tubes) and building new BasePieces can cause the complete layout to change, so that following Pieces 
        // (like Hatches) aren't fit right to the corresponding objects. Additionally we need to change the 
        // The Method is not final. Some BasePiece Relations may be better validated by Face- or Cell-Connections, others still need fixed positons like 
        // Solarpanels or Furniture. For now the BuildIndex should work. 
        private List<BasePiece> GenerateBaseLayouts(List<BasePiece> basePieces)
        {
            List<BasePiece> internalList = new List<BasePiece>();

            // Basic sorting via BuildIndex
            basePieces.Sort((x, y) => x.BuildIndex.CompareTo(y.BuildIndex));

            // Corridors & Rooms & Foundations & Connectors first >> also needed in combination with sort order because old bases may be extended by new pieces
            // include not finished pieces for correct base targetting and face connecting

            foreach (BasePiece item in basePieces)
            {
                if (item.TechType.Name.Contains("Corridor") || item.TechType.Name.Contains("Room") || item.TechType.Name.Contains("Foundation") || item.TechType.Name.Contains("Connector") || item.TechType.Name.Contains("Observatory") || (item.TechType.Name.Contains("Moonpool") && !item.TechType.Name.Contains("Console")) )
                {
                    internalList.Add(item);

                    // If current BasePiece has Hull-Fortifications then add them here, otherwise the hull-integrity can be too low 
                    // causing breaches before the fortifications are loaded later on another BasePiece
                    // #ISSUE 1030# 
                    // Rework or completely remove later, when figured out how to supress hull integrity calculation and update while InitialSync
                    foreach (BasePiece item2 in basePieces)
                    {
                        if ((item2.TechType.Name.Contains("Reinforcement") || item2.TechType.Name.Contains("Bulkhead")) && item2.ConstructionCompleted && item.ItemPosition == item2.ItemPosition && !internalList.Contains(item2))
                        {
                            internalList.Add(item2);
                        }
                    }
                }
            }

            // Place now the WaterTanks, they need to be placed before the Hatches, because Hatches can also be attached to Tanks
            foreach (BasePiece item in basePieces)
            {
                if (item.ConstructionCompleted && (item.TechType.Name.Contains("Water")) && !internalList.Contains(item))
                {
                    internalList.Add(item);
                }
            }

            // Place all Hatches now, because if they are attached to Rooms, they create small corridors which can be holders for fabricators or lockers, etc.
            foreach (BasePiece item in basePieces)
            {
                if (item.ConstructionCompleted && (item.TechType.Name.Contains("Hatch")) && !internalList.Contains(item))
                {
                    internalList.Add(item);
                }
            }

            // All finished energy storage pieces. They need to be placed before energy-consuming objects are placed
            foreach (BasePiece item in basePieces)
            {
                if (item.ConstructionCompleted && (item.TechType.Name.Contains("Solar") || item.TechType.Name.ToUpper().Contains("REACTOR") || item.TechType.Name.Contains("ThermalPlant") || item.TechType.Name.Contains("PowerTransmitter")) && !internalList.Contains(item))
                {
                    internalList.Add(item);
                }
            }

            //all other finished pieces that are no furniture
            foreach (BasePiece item in basePieces)
            {
                if (item.ConstructionCompleted && !item.IsFurniture && !internalList.Contains(item))
                {
                    internalList.Add(item);
                }
            }

            //all finished furniture pieces
            foreach (BasePiece item in basePieces)
            {
                if (item.ConstructionCompleted && !internalList.Contains(item))
                {
                    internalList.Add(item);
                }
            }

            //the rest of the pieces
            foreach (BasePiece item in basePieces)
            {
                if (!internalList.Contains(item))
                {
                    internalList.Add(item);
                }
            }

            return internalList;
        }
    }
}
