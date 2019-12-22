using System;
using System.Collections.Generic;
using ProtoBufNet;
using UnityEngine;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class EscapePodModel
    {
        [ProtoMember(1)]
        public NitroxId Id { get; set; }

        [ProtoMember(2)]
        public Vector3 Location { get; set; }

        [ProtoMember(3)]
        public NitroxId FabricatorId { get; set; }

        [ProtoMember(4)]
        public NitroxId MedicalFabricatorId { get; set; }

        [ProtoMember(5)]
        public NitroxId StorageContainerId { get; set; }

        [ProtoMember(6)]
        public NitroxId RadioId { get; set; }

        [ProtoMember(7)]
        public List<NitroxId> AssignedPlayers { get; set; } = new List<NitroxId>();

        [ProtoMember(8)]
        public bool Damaged { get; set; }

        [ProtoMember(9)]
        public bool RadioDamaged { get; set; }

        public void InitEscapePodModel(NitroxId id, Vector3 location, NitroxId fabricatorId, NitroxId medicalFabricatorId, NitroxId storageContainerId, NitroxId radioId, bool damaged, bool radioDamaged)
        {
            Id = id;
            Location = location;
            FabricatorId = fabricatorId;
            MedicalFabricatorId = medicalFabricatorId;
            StorageContainerId = storageContainerId;
            RadioId = radioId;
            Damaged = damaged;
            RadioDamaged = radioDamaged;
        }

        public override string ToString()
        {
            string toString = "[EscapePodModel - Id: " + Id + " Location:" + Location + " FabricatorId: " + FabricatorId + " MedicalFabricatorGuid: " + MedicalFabricatorId + " StorageContainerGuid: " + StorageContainerId + " RadioGuid: " + RadioId + " AssignedPlayers: {";

            foreach (NitroxId playerId in AssignedPlayers)
            {
                toString += playerId + " ";
            }

            return toString + "} Damaged: " + Damaged + " RadioDamaged: " + RadioDamaged + "]";
        }
    }
}
