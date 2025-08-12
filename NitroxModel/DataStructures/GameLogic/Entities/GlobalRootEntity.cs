using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities;

[Serializable, DataContract]
[ProtoInclude(50, typeof(BuildEntity))]
#if SUBNAUTICA
[ProtoInclude(51, typeof(EscapePodWorldEntity))]
#endif
[ProtoInclude(52, typeof(InteriorPieceEntity))]
[ProtoInclude(53, typeof(MapRoomEntity))]
[ProtoInclude(54, typeof(ModuleEntity))]
[ProtoInclude(55, typeof(MoonpoolEntity))]
[ProtoInclude(56, typeof(PlanterEntity))]
[ProtoInclude(57, typeof(PlayerWorldEntity))]
[ProtoInclude(58, typeof(VehicleWorldEntity))]
[ProtoInclude(59, typeof(RadiationLeakEntity))]
public class GlobalRootEntity : WorldEntity
{
    [IgnoreConstructor]
    protected GlobalRootEntity()
    {
        // Constructor for serialization. Has to be "protected" for json serialization.
    }

    /// <remarks>Used for deserialization</remarks>
    public GlobalRootEntity(NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities) :
        base(transform, level, classId, spawnedByServer, id, techType, metadata, parentId, childEntities)
    { }
}
