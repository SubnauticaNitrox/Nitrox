using System;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModelBuilder
{
    //This is not implemented because it is meant to act as a guide for other developers.
    //If you are ready to tackle handling the visual setup of equipment, then setup a builder like
    //this one and expose it through the PlayerModelBuildContext.
    public class PlayerRebreatherModelBuilder : EquipmentModelBuilder
    {
        public PlayerRebreatherModelBuilder(GameObject modelGeometry)
            : base(modelGeometry)
        {
        }

        protected override void HandleBuild(RemotePlayer player)
        {
            throw new NotImplementedException();
        }
    }
}
