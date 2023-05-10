using System;
using System.Runtime.Serialization;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities;

[Serializable, DataContract]

[ProtoInclude(141, typeof(EscapePodWorldEntity))]
[ProtoInclude(142, typeof(PlayerWorldEntity))]
[ProtoInclude(143, typeof(VehicleWorldEntity))]
[ProtoInclude(144, typeof(BuildEntity))]
[ProtoInclude(145, typeof(ModuleEntity))]
[ProtoInclude(146, typeof(GhostEntity))]
[ProtoInclude(147, typeof(InteriorPieceEntity))]
[ProtoInclude(148, typeof(MoonpoolEntity))]
[ProtoInclude(149, typeof(WaterParkCreatureEntity))]
public class GlobalRootEntity : WorldEntity
{
    public static GlobalRootEntity From(WorldEntity worldEntity)
    {
        return new()
        {
            Transform = worldEntity.Transform,
            Level = worldEntity.Level,
            ClassId = worldEntity.ClassId,
            SpawnedByServer = worldEntity.SpawnedByServer,
            Id = worldEntity.Id,
            TechType = worldEntity.TechType,
            Metadata = worldEntity.Metadata,
            ParentId = worldEntity.ParentId,
            ChildEntities = worldEntity.ChildEntities
        };
    }
}
