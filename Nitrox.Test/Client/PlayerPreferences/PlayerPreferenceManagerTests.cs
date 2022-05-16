using System;
using System.Collections.Generic;
using FluentAssertions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using NitroxClient.GameLogic.PlayerLogic.PlayerPreferences;
using NitroxModel.MultiplayerSession;
using NitroxModel_Subnautica.DataStructures;
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

            PlayerPreference playerPreference = new PlayerPreference(TestConstants.TEST_PLAYER_NAME, RandomColorGenerator.GenerateColor().ToUnity());
            Color preferredColor = playerPreference.PreferredColor();

            //When
            playerPreferenceManager.SetPreference(TestConstants.TEST_IP_ADDRESS, playerPreference);
            PlayerPreference result = playerPreferenceManager.GetPreference(TestConstants.TEST_IP_ADDRESS);

            //Then
            result.PlayerName.Should().Be(TestConstants.TEST_PLAYER_NAME);
            result.RedAdditive.Should().Be(preferredColor.r);
            result.GreenAdditive.Should().Be(preferredColor.g);
            result.BlueAdditive.Should().Be(preferredColor.b);
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

            PlayerPreference playerPreference = new PlayerPreference(TestConstants.TEST_PLAYER_NAME, RandomColorGenerator.GenerateColor().ToUnity());

            Color newColor = RandomColorGenerator.GenerateColor().ToUnity();
            PlayerPreference newPlayerPreference = new PlayerPreference(TestConstants.TEST_PLAYER_NAME, newColor);

            //When
            playerPreferenceManager.SetPreference(TestConstants.TEST_IP_ADDRESS, playerPreference);
            playerPreferenceManager.SetPreference(TestConstants.TEST_IP_ADDRESS, newPlayerPreference);
            PlayerPreference result = playerPreferenceManager.GetPreference(TestConstants.TEST_IP_ADDRESS);

            //Then
            result.RedAdditive.Should().Be(newColor.r);
            result.GreenAdditive.Should().Be(newColor.g);
            result.BlueAdditive.Should().Be(newColor.b);
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

            PlayerPreference playerPreference = new PlayerPreference(TestConstants.TEST_PLAYER_NAME, RandomColorGenerator.GenerateColor().ToUnity());

            //Act
            Action action = () => playerPreferenceManager.SetPreference(null, playerPreference);

            //Assert
            action.Should().Throw<ArgumentNullException>();
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
            action.Should().Throw<ArgumentNullException>();
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

            PlayerPreference firstPreference = new PlayerPreference(TestConstants.TEST_PLAYER_NAME, RandomColorGenerator.GenerateColor().ToUnity());

            string firstIpAddress = "127.0.0.1";
            playerPreferenceManager.SetPreference(firstIpAddress, firstPreference);

            PlayerPreference secondPreference = new PlayerPreference(TestConstants.TEST_PLAYER_NAME, RandomColorGenerator.GenerateColor().ToUnity());

            string secondIpAddress = "123.456.789.321";
            playerPreferenceManager.SetPreference(secondIpAddress, secondPreference);

            PlayerPreference thirdPreference = new PlayerPreference(TestConstants.TEST_PLAYER_NAME, RandomColorGenerator.GenerateColor().ToUnity());
            Color expectedColor = thirdPreference.PreferredColor();

            string thirdIpAddress = "000.000.000.000";
            playerPreferenceManager.SetPreference(thirdIpAddress, thirdPreference);

            //When
            PlayerPreference result = playerPreferenceManager.GetPreference(TestConstants.TEST_IP_ADDRESS);

            //Then
            result.PlayerName.Should().Be(thirdPreference.PlayerName);
            result.RedAdditive.Should().Be(expectedColor.r);
            result.GreenAdditive.Should().Be(expectedColor.g);
            result.BlueAdditive.Should().Be(expectedColor.b);
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
            action.Should().Throw<ArgumentNullException>();
        }
    }
}
