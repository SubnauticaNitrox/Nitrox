using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nitrox.Client.Communication.Abstract;
using Nitrox.Client.Communication.MultiplayerSession.ConnectionState;
using Nitrox.Model.MultiplayerSession;
using Nitrox.Model.Packets;
using NSubstitute;

namespace Nitrox.Test.Client.Communication.MultiplayerSessionTests.ConnectionStateTests.ConnectionNegotiatingStateTests
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
            connectionState.NegotiateReservation(connectionContext);

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
            connectionState.NegotiateReservation(connectionContext);

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
            Action action = () => connectionState.NegotiateReservation(connectionContext);

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
            Action action = () => connectionState.NegotiateReservation(connectionContext);

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
