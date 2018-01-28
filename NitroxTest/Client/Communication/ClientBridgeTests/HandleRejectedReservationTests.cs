using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NitroxClient.Communication.Abstract;
using FluentAssertions;
using NitroxClient.Communication;
using NitroxClient.Communication.Exceptions;
using NitroxModel.Packets;

namespace NitroxTest.Client.Communication.ClientBridgeTests
{
    [TestClass]
    public class HandleRejectedReservationTests
    {
        private ClientBridge clientBridge;
        private string correlationId;

        [TestInitialize]
        public void GivenAnInitializedClientBridge()
        {
            //Given
            var serverClient = Substitute.For<IClient>();
            serverClient.IsConnected.Returns(false);
            serverClient
                .When(client => client.start(Arg.Any<string>()))
                .Do(info => serverClient.IsConnected.Returns(true));

            serverClient
                .When(client => client.send(Arg.Any<ReservePlayerSlot>()))
                .Do(info => this.correlationId = info.Arg<ReservePlayerSlot>().CorrelationId);

            this.clientBridge = new ClientBridge(serverClient);
        }

        [TestMethod]
        public void TheBridgeShouldBeInRejectedStateAfterHandlingAReservationRejection()
        {
            //When
            this.clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            this.clientBridge.handleRejectedReservation(this.correlationId, ReservationRejectionReason.PlayerNameInUse);

            //Then
            this.clientBridge.CurrentState.Should().Be(ClientBridgeState.ReservationRejected);
            this.clientBridge.ReservationRejectionReason.Should().Be(ReservationRejectionReason.PlayerNameInUse);
        }

        [TestMethod]
        public void TheBridgeShouldThrowAnInvalidRejectionReasonExceptionWhenHandlingARejectionReasonOfNone()
        {
            //When
            this.clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => this.clientBridge.handleRejectedReservation(this.correlationId, ReservationRejectionReason.None);

            //Then
            action.ShouldThrow<InvalidReservationRejectionReasonException>();
        }

        [TestMethod]
        public void TheBridgeShouldThrowAProhibitedReservationRejectionExceptionWhenHandlingAReservationRejectionAfterConfirmingOne()
        {
            //When
            this.clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            this.clientBridge.confirmReservation(correlationId, TestConstants.TEST_RESERVATION_KEY);
            Action action = () => this.clientBridge.handleRejectedReservation(correlationId, ReservationRejectionReason.PlayerNameInUse);

            //Then
            action.ShouldThrow<ProhibitedReservationRejectionException>();
        }

        [TestMethod]
        public void TheBridgeShouldThrowAProhibitedReservationRejectionExceptionWhenHandlingAReservationRejectionAfterOneHasBeenClaimed()
        {
            //When
            this.clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            this.clientBridge.confirmReservation(correlationId, TestConstants.TEST_RESERVATION_KEY);
            this.clientBridge.claimReservation();
            Action action = () => this.clientBridge.handleRejectedReservation(correlationId, ReservationRejectionReason.PlayerNameInUse);

            //Then
            action.ShouldThrow<ProhibitedReservationRejectionException>();
        }

        [TestMethod]
        public void TheBridgeShouldThrowAnUncorrelatedMessageExceptionWhenProvidedWithAnIncorrectCorrelationId()
        {
            //When
            var incorrectCorrelationId = "WRONG";
            this.clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => this.clientBridge.handleRejectedReservation(incorrectCorrelationId, ReservationRejectionReason.PlayerNameInUse);

            //Then
            action.ShouldThrow<UncorrelatedMessageException>();
            this.clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }

        [TestMethod]
        public void TheBridgeShouldThrowAParameterValidationExceptionWhenProvidedWithANullCorrelationId()
        {
            //When
            string nullCorrelationId = null;
            this.clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => this.clientBridge.handleRejectedReservation(nullCorrelationId, ReservationRejectionReason.PlayerNameInUse);

            //Then
            action.ShouldThrow<ParameterValidationException>().And
                .Should().Match<ParameterValidationException>(ex => ex.FaultingParameterName == "correlationId" && ex.Message == "The value cannot be null.");
            this.clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }

        [TestMethod]
        public void TheBridgeShouldThrowAParameterValidationExceptionWhenProvidedWithABlankCorrelationId()
        {
            //When
            string blankCorrelationId = string.Empty;
            this.clientBridge.connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => this.clientBridge.handleRejectedReservation(blankCorrelationId, ReservationRejectionReason.PlayerNameInUse);

            //Then
            action.ShouldThrow<ParameterValidationException>().And
                .Should().Match<ParameterValidationException>(ex => ex.FaultingParameterName == "correlationId" && ex.Message == "The value cannot be blank.");
            this.clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }
    }
}
