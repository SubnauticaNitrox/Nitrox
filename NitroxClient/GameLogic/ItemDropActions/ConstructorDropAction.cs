using NitroxModel.Helper;
using UnityEngine;

namespace NitroxClient.GameLogic.ItemDropActions
{
    public class ConstructorDropAction : ItemDropAction
    {
        public override void ProcessDroppedItem(GameObject gameObject)
        {
            Constructor constructor = gameObject.GetComponent<Constructor>();
            Validate.NotNull(constructor, "Gameobject did not have a corresponding constructor component");

            constructor.Deploy(true);
        }
    }
}
