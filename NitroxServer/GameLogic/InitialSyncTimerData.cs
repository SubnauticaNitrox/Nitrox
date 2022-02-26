using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroxModel.MultiplayerSession;
using NitroxServer.Communication;

namespace NitroxServer.GameLogic
{
    /// <summary>
    /// Contains data used in InitialSyncTimer callback
    /// 
    /// For use with <see cref="System.Threading.Timer"/>
    /// </summary>
    internal class InitialSyncTimerData
    {
        public readonly NitroxConnection Connection;
        public readonly AuthenticationContext Context;
        public readonly int MaxCounter;

        /// <summary>
        /// Keeps track of how many times the timer has elapsed
        /// </summary>
        public int Counter = 0;

        /// <summary>
        /// Set to true if disposing the timer
        /// </summary>
        public bool Disposing = false;

        public InitialSyncTimerData(NitroxConnection connection, AuthenticationContext context, int initialSyncTimeout)
        {
            Connection = connection;
            Context = context;
            MaxCounter = (int)Math.Ceiling(initialSyncTimeout / 200f);
        }
    }
}
