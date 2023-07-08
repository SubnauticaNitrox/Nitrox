using Avalonia.Input;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views.Abstract;

namespace Nitrox.Launcher.Views;

public partial class ManageServerView : RoutableViewBase<ManageServerViewModel>
{
    public ManageServerView()
    {
        InitializeComponent();
    }
    
    private void InputElement_OnKeyDown(object sender, KeyEventArgs e)
    {
        e.Handled = e.Key is < Key.D0 or > Key.D9;
    }
}
