using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayerJoin : Authenticate
    {
        public Color PlayerColor { get; }

        public PlayerJoin(string playerId, Color playerColor) : base(playerId)
        {
            PlayerColor = playerColor;
        }

        public override string ToString()
        {
            return "[PlayerJoin - PlayerId: " + PlayerId + " PlayerColor: " + PlayerColor + "]";
        }
    }
}
