using System.Collections;

namespace NitroxClient.MonoBehaviours.Gui.Modals;

public class InfoModal : Modal
{
    public InfoModal() : base(yesButtonText: "Ok", isAvoidable: true, transparency: 0.93f)
    { }

    public void Show(string actionText)
    {
        ModalText = actionText;
        Show();
    }

    public IEnumerator ShowAsync(string actionText)
    {
        ModalText = actionText;
        yield return ShowAsync();
    }
}
