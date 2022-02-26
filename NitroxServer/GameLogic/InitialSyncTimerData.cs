using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NitroxModel.MultiplayerSession;
using NitroxServer.Communication;

namespace NitroxServer.GameLogic
{
    internal class InitialSyncTimerData
    {
        public int Counter = 0;
        public NitroxConnection Connection;
        public AuthenticationContext Context;
        public int MaxCounter;
        public bool Disposing = false;

        public InitialSyncTimerData(NitroxConnection connection, AuthenticationContext context, int initialSyncTimeout)
        {
            Connection = connection;
            Context = context;
            MaxCounter = (int)Math.Ceiling(initialSyncTimeout / 200f);
        }
    }
}
