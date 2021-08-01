using System.Collections.Generic;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.Input.KeyBindings.Actions
{
    public class GhostSlayerBindingAction : KeyBindingAction
    {
        // arbitrary distance, pretty close
        static readonly float maxSlayDistance = 3f;
        public override void Execute()
        {
            BaseGhost[] ghosts = GameObject.FindObjectsOfType<BaseGhost>();
            List<BaseGhost> nearbyGhosts = new List<BaseGhost>();
            GameObject localPlayer = Utils.GetLocalPlayer().gameObject;
            // grab nearby ghosts so it's more of a targeted action
            ghosts.ForEach((ghost) =>
            {
                if (Vector3.Distance(ghost.transform.position, localPlayer.transform.position) < maxSlayDistance)
                {
                    nearbyGhosts.Add(ghost);
                }
            });
            Log.Info("ghosts found for slaying: " + nearbyGhosts.Count);
            for (int i = nearbyGhosts.Count - 1; i >= 0; i--)
            {
                NitroxServiceLocator.LocateService<Building>().DeconstructionComplete(nearbyGhosts[i].transform.parent.gameObject);
                UnityEngine.GameObject.Destroy(nearbyGhosts[i].transform.parent.gameObject);
            }
        }
    }
}
