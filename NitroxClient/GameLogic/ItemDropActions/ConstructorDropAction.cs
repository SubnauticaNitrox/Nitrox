using System.Reflection;
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

            MethodInfo deploy = typeof(Constructor).GetMethod("Deploy", BindingFlags.NonPublic | BindingFlags.Instance);
            Validate.NotNull(deploy, "Could not find 'Deploy' method in class 'Constructor'");

            deploy.Invoke(constructor, new object[] { true });
        }
    }
}
