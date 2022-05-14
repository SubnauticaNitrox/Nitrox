using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NitroxServer.Communication
{
    public enum NitroxConnectionState
    {
        Unknown,
        Disconnected,
        Connected,
        Reserved,
        InGame
    }
}
