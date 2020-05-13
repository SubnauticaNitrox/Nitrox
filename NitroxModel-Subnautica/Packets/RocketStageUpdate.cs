using System;
using NitroxModel.Packets;
using NitroxModel.DataStructures;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class RocketStageUpdate : Packet
    {
        public NitroxId Id { get; }
        public TechType TechType { get; }
        public int NewStage { get; }

        public RocketStageUpdate(NitroxId id, TechType techType, int newStage)
        {
            Id = id;
            TechType = techType;
            NewStage = newStage;
        }

        public override string ToString()
        {
            return $"[RocketStageUpdate Id: {Id}, TechType: {TechType}, Stage: {NewStage}]";
        }
    }
}
