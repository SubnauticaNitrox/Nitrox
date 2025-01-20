using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using CommunityToolkit.Mvvm.Messaging;

namespace Nitrox.Launcher.Models.Behaviors;

/// <summary>
///     Focuses the <see cref="Behavior.AssociatedObject" /> when its parent view is shown.
/// </summary>
public class SetFocusedOnViewShowBehavior : Behavior<Control>
{
    public SetFocusedOnViewShowBehavior()
    {
        WeakReferenceMessenger.Default.Register<ViewShownMessage>(this, (_, _) => Focus());
    }

    protected override void OnAttachedToVisualTree() => Focus();

    private void Focus()
    {
        if (AssociatedObject == null)
        {
            return;
        }
        if (!AssociatedObject.IsEffectivelyVisible)
        {
            return;
        }

        AssociatedObject.Focus();
        if (AssociatedObject is TextBox textBox)
        {
            textBox.SelectAll();
        }
    }
}
