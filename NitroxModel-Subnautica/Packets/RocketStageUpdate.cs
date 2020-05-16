using System;
using NitroxModel.Packets;
using NitroxModel.DataStructures;

namespace NitroxModel_Subnautica.Packets
{
    [Serializable]
    public class RocketStageUpdate : Packet
    {
        public NitroxId Id { get; }
        public int NewRocketStage { get; }
        public NitroxId ConstructorId { get; }
        public TechType CurrentStageTech { get; }
        public byte[] SerializedBuiltGameObject { get; }

        public RocketStageUpdate(NitroxId id, NitroxId constructorId, int newRocketStage, TechType currentStageTech, byte[] serializedBuiltGameObject)
        {
            Id = id;
            ConstructorId = constructorId;
            NewRocketStage = newRocketStage;
            CurrentStageTech = currentStageTech;
            SerializedBuiltGameObject = serializedBuiltGameObject;
        }

        public override string ToString()
        {
            return $"[RocketStageUpdate - Id: {Id}, ConstructorId: {ConstructorId}, NewRocketStage: {NewRocketStage}, CurrentStageTech: {CurrentStageTech}]";
        }
    }
}
