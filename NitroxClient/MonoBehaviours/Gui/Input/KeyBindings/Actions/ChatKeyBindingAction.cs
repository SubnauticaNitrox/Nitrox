using NitroxClient.GameLogic.ChatUI;

namespace NitroxClient.MonoBehaviours.Gui.Input.KeyBindings.Actions
{
    public class ChatKeyBindingAction : KeyBindingAction
    {
        public override void Execute()
        {
            PlayerChatManager.Main.SelectChat();
        }
    }
}
