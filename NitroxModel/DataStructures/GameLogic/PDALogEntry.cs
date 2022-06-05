using System;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable]
[JsonContractTransition]
public class PDALogEntry
{
    [JsonMemberTransition]
    public string Key;

    [JsonMemberTransition]
    public float Timestamp;

    public PDALogEntry(string key, float timestamp)
    {
        Key = key;
        Timestamp = timestamp;
    }

    public override string ToString()
    {
        return $"[PDALogEntry - Key: {Key}, Timestamp: {Timestamp}]";
    }
}
