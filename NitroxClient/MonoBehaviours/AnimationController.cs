using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class AnimationController : MonoBehaviour
    {
        public static readonly float SMOOTHING_SPEED = 0.95f;
        public Animator animator;

        Vector3 lastPosition = Vector3.zero;
        Vector3 velocity = Vector3.zero;
        float speed;

        private Dictionary<string, bool> animationStatusById = new Dictionary<string, bool>()
        {
            { "is_underwater", true }
        };

        public void Start()
        {
            animator = gameObject.GetComponent<Animator>();
        }

        public void Update()
        {
            foreach (KeyValuePair<string, bool> kvp in animationStatusById)
            {
                //For whatever reason, attempting to setbool once most of the time won't work
                //Will investigate soon but this seems to work for now
                animator.SetBool(kvp.Key, kvp.Value);
            }
        }

        public void FixedUpdate()
        {
            velocity += (transform.position - lastPosition) * 2f;
            speed = velocity.magnitude;
            lastPosition = transform.position;

            animator.SetFloat("move_speed", speed);
            animator.SetFloat("move_speed_x", velocity.x);
            animator.SetFloat("move_speed_y", velocity.y);
            animator.SetFloat("move_speed_z", velocity.z);

            velocity *= SMOOTHING_SPEED;
        }

        public void SetBool(string name, bool value)
        {
            animationStatusById[name] = value;
        }
    }
}
