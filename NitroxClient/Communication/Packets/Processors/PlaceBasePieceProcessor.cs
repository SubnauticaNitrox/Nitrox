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
    public class PlaceBasePieceProcessor : ClientPacketProcessor<PlaceBasePiece>
    {
        public override void Process(PlaceBasePiece basePiecePacket)
        {
            ConstructItem(basePiecePacket.Guid, basePiecePacket.ItemPosition, basePiecePacket.Rotation, basePiecePacket.Camera, basePiecePacket.TechType, basePiecePacket.ParentBaseGuid);
        }

        public void ConstructItem(String guid, Vector3 position, Quaternion rotation, Transform cameraTransform, TechType techType, Optional<String> parentBaseGuid)
        {
            GameObject buildPrefab = CraftData.GetBuildPrefab(techType);
            MultiplayerBuilder.overridePosition = position;
            MultiplayerBuilder.overrideQuaternion = rotation;
            MultiplayerBuilder.overrideTransform = cameraTransform;
            MultiplayerBuilder.placePosition = position;
            MultiplayerBuilder.placeRotation = rotation;
            MultiplayerBuilder.Begin(buildPrefab);

            Optional<GameObject> parentBase = (parentBaseGuid.IsPresent()) ? GuidHelper.GetObjectFrom(parentBaseGuid.Get()) : Optional<GameObject>.Empty();

            ConstructableBase constructableBase = MultiplayerBuilder.TryPlaceBase(parentBase);
            GuidHelper.SetNewGuid(constructableBase.gameObject, guid);

            /**
             * Manually call start to initialize the object as we may need to interact with it within the same frame.
             */
            MethodInfo startCrafting = typeof(Constructable).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
            Validate.NotNull(startCrafting);
            startCrafting.Invoke(constructableBase, new object[] { });
        }
    }
}
