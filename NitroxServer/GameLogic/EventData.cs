using System.Collections.Generic;
using NitroxModel.Logger;
using ProtoBufNet;

namespace NitroxServer.GameLogic
{
    [ProtoContract]
    public class EventData
    {
        [ProtoMember(1)]
        public List<NitroxTimer> SerialisedTimers
        {
            get
            {
                lock (timers)
                {
                    return timers;
                }
            }
            set { timers = value; }
        }

        [ProtoIgnore]
        private List<NitroxTimer> timers = new List<NitroxTimer>();

        internal bool HasTimers()
        {
            return timers.Count > 0;
        }

        public void AddTimer(NitroxTimer timer)
        {
            lock (timers)
            {
                timers.Add(timer);
            }
        }

        public void PauseTimers()
        {
            lock (timers)
            {
                foreach(NitroxTimer timer in timers)
                {
                    timer.Stop();
                }
            }
        }

        public void StartTimers()
        {
            lock (timers)
            {
                foreach (NitroxTimer timer in timers)
                {
                    timer.Start();
                }
            }
        }

        public override string ToString()
        {
            return timers.Count.ToString();
        }
    }
}
