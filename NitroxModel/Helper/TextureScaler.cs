using UnityEngine;

namespace NitroxModel.Helper
{
    public static class TextureScaler
    {
        public static void Scale(Texture2D tex, int width, int height, FilterMode mode = FilterMode.Trilinear)
        {
            Rect texR = new Rect(0, 0, width, height);
            GpuScale(tex, width, height, mode);

            tex.Resize(width, height);
            tex.ReadPixels(texR, 0, 0, true);
            tex.Apply(true);
        }

        internal static void GpuScale(Texture2D src, int width, int height, FilterMode fmode)
        {
            src.filterMode = fmode;
            src.Apply(true);

            RenderTexture rtt = new RenderTexture(width, height, 32);

            Graphics.SetRenderTarget(rtt);

            GL.LoadPixelMatrix(0, 1, 1, 0);

            GL.Clear(true, true, new Color(0, 0, 0, 0));
            Graphics.DrawTexture(new Rect(0, 0, 1, 1), src);
        }
    }
}
