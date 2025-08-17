using System;

namespace NitroxModel.Extensions;

public static class UintExtensions
{
    public static string AsByteUnitText(this uint byteSize)
    {
        // Uint can't go past 4GiB, so we don't need to worry about overflow.
        string[] suf = { "B", "KiB", "MiB", "GiB" };
        if (byteSize == 0)
        {
            return $"0{suf[0]}";
        }
        int place = Convert.ToInt32(Math.Floor(Math.Log(byteSize, 1024)));
        double num = Math.Round(byteSize / Math.Pow(1024, place), 1);
        return num + suf[place];
    }
}
