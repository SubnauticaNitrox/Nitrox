﻿using System;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets;

[Serializable]
public class FMODCustomEmitterPacket : Packet
{
    public NitroxId Id { get; }
    public string AssetPath { get; }
    public bool Play { get; }

    public FMODCustomEmitterPacket(NitroxId id, string assetPath, bool play)
    {
        Id = id;
        AssetPath = assetPath;
        Play = play;
    }
}
