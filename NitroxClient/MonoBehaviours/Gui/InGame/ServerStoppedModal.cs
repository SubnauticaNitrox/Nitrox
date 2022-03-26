﻿namespace NitroxClient.MonoBehaviours.Gui.InGame;

public class ServerStoppedModal : Modal
{
    public ServerStoppedModal() : base(yesButtonText: "OK", modalText: Language.main.Get("Nitrox_ServerStopped"), freezeGame: true)
    {
    }

    public override void ClickYes()
    {
        IngameMenu.main.QuitGame(false);
    }
}
