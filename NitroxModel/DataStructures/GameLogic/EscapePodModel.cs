using System;
using System.Collections.Generic;
using ProtoBuf;
using UnityEngine;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class EscapePodModel
    {
        [ProtoMember(1)]
        public List<ushort> SerializedAssignedPlayers
        {
            get
            {
                lock (AssignedPlayers)
                {
                    return AssignedPlayers;
                }
            }
            set
            {
                AssignedPlayers = value;
            }
        }
        [ProtoMember(2)]
        public string Guid { get; set; }
        [ProtoMember(3)]
        public Vector3 Location { get; set; }
        [ProtoMember(4)]
        public string FabricatorGuid { get; set; }
        [ProtoMember(5)]
        public string MedicalFabricatorGuid { get; set; }
        [ProtoMember(6)]
        public string StorageContainerGuid { get; set; }
        [ProtoMember(7)]
        public string RadioGuid { get; set; }
        [ProtoIgnore]
        public List<ushort> AssignedPlayers { get; set; } = new List<ushort>();

        public void InitEscapePodModel(string guid, Vector3 location, string fabricatorGuid, string medicalFabricatorGuid, string storageContainerGuid, string radioGuid)
        {
            Guid = guid;
            Location = location;
            FabricatorGuid = fabricatorGuid;
            MedicalFabricatorGuid = medicalFabricatorGuid;
            StorageContainerGuid = storageContainerGuid;
            RadioGuid = radioGuid;
        }

        public override string ToString()
        {
            string toString = "[EscapePodModel - Guid: " + Guid + " Location:" + Location + " FabricatorGuid: " + FabricatorGuid + " MedicalFabricatorGuid: " + MedicalFabricatorGuid + " StorageContainerGuid: " + StorageContainerGuid + " RadioGuid: " + RadioGuid + " AssignedPlayers: {";

            foreach (ushort playerId in AssignedPlayers)
            {
                toString += playerId + " ";
            }

            return toString + "}]";
        }
    }
}
