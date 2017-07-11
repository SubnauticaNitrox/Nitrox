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
        Animator animator;

        public void Start()
        {
            animator = gameObject.GetComponent<Animator>();
        }

        public void Update()
        {
            animator.SetBool("is_underwater", true);
        }

        Vector3 lastPosition = Vector3.zero;
        Vector3 velocity = Vector3.zero;
        float speed;
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
    }
}
