namespace NitroxClient.MonoBehaviours.Gui.InGame
{
    /// <summary>
    ///     Extends the IngameMenu with a disconnect popup.
    /// </summary>
    public class LostConnectionModal : KickedModal
    {
        public override string SubWindowName => "LostConnectionModal";
        public override string ModalText => Language.main.Get("Nitrox_LostConnection");
    }
}
