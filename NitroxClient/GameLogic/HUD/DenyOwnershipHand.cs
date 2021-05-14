

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
#if SUBNAUTICA
            HandReticle.main.SetInteractText("Another player is interacting with that object.");
#elif BELOWZERO
            HandReticle.main.SetText(HandReticle.TextType.Use, "Another player is interacting with that object.", true);
#endif
            HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
        }
    }
}
