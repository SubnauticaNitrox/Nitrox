using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Buildings.New;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using System.Collections.Generic;

namespace NitroxModel.Packets;

public sealed class PlaceGhost : Packet
{
    public GhostEntity GhostEntity;

    public PlaceGhost(GhostEntity ghostEntity)
    {
        GhostEntity = ghostEntity;
    }

    public override string ToString()
    {
        return $"PlaceGhost [GhostEntity: {GhostEntity}]";
    }
}

public sealed class PlaceModule : Packet
{
    public ModuleEntity ModuleEntity;

    public PlaceModule(ModuleEntity moduleEntity)
    {
        ModuleEntity = moduleEntity;
    }

    public override string ToString()
    {
        return $"PlaceModule [ModuleEntity: {ModuleEntity}]";
    }
}

public sealed class ModifyConstructedAmount : Packet
{
    public NitroxId GhostId;
    public float ConstructedAmount;

    public ModifyConstructedAmount(NitroxId ghostId, float constructedAmount)
    {
        GhostId = ghostId;
        ConstructedAmount = constructedAmount;
    }

    public override string ToString()
    {
        return $"ModifyConstructedAmount [GhostId: {GhostId}, ConstructedAmount: {ConstructedAmount}]";
    }
}

public sealed class PlaceBase : Packet
{
    public NitroxId FormerGhostId;
    public BuildEntity BuildEntity;

    public PlaceBase(NitroxId formerGhostId, BuildEntity buildEntity)
    {
        FormerGhostId = formerGhostId;
        BuildEntity = buildEntity;
    }

    public override string ToString()
    {
        return $"PlaceBase [FormerGhostId: {FormerGhostId}, BuildEntity: {BuildEntity}]";
    }
}

public sealed class UpdateBase : Packet
{
    public NitroxId BaseId;
    public NitroxId FormerGhostId;
    public SavedBase SavedBase;
    public Entity BuiltPieceEntity;
    public Dictionary<NitroxId, NitroxBaseFace> UpdatedChildren;
    public Dictionary<NitroxId, NitroxInt3> UpdatedMoonpools;
    public Dictionary<NitroxId, NitroxInt3> UpdatedMapRooms;

    public UpdateBase(NitroxId baseId, NitroxId formerGhostId, SavedBase savedBase, Entity builtPieceEntity, Dictionary<NitroxId, NitroxBaseFace> updatedChildren, Dictionary<NitroxId, NitroxInt3> updatedMoonpools, Dictionary<NitroxId, NitroxInt3> updatedMapRooms)
    {
        BaseId = baseId;
        FormerGhostId = formerGhostId;
        SavedBase = savedBase;
        BuiltPieceEntity = builtPieceEntity;
        UpdatedChildren = updatedChildren;
        UpdatedMoonpools = updatedMoonpools;
        UpdatedMapRooms = updatedMapRooms;
    }

    public override string ToString()
    {
        return $"UpdateBase [BaseId: {BaseId}, FormerGhostId: {FormerGhostId}, SavedBase: {SavedBase}, BuiltPieceEntity: {BuiltPieceEntity}, UpdatedChildren: {UpdatedChildren.Count}, UpdatedMoonpools: {UpdatedMoonpools.Count}, UpdatedMapRooms: {UpdatedMapRooms.Count}]";
    }
}

public sealed class BaseDeconstructed : Packet
{
    public NitroxId FormerBaseId;
    public GhostEntity ReplacerGhost;

    public BaseDeconstructed(NitroxId formerBaseId, GhostEntity replacerGhost)
    {
        FormerBaseId = formerBaseId;
        ReplacerGhost = replacerGhost;
    }

    public override string ToString()
    {
        return $"BaseDeconstructed [FormerBaseId: {FormerBaseId}, ReplacerGhost: {ReplacerGhost}]";
    }
}

public class PieceDeconstructed : Packet
{
    public NitroxId BaseId;
    public NitroxId PieceId;
    public BuildPieceIdentifier BuildPieceIdentifier;
    public GhostEntity ReplacerGhost;
    public SavedBase SavedBase;

    public PieceDeconstructed(NitroxId baseId, NitroxId pieceId, BuildPieceIdentifier buildPieceIdentifier, GhostEntity replacerGhost, SavedBase savedBase)
    {
        BaseId = baseId;
        PieceId = pieceId;
        BuildPieceIdentifier = buildPieceIdentifier;
        ReplacerGhost = replacerGhost;
        SavedBase = savedBase;
    }

    public override string ToString()
    {
        return $"PieceDeconstructed [BaseId: {BaseId}, PieceId: {PieceId}, PieceIdentifier: {BuildPieceIdentifier}, ReplacerGhost: {ReplacerGhost}]";
    }
}

public sealed class WaterParkDeconstructed : PieceDeconstructed
{
    public InteriorPieceEntity NewWaterPark;

    public WaterParkDeconstructed(NitroxId baseId, NitroxId pieceId, BuildPieceIdentifier buildPieceIdentifier, GhostEntity replacerGhost, SavedBase savedBase, InteriorPieceEntity newWaterPark) : base(baseId, pieceId, buildPieceIdentifier, replacerGhost, savedBase)
    {
        NewWaterPark = newWaterPark;
    }

    public override string ToString()
    {
        return $"WaterParkDeconstructed [{base.ToString()}, NewWaterPark: {NewWaterPark}]";
    }
}
