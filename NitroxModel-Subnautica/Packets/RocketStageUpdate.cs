using NitroxModel.DataStructures;
using NitroxModel.Packets;
using ZeroFormatter;

namespace NitroxModel_Subnautica.Packets
{
    [ZeroFormattable]
    public class RocketStageUpdate : Packet
    {
        [Index(0)]
        public virtual NitroxId Id { get; protected set; }
        [Index(1)]
        public virtual int NewStage { get; protected set; }
        [Index(2)]
        public virtual TechType CurrentStageTech { get; protected set; }

        private RocketStageUpdate() { }

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
