using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Packets;
using System;
using System.Reflection;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PlaceFurnitureProcessor : GenericPacketProcessor<PlaceFurniture>
    {
        private static GameObject otherPlayerCamera;

        public override void Process(PlaceFurniture placeFurniturePacket)
        {
            Optional<TechType> opTechType = ApiHelper.TechType(placeFurniturePacket.TechType);

            if (opTechType.IsPresent())
            {
                TechType techType = opTechType.Get();

                if(otherPlayerCamera == null)
                {
                    otherPlayerCamera = new GameObject();
                }

                ApiHelper.SetTransform(otherPlayerCamera.transform, placeFurniturePacket.Camera);

                ConstructItem(placeFurniturePacket.Guid, placeFurniturePacket.SubGuid, ApiHelper.Vector3(placeFurniturePacket.ItemPosition), ApiHelper.Quaternion(placeFurniturePacket.Rotation), otherPlayerCamera.transform, techType);
            }
            else
            {
                Console.WriteLine("Could not identify tech type for " + placeFurniturePacket.TechType);
            }
        }
        
        public void ConstructItem(String guid, String subGuid, Vector3 position, Quaternion rotation, Transform cameraTransform, TechType techType)
        {
            GameObject buildPrefab = CraftData.GetBuildPrefab(techType);
            MultiplayerBuilder.overridePosition = position;
            MultiplayerBuilder.overrideQuaternion = rotation;
            MultiplayerBuilder.overrideTransform = cameraTransform;
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

            Constructable constructable = gameObject.GetComponentInParent<Constructable>();
            Validate.NotNull(constructable);          

            /**
             * Manually call start to initialize the object as we may need to interact with it within the same frame.
             */
            MethodInfo startCrafting = typeof(Constructable).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
            Validate.NotNull(startCrafting);
            startCrafting.Invoke(constructable, new object[] { });
        }        
    }
}
