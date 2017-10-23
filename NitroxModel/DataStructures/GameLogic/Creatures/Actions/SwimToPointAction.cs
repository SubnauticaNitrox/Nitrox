using System;
using UnityEngine;

namespace NitroxModel.GameLogic.Creatures.Actions
{
    [Serializable]
    public class SwimToPointAction : SerializableCreatureAction
    {
        public Vector3 Target { get; }

        public SwimToPointAction(Vector3 target)
        {
            Target = target;
        }

        public CreatureAction GetCreatureAction(GameObject gameObject)
        {
            SwimToPoint swimToPoint = gameObject.GetComponent<SwimToPoint>();

            if (swimToPoint == null)
            {
                swimToPoint = gameObject.AddComponent<SwimToPoint>();
            }
            
            swimToPoint.Target = Target;

            return swimToPoint;
        }
    }
}
