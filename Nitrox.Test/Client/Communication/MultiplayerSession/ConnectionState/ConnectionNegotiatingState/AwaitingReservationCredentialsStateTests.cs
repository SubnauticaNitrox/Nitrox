using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nitrox.Test.Client.Communication.MultiplayerSession;
using NitroxClient.Communication.Abstract;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NSubstitute;

namespace NitroxClient.Communication.MultiplayerSession.ConnectionState
{
    [TestClass]
    public class AwaitingReservationCredentialsStateTests
    {
        [TestMethod]
        public void NegotiateShouldSendServerAuthorityReservationRequest()
        {
            // Arrange
            IClient serverClient = Substitute.For<IClient>();
            serverClient.IsConnected.Returns(true);

            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.Client.Returns(serverClient);
            connectionContext.PlayerSettings.Returns(TestConstants.TEST_PLAYER_SETTINGS);
            connectionContext.AuthenticationContext.Returns(TestConstants.TEST_AUTHENTICATION_CONTEXT);

            AwaitingReservationCredentials connectionState = new AwaitingReservationCredentials();

            // Act
            connectionState.NegotiateReservationAsync(connectionContext);

            // Assert
            serverClient.Received().Send(Arg.Any<MultiplayerSessionReservationRequest>());
        }

        [TestMethod]
        public void NegotiateShouldTransitionToAwaitingSessionReservationState()
        {
            // Arrange
            IClient serverClient = Substitute.For<IClient>();
            serverClient.IsConnected.Returns(true);

            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.Client.Returns(serverClient);
            connectionContext.PlayerSettings.Returns(TestConstants.TEST_PLAYER_SETTINGS);
            connectionContext.AuthenticationContext.Returns(TestConstants.TEST_AUTHENTICATION_CONTEXT);

            AwaitingReservationCredentials connectionState = new AwaitingReservationCredentials();

            // Act
            connectionState.NegotiateReservationAsync(connectionContext);

            // Assert
            connectionContext.Received().UpdateConnectionState(Arg.Any<AwaitingSessionReservation>());
        }

        [TestMethod]
        public void NegotiateShouldThrowInvalidOperationExceptionWhenPlayerSettingsIsNull()
        {
            // Arrange
            IClient serverClient = Substitute.For<IClient>();
            serverClient.IsConnected.Returns(true);

            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.PlayerSettings.Returns((PlayerSettings)null);
            connectionContext.AuthenticationContext.Returns(TestConstants.TEST_AUTHENTICATION_CONTEXT);

            AwaitingReservationCredentials connectionState = new AwaitingReservationCredentials();

            // Act
            Action action = () => connectionState.NegotiateReservationAsync(connectionContext);

            // Assert
            action.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void NegotiateShouldThrowInvalidOperationExceptionWhenAuthenticationContextIsNull()
        {
            // Arrange
            IClient serverClient = Substitute.For<IClient>();
            serverClient.IsConnected.Returns(true);

            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.SessionPolicy.Returns(TestConstants.TEST_SESSION_POLICY);
            connectionContext.AuthenticationContext.Returns((AuthenticationContext)null);

            AwaitingReservationCredentials connectionState = new AwaitingReservationCredentials();

            // Act
            Action action = () => connectionState.NegotiateReservationAsync(connectionContext);

            // Assert
            action.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void JoinSessionShouldThrowInvalidOperationException()
        {
            // Arrange
            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            AwaitingReservationCredentials connectionState = new AwaitingReservationCredentials();

            // Act
            Action action = () => connectionState.JoinSession(connectionContext);

            // Assert
            action.Should().Throw<InvalidOperationException>();
        }

        [TestMethod]
        public void DisconnectShouldStopTheClient()
        {
            // Arrange
            IClient serverClient = Substitute.For<IClient>();
            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.Client.Returns(serverClient);

            AwaitingReservationCredentials connectionState = new AwaitingReservationCredentials();

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

            AwaitingReservationCredentials connectionState = new AwaitingReservationCredentials();

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

            AwaitingReservationCredentials connectionState = new AwaitingReservationCredentials();

            // Act
            connectionState.Disconnect(connectionContext);

            // Assert
            connectionContext.Received().UpdateConnectionState(Arg.Any<Disconnected>());
        }
    }
}
