using System;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxClient.Communication;
using NitroxClient.Communication.Abstract;
using NitroxClient.Communication.MultiplayerSessionState;
using NSubstitute;

namespace NitroxTest.Client.Communication.SessionManagerTests
{
    [TestClass]
    public class MultiplayerSessionManagerStateChangeTests
    {
        private MultiplayerSessionManager sessionManager;
        private IMultiplayerSessionState testState;

        [TestInitialize]
        public void GivenAnInitializedSessionManager()
        {
            //Given
            IClient client = Substitute.For<IClient>();
            sessionManager = new MultiplayerSessionManager(client);
            sessionManager.MonitorEvents<MultiplayerSessionManager>();
            
            testState = Substitute.For<IMultiplayerSessionState>();
        }

        [TestMethod]
        public void ShouldInitializeInDisconnectedStage()
        {
            //Then
            sessionManager.CurrentState.ConnectionStage.Should().Be(MultiplayerSessionConnectionStage.Disconnected);
        }

        [TestMethod]
        public void ShouldNotifyOfStateChange()
        {
            //When
            sessionManager.ChangeState(testState);

            //Then
            sessionManager.ShouldRaise(nameof(IMultiplayerSessionManager.MultiplayerSessionManagerStateChanged));
        }

        [TestMethod]
        public void ShouldApplyStateOnChange()
        {
            //When
            sessionManager.ChangeState(testState);

            //Then
            testState.Received().Apply(Arg.Is(sessionManager));
        }

        [TestMethod]
        public void ShouldThrowInvalidMultiplayerSessionStateExceptionWhenGivenNullReferenceForStateObject()
        {
            //When
            Action action = () => sessionManager.ChangeState(null);

            //Then
            action.ShouldThrow<ArgumentNullException>();
        }
    }
}
