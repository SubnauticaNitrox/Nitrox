using System;
using NitroxModel.Packets;
using NitroxModel.DataStructures;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class RocketStageUpdate : Packet
    {
        public NitroxId Id { get; }
        public int NewStage { get; }
        public NitroxId ConstructorId { get; }
        public TechType CurrentStageTech { get; }
        public byte[] SerializedGameObject { get; }

        public RocketStageUpdate(NitroxId id, NitroxId constructorId, int newStage, TechType currentStageTech, byte[] serializedGameObject)
        {
            Id = id;
            ConstructorId = constructorId;
            NewStage = newStage;
            CurrentStageTech = currentStageTech;
            SerializedGameObject = serializedGameObject;
        }

        public override string ToString()
        {
            return $"[RocketStageUpdate - Id: {Id}, ConstructorId: {ConstructorId}, NewRocketStage: {NewStage}, CurrentStageTech: {CurrentStageTech}]";
        }
    }
}
