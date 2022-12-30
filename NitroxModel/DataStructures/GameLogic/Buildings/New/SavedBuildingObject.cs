using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic.Buildings.New.Metadata;
using NitroxModel.DataStructures.Unity;

namespace NitroxModel.DataStructures.GameLogic.Buildings.New;

// TODO: Verify if LocalScale is necessary
public class SavedBase
{
    public NitroxInt3 BaseShape;
    public int[] Faces;
    public int[] Cells;
    public byte[] Links;
    public NitroxInt3 CellOffset;
    public byte[] Masks;
    public int[] IsGlass;
    public NitroxInt3 Anchor;
}

public class SavedBuild
{
    public NitroxId NitroxId;
    public NitroxVector3 Position;
    public NitroxQuaternion Rotation;
    public NitroxVector3 LocalScale;

    public SavedBase Base;

    public List<SavedInteriorPiece> InteriorPieces;
    public List<SavedModule> Modules;
    public List<SavedGhost> Ghosts;
}

public class SavedGlobalRoot
{
    public List<SavedBuild> Builds;
    public List<SavedModule> Modules;
    public List<SavedGhost> Ghosts;
}

public class SavedInteriorPiece
{
    public string ClassId;
    public NitroxBaseFace BaseFace;
    public float Constructed;
}

public class SavedModule
{
    public string ClassId;
    public NitroxId NitroxId;
    public NitroxVector3 Position;
    public NitroxQuaternion Rotation;
    public NitroxVector3 LocalScale;

    public float ConstructedAmount;
    public bool IsInside;
}

public class SavedGhost : SavedModule
{
    public NitroxBaseFace BaseFace;
    public SavedBase Base;
    public GhostMetadata Metadata;
    public NitroxTechType TechType;
}
