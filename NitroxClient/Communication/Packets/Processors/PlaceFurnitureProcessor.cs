using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Helper.GameLogic;
using NitroxModel.Packets;
using System;
using System.Reflection;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PlaceFurnitureProcessor : ClientPacketProcessor<PlaceFurniture>
    {
        public override void Process(PlaceFurniture placeFurniturePacket)
        {
            ConstructItem(placeFurniturePacket.Guid, placeFurniturePacket.SubGuid, placeFurniturePacket.ItemPosition, placeFurniturePacket.Rotation, placeFurniturePacket.Camera, placeFurniturePacket.TechType);
        }

        public void ConstructItem(String guid, Optional<String> subGuid, Vector3 position, Quaternion rotation, Transform cameraTransform, TechType techType)
        {
            GameObject buildPrefab = CraftData.GetBuildPrefab(techType);
            MultiplayerBuilder.overridePosition = position;
            MultiplayerBuilder.overrideQuaternion = rotation;
            MultiplayerBuilder.overrideTransform = cameraTransform;
            MultiplayerBuilder.placePosition = position;
            MultiplayerBuilder.placeRotation = rotation;
            MultiplayerBuilder.Begin(buildPrefab);

            SubRoot subRoot = null;

            if (subGuid.IsPresent())
            {
                Optional<GameObject> opSub = GuidHelper.GetObjectFrom(subGuid.Get());
                if (opSub.IsPresent())
                {
                    subRoot = opSub.Get().GetComponent<SubRoot>();
                }
            }

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
