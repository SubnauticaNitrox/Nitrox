﻿using NitroxClient.Communication.Packets.Processors.Abstract;
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
    public class PlaceBasePieceProcessor : ClientPacketProcessor<PlaceBasePiece>
    {
        private GameObject otherPlayerCamera;

        public override void Process(PlaceBasePiece basePiecePacket)
        {
            Optional<TechType> opTechType = ApiHelper.TechType(basePiecePacket.TechType);

            if (opTechType.IsPresent())
            {
                TechType techType = opTechType.Get();

                if (otherPlayerCamera == null)
                {
                    otherPlayerCamera = new GameObject();
                }

                ApiHelper.SetTransform(otherPlayerCamera.transform, basePiecePacket.Camera);

                ConstructItem(basePiecePacket.Guid, ApiHelper.Vector3(basePiecePacket.ItemPosition), ApiHelper.Quaternion(basePiecePacket.Rotation), otherPlayerCamera.transform, techType, basePiecePacket.ParentBaseGuid);
            }
            else
            {
                Console.WriteLine("Could not identify tech type for " + basePiecePacket.TechType);
            }
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
