using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Unity;

namespace NitroxServer.GameLogic.Entities.Spawning;

public class EntitySpawnPoint
{
    // Fields from EntitySlotData
    public string BiomeType { get; }
    public List<string> AllowedTypes { get; }
    public float Density { get; }
    public NitroxVector3 LocalPosition { get; set; }
    public NitroxQuaternion LocalRotation { get; set; }
    
    public readonly List<EntitySpawnPoint> Children = new List<EntitySpawnPoint>();
    public AbsoluteEntityCell AbsoluteEntityCell { get; }
    public NitroxVector3 Scale { get; protected set; }
    public string ClassId { get; }
    public bool CanSpawnCreature { get; private set; }
    public EntitySpawnPoint Parent { get; set; }

    public EntitySpawnPoint(AbsoluteEntityCell absoluteEntityCell, NitroxVector3 localPosition, NitroxQuaternion localRotation, List<string> allowedTypes, float density, string biomeType)
    {
        AbsoluteEntityCell = absoluteEntityCell;
        LocalPosition = localPosition;
        LocalRotation = localRotation;
        BiomeType = biomeType;
        Density = density;
        AllowedTypes = allowedTypes;
    }

    public EntitySpawnPoint(AbsoluteEntityCell absoluteEntityCell, NitroxVector3 localPosition, NitroxQuaternion localRotation, NitroxVector3 scale, string classId)
    {
        AbsoluteEntityCell = absoluteEntityCell;
        ClassId = classId;
        Density = 1;
        LocalPosition = localPosition;
        Scale = scale;
        LocalRotation = localRotation;
    }

    public override string ToString() => $"[EntitySpawnPoint - {AbsoluteEntityCell}, Local Position: {LocalPosition}, Local Rotation: {LocalRotation}, Scale: {Scale}, Class Id: {ClassId}, Biome Type: {BiomeType}, Density: {Density}, Can Spawn Creature: {CanSpawnCreature}]";
}
