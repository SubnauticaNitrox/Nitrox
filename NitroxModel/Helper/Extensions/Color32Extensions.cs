using UnityEngine;

namespace NitroxModel.Helper.Extensions
{
    public static class Color32Extensions
    {
        public static string AsHexString(this Color32 color)
        {
            return $"#{color.r:X2}{color.g:X2}{color.b:X2}";
        }
    }
}
