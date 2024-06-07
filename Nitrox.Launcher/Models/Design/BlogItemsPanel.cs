using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Layout;
using Avalonia.Utilities;
using static System.Math;

namespace Nitrox.Launcher.Models.Design;

/// <summary>
/// Custom panel that arranges stretchable child controls in a wrap panel. Code inspired by Avalonia's WrapPanel (https://github.com/AvaloniaUI/Avalonia/blob/master/src/Avalonia.Controls/WrapPanel.cs).
/// </summary>
public class BlogItemsPanel : Panel, INavigableContainer
{
    /// <summary>
    /// Defines the <see cref="Orientation"/> property.
    /// </summary>
    public static readonly StyledProperty<Orientation> OrientationProperty =
        AvaloniaProperty.Register<WrapPanel, Orientation>(nameof(Orientation), defaultValue: Orientation.Horizontal);

    /// <summary>
    /// Defines the <see cref="ItemWidth"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ItemWidthProperty =
        AvaloniaProperty.Register<WrapPanel, double>(nameof(ItemWidth), double.NaN);

    /// <summary>
    /// Defines the <see cref="ItemHeight"/> property.
    /// </summary>
    public static readonly StyledProperty<double> ItemHeightProperty =
        AvaloniaProperty.Register<WrapPanel, double>(nameof(ItemHeight), double.NaN);

    /// <summary>
    /// Initializes static members of the <see cref="WrapPanel"/> class.
    /// </summary>
    static BlogItemsPanel()
    {
        AffectsMeasure<WrapPanel>(OrientationProperty, ItemWidthProperty, ItemHeightProperty);
    }

    /// <summary>
    /// Gets or sets the orientation in which child controls will be laid out.
    /// </summary>
    public Orientation Orientation
    {
        get => GetValue(OrientationProperty);
        set => SetValue(OrientationProperty, value);
    }

    /// <summary>
    /// Gets or sets the width of all items in the WrapPanel.
    /// </summary>
    public double ItemWidth
    {
        get => GetValue(ItemWidthProperty);
        set => SetValue(ItemWidthProperty, value);
    }

    /// <summary>
    /// Gets or sets the height of all items in the WrapPanel.
    /// </summary>
    public double ItemHeight
    {
        get => GetValue(ItemHeightProperty);
        set => SetValue(ItemHeightProperty, value);
    }

    /// <summary>
    /// Gets the next control in the specified direction.
    /// </summary>
    /// <param name="direction">The movement direction.</param>
    /// <param name="from">The control from which movement begins.</param>
    /// <param name="wrap">Whether to wrap around when the first or last item is reached.</param>
    /// <returns>The control.</returns>
    IInputElement? INavigableContainer.GetControl(NavigationDirection direction, IInputElement? from, bool wrap)
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
        else
        {
            return null;
        }
    }

    /// <inheritdoc/>
    protected override Size MeasureOverride(Size constraint)
    {
        Orientation orientation = Orientation;
        Controls children = Children;
        UVSize curLineSize = new(orientation);
        UVSize panelSize = new(orientation);
        UVSize uvConstraint = new(orientation, constraint.Width, constraint.Height);

        int itemsPerRow = (int)(constraint.Width / ItemWidth);
        double adjustedWidth = constraint.Width / itemsPerRow;

        for (int i = 0, count = children.Count; i < count; i++)
        {
            Control child = children[i];
            child.Measure(new Size(adjustedWidth, constraint.Height));

            UVSize sz = new(orientation, adjustedWidth, child.DesiredSize.Height);

            if (MathUtilities.GreaterThan(curLineSize.U + sz.U, uvConstraint.U)) // Need to switch to another line
            {
                panelSize.U = Max(curLineSize.U, panelSize.U);
                panelSize.V += curLineSize.V;
                curLineSize = sz;

                if (MathUtilities.GreaterThan(sz.U, uvConstraint.U)) // The element is wider then the constraint - give it a separate line
                {
                    panelSize.U = Max(sz.U, panelSize.U);
                    panelSize.V += sz.V;
                    curLineSize = new UVSize(orientation);
                }
            }
            else // Continue to accumulate a line
            {
                curLineSize.U += sz.U;
                curLineSize.V = Max(sz.V, curLineSize.V);
            }
        }

        panelSize.U = Max(curLineSize.U, panelSize.U);
        panelSize.V += curLineSize.V;

        return new Size(panelSize.Width, panelSize.Height);
    }

    /// <inheritdoc/>
    protected override Size ArrangeOverride(Size finalSize)
    {
        Orientation orientation = Orientation;
        Controls children = Children;
        int firstInLine = 0;
        double accumulatedV = 0;
        UVSize curLineSize = new(orientation);
        UVSize uvFinalSize = new(orientation, finalSize.Width, finalSize.Height);

        int itemsPerRow = (int)(finalSize.Width / ItemWidth);
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
                curLineSize.U += sz.U;
                curLineSize.V = Max(sz.V, curLineSize.V);
            }
        }

        if (firstInLine < children.Count)
        {
            ArrangeLine(accumulatedV, curLineSize.V, firstInLine, children.Count, adjustedWidth);
        }

        return finalSize;
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

    private struct UVSize
    {
        internal UVSize(Orientation orientation, double width, double height)
        {
            U = V = 0d;
            _orientation = orientation;
            Width = width;
            Height = height;
        }

        internal UVSize(Orientation orientation)
        {
            U = V = 0d;
            _orientation = orientation;
        }

        internal double U;
        internal double V;
        private Orientation _orientation;

        internal double Width
        {
            get => _orientation == Orientation.Horizontal ? U : V;
            set { if (_orientation == Orientation.Horizontal) U = value; else V = value; }
        }
        internal double Height
        {
            get => _orientation == Orientation.Horizontal ? V : U;
            set { if (_orientation == Orientation.Horizontal) V = value; else U = value; }
        }
    }
}