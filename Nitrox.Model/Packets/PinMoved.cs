using System;
using System.Collections.Generic;

namespace Nitrox.Model.Packets;

[Serializable]
public class PinnedRecipeMoved : Packet
{
    public List<int> RecipePins { get; }
    
    public PinnedRecipeMoved(List<int> recipePins)
    {
        RecipePins = recipePins;
    }
}
