using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxClient.GameLogic.PlayerPreferences;
using NitroxModel.MultiplayerSession;
using NSubstitute;
using UnityEngine;

namespace NitroxTest.Client.PlayerPreferences
{
    [TestClass]
    public class PlayerPreferenceManagerTests
    {
        [TestMethod]
        public void ShouldBeAbleToRetrieveANewPreferenceEntry()
        {
            //Given
            PlayerPreferenceState playerPreferenceState = new PlayerPreferenceState();
            playerPreferenceState.Preferences = new Dictionary<string, PlayerPreference>();

            IPreferenceStateProvider stateProvider = Substitute.For<IPreferenceStateProvider>();
            stateProvider.GetPreferenceState().Returns(playerPreferenceState);

            PlayerPreferenceManager playerPreferenceManager = new PlayerPreferenceManager(stateProvider);

            PlayerPreference playerPreference = new PlayerPreference
            {
                PlayerColor = RandomColorGenerator.GenerateColor(),
                PlayerName = TestConstants.TEST_PLAYER_NAME
            };

            //When
            playerPreferenceManager.SetPreference(TestConstants.TEST_IP_ADDRESS, playerPreference);
            PlayerPreference result = playerPreferenceManager.GetPreference(TestConstants.TEST_IP_ADDRESS);

            //Then
            result.Should().NotBe(playerPreference);
            result.PlayerName.Should().Be(TestConstants.TEST_PLAYER_NAME);
            result.PlayerColor.Should().Be(playerPreference.PlayerColor);
        }

        [TestMethod]
        public void ShouldBeAbleToRetrieveUpdatedPreferencesForAnExistingIpAddress()
        {
            //Given
            PlayerPreferenceState playerPreferenceState = new PlayerPreferenceState();
            playerPreferenceState.Preferences = new Dictionary<string, PlayerPreference>();

            IPreferenceStateProvider stateProvider = Substitute.For<IPreferenceStateProvider>();
            stateProvider.GetPreferenceState().Returns(playerPreferenceState);

            PlayerPreferenceManager playerPreferenceManager = new PlayerPreferenceManager(stateProvider);

            PlayerPreference playerPreference = new PlayerPreference
            {
                PlayerColor = RandomColorGenerator.GenerateColor(),
                PlayerName = TestConstants.TEST_PLAYER_NAME
            };

            Color newColor = RandomColorGenerator.GenerateColor();
            PlayerPreference newPlayerPreference = new PlayerPreference
            {
                PlayerColor = newColor,
                PlayerName = TestConstants.TEST_PLAYER_NAME
            };

            //When
            playerPreferenceManager.SetPreference(TestConstants.TEST_IP_ADDRESS, playerPreference);
            playerPreferenceManager.SetPreference(TestConstants.TEST_IP_ADDRESS, newPlayerPreference);
            PlayerPreference result = playerPreferenceManager.GetPreference(TestConstants.TEST_IP_ADDRESS);

            //Then
            result.Should().NotBe(newPlayerPreference);
            result.PlayerColor.Should().Be(newColor);
        }

        [TestMethod]
        public void SetPreferenceShouldThrowExceptionWhenGivenANullIpAddress()
        {
            //Arrange
            PlayerPreferenceState playerPreferenceState = new PlayerPreferenceState();
            playerPreferenceState.Preferences = new Dictionary<string, PlayerPreference>();

            IPreferenceStateProvider stateProvider = Substitute.For<IPreferenceStateProvider>();
            stateProvider.GetPreferenceState().Returns(playerPreferenceState);

            PlayerPreferenceManager playerPreferenceManager = new PlayerPreferenceManager(stateProvider);

            PlayerPreference playerPreference = new PlayerPreference
            {
                PlayerColor = RandomColorGenerator.GenerateColor(),
                PlayerName = TestConstants.TEST_PLAYER_NAME
            };

            //Act
            Action action = () => playerPreferenceManager.SetPreference(null, playerPreference);

            //Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void SetPreferenceShouldThrowExceptionWhenGivenANullJoinServerSettingsReference()
        {
            //Arrange
            PlayerPreferenceState playerPreferenceState = new PlayerPreferenceState();
            playerPreferenceState.Preferences = new Dictionary<string, PlayerPreference>();

            IPreferenceStateProvider stateProvider = Substitute.For<IPreferenceStateProvider>();
            stateProvider.GetPreferenceState().Returns(playerPreferenceState);

            PlayerPreferenceManager playerPreferenceManager = new PlayerPreferenceManager(stateProvider);

            //Act
            Action action = () => playerPreferenceManager.SetPreference(TestConstants.TEST_IP_ADDRESS, null);

            //Assert
            action.ShouldThrow<ArgumentNullException>();
        }

        [TestMethod]
        public void ShouldGetTheLastSetPlayerPreferenceWhenJoiningANewServer()
        {
            //Given
            PlayerPreferenceState playerPreferenceState = new PlayerPreferenceState();
            playerPreferenceState.Preferences = new Dictionary<string, PlayerPreference>();

            IPreferenceStateProvider stateProvider = Substitute.For<IPreferenceStateProvider>();
            stateProvider.GetPreferenceState().Returns(playerPreferenceState);

            PlayerPreferenceManager playerPreferenceManager = new PlayerPreferenceManager(stateProvider);

            PlayerPreference firstPreference = new PlayerPreference
            {
                PlayerColor = RandomColorGenerator.GenerateColor(),
                PlayerName = TestConstants.TEST_PLAYER_NAME
            };

            string firstIpAddress = "127.0.0.1";
            playerPreferenceManager.SetPreference(firstIpAddress, firstPreference);

            PlayerPreference secondPreference = new PlayerPreference
            {
                PlayerColor = RandomColorGenerator.GenerateColor(),
                PlayerName = TestConstants.TEST_PLAYER_NAME
            };

            string secondIpAddress = "123.456.789.321";
            playerPreferenceManager.SetPreference(secondIpAddress, secondPreference);

            PlayerPreference thirdPreference = new PlayerPreference
            {
                PlayerColor = RandomColorGenerator.GenerateColor(),
                PlayerName = TestConstants.TEST_PLAYER_NAME
            };

            string thirdIpAddress = "000.000.000.000";
            playerPreferenceManager.SetPreference(thirdIpAddress, thirdPreference);

            //When
            PlayerPreference result = playerPreferenceManager.GetPreference(TestConstants.TEST_IP_ADDRESS);

            //Then
            result.Should().NotBe(thirdPreference);
            result.PlayerName.Should().Be(thirdPreference.PlayerName);
            result.PlayerColor.Should().Be(thirdPreference.PlayerColor);
        }

        [TestMethod]
        public void ShouldBeAbleToRetrieveADefaultPreferenceWhenThePlayerHasNone()
        {
            //Given
            PlayerPreferenceState playerPreferenceState = new PlayerPreferenceState();
            playerPreferenceState.Preferences = new Dictionary<string, PlayerPreference>();

            IPreferenceStateProvider stateProvider = Substitute.For<IPreferenceStateProvider>();
            stateProvider.GetPreferenceState().Returns(playerPreferenceState);

            PlayerPreferenceManager playerPreferenceManager = new PlayerPreferenceManager(stateProvider);

            //When
            PlayerPreference result = playerPreferenceManager.GetPreference(TestConstants.TEST_IP_ADDRESS);

            //Then
            result.Should().NotBeNull();
            result.PlayerName.Should().BeNullOrEmpty();
            result.PlayerColor.Should().NotBeNull();
        }

        [TestMethod]
        public void GetPreferenceShouldThrowExceptionWhenGivenNullIpAddress()
        {
            //Arrange
            PlayerPreferenceState playerPreferenceState = new PlayerPreferenceState();
            playerPreferenceState.Preferences = new Dictionary<string, PlayerPreference>();

            IPreferenceStateProvider stateProvider = Substitute.For<IPreferenceStateProvider>();
            stateProvider.GetPreferenceState().Returns(playerPreferenceState);

            PlayerPreferenceManager playerPreferenceManager = new PlayerPreferenceManager(stateProvider);

            //Act
            Action action = () => playerPreferenceManager.GetPreference(null);

            //Assert
            action.ShouldThrow<ArgumentNullException>();
        }
    }
}
