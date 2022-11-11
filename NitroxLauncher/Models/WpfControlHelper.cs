using System;
using System.Windows.Controls;
using System.Windows.Media;

namespace NitroxLauncher.Models
{
    public static class WpfControlHelper
    {
        public static T FindDataContextInAncestors<T>(this Control control)
        {
            do
            {
                if (control.DataContext is T)
                {
                    return (T)control.DataContext;
                }
                control = VisualTreeHelper.GetParent(control) as Control;
            } while (control != null);

            return default;
        }
    }
}
