using NitroxClient.GameLogic.ChatUI;

namespace NitroxClient.MonoBehaviours.Gui.Input.KeyBindings.Actions
{
    public class ChatKeyBindingAction : KeyBindingAction
    {
        PlayerChatManager chatManager = new PlayerChatManager();

        public override void Execute()
        {
            chatManager.ShowChat();
        }
    }
}
