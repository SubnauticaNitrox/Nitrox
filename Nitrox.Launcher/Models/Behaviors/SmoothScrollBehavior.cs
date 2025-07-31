using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Avalonia;
using Avalonia.Animation.Easings;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Layout;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace Nitrox.Launcher.Models.Behaviors;

public class SmoothScrollBehavior : StyledElementBehavior<ScrollViewer>
{
    public enum ChangeSize
    {
        Line,
        Page
    }

    private const double ANIMATION_DURATION = 170; // animation duration in milliseconds

    /// <summary>
    ///     ScrollStepSize DirectProperty definition
    /// </summary>
    public static readonly DirectProperty<SmoothScrollBehavior, double> ScrollStepSizeProperty =
        AvaloniaProperty.RegisterDirect<SmoothScrollBehavior, double>($"{nameof(ScrollStepSize)}Property",
                                                                                      o => o.ScrollStepSize,
                                                                                      (o, v) => o.ScrollStepSize = v);

    public static readonly StyledProperty<ChangeSize> ScrollChangeSizeProperty =
        AvaloniaProperty.Register<SmoothScrollBehavior, ChangeSize>(nameof(ScrollChangeSize));

    private DateTime animationStartTime;

    private bool isAnimating;

    private ScrollContentPresenter? scp;

    private double scrollStepSize = 100;
    private double startOffset;
    private double targetOffset;

    public double ScrollStepSize
    {
        get => scrollStepSize;
        set => SetAndRaise(ScrollStepSizeProperty, ref scrollStepSize, value);
    }

    public ChangeSize ScrollChangeSize
    {
        get => GetValue(ScrollChangeSizeProperty);
        set => SetValue(ScrollChangeSizeProperty, value);
    }

    protected override void OnAttached()
    {
        base.OnAttached();
        AssociatedObject!.AddHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged, RoutingStrategies.Tunnel | RoutingStrategies.Bubble);
        AssociatedObject!.SetValue(ScrollChangeSizeProperty, ChangeSize.Line);

        AssociatedObject.Loaded += AssociatedObject_Loaded;
    }

    protected override void OnDetaching()
    {
        base.OnDetaching();
        AssociatedObject!.RemoveHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged);
    }

    private void AssociatedObject_Loaded(object? sender, RoutedEventArgs e)
    {
        if (AssociatedObject == null)
        {
            return;
        }

        scp = AssociatedObject?.Presenter as ScrollContentPresenter;

        AssociatedObject!.Loaded -= AssociatedObject_Loaded;
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (!IsEnabled)
        {
            e.Handled = !scp?.IsScrollChainingEnabled ?? false;
            return;
        }
        scp ??= AssociatedObject?.Presenter as ScrollContentPresenter;
        if (scp == null)
        {
            e.Handled = !scp?.IsScrollChainingEnabled ?? false;
            return;
        }

        object? src = e.Source;
        while (src != null && src != scp)
        {
            if (src is ScrollContentPresenter scp2)
            {
                if (scp2 == scp)
                {
                    break;
                }

                if ((e.Delta.Y > 0 && scp2.Offset.Y == 0) ||
                    (e.Delta.Y < 0 && Math.Abs(scp2.Offset.Y - (scp2!.Extent.Height - scp2!.Viewport.Height)) < 0.0002)) // scroll up or down & it's max
                {
                    src = scp2.GetVisualParent(); // take next parent
                }
                else
                {
                    return;
                }
            }
            else if (src is Visual visual)
            {
                src = visual.GetVisualParent();
            }
        }

        if (!ReferenceEquals(src, scp))
        {
            e.Handled = !(src as ScrollContentPresenter)?.IsScrollChainingEnabled ?? false;
            return;
        }

        Vector delta = e.Delta;
        double x = scp!.Offset.X;
        double y = scp!.Offset.Y;
        double maxOffsetY = scp!.Extent.Height - scp!.Viewport.Height;

        ILogicalScrollable? scrollable = scp?.Child as ILogicalScrollable;
        bool isLogical = scrollable?.IsLogicalScrollEnabled == true;
        if (scp!.Extent.Height > scp!.Viewport.Height)
        {
            double height = isLogical ? scrollable!.ScrollSize.Height : ScrollStepSize;
            y += -delta.Y * height;
            y = double.Max(y, 0);
            y = double.Min(y, maxOffsetY);
        }

        Vector newOffset = SnapOffset(new Vector(x, y), delta, true);
        double step = double.Abs(newOffset.Y - scp!.Offset.Y);

        if (delta.Y > 0) // Scroll up
        {
            if (ScrollChangeSize == ChangeSize.Line)
            {
                AnimateScroll(-step);
            }
            else
            {
                AnimateScroll(-AssociatedObject!.Bounds.Height);
            }
        }
        else // Scroll down
        {
            if (ScrollChangeSize == ChangeSize.Line)
            {
                AnimateScroll(step);
            }
            else
            {
                AnimateScroll(AssociatedObject!.Bounds.Height);
            }
        }

        bool offsetChanged = newOffset != scp.Offset;
        e.Handled = !scp.IsScrollChainingEnabled || offsetChanged;
    }

    private void AnimateScroll(double delta)
    {
        DateTime currentTime = DateTime.Now;

        if (isAnimating)
        {
            // Calculate elapsed time and progress of the current animation
            double elapsedTime = (currentTime - animationStartTime).TotalMilliseconds;
            double progress = double.Min(elapsedTime / ANIMATION_DURATION, 1.0);

            // Use easing for the current progress
            SineEaseOut? easing = new();
            double easedProgress = easing.Ease(progress);

            // Update the current offset considering easing
            startOffset += easedProgress * (targetOffset - startOffset);

            // Update _targetOffset with new delta
            targetOffset += delta;
            targetOffset = double.Clamp(targetOffset, 0, double.Max(0, AssociatedObject!.Extent.Height));

            animationStartTime = currentTime;
        }
        else
        {
            isAnimating = true;
            startOffset = AssociatedObject!.Offset.Y;
            targetOffset = startOffset + delta;

            targetOffset = double.Clamp(targetOffset, 0, double.Max(0, AssociatedObject!.Extent.Height));

            animationStartTime = currentTime;
            _ = Animate();
        }
    }

    private async Task Animate()
    {
        while (isAnimating)
        {
            double elapsedTime = (DateTime.Now - animationStartTime).TotalMilliseconds;

            if (elapsedTime >= ANIMATION_DURATION)
            {
                // End the animation
                AssociatedObject!.Offset = new Vector(AssociatedObject!.Offset.X, targetOffset);
                isAnimating = false;
                break;
            }

            // Animation progress from 0 to 1
            double progress = elapsedTime / ANIMATION_DURATION;
            SineEaseOut? easing = new();
            double easedProgress = easing.Ease(progress);

            // Calculate new offset
            double currentOffset = startOffset + easedProgress * (targetOffset - startOffset);

            // Apply offset
            AssociatedObject!.Offset = new Vector(AssociatedObject!.Offset.X, currentOffset);

            await Task.Delay(10); // Update every 10ms
        }
    }

    private static (double previous, double next) FindNearestSnapPoint(List<double> snapPoints, double value)
    {
        int point = snapPoints.BinarySearch(value, Comparer<double>.Default);

        double previousSnapPoint, nextSnapPoint;

        if (point < 0)
        {
            point = ~point;

            previousSnapPoint = snapPoints[int.Max(0, point - 1)];
            nextSnapPoint = point >= snapPoints.Count ? snapPoints.Last() : snapPoints[int.Max(0, point)];
        }
        else
        {
            previousSnapPoint = nextSnapPoint = snapPoints[int.Max(0, point)];
        }

        return (previousSnapPoint, nextSnapPoint);
    }

    private IScrollSnapPointsInfo? GetScrollSnapPointsInfo(object? content)
    {
        object? scrollable = content;

        if (scp!.Content is ItemsControl itemsControl)
        {
            scrollable = itemsControl.Presenter?.Panel;
        }

        if (scp!.Content is ItemsPresenter itemsPresenter)
        {
            scrollable = itemsPresenter.Panel;
        }

        IScrollSnapPointsInfo? snapPointsInfo = scrollable as IScrollSnapPointsInfo;

        return snapPointsInfo;
    }

    private Vector SnapOffset(Vector offset, Vector direction = default, bool snapToNext = false)
    {
        IScrollSnapPointsInfo? scrollable = GetScrollSnapPointsInfo(scp!.Content);

        if (scrollable is null || scp!.VerticalSnapPointsType == SnapPointsType.None)
        {
            return offset;
        }

        Vector diff = GetAlignmentDiff();

        bool areVerticalSnapPointsRegular = false;
        List<double> verticalSnapPoints = [];
        double verticalSnapPoint = 0;
        double verticalSnapPointOffset = 0;

        if (scrollable is { } scrollSnapPointsInfo)
        {
            areVerticalSnapPointsRegular = scrollSnapPointsInfo.AreVerticalSnapPointsRegular;

            if (!areVerticalSnapPointsRegular)
            {
                verticalSnapPoints = [..scrollSnapPointsInfo.GetIrregularSnapPoints(Orientation.Vertical, scp!.VerticalSnapPointsAlignment)];
            }
            else
            {
                verticalSnapPoints = new List<double>();
                verticalSnapPoint = scrollSnapPointsInfo.GetRegularSnapPoints(Orientation.Vertical, scp!.VerticalSnapPointsAlignment, out verticalSnapPointOffset);
            }
        }

        if (scp!.VerticalSnapPointsType != SnapPointsType.None && (areVerticalSnapPointsRegular || verticalSnapPoints.Count > 0) && (!snapToNext || (snapToNext && direction.Y != 0)))
        {
            Vector estimatedOffset = new(offset.X, offset.Y + diff.Y);
            double previousSnapPoint = 0, nextSnapPoint = 0, midPoint = 0;

            if (areVerticalSnapPointsRegular)
            {
                previousSnapPoint = (int)(estimatedOffset.Y / verticalSnapPoint) * verticalSnapPoint + verticalSnapPointOffset;
                nextSnapPoint = previousSnapPoint + verticalSnapPoint;
                midPoint = (previousSnapPoint + nextSnapPoint) / 2;
            }
            else if (verticalSnapPoints.Count > 0)
            {
                (previousSnapPoint, nextSnapPoint) = FindNearestSnapPoint(verticalSnapPoints, estimatedOffset.Y);
                midPoint = (previousSnapPoint + nextSnapPoint) / 2;
            }

            double nearestSnapPoint = snapToNext
                ? direction.Y > 0 ? previousSnapPoint : nextSnapPoint
                : estimatedOffset.Y < midPoint
                    ? previousSnapPoint
                    : nextSnapPoint;

            offset = new Vector(offset.X, nearestSnapPoint - diff.Y);
        }

        Vector GetAlignmentDiff()
        {
            Vector vector = default;

            switch (scp!.VerticalSnapPointsAlignment)
            {
                case SnapPointsAlignment.Center:
                    vector += new Vector(0, scp!.Viewport.Height / 2);
                    break;
                case SnapPointsAlignment.Far:
                    vector += new Vector(0, scp!.Viewport.Height);
                    break;
            }

            switch (scp!.HorizontalSnapPointsAlignment)
            {
                case SnapPointsAlignment.Center:
                    vector += new Vector(scp!.Viewport.Width / 2, 0);
                    break;
                case SnapPointsAlignment.Far:
                    vector += new Vector(scp!.Viewport.Width, 0);
                    break;
            }

            return vector;
        }

        return offset;
    }
}
