using System;
using UnityEngine;

namespace NitroxModel.GameLogic.Creatures.Actions
{
    /**
     * Simple clone of SwimToTarget that can take a Vector3 or generate a target randomly
     */
    public class SwimToPoint : CreatureAction
    {
        public Vector3 Target { get; set; }

        public float swimVelocity = 4f;
        public float swimInterval = 1f;
        private float timeNextSwim;
        private float targetTime = -1f;
        
        public void AssignRandomTarget()
        {
            Target = gameObject.transform.position + (UnityEngine.Random.onUnitSphere * 10);
            Console.WriteLine(this.gameObject.name + " moving to random spot: " + Target);
        }
                
        public override float Evaluate(Creature creature)
        {
            if (Target == null)
            {
                return 0f;
            }

            return base.GetEvaluatePriority();
        }
        
        public override void Perform(Creature creature, float deltaTime)
        {
            if (Target != null && Time.time > timeNextSwim)
            {
                float velocity = swimVelocity;
                if (targetTime > 0f)
                {
                    float num = targetTime - Time.time;
                    float num2 = Vector3.Distance(transform.position, Target);
                    float num3 = 2f * swimVelocity;
                    velocity = ((num <= 0f) ? num3 : Mathf.Clamp(num2 / num, 1f, num3));
                }
                timeNextSwim = Time.time + swimInterval;
                swimBehaviour.SwimTo(Target, velocity);
            }
        }
    }
}
