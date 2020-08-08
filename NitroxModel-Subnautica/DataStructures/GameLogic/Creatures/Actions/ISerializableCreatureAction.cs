using UnityEngine;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Creatures.Actions
{
    public interface ISerializableCreatureAction
    {
        CreatureAction GetCreatureAction(GameObject gameObject);
    }
}
