﻿using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class AnimationSender : MonoBehaviour
    {
        AnimChangeState lastUnderwaterState = AnimChangeState.Unset;
        public void Update()
        {
            AnimChangeState underwaterState = (AnimChangeState)(Player.main.GetIsUnderwater() ? 1 : 0);
            if (lastUnderwaterState != underwaterState)
            {
                Multiplayer.Logic.Player.AnimationChange(AnimChangeType.Underwater, underwaterState);
                lastUnderwaterState = underwaterState;
            }
        }
    }

    public enum AnimChangeState
    {
        Off,
        On,
        Unset
    }

    public enum AnimChangeType
    {
        Underwater
    }
}
