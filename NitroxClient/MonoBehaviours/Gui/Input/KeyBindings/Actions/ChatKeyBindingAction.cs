using NitroxClient.GameLogic.ChatUI;
using NitroxModel.Core;

namespace NitroxClient.MonoBehaviours.Gui.Input.KeyBindings.Actions
{
    public class ChatKeyBindingAction : KeyBindingAction
    {
        public override void Execute()
        {
            PlayerChat chatManager = NitroxServiceLocator.LocateService<PlayerChat>();
            chatManager.ShowChat();
        }
    }
}
