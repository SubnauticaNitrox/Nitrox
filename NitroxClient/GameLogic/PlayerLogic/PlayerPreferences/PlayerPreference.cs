using System;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerPreferences
{
    [Serializable]
    public class PlayerPreference : IEquatable<PlayerPreference>
    {
        public string PlayerName { get; private set; }
        public float RedAdditive { get; private set; }
        public float GreenAdditive { get; private set; }
        public float BlueAdditive { get; private set; }

        public PlayerPreference()
        {
        }

        public PlayerPreference(Color playerColor)
        {
            RedAdditive = playerColor.r;
            GreenAdditive = playerColor.g;
            BlueAdditive = playerColor.b;
        }

        public PlayerPreference(string playerName, Color playerColor)
        {
            PlayerName = playerName;
            RedAdditive = playerColor.r;
            GreenAdditive = playerColor.g;
            BlueAdditive = playerColor.b;
        }

        public bool Equals(PlayerPreference other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return string.Equals(PlayerName, other.PlayerName) && RedAdditive.Equals(other.RedAdditive) && GreenAdditive.Equals(other.GreenAdditive) && BlueAdditive.Equals(other.BlueAdditive);
        }

        public PlayerPreference Clone()
        {
            return new PlayerPreference
            {
                PlayerName = PlayerName,
                RedAdditive = RedAdditive,
                GreenAdditive = GreenAdditive,
                BlueAdditive = BlueAdditive
            };
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType() != GetType())
            {
                return false;
            }

            return Equals((PlayerPreference)obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                int hashCode = PlayerName != null ? PlayerName.GetHashCode() : 0;
                hashCode = (hashCode * 397) ^ RedAdditive.GetHashCode();
                hashCode = (hashCode * 397) ^ GreenAdditive.GetHashCode();
                hashCode = (hashCode * 397) ^ BlueAdditive.GetHashCode();
                return hashCode;
            }
        }
    }

    //LitJson does not seem to be capable of ignoring certain properties.
    public static class PlayerPreferenceExtensions
    {
        public static Color PreferredColor(this PlayerPreference playerPreference)
        {
            return new Color(playerPreference.RedAdditive, playerPreference.GreenAdditive, playerPreference.BlueAdditive);
        }
    }
}
