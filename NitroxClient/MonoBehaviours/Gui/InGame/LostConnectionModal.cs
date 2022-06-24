namespace NitroxClient.MonoBehaviours.Gui.InGame;

/// <summary>
///     Extends the IngameMenu with a disconnect popup.
/// </summary>
public class LostConnectionModal : Modal
{
    public LostConnectionModal() : base(yesButtonText: "OK", modalText: Language.main.Get("Nitrox_LostConnection"), freezeGame: true, transparency: 1.0f)
    {
    }

    public override void ClickYes()
    {
        IngameMenu.main.QuitGame(false);
    }
}
