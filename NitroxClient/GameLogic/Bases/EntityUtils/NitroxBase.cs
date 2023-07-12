using NitroxClient.GameLogic.Helper;
using NitroxModel.DataStructures.GameLogic.Bases;
using NitroxModel_Subnautica.DataStructures;
using System;
using System.Text;

namespace NitroxClient.GameLogic.Bases.EntityUtils;

public static class NitroxBase
{
    public static SavedBase From(Base targetBase)
    {
        Func<byte[], byte[]> c = SerializationHelper.CompressBytes;

        SavedBase savedBase = new();
        if (targetBase.baseShape != null)
        {
            savedBase.BaseShape = targetBase.baseShape.ToInt3().ToDto();
        }
        if (targetBase.faces != null)
        {
            savedBase.Faces = c(Array.ConvertAll(targetBase.faces, faceType => (byte)faceType));
        }
        if (targetBase.cells != null)
        {
            savedBase.Cells = c(Array.ConvertAll(targetBase.cells, cellType => (byte)cellType));
        }
        if (targetBase.links != null)
        {
            savedBase.Links = c(targetBase.links);
            savedBase.PrecompressionSize = targetBase.links.Length;
        }
        if (targetBase.cellOffset != null)
        {
            savedBase.CellOffset = targetBase.cellOffset.ToDto();
        }
        if (targetBase.masks != null)
        {
            savedBase.Masks = c(targetBase.masks);
        }
        if (targetBase.isGlass != null)
        {
            savedBase.IsGlass = c(Array.ConvertAll(targetBase.isGlass, isGlass => isGlass ? (byte)1 : (byte)0));
        }
        if (targetBase.anchor != null)
        {
            savedBase.Anchor = targetBase.anchor.ToDto();
        }
        return savedBase;
    }

    public static void ApplyTo(this SavedBase savedBase, Base @base)
    {

        Func<byte[], int, byte[]> d = SerializationHelper.DecompressBytes;
        int size = savedBase.PrecompressionSize;

        if (savedBase.BaseShape != null)
        {
            @base.baseShape = new(); // Reset it so that the following instruction is understood as a change
            @base.SetSize(savedBase.BaseShape.ToUnity());
        }
        if (savedBase.Faces != null)
        {
            @base.faces = Array.ConvertAll(d(savedBase.Faces, size * 6), faceType => (Base.FaceType)faceType);
        }
        if (savedBase.Cells != null)
        {
            @base.cells = Array.ConvertAll(d(savedBase.Cells, size), cellType => (Base.CellType)cellType);
        }
        if (savedBase.Links != null)
        {
            @base.links = d(savedBase.Links, size);
        }
        if (savedBase.CellOffset != null)
        {
            @base.cellOffset = new(savedBase.CellOffset.ToUnity());
        }
        if (savedBase.Masks != null)
        {
            @base.masks = d(savedBase.Masks, size);
        }
        if (savedBase.IsGlass != null)
        {
            @base.isGlass = Array.ConvertAll(d(savedBase.IsGlass, size), num => num == 1);
        }
        if (savedBase.Anchor != null)
        {
            @base.anchor = new(savedBase.Anchor.ToUnity());
        }
    }

    public static string ToString(this SavedBase savedBase)
    {
        StringBuilder builder = new();
        if (savedBase.BaseShape != null)
        {
            builder.AppendLine($"BaseShape: [{string.Join(";", savedBase.BaseShape)}]");
        }
        if (savedBase.Faces != null)
        {
            builder.AppendLine($"Faces: {string.Join(", ", savedBase.Faces)}");
        }
        if (savedBase.Cells != null)
        {
            builder.AppendLine($"Cells: {string.Join(", ", savedBase.Cells)}");
        }
        if (savedBase.Links != null)
        {
            builder.AppendLine($"Links: {string.Join(", ", savedBase.Links)}");
        }
        if (savedBase.CellOffset != null)
        {
            builder.AppendLine($"CellOffset: [{string.Join(";", savedBase.CellOffset)}]");
        }
        if (savedBase.Masks != null)
        {
            builder.AppendLine($"Masks: {string.Join(", ", savedBase.Masks)}");
        }
        if (savedBase.IsGlass != null)
        {
            builder.AppendLine($"IsGlass: {string.Join(", ", savedBase.IsGlass)}");
        }
        if (savedBase.Anchor != null)
        {
            builder.AppendLine($"CellOffset: [{string.Join(";", savedBase.Anchor)}]");
        }
        return builder.ToString();
    }
}
