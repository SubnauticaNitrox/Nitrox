using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.Reactive;

namespace Nitrox.Launcher.Models.Design;

/// <summary>
///     Container class for our attached properties.
/// </summary>
public class NitroxAttached : AvaloniaObject
{
    public static readonly AttachedProperty<bool> SelectedProperty = AvaloniaProperty.RegisterAttached<NitroxAttached, Interactive, bool>("Selected");
    public static readonly AttachedProperty<bool> AutoScrollToHomeProperty = AvaloniaProperty.RegisterAttached<NitroxAttached, ScrollViewer, bool>("AutoScrollToHome");
    public static readonly AttachedProperty<Orientation> PrimaryScrollWheelDirectionProperty = AvaloniaProperty.RegisterAttached<NitroxAttached, ScrollViewer, Orientation>("PrimaryScrollWheelDirection", Orientation.Vertical);
    public static readonly AttachedProperty<bool> IsNumericInputProperty = AvaloniaProperty.RegisterAttached<NitroxAttached, InputElement, bool>("IsNumericInput");
    public static readonly AttachedProperty<bool> HasUserInteractedProperty = AvaloniaProperty.RegisterAttached<NitroxAttached, InputElement, bool>("HasUserInteracted");
    public static readonly AttachedProperty<bool> UseCustomTitleBarProperty = AvaloniaProperty.RegisterAttached<NitroxAttached, Window, bool>("UseCustomTitleBar", true);
    internal static readonly AsyncCommandButtonTagger AsyncCommandButtonTagger;

    static NitroxAttached()
    {
        InputElement.LostFocusEvent.Raised.Subscribe(new AnonymousObserver<(object, RoutedEventArgs)>(HasUserInteractedOnNext));
        InputElement.TextInputEvent.Raised.Subscribe(new AnonymousObserver<(object, RoutedEventArgs)>(HasUserInteractedOnNext));
        AsyncCommandButtonTagger = new AsyncCommandButtonTagger("busy");

        void HasUserInteractedOnNext((object Sender, RoutedEventArgs EventArgs) args)
        {
            if (args.Sender is InputElement element)
            {
                SetHasUserInteracted(element, true);
            }
        }
    }

    public static bool GetSelected(AvaloniaObject element) => element.GetValue(SelectedProperty);

    public static void SetSelected(AvaloniaObject obj, bool value) => obj.SetValue(SelectedProperty, value);

    public static void SetAutoScrollToHome(AvaloniaObject obj, bool value)
    {
        static void VisualAttached(object sender, VisualTreeAttachmentEventArgs e) => (sender as ScrollViewer)?.ScrollToHome();

        obj.SetValue(AutoScrollToHomeProperty, value);
        if (obj is not Visual visual)
        {
            return;
        }

        if (value)
        {
            visual.AttachedToVisualTree += VisualAttached;
        }
        else
        {
            visual.AttachedToVisualTree -= VisualAttached;
        }
    }

    public static bool GetAutoScrollToHome(AvaloniaObject element) => element.GetValue(AutoScrollToHomeProperty);

    public static Orientation GetPrimaryScrollWheelDirection(AvaloniaObject obj) => obj.GetValue(PrimaryScrollWheelDirectionProperty);

    /// <summary>
    ///     Changes scroll wheel input to move scroll viewer left and right if set to <see cref="Orientation.Horizontal"/>.
    /// </summary>
    public static void SetPrimaryScrollWheelDirection(AvaloniaObject obj, Orientation orientation)
    {
        static void RotatedOrientationWheelHandler(object sender, PointerWheelEventArgs e)
        {
            ScrollViewer scrollViewer = sender as ScrollViewer;
            if (scrollViewer == null)
            {
                return;
            }
            if (GetPrimaryScrollWheelDirection(scrollViewer) == Orientation.Vertical)
            {
                return;
            }

            if (e.Delta.Y < 0)
            {
                for (int i = 0; i <= -e.Delta.Y; i++)
                {
                    scrollViewer.LineRight();
                }
            }
            else
            {
                for (int i = 0; i <= e.Delta.Y; i++)
                {
                    scrollViewer.LineLeft();
                }
            }
            e.Handled = true;
        }

        obj.SetValue(PrimaryScrollWheelDirectionProperty, orientation);
        if (obj is not ScrollViewer scrollViewer)
        {
            return;
        }

        switch (orientation)
        {
            case Orientation.Horizontal:
                scrollViewer.PointerWheelChanged += RotatedOrientationWheelHandler;
                break;
            case Orientation.Vertical:
                scrollViewer.PointerWheelChanged -= RotatedOrientationWheelHandler;
                break;
        }
    }

    public static void SetIsNumericInput(AvaloniaObject obj, bool value)
    {
        static void OnKeyDown(object sender, KeyEventArgs e)
        {
            switch (e.Key)
            {
                case Key.Up:
                case Key.Down:
                    if (sender is not TextBox textBox)
                    {
                        throw new NotSupportedException($"{sender.GetType()} is not supported by property {nameof(IsNumericInputProperty)}");
                    }

                    string previousText = textBox.Text;
                    if (int.TryParse(textBox.Text, out int val))
                    {
                        val += e.Key == Key.Up ? 1 : -1;
                    }
                    textBox.Text = Math.Clamp(val, 0, int.MaxValue).ToString();
                    if (textBox.Text.Length > textBox.MaxLength)
                    {
                        textBox.Text = previousText;
                    }
                    break;
            }
        }

        if (obj is not InputElement inputElement)
        {
            return;
        }

        if (value)
        {
            inputElement.KeyDown += OnKeyDown;
        }
        else
        {
            inputElement.KeyDown -= OnKeyDown;
        }
    }

    public static bool GetHasUserInteracted(InputElement input) => input.GetValue(HasUserInteractedProperty);
    
    public static void SetHasUserInteracted(InputElement input, bool value) => input.SetValue(HasUserInteractedProperty, value);
    
    public static bool GetUseCustomTitleBar(Window window) => window.GetValue(UseCustomTitleBarProperty);
    
    public static void SetUseCustomTitleBar(Window window, bool value) => window.SetValue(UseCustomTitleBarProperty, value);
}
