using FluentAssertions;
using FluentAssertions.Events;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession;
using NitroxModel.Packets;
using NSubstitute;

namespace NitroxTest.Client.Communication.MultiplayerSessionTests
{
    [TestClass]
    public class MultiplayerSessionMangerTests
    {
        [TestMethod]
        public void ManagerShouldInitializeInDisconnectedStage()
        {
            // Arrange
            IClient client = Substitute.For<IClient>();

            // Act
            IMultiplayerSession multiplayerSession = new MultiplayerSessionManager(client);

            // Assert
            multiplayerSession.CurrentState.CurrentStage.Should().Be(MultiplayerSessionConnectionStage.DISCONNECTED);
        }

        [TestMethod]
        public void ManagerShouldInitializeWithClient()
        {
            // Arrange
            IClient client = Substitute.For<IClient>();

            // Act
            IMultiplayerSession multiplayerSession = new MultiplayerSessionManager(client);

            // Assert
            multiplayerSession.Client.Should().Be(client);
        }

        [TestMethod]
        public void ConnectShouldSetIpAddress()
        {
            // Arrange
            IClient client = Substitute.For<IClient>();
            IMultiplayerSession multiplayerSession = new MultiplayerSessionManager(client, TestConstants.TEST_CONNECTION_STATE);

            // Act
            multiplayerSession.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_SERVER_PORT);

            // Assert
            multiplayerSession.IpAddress.Should().Be(TestConstants.TEST_IP_ADDRESS);
            multiplayerSession.ServerPort.Should().Be(TestConstants.TEST_SERVER_PORT);
        }

        [TestMethod]
        public void ProcessSessionPolicyShouldSetThePolicy()
        {
            // Arrange
            IClient client = Substitute.For<IClient>();
            IMultiplayerSession multiplayerSession = new MultiplayerSessionManager(client, TestConstants.TEST_CONNECTION_STATE);

            // Act
            multiplayerSession.ProcessSessionPolicy(TestConstants.TEST_SESSION_POLICY);

            // Assert
            multiplayerSession.SessionPolicy.Should().Be(TestConstants.TEST_SESSION_POLICY);
        }

        [TestMethod]
        public void RequestSessionReservationShouldSetSettingsAndAuthContext()
        {
            // Arrange
            IClient client = Substitute.For<IClient>();
            IMultiplayerSession multiplayerSession = new MultiplayerSessionManager(client, TestConstants.TEST_CONNECTION_STATE);

            // Act
            multiplayerSession.RequestSessionReservation(TestConstants.TEST_PLAYER_SETTINGS, TestConstants.TEST_AUTHENTICATION_CONTEXT);

            // Assert
            multiplayerSession.PlayerSettings.Should().Be(TestConstants.TEST_PLAYER_SETTINGS);
            multiplayerSession.AuthenticationContext.Should().Be(TestConstants.TEST_AUTHENTICATION_CONTEXT);
        }

        [TestMethod]
        public void ProcessReservationResponsePacketShouldSetTheReservation()
        {
            // Arrange
            MultiplayerSessionReservation successfulReservation = new MultiplayerSessionReservation(
                TestConstants.TEST_CORRELATION_ID,
                TestConstants.TEST_PLAYER_ID,
                TestConstants.TEST_RESERVATION_KEY);

            IClient client = Substitute.For<IClient>();
            IMultiplayerSession multiplayerSession = new MultiplayerSessionManager(client, TestConstants.TEST_CONNECTION_STATE);

            // Act
            multiplayerSession.ProcessReservationResponsePacket(successfulReservation);

            // Assert
            multiplayerSession.Reservation.Should().Be(successfulReservation);
        }

        [TestMethod]
        public void UpdateStateShouldRaiseEvent()
        {
            // Arrange
            IClient client = Substitute.For<IClient>();
            IMultiplayerSession multiplayerSession = new MultiplayerSessionManager(client);
            IMultiplayerSessionConnectionContext connectionContext = (IMultiplayerSessionConnectionContext)multiplayerSession;
            IMonitor<IMultiplayerSession> monitor = multiplayerSession.Monitor();

            // Act
            connectionContext.UpdateConnectionState(TestConstants.TEST_CONNECTION_STATE);

            // Assert
            monitor.Should().Raise("ConnectionStateChanged");
        }
    }
}
