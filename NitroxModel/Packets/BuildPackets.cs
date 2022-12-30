using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.New;

namespace NitroxModel.Packets;

public sealed class PlaceGhost : Packet
{
    public NitroxId ParentId;
    public SavedGhost SavedGhost;

    public PlaceGhost(NitroxId parentId, SavedGhost savedGhost)
    {
        ParentId = parentId;
        SavedGhost = savedGhost;
    }

    public override string ToString()
    {
        return $"PlaceGhost [ParentId: {ParentId}, SavedGhost: {SavedGhost}]";
    }
}

public sealed class PlaceModule : Packet
{
    public NitroxId ParentId;
    public SavedModule SavedModule;

    public PlaceModule(NitroxId parentId, SavedModule savedModule)
    {
        ParentId = parentId;
        SavedModule = savedModule;
    }

    public override string ToString()
    {
        return $"PlaceModule [ParentId: {ParentId}, SavedGhost: {SavedModule}]";
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

public class PlaceBase : Packet
{
    public NitroxId FormerGhostId;
    public SavedBuild SavedBuild;

    public PlaceBase(NitroxId formerGhostId, SavedBuild savedBuild)
    {
        FormerGhostId = formerGhostId;
        SavedBuild = savedBuild;
    }

    public override string ToString()
    {
        return $"PlaceBase [FormerGhostId: {FormerGhostId}, SavedBuild: {SavedBuild}]";
    }
}

public sealed class UpdateBase : PlaceBase
{
    public NitroxId BaseId;

    public UpdateBase(NitroxId baseId, NitroxId formerGhostId, SavedBuild savedBuild) : base(formerGhostId, savedBuild)
    {
        BaseId = baseId;
    }

    public override string ToString()
    {
        return $"UpdateBase [BaseId: {BaseId}, FormerGhostId: {FormerGhostId}, SavedBuild: {SavedBuild}]";
    }
}

public sealed class BaseDeconstructed : Packet
{
    public NitroxId FormerBaseId;
    public SavedGhost ReplacerGhost;

    public BaseDeconstructed(NitroxId formerBaseId, SavedGhost replacerGhost)
    {
        FormerBaseId = formerBaseId;
        ReplacerGhost = replacerGhost;
    }

    public override string ToString()
    {
        return $"BaseDeconstructed [FormerBaseId: {FormerBaseId}, ReplacerGhost: {ReplacerGhost}]";
    }
}

public sealed class PieceDeconstructed : Packet
{
    public NitroxId BaseId;
    public NitroxId PieceId;
    public BuildPieceIdentifier BuildPieceIdentifier;
    public SavedGhost ReplacerGhost;

    public PieceDeconstructed(NitroxId baseId, NitroxId pieceId, BuildPieceIdentifier buildPieceIdentifier, SavedGhost replacerGhost)
    {
        BaseId = baseId;
        PieceId = pieceId;
        BuildPieceIdentifier = buildPieceIdentifier;
        ReplacerGhost = replacerGhost;
    }

    public override string ToString()
    {
        return $"PieceDeconstructed [BaseId: {BaseId}, PieceId: {PieceId}, PieceIdentifier: {BuildPieceIdentifier}, ReplacerGhost: {ReplacerGhost}]";
    }
}
