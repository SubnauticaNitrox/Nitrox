using System.Runtime.Serialization;
using System.Text;

namespace NitroxModel.DataStructures.GameLogic.Bases;

[DataContract]
public class BaseData
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

    public override string ToString()
    {
        StringBuilder builder = new();
        if (BaseShape != null)
        {
            builder.AppendLine($"BaseShape: [{string.Join(";", BaseShape)}]");
        }
        if (Faces != null)
        {
            builder.AppendLine($"Faces: {string.Join(", ", Faces)}");
        }
        if (Cells != null)
        {
            builder.AppendLine($"Cells: {string.Join(", ", Cells)}");
        }
        if (Links != null)
        {
            builder.AppendLine($"Links: {string.Join(", ", Links)}");
        }
        if (CellOffset != null)
        {
            builder.AppendLine($"CellOffset: [{string.Join(";", CellOffset)}]");
        }
        if (Masks != null)
        {
            builder.AppendLine($"Masks: {string.Join(", ", Masks)}");
        }
        if (IsGlass != null)
        {
            builder.AppendLine($"IsGlass: {string.Join(", ", IsGlass)}");
        }
        if (Anchor != null)
        {
            builder.AppendLine($"CellOffset: [{string.Join(";", Anchor)}]");
        }
        return builder.ToString();
    }
}
