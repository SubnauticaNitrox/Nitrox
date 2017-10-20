using NitroxClient.Unity.Helper;
using NitroxModel.Logger;
using Steamworks;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.Settings {
    public class SettingsOptions {
        public void AddInputFieldOption(int tabIndex, string label, string placeholder, string value, uGUI_OptionsPanel uGUI_Options, UnityAction<string> callback = null) {
            uGUI_MainMenu uGUI_Main = uGUI_MainMenu.main;
            uGUI_TabbedControlsPanel uGUI_Control = uGUI_Main.GetComponentInChildren<uGUI_TabbedControlsPanel>();
            GameObject InputOption = new GameObject();
            GameObject input_field = Object.Instantiate(uGUI_Options.keyRedemptionPrefab.transform.GetChild(1).gameObject);
            GameObject caption = Object.Instantiate(uGUI_Options.toggleOptionPrefab.transform.GetChild(0).GetChild(2).gameObject);

            caption.GetComponent<Text>().text = label;
            input_field.transform.SetParent(caption.transform);
            caption.transform.SetParent(InputOption.transform);

            InputOption.name = "uGUI_InputFieldOption";
            InputOption.AddComponent<LayoutElement>();
            InputOption.GetComponent<LayoutElement>().minHeight = 60;
            InputOption.GetComponent<LayoutElement>().preferredWidth = -1;
            InputOption.GetComponent<LayoutElement>().preferredHeight = 60;
            input_field.transform.localPosition = new Vector3(750, 15, input_field.transform.localPosition.z);

            GameObject gameObject = uGUI_Control.AddItem(tabIndex, InputOption);

            InputField componentInChildren = gameObject.GetComponentInChildren<InputField>();
            Utils.Assert(componentInChildren != null, "see log", null);

            componentInChildren.text = value;
            componentInChildren.GetAllComponentsInChildren<Text>()[0].text = placeholder;
            if (callback != null) {
                componentInChildren.onValueChanged.AddListener(callback);
            }
        }

        public void AddColorImage(int tabIndex, string label, Color value, uGUI_OptionsPanel uGUI_Options) {
            uGUI_MainMenu uGUI_Main = uGUI_MainMenu.main;
            uGUI_TabbedControlsPanel uGUI_Control = uGUI_Main.GetComponentInChildren<uGUI_TabbedControlsPanel>();
            GameObject ColorField = new GameObject();
            GameObject caption = Object.Instantiate(uGUI_Options.toggleOptionPrefab.transform.GetChild(0).GetChild(2).gameObject);
            GameObject image = new GameObject();

            image.AddComponent<Image>();
            caption.GetComponent<Text>().text = label;
            image.transform.SetParent(caption.transform);
            caption.transform.SetParent(ColorField.transform);

            ColorField.name = "uGUI_ColorImage";
            ColorField.AddComponent<LayoutElement>();
            ColorField.GetComponent<LayoutElement>().minHeight = 60;
            ColorField.GetComponent<LayoutElement>().preferredWidth = -1;
            ColorField.GetComponent<LayoutElement>().preferredHeight = 60;
            image.transform.localPosition = new Vector3(235, 0, image.transform.localPosition.z);
            image.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 50);
            image.GetComponent<Image>().sprite = uGUI_Options.troubleshootingPrefab.GetComponentInChildren<Image>().sprite;
            image.GetComponent<Image>().type = Image.Type.Sliced;
            image.GetComponent<Image>().color = SettingsManager.GetColor();

            GameObject gameObject = uGUI_Control.AddItem(tabIndex, ColorField);
            SettingsManager.Color_Image = gameObject.GetComponentInChildren<Image>();
        }

        public void AddKeybindOption(int tabIndex, string label, string value, uGUI_OptionsPanel uGUI_Options, UnityAction<string> callback = null) {
            uGUI_MainMenu uGUI_Main = uGUI_MainMenu.main;
            uGUI_TabbedControlsPanel uGUI_Control = uGUI_Main.GetComponentInChildren<uGUI_TabbedControlsPanel>();
            GameObject bindingOption = Object.Instantiate(uGUI_Control.bindingOptionPrefab);

            foreach (uGUI_Binding item in bindingOption.GetComponentsInChildren<uGUI_Binding>()) {
                GameObject obj = item.gameObject;
                Object.DestroyImmediate(item);

                if (obj.name == "Secondary Binding") {
                    obj.GetComponent<Image>().enabled = false;
                    obj.transform.GetChild(0).gameObject.SetActive(false);
                } else {
                    obj.AddComponent<uGUI_Binding_Nitrox>();
                }
            }

            Object.DestroyImmediate(bindingOption.GetComponentInChildren<uGUI_Bindings>());

            bindingOption.name = "uGUI_BindingOptionOption";
            bindingOption.GetComponentInChildren<Text>().text = label;
            bindingOption.transform.GetChild(2).localPosition = new Vector3(25, -23.8f, 0);

            GameObject gameObject = uGUI_Control.AddItem(tabIndex, bindingOption);

            uGUI_Binding_Nitrox componentInChildren = gameObject.GetComponentInChildren<uGUI_Binding_Nitrox>();
            componentInChildren.currentText = componentInChildren.gameObject.RequireComponentInChildren<Text>();
            componentInChildren.device = GameInput.Device.Keyboard;
            componentInChildren.value = value;

            if (callback != null) {
                componentInChildren.onValueChanged.AddListener(callback);
            }
        }

    }
}
