using NitroxClient.MonoBehaviours.Gui.Settings;
using NitroxClient.Unity.Helper;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours
{
    public class MultiplayerSettings : MonoBehaviour
    {
        private int TabIndex;
        private uGUI_OptionsPanel OptionsPanel;

        private void Start()
        {
            OptionsPanel = gameObject.RequireComponent<uGUI_OptionsPanel>();

            TabIndex = OptionsPanel.AddTab("Multiplayer");
            AddInputFieldOption("Name", "Enter Name", SettingsManager.Name, new UnityAction<string>(newName => SettingsManager.Name = newName));
            OptionsPanel.AddHeading(TabIndex, "Player color");
            OptionsPanel.AddSliderOption(TabIndex, "Red", SettingsManager.PlayerColor.r, 0, new UnityAction<float>(SettingsManager.SetPlayerColorR));
            OptionsPanel.AddSliderOption(TabIndex, "Green", SettingsManager.PlayerColor.g, 0, new UnityAction<float>(SettingsManager.SetPlayerColorG));
            OptionsPanel.AddSliderOption(TabIndex, "Blue", SettingsManager.PlayerColor.b, 0, new UnityAction<float>(SettingsManager.SetPlayerColorB));
            AddColorImage("Color", SettingsManager.PlayerColor);
            SettingsManager.RefreshColorImage();
        }

        public void AddInputFieldOption(string label, string placeholder, string value, UnityAction<string> callback = null)
        {
            GameObject gameObject = OptionsPanel.AddItem(TabIndex, InstantiateOptionsRoot(Instantiate(OptionsPanel.keyRedemptionPrefab.FindChild("InputField")), new Vector3(750, -30, 0), 200, "uGUI_InputFieldOption", label));
            InputField inputField = gameObject.RequireComponentInChildren<InputField>();

            inputField.text = value;
            inputField.GetComponentInChildren<Text>().text = placeholder;
            if (callback != null)
            {
                inputField.onValueChanged.AddListener(callback);
            }
        }

        public void AddColorImage(string label, Color value)
        {
            Image image = new GameObject().AddComponent<Image>();

            RectTransform rect = image.GetComponent<RectTransform>();
            rect.sizeDelta = new Vector2(200, 25);
            rect.localScale = new Vector3(2, 2, 1);
            image.sprite = OptionsPanel.troubleshootingPrefab.GetComponentInChildren<Image>().sprite;
            image.type = Image.Type.Sliced;
            image.color = value;

            GameObject gameObject = OptionsPanel.AddItem(TabIndex, InstantiateOptionsRoot(image.gameObject, new Vector3(235, 0, 0), 50, "uGUI_ColorImage", label));
            SettingsManager.ColorImage = gameObject.RequireComponentInChildren<Image>();
        }

        GameObject InstantiateOptionsRoot(GameObject child, Vector3 childPos, int inGameMenuPosX, string name, string label)
        {
            GameObject root = Instantiate(OptionsPanel.toggleOptionPrefab);
            GameObject caption = Instantiate(OptionsPanel.bindingOptionPrefab.FindChild("Caption"));
            DestroyImmediate(root.GetComponentInChildren<Toggle>().gameObject);

            if (Player.main != null)
            {
                childPos.x = childPos.x - inGameMenuPosX;
                RectTransform rect = child.GetComponent<RectTransform>();
                rect.sizeDelta = new Vector2(rect.sizeDelta.x - inGameMenuPosX / 2, rect.sizeDelta.y);
            }
            child.transform.localPosition = childPos;
            root.name = name;

            caption.GetComponent<Text>().text = label;
            child.transform.SetParent(caption.transform, false);
            caption.transform.SetParent(root.transform, false);
            return root;
        }
    }
}
