using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    public class EscapePodModel
    {
        public String Guid { get; }
        public Vector3 Location { get; }
        public String FabricatorGuid { get; }
        public String MedicalFabricatorGuid { get; }
        public String StorageContainerGuid { get; }
        public String RadioGuid { get; }
        public List<String> AssignedPlayers { get; } = new List<String>();

        public EscapePodModel(String guid, Vector3 location, String fabricatorGuid, String medicalFabricatorGuid, String storageContainerGuid, String radioGuid) : base()
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
            String toString = "[EscapePodModel - Guid: " + Guid + " Location:" + Location + " FabricatorGuid: " + FabricatorGuid + " MedicalFabricatorGuid: " + MedicalFabricatorGuid + " StorageContainerGuid: " + StorageContainerGuid + " RadioGuid: " + RadioGuid + " AssignedPlayers: {";

            foreach (String playerId in AssignedPlayers)
            {
                toString += playerId + " ";
            }

            return toString + "}]";
        }
    }
}
