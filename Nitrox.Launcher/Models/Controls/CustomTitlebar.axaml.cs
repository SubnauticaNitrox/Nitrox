using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using CommunityToolkit.Mvvm.Input;

namespace Nitrox.Launcher.Models.Controls;

public partial class CustomTitlebar : TemplatedControl
{
    public static readonly DirectProperty<CustomTitlebar, bool> ShowTitleProperty =
        AvaloniaProperty.RegisterDirect<CustomTitlebar, bool>(
            nameof(showTitle),
            o => o.showTitle,
            (o, v) => o.showTitle = v, true);

    public static readonly DirectProperty<CustomTitlebar, bool> CanMaximizeProperty =
        AvaloniaProperty.RegisterDirect<CustomTitlebar, bool>(
            nameof(CanMaximize),
            o => o.CanMaximize,
            (o, v) => o.CanMaximize = v, true);

    public static readonly DirectProperty<CustomTitlebar, bool> CanMinimizeProperty =
        AvaloniaProperty.RegisterDirect<CustomTitlebar, bool>(
            nameof(CanMinimize),
            o => o.CanMinimize,
            (o, v) => o.CanMinimize = v, true);

    private bool showTitle = true;
    private bool canMaximize = true;
    private bool canMinimize = true;

    public bool ShowTitle
    {
        get => showTitle;
        set => SetAndRaise(ShowTitleProperty, ref showTitle, value);
    }

    public bool CanMaximize
    {
        get => canMaximize;
        set => SetAndRaise(CanMaximizeProperty, ref canMaximize, value);
    }

    public bool CanMinimize
    {
        get => canMinimize;
        set => SetAndRaise(CanMinimizeProperty, ref canMinimize, value);
    }

    [RelayCommand]
    public void Minimize()
    {
        if (!CanMinimize)
        {
            return;
        }
        if (this.GetWindow() is not { } window)
        {
            return;
        }
        window.WindowState = WindowState.Minimized;
    }

    [RelayCommand]
    public void ToggleMaximize()
    {
        if (!CanMaximize)
        {
            return;
        }
        if (this.GetWindow() is not { } window)
        {
            return;
        }
        window.WindowState = window.WindowState == WindowState.Normal ? WindowState.Maximized : WindowState.Normal;
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        PointerPressed += OnPointerPressed;
        DoubleTapped += OnDoubleTapped;
        base.OnLoaded(e);
    }

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        PointerPressed -= OnPointerPressed;
        DoubleTapped -= OnDoubleTapped;
        base.OnUnloaded(e);
    }

    private void OnPointerPressed(object sender, PointerPressedEventArgs e)
    {
        if (e.Source is Visual element && element.GetWindow() is { } window)
        {
            window.BeginMoveDrag(e);
        }
    }

    private void OnDoubleTapped(object sender, TappedEventArgs e) => ToggleMaximize();

    [RelayCommand]
    private void Close()
    {
        if (this.GetWindow() is not { } window)
        {
            return;
        }
        window.CloseByUser();
    }
}
