using System.Collections.Generic;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class AnimationController : MonoBehaviour
    {
        private const float SMOOTHING_SPEED = 4f;
        private Animator animator;

        public bool UpdatePlayerAnimations { get; set; } = true;
        public Quaternion AimingRotation { get; set; }
        public Vector3 Velocity { get; set; }
        public Quaternion BodyRotation { get; set; }

        private Vector3 smoothedVelocity = Vector3.zero;
        private float smoothViewPitch;


        private Dictionary<string, bool> animationStatusById = new Dictionary<string, bool>()
        {
            { "is_underwater", true }
        };

        public void Start()
        {
            animator = GetComponent<Animator>();
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
                Vector3 rotationCorrectedVelocity = gameObject.transform.rotation.GetInverse() * Velocity;

                smoothedVelocity = UWE.Utils.SlerpVector(smoothedVelocity, rotationCorrectedVelocity, Vector3.Normalize(rotationCorrectedVelocity - smoothedVelocity) * SMOOTHING_SPEED * Time.fixedDeltaTime);

                SafeAnimator.SetFloat(animator, "move_speed", smoothedVelocity.magnitude);
                SafeAnimator.SetFloat(animator, "move_speed_x", smoothedVelocity.x);
                SafeAnimator.SetFloat(animator, "move_speed_y", smoothedVelocity.y);
                SafeAnimator.SetFloat(animator, "move_speed_z", smoothedVelocity.z);

                float viewPitch = AimingRotation.eulerAngles.x;
                if (viewPitch > 180f)
                {
                    viewPitch -= 360f;
                }
                viewPitch = -viewPitch;
                smoothViewPitch = Mathf.Lerp(smoothViewPitch, viewPitch, 4f * Time.fixedDeltaTime);
                animator.SetFloat("view_pitch", smoothViewPitch);
            }
        }

        public void SetBool(string name, bool value)
        {
            animationStatusById[name] = value;
        }
    }
}
