using System;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable]
[JsonContractTransition]
public class InteractiveChildObjectIdentifier
{
    [JsonMemberTransition]
    public NitroxId Id { get; set; }

    [JsonMemberTransition]
    public string GameObjectNamePath { get; set; }

    public InteractiveChildObjectIdentifier(NitroxId id, string gameObjectNamePath)
    {
        Id = id;
        GameObjectNamePath = gameObjectNamePath;
    }

    public override string ToString()
    {
        return $"[InteractiveChildObjectIdentifier - Id: {Id},GameObjectNamePath: {GameObjectNamePath}]";
    }
}
