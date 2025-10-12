using System;
using System.Collections.Generic;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class PinnedRecipeMoved : Packet
{
    public List<int> RecipePins { get; }
    
    public PinnedRecipeMoved(List<int> recipePins)
    {
        RecipePins = recipePins;
    }
}
