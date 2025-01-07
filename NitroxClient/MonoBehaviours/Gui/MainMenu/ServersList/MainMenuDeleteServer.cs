namespace NitroxClient.MonoBehaviours.Gui.MainMenu.ServersList;

public class MainMenuDeleteServer : uGUI_NavigableControlGrid, uGUI_IButtonReceiver
{
    public MainMenuServerButton serverButton;

    private void Start() => interGridNavigation = new uGUI_InterGridNavigation();

    public bool OnButtonDown(GameInput.Button button)
    {
        if (button != GameInput.Button.UICancel)
        {
            return false;
        }

        OnBack();
        return true;
    }

    public void OnBack() => serverButton.CancelDelete();
}
