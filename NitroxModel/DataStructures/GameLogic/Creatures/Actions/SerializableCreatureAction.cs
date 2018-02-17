using UnityEngine;

namespace NitroxModel.DataStructures.GameLogic.Creatures.Actions
{
    public interface SerializableCreatureAction
    {
        CreatureAction GetCreatureAction(GameObject gameObject);
    }
}
