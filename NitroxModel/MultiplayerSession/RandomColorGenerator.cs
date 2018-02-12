using System;
using UnityEngine;
using Random = System.Random;

namespace NitroxModel.MultiplayerSession
{
    public static class RandomColorGenerator
    {
        public static Color GenerateColor()
        {
            Random rand = new Random(Guid.NewGuid().ToString().GetHashCode());
            return new Color(rand.Next(256), rand.Next(256), rand.Next(256));
        }
    }
}
