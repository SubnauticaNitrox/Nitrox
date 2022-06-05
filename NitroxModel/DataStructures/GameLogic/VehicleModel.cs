using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.Unity;
using NitroxModel.DataStructures.Util;
using NitroxModel.Serialization;

namespace NitroxModel.DataStructures.GameLogic;

[Serializable]
[JsonContractTransition]
public class VehicleModel
{
    [JsonMemberTransition]
    public NitroxTechType TechType { get; }

    [JsonMemberTransition]
    public NitroxId Id { get; set; }

    [JsonMemberTransition]
    public NitroxVector3 Position { get; set; }

    [JsonMemberTransition]
    public NitroxQuaternion Rotation { get; set; }

    [JsonMemberTransition]
    public ThreadSafeList<InteractiveChildObjectIdentifier> InteractiveChildIdentifiers { get; } = new();

    [JsonMemberTransition]
    public Optional<NitroxId> DockingBayId { get; set; }

    [JsonMemberTransition]
    public string Name { get; set; }

    [JsonMemberTransition]
    public NitroxVector3[] HSB { get; set; }

    [JsonMemberTransition]
    public float Health { get; set; }

    public VehicleModel(NitroxTechType techType, NitroxId id, NitroxVector3 position, NitroxQuaternion rotation, IEnumerable<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, Optional<NitroxId> dockingBayId, string name, NitroxVector3[] hsb, float health)
    {
        TechType = techType;
        Id = id;
        Position = position;
        Rotation = rotation;
        InteractiveChildIdentifiers = new ThreadSafeList<InteractiveChildObjectIdentifier>(interactiveChildIdentifiers);
        DockingBayId = dockingBayId;
        Name = name;
        HSB = hsb;
        Health = health;
    }

    public override string ToString()
    {
        return $"[VehicleModel - TechType: {TechType}, Id: {Id}, Position: {Position}, Rotation: {Rotation}, Name: {Name}, Health: {Health}, DockingBayId: {DockingBayId}]";
    }
}
