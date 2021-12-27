using UnityEngine;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Creatures.Actions
{
    public interface SerializableCreatureAction // TODO: create ZeroFormatter union when eventually implemented
    {
        CreatureAction GetCreatureAction(GameObject gameObject);
    }
}
