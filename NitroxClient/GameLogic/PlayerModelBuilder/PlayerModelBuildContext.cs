using NitroxClient.GameLogic.PlayerModelBuilder.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModelBuilder
{
    //Use this context object to develop a fluent api for setting up your player model transformations.
    //EX: Create a TankModelBuilder to handle the regular tank and then add a method here called 'WithRegularTank()' 
    //that instantiates the TankModelBuilder, passes it to the LinkBuilder method, and then returns 'this' 
    public class PlayerModelBuildContext
    {
        private GameObject modelGeometry;

        public PlayerModelBuildContext(GameObject modelGeometry)
        {
            this.modelGeometry = modelGeometry;
        }

        public IPlayerModelBuilder RootBuilder { get; private set; }

        public PlayerModelBuildContext WithPing()
        {
            PlayerPingBuilder builder = new PlayerPingBuilder();
            LinkBuilder(builder);
            return this;
        }

        public PlayerModelBuildContext WithRegularDiveSuit()
        {
            RegularDiveSuitBuilder builder = new RegularDiveSuitBuilder(modelGeometry);
            LinkBuilder(builder);
            return this;
        }

        public PlayerModelBuildContext WithRebreather()
        {
            PlayerRebreatherModelBuilder modelBuilder = new PlayerRebreatherModelBuilder(modelGeometry);
            LinkBuilder(modelBuilder);
            return this;
        }

        private void LinkBuilder(BasePlayerModelBuildHandler buildHandler)
        {
            if (RootBuilder != null)
            {
                buildHandler.SetSuccessor((IPlayerModelBuildHandler)RootBuilder);
            }

            RootBuilder = buildHandler;
        }
    }
}
