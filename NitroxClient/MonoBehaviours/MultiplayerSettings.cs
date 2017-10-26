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
            AddInputFieldOption("Name", "Enter Name", SettingsManager.GetName(), new UnityAction<string>(newName => SettingsManager.Name= newName));
            OptionsPanel.AddHeading(TabIndex, "Player color");
            OptionsPanel.AddSliderOption(TabIndex, "Red", SettingsManager.PlayerColor.r, 0, new UnityAction<float>(SettingsManager.SetPlayerColorR));
            OptionsPanel.AddSliderOption(TabIndex, "Green", SettingsManager.PlayerColor.g, 0, new UnityAction<float>(SettingsManager.SetPlayerColorG));
            OptionsPanel.AddSliderOption(TabIndex, "Blue", SettingsManager.PlayerColor.b, 0, new UnityAction<float>(SettingsManager.SetPlayerColorB));
            AddColorImage("Color", SettingsManager.PlayerColor);
            SettingsManager.RefreshColorImage();
        }

        public void AddInputFieldOption(string label, string placeholder, string value, UnityAction<string> callback = null)
        {
            GameObject input_field = Instantiate(OptionsPanel.keyRedemptionPrefab.FindChild("InputField"));
            GameObject gameObject = OptionsPanel.AddItem(TabIndex, InstantiateOptionsRoot(input_field, new Vector3(750, -30, 0), "uGUI_InputFieldOption", label));

            InputField componentInChildren = gameObject.RequireComponentInChildren<InputField>();

            componentInChildren.text = value;
            componentInChildren.GetComponentsInChildren<Text>()[0].text = placeholder;
            if (callback != null)
            {
                componentInChildren.onValueChanged.AddListener(callback);
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

            GameObject gameObject = OptionsPanel.AddItem(TabIndex, InstantiateOptionsRoot(image.gameObject, new Vector3(235, 0, 0), "uGUI_ColorImage", label));
            SettingsManager.ColorImage = gameObject.RequireComponentInChildren<Image>();
        }

        GameObject InstantiateOptionsRoot(GameObject child, Vector3 childPos, string name, string label)
        {
            GameObject root = new GameObject {
                name = name
            };
            LayoutElement layout = root.AddComponent<LayoutElement>();
            layout.minHeight = 60;
            layout.preferredWidth = -1;
            layout.preferredHeight = 60;

            child.transform.localPosition = childPos;

            GameObject caption = Instantiate(OptionsPanel.bindingOptionPrefab.FindChild("Caption"));
            caption.GetComponent<Text>().text = label;
            child.transform.SetParent(caption.transform, false);
            caption.transform.SetParent(root.transform, false);
            return root;
        }
    }
}
