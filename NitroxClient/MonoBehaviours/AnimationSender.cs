using NitroxClient.GameLogic;
using NitroxModel.Core;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class AnimationSender : MonoBehaviour
    {
        private LocalPlayer localPlayer;

        AnimChangeState lastUnderwaterState = AnimChangeState.UNSET;

        public void Awake()
        {
            localPlayer = NitroxServiceLocator.LocateService<LocalPlayer>();
        }

        public void Update()
        {
            AnimChangeState underwaterState = (AnimChangeState)(Player.main.IsUnderwater() ? 1 : 0);
            if (lastUnderwaterState != underwaterState)
            {
                localPlayer.AnimationChange(AnimChangeType.UNDERWATER, underwaterState);
                lastUnderwaterState = underwaterState;
            }
        }
    }

    public enum AnimChangeState
    {
        OFF,
        ON,
        UNSET
    }

    public enum AnimChangeType
    {
        UNDERWATER,
        BENCH
    }
}
