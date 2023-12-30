extern alias JB;
using System;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Platform;
using Avalonia.Rendering.SceneGraph;
using Avalonia.Skia;
using SkiaSharp;
using AControls = Avalonia.Controls.Controls;

namespace Nitrox.Launcher.Models.Design.Controls;

/// <summary>
/// Draws a blur filter over the already rendered content.
/// </summary>
/// <remarks>
/// Based off of GrayscaleControl
/// </remarks>
public class BlurControl : Decorator
{
    static BlurControl()
    {
        AffectsRender<BlurControl>(OpacityProperty);
    }

    public override void Render(DrawingContext context)
    {
        context.Custom(new BlurBehindRenderOperation((byte)Math.Round(byte.MaxValue * Opacity), new Rect(default, Bounds.Size)));
    }

    private class BlurBehindRenderOperation : ICustomDrawOperation
    {
        private readonly byte opacity;
        private readonly Rect bounds;

        public Rect Bounds => bounds;

        public BlurBehindRenderOperation(byte opacity, Rect bounds)
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
            using SKImageFilter blurFilter = SKImageFilter.CreateBlur(7, 7);
            using SKPaint paint = new()
            {
                Shader = backdropShader,
                ImageFilter = blurFilter,
                Color = new SKColor(0, 0, 0, opacity)
            };
            skia.SkCanvas.DrawRect(0, 0, (float)bounds.Width, (float)bounds.Height, paint);
        }

        public bool Equals(ICustomDrawOperation other) => other is BlurBehindRenderOperation op && op.bounds == bounds;
    }
}
