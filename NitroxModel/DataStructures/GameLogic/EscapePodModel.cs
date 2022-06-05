using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable]
[JsonContractTransition]
public class EscapePodModel
{
    public const int PLAYERS_PER_ESCAPE_POD = 50;

    [JsonMemberTransition]
    public NitroxId Id { get; set; }

    [JsonMemberTransition]
    public NitroxVector3 Location { get; set; }

    [JsonMemberTransition]
    public NitroxId FabricatorId { get; set; }

    [JsonMemberTransition]
    public NitroxId MedicalFabricatorId { get; set; }

    [JsonMemberTransition]
    public NitroxId StorageContainerId { get; set; }

    [JsonMemberTransition]
    public NitroxId RadioId { get; set; }

    [JsonMemberTransition]
    public List<ushort> AssignedPlayers { get; set; } = new();

    [JsonMemberTransition]
    public bool Damaged { get; set; }

    [JsonMemberTransition]
    public bool RadioDamaged { get; set; }

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
        return false; //AssignedPlayers.Count >= PLAYERS_PER_ESCAPE_POD; // TODO FIX THIS
    }

    public override string ToString()
    {
        return $"[EscapePodModel - Id: {Id}, Location:{Location}, FabricatorId: {FabricatorId}, MedicalFabricatorGuid: {MedicalFabricatorId}, StorageContainerGuid: {StorageContainerId}, RadioGuid: {RadioId}, AssignedPlayers: {string.Join(" ", AssignedPlayers)},  Damaged: {Damaged}, RadioDamaged: {RadioDamaged}]";
    }
}
