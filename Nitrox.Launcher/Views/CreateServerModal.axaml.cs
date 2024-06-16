using Avalonia.Input;
using Nitrox.Launcher.Views.Abstract;

namespace Nitrox.Launcher.Views;

public partial class CreateServerModal : ModalBase
{
    public CreateServerModal()
    {
        InitializeComponent();
    }

    private void Window_OnPointerPressed(object sender, PointerPressedEventArgs e) => Focus(); // Allow for de-focusing textboxes when clicking outside of them.
}
