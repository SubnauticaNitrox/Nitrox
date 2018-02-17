using NitroxClient.GameLogic.PlayerModelBuilder.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModelBuilder
{
    // The vision here is to use this director as our central tool for managing remote model appearance alterations like changing equipment or in-game changes to appearance.
    // This could probably very easily be extended to work with the local Player later down the line.
    public class PlayerModelDirector
    {
        private PlayerModelBuildContext buildContext;
        private readonly RemotePlayer player;

        public PlayerModelDirector(RemotePlayer player)
        {
            this.player = player;
        }

        public PlayerModelBuildContext StagePlayer()
        {
            GameObject modelGeometry = player.PlayerModel.transform.Find("male_geo").gameObject;
            buildContext = new PlayerModelBuildContext(modelGeometry);
            return buildContext;
        }

        public void Construct()
        {
            if (buildContext?.RootBuilder != null)
            {
                IPlayerModelBuilder builder = buildContext.RootBuilder;
                builder.Build(player);
                buildContext = null;
            }
        }
    }
}
