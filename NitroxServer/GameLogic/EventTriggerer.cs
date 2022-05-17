using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Timers;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxServer.Helper;
using NitroxServer.GameLogic.Unlockables;

namespace NitroxServer.GameLogic
{
    /// <summary>
    /// Keeps track of Aurora-related events
    /// </summary>
    public class EventTriggerer
    {
        internal readonly Dictionary<string, Timer> eventTimers = new();
        private readonly Stopwatch stopWatch = new();
        private readonly PlayerManager playerManager;
        private readonly PDAStateData pdaStateData;
        private readonly StoryGoalData storyGoalData;
        private string seed;

        public double AuroraExplosionTimeMs;
        // Necessary to calculate the timers correctly
        public double AuroraWarningTimeMs;

        private double elapsedTimeOutsideStopWatchMs;

        /// <summary>
        /// Total Elapsed Time in milliseconds
        /// </summary>
        public double ElapsedTimeMs
        {
            get => stopWatch.ElapsedMilliseconds + elapsedTimeOutsideStopWatchMs;
            internal set
            {
                foreach (Timer timer in eventTimers.Values)
                {
                    timer.Interval = Math.Max(1, timer.Interval - (value - ElapsedTimeMs));
                }
                elapsedTimeOutsideStopWatchMs = value - stopWatch.ElapsedMilliseconds;
            }
        }

        /// <summary>
        /// Total Elapsed Time in seconds
        /// </summary>
        public double ElapsedSeconds
        {
            get => ElapsedTimeMs * 0.001;
            private set => ElapsedTimeMs = value * 1000;
        }

        /// <summary>
        /// Subnautica's equivalent of days
        /// </summary>
        // Using ceiling because days count start at 1 and not 0
        public int Day => (int)Math.Ceiling(ElapsedTimeMs / TimeSpan.FromMinutes(20).TotalMilliseconds);

        public EventTriggerer(PlayerManager playerManager, PDAStateData pdaStateData, StoryGoalData storyGoalData, string seed, double elapsedTime, double? auroraExplosionTime, double? auroraWarningTime)
        {
            this.playerManager = playerManager;
            this.pdaStateData = pdaStateData;
            this.storyGoalData = storyGoalData;
            this.seed = seed;
            // Default time in Base SN is 480s
            elapsedTimeOutsideStopWatchMs = elapsedTime == 0 ? TimeSpan.FromSeconds(480).TotalMilliseconds : elapsedTime;
            AuroraExplosionTimeMs = auroraExplosionTime ?? GenerateDeterministicAuroraTime(seed);
            AuroraWarningTimeMs = auroraWarningTime ?? ElapsedTimeMs;
            CreateEventTimers();
            stopWatch.Start();
            Log.Debug($"Event Triggerer started! ElapsedTime={Math.Floor(ElapsedSeconds)}s");
            Log.Debug($"Aurora will explode in {GetMinutesBeforeAuroraExplosion()} minutes");
        }

        /// <summary>
        /// Creates every timer that will keep track of the time before we need to trigger an event
        /// </summary>
        private void CreateEventTimers()
        {
            double ExplosionCycleDuration = AuroraExplosionTimeMs - AuroraWarningTimeMs;
            // If aurora's warning is set to later than explosion's time, we don't want to create any timer
            if (ExplosionCycleDuration < 0)
            {
                return;
            }
            double TimePassedSinceWarning = ElapsedTimeMs - AuroraWarningTimeMs;
            CreateTimer(ExplosionCycleDuration * 0.2d - TimePassedSinceWarning, StoryEventSend.EventType.PDA_EXTRA, "Story_AuroraWarning1");
            CreateTimer(ExplosionCycleDuration * 0.5d - TimePassedSinceWarning, StoryEventSend.EventType.PDA_EXTRA, "Story_AuroraWarning2");
            CreateTimer(ExplosionCycleDuration * 0.8d - TimePassedSinceWarning, StoryEventSend.EventType.PDA_EXTRA, "Story_AuroraWarning3");
            // Story_AuroraWarning4 and Story_AuroraExplosion must occur at the same time
            CreateTimer(ExplosionCycleDuration - TimePassedSinceWarning, StoryEventSend.EventType.PDA_EXTRA, "Story_AuroraWarning4");
            CreateTimer(ExplosionCycleDuration - TimePassedSinceWarning, StoryEventSend.EventType.EXTRA, "Story_AuroraExplosion");
        }

        /// <summary>
        /// When starting the server, if some events already happened, the time parameter will be &lt; 0
        /// in which case we don't want to create the timer
        /// </summary>
        /// <param name="time">In milliseconds</param>
        private void CreateTimer(double time, StoryEventSend.EventType eventType, string key)
        {
            if (time <= 0)
            {
                return;
            }

            Timer timer = new()
            {
                Interval = time,
                Enabled = true,
                AutoReset = false
            };
            timer.Elapsed += delegate
            {
                eventTimers.Remove(key);
                Log.Info($"Triggering event type {eventType} at time {time} with param {key}");
                playerManager.SendPacketToAllPlayers(new StoryEventSend(eventType, key));
            };

            eventTimers.Add(key, timer);
        }

        /// <summary>
        /// Tells the players to start Aurora's explosion event
        /// </summary>
        /// <param name="cooldown">Wether we should make Aurora explode instantly or after a short countdown</param>
        public void ExplodeAurora(bool cooldown)
        {
            ClearTimers();
            AuroraExplosionTimeMs = ElapsedTimeMs;
            // Explode aurora with a cooldown is like default game just before aurora is about to explode
            if (cooldown)
            {
                // These lines should be filled with the same informations as in the constructor
                playerManager.SendPacketToAllPlayers(new StoryEventSend(StoryEventSend.EventType.PDA_EXTRA, "Story_AuroraWarning4"));
                playerManager.SendPacketToAllPlayers(new StoryEventSend(StoryEventSend.EventType.EXTRA, "Story_AuroraExplosion"));
                Log.Info("Started Aurora's explosion sequence");
            }
            else
            {
                // This will make aurora explode instantly on clients
                playerManager.SendPacketToAllPlayers(new AuroraExplodeNow());
                Log.Info("Exploded Aurora");
            }
        }

        /// <summary>
        /// Tells the players to start Aurora's restoration event
        /// </summary>
        public void RestoreAurora()
        {
            ClearTimers();
            AuroraExplosionTimeMs = GenerateDeterministicAuroraTime(seed) + ElapsedTimeMs;
            AuroraWarningTimeMs = ElapsedTimeMs;
            CreateEventTimers();
            // We need to clear these entries from PdaLog and CompletedGoals to make sure that the client, when reconnecting, doesn't have false information
            foreach (string timerKey in eventTimers.Keys)
            {
                PDALogEntry logEntry = pdaStateData.PdaLog.Find(entry => entry.Key == timerKey);
                // Wether or not we found the entry doesn't matter
                pdaStateData.PdaLog.Remove(logEntry);
                storyGoalData.CompletedGoals.Remove(timerKey);
            }
            playerManager.SendPacketToAllPlayers(new AuroraRestore());
            Log.Info($"Restored Aurora, will explode again in {GetMinutesBeforeAuroraExplosion()} minutes");
        }

        /// <summary>
        /// Removes every timer that's still alive
        /// </summary>
        private void ClearTimers()
        {
            foreach (Timer timer in eventTimers.Values)
            {
                timer.Stop();
                timer.Dispose();
            }
            eventTimers.Clear();
        }

        /// <summary>
        /// Calculate the future Aurora's explosion time in a deterministic manner
        /// </summary>
        private double GenerateDeterministicAuroraTime(string seed)
        {
            // Copied from CrashedShipExploder.SetExplodeTime() and changed from seconds to ms
            DeterministicGenerator generator = new(seed, nameof(EventTriggerer));
            return elapsedTimeOutsideStopWatchMs + generator.NextDouble(2.3d, 4d) * 1200d * 1000d;
        }

        /// <summary>
        /// Restarts every event timer
        /// </summary>
        public void StartWorld()
        {
            stopWatch.Start();
            foreach (Timer eventTimer in eventTimers.Values)
            {
                eventTimer.Start();
            }
        }

        /// <summary>
        /// Pauses every event timer
        /// </summary>
        public void PauseWorld()
        {
            stopWatch.Stop();
            foreach (Timer eventTimer in eventTimers.Values)
            {
                eventTimer.Stop();
            }
        }

        /// <summary>
        /// Calculates the time before the aurora explosion
        /// </summary>
        /// <returns>The time in minutes before aurora explodes or -1 if it already exploded</returns>
        private double GetMinutesBeforeAuroraExplosion()
        {
            return AuroraExplosionTimeMs > ElapsedTimeMs ? Math.Round((AuroraExplosionTimeMs - ElapsedTimeMs) / 60000) : -1;
        }

        /// <summary>
        /// Makes a nice status for the summary command for example
        /// </summary>
        public string GetAuroraStateSummary()
        {
            double minutesBeforeExplosion = GetMinutesBeforeAuroraExplosion();
            if (minutesBeforeExplosion < 0)
            {
                return "already exploded";
            }
            string stateNumber = "";
            if (eventTimers.Count > 0)
            {
                // Event timer events should always have a number at last character
                string nextEventKey = eventTimers.ElementAt(0).Key;
                stateNumber = $" [{nextEventKey[nextEventKey.Length - 1]}/4]";
            }
            
            return $"explodes in {minutesBeforeExplosion} minutes{stateNumber}";
        }

        internal void ResetWorld()
        {
            stopWatch.Reset();
        }

        /// <summary>
        /// Set current time (replication of SN's system)
        /// </summary>
        /// <param name="type">Type of the operation to apply</param>
        public void ChangeTime(TimeModification type)
        {
            switch (type)
            {
                case TimeModification.DAY:
                    ElapsedTimeMs += 1200000.0 - ElapsedTimeMs % 1200000.0 + 600000.0;
                    break;
                case TimeModification.NIGHT:
                    ElapsedTimeMs += 1200000.0 - ElapsedTimeMs % 1200000.0;
                    break;
                case TimeModification.SKIP:
                    ElapsedTimeMs += 600000.0 - ElapsedTimeMs % 600000.0;
                    break;
            }

            playerManager.SendPacketToAllPlayers(new TimeChange(ElapsedSeconds, false));
        }

        public enum TimeModification
        {
            DAY, NIGHT, SKIP
        }
    }
}
