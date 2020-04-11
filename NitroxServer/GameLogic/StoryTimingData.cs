using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxModel.Logger;
using ProtoBufNet;

namespace NitroxServer.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class StoryTimingData
    {
        [ProtoMember(1)]
        public double ElapsedTime { get; set; }

        [ProtoMember(2)]
        public double? AuroraExplosionTime { get; set; }

        [ProtoIgnore]
        public EventTriggerer EventTriggerer;

        [ProtoBeforeSerialization]
        public void ProtoBeforeSerialization()
        {
            if (EventTriggerer != null)
            {
                EventTriggerer.Serialize();
            }
            else
            {
                Log.Warn("Event Triggerer failed to serialize (null)");
            }
        }
    }
}
