using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;
using FluentAssertions;
using NitroxClient.Communication;
using NitroxClient.Communication.Exceptions;

namespace NitroxTest.Client.Communication.ClientBridgeTests
{
    [TestClass]
    public class SendTests
    {
        private IClient serverClient;
        private ClientBridge clientBridge;
        private string correlationId;
        private Packet packet;

        [TestInitialize]
        public void GivenAnInitializedClientBridge()
        {
            //Given
            serverClient = Substitute.For<IClient>();
            serverClient.IsConnected.Returns(false);
            serverClient
                .When(client => client.start(Arg.Any<string>()))
                .Do(info => serverClient.IsConnected.Returns(true));

            serverClient
                .When(client => client.send(Arg.Any<ReservePlayerSlot>()))
                .Do(info => correlationId = info.Arg<ReservePlayerSlot>().CorrelationId);

            clientBridge = new ClientBridge(serverClient);

            packet = Substitute.For<Packet>();
        }

        [TestMethod]
        public void ShouldBeAbleToSendAPacketThroughAConnectedClientBridge()
        {
            //When
            clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            clientBridge.confirmReservation(correlationId, TestConstants.TEST_RESERVATION_KEY);

            clientBridge.send(packet);

            //Then
            serverClient.Received().send(Arg.Is(packet));
        }

        [TestMethod]
        public void ShouldThrowAProhibitedSendAttemptWhenTheClientBridgeIsDisconnected()
        {
            //When
            Action action = () => clientBridge.send(packet);

            //Then
            action.ShouldThrow<ProhibitedSendAttemptException>();
        }

        [TestMethod]
        public void ShouldThrowAProhibitedSendAttemptWhenTheClientBridgeIsWaiting()
        {
            //When
            clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => clientBridge.send(packet);

            //Then
            action.ShouldThrow<ProhibitedSendAttemptException>();
        }

        [TestMethod]
        public void ShouldThrowAProhibitedSendAttemptWhenTheClientBridgeIsFailed()
        {
            //When
            try
            {
                clientBridge.confirmReservation(null, null);
            }
            catch (Exception) { }
            Action action = () => clientBridge.send(packet);

            //Then
            action.ShouldThrow<ProhibitedSendAttemptException>();
        }

        [TestMethod]
        public void ShouldThrowAProhibitedSendAttemptWhenTheClientBridgeIsReservationRejected()
        {
            //When
            clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            clientBridge.handleRejectedReservation(correlationId, ReservationRejectionReason.PlayerNameInUse);
            Action action = () => clientBridge.send(packet);

            //Then
            action.ShouldThrow<ProhibitedSendAttemptException>();
        }
    }
}
