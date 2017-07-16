using NitroxClient.Communication.Packets.Processors.Base;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxModel.DataStructures.Util;
using NitroxModel.Packets;
using System;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PlaceFurnitureProcessor : GenericPacketProcessor<PlaceFurniture>
    {
        public override void Process(PlaceFurniture placeFurniturePacket)
        {
            Optional<TechType> opTechType = ApiHelper.TechType(placeFurniturePacket.TechType);

            if (opTechType.IsPresent())
            {
                TechType techType = opTechType.Get();
                GameObject techPrefab = TechTree.main.GetGamePrefab(techType);
                ConstructItem(placeFurniturePacket.Guid, placeFurniturePacket.SubGuid, ApiHelper.Vector3(placeFurniturePacket.ItemPosition), ApiHelper.Quaternion(placeFurniturePacket.Rotation), techType);
            }
            else
            {
                Console.WriteLine("Could not identify tech type for " + placeFurniturePacket.TechType);
            }
        }
        
        public void ConstructItem(String guid, String subGuid, Vector3 position, Quaternion rotation, TechType techType)
        {
            GameObject buildPrefab = CraftData.GetBuildPrefab(techType);
            MultiplayerBuilder.overridePosition = position;
            MultiplayerBuilder.overrideQuaternion = rotation;
            MultiplayerBuilder.placePosition = position;
            MultiplayerBuilder.placeRotation = rotation;
            MultiplayerBuilder.Begin(buildPrefab);

            Optional<GameObject> opSub = GuidHelper.GetObjectFrom(subGuid);

            if (opSub.IsEmpty())
            {
                Console.WriteLine("Could not locate sub with guid" + subGuid);
                return;
            }

            SubRoot subRoot = opSub.Get().GetComponent<SubRoot>();

            GameObject gameObject = MultiplayerBuilder.TryPlaceFurniture(subRoot);
            GuidHelper.SetNewGuid(gameObject, guid);
        }        
    }
}
