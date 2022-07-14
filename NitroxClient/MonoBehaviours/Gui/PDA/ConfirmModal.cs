using System;

namespace NitroxClient.MonoBehaviours.Gui.InGame;

public class ConfirmModal : Modal
{
    private Action yesCallback;

    public ConfirmModal() : base(yesButtonText: "Confirm", hideNoButton: false, noButtonText: "Cancel", isAvoidable: true, transparency: 0.93f)
    { }

    public void Show(string actionText, Action yesCallback)
    {
        ModalText = actionText;
        this.yesCallback = yesCallback;
        Show();
    }

    public override void ClickYes()
    {
        yesCallback?.Invoke();
        Hide();
        OnDeselect();
    }

    public override void ClickNo()
    {
        Hide();
        OnDeselect();
    }

    public override void OnDeselect()
    {
        yesCallback = null;
    }
}
