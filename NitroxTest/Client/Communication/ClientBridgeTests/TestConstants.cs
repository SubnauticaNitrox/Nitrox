using NitroxModel.PlayerSlot;

namespace NitroxTest.Client.Communication.ClientBridgeTests
{
    internal static class TestConstants
    {
        public const string TEST_IP_ADDRESS = "#.#.#.#";
        public const string TEST_PLAYER_NAME = "TEST";
        public const string TEST_RESERVATION_KEY = "@#*(&";
        public const PlayerSlotReservationState TEST_REJECTION_STATE = PlayerSlotReservationState.Rejected | PlayerSlotReservationState.UniquePlayerNameConstraintViolated;
    }
}
