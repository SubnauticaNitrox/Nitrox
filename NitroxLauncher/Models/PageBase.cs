using CommunityToolkit.Mvvm.ComponentModel;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace NitroxLauncher.Models;

[ObservableObject]
public abstract partial class PageBase : Page
{
    /// <summary>
    /// Opens default browser with a specific link
    /// </summary>
    protected void OnRequestNavigate(object sender, RequestNavigateEventArgs e)
    {
        Process.Start(new ProcessStartInfo(e.Uri.AbsoluteUri));
        e.Handled = true;
    }
}
