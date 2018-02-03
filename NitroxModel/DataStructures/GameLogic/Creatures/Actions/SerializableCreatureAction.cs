using UnityEngine;

namespace NitroxModel.GameLogic.Creatures.Actions
{
    public interface SerializableCreatureAction
    {
        CreatureAction GetCreatureAction(GameObject gameObject);
    }
}
