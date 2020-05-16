using System;
using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using ProtoBufNet;
using UnityEngine;
using NitroxModel.DataStructures;

namespace NitroxModel_Subnautica.DataStructures.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class NeptuneRocketModel : VehicleModel
    {
        [ProtoMember(1)]
        public int CurrentRocketStage { get; set; }

        [ProtoMember(2)]
        public float ElevatorPosition { get; set; }

        [ProtoMember(3)]
        public bool ElevatorUp { get; set; }

        public NeptuneRocketModel()
        {
            //For serialization purposes
        }

        public NeptuneRocketModel(NitroxModel.DataStructures.TechType techType, NitroxId id, Vector3 position, Quaternion rotation, List<InteractiveChildObjectIdentifier> interactiveChildIdentifiers, Optional<NitroxId> dockingBayId, string name, Vector3[] hsb, float health)
            : base(techType, id, position, rotation, interactiveChildIdentifiers, dockingBayId, name, hsb, health)
        {
            CurrentRocketStage = 0;
            ElevatorPosition = 0f;
            ElevatorUp = false;
        }

        public void IncrementStage()
        {
            CurrentRocketStage += 1;
        }

        public override string ToString()
        {
            return $"[NeptuneRocketModel : {base.ToString()}, CurrentRocketStage: {CurrentRocketStage}, ElevatorUp: {ElevatorUp}, ElevatorPosition: {ElevatorPosition}]";
        }
    }
}
