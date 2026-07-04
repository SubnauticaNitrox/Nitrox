using UnityEngine;

namespace NitroxClient.GameLogic.Bases;

public static class MapRoomCameraStatusOverlay
{
    private const float DEFAULT_DURATION_SECONDS = 3f;

    private static OverlayBehaviour overlay;

    public static void ShowAvailable(float durationSeconds = DEFAULT_DURATION_SECONDS)
    {
        Show("AVAILABLE", new Color(0.2f, 1f, 0.2f, 1f), durationSeconds);
    }

    public static void ShowInUse(float durationSeconds = DEFAULT_DURATION_SECONDS)
    {
        Show("IN USE", new Color(1f, 0.15f, 0.15f, 1f), durationSeconds);
    }

    private static void Show(string text, Color color, float durationSeconds)
    {
        EnsureOverlay();
        overlay.Show(text, color, durationSeconds);
    }

    private static void EnsureOverlay()
    {
        if (overlay)
        {
            return;
        }

        GameObject overlayObject = new("[Nitrox] MapRoomCameraStatusOverlay");
        UnityEngine.Object.DontDestroyOnLoad(overlayObject);
        overlay = overlayObject.AddComponent<OverlayBehaviour>();
    }

    private sealed class OverlayBehaviour : MonoBehaviour
    {
        private const float X = 24f;
        private const float Y = 24f;
        private const float WIDTH = 420f;
        private const float HEIGHT = 64f;

        private string text;
        private Color color;
        private float hideAtTime;
        private GUIStyle style;

        public void Show(string statusText, Color statusColor, float durationSeconds)
        {
            text = statusText;
            color = statusColor;
            hideAtTime = Time.realtimeSinceStartup + durationSeconds;
        }

        private void OnGUI()
        {
            if (string.IsNullOrEmpty(text) || Time.realtimeSinceStartup > hideAtTime)
            {
                return;
            }

            style ??= new GUIStyle(GUI.skin.label)
            {
                fontSize = 30,
                fontStyle = FontStyle.Bold,
                alignment = TextAnchor.UpperLeft
            };

            style.normal.textColor = color;

            GUI.depth = -1000;
            GUI.Label(new Rect(X, Y, WIDTH, HEIGHT), text, style);
        }
    }
}
