using System;
using UnityEngine;

namespace NitroxModel.Packets
{
    [Serializable]
    public class ExosuitSpawnedArmAction : Packet
    {
        public string ExoGuid { get; }
        public string LeftArmGuid { get; }
        public string RightArmGuid { get; }

        public ExosuitSpawnedArmAction(string exoGuid, string leftArmGuid, string rightArmGuid)
        {
            ExoGuid = exoGuid;
            LeftArmGuid = leftArmGuid;
            RightArmGuid = rightArmGuid;
        }

        public override string ToString()
        {
            return "[ExosuitModulesAction - Guid: " + ExoGuid + " LeftArmGuid: " + LeftArmGuid + " RightArmGuid: " + RightArmGuid + "]";
        }
    }
}
