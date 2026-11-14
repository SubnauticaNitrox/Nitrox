namespace NitroxClient.MonoBehaviours.Gui.Modals;

public enum PictureFrameBudgetChoice
{
    Continue,
    RaiseCapForSession,
    Disable
}

public class PictureFrameBudgetModal : Modal
{
    public PictureFrameBudgetChoice? Choice { get; private set; }

    public PictureFrameBudgetModal() : base(
        yesButtonText: Language.main.Get("Nitrox_PictureFrameBudgetContinue"), hideNoButton: false, noButtonText: Language.main.Get("Nitrox_PictureFrameBudgetDisable"),
        hasThirdButton: true, thirdButtonText: Language.main.Get("Nitrox_PictureFrameBudgetRaiseCap"),
        isAvoidable: false, transparency: 0.93f, height: 300f)
    { }

    public void Show(string actionText)
    {
        Choice = null;
        ModalText = actionText;
        Show();
    }

    public override void ClickYes()
    {
        Choice = PictureFrameBudgetChoice.Continue;
        Hide();
    }

    public override void ClickNo()
    {
        Choice = PictureFrameBudgetChoice.Disable;
        Hide();
    }

    public override void ClickThird()
    {
        Choice = PictureFrameBudgetChoice.RaiseCapForSession;
        Hide();
    }
}
