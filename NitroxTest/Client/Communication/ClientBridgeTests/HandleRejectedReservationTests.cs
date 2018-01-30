using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NSubstitute;
using NitroxClient.Communication.Abstract;
using FluentAssertions;
using NitroxClient.Communication;
using NitroxClient.Communication.Exceptions;
using NitroxModel.Packets;
using NitroxModel.PlayerSlot;

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
                .When(client => client.Start(Arg.Any<string>()))
                .Do(info => serverClient.IsConnected.Returns(true));

            serverClient
                .When(client => client.Send(Arg.Any<ReservePlayerSlot>()))
                .Do(info => this.correlationId = info.Arg<ReservePlayerSlot>().CorrelationId);

            this.clientBridge = new ClientBridge(serverClient);
        }

        [TestMethod]
        public void TheBridgeShouldBeInRejectedStateAfterHandlingAReservationRejection()
        {
            //When
            this.clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            this.clientBridge.HandleRejectedReservation(this.correlationId, ReservationRejectionReason.PlayerNameInUse);

            //Then
            this.clientBridge.CurrentState.Should().Be(ClientBridgeState.ReservationRejected);
            this.clientBridge.ReservationRejectionReason.Should().Be(ReservationRejectionReason.PlayerNameInUse);
        }

        [TestMethod]
        public void TheBridgeShouldThrowAnInvalidRejectionReasonExceptionWhenHandlingARejectionReasonOfNone()
        {
            //When
            this.clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => this.clientBridge.HandleRejectedReservation(this.correlationId, ReservationRejectionReason.None);

            //Then
            action.ShouldThrow<InvalidReservationRejectionReasonException>();
        }

        [TestMethod]
        public void TheBridgeShouldThrowAProhibitedReservationRejectionExceptionWhenHandlingAReservationRejectionAfterConfirmingOne()
        {
            //When
            this.clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            this.clientBridge.ConfirmReservation(correlationId, TestConstants.TEST_RESERVATION_KEY);
            Action action = () => this.clientBridge.HandleRejectedReservation(correlationId, ReservationRejectionReason.PlayerNameInUse);

            //Then
            action.ShouldThrow<ProhibitedReservationRejectionException>();
        }

        [TestMethod]
        public void TheBridgeShouldThrowAProhibitedReservationRejectionExceptionWhenHandlingAReservationRejectionAfterOneHasBeenClaimed()
        {
            //When
            this.clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            this.clientBridge.ConfirmReservation(correlationId, TestConstants.TEST_RESERVATION_KEY);
            this.clientBridge.ClaimReservation();
            Action action = () => this.clientBridge.HandleRejectedReservation(correlationId, ReservationRejectionReason.PlayerNameInUse);

            //Then
            action.ShouldThrow<ProhibitedReservationRejectionException>();
        }

        [TestMethod]
        public void TheBridgeShouldThrowAnUncorrelatedMessageExceptionWhenProvidedWithAnIncorrectCorrelationId()
        {
            //When
            var incorrectCorrelationId = "WRONG";
            this.clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => this.clientBridge.HandleRejectedReservation(incorrectCorrelationId, ReservationRejectionReason.PlayerNameInUse);

            //Then
            action.ShouldThrow<UncorrelatedMessageException>();
            this.clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }

        [TestMethod]
        public void TheBridgeShouldThrowAParameterValidationExceptionWhenProvidedWithANullCorrelationId()
        {
            //When
            string nullCorrelationId = null;
            this.clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => this.clientBridge.HandleRejectedReservation(nullCorrelationId, ReservationRejectionReason.PlayerNameInUse);

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
            this.clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => this.clientBridge.HandleRejectedReservation(blankCorrelationId, ReservationRejectionReason.PlayerNameInUse);

            //Then
            action.ShouldThrow<ParameterValidationException>().And
                .Should().Match<ParameterValidationException>(ex => ex.FaultingParameterName == "correlationId" && ex.Message == "The value cannot be blank.");
            this.clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }
    }
}
