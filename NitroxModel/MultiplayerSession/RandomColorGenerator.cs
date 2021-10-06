using NitroxModel.DataStructures.Unity;

namespace NitroxModel.MultiplayerSession
{
    public static class RandomColorGenerator
    {
        private static readonly System.Random random = new System.Random();

        public static NitroxColor GenerateColor()
        {
            int r = random.Next();
            return new NitroxColor((byte)r / 255f, (byte)(r >> 8) / 255f, (byte)(r >> 16) / 255f, 1f);
        }
    }
}
