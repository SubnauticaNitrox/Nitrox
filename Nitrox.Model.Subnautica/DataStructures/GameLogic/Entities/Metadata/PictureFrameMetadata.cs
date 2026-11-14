using System;
using System.Runtime.Serialization;
using BinaryPack.Attributes;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;

/// <summary>
/// metadata for a picture frame entity.
/// </summary>
[Serializable]
[DataContract]
public class PictureFrameMetadata : EntityMetadata
{
    /// <summary>
    /// SHA-256 hex digest of the synced JPEG bytes. Null/empty means no picture is set.
    /// </summary>
    [DataMember(Order = 1)]
    public string? ContentHash { get; }

    [IgnoreConstructor]
    protected PictureFrameMetadata()
    {
        // Just exists for serialisation
    }

    public PictureFrameMetadata(string? contentHash)
    {
        ContentHash = contentHash;
    }

    public override string ToString()
    {
        return $"[PictureFrameMetadata ContentHash: {ContentHash}]";
    }
}
