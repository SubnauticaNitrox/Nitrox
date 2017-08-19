using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class AnimationController : MonoBehaviour
    {
        private const float SMOOTHING_SPEED = 6f;
        public Animator animator;

        public bool UpdatePlayerAnimations { get; set; } = true;
        public Quaternion AimingRotation { get; set; }

        private Vector3 lastPosition = Vector3.zero;
        private Vector3 smoothedVelocity = Vector3.zero;

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
            if (UpdatePlayerAnimations)
            {
                foreach (KeyValuePair<string, bool> kvp in animationStatusById)
                {
                    //For whatever reason, attempting to setbool once most of the time won't work
                    //Will investigate soon but this seems to work for now
                    animator.SetBool(kvp.Key, kvp.Value);
                }
            }
        }

        public void FixedUpdate()
        {
            if (UpdatePlayerAnimations)
            {
                Vector3 velocity = AimingRotation.GetInverse() * (1 / Time.fixedDeltaTime * (transform.position - lastPosition));
                lastPosition = transform.position;

                smoothedVelocity = UWE.Utils.SlerpVector(smoothedVelocity, velocity, Vector3.Normalize(velocity - smoothedVelocity) * SMOOTHING_SPEED * Time.fixedDeltaTime);
                SafeAnimator.SetFloat(animator, "move_speed", smoothedVelocity.magnitude);
                SafeAnimator.SetFloat(animator, "move_speed_x", smoothedVelocity.x);
                SafeAnimator.SetFloat(animator, "move_speed_y", smoothedVelocity.y);
                SafeAnimator.SetFloat(animator, "move_speed_z", smoothedVelocity.z);

                float num = AimingRotation.eulerAngles.x;
                if (num > 180f)
                {
                    num -= 360f;
                }
                num = -num;
                animator.SetFloat("view_pitch", num);
            }
        }

        public void SetBool(string name, bool value)
        {
            animationStatusById[name] = value;
        }
    }
}
