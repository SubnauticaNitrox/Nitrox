using System;
using NitroxModel.DataStructures;
using NitroxModel.Packets;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class RocketStageUpdate : Packet
    {
        public NitroxId Id { get; }
        public int NewStage { get; }
        public TechType CurrentStageTech { get; }

        public RocketStageUpdate(NitroxId id, int newStage, TechType currentStageTech)
        {
            Id = id;
            NewStage = newStage;
            CurrentStageTech = currentStageTech;
        }

        public override string ToString()
        {
            return $"[RocketStageUpdate - Id: {Id}, NewRocketStage: {NewStage}, CurrentStageTech: {CurrentStageTech}]";
        }
    }
}
