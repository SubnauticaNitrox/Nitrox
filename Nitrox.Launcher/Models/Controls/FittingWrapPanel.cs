using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Utilities;
using static System.Math;

namespace Nitrox.Launcher.Models.Controls;

/// <summary>
///     Panel that arranges stretchable child controls to fit min width, up to the limit of <see cref="MinItemWidth" />.
///     Code inspired by Avalonia's WrapPanel
///     (https://github.com/AvaloniaUI/Avalonia/blob/master/src/Avalonia.Controls/WrapPanel.cs).
/// </summary>
/// <remarks>
///     Looks similar to YouTube video layout.
/// </remarks>
public class FittingWrapPanel : Panel, INavigableContainer
{
    public static readonly StyledProperty<double> MinItemWidthProperty =
        AvaloniaProperty.Register<WrapPanel, double>(nameof(MinItemWidth), 100);

    public double MinItemWidth
    {
        get => GetValue(MinItemWidthProperty);
        set => SetValue(MinItemWidthProperty, value);
    }

    static FittingWrapPanel()
    {
        AffectsMeasure<WrapPanel>(MinItemWidthProperty);
    }

    /// <inheritdoc />
    protected override Size MeasureOverride(Size constraint)
    {
        UVSize curLineSize = new();
        UVSize panelSize = new();
        UVSize uvConstraint = new(constraint.Width, constraint.Height);

        int itemsPerRow = (int)Min(constraint.Width / MinItemWidth, Max(Children.Count, 1));
        double adjustedWidth = constraint.Width / itemsPerRow;

        for (int i = 0, count = Children.Count; i < count; i++)
        {
            Control child = Children[i];
            child.Measure(new Size(adjustedWidth, constraint.Height));

            UVSize sz = new(adjustedWidth, child.DesiredSize.Height);

            if (MathUtilities.GreaterThan(curLineSize.Width + sz.Width, uvConstraint.Width)) // Need to switch to another line
            {
                panelSize = new UVSize { Width = Max(curLineSize.Width, panelSize.Width), Height = panelSize.Height + curLineSize.Height };
                curLineSize = sz;

                if (MathUtilities.GreaterThan(sz.Width, uvConstraint.Width)) // The element is wider then the constraint - give it a separate line
                {
                    panelSize = new UVSize { Width = Max(sz.Width, panelSize.Width), Height = panelSize.Height + sz.Height };
                    curLineSize = new UVSize();
                }
            }
            else // Continue to accumulate a line
            {
                curLineSize = new UVSize { Width = curLineSize.Width + sz.Width, Height = Max(sz.Height, curLineSize.Height) };
            }
        }

        panelSize = new UVSize { Width = Max(curLineSize.Width, panelSize.Width), Height = panelSize.Height + curLineSize.Height };

        return new Size(panelSize.Width, panelSize.Height);
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        int firstInLine = 0;
        double accumulatedV = 0;
        UVSize curLineSize = new();
        UVSize uvFinalSize = new(finalSize.Width, finalSize.Height);

        int itemsPerRow = (int)Min(finalSize.Width / MinItemWidth, Max(Children.Count, 1));
        double adjustedWidth = finalSize.Width / itemsPerRow;

        for (int i = 0; i < Children.Count; i++)
        {
            Control child = Children[i];
            UVSize sz = new(adjustedWidth, child.DesiredSize.Height);

            if (MathUtilities.GreaterThan(curLineSize.Width + sz.Width, uvFinalSize.Width)) // Need to switch to another line
            {
                ArrangeLine(accumulatedV, curLineSize.Height, firstInLine, i, adjustedWidth);

                accumulatedV += curLineSize.Height;
                curLineSize = sz;

                if (MathUtilities.GreaterThan(sz.Width, uvFinalSize.Width)) // The element is wider then the constraint - give it a separate line
                {
                    ArrangeLine(accumulatedV, sz.Height, i, ++i, adjustedWidth);

                    accumulatedV += sz.Height;
                    curLineSize = new UVSize();
                }
                firstInLine = i;
            }
            else // Continue to accumulate a line
            {
                curLineSize = new UVSize { Width = curLineSize.Width + sz.Width, Height = Max(sz.Height, curLineSize.Height) };
            }
        }

        if (firstInLine < Children.Count)
        {
            ArrangeLine(accumulatedV, curLineSize.Height, firstInLine, Children.Count, adjustedWidth);
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
        Avalonia.Controls.Controls children = Children;
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
                index -= 1;
                break;
            case NavigationDirection.Right:
                index += 1;
                break;
            case NavigationDirection.Up:
            case NavigationDirection.Down:
                index = -1;
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
        Avalonia.Controls.Controls children = Children;
        double u = 0;

        for (int i = start; i < end; i++)
        {
            Control child = children[i];
            child.Arrange(new Rect(u, v, itemU, lineV));
            u += itemU;
        }
    }

    private readonly struct UVSize
    {

        internal UVSize(double width, double height)
        {
            Width = width;
            Height = height;
        }

        public double Width { get; init; }

        internal double Height { get; init; }
    }
}
