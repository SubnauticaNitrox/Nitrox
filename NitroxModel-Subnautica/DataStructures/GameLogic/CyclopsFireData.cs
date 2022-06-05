using System;
using NitroxModel.DataStructures;
using NitroxModel.Serialization;

namespace NitroxModel_Subnautica.DataStructures.GameLogic;

[Serializable]
[JsonContractTransition]
public class CyclopsFireData
{
    [JsonMemberTransition]
    public NitroxId FireId { get; set; }

    [JsonMemberTransition]
    public NitroxId CyclopsId { get; set; }

    [JsonMemberTransition]
    public CyclopsRooms Room { get; set; }

    [JsonMemberTransition]
    public int NodeIndex { get; set; }

    public CyclopsFireData(NitroxId fireId, NitroxId cyclopsId, CyclopsRooms room, int nodeIndex)
    {
        FireId = fireId;
        CyclopsId = cyclopsId;
        Room = room;
        NodeIndex = nodeIndex;
    }

    public override string ToString()
    {
        return $"[CyclopsFireData - FireId: {FireId}, CyclopsId: {CyclopsId}, Room: {Room}, NodeIndex: {NodeIndex}]";
    }
}
