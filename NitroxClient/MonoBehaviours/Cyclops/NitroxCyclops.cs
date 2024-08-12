using System.Collections.Generic;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.ChatUI;
using NitroxClient.GameLogic.PlayerLogic;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Cyclops;

/// <summary>
/// Script responsible for managing all player movement-related interactions.
/// </summary>
public class NitroxCyclops : MonoBehaviour
{
    public VirtualCyclops Virtual;
    private CyclopsMotor cyclopsMotor;
    private SubRoot subRoot;
    private SubControl subControl;
    private Rigidbody rigidbody;
    private WorldForces worldForces;
    private Stabilizer stabilizer;
    private CharacterController controller;
    private int ballasts;

    public readonly Dictionary<INitroxPlayer, CyclopsPawn> Pawns = [];

    public void Start()
    {
        cyclopsMotor = Player.mainObject.GetComponent<CyclopsMotor>();
        subRoot = GetComponent<SubRoot>();
        subControl = GetComponent<SubControl>();
        rigidbody = GetComponent<Rigidbody>();
        worldForces = GetComponent<WorldForces>();
        stabilizer = GetComponent<Stabilizer>();
        controller = cyclopsMotor.controller;
        ballasts = GetComponentsInChildren<BallastWeight>(true).Length;

        UWE.Utils.SetIsKinematicAndUpdateInterpolation(rigidbody, false, true);

        GetComponent<SubFire>().enabled = false;

        FixSubNameEditor();
    }

    /// <summary>
    /// Triggers required on-remove callbacks on children player objects, including the local player.
    /// </summary>
    public void RemoveAllPlayers()
    {
        foreach (RemotePlayerIdentifier remotePlayerIdentifier in GetComponentsInChildren<RemotePlayerIdentifier>(true))
        {
            remotePlayerIdentifier.RemotePlayer.ResetStates();
            OnPlayerExit(remotePlayerIdentifier.RemotePlayer);
        }
        OnLocalPlayerExit();
    }

    /// <summary>
    /// Parents local player to the cyclops and registers it in the current cyclops.
    /// </summary>
    public void OnLocalPlayerEnter()
    {
        Virtual = VirtualCyclops.Instance;
        Virtual.SetCurrentCyclops(this);

        Player.mainObject.transform.parent = subRoot.transform;
        CyclopsPawn pawn = AddPawnForPlayer(this.Resolve<ILocalNitroxPlayer>());
        cyclopsMotor.SetCyclops(this, subRoot, pawn);
        cyclopsMotor.ToggleCyclopsMotor(true);
    }

    /// <summary>
    /// Unregisters the local player from the current cyclops. Ensures the player is not weirdly rotated when it leaves the cyclops.
    /// </summary>
    public void OnLocalPlayerExit()
    {
        RemovePawnForPlayer(this.Resolve<ILocalNitroxPlayer>());
        Player.main.transform.parent = null;
        Player.main.transform.rotation = Quaternion.identity;
        cyclopsMotor.ToggleCyclopsMotor(false);
        cyclopsMotor.Pawn = null;

        Virtual.SetCurrentCyclops(null);
    }

    /// <summary>
    /// Registers a remote player for it to get a pawn in the current cyclops.
    /// </summary>
    public void OnPlayerEnter(RemotePlayer remotePlayer)
    {
        remotePlayer.Pawn = AddPawnForPlayer(remotePlayer);
    }

    /// <summary>
    /// Unregisters a remote player from the current cyclops.
    /// </summary>
    public void OnPlayerExit(RemotePlayer remotePlayer)
    {
        RemovePawnForPlayer(remotePlayer);
        remotePlayer.Pawn = null;
    }

    public void MaintainPawns()
    {
        foreach (CyclopsPawn pawn in Pawns.Values)
        {
            if (pawn.MaintainPredicate())
            {
                pawn.MaintainPosition();
            }
        }
    }

    public CyclopsPawn AddPawnForPlayer(INitroxPlayer player)
    {
        if (!Pawns.TryGetValue(player, out CyclopsPawn pawn))
        {
            pawn = new(player, this);
            Pawns.Add(player, pawn);
        }
        return pawn;
    }

    public void RemovePawnForPlayer(INitroxPlayer player)
    {
        if (Pawns.TryGetValue(player, out CyclopsPawn pawn))
        {
            pawn.Terminate();
        }
        Pawns.Remove(player);
    }

    public void SetBroadcasting()
    {
        worldForces.enabled = true;
        stabilizer.stabilizerEnabled = true;
    }

    public void SetReceiving()
    {
        worldForces.enabled = false;
        stabilizer.stabilizerEnabled = false;
    }

    private void FixSubNameEditor()
    {
        CyclopsSubNameScreen cyclopsSubNameScreen = transform.GetComponentInChildren<CyclopsSubNameScreen>(true);

        cyclopsSubNameScreen.gameObject.AddComponent<SubNameFixer>().Initialize(this, cyclopsSubNameScreen);
    }

    /// <summary>
    /// With the changes to the Player's colliders, the sub name screen doesn't detect the player coming close to it.
    /// The easiest workaround is to replace proximity detection by distance checks.
    /// </summary>
    class SubNameFixer : MonoBehaviour
    {
        private const float DETECTION_RANGE = 3f;
        private const string ANIMATOR_PARAM = "PanelActive";
        private NitroxCyclops cyclops;
        private CyclopsSubNameScreen cyclopsSubNameScreen;
        private bool playerIn;

        public void Initialize(NitroxCyclops cyclops, CyclopsSubNameScreen cyclopsSubNameScreen)
        {
            this.cyclops = cyclops;
            this.cyclopsSubNameScreen = cyclopsSubNameScreen;
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
                    cyclopsSubNameScreen.animator.SetBool(ANIMATOR_PARAM, true);
                    cyclopsSubNameScreen.ContentOn();
                }
                return;
            }

            if (playerIn)
            {
                playerIn = false;
                cyclopsSubNameScreen.animator.SetBool(ANIMATOR_PARAM, false);
                cyclopsSubNameScreen.Invoke(nameof(cyclopsSubNameScreen.ContentOff), 0.5f);
            }
        }
    }

    // TODO: all of the below stuff is purely for testing and will probably get removed before merge
    // EXCEPT for the MaintainPawns line in Update()
    private Vector3 forward => subRoot.subAxis.forward;
    private Vector3 up => subRoot.subAxis.up;
    private Vector3 right => subRoot.subAxis.right;

    public bool Autopilot;
    public bool Sinus;
    public bool Rolling;
    public bool Torqing;
    public bool RenderersToggled;

    public float Forward;
    public float Up;
    public float VerticalPeriod = 1f;
    public float Torque;
    public float Roll;

    public void Reset(Vector3 positionOffset)
    {
        Torqing = false;
        Rolling = false;
        rigidbody.velocity = Vector3.zero;
        rigidbody.angularVelocity = Vector3.zero;
        transform.position = new Vector3(70f, -16f, 0f) + positionOffset;
        transform.rotation = Quaternion.Euler(new(360f, 270f, 0f));

        if (Player.main.currentSub == subRoot || !Player.main.currentSub)
        {
            Player.main.SetCurrentSub(subRoot, true);
            Player.main.SetPosition(transform.position + up);
            cyclopsMotor.ToggleCyclopsMotor(true);
            cyclopsMotor.Pawn.SetReference();
        }
    }

    private double latestReset;

    public void ResetAll()
    {
        Vector3 offset = Vector3.zero;
        foreach (NitroxCyclops cyclops in LargeWorldStreamer.main.globalRoot.GetComponentsInChildren<NitroxCyclops>())
        {
            offset += new Vector3(60f, 30f, 0);
            cyclops.Reset(offset);
        }

        Log.InGame("Reset all cyclops");
    }

    public void Update()
    {
        if (!this.Resolve<PlayerChatManager>().IsChatSelected && !DevConsole.instance.selected)
        {
            if (Input.GetKeyUp(KeyCode.R))
            {
                if (latestReset + 10d < DayNightCycle.main.timePassed)
                {
                    latestReset = DayNightCycle.main.timePassed;
                    ResetAll();
                }
            }
            if (Input.GetKeyUp(KeyCode.N))
            {
                SetReceiving();
                Rolling = !Rolling;
                Autopilot = true;
                Roll = Rolling.ToFloat();
                Log.InGame($"Rolling: {Rolling}");
            }
            if (Input.GetKeyUp(KeyCode.B))
            {
                SetReceiving();
                Torqing = !Torqing;
                Autopilot = true;
                Torque = Torqing.ToFloat();
                Log.InGame($"Torqing: {Torqing}");
            }
        }

        MaintainPawns();
    }

    public void FixedUpdate()
    {
        MoveAutopilot();
    }

    public void MoveAutopilot()
    {
        if (!Autopilot)
        {
            return;
        }

        // https://docs.unity3d.com/ScriptReference/Rigidbody.AddTorque.html
        Vector3 cyclopsTorqueFactor = up * subControl.BaseTurningTorque * subControl.turnScale;
        rigidbody.angularVelocity += cyclopsTorqueFactor * Torque * Time.fixedDeltaTime;

        Vector3 cyclopsRollFactor = right * subControl.BaseTurningTorque * subControl.turnScale;
        rigidbody.angularVelocity += cyclopsRollFactor * Roll * Time.fixedDeltaTime;

        // https://docs.unity3d.com/ScriptReference/Rigidbody.AddForce.html
        Vector3 cyclopsVerticalFactor = up * (subControl.BaseVerticalAccel + ballasts * subControl.AccelPerBallast) * subControl.accelScale;
        if (Sinus)
        {
            cyclopsVerticalFactor *= Mathf.Sin(2 * Mathf.PI * Time.fixedTime / VerticalPeriod);
        }
        rigidbody.velocity += cyclopsVerticalFactor * Up * Time.fixedDeltaTime;

        Vector3 cyclopsForwardFactor = forward * subControl.BaseForwardAccel * subControl.accelScale;
        rigidbody.velocity += cyclopsForwardFactor * Forward * Time.fixedDeltaTime;

        subControl.appliedThrottle = true;
    }
}
