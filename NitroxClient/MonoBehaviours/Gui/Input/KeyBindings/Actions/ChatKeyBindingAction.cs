using System;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Gui.Input.KeyBindings.Actions
{
    public class ChatKeyBindingAction : KeyBindingAction
    {

        public override void Execute()
        {
            Console.WriteLine("Chat Key Press!!!");
        }
    }
}
