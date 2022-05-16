using UnityEngine;

namespace NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract
{
    public interface ILocalNitroxPlayer : INitroxPlayer
    {
        //This serves as a "clean" player model to clone from. The root player model is going to be recolored as well
        //which would change our HSV filter parameters. Who wants to hit a moving target?
        GameObject BodyPrototype { get; }
    }
}
