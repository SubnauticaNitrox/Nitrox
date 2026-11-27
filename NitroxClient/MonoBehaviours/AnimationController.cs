using UnityEngine;

namespace NitroxClient.MonoBehaviours;

// Shouldn't this class be named after the armscontroller?
public class AnimationController : MonoBehaviour
{
    private const float SMOOTHING_SPEED = 4f;
    private Animator animator;
    private ArmsController armsController;
    private float viewTurn;
    private float lastViewTurnUpdateTime;

    public bool UpdatePlayerAnimations { get; set; } = true;
    public Quaternion AimingRotation { get; set; }
    public Vector3 Velocity { get; set; }
    public Quaternion BodyRotation { get; set; }

    private Vector3 smoothedVelocity = Vector3.zero;
    private float smoothViewPitch;

    public void Awake()
    {
        animator = GetComponent<Animator>();
        armsController = GetComponent<ArmsController>();
        lastViewTurnUpdateTime = -1f;

        this["is_underwater"] = true;
    }

    public void FixedUpdate()
    {
        if (UpdatePlayerAnimations)
        {
            Vector3 rotationCorrectedVelocity = transform.rotation.GetInverse() * Velocity;

            smoothedVelocity = UWE.Utils.SlerpVector(smoothedVelocity, rotationCorrectedVelocity, Vector3.Normalize(rotationCorrectedVelocity - smoothedVelocity) * (SMOOTHING_SPEED * Time.fixedDeltaTime));

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

            float currentViewTurn = Time.time - lastViewTurnUpdateTime <= 0.1f ? viewTurn : 0f;
            animator.SetFloat("view_turn", currentViewTurn, armsController.turnAnimationDampTime, Time.fixedDeltaTime);
        }
    }

	public bool this[string name]
	{
		get => animator.GetBool(name);
		set => animator.SetBool(name, value);
	}

	internal void SetFloat(string name, float value)
	{
		animator.SetFloat(name, value);
	}

	internal void SetFloat(int id, float value)
	{
		animator.SetFloat(id, value);
	}

    public void SetViewTurn(float value)
    {
        viewTurn = value;
        lastViewTurnUpdateTime = Time.time;
    }

    public void SetTrigger(string name)
    {
        animator.SetTrigger(name);
    }

	public void Reset()
	{
		animator.Rebind();
		animator.Update(0f);
	}
}
