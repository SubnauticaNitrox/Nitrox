using System;
using System.ComponentModel;
using ProtoBufNet;

namespace NitroxServer.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class NitroxTimer : System.Timers.Timer
    {
        [ProtoIgnore]
        private DateTime m_dueTime;
        [ProtoMember(2)]
        public string Key;
        [ProtoIgnore]
        private double? timeLeft;
        [ProtoMember(3)]
        public double? TimeLeft
        {
            get
            {
                return timeLeft;
            }
            set { timeLeft = value; }
        }

        public NitroxTimer() : base()
        {
            Elapsed += ElapsedAction;
        }

        new protected void Dispose()
        {
            Elapsed -= ElapsedAction;
            base.Dispose();
        }

        [ProtoIgnore]
        public string TimeLeftDate
        {
            get
            {
                return (m_dueTime - DateTime.Now).ToString(@"hh\:mm\:ss") + " - " + Key + " - " + timeLeft;
            }
        }

        new public void Start()
        {
            if (timeLeft != null)
            {
                m_dueTime = DateTime.Now.AddMilliseconds(timeLeft.Value);
            }
            else
            {
                m_dueTime = DateTime.Now.AddMilliseconds(Interval);
            }
            base.Start();
        }

        new public void Stop()
        {
            timeLeft = (m_dueTime - DateTime.Now).TotalMilliseconds;
            base.Stop();
        }

        private void ElapsedAction(object sender, System.Timers.ElapsedEventArgs e)
        {
            if (AutoReset)
            {
                m_dueTime = DateTime.Now.AddMilliseconds(Interval);
            }
        }

        public Delegate returned()
        {
            return Events[0];
        }
    }
}
