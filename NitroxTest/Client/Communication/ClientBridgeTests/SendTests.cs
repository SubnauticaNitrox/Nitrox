using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxClient.Communication;
using NitroxClient.Communication.Exceptions;
using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;
using NSubstitute;
using System;

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
                .When(client => client.Start(Arg.Any<string>()))
                .Do(info => serverClient.IsConnected.Returns(true));

            serverClient
                .When(client => client.Send(Arg.Any<ReservePlayerSlot>()))
                .Do(info => correlationId = info.Arg<ReservePlayerSlot>().CorrelationId);

            clientBridge = new ClientBridge(serverClient);

            packet = Substitute.For<Packet>();
        }

        [TestMethod]
        public void ShouldBeAbleToSendAPacketThroughAConnectedClientBridge()
        {
            //When
            clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            clientBridge.ConfirmReservation(correlationId, TestConstants.TEST_RESERVATION_KEY);
            clientBridge.ClaimReservation();

            clientBridge.Send(packet);

            //Then
            serverClient.Received().Send(Arg.Is(packet));
        }

        [TestMethod]
        public void ShouldThrowAProhibitedSendAttemptWhenTheClientBridgeIsDisconnected()
        {
            //When
            Action action = () => clientBridge.Send(packet);

            //Then
            action.ShouldThrow<ProhibitedSendAttemptException>();
        }

        [TestMethod]
        public void ShouldThrowAProhibitedSendAttemptWhenTheClientBridgeIsWaiting()
        {
            //When
            clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => clientBridge.Send(packet);

            //Then
            action.ShouldThrow<ProhibitedSendAttemptException>();
        }

        [TestMethod]
        public void ShouldThrowAProhibitedSendAttemptWhenTheClientBridgeIsFailed()
        {
            //When
            try
            {
                clientBridge.ConfirmReservation(null, null);
            }
            catch (Exception) { }
            Action action = () => clientBridge.Send(packet);

            //Then
            action.ShouldThrow<ProhibitedSendAttemptException>();
        }

        [TestMethod]
        public void ShouldThrowAProhibitedSendAttemptWhenTheClientBridgeIsReservationRejected()
        {
            //When
            clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            clientBridge.HandleRejectedReservation(correlationId, TestConstants.TEST_REJECTION_STATE);
            Action action = () => clientBridge.Send(packet);

            //Then
            action.ShouldThrow<ProhibitedSendAttemptException>();
        }
    }
}
