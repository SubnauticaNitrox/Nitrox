using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using BinaryPack.Attributes;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Entities
{
    /*
     * A world entity is an object physically in the world with a transform.  It is either a global root entity
     * or something that phases out with the clip map manager.
     */
    [Serializable]
    [DataContract]
    [ProtoInclude(50, typeof(PlaceholderGroupWorldEntity))]
    [ProtoInclude(51, typeof(CellRootEntity))]
    [ProtoInclude(52, typeof(GlobalRootEntity))]
    public class WorldEntity : Entity
    {
        public AbsoluteEntityCell AbsoluteEntityCell => new AbsoluteEntityCell(Transform.Position, Level);

        [DataMember(Order = 1)]
        public NitroxTransform Transform { get; set; }

        [DataMember(Order = 2)]
        public int Level { get; set; }

        /// <summary>
        ///     Gets the prefab class id assigned by Unity Engine. This is a unique <see cref="Guid"/>. 
        /// </summary>
        /// <remarks>
        ///     <a href="https://docs.unity3d.com/Manual/Prefabs.html">What is a prefab?</a>
        /// </remarks>
        [DataMember(Order = 3)]
        public string ClassId { get; set; }

        /// <summary>
        ///     Keeps track if an entity was spawned by the server or a player
        ///     Server-spawned entities need to be techType white-listed to be simulated
        /// </summary>
        [DataMember(Order = 4)]
        public bool SpawnedByServer;
        
        [IgnoreConstructor]
        protected WorldEntity()
        {
            // Constructor for serialization. Has to be "protected" for json serialization.
        }

        public WorldEntity(NitroxVector3 localPosition, NitroxQuaternion localRotation, NitroxVector3 scale, NitroxTechType techType, int level, string classId, bool spawnedByServer, NitroxId id, Entity parentEntity)
        {
            Transform = new NitroxTransform(localPosition, localRotation, scale);
            TechType = techType;
            Id = id;
            Level = level;
            ClassId = classId;
            SpawnedByServer = spawnedByServer;

            if (parentEntity != null)
            {
                ParentId = parentEntity.Id;

                if (parentEntity is WorldEntity weParent)
                {
                    Transform.SetParent(weParent.Transform, false);
                }
            }
        }

        /// <remarks>Used for deserialization</remarks>
        public WorldEntity(NitroxTransform transform, int level, string classId, bool spawnedByServer, NitroxId id, NitroxTechType techType, EntityMetadata metadata, NitroxId parentId, List<Entity> childEntities)
        {
            Id = id;
            TechType = techType;
            Metadata = metadata;
            ParentId = parentId;
            Transform = transform;
            ChildEntities = childEntities;
            Level = level;
            ClassId = classId;
            SpawnedByServer = spawnedByServer;
        }

        public override string ToString()
        {
            return $"[WorldEntity Transform: {Transform} Level: {Level} ClassId: {ClassId} SpawnedByServer: {SpawnedByServer} {base.ToString()}]";
        }
    }
}
