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
                //TODO: remove, Logs the count of timers
                Log.Info("Paused " + timers.Count);
                foreach(NitroxTimer timer in timers)
                {
                    timer.Stop();
                    //TODO: remove, how long left till event
                    Log.Info(timer.TimeLeftDate);
                }
            }
        }

        public void StartTimers()
        {
            lock (timers)
            {
                //TODO: remove, Logs the count of timers
                Log.Info("Started " + timers.Count);
                foreach (NitroxTimer timer in timers)
                {
                    timer.Start();
                    //TODO: remove, how long left till event
                    Log.Info("Started " + timer.TimeLeftDate);
                }
            }
        }

        public override string ToString()
        {
            return timers.Count.ToString();
        }
    }
}
