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
        private readonly string connection = "109.41.67.22";
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
            string serverName = connection;
            try
            {
                var addresses = Dns.GetHostAddresses(connection);
                if (addresses.Count() == 0)
                {
                    Assert.Fail();
                    return;
                }
                serverName = addresses[0].ToString();
            } catch (SocketException e)
            {
                NitroxModel.Logger.Log.Debug("Socket exception thrown. This can be ok. Message: {0}", e.Message);
            }

            liteNetLibClient.Start(serverName, port);            
            Assert.AreEqual(liteNetLibClient.IsConnected, true);
        }
    }
}
