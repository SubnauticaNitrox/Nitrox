using System.Windows.Controls;
using System.Windows.Media;

namespace NitroxLauncher.Models;

public static class Extensions
{
    public static T FindDataContextInAncestors<T>(this Control control)
    {
        do
        {
            if (control.DataContext is T t)
            {
                return t;
            }

            control = VisualTreeHelper.GetParent(control) as Control;
        } while (control != null);

        return default;
    }
}
