using System;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Packets;

namespace Nitrox.Model.Subnautica.Packets
{
    /// <summary>
    /// Triggered when a generic fire has been created (not node-based).
    /// </summary>
    [Serializable]
    public class FireCreated : Packet
    {
        public GenericFireData FireCreatedData { get; }

        public FireCreated(NitroxId id, NitroxId parentId, NitroxVector3 position, NitroxQuaternion rotation)
        {
            FireCreatedData = new GenericFireData(id, parentId, position, rotation);
        }

        public FireCreated(GenericFireData fireCreatedData)
        {
            FireCreatedData = fireCreatedData;
        }

        public override string ToString()
        {
            return $"[FireCreated - {FireCreatedData.FireId}]";
        }
    }
}
