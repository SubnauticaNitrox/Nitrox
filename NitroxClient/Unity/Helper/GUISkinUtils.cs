using UnityEngine;

namespace NitroxClient.Unity.Helper
{
    public static class GUISkinUtils
    {
        private static Texture2D unityWindowTexture;

        static GUISkinUtils()
        {
            if (unityWindowTexture == null)
            {
                // TODO: Find a way to load the "window" background texture that is in Library/standard unity resources.
                //unityWindowTexture = Resources.Load<Texture2D>("TODO");
            }
        }

        public static GUISkin CreateDefault()
        {
            // TODO: Create default GUISkin to base new skins on.
            return new GUISkin()
            {

            };
        }
    }
}
