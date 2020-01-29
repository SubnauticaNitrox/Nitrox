using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ProtoBufNet;

namespace NitroxServer.GameLogic.Bases
{
    [ProtoContract]
    [ProtoInclude(300, typeof(BaseData))]
    public interface IBaseData
    {
        BaseData ToBaseData();
    }
}
