using UnityEngine;

namespace NitroxModel.MultiplayerSession
{
    public static class RandomColorGenerator
    {
        public static Color GenerateColor()
        {
            return new Color32((byte)Random.Range(0, 255), (byte)Random.Range(0, 255), (byte)Random.Range(0, 255), 255);
        }
    }
}
