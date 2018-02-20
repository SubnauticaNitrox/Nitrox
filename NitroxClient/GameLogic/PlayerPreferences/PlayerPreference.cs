using UnityEngine;

namespace NitroxClient.GameLogic.PlayerPreferences
{
    public class PlayerPreference
    {
        public string PlayerName { get; set; }
        public Color PlayerColor { get; set; }

        public PlayerPreference Clone()
        {
            return new PlayerPreference()
            {
                PlayerColor = PlayerColor,
                PlayerName = PlayerName
            };
        }
    }
}
