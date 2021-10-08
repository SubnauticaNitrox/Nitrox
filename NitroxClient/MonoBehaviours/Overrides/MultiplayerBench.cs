using System;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Overrides
{
    public class MultiplayerBench : Bench
    {
        private Side side;

        public static MultiplayerBench FromBench(Bench origin, GameObject target, Side side, GameObject animatorRoot)
        {
            Animator animator = animatorRoot.GetComponent<Animator>();
            Transform playerTarget = animatorRoot.transform.Find("root/cine_loc/player_target");
            Transform playerOutTarget = animatorRoot.transform.Find("out_target");

            MultiplayerBench bench = target.AddComponent<MultiplayerBench>();
            bench.frontObstacleCheck = origin.frontObstacleCheck;
            bench.backObstacleCheck = origin.backObstacleCheck;
            bench.frontAnimRotation = origin.frontAnimRotation;
            bench.backAnimRotation = origin.backAnimRotation;
            bench.checkDistance = origin.checkDistance;
            bench.handText = origin.handText;
            bench.triggerType = origin.triggerType;
            bench.volumeTriggerType = origin.volumeTriggerType;
            bench.standUpCinematicController = origin.standUpCinematicController;
            bench.cinematicController = origin.cinematicController;

            bench.onCinematicStart = new CinematicModeEvent();
            bench.onCinematicEnd = new CinematicModeEvent();
            bench.side = side;

            bench.animator = animator;
            bench.playerTarget = playerTarget;
            bench.cinematicController.animatedTransform = playerTarget;
            bench.cinematicController.animator = animator;
            bench.cinematicController.informGameObject = target;
            bench.standUpCinematicController.animatedTransform = playerTarget;
            bench.standUpCinematicController.endTransform = playerOutTarget;
            bench.standUpCinematicController.animator = animator;
            bench.standUpCinematicController.informGameObject = target;

            return bench;
        }

        public override void OnHandClick(GUIHand hand)
        {
            standUpCinematicController.transform.localPosition = side switch
            {
                Side.LEFT => new Vector3(-0.75f, 0.082f, 0),
                Side.CENTER => new Vector3(0, 0.082f, 0),
                Side.RIGHT => new Vector3(0.75f, 0.082f, 0),
                _ => throw new ArgumentOutOfRangeException()
            };

            base.OnHandClick(hand);
        }

        public enum Side
        {
            LEFT,
            CENTER,
            RIGHT
        }
    }
}
