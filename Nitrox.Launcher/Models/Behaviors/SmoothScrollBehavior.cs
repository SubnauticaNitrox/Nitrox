using System;
using System.Threading;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;

namespace Nitrox.Launcher.Models.Behaviors;

public abstract class SmoothScrollBehavior
{
    private static CancellationTokenSource animationTokenSource;

    public static readonly AttachedProperty<bool> SmoothScrollProperty =
        AvaloniaProperty.RegisterAttached<SmoothScrollBehavior, ScrollViewer, bool>("SmoothScroll");

    /// <summary>
    ///     Gets or sets the target offset which was last used as smooth scrolling target.
    /// </summary>
    /// <remarks>
    ///     lastOffsetProperty is needed here since the ScrollViewer.Offset property is already set to the target offset when
    ///     the PointerWheelChanged event is raised
    /// </remarks>
    private static readonly AttachedProperty<Vector> lastOffsetProperty =
        AvaloniaProperty.RegisterAttached<SmoothScrollBehavior, ScrollViewer, Vector>("LastOffset", new Vector(0, 0));

    private static readonly Easing smoothScrollEasing = new ExponentialEaseOut();

    static SmoothScrollBehavior()
    {
        SmoothScrollProperty.Changed.Subscribe(OnEnableSmoothScrollingChanged);
    }

    public static bool GetSmoothScroll(ScrollViewer element) => element.GetValue(SmoothScrollProperty);

    public static void SetSmoothScroll(ScrollViewer element, bool value) => element.SetValue(SmoothScrollProperty, value);

    private static Vector GetLastOffset(ScrollViewer element) => element.GetValue(lastOffsetProperty);

    private static void SetLastOffset(ScrollViewer element, Vector value) => element.SetValue(lastOffsetProperty, value);

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

    private static void OnPointerWheelChanged(object sender, PointerWheelEventArgs e)
    {
        if (sender is not ScrollViewer scrollViewer)
        {
            return;
        }

        // Cancel ongoing animations
        if (animationTokenSource is { IsCancellationRequested: false })
        {
            animationTokenSource.Cancel();
            animationTokenSource.Dispose();
        }

        // Get new offset (already set on each ScrollViewer as attached property)
        Vector lastOffset = GetLastOffset(scrollViewer);
        Vector newOffset = scrollViewer.Offset;
        if (lastOffset != newOffset)
        {
            animationTokenSource = new CancellationTokenSource();
            SetLastOffset(scrollViewer, newOffset);
            AnimateScrollToTargetAsync(scrollViewer, lastOffset, newOffset, animationTokenSource.Token).ContinueWithHandleError();
        }
    }

    private static async Task AnimateScrollToTargetAsync(ScrollViewer scrollViewer, Vector previousOffset, Vector targetOffset, CancellationToken cancellationToken = default)
    {
        try
        {
            Animation animation = new()
            {
                Duration = TimeSpan.FromMilliseconds(250),
                Easing = smoothScrollEasing,
                Children =
                {
                    new KeyFrame
                    {
                        Cue = new Cue(0),
                        Setters = { new Setter(ScrollViewer.OffsetProperty, previousOffset) }
                    },
                    new KeyFrame
                    {
                        Cue = new Cue(1),
                        Setters = { new Setter(ScrollViewer.OffsetProperty, targetOffset) }
                    }
                }
            };
            await animation.RunAsync(scrollViewer, cancellationToken);
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
    }
}
