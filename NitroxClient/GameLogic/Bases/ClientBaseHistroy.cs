using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.Bases.Spawning;
using NitroxClient.GameLogic.Helper;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Util;
using NitroxModel.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.Helper;
using UnityEngine;
using static NitroxClient.GameLogic.Helper.TransientLocalObjectManager;

namespace NitroxClient.GameLogic
{
    public class ClientBaseHistory
    {
        private readonly IPacketSender packetSender;
        private readonly RotationMetadataFactory rotationMetadataFactory;
        public ClientBaseHistory(RotationMetadataFactory rotationMetadataFactory)
        {
            this.rotationMetadataFactory = rotationMetadataFactory;
        }
        List<BasePiece> clientBaseHistroy = new List<BasePiece>();
        public void BuildUpdate(BaseGhost baseGhost, ConstructableBase constructableBase, Base targetBase, TechType techType, Quaternion quaternion)
        {
            List<BasePiece> ClientBaseData = new List<BasePiece>();
            List<string> ClientBaseDataList = new List<string>();
            NitroxId id = NitroxEntity.GetId(constructableBase.gameObject);
            NitroxId parentBaseId = null;

            if (targetBase != null)
            {
                parentBaseId = NitroxEntity.GetId(targetBase.gameObject);
            }
            else if (constructableBase != null)
            {
                Base playerBase = constructableBase.gameObject.GetComponentInParent<Base>();

                if (playerBase != null)
                {
                    parentBaseId = NitroxEntity.GetId(playerBase.gameObject);
                }
            }

            if (parentBaseId == null)
            {
                Base playerBase = baseGhost.gameObject.GetComponentInParent<Base>();

                if (playerBase != null)
                {
                    parentBaseId = NitroxEntity.GetId(playerBase.gameObject);
                }
            }

            Vector3 placedPosition = constructableBase.gameObject.transform.position;
            Transform camera = Camera.main.transform;
            Optional<RotationMetadata> rotationMetadata = rotationMetadataFactory.From(baseGhost);

            BasePiece basePiece = new BasePiece(id, placedPosition, quaternion, camera.position, camera.rotation, techType.Model(), Optional.OfNullable(parentBaseId), false, rotationMetadata);
            PlaceBasePiece placedBasePiece = new PlaceBasePiece(basePiece);
            clientBaseHistroy.Add(basePiece);
        }
    }
}
