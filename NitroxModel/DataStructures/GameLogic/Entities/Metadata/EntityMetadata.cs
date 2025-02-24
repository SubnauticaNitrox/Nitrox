using System;
using System.Runtime.Serialization;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata.Bases;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities.Metadata
{
    [Serializable]
    [DataContract]
    [ProtoInclude(50, typeof(KeypadMetadata))]
    [ProtoInclude(51, typeof(SealedDoorMetadata))]
    [ProtoInclude(52, typeof(PrecursorDoorwayMetadata))]
    [ProtoInclude(53, typeof(PrecursorTeleporterMetadata))]
    [ProtoInclude(54, typeof(PrecursorKeyTerminalMetadata))]
    [ProtoInclude(55, typeof(PrecursorTeleporterActivationTerminalMetadata))]
    [ProtoInclude(56, typeof(StarshipDoorMetadata))]
    [ProtoInclude(57, typeof(WeldableWallPanelGenericMetadata))]
    [ProtoInclude(58, typeof(IncubatorMetadata))]
    [ProtoInclude(59, typeof(EntitySignMetadata))]
    [ProtoInclude(60, typeof(ConstructorMetadata))]
    [ProtoInclude(61, typeof(FlashlightMetadata))]
    [ProtoInclude(62, typeof(BatteryMetadata))]
    [ProtoInclude(63, typeof(RepairedComponentMetadata))]
    [ProtoInclude(64, typeof(CrafterMetadata))]
    [ProtoInclude(65, typeof(PlantableMetadata))]
    [ProtoInclude(66, typeof(CyclopsMetadata))]
    [ProtoInclude(67, typeof(RocketMetadata))]
    [ProtoInclude(68, typeof(CyclopsLightingMetadata))]
    [ProtoInclude(69, typeof(FireExtinguisherHolderMetadata))]
    [ProtoInclude(70, typeof(PlayerMetadata))]
    [ProtoInclude(71, typeof(GhostMetadata))]
    [ProtoInclude(72, typeof(WaterParkCreatureMetadata))]
    [ProtoInclude(73, typeof(NamedColoredMetadata))]
    [ProtoInclude(74, typeof(BeaconMetadata))]
    [ProtoInclude(75, typeof(FlareMetadata))]
    [ProtoInclude(76, typeof(RadiationMetadata))]
    [ProtoInclude(77, typeof(CrashHomeMetadata))]
    [ProtoInclude(78, typeof(EatableMetadata))]
    [ProtoInclude(79, typeof(SeaTreaderMetadata))]
    [ProtoInclude(80, typeof(StayAtLeashPositionMetadata))]
    [ProtoInclude(81, typeof(EggMetadata))]
    public abstract class EntityMetadata
    {
    }
}
