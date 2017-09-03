using UnityEngine;
using RootMotion.FinalIK;
using System;

namespace NitroxClient.MonoBehaviours
{
    // Shouldn't this class be named after the armscontroller?
    public class AnimationController : MonoBehaviour
    {
        private const float SMOOTHING_SPEED = 4f;
        private Animator animator;

        public bool UpdatePlayerAnimations { get; set; } = true;
        public Quaternion AimingRotation { get; set; }
        public Vector3 Velocity { get; set; }
        public Quaternion BodyRotation { get; set; }

        //public FullBodyBipedIK ik;
        //private ArmAiming leftAim, rightAim;

        private Vector3 smoothedVelocity = Vector3.zero;
        private float smoothViewPitch;

        public void Start()
        {
            //leftAim.FindAimer(gameObject, null);
            animator = GetComponent<Animator>();
            //ik = GetComponent<FullBodyBipedIK>();

            //TODO: FindAimer
            this["is_underwater"] = true;
        }

        public void FixedUpdate()
        {
            if (UpdatePlayerAnimations)
            {
                Vector3 rotationCorrectedVelocity = gameObject.transform.rotation.GetInverse() * Velocity;

                smoothedVelocity = UWE.Utils.SlerpVector(smoothedVelocity, rotationCorrectedVelocity, Vector3.Normalize(rotationCorrectedVelocity - smoothedVelocity) * SMOOTHING_SPEED * Time.fixedDeltaTime);

                animator.SetFloat("move_speed", smoothedVelocity.magnitude);
                animator.SetFloat("move_speed_x", smoothedVelocity.x);
                animator.SetFloat("move_speed_y", smoothedVelocity.y);
                animator.SetFloat("move_speed_z", smoothedVelocity.z);

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

        public bool this[string name]
        {
            get { return animator.GetBool(name); }
            set { animator.SetBool(name, value); }
        }

        internal void SetFloat(string name, float value)
        {
            animator.SetFloat(name, value);
        }

        //private class ArmAiming
        //{
        //    public void FindAimer(GameObject ikObj, Transform aimedXform)
        //    {
        //        // TODO: check where this comes from...
        //        foreach (AimIK aimIK in ikObj.GetComponentsInChildren<AimIK>())
        //        {
        //            if (aimIK.solver.transform == aimedXform)
        //            {
        //                if (aimer != null)
        //                {
        //                    UWE.Utils.LogReport("Found multiple AimIK components on " + ikObj.GetFullHierarchyPath() + " that manipulate the transform " + aimedXform.gameObject.GetFullHierarchyPath(), null);
        //                }
        //                else
        //                {
        //                    aimer = aimIK;
        //                }
        //            }
        //        }
        //        if (aimer == null)
        //        {
        //            UWE.Utils.LogReport("Could not find AimIK comp on " + ikObj.GetFullHierarchyPath() + " for transform " + aimedXform.gameObject.GetFullHierarchyPath(), null);
        //        }
        //    }

        //    public void Update(float smoothTime)
        //    {
        //        if (aimer == null)
        //        {
        //            return;
        //        }
        //        aimer.solver.IKPositionWeight = Mathf.SmoothDamp(aimer.solver.IKPositionWeight, (!shouldAim) ? 0f : 1f, ref weightVelocity, smoothTime);
        //    }

        //    public AimIK aimer;
        //    public bool shouldAim;
        //    private float weightVelocity;
        //}
    }
}
