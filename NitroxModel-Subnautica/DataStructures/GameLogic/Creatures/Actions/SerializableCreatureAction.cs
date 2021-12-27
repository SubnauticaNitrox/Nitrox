using UnityEngine;
using ZeroFormatter;

namespace NitroxModel_Subnautica.DataStructures.GameLogic.Creatures.Actions
{
    [DynamicUnion]
    public interface SerializableCreatureAction // TODO: create ZeroFormatter union when eventually implemented
    {
        CreatureAction GetCreatureAction(GameObject gameObject);
    }
}
