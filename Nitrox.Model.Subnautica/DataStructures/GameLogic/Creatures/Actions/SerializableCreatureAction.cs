using UnityEngine;

namespace Nitrox.Model.Subnautica.DataStructures.GameLogic.Creatures.Actions
{
    public interface SerializableCreatureAction
    {
        CreatureAction GetCreatureAction(GameObject gameObject);
    }
}
