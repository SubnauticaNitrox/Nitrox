using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic.Buildings.New.Metadata;
using NitroxModel.DataStructures.Unity;
using ProtoBufNet;

namespace NitroxModel.DataStructures.GameLogic.Buildings.New;

// TODO: Verify if LocalScale is necessary

[ProtoContract]
public class SavedBase
{
    [ProtoMember(1)]
    public NitroxInt3 BaseShape;

    [ProtoMember(2)]
    public int[] Faces;

    [ProtoMember(3)]
    public int[] Cells;

    [ProtoMember(4)]
    public byte[] Links;

    [ProtoMember(5)]
    public NitroxInt3 CellOffset;

    [ProtoMember(6)]
    public byte[] Masks;

    [ProtoMember(7)]
    public int[] IsGlass;

    [ProtoMember(8)]
    public NitroxInt3 Anchor;
}

[ProtoContract]
public class SavedBuild
{
    [ProtoMember(1)]
    public NitroxId NitroxId;

    [ProtoMember(2)]
    public NitroxVector3 Position;

    [ProtoMember(3)]
    public NitroxQuaternion Rotation;

    [ProtoMember(4)]
    public NitroxVector3 LocalScale;

    [ProtoMember(5)]
    public SavedBase Base;

    [ProtoMember(6)]
    public List<SavedInteriorPiece> InteriorPieces = new();

    [ProtoMember(7)]
    public List<SavedModule> Modules = new();

    [ProtoMember(8)]
    public List<SavedGhost> Ghosts = new();
}

[ProtoContract]
public class SavedGlobalRoot
{
    [ProtoMember(1)]
    public List<SavedBuild> Builds = new();
    
    [ProtoMember(2)]
    public List<SavedModule> Modules = new();
    
    [ProtoMember(3)]
    public List<SavedGhost> Ghosts = new();
}

[ProtoContract]
public class SavedInteriorPiece
{
    [ProtoMember(1)]
    public string ClassId;

    [ProtoMember(2)]
    public NitroxBaseFace BaseFace;

    [ProtoMember(3)]
    public float Constructed;
}

[ProtoContract]
public class SavedModule
{
    [ProtoMember(1)]
    public string ClassId;

    [ProtoMember(2)]
    public NitroxId NitroxId;

    [ProtoMember(3)]
    public NitroxVector3 Position;

    [ProtoMember(4)]
    public NitroxQuaternion Rotation;

    [ProtoMember(5)]
    public NitroxVector3 LocalScale;

    [ProtoMember(6)]
    public float ConstructedAmount;

    [ProtoMember(7)]
    public bool IsInside;
}

[ProtoContract]
public class SavedGhost : SavedModule
{
    [ProtoMember(1)]
    public NitroxBaseFace BaseFace;

    [ProtoMember(2)]
    public SavedBase Base;

    [ProtoMember(3)]
    public GhostMetadata Metadata;

    [ProtoMember(4)]
    public NitroxTechType TechType;
}
