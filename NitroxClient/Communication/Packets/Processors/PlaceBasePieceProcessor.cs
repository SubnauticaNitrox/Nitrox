using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.MonoBehaviours.Overrides;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Packets;
using System;
using System.Reflection;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PlaceBasePieceProcessor : ClientPacketProcessor<PlaceBasePiece>
    {
        public override void Process(PlaceBasePiece packet)
        {
            GameObject buildPrefab = CraftData.GetBuildPrefab(packet.TechType);
            MultiplayerBuilder.overridePosition = packet.ItemPosition;
            MultiplayerBuilder.overrideQuaternion = packet.Rotation;
            MultiplayerBuilder.overrideTransform = new GameObject().transform;
            MultiplayerBuilder.overrideTransform.position = packet.CameraPosition;
            MultiplayerBuilder.overrideTransform.rotation = packet.CameraRotation;
            MultiplayerBuilder.placePosition = packet.ItemPosition;
            MultiplayerBuilder.placeRotation = packet.Rotation;
            MultiplayerBuilder.Begin(buildPrefab);

            Optional<GameObject> parentBase = (packet.ParentBaseGuid.IsPresent()) ? GuidHelper.GetObjectFrom(packet.ParentBaseGuid.Get()) : Optional<GameObject>.Empty();

            ConstructableBase constructableBase = MultiplayerBuilder.TryPlaceBase(parentBase);
            GuidHelper.SetNewGuid(constructableBase.gameObject, packet.Guid);

            /**
             * Manually call start to initialize the object as we may need to interact with it within the same frame.
             */
            MethodInfo startCrafting = typeof(Constructable).GetMethod("Start", BindingFlags.NonPublic | BindingFlags.Instance);
            Validate.NotNull(startCrafting);
            startCrafting.Invoke(constructableBase, new object[] { });
        }
    }
}
