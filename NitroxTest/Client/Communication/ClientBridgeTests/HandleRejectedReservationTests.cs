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
            IClient serverClient = Substitute.For<IClient>();
            serverClient.IsConnected.Returns(false);
            serverClient
                .When(client => client.Start(Arg.Any<string>()))
                .Do(info => serverClient.IsConnected.Returns(true));

            serverClient
                .When(client => client.Send(Arg.Any<ReservePlayerSlot>()))
                .Do(info => correlationId = info.Arg<ReservePlayerSlot>().CorrelationId);

            clientBridge = new ClientBridge(serverClient);
        }

        [TestMethod]
        public void TheBridgeShouldBeInRejectedStateAfterHandlingAReservationRejection()
        {
            //When
            clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            clientBridge.HandleRejectedReservation(correlationId, TestConstants.TEST_REJECTION_STATE);

            //Then
            clientBridge.CurrentState.Should().Be(ClientBridgeState.ReservationRejected);
            clientBridge.ReservationState.Should().HaveFlag(TestConstants.TEST_REJECTION_STATE);
        }

        [TestMethod]
        public void TheBridgeShouldThrowAnInvalidRejectionReasonExceptionWhenHandlingARejectionReasonOfNone()
        {
            //When
            clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => clientBridge.HandleRejectedReservation(correlationId, PlayerSlotReservationState.Reserved);

            //Then
            action.ShouldThrow<InvalidReservationRejectionReasonException>();
        }

        [TestMethod]
        public void TheBridgeShouldThrowAProhibitedReservationRejectionExceptionWhenHandlingAReservationRejectionAfterConfirmingOne()
        {
            //When
            clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            clientBridge.ConfirmReservation(correlationId, TestConstants.TEST_RESERVATION_KEY);
            Action action = () => clientBridge.HandleRejectedReservation(correlationId, TestConstants.TEST_REJECTION_STATE);

            //Then
            action.ShouldThrow<ProhibitedReservationRejectionException>();
        }

        [TestMethod]
        public void TheBridgeShouldThrowAProhibitedReservationRejectionExceptionWhenHandlingAReservationRejectionAfterOneHasBeenClaimed()
        {
            //When
            clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            clientBridge.ConfirmReservation(correlationId, TestConstants.TEST_RESERVATION_KEY);
            clientBridge.ClaimReservation();
            Action action = () => clientBridge.HandleRejectedReservation(correlationId, TestConstants.TEST_REJECTION_STATE);

            //Then
            action.ShouldThrow<ProhibitedReservationRejectionException>();
        }

        [TestMethod]
        public void TheBridgeShouldThrowAnUncorrelatedMessageExceptionWhenProvidedWithAnIncorrectCorrelationId()
        {
            //When
            string incorrectCorrelationId = "WRONG";
            clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => clientBridge.HandleRejectedReservation(incorrectCorrelationId, TestConstants.TEST_REJECTION_STATE);

            //Then
            action.ShouldThrow<UncorrelatedMessageException>();
            clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }

        [TestMethod]
        public void TheBridgeShouldThrowAParameterValidationExceptionWhenProvidedWithANullCorrelationId()
        {
            //When
            string nullCorrelationId = null;
            clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => clientBridge.HandleRejectedReservation(nullCorrelationId, TestConstants.TEST_REJECTION_STATE);

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
            clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => this.clientBridge.HandleRejectedReservation(blankCorrelationId, TestConstants.TEST_REJECTION_STATE);

            //Then
            action.ShouldThrow<ParameterValidationException>().And
                .Should().Match<ParameterValidationException>(ex => ex.FaultingParameterName == "correlationId" && ex.Message == "The value cannot be blank.");
            clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }
    }
}
