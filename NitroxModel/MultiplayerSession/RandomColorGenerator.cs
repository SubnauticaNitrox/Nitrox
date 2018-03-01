using UnityEngine;

namespace NitroxModel.MultiplayerSession
{
    public static class RandomColorGenerator
    {
        private static readonly System.Random random = new System.Random();

        public static Color GenerateColor()
        {
            return new Color32(random.NextByte(), random.NextByte(), random.NextByte(), 255);
        }
    }
}
