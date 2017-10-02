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
        public List<String> AssignedPlayers { get; }

        public EscapePodModel(String guid, Vector3 location, String fabricatorGuid, String medicalFabricatorGuid, String storageContainerGuid, String radioGuid) : base()
        {
            this.Guid = guid;
            this.Location = location;
            this.FabricatorGuid = fabricatorGuid;
            this.MedicalFabricatorGuid = medicalFabricatorGuid;
            this.StorageContainerGuid = storageContainerGuid;
            this.RadioGuid = radioGuid;
            this.AssignedPlayers = new List<String>();
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
