using NitroxClient.MonoBehaviours.Gui.Helper;
using NitroxClient.Unity.Helper;
using NitroxModel.Helper;
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

        private static readonly Vector2 OXYGEN_BAR_POSITION_OFFSET = new Vector2(200, 160);
        private static readonly Vector2 HEALTH_BAR_POSITION_OFFSET = new Vector2(150, 160);
        private static readonly Vector2 FOOD_BAR_POSITION_OFFSET = new Vector2(100, 160);
        private static readonly Vector2 WATER_BAR_POSITION_OFFSET = new Vector2(50, 160);

        private string playerName;
        private int position;

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

        private Bar oxygenBar;
        private Bar healthBar;
        private Bar foodBar;
        private Bar waterBar;

        private GameObject background;
        private GameObject playerNameText;

        public void CreateVitals(ulong playerId, int position)
        {
            this.playerName = playerId.ToString();
            this.position = position;

            uGUI_HealthBar originalBar = FindObjectOfType<uGUI_HealthBar>();

            if (originalBar == null)
            {
                Log.Warn("uGUI_HealthBar does not exist. Are you playing on creative?");
                // TODO: Make sure it works when the world changes back to survival.
                return;
            }

            oxygenBar = CreateBar(originalBar, OXYGEN_BAR_COLOR, OXYGEN_BAR_BORDER_COLOR, "s");
            healthBar = CreateBar(originalBar, HEALTH_BAR_COLOR, HEALTH_BAR_BORDER_COLOR, "%");
            foodBar = CreateBar(originalBar, FOOD_BAR_COLOR, FOOD_BAR_BORDER_COLOR, "%");
            waterBar = CreateBar(originalBar, WATER_BAR_COLOR, WATER_BAR_BORDER_COLOR, "%");

            Canvas canvas = oxygenBar.GameObject.GetComponentInParent<Canvas>();
            GameObject componentParent = oxygenBar.GameObject.transform.parent.gameObject;
            Quaternion rotation = oxygenBar.GameObject.transform.localRotation;

            CreateBackground(canvas, componentParent, rotation);
            CreatePlayerNameText();
            SetNewPosition(position);
        }

        private Bar CreateBar(uGUI_HealthBar originalBar, Color color, Color borderColor, string smoothedValueUnit)
        {
            GameObject cloned = Instantiate(originalBar.gameObject);
            cloned.transform.SetParent(originalBar.gameObject.transform.parent.transform);
            Destroy(cloned.GetComponent<uGUI_HealthBar>());

            cloned.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            cloned.transform.localRotation = originalBar.gameObject.transform.localRotation;
            cloned.RequireTransform("Icon").localRotation = Quaternion.Euler(0f, 180, 0f);

            Canvas canvas = originalBar.gameObject.GetComponentInParent<Canvas>();

            // Not sure why this is needed, but if a initial transform is not set
            // then it will be in a weird, inconsistent state.
            SetBarPostion(cloned.gameObject, new Vector2(0, 0), canvas);

            uGUI_CircularBar newBar = cloned.GetComponentInChildren<uGUI_CircularBar>();
            newBar.texture = originalBar.bar.texture;
            newBar.overlay = originalBar.bar.overlay;
            newBar.color = color;
            newBar.borderColor = borderColor;

            return new Bar(cloned, new SmoothedValue(100, 100, 100, 100), smoothedValueUnit);
        }

        private void CreateBackground(Canvas canvas, GameObject parent, Quaternion rotation)
        {
            background = new GameObject();
            Image image = background.AddComponent<Image>();
            image.transform.SetParent(parent.transform);
            image.sprite = GetPlayerBackgroundSprite();

            Vector2 screenPosition = new Vector2(Screen.width - 120, Screen.height - (150 * position));
            Vector2 worldPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPosition, Camera.main, out worldPosition);
            background.transform.position = canvas.transform.TransformPoint(worldPosition);
            background.SetActive(true);
            background.transform.localScale = new Vector3(2.7f, 1.2f, 1);
            background.transform.localRotation = rotation;
            background.transform.SetAsFirstSibling();
        }

        private void CreatePlayerNameText()
        {
            playerNameText = new GameObject();
            GUIText GUIText = playerNameText.AddComponent<GUIText>();
            playerNameText.AddComponent<GUITextShadow>();
            GUIText.name = "RemotePlayer" + playerName;
            GUIText.alignment = TextAlignment.Center;
            GUIText.fontSize = 18;
            GUIText.text = playerName;
            ErrorMessage em = (ErrorMessage)ReflectionHelper.ReflectionGet<ErrorMessage>(null, "main", false, true);
            playerNameText.layer = em.gameObject.layer;
            playerNameText.layer = em.gameObject.layer;
            // em does not have a parent anymore on latest stable, so the stats position is incorrect.
            playerNameText.transform.parent = em.transform;//.parent.transform;
        }

        private Sprite GetPlayerBackgroundSprite()
        {
            byte[] FileData = Properties.Resources.playerBackgroundImage;
            Texture2D circleSquare = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            circleSquare.LoadImage(FileData);
            return Sprite.Create(circleSquare, new Rect(0, 0, circleSquare.width, circleSquare.height), new Vector2(0, 0), 100);
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

            int rounded = Mathf.RoundToInt(amount);

            Text text = bar.GameObject.GetComponentInChildren<Text>();
            text.text = ((int)rounded).ToString() + bar.ValueUnit;
        }

        public void SetNewPosition(int newPosition)
        {
            Canvas canvas = healthBar.GameObject.GetComponentInParent<Canvas>();
            SetBarPostion(oxygenBar.GameObject, OXYGEN_BAR_POSITION_OFFSET, canvas);
            SetBarPostion(healthBar.GameObject, HEALTH_BAR_POSITION_OFFSET, canvas);
            SetBarPostion(foodBar.GameObject, FOOD_BAR_POSITION_OFFSET, canvas);
            SetBarPostion(waterBar.GameObject, WATER_BAR_POSITION_OFFSET, canvas);

            GUIText gUIText = playerNameText.GetComponent<GUIText>();
            gUIText.transform.position = new Vector3(0.91f, 0.90f - ((position - 1) * 0.15f), 1f);
        }

        private void SetBarPostion(GameObject barGameObject, Vector2 positionOffset, Canvas canvas)
        {
            Vector2 screenPosition = new Vector2(Screen.width - positionOffset.x, Screen.height - (positionOffset.y * position));
            Vector2 worldPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPosition, Camera.main, out worldPosition);
            barGameObject.transform.position = canvas.transform.TransformPoint(worldPosition);
        }

        void OnDestroy()
        {
            Destroy(oxygenBar.GameObject);
            Destroy(healthBar.GameObject);
            Destroy(foodBar.GameObject);
            Destroy(waterBar.GameObject);
            Destroy(background);
            Destroy(playerNameText);
        }
    }
}
