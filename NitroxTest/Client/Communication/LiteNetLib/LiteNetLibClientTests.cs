using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxClient;
using NitroxClient.Communication.NetworkingLayer.LiteNetLib;
using NitroxModel.Core;

namespace NitroxTest.Client.Communication.LiteNetLib
{
    [TestClass]
    public class LiteNetLibClientTests
    {
        private readonly string connection = "ghaarg";
        private readonly int port = 11000;
        private LiteNetLibClient liteNetLibClient;

        [TestInitialize]
        public void TestInitialize()
        {
            NitroxServiceLocator.InitializeDependencyContainer(new ClientAutoFacRegistrar());
            NitroxServiceLocator.BeginNewLifetimeScope();
            liteNetLibClient = new LiteNetLibClient();
        }

        [TestMethod]
        public void ConnectViaPunch()
        {
            liteNetLibClient.Start(connection, port);            
            Assert.AreEqual(liteNetLibClient.IsConnected, true);
        }
    }
}
