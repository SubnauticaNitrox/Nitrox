using Avalonia.Controls;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views.Abstract;

namespace Nitrox.Launcher.Views;

internal partial class EmbeddedServerView : RoutableViewBase<EmbeddedServerViewModel>
{
    public EmbeddedServerView()
    {
        InitializeComponent();
    }

    private void ItemsControl_OnSizeChanged(object? sender, SizeChangedEventArgs e)
    {
        if (e.WidthChanged)
        {
            ItemsControl.MinWidth = ItemsControl.Bounds.Width;
        }
    }
}
