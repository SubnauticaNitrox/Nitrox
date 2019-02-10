using UnityEngine;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Creatures.Actions
{
    public interface SerializableCreatureAction
    {
        CreatureAction GetCreatureAction(GameObject gameObject);
    }
}
