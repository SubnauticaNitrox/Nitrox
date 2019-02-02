using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSession.ConnectionState;
using NitroxModel.Packets;
using NitroxModel.Packets.Exceptions;
using NSubstitute;

namespace NitroxTest.Client.Communication.MultiplayerSessionTests.ConnectionStateTests.ConnectionNegotiatingStateTests
{
    [TestClass]
    public class EstablishingSessionPolicyStateTests
    {
        [TestMethod]
        public void NegotiateShouldTransitionToAwaitingReservationCredentialsState()
        {
            // Arrange
            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.SessionPolicy.Returns(new MultiplayerSessionPolicy(TestConstants.TEST_CORRELATION_ID, false));

            EstablishingSessionPolicy connectionState = new EstablishingSessionPolicy(TestConstants.TEST_CORRELATION_ID);

            // Act
            connectionState.NegotiateReservation(connectionContext);

            // Assert
            connectionContext.Received().UpdateConnectionState(Arg.Any<AwaitingReservationCredentials>());
        }

        [TestMethod]
        public void NegotiateShouldThrowUncorrelatedPacketExceptionWhenThePolicyHasTheWrongCorrelationId()
        {
            // Arrange
            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.SessionPolicy.Returns(new MultiplayerSessionPolicy("wrong", false));

            EstablishingSessionPolicy connectionState = new EstablishingSessionPolicy(TestConstants.TEST_CORRELATION_ID);

            // Act
            Action action = () => connectionState.NegotiateReservation(connectionContext);

            // Assert
            action.ShouldThrow<UncorrelatedPacketException>();
        }

        [TestMethod]
        public void NegotiateShouldThrowInvalidOperationExceptionIfTheSessionPolicyIsNull()
        {
            // Arrange
            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.SessionPolicy.Returns((MultiplayerSessionPolicy)null);

            EstablishingSessionPolicy connectionState = new EstablishingSessionPolicy(TestConstants.TEST_CORRELATION_ID);

            // Act
            Action action = () => connectionState.NegotiateReservation(connectionContext);

            // Assert
            action.ShouldThrow<InvalidOperationException>();
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
            action.ShouldThrow<InvalidOperationException>();
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
