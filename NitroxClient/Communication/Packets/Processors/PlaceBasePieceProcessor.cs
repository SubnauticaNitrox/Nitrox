using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PlaceBasePieceProcessor : GenericPacketProcessor<PlaceBasePiece>
    {
        public override void Process(PlaceBasePiece basePiecePacket)
        {
            Optional<TechType> opTechType = ApiHelper.TechType(basePiecePacket.TechType);

            if (opTechType.IsPresent())
            {
                TechType techType = opTechType.Get();
                GameObject techPrefab = TechTree.main.GetGamePrefab(techType);
                ConstructItem(basePiecePacket.Guid, ApiHelper.Vector3(basePiecePacket.ItemPosition), ApiHelper.Quaternion(basePiecePacket.Rotation), techType, basePiecePacket.ParentBaseGuid);
            }
            else
            {
                Console.WriteLine("Could not identify tech type for " + basePiecePacket.TechType);
            }
        }
        
        public void ConstructItem(String guid, Vector3 position, Quaternion rotation, TechType techType, Optional<String> parentBaseGuid)
        {
            GameObject buildPrefab = CraftData.GetBuildPrefab(techType);
            MultiplayerBuilder.overridePosition = position;
            MultiplayerBuilder.overrideQuaternion = rotation;
            MultiplayerBuilder.placePosition = position;
            MultiplayerBuilder.placeRotation = rotation;
            MultiplayerBuilder.Begin(buildPrefab);

            Optional<GameObject> parentBase = (parentBaseGuid.IsPresent()) ? GuidHelper.GetObjectFrom(parentBaseGuid.Get()) : Optional<GameObject>.Empty();

            ConstructableBase constructableBase = MultiplayerBuilder.TryPlaceBase(parentBase);
            GuidHelper.SetNewGuid(constructableBase.gameObject, guid);
        }        
    }
}
