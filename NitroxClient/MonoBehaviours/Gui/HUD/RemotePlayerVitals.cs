using System;
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

        public String playerName;
        public int position;

        private GameObject oxygenBar;
        private GameObject healthBar;
        private GameObject foodBar;
        private GameObject waterBar;
        private GameObject background;
        private GameObject playerNameText;
       
        public void CreateVitals(String playerName, int position)
        {
            this.playerName = playerName;
            this.position = position;
                        
            oxygenBar = cloneCircularBarParent(OXYGEN_BAR_COLOR, OXYGEN_BAR_BORDER_COLOR);
            healthBar = cloneCircularBarParent(HEALTH_BAR_COLOR, HEALTH_BAR_BORDER_COLOR);
            foodBar = cloneCircularBarParent(FOOD_BAR_COLOR, FOOD_BAR_BORDER_COLOR);
            waterBar = cloneCircularBarParent(WATER_BAR_COLOR, WATER_BAR_BORDER_COLOR);
                        
            Canvas canvas = oxygenBar.GetComponentInParent<Canvas>();
            GameObject componentParent = oxygenBar.transform.parent.gameObject;
            Quaternion rotation = oxygenBar.transform.localRotation;

            CreateBackground(canvas, componentParent, rotation);
            CreatePlayerNameText();
            SetNewPosition(position);
        }

        private GameObject cloneCircularBarParent(Color color, Color borderColor)
        {
            uGUI_HealthBar healthBar = (uGUI_HealthBar)FindObjectOfType(typeof(uGUI_HealthBar));

            GameObject cloned = Instantiate(healthBar.gameObject);
            cloned.transform.SetParent(healthBar.gameObject.transform.parent.transform);
            Destroy(cloned.GetComponent<uGUI_HealthBar>());
            
            cloned.transform.localScale = new Vector3(0.7f, 0.7f, 0.7f);
            cloned.transform.localRotation = healthBar.gameObject.transform.localRotation;
            cloned.transform.Find("Icon").localRotation = Quaternion.Euler(0f, 180, 0f);

            Canvas canvas = healthBar.gameObject.GetComponentInParent<Canvas>();

            //Not sure why this is needed, but if a initial transform is not set 
            //then it will be in a weird, inconsistent state.
            SetBarPostion(cloned, new Vector2(0, 0), canvas);

            uGUI_CircularBar newBar = cloned.GetComponentInChildren<uGUI_CircularBar>();
            newBar.texture = healthBar.bar.texture;
            newBar.overlay = healthBar.bar.overlay;
            newBar.color = color;
            newBar.borderColor = borderColor;

            setBarAmount(cloned, 100);

            return cloned;
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
            GUIText gUIText = playerNameText.AddComponent<GUIText>();
            playerNameText.AddComponent(typeof(GUITextShadow));
            gUIText.name = "RemotePlayer" + playerName;
            gUIText.alignment = TextAlignment.Center;
            gUIText.fontSize = 18;
            gUIText.text = playerName;
            playerNameText.layer = ErrorMessage.main.gameObject.layer;
            playerNameText.transform.parent = ErrorMessage.main.transform.parent.transform;
        }

        private Sprite GetPlayerBackgroundSprite()
        {
            byte[] FileData = Properties.Resources.playerBackgroundImage;            
            Texture2D circleSquare = new Texture2D(2, 2, TextureFormat.ARGB32, false);
            circleSquare.LoadImage(FileData);
            return Sprite.Create(circleSquare, new Rect(0, 0, circleSquare.width, circleSquare.height), new Vector2(0, 0), 100);
        }

        public void SetOxygen(float oxygen, float maxOxygen)
        {
            setBarAmount(oxygenBar, oxygen, maxOxygen);
        }

        public void SetHealth(float health)
        {
            setBarAmount(healthBar, health);
        }

        public void SetFood(float food)
        {
            setBarAmount(foodBar, food);
        }

        public void SetWater(float water)
        {
            setBarAmount(waterBar, water);
        }

        private void setBarAmount(GameObject barGameObject, float amount, float max = 100.0f)
        {
            uGUI_CircularBar bar = barGameObject.GetComponentInChildren<uGUI_CircularBar>();
            bar.value = amount / max;

            int rounded = Mathf.RoundToInt(amount);

            Text text = barGameObject.GetComponentInChildren<Text>();
            text.text = ((int)rounded).ToString();
        }

        public void SetNewPosition(int newPosition)
        {
            Canvas canvas = healthBar.GetComponentInParent<Canvas>();
            SetBarPostion(oxygenBar, OXYGEN_BAR_POSITION_OFFSET, canvas);
            SetBarPostion(healthBar, HEALTH_BAR_POSITION_OFFSET, canvas);
            SetBarPostion(foodBar, FOOD_BAR_POSITION_OFFSET, canvas);
            SetBarPostion(waterBar, WATER_BAR_POSITION_OFFSET, canvas);

            GUIText gUIText = playerNameText.GetComponent<GUIText>();
            gUIText.transform.position = new Vector3(0.91f, 0.90f - ((position - 1) * 0.15f), 1f);
        }

        private void SetBarPostion(GameObject bar, Vector2 positionOffset, Canvas canvas)
        {
            Vector2 screenPosition = new Vector2(Screen.width - positionOffset.x, Screen.height - (positionOffset.y * position));
            Vector2 worldPosition;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(canvas.transform as RectTransform, screenPosition, Camera.main, out worldPosition);
            bar.transform.position = canvas.transform.TransformPoint(worldPosition);
        }

        void OnDestroy()
        {
            Destroy(oxygenBar);
            Destroy(healthBar);
            Destroy(foodBar);
            Destroy(waterBar);
            Destroy(background);
            Destroy(playerNameText);
        }
    }
}
