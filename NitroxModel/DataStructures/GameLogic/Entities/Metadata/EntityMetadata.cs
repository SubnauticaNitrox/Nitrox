﻿using System;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [Serializable]
    [ProtoContract]
    [ProtoInclude(50, typeof(KeypadMetadata))]
    [ProtoInclude(60, typeof(SealedDoorMetadata))]
    [ProtoInclude(70, typeof(PrecursorDoorwayMetadata))]
    [ProtoInclude(80, typeof(PrecursorTeleporterMetadata))]
    [ProtoInclude(90, typeof(PrecursorKeyTerminalMetadata))]
    [ProtoInclude(100, typeof(PrecursorTeleporterActivationTerminalMetadata))]
    public abstract class EntityMetadata
    {
    }
}
