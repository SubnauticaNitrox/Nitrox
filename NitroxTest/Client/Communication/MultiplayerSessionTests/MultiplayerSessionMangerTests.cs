using FluentAssertions;
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
            multiplayerSession.CurrentState.CurrentStage.ShouldBeEquivalentTo(MultiplayerSessionConnectionStage.DISCONNECTED);
        }

        [TestMethod]
        public void ManagerShouldInitializeWithClient()
        {
            // Arrange
            IClient client = Substitute.For<IClient>();

            // Act
            IMultiplayerSession multiplayerSession = new MultiplayerSessionManager(client);

            // Assert
            multiplayerSession.Client.ShouldBeEquivalentTo(client);
        }

        [TestMethod]
        public void ConnectShouldSetIpAddress()
        {
            // Arrange
            IClient client = Substitute.For<IClient>();
            IMultiplayerSession multiplayerSession = new MultiplayerSessionManager(client, TestConstants.TEST_CONNECTION_STATE);

            // Act
            multiplayerSession.Connect(TestConstants.TEST_IP_ADDRESS,TestConstants.TEST_SERVER_PORT);

            // Assert
            multiplayerSession.IpAddress.ShouldBeEquivalentTo(TestConstants.TEST_IP_ADDRESS);
            multiplayerSession.ServerPort.ShouldBeEquivalentTo(TestConstants.TEST_SERVER_PORT);
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
            multiplayerSession.SessionPolicy.ShouldBeEquivalentTo(TestConstants.TEST_SESSION_POLICY);
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
            multiplayerSession.PlayerSettings.ShouldBeEquivalentTo(TestConstants.TEST_PLAYER_SETTINGS);
            multiplayerSession.AuthenticationContext.ShouldBeEquivalentTo(TestConstants.TEST_AUTHENTICATION_CONTEXT);
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
            multiplayerSession.Reservation.ShouldBeEquivalentTo(successfulReservation);
        }

        [TestMethod]
        public void UpdateStateShouldRaiseEvent()
        {
            // Arrange
            IClient client = Substitute.For<IClient>();
            IMultiplayerSession multiplayerSession = new MultiplayerSessionManager(client);
            IMultiplayerSessionConnectionContext connectionContext = (IMultiplayerSessionConnectionContext)multiplayerSession;
            multiplayerSession.MonitorEvents();

            // Act
            connectionContext.UpdateConnectionState(TestConstants.TEST_CONNECTION_STATE);

            // Assert
            multiplayerSession.ShouldRaise("ConnectionStateChanged");
        }
    }
}
