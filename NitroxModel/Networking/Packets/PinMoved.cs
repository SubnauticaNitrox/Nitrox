using System;
using System.Collections.Generic;

namespace NitroxModel.Networking.Packets;

[Serializable]
public record PinnedRecipeMoved : Packet
{
    public List<int> RecipePins { get; }
    
    public PinnedRecipeMoved(List<int> recipePins)
    {
        RecipePins = recipePins;
    }
}
