using NitroxClient.GameLogic.PlayerModel.Abstract;
using NitroxClient.Unity.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModel
{
    public class RegularDiveSuitBuilder : IPlayerModelBuilder
    {
        private readonly GameObject modelGeometry;

        public RegularDiveSuitBuilder(GameObject modelGeometry)
        {
            this.modelGeometry = modelGeometry;
        }

        public void Build(INitroxPlayer player)
        {
            
        }
    }
}
