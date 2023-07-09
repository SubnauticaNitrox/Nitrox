using Avalonia.Controls;
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

    private void ScrollViewer_OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
    {
        // Change scroll wheel input to move scroll viewer left and right
        ScrollViewer scrollViewer = (ScrollViewer)sender;
        if (e.Delta.Y < 0)
        {
            scrollViewer.LineRight();
        }
        else
        {
            scrollViewer.LineLeft();
        }
        e.Handled = true;
    }
}
