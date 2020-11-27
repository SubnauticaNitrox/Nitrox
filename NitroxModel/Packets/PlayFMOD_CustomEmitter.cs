using System;
using NitroxModel.DataStructures;

namespace NitroxModel.Packets
{
    [Serializable]
    public class PlayFMOD_CustomEmitter : Packet
    {
        public NitroxId Id { get; }
        public int ComponentId { get; }
        public bool Play { get; }

        public PlayFMOD_CustomEmitter(NitroxId id, int componentId, bool play)
        {
            Id = id;
            ComponentId = componentId;
            Play = play;
        }

        public override string ToString()
        {
            return $"[PlayFMOD_CustomEmitter - Id: {Id}, ComponentId: {ComponentId}, Play {Play}]";
        }
    }
}
