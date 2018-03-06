using UnityEngine;

namespace NitroxModel.MultiplayerSession
{
    public static class RandomColorGenerator
    {
        private static readonly System.Random random = new System.Random();

        public static Color GenerateColor()
        {
            int r = random.Next();
            return new Color32((byte)r, (byte)(r >> 8), (byte)(r >> 16), 255);
        }
    }
}
