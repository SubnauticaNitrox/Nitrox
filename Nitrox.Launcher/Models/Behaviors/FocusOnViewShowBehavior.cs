using Avalonia.Controls;
using Avalonia.Xaml.Interactivity;
using CommunityToolkit.Mvvm.Messaging;

namespace Nitrox.Launcher.Models.Behaviors;

/// <summary>
///     Focuses the <see cref="Behavior.AssociatedObject" /> when its parent view is shown.
/// </summary>
public class FocusOnViewShowBehavior : Behavior<Control>
{
    protected override void OnAttached()
    {
        WeakReferenceMessenger.Default.Register<ViewShownMessage>(this, static (obj, _) => (obj as FocusOnViewShowBehavior)?.Focus());
        base.OnAttached();
    }

    protected override void OnDetaching()
    {
        WeakReferenceMessenger.Default.UnregisterAll(this);
        base.OnDetaching();
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
