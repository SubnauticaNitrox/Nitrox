using NitroxClient.MonoBehaviours.Discord;
using NitroxClient.MonoBehaviours.Gui.MainMenu;

namespace NitroxClient.MonoBehaviours.Gui.Input.KeyBindings.Actions;

public class DiscordFocusBindingAction : KeyBindingAction
{
    public override void Execute()
    {
        DiscordJoinRequestGui.Select();
    }
}
