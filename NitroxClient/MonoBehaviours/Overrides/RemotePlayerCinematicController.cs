using System;
using NitroxClient.GameLogic;
using UnityEngine;
using static PlayerCinematicController;

namespace NitroxClient.MonoBehaviours.Overrides;

/// <summary>
/// Override for <see cref="PlayerCinematicController"/>
/// </summary>
public class RemotePlayerCinematicController : MonoBehaviour, IManagedUpdateBehaviour, IManagedLateUpdateBehaviour
{
    [AssertNotNull] public Transform animatedTransform;

    public GameObject informGameObject;

    public Transform endTransform;

    public bool onlyUseEndTransformInVr;

    public bool playInVr;

    public string playerViewAnimationName = "";

    public string playerViewInterpolateAnimParam = "";

    public string animParam = "cinematicMode";

    public string interpolateAnimParam = "";

    public float interpolationTime = 0.25f;

    public float interpolationTimeOut = 0.25f;

    public string receiversAnimParam = "";

    public GameObject[] animParamReceivers;

    public bool interpolateDuringAnimation;

    public bool debug;

    public Animator animator;

    [NonSerialized] public bool cinematicModeActive;

    private Vector3 playerFromPosition = Vector3.zero;

    private Quaternion playerFromRotation = Quaternion.identity;

    private bool onCinematicModeEndCall;

    private float timeStateChanged;

    private State _state;

    private RemotePlayer player;

    private bool subscribed;

    private bool _animState;

    public static int cinematicModeCount { get; private set; }

    public static float cinematicActivityStart { get; private set; }

    private State state
    {
        get => _state;
        set
        {
            timeStateChanged = Time.time;
            _state = value;
        }
    }

    public bool animState
    {
        get => _animState;
        private set
        {
            if (value == _animState)
            {
                return;
            }

            if (debug)
            {
                Debug.Log($"setting cinematic controller {gameObject.name} to: {value}");
            }

            _animState = value;
            if (animParam.Length > 0)
            {
                SafeAnimator.SetBool(animator, animParam, value);
            }

            if (receiversAnimParam.Length > 0)
            {
                for (int i = 0; i < animParamReceivers.Length; i++)
                {
                    animParamReceivers[i].GetComponent<IAnimParamReceiver>()?.ForwardAnimationParameterBool(receiversAnimParam, value);
                }
            }

            if (playerViewAnimationName.Length > 0 && player != null)
            {
                Animator componentInChildren = player.Body.GetComponentInChildren<Animator>();
                if (componentInChildren != null && componentInChildren.gameObject.activeInHierarchy)
                {
                    SafeAnimator.SetBool(componentInChildren, playerViewAnimationName, value);
                }
            }

            SetVrActiveParam();
        }
    }

    public int managedUpdateIndex { get; set; }

    public int managedLateUpdateIndex { get; set; }

    public string GetProfileTag()
    {
        return nameof(RemotePlayerCinematicController);
    }

    public void SetPlayer(RemotePlayer setplayer)
    {
        if (subscribed && player != setplayer)
        {
            Subscribe(player, false);
            Subscribe(setplayer, true);
        }

        player = setplayer;
    }

    public RemotePlayer GetPlayer()
    {
        return player;
    }

    private void AddToUpdateManager()
    {
        BehaviourUpdateUtils.Register((IManagedUpdateBehaviour)this);
        BehaviourUpdateUtils.Register((IManagedLateUpdateBehaviour)this);
    }

    private void RemoveFromUpdateManager()
    {
        BehaviourUpdateUtils.Deregister((IManagedUpdateBehaviour)this);
        BehaviourUpdateUtils.Deregister((IManagedLateUpdateBehaviour)this);
    }

    private void OnEnable()
    {
        AddToUpdateManager();
    }

    private void OnDestroy()
    {
        RemoveFromUpdateManager();
    }

    private void Start()
    {
        SetVrActiveParam();
    }

    private void SetVrActiveParam()
    {
        string paramaterName = "vr_active";
        bool vrAnimationMode = GameOptions.GetVrAnimationMode();
        if (animator != null)
        {
            animator.SetBool(paramaterName, vrAnimationMode);
        }

        foreach (GameObject animatedObject in animParamReceivers)
        {
            animatedObject.GetComponent<IAnimParamReceiver>()?.ForwardAnimationParameterBool(paramaterName, vrAnimationMode);
        }
    }

    private bool UseEndTransform()
    {
        if (endTransform == null)
        {
            return false;
        }

        if (onlyUseEndTransformInVr)
        {
            return GameOptions.GetVrAnimationMode();
        }

        return true;
    }

    private void SkipCinematic(RemotePlayer player)
    {
        this.player = player;
        if (player != null)
        {
            Transform component = player.Body.GetComponent<Transform>();
            if (UseEndTransform())
            {
                component.position = endTransform.position;
                component.rotation = endTransform.rotation;
            }
        }

        if (informGameObject != null)
        {
            informGameObject.SendMessage(nameof(CinematicModeTriggerBase.OnPlayerCinematicModeEnd), this, SendMessageOptions.DontRequireReceiver);
        }
    }

    public void StartCinematicMode(RemotePlayer setplayer)
    {
        if (debug)
        {
            Debug.Log($"{gameObject.name}.StartCinematicMode");
        }

        if (!cinematicModeActive)
        {
            player = null;
            if (!playInVr && GameOptions.GetVrAnimationMode())
            {
                if (debug)
                {
                    Debug.Log($"{gameObject.name} skip cinematic");
                }

                SkipCinematic(setplayer);
                return;
            }

            animator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
            cinematicModeActive = true;
            if (setplayer != null)
            {
                SetPlayer(setplayer);
                Subscribe(player, true);
            }

            state = State.In;
            if (informGameObject != null)
            {
                informGameObject.SendMessage(nameof(DockedVehicleHandTarget.OnPlayerCinematicModeStart), this, SendMessageOptions.DontRequireReceiver);
            }

            if (player != null)
            {
                Transform component = player.Body.GetComponent<Transform>();
                playerFromPosition = component.position;
                playerFromRotation = component.rotation;
                if (playerViewInterpolateAnimParam.Length > 0)
                {
                    SafeAnimator.SetBool(player.Body.GetComponentInChildren<Animator>(), playerViewInterpolateAnimParam, true);
                }
            }

            if (interpolateAnimParam.Length > 0)
            {
                SafeAnimator.SetBool(animator, interpolateAnimParam, true);
            }

            if (interpolateDuringAnimation)
            {
                animState = true;
            }

            if (debug)
            {
                Debug.Log($"{gameObject.name} successfully started cinematic");
            }

            if (cinematicModeCount == 0)
            {
                cinematicActivityStart = Time.time;
            }

            cinematicModeCount++;
        }
        else if (debug)
        {
            Debug.Log($"{gameObject.name} cinematic already active!");
        }
    }

    public void EndCinematicMode(bool reset = false)
    {
        if (cinematicModeActive)
        {
            if (reset) // Added by us
            {
                animator.Rebind();
                animator.Update(0f);
            }

            animator.cullingMode = AnimatorCullingMode.CullCompletely;
            animState = false;
            state = State.None;
            cinematicModeActive = false;
            cinematicModeCount--;
        }
    }

    public void OnPlayerCinematicModeEnd()
    {
        if (!cinematicModeActive || onCinematicModeEndCall)
        {
            return;
        }

        if (player != null)
        {
            UpdatePlayerPosition();
        }

        animState = false;
        if (UseEndTransform())
        {
            state = State.Out;
            if (player != null)
            {
                Transform component = player.Body.GetComponent<Transform>();
                playerFromPosition = component.position;
                playerFromRotation = component.rotation;
            }
        }
        else
        {
            EndCinematicMode();
        }

        if (informGameObject != null)
        {
            onCinematicModeEndCall = true;
            informGameObject.SendMessage(nameof(DockedVehicleHandTarget.OnPlayerCinematicModeEnd), this, SendMessageOptions.DontRequireReceiver);
            onCinematicModeEndCall = false;
        }
    }

    private void UpdatePlayerPosition()
    {
        Transform component = player.Body.GetComponent<Transform>();
        component.position = animatedTransform.position;
        component.rotation = animatedTransform.rotation;
    }

    public void ManagedLateUpdate()
    {
        if (!cinematicModeActive)
        {
            return;
        }

        float num = Time.time - timeStateChanged;
        float timedOutScalar;
        Transform transform = null;
        if (player != null && player.Body)
        {
            transform = player.Body.GetComponent<Transform>();
        }

        bool isVrAnimationMode = !GameOptions.GetVrAnimationMode();
        switch (state)
        {
            case State.In:
                timedOutScalar = interpolationTime != 0f && isVrAnimationMode ? Mathf.Clamp01(num / interpolationTime) : 1f;
                if (player != null && transform)
                {
                    transform.position = Vector3.Lerp(playerFromPosition, animatedTransform.position, timedOutScalar);
                    transform.rotation = Quaternion.Slerp(playerFromRotation, animatedTransform.rotation, timedOutScalar);
                }

                if (timedOutScalar == 1f)
                {
                    state = State.Update;
                    animState = true;
                    if (interpolateAnimParam.Length > 0)
                    {
                        SafeAnimator.SetBool(animator, interpolateAnimParam, false);
                    }

                    if (playerViewInterpolateAnimParam.Length > 0 && player != null)
                    {
                        SafeAnimator.SetBool(player.Body.GetComponentInChildren<Animator>(), playerViewInterpolateAnimParam, false);
                    }
                }

                break;
            case State.Update:
                if (player != null)
                {
                    UpdatePlayerPosition();
                }

                break;
            case State.Out:
                timedOutScalar = interpolationTimeOut != 0f && isVrAnimationMode ? Mathf.Clamp01(num / interpolationTimeOut) : 1f;
                if (player != null && transform)
                {
                    transform.position = Vector3.Lerp(playerFromPosition, endTransform.position, timedOutScalar);
                    transform.rotation = Quaternion.Slerp(playerFromRotation, endTransform.rotation, timedOutScalar);
                }

                if (timedOutScalar == 1f)
                {
                    EndCinematicMode();
                }

                break;
        }
    }

    public bool IsCinematicModeActive()
    {
        return cinematicModeActive;
    }

    private void OnDisable()
    {
        RemoveFromUpdateManager();
        if (subscribed)
        {
            Subscribe(player, false);
        }

        EndCinematicMode();
    }

    private void OnPlayerDeath(RemotePlayer player)
    {
        EndCinematicMode();
        animator.Rebind();
    }

    private void Subscribe(RemotePlayer player, bool state)
    {
        if (player == null)
        {
            subscribed = false;
        }
        else if (subscribed != state)
        {
            if (state)
            {
                player.PlayerDeathEvent.AddHandler(gameObject, OnPlayerDeath);
            }
            else
            {
                player.PlayerDeathEvent.RemoveHandler(gameObject, OnPlayerDeath);
            }

            subscribed = state;
        }
    }

    public void ManagedUpdate()
    {
        if (!cinematicModeActive && subscribed)
        {
            Subscribe(player, false);
        }
    }
}
