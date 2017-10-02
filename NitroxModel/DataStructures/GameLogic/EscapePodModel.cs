using System;
using System.Collections.Generic;
using UnityEngine;

namespace NitroxModel.DataStructures.GameLogic
{
    [Serializable]
    public class EscapePodModel
    {
        public String Guid { get; private set; }
        public Vector3 Location { get { return serializableLocation.ToVector3(); } }
        public String FabricatorGuid { get; private set; }
        public String MedicalFabricatorGuid { get; private set; }
        public String StorageContainerGuid { get; private set; }
        public String RadioGuid { get; private set; }
        public List<String> AssignedPlayers { get; private set; }
        
        private SerializableVector3 serializableLocation;
        
        public EscapePodModel(String guid, Vector3 location, String fabricatorGuid, String medicalFabricatorGuid, String storageContainerGuid, String radioGuid) : base()
        {
            this.Guid = guid;
            this.serializableLocation = SerializableVector3.From(location);
            this.FabricatorGuid = fabricatorGuid;
            this.MedicalFabricatorGuid = medicalFabricatorGuid;
            this.StorageContainerGuid = storageContainerGuid;
            this.RadioGuid = radioGuid;
            this.AssignedPlayers = new List<String>();
        }

        public override string ToString()
        {
            String toString = "[EscapePodModel - Guid: " + Guid + " Location:" + serializableLocation + " FabricatorGuid: " + FabricatorGuid + " MedicalFabricatorGuid: " + MedicalFabricatorGuid + " StorageContainerGuid: " + StorageContainerGuid + " RadioGuid: " + RadioGuid + " AssignedPlayers: {";

            foreach(String playerId in AssignedPlayers)
            {
                toString += playerId + " ";
            }

            return toString + "}]";
        }
    }
}
