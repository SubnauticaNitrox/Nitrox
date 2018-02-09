using NitroxClient.Communication.Abstract;
using NitroxModel.Core;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using NSubstitute;
using SimpleInjector;

namespace NitroxTest.Client.Communication.MultiplayerSessionTests
{
    internal static class TestConstants
    {
        public const string TEST_IP_ADDRESS = "#.#.#.#";
        public const string TEST_PLAYER_NAME = "TEST";
        public const string TEST_RESERVATION_KEY = "@#*(&";
        public const string TEST_CORRELATION_ID = "CORRELATED";
        public const MultiplayerSessionReservationState TEST_REJECTION_STATE = MultiplayerSessionReservationState.Rejected | MultiplayerSessionReservationState.UniquePlayerNameConstraintViolated;
        public static readonly AuthenticationContext TEST_AUTHENTICATION_CONTEXT = new AuthenticationContext(TEST_PLAYER_NAME);
        public static readonly MultiplayerSessionPolicy TEST_SESSION_POLICY = new MultiplayerSessionPolicy(TEST_CORRELATION_ID);
        public static readonly PlayerSettings TEST_PLAYER_SETTINGS = new PlayerSettings();
        public static readonly IMultiplayerSessionConnectionState TEST_CONNECTION_STATE = Substitute.For<IMultiplayerSessionConnectionState>();
    }

    internal class SimpleInjectorContainerBuilder : ISimpleInjectorContainerBuilder
    {
        public Container BuildContainer()
        {
            Container testContainer = new Container();
            
            return testContainer;
        }
    }
}
