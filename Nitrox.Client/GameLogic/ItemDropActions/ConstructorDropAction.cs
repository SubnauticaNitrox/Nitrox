using System.Reflection;
using Nitrox.Model.Helper;
using UnityEngine;

namespace Nitrox.Client.GameLogic.ItemDropActions
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
