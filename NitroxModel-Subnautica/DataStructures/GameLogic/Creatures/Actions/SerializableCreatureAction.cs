using System;
using UnityEngine;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Creatures.Actions
{
    public interface SerializableCreatureAction
    {
        CreatureAction GetCreatureAction(GameObject gameObject);
    }

    // SerializableCreatureAction is not implemented yet but test require that at least one class inherits it
    [Serializable]
    public class EmptyCreatureAction: SerializableCreatureAction
    {
        public CreatureAction GetCreatureAction(GameObject gameObject) => null;
    }
}
