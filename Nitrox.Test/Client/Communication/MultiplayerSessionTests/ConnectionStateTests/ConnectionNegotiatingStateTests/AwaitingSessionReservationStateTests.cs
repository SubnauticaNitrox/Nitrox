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
    public class AwaitingSessionReservationStateTests
    {
        [TestMethod]
        public void NegotiateShouldTransitionToSessionRevervedAfterReceivingSuccessfulReservation()
        {
            // Arrange
            MultiplayerSessionReservation successfulReservation = new MultiplayerSessionReservation(
                TestConstants.TEST_CORRELATION_ID,
                TestConstants.TEST_PLAYER_ID,
                TestConstants.TEST_RESERVATION_KEY);

            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.Reservation.Returns(successfulReservation);

            AwaitingSessionReservation connectionState = new AwaitingSessionReservation(TestConstants.TEST_CORRELATION_ID);

            // Act
            connectionState.NegotiateReservation(connectionContext);

            // Assert
            connectionContext.Received().UpdateConnectionState(Arg.Any<SessionReserved>());
        }

        [TestMethod]
        public void NegotiateShouldThrowUncorrelatedPacketExceptionWhenTheReservationHasTheWrongCorrelationId()
        {
            // Arrange
            MultiplayerSessionReservation successfulReservation = new MultiplayerSessionReservation(
                "wrong",
                TestConstants.TEST_PLAYER_ID,
                TestConstants.TEST_RESERVATION_KEY);

            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.Reservation.Returns(successfulReservation);

            AwaitingSessionReservation connectionState = new AwaitingSessionReservation(TestConstants.TEST_CORRELATION_ID);

            // Act
            Action action = () => connectionState.NegotiateReservation(connectionContext);

            // Assert
            action.Should().Throw<UncorrelatedPacketException>();
        }

        [TestMethod]
        public void NegotiateShouldTransitionToSessionReservationRejectedAfterReceivingRejectedReservation()
        {
            // Arrange
            MultiplayerSessionReservation rejectedReservation = new MultiplayerSessionReservation(TestConstants.TEST_CORRELATION_ID, TestConstants.TEST_REJECTION_STATE);

            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.Reservation.Returns(rejectedReservation);

            AwaitingSessionReservation connectionState = new AwaitingSessionReservation(TestConstants.TEST_CORRELATION_ID);

            // Act
            connectionState.NegotiateReservation(connectionContext);

            // Assert
            connectionContext.Received().UpdateConnectionState(Arg.Any<SessionReservationRejected>());
        }

        [TestMethod]
        public void NegotiateShouldThrowInvalidOperationExceptionWhenTheReservationIsNull()
        {
            // Arrange
            IMultiplayerSessionConnectionContext connectionContext = Substitute.For<IMultiplayerSessionConnectionContext>();
            connectionContext.Reservation.Returns((MultiplayerSessionReservation)null);

            AwaitingSessionReservation connectionState = new AwaitingSessionReservation(TestConstants.TEST_CORRELATION_ID);

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
            AwaitingSessionReservation connectionState = new AwaitingSessionReservation(TestConstants.TEST_CORRELATION_ID);

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

            AwaitingSessionReservation connectionState = new AwaitingSessionReservation(TestConstants.TEST_CORRELATION_ID);

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

            AwaitingSessionReservation connectionState = new AwaitingSessionReservation(TestConstants.TEST_CORRELATION_ID);

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

            AwaitingSessionReservation connectionState = new AwaitingSessionReservation(TestConstants.TEST_CORRELATION_ID);

            // Act
            connectionState.Disconnect(connectionContext);

            // Assert
            connectionContext.Received().UpdateConnectionState(Arg.Any<Disconnected>());
        }
    }
}
