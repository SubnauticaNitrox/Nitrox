using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBufNet;

namespace NitroxServer.GameLogic.Players
{
    [ProtoContract]
    [ProtoInclude(400, typeof(PlayerData))]
    public interface IPlayerData
    {
        PlayerData ToPlayerData();
    }
}
