using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.Exceptions;
using NitroxModel.Packets;
using NitroxModel.Packets.Exceptions;
using NitroxModel.PlayerSlot;
using NSubstitute;
using System;

namespace NitroxTest.Client.Communication.ClientBridgeTests
{
    [TestClass]
    public class ConfirmReservationTests
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
                .Do(info => this.correlationId = info.Arg<ReservePlayerSlot>().CorrelationId);

            clientBridge = new ClientBridge(serverClient);
        }

        [TestMethod]
        public void TheBridgeShouldBeReservedAfterConfirmingAReservation()
        {
            //When
            clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            clientBridge.ConfirmReservation(correlationId, TestConstants.TEST_RESERVATION_KEY);

            //Then
            clientBridge.CurrentState.Should().Be(ClientBridgeState.Reserved);
            clientBridge.ReservationState.Should().Be(PlayerSlotReservationState.Reserved);
        }

        [TestMethod]
        public void TheBridgeShouldThrowAnInvalidReservationExceptionIfConfirmingAReservationWhileItIsNotWaiting()
        {
            //When
            Action action = () => this.clientBridge.ConfirmReservation(this.correlationId, TestConstants.TEST_RESERVATION_KEY);

            //Then
            action.ShouldThrow<InvalidReservationException>();
            clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }

        [TestMethod]
        public void TheBridgeShouldThrowAnInvalidReservationExceptionIfConfirmingAReserverationWhileItIsAlreadyReserved()
        {
            //When
            clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            clientBridge.ConfirmReservation(correlationId, TestConstants.TEST_RESERVATION_KEY);
            Action action = () => clientBridge.ConfirmReservation(null, null);

            //Then
            action.ShouldThrow<InvalidReservationException>();
            clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }

        [TestMethod]
        public void TheBridgeShouldThrowAnInvalidReservationExceptionIfConfirmingAReserverationWhileItIsAlreadyConnected()
        {
            //When
            clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            clientBridge.ConfirmReservation(correlationId, TestConstants.TEST_RESERVATION_KEY);
            clientBridge.ClaimReservation();
            Action action = () => clientBridge.ConfirmReservation(null, null);

            //Then
            action.ShouldThrow<InvalidReservationException>();
            clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }

        [TestMethod]
        public void TheBridgeShouldThrowAnUncorrelatedMessageExceptionIfConfirmingAReservationWithTheIncorrectCorrelationId()
        {
            //When
            string incorrectCorrelationId = "WRONG";
            clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => this.clientBridge.ConfirmReservation(incorrectCorrelationId, TestConstants.TEST_RESERVATION_KEY);

            //Then
            action.ShouldThrow<UncorrelatedMessageException>();
            clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }

        [TestMethod]
        public void TheBridgeShouldThrowAParameterValidationExceptionIfConfirmaingAReservationWithANullCorrelationId()
        {
            //When
            string nullCorrelationId = null;
            clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => this.clientBridge.ConfirmReservation(nullCorrelationId, TestConstants.TEST_RESERVATION_KEY);

            //Then
            action.ShouldThrow<ParameterValidationException>().And
                .Should().Match<ParameterValidationException>(ex => ex.FaultingParameterName == "correlationId" && ex.Message == "The value cannot be null.");
            clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }

        [TestMethod]
        public void TheBridgeShouldThrowAParameterValidationExceptionIfConfirmaingAReservationWithABlankCorrelationId()
        {
            //When
            string blankCorrelationId = string.Empty;
            clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => clientBridge.ConfirmReservation(blankCorrelationId, TestConstants.TEST_RESERVATION_KEY);

            //Then
            action.ShouldThrow<ParameterValidationException>().And
                .Should().Match<ParameterValidationException>(ex => ex.FaultingParameterName == "correlationId" && ex.Message == "The value cannot be blank.");
            clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }

        [TestMethod]
        public void TheBridgeShouldThrowAParameterValidationExceptionIfConfirmaingAReservationWithANullReservationKey()
        {
            //When
            string nullReservationKey = null;
            clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => this.clientBridge.ConfirmReservation(correlationId, nullReservationKey);

            //Then
            action.ShouldThrow<ParameterValidationException>().And
                .Should().Match<ParameterValidationException>(ex => ex.FaultingParameterName == "reservationKey" && ex.Message == "The value cannot be null.");
            clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);

        }

        [TestMethod]
        public void TheBridgeShouldThrowAParameterValidationExceptionIfConfirmaingAReservationWithABlankReservationKey()
        {
            //When
            string blankReservationKey = string.Empty;
            clientBridge.Connect(TestConstants.TEST_IP_ADDRESS, TestConstants.TEST_PLAYER_NAME);
            Action action = () => this.clientBridge.ConfirmReservation(correlationId, blankReservationKey);

            //Then
            action.ShouldThrow<ParameterValidationException>().And
                .Should().Match<ParameterValidationException>(ex => ex.FaultingParameterName == "reservationKey" && ex.Message == "The value cannot be blank.");
            clientBridge.CurrentState.Should().Be(ClientBridgeState.Failed);
        }
    }
}
