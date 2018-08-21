using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    public class EscapePodModel
    {
        public string Guid { get; }
        public Vector3 Location { get; }
        public string FabricatorGuid { get; }
        public string MedicalFabricatorGuid { get; }
        public string StorageContainerGuid { get; }
        public string RadioGuid { get; }
        public List<ushort> AssignedPlayers { get; } = new List<ushort>();

        public EscapePodModel(string guid, Vector3 location, string fabricatorGuid, string medicalFabricatorGuid, string storageContainerGuid, string radioGuid)
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
