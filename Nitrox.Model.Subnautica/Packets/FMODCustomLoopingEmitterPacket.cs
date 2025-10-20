﻿using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class FMODCustomLoopingEmitterPacket : Packet
{
    public NitroxId Id { get; }
    public string AssetPath { get; }

    public FMODCustomLoopingEmitterPacket(NitroxId id, string assetPath)
    {
        Id = id;
        AssetPath = assetPath;
    }
}
