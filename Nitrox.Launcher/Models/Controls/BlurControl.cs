using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;

namespace Nitrox.Launcher.Models.Controls;

/// <summary>
///     Draws a blur filter over the already rendered content.
/// </summary>
/// <remarks>
///     Based off of GrayscaleControl
/// </remarks>
public sealed class BlurControl : Decorator
{
    public static readonly StyledProperty<float> BlurStrengthProperty =
        AvaloniaProperty.Register<BlurControl, float>(nameof(BlurStrength), 5);

    /// <summary>
    ///     Sets or gets how strong the blur should be. Defaults to 5.
    /// </summary>
    public float BlurStrength
    {
        get => GetValue(BlurStrengthProperty);
        set => SetValue(BlurStrengthProperty, value);
    }

    static BlurControl()
    {
        ClipToBoundsProperty.OverrideDefaultValue<BlurControl>(true);
        AffectsRender<BlurControl>(OpacityProperty);
        AffectsRender<BlurControl>(BlurStrengthProperty);
    }

    public override void Render(DrawingContext context)
    {
        context.Custom(new BlurBehindRenderOperation((byte)Math.Round(byte.MaxValue * Opacity), BlurStrength, new Rect(default, Bounds.Size)));
    }

    private sealed record BlurBehindRenderOperation : ICustomDrawOperation
    {
        private readonly Rect bounds;
        private readonly byte opacity;
        private readonly float strength;

        public Rect Bounds => bounds;

        public BlurBehindRenderOperation(byte opacity, float strength, Rect bounds)
        {
            this.opacity = opacity;
            this.strength = strength;
            this.bounds = bounds;
        }

        public void Dispose()
        {
        }

        public bool HitTest(Point p) => bounds.Contains(p);

        public void Render(ImmediateDrawingContext context)
        {
            ISkiaSharpApiLeaseFeature leaseFeature = context.TryGetFeature<ISkiaSharpApiLeaseFeature>();
            if (leaseFeature == null)
            {
                return;
            }
            using ISkiaSharpApiLease skia = leaseFeature.Lease();
            if (!skia.SkCanvas.TotalMatrix.TryInvert(out SKMatrix currentInvertedTransform))
            {
                return;
            }
            if (skia.SkSurface == null)
            {
                return;
            }

            using SKImage backgroundSnapshot = skia.SkSurface.Snapshot();
            using SKShader backdropShader = SKShader.CreateImage(backgroundSnapshot, SKShaderTileMode.Clamp, SKShaderTileMode.Clamp, currentInvertedTransform);
            using SKImageFilter blurFilter = SKImageFilter.CreateBlur(strength, strength);
            using SKPaint paint = new();
            paint.Shader = backdropShader;
            paint.ImageFilter = blurFilter;
            paint.Color = new SKColor(0, 0, 0, opacity);
            skia.SkCanvas.DrawRect(0, 0, (float)bounds.Width, (float)bounds.Height, paint);
        }

        public bool Equals(ICustomDrawOperation other) => Equals(other as BlurBehindRenderOperation);
    }
}
