using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class AnimationSender : MonoBehaviour
    {
        AnimChangeState lastUnderwaterState = AnimChangeState.UNSET;
        public void Update()
        {
            AnimChangeState underwaterState = (AnimChangeState)(Player.main.IsUnderwater() ? 1 : 0);
            if (lastUnderwaterState != underwaterState)
            {
                Multiplayer.Logic.Player.AnimationChange(AnimChangeType.UNDERWATER, underwaterState);
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
