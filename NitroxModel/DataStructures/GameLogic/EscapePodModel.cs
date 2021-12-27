using System.Collections.Generic;
using NitroxModel.DataStructures.Unity;
using ProtoBufNet;
using ZeroFormatter;

namespace NitroxModel.DataStructures.GameLogic
{
    [ZeroFormattable]
    [ProtoContract]
    public class EscapePodModel
    {
        public const int PLAYERS_PER_ESCAPEPOD = 50;

        [Index(0)]
        [ProtoMember(1)]
        public virtual NitroxId Id { get; set; }

        [Index(1)]
        [ProtoMember(2)]
        public virtual NitroxVector3 Location { get; set; }

        [Index(2)]
        [ProtoMember(3)]
        public virtual NitroxId FabricatorId { get; set; }

        [Index(3)]
        [ProtoMember(4)]
        public virtual NitroxId MedicalFabricatorId { get; set; }

        [Index(4)]
        [ProtoMember(5)]
        public virtual NitroxId StorageContainerId { get; set; }

        [Index(5)]
        [ProtoMember(6)]
        public virtual NitroxId RadioId { get; set; }

        [Index(6)]
        [ProtoMember(7)]
        public virtual List<ushort> AssignedPlayers { get; set; } = new List<ushort>();

        [Index(7)]
        [ProtoMember(8)]
        public virtual bool Damaged { get; set; }

        [Index(8)]
        [ProtoMember(9)]
        public virtual bool RadioDamaged { get; set; }

        public void InitEscapePodModel(NitroxId id, NitroxVector3 location, NitroxId fabricatorId, NitroxId medicalFabricatorId, NitroxId storageContainerId, NitroxId radioId, bool damaged, bool radioDamaged)
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

        public bool IsFull()
        {
            return false; //AssignedPlayers.Count >= PLAYERS_PER_ESCAPEPOD; // TODO FIX THIS
        }

        public override string ToString()
        {
            string toString = "[EscapePodModel - Id: " + Id + " Location:" + Location + " FabricatorId: " + FabricatorId + " MedicalFabricatorGuid: " + MedicalFabricatorId + " StorageContainerGuid: " + StorageContainerId + " RadioGuid: " + RadioId + " AssignedPlayers: {";

            foreach (ushort playerId in AssignedPlayers)
            {
                toString += playerId + " ";
            }

            return toString + "} Damaged: " + Damaged + " RadioDamaged: " + RadioDamaged + "]";
        }
    }
}
