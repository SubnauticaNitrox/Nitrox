using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession.ConnectionState;
using NitroxModel.Packets;
using NSubstitute;

namespace NitroxTest.Client.Communication.MultiplayerSessionTests.ConnectionStateTests.ConnectionNegotiatedStateTests
{
    [TestClass]
    public class SessionReservedStateTests
    {
        [TestMethod]
        public void NegotiateShouldThrowInvalidOperationException()
        {
            // Arrange
            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            SessionReserved connectionState = new SessionReserved();

            // Act
            Action action = () => connectionState.NegotiateReservation(connectionContext);

            // Assert
            action.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void JoinSessionShouldSendPlayerJoiningMultiplayerSessionPacket()
        {
            // Arrange
            MultiplayerSessionReservation successfulReservation = new MultiplayerSessionReservation(
                TestConstants.TEST_CORRELATION_ID,
                TestConstants.TEST_PLAYER_ID,
                TestConstants.TEST_RESERVATION_KEY);

            IClient client = Substitute.For<IClient>();
            client.IsConnected.Returns(true);

            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.Reservation.Returns(successfulReservation);
            connectionContext.Client.Returns(client);

            SessionReserved connectionState = new SessionReserved();

            // Act
            connectionState.JoinSession(connectionContext);

            // Assert
            client.Received().Send(Arg.Any<PlayerJoiningMultiplayerSession>());
        }

        [TestMethod]
        public void JoinSessionShouldTransitionToSessionJoinedState()
        {
            // Arrange
            MultiplayerSessionReservation successfulReservation = new MultiplayerSessionReservation(
                TestConstants.TEST_CORRELATION_ID,
                TestConstants.TEST_PLAYER_ID,
                TestConstants.TEST_RESERVATION_KEY);

            IClient serverClient = Substitute.For<IClient>();
            serverClient.IsConnected.Returns(true);

            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.Reservation.Returns(successfulReservation);
            connectionContext.Client.Returns(serverClient);

            SessionReserved connection = new SessionReserved();

            // Act
            connection.JoinSession(connectionContext);

            // Assert
            connectionContext.Received().UpdateConnectionState(Arg.Any<SessionJoined>());
        }

        [TestMethod]
        public void DisconnectShouldStopTheClient()
        {
            // Arrange
            IClient serverClient = Substitute.For<IClient>();
            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.Client.Returns(serverClient);

            SessionReserved connectionState = new SessionReserved();

            // Act
            connectionState.Disconnect(connectionContext);

            // Assert
            serverClient.Received().Stop();
        }

        [TestMethod]
        public void DisconnectShouldResetTheConnectionContext()
        {
            // Arrange
            IClient serverClient = Substitute.For<IClient>();
            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.Client.Returns(serverClient);

            SessionReserved connectionState = new SessionReserved();

            // Act
            connectionState.Disconnect(connectionContext);

            // Assert
            connectionContext.Received().ClearSessionState();
        }

        [TestMethod]
        public void DisconnectShouldTransitionToDisconnectedState()
        {
            // Arrange
            IClient serverClient = Substitute.For<IClient>();
            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.Client.Returns(serverClient);

            SessionReserved connection = new SessionReserved();

            // Act
            connection.Disconnect(connectionContext);

            // Assert
            connectionContext.Received().UpdateConnectionState(Arg.Any<Disconnected>());
        }
    }
}
