using System;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Cyclops;

/// <summary>
/// With the changes to the Player's colliders, the cyclops doesn't detect the player entering or leaving to triggers
/// The easiest workaround is to replace proximity detection by distance checks.
/// </summary>
/// <remarks>
/// Works for <see cref="CyclopsLightingPanel"/> and <see cref="CyclopsSubNameScreen"/>.
/// </remarks>
public class TriggerWorkaround : MonoBehaviour
{
    private const float DETECTION_RANGE = 5f;
    private const string ANIMATOR_PARAM = "PanelActive";
    private bool playerIn;

    private NitroxCyclops cyclops;
    private Animator animator;
    private Action onEnterCallback;
    private string onExitInvokeCallback;
    private MonoBehaviour targetBehaviour;

    public void Initialize(NitroxCyclops cyclops, Animator animator, Action onEnterCallback, string onExitInvokeCallback, MonoBehaviour targetBehaviour)
    {
        this.cyclops = cyclops;
        this.animator = animator;
        this.onEnterCallback = onEnterCallback;
        this.onExitInvokeCallback = onExitInvokeCallback;
        this.targetBehaviour = targetBehaviour;
    }

    /// <summary>
    /// Code adapted from <see cref="CyclopsSubNameScreen.OnTriggerEnter"/> and <see cref="CyclopsSubNameScreen.OnTriggerExit"/>
    /// </summary>
    public void Update()
    {
        // Virtual is not null only when the local player is aboard
        if (cyclops.Virtual && Vector3.Distance(Player.main.transform.position, transform.position) < DETECTION_RANGE)
        {
            if (!playerIn)
            {
                playerIn = true;
                animator.SetBool(ANIMATOR_PARAM, true);
                onEnterCallback();
                targetBehaviour.CancelInvoke(onExitInvokeCallback);
            }
            return;
        }

        if (playerIn)
        {
            playerIn = false;
            animator.SetBool(ANIMATOR_PARAM, false);
            targetBehaviour.Invoke(onExitInvokeCallback, 0.5f);
        }
    }
}
