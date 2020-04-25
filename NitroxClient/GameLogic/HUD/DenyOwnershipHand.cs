

using UnityEngine;

namespace NitroxClient.GameLogic.HUD
{
    public class DenyOwnershipHand : MonoBehaviour
    {
        void Start()
        {
            // Force the message to go away after a few seconds.
            Destroy(this, 2);
        }

        void Update()
        {
            HandReticle.main.SetInteractText("Another player is interacting with that object.");
            HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
        }
    }
}
