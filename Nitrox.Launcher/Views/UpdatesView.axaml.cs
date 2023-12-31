using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views.Abstract;

namespace Nitrox.Launcher.Views;

public partial class UpdatesView : RoutableViewBase<UpdatesViewModel>
{
    public UpdatesView()
    {
        InitializeComponent();
    }
}