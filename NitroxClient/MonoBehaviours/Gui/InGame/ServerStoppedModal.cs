namespace NitroxClient.MonoBehaviours.Gui.InGame;

public class ServerStoppedModal : KickedModal
{
    public override string SubWindowName => "ServerStoppedModal";
    public override string ModalText => Language.main.Get("Nitrox_ServerStopped");
}
