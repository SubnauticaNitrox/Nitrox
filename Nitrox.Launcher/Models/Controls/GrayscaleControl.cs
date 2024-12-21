extern alias JB;
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
/// Draws a grayscale filter over the already rendered content.
/// </summary>
/// <remarks>
/// Code from:<br/>
///  - Draw-on-top logic: https://gist.github.com/kekekeks/ac06098a74fe87d49a9ff9ea37fa67bc <br/>
///  - Grayscale logic: https://learn.microsoft.com/en-us/xamarin/xamarin-forms/user-interface/graphics/skiasharp/effects/color-filters <br/>
/// </remarks>
public class GrayscaleControl : Decorator
{
    static GrayscaleControl()
    {
        AffectsRender<GrayscaleControl>(OpacityProperty);
    }

    public override void Render(DrawingContext context)
    {
        context.Custom(new GrayscaleBehindRenderOperation((byte)Math.Round(byte.MaxValue * Opacity), new Rect(default, Bounds.Size)));
    }

    private class GrayscaleBehindRenderOperation : ICustomDrawOperation
    {
        private static readonly float[] grayscaleColorFilterMatrix =
        {
            0.21f, 0.72f, 0.07f, 0, 0,
            0.21f, 0.72f, 0.07f, 0, 0,
            0.21f, 0.72f, 0.07f, 0, 0,
            0, 0, 0, 1, 0
        };

        private readonly byte opacity;
        private readonly Rect bounds;

        public Rect Bounds => bounds;

        public GrayscaleBehindRenderOperation(byte opacity, Rect bounds)
        {
            this.opacity = opacity;
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
            using SKImageFilter grayscaleFilter = SKImageFilter.CreateColorFilter(CreateGrayscaleColorFilter());
            using SKPaint paint = new()
            {
                Shader = backdropShader,
                ImageFilter = grayscaleFilter,
                Color = new SKColor(0, 0, 0, opacity)
            };
            skia.SkCanvas.DrawRect(0, 0, (float)bounds.Width, (float)bounds.Height, paint);
        }

        public bool Equals(ICustomDrawOperation other) => other is GrayscaleBehindRenderOperation op && op.bounds == bounds;

        private static SKColorFilter CreateGrayscaleColorFilter() => SKColorFilter.CreateColorMatrix(grayscaleColorFilterMatrix);
    }
}
