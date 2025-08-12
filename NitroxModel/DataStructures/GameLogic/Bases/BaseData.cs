using System;
using System.Linq;
using System.Runtime.Serialization;

namespace NitroxModel.DataStructures.GameLogic.Bases;

[DataContract]
public class BaseData : IEquatable<BaseData>
{
    [DataMember(Order = 1)]
    public NitroxInt3 BaseShape;

    [DataMember(Order = 2)]
    public NitroxInt3 CellOffset;

    [DataMember(Order = 3)]
    public NitroxInt3 Anchor;

    [DataMember(Order = 4)]
    public int PreCompressionSize;

    [DataMember(Order = 5)]
    public byte[] Faces;

    [DataMember(Order = 6)]
    public byte[] Cells;

    [DataMember(Order = 7)]
    public byte[] Links;

    [DataMember(Order = 8)]
    public byte[] Masks;

    [DataMember(Order = 9)]
    public byte[] IsGlass;

    public bool Equals(BaseData other)
    {
        if (ReferenceEquals(null, other))
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return BaseShape.Equals(other.BaseShape) &&
               CellOffset.Equals(other.CellOffset) &&
               Anchor.Equals(other.Anchor) &&
               PreCompressionSize == other.PreCompressionSize &&
               Faces.SequenceEqual(other.Faces) &&
               Cells.SequenceEqual(other.Cells) &&
               Links.SequenceEqual(other.Links) &&
               Masks.SequenceEqualOrBothNull(other.Masks) &&
               IsGlass.SequenceEqual(other.IsGlass);
    }

    public override bool Equals(object obj)
    {
        if (ReferenceEquals(null, obj)) return false;
        if (ReferenceEquals(this, obj)) return true;
        if (obj.GetType() != this.GetType()) return false;
        return Equals((BaseData)obj);
    }

    public override int GetHashCode()
    {
        unchecked
        {
            int hashCode = BaseShape.GetHashCode();
            hashCode = (hashCode * 397) ^ CellOffset.GetHashCode();
            hashCode = (hashCode * 397) ^ Anchor.GetHashCode();
            hashCode = (hashCode * 397) ^ PreCompressionSize;
            hashCode = (hashCode * 397) ^ (Faces != null ? Faces.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (Cells != null ? Cells.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (Links != null ? Links.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (Masks != null ? Masks.GetHashCode() : 0);
            hashCode = (hashCode * 397) ^ (IsGlass != null ? IsGlass.GetHashCode() : 0);
            return hashCode;
        }
    }
}
