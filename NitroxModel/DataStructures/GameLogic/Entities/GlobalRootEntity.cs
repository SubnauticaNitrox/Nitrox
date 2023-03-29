using System;
using System.Runtime.Serialization;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities;

[Serializable, DataContract]
[ProtoInclude(10, typeof(BuildEntity))]
[ProtoInclude(20, typeof(ModuleEntity))]
[ProtoInclude(30, typeof(GhostEntity))]
[ProtoInclude(40, typeof(InteriorPieceEntity))]
[ProtoInclude(50, typeof(WorldEntity))]
public class GlobalRootEntity : Entity
{

}
