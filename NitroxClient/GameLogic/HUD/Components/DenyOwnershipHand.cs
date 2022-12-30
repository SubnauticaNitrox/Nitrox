using UnityEngine;

namespace NitroxClient.GameLogic.HUD.Components
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
            //TODO: Check if this should be Hand
            HandReticle.main.SetText(HandReticle.TextType.Hand, "Another player is interacting with that object.", false, GameInput.Button.None);
            HandReticle.main.SetText(HandReticle.TextType.HandSubscript, string.Empty, false, GameInput.Button.None);
            HandReticle.main.SetIcon(HandReticle.IconType.HandDeny, 1f);
        }
    }
}
