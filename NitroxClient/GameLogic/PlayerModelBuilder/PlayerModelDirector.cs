using System.Collections.Generic;
using NitroxClient.GameLogic.PlayerModelBuilder.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModelBuilder
{
    //TODO: Refactor this into something that is event driven.
    public class PlayerModelDirector
    {
        private readonly INitroxPlayer player;
        private readonly List<IPlayerModelBuilder> playerModelBuilders = new List<IPlayerModelBuilder>();

        public PlayerModelDirector(INitroxPlayer player)
        {
            this.player = player;
        }

        public PlayerModelDirector AddPing()
        {
            PlayerPingBuilder builder = new PlayerPingBuilder();
            playerModelBuilders.Add(builder);
            return this;
        }

        public PlayerModelDirector AddDiveSuit()
        {
            GameObject modelGeometry = player.PlayerModel.transform.Find("male_geo").gameObject;
            RegularDiveSuitBuilder builder = new RegularDiveSuitBuilder(modelGeometry);
            playerModelBuilders.Add(builder);

            return this;
        }

        public void Construct()
        {
            playerModelBuilders.ForEach(builder => builder.Build(player));
        }
    }
}
