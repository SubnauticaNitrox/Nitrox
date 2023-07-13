using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Bases;

[DataContract]
public class SavedBase
{
    [DataMember(Order = 1)]
    public NitroxInt3 BaseShape;

    [DataMember(Order = 2)]
    public byte[] Faces;

    [DataMember(Order = 3)]
    public byte[] Cells;

    [DataMember(Order = 4)]
    public byte[] Links;

    [DataMember(Order = 5)]
    public NitroxInt3 CellOffset;

    [DataMember(Order = 6)]
    public byte[] Masks;

    [DataMember(Order = 7)]
    public byte[] IsGlass;

    [DataMember(Order = 8)]
    public NitroxInt3 Anchor;

    [DataMember(Order = 9)]
    public int PrecompressionSize;
}

