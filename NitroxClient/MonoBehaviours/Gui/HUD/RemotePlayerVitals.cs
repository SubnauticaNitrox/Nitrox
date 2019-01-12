using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours.Gui.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Core;
using NitroxModel.Logger;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.HUD
{
    public class RemotePlayerVitals : MonoBehaviour
    {
        private static readonly Color OXYGEN_BAR_COLOR = new Color(0.168f, 0.666f, 0.60f, 1.0f);
        private static readonly Color OXYGEN_BAR_BORDER_COLOR = new Color(0.227f, 0.949f, 0.969f, 1.0f);
        private static readonly Color HEALTH_BAR_COLOR = new Color(0.859f, 0.373f, 0.251f, 1.0f);
        private static readonly Color HEALTH_BAR_BORDER_COLOR = new Color(0.824f, 0.651f, 0.424f, 1.0f);
        private static readonly Color FOOD_BAR_COLOR = new Color(0.965f, 0.655f, 0.149f, 1.0f);
        private static readonly Color FOOD_BAR_BORDER_COLOR = new Color(0.957f, 0.914f, 0.251f, 1.0f);
        private static readonly Color WATER_BAR_COLOR = new Color(0.212f, 0.663f, 0.855f, 1.0f);
        private static readonly Color WATER_BAR_BORDER_COLOR = new Color(0.227f, 0.949f, 0.969f, 1.0f);
        private Bar foodBar;
        private Bar healthBar;

        private Bar oxygenBar;

        private string playerName;
        private Bar waterBar;

        private Canvas canvas;

        /// <summary>
        ///     Creates a player vitals UI elements for the player id.
        /// </summary>
        /// <param name="playerId">Unique player id to create the vitals UI elements for.</param>
        /// <returns></returns>
        public static RemotePlayerVitals CreateForPlayer(ushort playerId)
        {
            RemotePlayerVitals vitals = new GameObject().AddComponent<RemotePlayerVitals>();
            RemotePlayer remotePlayer = NitroxServiceLocator.LocateService<PlayerManager>().Find(playerId).Get();

            vitals.canvas = vitals.CreateCanvas(remotePlayer.Body.transform);

            vitals.playerName = remotePlayer.PlayerName;
            vitals.CreatePlayerName(vitals.canvas);
            vitals.CreateStats(remotePlayer, vitals.canvas);

            return vitals;
        }

        public void SetOxygen(float oxygen, float maxOxygen, float smoothTime = 0.1f)
        {
            oxygenBar.SmoothedValue.TargetValue = oxygen;
            oxygenBar.SmoothedValue.MaxValue = maxOxygen;
            oxygenBar.SmoothedValue.SmoothTime = smoothTime;
        }

        public void SetHealth(float health, float smoothTime = 0.1f)
        {
            healthBar.SmoothedValue.TargetValue = health;
            healthBar.SmoothedValue.SmoothTime = smoothTime;
        }

        public void SetFood(float food, float smoothTime = 0.1f)
        {
            foodBar.SmoothedValue.TargetValue = food;
            foodBar.SmoothedValue.SmoothTime = smoothTime;
        }

        public void SetWater(float water, float smoothTime = 0.1f)
        {
            waterBar.SmoothedValue.TargetValue = water;
            waterBar.SmoothedValue.SmoothTime = smoothTime;
        }

        public void LateUpdate()
        {
            UpdateSmoothValue(oxygenBar);
            UpdateSmoothValue(healthBar);
            UpdateSmoothValue(foodBar);
            UpdateSmoothValue(waterBar);

            // Make canvas face camera.
            canvas.transform.forward = Camera.main.transform.forward;
        }

        private Canvas CreateCanvas(Transform playerTransform)
        {
            // Canvas
            GameObject vitals = new GameObject();
            vitals.name = "RemotePlayerVitals";
            vitals.AddComponent<Canvas>();
            vitals.transform.SetParent(playerTransform, false);
            vitals.transform.localPosition = new Vector3(0, 0, 0);

            Canvas vitalsCanvas = vitals.GetComponent<Canvas>();
            vitalsCanvas.renderMode = RenderMode.WorldSpace;

            CanvasScaler scaler = vitals.AddComponent<CanvasScaler>();
            scaler.dynamicPixelsPerUnit = 100;

            return vitalsCanvas;
        }

        private void CreateStats(RemotePlayer remotePlayer, Canvas canvas)
        {
            GameObject originalBar = FindObjectOfType<uGUI_HealthBar>().gameObject;
            healthBar = CreateBar(originalBar, BarType.Health, canvas);

            originalBar = FindObjectOfType<uGUI_OxygenBar>().gameObject;
            oxygenBar = CreateBar(originalBar, BarType.Oxygen, canvas);

            originalBar = FindObjectOfType<uGUI_FoodBar>().gameObject;
            foodBar = CreateBar(originalBar, BarType.Food, canvas);

            originalBar = FindObjectOfType<uGUI_WaterBar>().gameObject;
            waterBar = CreateBar(originalBar, BarType.Water, canvas);
        }

        private Bar CreateBar(GameObject originalBar, BarType type, Canvas canvas)
        {
            GameObject cloned = Instantiate(originalBar);
            cloned.transform.SetParent(canvas.transform);

            cloned.transform.localScale = new Vector3(0.0007f, 0.0007f, 0.0007f);
            cloned.transform.rotation = canvas.transform.rotation;

            uGUI_CircularBar newBar = cloned.GetComponentInChildren<uGUI_CircularBar>();
            newBar.texture = originalBar.GetComponentInChildren<uGUI_CircularBar>().texture;
            newBar.overlay = originalBar.GetComponentInChildren<uGUI_CircularBar>().overlay;
            string valueUnit = "%";
            switch (type)
            {
                case BarType.Health:
                    newBar.color = HEALTH_BAR_COLOR;
                    newBar.borderColor = HEALTH_BAR_BORDER_COLOR;
                    cloned.transform.localPosition = new Vector3(-0.075f, 0.35f, 0f);
                    cloned.name = playerName + "'s Health";
                    cloned.RequireTransform("Icon").localRotation = Quaternion.Euler(0f, 0f, 0f);
                    Destroy(cloned.GetComponent<uGUI_HealthBar>());
                    cloned.transform.localScale = new Vector3(0.0007f, 0.0007f, 0.0007f);
                    break;
                case BarType.Oxygen:
                    newBar.color = OXYGEN_BAR_COLOR;
                    newBar.borderColor = OXYGEN_BAR_BORDER_COLOR;
                    cloned.transform.localPosition = new Vector3(-0.025f, 0.35f, 0f);
                    valueUnit = "s";
                    cloned.name = playerName + "'s Oxygen";
                    cloned.RequireTransform("OxygenTextLabel").localRotation = Quaternion.Euler(0f, 270f, 0f);
                    Destroy(cloned.GetComponent<uGUI_OxygenBar>());
                    cloned.transform.localScale = new Vector3(0.0003f, 0.0003f, 0.0003f);
                    break;
                case BarType.Food:
                    newBar.color = FOOD_BAR_COLOR;
                    newBar.borderColor = FOOD_BAR_BORDER_COLOR;
                    cloned.transform.localPosition = new Vector3(0.025f, 0.35f, 0f);
                    cloned.name = playerName + "'s Food";
                    cloned.RequireTransform("Icon").localRotation = Quaternion.Euler(0f, 0f, 0f);
                    Destroy(cloned.GetComponent<uGUI_FoodBar>());
                    cloned.transform.localScale = new Vector3(0.0007f, 0.0007f, 0.0007f);
                    break;
                case BarType.Water:
                    newBar.color = WATER_BAR_COLOR;
                    newBar.borderColor = WATER_BAR_BORDER_COLOR;
                    cloned.transform.localPosition = new Vector3(0.075f, 0.35f, 0f);
                    cloned.name = playerName + "'s Water";
                    cloned.RequireTransform("Icon").localRotation = Quaternion.Euler(0f, 0f, 0f);
                    Destroy(cloned.GetComponent<uGUI_WaterBar>());
                    cloned.transform.localScale = new Vector3(0.0007f, 0.0007f, 0.0007f);
                    break;
                default:
                    Log.Info("Unhandled bar type: " + type);
                    break;
            }

            return new Bar(newBar.gameObject, new SmoothedValue(100, 100, 100, 100), valueUnit);
        }

        private void CreatePlayerName(Canvas canvas)
        {
            GameObject name;
            Text nameText;
            RectTransform namePosition;

            // Text
            name = new GameObject();
            name.transform.parent = canvas.transform;
            name.name = "RemotePlayerName";

            nameText = name.AddComponent<Text>();
            nameText.font = Resources.GetBuiltinResource<Font>("Arial.ttf");
            nameText.text = playerName;
            nameText.transform.localScale = new Vector3(0.01f, 0.01f, 1f);
            nameText.transform.rotation = canvas.transform.rotation;
            nameText.fontSize = 14;
            nameText.alignment = TextAnchor.MiddleCenter;

            // Text position
            namePosition = name.GetComponent<RectTransform>();
            namePosition.localPosition = new Vector3(0, 0.4f, 0);
            namePosition.sizeDelta = new Vector2(200, 100);
        }

        private void UpdateSmoothValue(Bar bar)
        {
            if (bar != null)
            {
                float vel = 0;
                SmoothedValue smoothedValue = bar.SmoothedValue;
                smoothedValue.CurrentValue = Mathf.SmoothDamp(smoothedValue.CurrentValue, smoothedValue.TargetValue, ref vel, smoothedValue.SmoothTime);
                SetBarAmount(bar, smoothedValue.CurrentValue, smoothedValue.MaxValue);
            }
        }

        private void SetBarAmount(Bar bar, float amount, float max)
        {
            uGUI_CircularBar circularBar = bar.GameObject.GetComponentInChildren<uGUI_CircularBar>();
            circularBar.value = amount / max;
        }

        private void OnDestroy()
        {
            Destroy(oxygenBar.GameObject);
            Destroy(healthBar.GameObject);
            Destroy(foodBar.GameObject);
            Destroy(waterBar.GameObject);
        }

        private class Bar
        {
            public readonly GameObject GameObject;
            public readonly SmoothedValue SmoothedValue;
            public readonly string ValueUnit;

            public Bar(GameObject gameObject, SmoothedValue smoothedValue, string valueUnit)
            {
                GameObject = gameObject;
                SmoothedValue = smoothedValue;
                ValueUnit = valueUnit;
            }
        }

        private enum BarType
        {
            Health,
            Oxygen,
            Food,
            Water
        }
    }
}
