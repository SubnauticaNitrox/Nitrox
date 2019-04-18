using System;
using System.Collections.Generic;
using System.Text;

namespace NitroxServerUdpPunch.Communication.NetworkingLayer
{
    interface IPunchServer
    {
        void Start();
        void Process();
    }
}
