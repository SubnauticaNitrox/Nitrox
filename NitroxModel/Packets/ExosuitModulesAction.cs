using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ExosuitModulesAction : Packet
    {
        public TorpedoType TorpedoType { get; }
        public Transform SiloTransform { get; }
        public string Guid { get; }
        public Vector3 Forward { get; }
        public Quaternion Rotation { get; }

        public ExosuitModulesAction(TorpedoType torpedoType, Transform siloTransform, string guid, Vector3 forward, Quaternion rotation)
        {
            TorpedoType = torpedoType;
            SiloTransform = siloTransform;
            Guid = guid;
            Forward = forward;
            Rotation = rotation;
        }
        public override string ToString()
        {
            return "[ExosuitModulesAction - TorpedoType: " + TorpedoType + " siloTransform: " + SiloTransform + " Guid:" + Guid + " Forward: " + Forward + " Rotation: " + Rotation + "]";
        }
    }
}
