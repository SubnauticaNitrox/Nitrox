using System;
using NitroxModel.Logger;
using NitroxModel.Packets;
using ProtoBufNet;

namespace NitroxServer.GameLogic
{
    [Serializable]
    [ProtoContract]
    public class NitroxTimer : System.Timers.Timer
    {
        [ProtoMember(1)]
        public string Key;
        [ProtoMember(2)]
        public double? TimeLeft
        {
            get
            {
                return timeLeft;
            }
            set { timeLeft = value; }
        }
        [ProtoMember(3)]
        public double InitialInterval;
        [ProtoMember(4)]
        public PlayerManager PlayerManager;
        [ProtoMember(5)]
        public StoryEventType EventType;
        [ProtoIgnore]
        private double? timeLeft;
        [ProtoIgnore]
        private DateTime m_dueTime;

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
                return (m_dueTime - DateTime.Now).ToString(@"hh\:mm\:ss") + " - " + Key;
            }
        }

        new public void Start()
        {
            if (timeLeft != null)
            {
                m_dueTime = DateTime.Now.AddMilliseconds(timeLeft.Value);
                base.Interval = timeLeft.Value;
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
            Log.Info("Triggering event type " + EventType.ToString() + " at time " + DateTime.Now.AddMilliseconds(Interval).ToString(@"hh\:mm\:ss") + " with param " + Key.ToString());
            PlayerManager.SendPacketToAllPlayers(new StoryEventSend(EventType, Key));
            if (AutoReset)
            {
                m_dueTime = DateTime.Now.AddMilliseconds(InitialInterval);
            }
        }

        public Delegate returned()
        {
            return Events[0];
        }
    }
}
