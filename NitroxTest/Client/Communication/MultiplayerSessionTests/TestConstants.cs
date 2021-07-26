using NitroxClient.Communication.Abstract;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NSubstitute;

namespace NitroxTest.Client.Communication.MultiplayerSessionTests
{
    internal static class TestConstants
    {
        public const string TEST_IP_ADDRESS = "#.#.#.#";
        public const int TEST_SERVER_PORT = 11000;
        public const ushort TEST_PLAYER_ID = 1;
        public const string TEST_PLAYER_NAME = "TEST";
        public const string TEST_RESERVATION_KEY = "@#*(&";
        public const string TEST_CORRELATION_ID = "CORRELATED";
        public const MultiplayerSessionReservationState TEST_REJECTION_STATE = MultiplayerSessionReservationState.REJECTED | MultiplayerSessionReservationState.UNIQUE_PLAYER_NAME_CONSTRAINT_VIOLATED;
        public static readonly AuthenticationContext TEST_AUTHENTICATION_CONTEXT = new AuthenticationContext(TEST_PLAYER_NAME);
        public static readonly MultiplayerSessionPolicy TEST_SESSION_POLICY = new MultiplayerSessionPolicy(TEST_CORRELATION_ID, false);
        public static readonly PlayerSettings TEST_PLAYER_SETTINGS = new PlayerSettings(RandomColorGenerator.GenerateColor());
        public static readonly IMultiplayerSessionConnectionState TEST_CONNECTION_STATE = Substitute.For<IMultiplayerSessionConnectionState>();
    }
}
