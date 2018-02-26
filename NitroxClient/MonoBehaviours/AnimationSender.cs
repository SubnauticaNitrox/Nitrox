using NitroxClient.GameLogic;
using NitroxModel.Core;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class AnimationSender : MonoBehaviour
    {
        private LocalPlayer localPlayerBroadcaster;

        AnimChangeState lastUnderwaterState = AnimChangeState.UNSET;

        public void Awake()
        {
            localPlayerBroadcaster = NitroxServiceLocator.LocateService<LocalPlayer>();
        }

        public void Update()
        {
            AnimChangeState underwaterState = (AnimChangeState)(Player.main.IsUnderwater() ? 1 : 0);
            if (lastUnderwaterState != underwaterState)
            {
                localPlayerBroadcaster.AnimationChange(AnimChangeType.UNDERWATER, underwaterState);
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
        UNDERWATER
    }
}
