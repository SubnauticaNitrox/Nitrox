using System.Diagnostics;
using Avalonia.Input;
using Nitrox.Launcher.ViewModels;
using Nitrox.Launcher.Views.Abstract;

namespace Nitrox.Launcher.Views;

public partial class BlogView : RoutableViewBase<BlogViewModel>
{
    public BlogView()
    {
        InitializeComponent();
    }

    private void NitroxBlogTextBlock_OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        Process.Start(new ProcessStartInfo("https://nitroxblog.rux.gg/") { UseShellExecute = true, Verb = "open" })?.Dispose();
    }
}