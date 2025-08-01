using System;
using System.IO;
using Avalonia;
using Avalonia.Automation;
using Avalonia.Automation.Peers;
using Avalonia.Controls;
using Avalonia.Controls.Automation.Peers;
using Avalonia.LogicalTree;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Metadata;
using Avalonia.Platform;
using Avalonia.Reactive;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

namespace Nitrox.Launcher.Models.Controls;

/// <summary>
///     Displays a <see cref="Bitmap" /> as a recolored image given a <see cref="Color" />.
/// </summary>
public sealed class RecolorImage : Control
{
    /// <summary>
    ///     Defines the <see cref="Source" /> property.
    /// </summary>
    public static readonly StyledProperty<Bitmap> SourceProperty =
        AvaloniaProperty.Register<RecolorImage, Bitmap>(nameof(Source));

    public static readonly StyledProperty<Stretch> StretchProperty =
        Image.StretchProperty.AddOwner<RecolorImage>();

    public static readonly StyledProperty<StretchDirection> StretchDirectionProperty =
        Image.StretchDirectionProperty.AddOwner<RecolorImage>();

    public static readonly StyledProperty<Color> ColorProperty =
        AvaloniaProperty.Register<RecolorImage, Color>(nameof(Color), Colors.White);

    private readonly MemoryStream stream = new();
    private Rect dstRect;
    private Rect srcRect;
    private readonly IDisposable boundsSubscription;
    private readonly IDisposable sourceSubscription;

    public Color Color
    {
        get => GetValue(ColorProperty);
        set => SetValue(ColorProperty, value);
    }

    /// <summary>
    ///     Gets or sets the image that will be displayed.
    /// </summary>
    [Content]
    public Bitmap? Source
    {
        get => GetValue(SourceProperty);
        set => SetValue(SourceProperty, value);
    }

    /// <summary>
    ///     Gets or sets a value controlling how the image will be stretched.
    /// </summary>
    public Stretch Stretch
    {
        get => GetValue(StretchProperty);
        set => SetValue(StretchProperty, value);
    }

    /// <summary>
    ///     Gets or sets a value controlling in what direction the image will be stretched.
    /// </summary>
    public StretchDirection StretchDirection
    {
        get => GetValue(StretchDirectionProperty);
        set => SetValue(StretchDirectionProperty, value);
    }

    /// <inheritdoc />
    protected override bool BypassFlowDirectionPolicies => true;

    static RecolorImage()
    {
        AffectsRender<RecolorImage>(SourceProperty, StretchProperty, StretchDirectionProperty, ColorProperty);
        AffectsMeasure<RecolorImage>(SourceProperty, StretchProperty, StretchDirectionProperty);
        AutomationProperties.ControlTypeOverrideProperty.OverrideDefaultValue<RecolorImage>(AutomationControlType.Image);
    }

    public RecolorImage()
    {
        boundsSubscription = BoundsProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<Rect>>(BoundsChanged));
        sourceSubscription = SourceProperty.Changed.Subscribe(new AnonymousObserver<AvaloniaPropertyChangedEventArgs<Bitmap>>(SourceChanged));
    }

    protected override void OnDetachedFromLogicalTree(LogicalTreeAttachmentEventArgs e)
    {
        boundsSubscription.Dispose();
        sourceSubscription.Dispose();
        base.OnDetachedFromLogicalTree(e);
    }

    /// <summary>
    ///     Renders the control.
    /// </summary>
    /// <param name="context">The drawing context.</param>
    public sealed override void Render(DrawingContext context) => context.Custom(new RecolorImageRender(stream, dstRect, srcRect, Color));

    /// <summary>
    ///     Measures the control.
    /// </summary>
    /// <param name="availableSize">The available size.</param>
    /// <returns>The desired size of the control.</returns>
    protected override Size MeasureOverride(Size availableSize)
    {
        IImage source = Source;
        Size result = new();

        if (source != null)
        {
            result = Stretch.CalculateSize(availableSize, source.Size, StretchDirection);
        }

        return result;
    }

    /// <inheritdoc />
    protected override Size ArrangeOverride(Size finalSize)
    {
        IImage source = Source;

        if (source != null)
        {
            Size sourceSize = source.Size;
            Size result = Stretch.CalculateSize(finalSize, sourceSize);
            return result;
        }
        return new Size();
    }

    protected override AutomationPeer OnCreateAutomationPeer() => new ImageAutomationPeer(this);

    private void SourceChanged(object obj)
    {
        if (Source is not null)
        {
            Source.Save(stream);
        }
    }

    private void BoundsChanged(object obj)
    {
        if (Source is null)
        {
            return;
        }
        Rect viewPort = new(Bounds.Size);
        Size sourceSize = Source.Size;

        Vector scale = Stretch.CalculateScaling(Bounds.Size, sourceSize, StretchDirection);
        Size scaledSize = sourceSize * scale;
        dstRect = viewPort
                  .CenterRect(new Rect(scaledSize))
                  .Intersect(viewPort);
        srcRect = new Rect(sourceSize)
            .CenterRect(new Rect(dstRect.Size / scale));
    }

    private class RecolorImageRender(MemoryStream data, Rect src, Rect dest, Color color)
        : ICustomDrawOperation
    {
        private readonly Color color = color;
        private readonly MemoryStream data = data;
        private readonly Rect dest = dest;

        public Rect Bounds { get; } = src;

        public bool HitTest(Point p) => Bounds.Contains(p);

        public void Render(ImmediateDrawingContext drwContext)
        {
            ISkiaSharpApiLeaseFeature leaseFeature = drwContext.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature == null)
            {
                return;
            }

            using ISkiaSharpApiLease lease = leaseFeature.Lease();
            SKCanvas canvas = lease.SkCanvas;
            using SKBitmap bitmap = SKBitmap.Decode(data.ToArray());
            if (bitmap == null)
            {
                return;
            }
            using SKPaint paint = new();
            paint.IsAntialias = true;
            SKImage img = SKImage.FromBitmap(bitmap);
            paint.ImageFilter = SKImageFilter.CreateColorFilter(SKColorFilter.CreateBlendMode(color.ToSKColor(), SKBlendMode.Modulate));
            canvas.DrawImage(img, dest.ToSKRect(), Bounds.ToSKRect(), new SKSamplingOptions(SKCubicResampler.Mitchell), paint);
        }

        public bool Equals(ICustomDrawOperation other) => false;

        public void Dispose()
        {
            // ignored - we got no caches to dispose
        }
    }
}
