using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours.Cyclops;
using UnityEngine;
using UnityEngine.XR;

namespace NitroxClient.MonoBehaviours;

/// <summary>
/// A replacement for <see cref="GroundMotor"/> while Local Player is in a Cyclops.
/// </summary>
public partial class CyclopsMotor : GroundMotor
{
    public GroundMotor ActualMotor { get; private set; }
    public CyclopsPawn Pawn;

    private Transform body;
    private NitroxCyclops cyclops;
    private SubRoot sub;
    private Transform realAxis;
    private Transform virtualAxis;
    private WorldForces worldForces;

    public Vector3 Up => virtualAxis.up;
    public float DeltaTime => Time.fixedDeltaTime;

    private Vector3 verticalVelocity;
    private Vector3 latestVelocity;

    public new void Awake()
    {
        controller = GetComponent<CharacterController>();
        controller.enabled = false;
        controller.stepOffset = controllerSetup.stepOffset;
        controller.slopeLimit = controllerSetup.slopeLimit;

        rb = GetComponent<Rigidbody>();
        playerController = GetComponent<PlayerController>();
        worldForces = GetComponent<WorldForces>();

        body = Player.mainObject.transform.Find("body");
    }

    public override void SetEnabled(bool enabled)
    {
        base.SetEnabled(enabled);
        Setup(enabled);
    }

    public void Initialize(GroundMotor reference)
    {
        ActualMotor = reference;
        movement = reference.movement;
        jumping = reference.jumping;
        movingPlatform = reference.movingPlatform;
        sliding = reference.sliding;
        controllerSetup = reference.controllerSetup;
        floatingModeSetup = reference.floatingModeSetup;
        allowMidAirJumping = reference.allowMidAirJumping;
        minWindSpeedToAffectMovement = reference.minWindSpeedToAffectMovement;
        percentWindDampeningOnGround = reference.percentWindDampeningOnGround;
        percentWindDampeningInAir = reference.percentWindDampeningInAir;
        floatingModeEnabled = reference.floatingModeEnabled;
        forwardMaxSpeed = reference.forwardMaxSpeed;
        backwardMaxSpeed = reference.backwardMaxSpeed;
        strafeMaxSpeed = reference.strafeMaxSpeed;
        verticalMaxSpeed = reference.verticalMaxSpeed;
        climbSpeed = reference.climbSpeed;
        gravity = reference.gravity;
        forwardSprintModifier = reference.forwardSprintModifier;
        strafeSprintModifier = reference.strafeSprintModifier;
        groundAcceleration = reference.groundAcceleration;
        airAcceleration = reference.airAcceleration;
        jumpHeight = reference.jumpHeight;
        SetEnabled(false);
        RecalculateConstants();
    }

    public void SetCyclops(NitroxCyclops cyclops, SubRoot subRoot, CyclopsPawn pawn)
    {
        this.cyclops = cyclops;
        sub = subRoot;
        realAxis = sub.subAxis;
        virtualAxis = cyclops.Virtual.axis;
        Pawn = pawn;
    }

    public void Setup(bool enabled)
    {
        verticalVelocity = Vector3.zero;
        latestVelocity = Vector3.zero;

        if (enabled)
        {
            rb.isKinematic = false;
            Player.wantInterpolate = false;
            rb.detectCollisions = false;
            worldForces.lockInterpolation = true;
            worldForces.enabled = false;
            controller.detectCollisions = false;
            Player.mainCollider.isTrigger = true;
            UWE.Utils.EnterPhysicsSyncSection();
        }
        else
        {
            rb.isKinematic = true;
            Player.wantInterpolate = true;
            worldForces.lockInterpolation = false;
            rb.detectCollisions = true;
            worldForces.enabled = true;
            controller.detectCollisions = true;
            Player.mainCollider.isTrigger = false;
            UWE.Utils.ExitPhysicsSyncSection();
        }

        Pawn?.SetReference();
    }

    public override Vector3 UpdateMove()
    {
        if (!canControl)
        {
            return Vector3.zero;
        }

        // Compute movements velocities based on inputs and previous movement
        Position = Pawn.Position;
        Center = cyclops.Virtual.transform.TransformVector(Pawn.Controller.center);
        Pawn.Handle.transform.localRotation = body.localRotation;

        sprinting = false;
        verticalVelocity += CalculateVerticalVelocity();
        Vector3 horizontalVelocity = CalculateInputVelocity();

        // movement.velocity gives velocity info for the animations and footsteps
        movement.velocity = Move(horizontalVelocity);
        return movement.velocity;
    }

    /// <summary>
    /// Simulates player movement on its pawn and update the grounded state
    /// </summary>
    /// <remarks>
    /// Adapted from <see cref="GroundMotor.UpdateFunction"/>
    /// </remarks>
    /// <returns>Pawn's local velocity</returns>
    public Vector3 Move(Vector3 horizontalVelocity)
    {
        Vector3 beforePosition = Pawn.Position;

        Vector3 velocity = new(horizontalVelocity.x, verticalVelocity.y, horizontalVelocity.z);
        Vector3 movementThisFrame = velocity * DeltaTime;

        float step = Mathf.Max(Pawn.Controller.stepOffset, Mathf.Sqrt(movementThisFrame.x * movementThisFrame.x + movementThisFrame.z * movementThisFrame.z));
        if (grounded)
        {
            movementThisFrame -= step * Up;
        }

        Collision = Pawn.Controller.Move(movementThisFrame);

        float verticalDot = Vector3.Dot(verticalVelocity, Up);

        bool previouslyGrounded = grounded;
        CheckGrounded(Collision, verticalDot <= 0f);

        Vector3 velocityXZ = velocity._X0Z();
        Vector3 instantVelocity = (Pawn.Position - beforePosition) / DeltaTime;
        if (instantVelocity.sqrMagnitude <= 0.2f)
        {
            instantVelocity = velocity;
        }
        if (instantVelocity.y > 0f || Collision == CollisionFlags.None)
        {
            instantVelocity.y = velocity.y;
        }

        latestVelocity = instantVelocity;
        
        Vector3 instantVelocityXZ = instantVelocity._X0Z();
        if (velocityXZ == Vector3.zero)
        {
            latestVelocity = latestVelocity._0Y0();
        }
        else
        {
            float deviation = Vector3.Dot(instantVelocityXZ, velocityXZ) / velocityXZ.sqrMagnitude;
            latestVelocity = velocityXZ * Mathf.Clamp01(deviation) + latestVelocity.y * Up;
        }

        if (latestVelocity.y < velocity.y - 0.001)
        {
            if (latestVelocity.y < 0f)
            {
                latestVelocity.y = velocity.y;
            }
            else
            {
                jumping.holdingJumpButton = false;
            }
        }

        if (grounded)
        {
            verticalVelocity = Vector3.zero;

            if (!previouslyGrounded)
            {
                jumping.jumping = false;
                // Prefilled data is made to not hurt the player at any time when colliding with cyclops, but only to play the noise
                SendMessage(nameof(Player.OnLand), new MovementCollisionData
                {
                    impactVelocity = Vector3.one,
                    surfaceType = VFXSurfaceTypes.metal
                }, SendMessageOptions.DontRequireReceiver);
            }
        }
        // If player is no longer grounded after move
        else if (previouslyGrounded)
        {
            SendMessage("OnFall", SendMessageOptions.DontRequireReceiver);
            Pawn.Handle.transform.localPosition += step * Up;
        }

        return cyclops.transform.rotation * latestVelocity;
    }

    /// <summary>
    /// Calculates vertical velocity variation based on the grounded state.
    /// Code adapted from <see cref="GroundMotor.ApplyGravityAndJumping"/>.
    /// </summary>
    public Vector3 CalculateVerticalVelocity()
    {
        if (!jumpPressed)
        {
            jumping.holdingJumpButton = false;
            jumping.lastButtonDownTime = -100f;
        }
        if (jumpPressed && (jumping.lastButtonDownTime < 0f || flyCheatEnabled))
        {
            jumping.lastButtonDownTime = Time.time;
        }

        Vector3 verticalMove = Vector3.zero;

        if (!grounded)
        {
            verticalMove = -gravity * Up * DeltaTime;
            verticalMove.y = Mathf.Max(verticalMove.y, -movement.maxFallSpeed);
        }
        if (grounded || allowMidAirJumping || flyCheatEnabled)
        {
            if (Time.time - jumping.lastButtonDownTime < 0.2)
            {
                grounded = false;
                jumping.jumping = true;
                jumping.lastStartTime = Time.time;
                jumping.lastButtonDownTime = -100f;
                jumping.holdingJumpButton = true;
                Vector3 jumpDirection = Vector3.Slerp(Up, groundNormal, TooSteep() ? jumping.steepPerpAmount : jumping.perpAmount);
                verticalMove = jumpDirection * CalculateJumpVerticalSpeed(jumping.baseHeight);
                SendMessage("OnJump", SendMessageOptions.DontRequireReceiver);
            }
            else
            {
                jumping.holdingJumpButton = false;
            }
        }
        return verticalMove;
    }

    /// <summary>
    /// Calculates instantaneous horizontal velocity from input for the <see cref="Pawn"/> object.
    /// Code adapted from <see cref="GroundMotor.ApplyInputVelocityChange"/>.
    /// </summary>
    public Vector3 CalculateInputVelocity()
    {
        // Project the movement input to the right rotation
        float moveMinMagnitude = Mathf.Min(1f, movementInputDirection.magnitude);

        // We rotate the input in the right basis
        Vector3 input = movementInputDirection._X0Z();

        Transform forwardRef = Pawn.Handle.transform;

        Vector3 projectedForward = Vector3.ProjectOnPlane(forwardRef.forward, Up).normalized;
        Vector3 projectedRight = Vector3.ProjectOnPlane(forwardRef.right, Up).normalized;

        Vector3 moveDirection = (projectedForward * input.z + projectedRight * input.x).normalized;

        Vector3 velocity;
        // Manage sliding on slopes
        if (grounded && TooSteep())
        {
            velocity = GetSlidingDirection();
            Vector3 moveProjectedOnSlope =  Vector3.Project(movementInputDirection, velocity);
            velocity += moveProjectedOnSlope * sliding.speedControl + (movementInputDirection - moveProjectedOnSlope) * sliding.sidewaysControl;
            velocity *= sliding.slidingSpeed;
        }
        else
        {
            // Apply speed modifiers
            float modifier = 1f;
            if (sprintPressed && grounded)
            {
                float z = movementInputDirection.z;
                if (z > 0f)
                {
                    modifier *= forwardSprintModifier;
                }
                else if (z == 0f)
                {
                    modifier *= strafeSprintModifier;
                }
                sprinting = true;
            }
            velocity = moveDirection * forwardMaxSpeed * modifier * moveMinMagnitude;
        }
        if (XRSettings.enabled)
        {
            velocity *= VROptions.groundMoveScale;
        }

        if (grounded)
        {
            velocity = AdjustGroundVelocityToNormal(velocity, groundNormal);
        }
        else
        {
            latestVelocity.y = 0f;
        }

        float maxSpeed = GetMaxAcceleration(grounded) * DeltaTime;
        
        Vector3 difference = velocity - latestVelocity;
        if (difference.sqrMagnitude > maxSpeed * maxSpeed)
        {
            difference = difference.normalized * maxSpeed;
        }
        latestVelocity += difference;

        if (grounded)
        {
            latestVelocity.y = Mathf.Min(latestVelocity.y, 0f);
        }

        return latestVelocity;
    }

    private new Vector3 GetSlidingDirection()
    {
        return Vector3.ProjectOnPlane(groundNormal, Up).normalized;
    }

    private new bool TooSteep()
    {
        float dotUp = Vector3.Dot(groundNormal, Up);
        return dotUp <= Mathf.Cos(controller.slopeLimit * Mathf.Deg2Rad);
    }

    public void ToggleCyclopsMotor(bool toggled)
    {
        GroundMotor groundMotor = toggled ? this : ActualMotor;
        Player.main.playerController.SetEnabled(false);
        Player.main.groundMotor = groundMotor;

        Player.main.footStepSounds.groundMoveable = groundMotor;
        Player.main.groundMotor = groundMotor;
        Player.main.playerController.groundController = groundMotor;
        if (Player.main.playerController.activeController is GroundMotor)
        {
            Player.main.playerController.activeController = groundMotor;
        }
        // SetMotorMode sets some important variables in the motor abstract class PlayerMotor
        Player.main.playerController.SetMotorMode(Player.MotorMode.Walk);

        Player.main.playerController.SetEnabled(true);
    }
}
