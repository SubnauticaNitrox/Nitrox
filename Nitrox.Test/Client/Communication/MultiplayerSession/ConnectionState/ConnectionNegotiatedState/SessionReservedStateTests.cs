using Nitrox.Model.Subnautica.Packets;
using Nitrox.Test.Client.Communication.MultiplayerSession;
using NitroxClient.Communication.Abstract;
using NSubstitute;

namespace NitroxClient.Communication.MultiplayerSession.ConnectionState
{
    [TestClass]
    public class SessionReservedStateTests
    {
        [TestMethod]
        public void NegotiateShouldThrowInvalidOperationException()
        {
            // Arrange
            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            SessionReserved connectionState = new();

            // Act
            Action action = () => connectionState.NegotiateReservationAsync(connectionContext);

            // Assert
            action.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void JoinSessionShouldSendPlayerJoiningMultiplayerSessionPacket()
        {
            // Arrange
            MultiplayerSessionReservation successfulReservation = new(TestConstants.TestSessionId);

            IClient client = Substitute.For<IClient>();
            client.IsConnected.Returns(true);

            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.Reservation.Returns(successfulReservation);
            connectionContext.Client.Returns(client);

            SessionReserved connectionState = new();

            // Act
            connectionState.JoinSession(connectionContext);

            // Assert
            client.Received().Send(Arg.Any<PlayerJoiningMultiplayerSession>());
        }

        [TestMethod]
        public void JoinSessionShouldTransitionToSessionJoinedState()
        {
            // Arrange
            MultiplayerSessionReservation successfulReservation = new(TestConstants.TestSessionId);

            IClient serverClient = Substitute.For<IClient>();
            serverClient.IsConnected.Returns(true);

            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.Reservation.Returns(successfulReservation);
            connectionContext.Client.Returns(serverClient);

            SessionReserved connection = new();

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

            SessionReserved connectionState = new();

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

            SessionReserved connectionState = new();

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

            SessionReserved connection = new();

            // Act
            connection.Disconnect(connectionContext);

            // Assert
            connectionContext.Received().UpdateConnectionState(Arg.Any<Disconnected>());
        }
    }
}
