namespace NitroxClient.MonoBehaviours.Gui.InGame;

public class KickedModal : Modal
{
    public override string SubWindowName => "KickedModal";
    public override string YesButtonText => "OK";
    public override string NoButtonText => "NO";
    public override bool HideNoButton => true;
    public override bool IsAvoidable => false;
    public override string ModalText { get; set; }
    // When disconnected from the server, we don't want to keep playing
    public override bool FreezeGame => true;

    public void Show(string reason)
    {
        ModalText = reason;
        Show();
    }

    public override void ClickYes()
    {
        IngameMenu.main.QuitGame(false);
    }

    public override void ClickNo() { }
}
