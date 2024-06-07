using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Utilities;
using static System.Math;

namespace Nitrox.Launcher.Models.Design;

/// <summary>
///     Panel that arranges stretchable child controls to fit max width, up to the limit of <see cref="MaxItemWidth" />.
///     Code inspired by Avalonia's WrapPanel
///     (https://github.com/AvaloniaUI/Avalonia/blob/master/src/Avalonia.Controls/WrapPanel.cs).
/// </summary>
/// <remarks>
///     Looks similar to YouTube's video layout.
/// </remarks>
public class FittingWrapPanel : Panel, INavigableContainer
{
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<WrapPanel, Orientation>(nameof(Orientation));

    public static readonly StyledProperty<double> MaxItemWidthProperty =
        AvaloniaProperty.Register<WrapPanel, double>(nameof(MaxItemWidth), double.NaN);

    /// <summary>
    ///     Gets or sets the orientation in which child controls will be laid out.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    public double MaxItemWidth
    {
        get => GetValue(MaxItemWidthProperty);
        set => SetValue(MaxItemWidthProperty, value);
    }

    static FittingWrapPanel()
    {
        AffectsMeasure<WrapPanel>(OrientationProperty, MaxItemWidthProperty);
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size constraint)
    {
        Orientation orientation = Orientation;
        Controls children = Children;
        UVSize curLineSize = new(orientation);
        UVSize panelSize = new(orientation);
        UVSize uvConstraint = new(orientation, constraint.Width, constraint.Height);

        int itemsPerRow = (int)(constraint.Width / MaxItemWidth);
        double adjustedWidth = constraint.Width / itemsPerRow;

        for (int i = 0, count = children.Count; i < count; i++)
        {
            Control child = children[i];
            child.Measure(new Size(adjustedWidth, constraint.Height));

            UVSize sz = new(orientation, adjustedWidth, child.DesiredSize.Height);

            if (MathUtilities.GreaterThan(curLineSize.U + sz.U, uvConstraint.U)) // Need to switch to another line
            {
                panelSize = panelSize with { U = Max(curLineSize.U, panelSize.U), V = panelSize.V + curLineSize.V };
                curLineSize = sz;

                if (MathUtilities.GreaterThan(sz.U, uvConstraint.U)) // The element is wider then the constraint - give it a separate line
                {
                    panelSize = panelSize with { U = Max(sz.U, panelSize.U), V = panelSize.V + sz.V };
                    curLineSize = new UVSize(orientation);
                }
            }
            else // Continue to accumulate a line
            {
                curLineSize = curLineSize with { U = curLineSize.U + sz.U, V = Max(sz.V, curLineSize.V) };
            }
        }

        panelSize = panelSize with { U = Max(curLineSize.U, panelSize.U), V = panelSize.V + curLineSize.V };

        return new Size(panelSize.Width, panelSize.Height);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        Orientation orientation = Orientation;
        Controls children = Children;
        int firstInLine = 0;
        double accumulatedV = 0;
        UVSize curLineSize = new(orientation);
        UVSize uvFinalSize = new(orientation, finalSize.Width, finalSize.Height);

        int itemsPerRow = (int)(finalSize.Width / MaxItemWidth);
        double adjustedWidth = finalSize.Width / itemsPerRow;

        for (int i = 0; i < children.Count; i++)
        {
            Control child = children[i];
            UVSize sz = new(orientation, adjustedWidth, child.DesiredSize.Height);

            if (MathUtilities.GreaterThan(curLineSize.U + sz.U, uvFinalSize.U)) // Need to switch to another line
            {
                ArrangeLine(accumulatedV, curLineSize.V, firstInLine, i, adjustedWidth);

                accumulatedV += curLineSize.V;
                curLineSize = sz;

                if (MathUtilities.GreaterThan(sz.U, uvFinalSize.U)) // The element is wider then the constraint - give it a separate line
                {
                    ArrangeLine(accumulatedV, sz.V, i, ++i, adjustedWidth);

                    accumulatedV += sz.V;
                    curLineSize = new UVSize(orientation);
                }
                firstInLine = i;
            }
            else // Continue to accumulate a line
            {
                curLineSize = curLineSize with { U = curLineSize.U + sz.U, V = Max(sz.V, curLineSize.V) };
            }
        }

        if (firstInLine < children.Count)
        {
            ArrangeLine(accumulatedV, curLineSize.V, firstInLine, children.Count, adjustedWidth);
        }

        return finalSize;
    }

    /// <summary>
    ///     Gets the next control in the specified direction.
    /// </summary>
    /// <param name="direction">The movement direction.</param>
    /// <param name="from">The control from which movement begins.</param>
    /// <param name="wrap">Whether to wrap around when the first or last item is reached.</param>
    /// <returns>The control.</returns>
    IInputElement INavigableContainer.GetControl(NavigationDirection direction, IInputElement from, bool wrap)
    {
        Orientation orientation = Orientation;
        Controls children = Children;
        bool horiz = orientation == Orientation.Horizontal;
        int index = from is not null ? Children.IndexOf((Control)from) : -1;

        switch (direction)
        {
            case NavigationDirection.First:
                index = 0;
                break;
            case NavigationDirection.Last:
                index = children.Count - 1;
                break;
            case NavigationDirection.Next:
                ++index;
                break;
            case NavigationDirection.Previous:
                --index;
                break;
            case NavigationDirection.Left:
                index = horiz ? index - 1 : -1;
                break;
            case NavigationDirection.Right:
                index = horiz ? index + 1 : -1;
                break;
            case NavigationDirection.Up:
                index = horiz ? -1 : index - 1;
                break;
            case NavigationDirection.Down:
                index = horiz ? -1 : index + 1;
                break;
        }

        if (index >= 0 && index < children.Count)
        {
            return children[index];
        }
        return null;
    }

    private void ArrangeLine(double v, double lineV, int start, int end, double itemU)
    {
        Orientation orientation = Orientation;
        Controls children = Children;
        double u = 0;
        bool isHorizontal = orientation == Orientation.Horizontal;

        for (int i = start; i < end; i++)
        {
            Control child = children[i];
            double layoutSlotU = itemU;
            child.Arrange(new Rect(
                              isHorizontal ? u : v,
                              isHorizontal ? v : u,
                              isHorizontal ? layoutSlotU : lineV,
                              isHorizontal ? lineV : layoutSlotU));
            u += layoutSlotU;
        }
    }

    private readonly struct UVSize
    {
        public double U { get; init; }
        public double V { get; init; }

        private readonly Orientation orientation;

        internal UVSize(Orientation orientation, double width, double height)
        {
            this.orientation = orientation;
            Width = width;
            Height = height;
        }

        internal UVSize(Orientation orientation)
        {
            this.orientation = orientation;
        }

        internal double Width
        {
            get => orientation == Orientation.Horizontal ? U : V;
            init
            {
                if (orientation == Orientation.Horizontal)
                {
                    U = value;
                }
                else
                {
                    V = value;
                }
            }
        }

        internal double Height
        {
            get => orientation == Orientation.Horizontal ? V : U;
            init
            {
                if (orientation == Orientation.Horizontal)
                {
                    V = value;
                }
                else
                {
                    U = value;
                }
            }
        }
    }
}
