using System.Collections;

namespace NitroxClient.MonoBehaviours.Gui.Modals;

public class InfoModal : Modal
{
    public InfoModal() : base(yesButtonText: "Ok", isAvoidable: false, transparency: 0.93f, height: 400f)
    { }

    public void Show(string actionText)
    {
        ModalText = actionText;
        Show();
    }

    public override void ClickYes()
    {
        Hide();
        OnDeselect();
    }

    public IEnumerator ShowAsync(string actionText)
    {
        ModalText = actionText;
        yield return ShowAsync();
    }
}
