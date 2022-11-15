﻿using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Nitrox.Test.Client.Communication.MultiplayerSession;
using NitroxClient.Communication.Abstract;
using NitroxModel.Packets;
using NitroxModel.Packets.Exceptions;
using NSubstitute;

namespace NitroxClient.Communication.MultiplayerSession.ConnectionState
{
    [TestClass]
    public class EstablishingSessionPolicyStateTests
    {
        [TestMethod]
        public void NegotiateShouldTransitionToAwaitingReservationCredentialsState()
        {
            // Arrange
            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.SessionPolicy.Returns(new MultiplayerSessionPolicy(TestConstants.TEST_CORRELATION_ID, false, TestConstants.TEST_MAX_PLAYER_CONNECTIONS, false));

            EstablishingSessionPolicy connectionState = new EstablishingSessionPolicy(TestConstants.TEST_CORRELATION_ID);

            // Act
            connectionState.NegotiateReservationAsync(connectionContext);

            // Assert
            connectionContext.Received().UpdateConnectionState(Arg.Any<AwaitingReservationCredentials>());
        }

        [TestMethod]
        public void NegotiateShouldTransitionToAwaitingReservationCredentialsStateWithPassword()
        {
            // Arrange
            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.SessionPolicy.Returns(new MultiplayerSessionPolicy(TestConstants.TEST_CORRELATION_ID, false, TestConstants.TEST_MAX_PLAYER_CONNECTIONS, true));

            EstablishingSessionPolicy connectionState = new EstablishingSessionPolicy(TestConstants.TEST_CORRELATION_ID);

            // Act
            connectionState.NegotiateReservationAsync(connectionContext);

            // Assert
            connectionContext.Received().UpdateConnectionState(Arg.Any<AwaitingReservationCredentials>());
        }

        [TestMethod]
        public void NegotiateShouldThrowUncorrelatedPacketExceptionWhenThePolicyHasTheWrongCorrelationId()
        {
            // Arrange
            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.SessionPolicy.Returns(new MultiplayerSessionPolicy("wrong", false, TestConstants.TEST_MAX_PLAYER_CONNECTIONS, false));

            EstablishingSessionPolicy connectionState = new EstablishingSessionPolicy(TestConstants.TEST_CORRELATION_ID);

            // Act
            Action action = () => connectionState.NegotiateReservationAsync(connectionContext);

            // Assert
            action.Should().Throw<UncorrelatedPacketException>();
        }

        [TestMethod]
        public void NegotiateShouldThrowInvalidOperationExceptionIfTheSessionPolicyIsNull()
        {
            // Arrange
            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.SessionPolicy.Returns((MultiplayerSessionPolicy)null);

            EstablishingSessionPolicy connectionState = new EstablishingSessionPolicy(TestConstants.TEST_CORRELATION_ID);

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
            EstablishingSessionPolicy connectionState = new EstablishingSessionPolicy(TestConstants.TEST_CORRELATION_ID);

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

            EstablishingSessionPolicy connectionState = new EstablishingSessionPolicy(TestConstants.TEST_CORRELATION_ID);

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

            EstablishingSessionPolicy connectionState = new EstablishingSessionPolicy(TestConstants.TEST_CORRELATION_ID);

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

            EstablishingSessionPolicy connectionState = new EstablishingSessionPolicy(TestConstants.TEST_CORRELATION_ID);

            // Act
            connectionState.Disconnect(connectionContext);

            // Assert
            connectionContext.Received().UpdateConnectionState(Arg.Any<Disconnected>());
        }
    }
}
