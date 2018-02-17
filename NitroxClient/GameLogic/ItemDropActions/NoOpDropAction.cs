using UnityEngine;

namespace NitroxClient.GameLogic.ItemDropActions
{
    public class NoOpDropAction : ItemDropAction
    {
        public override void ProcessDroppedItem(GameObject gameObject)
        {
            // No drop action defined for this item - do nothing.
        }
    }
}
