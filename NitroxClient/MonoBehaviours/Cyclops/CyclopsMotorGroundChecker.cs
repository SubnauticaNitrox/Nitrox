using NitroxClient.GameLogic;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

/// <remarks>
/// Ground detection adapted from <see href="https://github.com/Unity-Technologies/Standard-Assets-Characters/blob/master/Assets/_Standard%20Assets/Characters/Scripts/Physics/OpenCharacterController.cs"/>
/// </remarks>
public partial class CyclopsMotor
{
    private const float CAST_DISTANCE = 0.001f;
    private const float CAST_EXTRA_DISTANCE = 0.001f;

    public const QueryTriggerInteraction QuerySetting = QueryTriggerInteraction.Ignore;
    public static readonly int LayerMaskExceptPlayer = ~CyclopsPawn.PLAYER_LAYER;

    /// <summary>
    /// Latest snapshot of the Pawn's global position. It is updated every frame before being used.
    /// </summary>
    public Vector3 Position;
    /// <summary>
    /// Latest snapshot of the globally transformed center offset. It is updated every frame before being used.
    /// </summary>
    public Vector3 Center;
    /// <summary>
    /// <see cref="CharacterController.height"/> scaled by the transform's y global scale
    /// </summary>
    public float Height;
    /// <summary>
    /// <see cref="CharacterController.radius"/> scaled by the transform's maximum global scale parameter
    /// </summary>
    public float Radius;
    /// <summary>
    /// Unscaled <see cref="CharacterController.skinWidth"/>
    /// </summary>
    public float SkinWidth;
    /// <summary>
    /// Snapshot of the latest <see cref="CollisionFlags"/> obtained when simulating movement on the pawn.
    /// </summary>
    private CollisionFlags Collision { get; set; }

    /// <summary>
    /// Checks if Pawn is grounded by up to 2 sphere casts. Updates the registered ground normal accordingly.
    /// </summary>
    public void CheckGrounded(CollisionFlags flags, bool cast)
    {
        if (cast)
        {
            Vector3 lowerPoint = GetLowerPoint();

            grounded = false;
            if (SphereCast(-Up, SkinWidth + CAST_DISTANCE, out RaycastHit hitInfo, lowerPoint, false))
            {
                grounded = true;
                hitInfo.distance = Mathf.Max(0f, hitInfo.distance - SkinWidth);
            }

            if (!grounded && SphereCast(-Up, CAST_DISTANCE + CAST_EXTRA_DISTANCE, out hitInfo, lowerPoint + Up * CAST_EXTRA_DISTANCE, true))
            {
                grounded = true;
                hitInfo.distance = Mathf.Max(0f, hitInfo.distance - SkinWidth);
            }

            groundNormal = hitInfo.normal;
            return;
        }

        // Exceptional case in which movement was made on the ground but the casts failed
        if (flags == CollisionFlags.Below)
        {
            grounded = true;
            groundNormal = Up;
            return;
        }

        grounded = false;
        groundNormal = Vector3.zero;
    }

    public bool SphereCast(Vector3 direction, float distance, out RaycastHit hitInfo, Vector3 spherePosition, bool big)
    {
        float radius = big ? Radius + SkinWidth : Radius;

        if (Physics.SphereCast(spherePosition, radius, direction, out hitInfo, distance + radius, LayerMaskExceptPlayer, QuerySetting))
        {
            return hitInfo.distance <= distance;
        }

        return false;
    }

    public Vector3 GetLowerPoint()
    {
        return Position + Center - Up * (Height * 0.5f - Radius);
    }

    public override void SetControllerHeight(float height, float cameraOffset)
    {
        base.SetControllerHeight(height, cameraOffset);
        RecalculateConstants();
    }

    public override void SetControllerRadius(float radius)
    {
        base.SetControllerRadius(radius);
        RecalculateConstants();
    }

    private void RecalculateConstants()
    {
        Vector3 scale = transform.lossyScale;
        Height = controller.height * scale.y;
        Radius = controller.radius * Mathf.Max(Mathf.Max(scale.x, scale.y), scale.z);
        SkinWidth = controller.skinWidth;
    }
}
