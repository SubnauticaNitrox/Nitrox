using System;
using System.Runtime.Serialization;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata.Bases;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [Serializable]
    [DataContract]
    [ProtoInclude(50, typeof(KeypadMetadata))]
    [ProtoInclude(60, typeof(SealedDoorMetadata))]
    [ProtoInclude(70, typeof(PrecursorDoorwayMetadata))]
    [ProtoInclude(80, typeof(PrecursorTeleporterMetadata))]
    [ProtoInclude(90, typeof(PrecursorKeyTerminalMetadata))]
    [ProtoInclude(100, typeof(PrecursorTeleporterActivationTerminalMetadata))]
    [ProtoInclude(110, typeof(StarshipDoorMetadata))]
    [ProtoInclude(120, typeof(WeldableWallPanelGenericMetadata))]
    [ProtoInclude(130, typeof(IncubatorMetadata))]
    [ProtoInclude(140, typeof(EntitySignMetadata))]
    [ProtoInclude(150, typeof(ConstructorMetadata))]
    [ProtoInclude(160, typeof(FlashlightMetadata))]
    [ProtoInclude(170, typeof(BatteryMetadata))]
    [ProtoInclude(180, typeof(RepairedComponentMetadata))]
    [ProtoInclude(190, typeof(CrafterMetadata))]
    [ProtoInclude(200, typeof(PlantableMetadata))]
    [ProtoInclude(210, typeof(CyclopsMetadata))]
    [ProtoInclude(220, typeof(SeamothMetadata))]
    [ProtoInclude(230, typeof(SubNameInputMetadata))]
    [ProtoInclude(240, typeof(RocketMetadata))]
    [ProtoInclude(250, typeof(CyclopsLightingMetadata))]
    [ProtoInclude(260, typeof(FireExtinguisherHolderMetadata))]
    [ProtoInclude(270, typeof(PlayerMetadata))]
    [ProtoInclude(280, typeof(GhostMetadata))]
    [ProtoInclude(290, typeof(WaterParkCreatureMetadata))]
    public abstract class EntityMetadata
    {
    }
}
