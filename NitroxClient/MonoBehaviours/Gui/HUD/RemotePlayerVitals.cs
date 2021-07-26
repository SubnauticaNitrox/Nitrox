using System;
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
        private Canvas canvas;
        private Bar foodBar;
        private Bar healthBar;
        private Bar oxygenBar;

        private string playerName;

        private Bar waterBar;

        /// <summary>
        ///     Creates a player vitals UI elements for the player id.
        /// </summary>
        /// <param name="playerId">Unique player id to create the vitals UI elements for.</param>
        /// <returns></returns>
        public static RemotePlayerVitals CreateForPlayer(ushort playerId)
        {
            RemotePlayerVitals vitals = new GameObject().AddComponent<RemotePlayerVitals>();
            RemotePlayer remotePlayer = NitroxServiceLocator.LocateService<PlayerManager>().Find(playerId).Value;

            vitals.canvas = vitals.CreateCanvas(remotePlayer.Body.transform);

            vitals.playerName = remotePlayer.PlayerName;
            vitals.CreatePlayerName(vitals.canvas);
            vitals.CreateStats(vitals.canvas);

            return vitals;
        }

        public void SetOxygen(float oxygen, float maxOxygen, float smoothTime = 0.1f)
        {
            oxygenBar?.SetTargetValue(oxygen, smoothTime);
            oxygenBar?.SetMaxValue(maxOxygen);
        }

        public void SetHealth(float health, float smoothTime = 0.1f)
        {
            healthBar?.SetTargetValue(health, smoothTime);
        }

        public void SetFood(float food, float smoothTime = 0.1f)
        {
            foodBar?.SetTargetValue(food, smoothTime);
        }

        public void SetWater(float water, float smoothTime = 0.1f)
        {
            waterBar?.SetTargetValue(water, smoothTime);
        }

        public void LateUpdate()
        {
            oxygenBar?.UpdateVisual();
            healthBar?.UpdateVisual();
            foodBar?.UpdateVisual();
            waterBar?.UpdateVisual();

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

        private void CreateStats(Canvas canvas)
        {
            healthBar = CreateBarIfAvailable(FindObjectOfType<uGUI_HealthBar>(), BarType.HEALTH, canvas);
            oxygenBar = CreateBarIfAvailable(FindObjectOfType<uGUI_OxygenBar>(), BarType.OXYGEN, canvas);
            foodBar = CreateBarIfAvailable(FindObjectOfType<uGUI_FoodBar>(), BarType.FOOD, canvas);
            waterBar = CreateBarIfAvailable(FindObjectOfType<uGUI_WaterBar>(), BarType.WATER, canvas);
        }

        private Bar CreateBarIfAvailable(MonoBehaviour behaviour, BarType barType, Canvas canvas)
        {
            return behaviour ? CreateBar(behaviour.gameObject, barType, canvas) : null;
        }

        private Bar CreateBar(GameObject originalBar, BarType type, Canvas canvas)
        {
            GameObject cloned = Instantiate(originalBar, canvas.transform, true);

            cloned.transform.localScale = new Vector3(0.0007f, 0.0007f, 0.0007f);
            cloned.transform.rotation = canvas.transform.rotation;

            uGUI_CircularBar newBar = cloned.GetComponentInChildren<uGUI_CircularBar>();
            newBar.texture = originalBar.GetComponentInChildren<uGUI_CircularBar>().texture;
            newBar.overlay = originalBar.GetComponentInChildren<uGUI_CircularBar>().overlay;
            switch (type)
            {
                case BarType.HEALTH:
                    newBar.color = HEALTH_BAR_COLOR;
                    newBar.borderColor = HEALTH_BAR_BORDER_COLOR;
                    cloned.transform.localPosition = new Vector3(-0.075f, 0.35f, 0f);
                    cloned.name = playerName + "'s Health";
                    cloned.RequireTransform("Icon").localRotation = Quaternion.Euler(0f, 0f, 0f);
                    Destroy(cloned.GetComponent<uGUI_HealthBar>());
                    cloned.transform.localScale = new Vector3(0.0007f, 0.0007f, 0.0007f);
                    break;
                case BarType.OXYGEN:
                    newBar.color = OXYGEN_BAR_COLOR;
                    newBar.borderColor = OXYGEN_BAR_BORDER_COLOR;
                    cloned.transform.localPosition = new Vector3(-0.025f, 0.35f, 0f);
                    cloned.name = playerName + "'s Oxygen";
                    cloned.RequireTransform("OxygenTextLabel").localRotation = Quaternion.Euler(0f, 270f, 0f);
                    Destroy(cloned.GetComponent<uGUI_OxygenBar>());
                    cloned.transform.localScale = new Vector3(0.0003f, 0.0003f, 0.0003f);
                    break;
                case BarType.FOOD:
                    newBar.color = FOOD_BAR_COLOR;
                    newBar.borderColor = FOOD_BAR_BORDER_COLOR;
                    cloned.transform.localPosition = new Vector3(0.025f, 0.35f, 0f);
                    cloned.name = playerName + "'s Food";
                    cloned.RequireTransform("Icon").localRotation = Quaternion.Euler(0f, 0f, 0f);
                    Destroy(cloned.GetComponent<uGUI_FoodBar>());
                    cloned.transform.localScale = new Vector3(0.0007f, 0.0007f, 0.0007f);
                    break;
                case BarType.WATER:
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

            return new Bar(newBar.gameObject, new SmoothedValue(100, 100, 100, 100));
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
            Transform nameTransform = nameText.transform;
            nameTransform.localScale = new Vector3(0.01f, 0.01f, 1f);
            nameTransform.rotation = canvas.transform.rotation;
            nameText.fontSize = 14;
            nameText.alignment = TextAnchor.MiddleCenter;

            // Text position
            namePosition = name.GetComponent<RectTransform>();
            namePosition.localPosition = new Vector3(0, 0.4f, 0);
            namePosition.sizeDelta = new Vector2(200, 100);
        }

        private void OnDestroy()
        {
            oxygenBar?.Dispose();
            healthBar?.Dispose();
            foodBar?.Dispose();
            waterBar?.Dispose();
        }

        private class Bar : IDisposable
        {
            private readonly GameObject gameObject;
            private readonly SmoothedValue smoothedValue;
            private bool isDisposed;

            public Bar(GameObject gameObject, SmoothedValue smoothedValue)
            {
                this.gameObject = gameObject;
                this.smoothedValue = smoothedValue;
            }

            public void SetTargetValue(float value, float smoothTime = 0.1f)
            {
                ThrowIfDisposed();

                smoothedValue.TargetValue = value;
                smoothedValue.SmoothTime = smoothTime;
            }

            public void SetMaxValue(float value)
            {
                ThrowIfDisposed();

                smoothedValue.MaxValue = value;
            }

            public void UpdateVisual()
            {
                ThrowIfDisposed();

                float vel = 0;
                smoothedValue.CurrentValue = Mathf.SmoothDamp(smoothedValue.CurrentValue, smoothedValue.TargetValue, ref vel, smoothedValue.SmoothTime);
                uGUI_CircularBar circularBar = gameObject.GetComponentInChildren<uGUI_CircularBar>();
                circularBar.value = smoothedValue.CurrentValue / smoothedValue.MaxValue;
            }

            public void Dispose()
            {
                if (isDisposed)
                {
                    return;
                }
                Destroy(gameObject);
                isDisposed = true;
            }

            private void ThrowIfDisposed()
            {
                if (isDisposed)
                {
                    throw new ObjectDisposedException("Tried to update visual on a disposed player stat.");
                }
            }
        }

        private enum BarType
        {
            HEALTH,
            OXYGEN,
            FOOD,
            WATER
        }
    }
}
