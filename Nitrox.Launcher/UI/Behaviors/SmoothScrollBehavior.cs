using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using System;
using System.Threading;

namespace Nitrox.Launcher.UI.Behaviors;

public abstract class SmoothScrollBehavior
{
    private static CancellationTokenSource animationTokenSource;
    
    public static readonly AttachedProperty<bool> EnableSmoothScrollingProperty =
        AvaloniaProperty.RegisterAttached<SmoothScrollBehavior, ScrollViewer, bool>("EnableSmoothScrolling");
    
    public static bool GetEnableSmoothScrolling(ScrollViewer element) => element.GetValue(EnableSmoothScrollingProperty);

    public static void SetEnableSmoothScrolling(ScrollViewer element, bool value) => element.SetValue(EnableSmoothScrollingProperty, value);

    // lastOffsetProperty is needed here since the ScrollViewer.Offset property is already set to the target offset when the PointerWheelChanged event is raised
    private static readonly AttachedProperty<Vector> lastOffsetProperty =
        AvaloniaProperty.RegisterAttached<SmoothScrollBehavior, ScrollViewer, Vector>("LastOffset", new Vector(0, 0));

    private static Vector GetLastOffset(ScrollViewer element) => element.GetValue(lastOffsetProperty);

    private static void SetLastOffset(ScrollViewer element, Vector value) => element.SetValue(lastOffsetProperty, value);
    
    static SmoothScrollBehavior()
    {
        EnableSmoothScrollingProperty.Changed.Subscribe(OnEnableSmoothScrollingChanged);
    }
        
    private static void OnEnableSmoothScrollingChanged(AvaloniaPropertyChangedEventArgs<bool> args)
    {
        if (args.Sender is not ScrollViewer scrollViewer)
        {
            return;
        }
        
        if (args.NewValue.GetValueOrDefault())
        {
            scrollViewer.AddHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged, handledEventsToo: true);
        }
        else
        {
            scrollViewer.RemoveHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged);
        }
    }
    
    private static void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer)
        {
            return;
        }

        // Cancel ongoing animations
        animationTokenSource?.Cancel();
        animationTokenSource?.Dispose();

        // Get new offset (already set within scrollviewer)
        Vector newOffset = scrollViewer.Offset;

        AnimateScroll(scrollViewer, newOffset);
        SetLastOffset(scrollViewer, newOffset);
    }

    private static async void AnimateScroll(ScrollViewer scrollViewer, Vector targetOffset)
    {
        animationTokenSource = new CancellationTokenSource();
        CancellationToken token = animationTokenSource.Token;

        Animation animation = new()
        {
            Duration = TimeSpan.FromMilliseconds(250),
            Easing = new ExponentialEaseOut(),
        };

        animation.Children.Add(new KeyFrame
        {
            Cue = new Cue(0),
            Setters =
            {
                new Setter(ScrollViewer.OffsetProperty, GetLastOffset(scrollViewer))
            }
        });

        animation.Children.Add(new KeyFrame
        {
            Cue = new Cue(1),
            Setters =
            {
                new Setter(ScrollViewer.OffsetProperty, targetOffset)
            }
        });

        try
        {
            if (GetLastOffset(scrollViewer) != targetOffset)
            {
                await animation.RunAsync(scrollViewer, token);
            }
        }
        catch (OperationCanceledException) { }
    }
}
