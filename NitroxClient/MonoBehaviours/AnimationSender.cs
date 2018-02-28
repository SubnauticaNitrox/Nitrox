using NitroxClient.GameLogic;
using NitroxModel.Core;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class AnimationSender : MonoBehaviour
    {
        private PlayerLogic playerBroadcaster;

        AnimChangeState lastUnderwaterState = AnimChangeState.UNSET;

        public void Awake()
        {
            playerBroadcaster = NitroxServiceLocator.LocateService<PlayerLogic>();
        }

        public void Update()
        {
            AnimChangeState underwaterState = (AnimChangeState)(Player.main.IsUnderwater() ? 1 : 0);
            if (lastUnderwaterState != underwaterState)
            {
                playerBroadcaster.AnimationChange(AnimChangeType.UNDERWATER, underwaterState);
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
