namespace NitroxClient.MonoBehaviours.Gui.InGame;

public class KickedModal : Modal
{
    // When disconnected from the server, we don't want to keep playing
    public KickedModal() : base(yesButtonText: "OK", freezeGame: true, transparency: 1.0f)
    {
    }

    public void Show(string reason)
    {
        ModalText = reason;
        Show();
    }

    public override void ClickYes()
    {
        IngameMenu.main.QuitGame(false);
    }
}
