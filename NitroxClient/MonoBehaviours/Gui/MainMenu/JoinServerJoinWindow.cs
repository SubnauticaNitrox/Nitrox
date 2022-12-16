using System;
using System.Collections;
using System.Threading.Tasks;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.ColorSwap.Strategy;
using NitroxClient.GameLogic.Settings;
using NitroxClient.Unity.Helper;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu;

public class JoinServerJoinWindow
{
    private readonly TaskCompletionSource<bool> isInitialised = new(false);

    private GameObject playerSettingsPanel;
    private uGUI_ColorPicker colorPicker;
    private TextMeshProUGUI lowerDetailText;
    private uGUI_InputField playerNameInputField;

    private bool isColorPickerSubscribed;

    public string PlayerName
    {
        get => playerNameInputField.text;
        set => playerNameInputField.text = value;
    }

    public void ActivateInputField() => playerNameInputField.ActivateInputField();

    public void SetIP(string serverIp) => lowerDetailText.text = $"{Language.main.Get("Nitrox_JoinServerIpAddress")}{Environment.NewLine}{(NitroxPrefs.HideIp.Value ? "****" : serverIp)}";

    public void SetHSB(Vector3 hsb) => colorPicker.SetHSB(hsb);

    public Color GetCurrentColor() => colorPicker.currentColor;

    public void SubscribeColorChanged()
    {
        if (isColorPickerSubscribed)
        {
            return;
        }

        colorPicker.onColorChange.AddListener(OnColorChange);

        isColorPickerSubscribed = true;
    }

    public void UnsubscribeColorChanged()
    {
        if (!playerSettingsPanel || !isColorPickerSubscribed)
        {
            return;
        }

        colorPicker.onColorChange.RemoveListener(OnColorChange);
        isColorPickerSubscribed = false;
    }

    private void OnColorChange(ColorChangeEventData eventData)
    {
        Color selectedColor = eventData.color;

        GameObject selectedColorGameObject = playerSettingsPanel.RequireGameObject("BaseTab/SelectedColor");

        Image baseTabSelectedColorImage = selectedColorGameObject.GetComponent<Image>();
        baseTabSelectedColorImage.color = selectedColor;
    }

    //This configures and re-positions the elements on the default "ColorGreyscale" menu to suite our purposes now.
    public IEnumerator Initialize(RectTransform joinServerBackground, UnityAction joinButtonCall, UnityAction cancelButtonCall)
    {
        AsyncOperationHandle<GameObject> request = AddressablesUtility.LoadAsync<GameObject>("Assets/Prefabs/Base/GeneratorPieces/BaseMoonpoolUpgradeConsole.prefab");
        yield return request;
        GameObject colorPickerPanelPrototype = request.Result.RequireGameObject("EditScreen/Active");

        InstantiateColorPickerPanelPrototype(colorPickerPanelPrototype, out playerSettingsPanel);
        InitializePlayerSettingsPanelElement(playerSettingsPanel, joinServerBackground);
        InitializeBaseTabElement(playerSettingsPanel, joinServerBackground);
        InitializeLowerDetailElement(playerSettingsPanel, out lowerDetailText);
        InitializePlayerNameInputElement(playerSettingsPanel, out playerNameInputField);
        InitializeColorPickerComponent(playerSettingsPanel, out colorPicker);
        InitializeColorPickerElement(playerSettingsPanel);
        InitializeButtonElements(playerSettingsPanel, joinServerBackground, joinButtonCall, cancelButtonCall);
        
        isInitialised.SetResult(true);
    }

    public Task IsReady() => isInitialised.Task;

    //Join and Cancel buttons
    private static void InitializeButtonElements(GameObject playerSettingsPanel, RectTransform joinServerBackground, UnityAction joinButtonCall, UnityAction cancelButtonCall)
    {
        GameObject cancelButtonGameObject = playerSettingsPanel.RequireGameObject("Button");
        GameObject joinButtonGameObject = UnityEngine.Object.Instantiate(cancelButtonGameObject, playerSettingsPanel.transform, false);

        //Click events
        Button joinButton = joinButtonGameObject.GetComponent<Button>();
        joinButton.onClick.AddListener(joinButtonCall);

        Button cancelButton = cancelButtonGameObject.GetComponent<Button>();
        cancelButton.onClick.AddListener(cancelButtonCall);

        RectTransform joinButtonTransform = (RectTransform)joinButtonGameObject.transform;
        joinButtonTransform.anchoredPosition = new Vector2(
            joinServerBackground.rect.width / 2f - joinButtonTransform.rect.width / 2f + 20f,
            -1f * (joinServerBackground.rect.height / 2f) + joinButtonTransform.rect.height / 2f + 3f);

        RectTransform cancelButtonTransform = (RectTransform)cancelButtonGameObject.transform;
        GameObject cancelButtonTextGameObject = cancelButtonTransform.RequireGameObject("Text");
        cancelButtonTextGameObject.GetComponent<TextMeshProUGUI>().text = Language.main.Get("Nitrox_Cancel");

        cancelButtonTransform.sizeDelta = new Vector2(cancelButtonTransform.rect.width * 0.85f, cancelButtonTransform.rect.height);
        cancelButtonTransform.anchoredPosition = new Vector2(
            -1f * joinServerBackground.rect.width / 2f + cancelButtonTransform.rect.width / 2f,
            -1f * (joinServerBackground.rect.height / 2f) + cancelButtonTransform.rect.height / 2f + 3f);

        //Flip the button over
        joinButtonTransform.sizeDelta = new Vector2(joinButtonTransform.rect.width * 0.85f, joinButtonTransform.rect.height);
        joinButtonTransform.Rotate(Vector3.forward * -180);

        GameObject joinButtonTextGameObject = joinButtonTransform.RequireGameObject("Text");
        joinButtonTextGameObject.GetComponent<TextMeshProUGUI>().text = Language.main.Get("Nitrox_Join");

        //Flip the text so it is no longer upside down after flipping the button.
        RectTransform joinButtonTextRectTransform = (RectTransform)joinButtonTextGameObject.transform;
        joinButtonTextRectTransform.Rotate(Vector3.forward * -180);
    }

    private static void InstantiateColorPickerPanelPrototype(GameObject colorPickerPanelPrototype, out GameObject playerSettingsPanel)
    {
        //Create a clone of the RocketBase color picker panel.
        playerSettingsPanel = UnityEngine.Object.Instantiate(colorPickerPanelPrototype);
        GameObject baseTab = playerSettingsPanel.RequireGameObject("BaseTab");
        GameObject serverNameLabel = playerSettingsPanel.RequireGameObject("Name Label");
        GameObject stripe1Tab = playerSettingsPanel.RequireGameObject("Stripe1Tab");
        GameObject interiorTab = playerSettingsPanel.RequireGameObject("InteriorTab");
        GameObject nameTab = playerSettingsPanel.RequireGameObject("NameTab");
        GameObject colorLabel = playerSettingsPanel.RequireGameObject("Color Label");

        //Enables pointer events that are a required for the uGUI_ColorPicker to work.
        CanvasGroup colorPickerCanvasGroup = playerSettingsPanel.AddComponent<CanvasGroup>();
        colorPickerCanvasGroup.blocksRaycasts = true;
        colorPickerCanvasGroup.ignoreParentGroups = true;
        colorPickerCanvasGroup.interactable = true;

        //Destroy everything that we know we will not be using.
        UnityEngine.Object.Destroy(playerSettingsPanel.GetComponent<uGUI_NavigableControlGrid>());
        UnityEngine.Object.Destroy(playerSettingsPanel.GetComponent<Image>());
        UnityEngine.Object.Destroy(baseTab.GetComponent<Button>());
        UnityEngine.Object.Destroy(stripe1Tab);
        UnityEngine.Object.Destroy(interiorTab);
        UnityEngine.Object.Destroy(nameTab);
        UnityEngine.Object.Destroy(colorLabel);
        UnityEngine.Object.Destroy(serverNameLabel);
    }

    //This panel acts as the parent of all other UI elements on the menu. It is parented by the cloned "SaveGame" menu.
    private static void InitializePlayerSettingsPanelElement(GameObject playerSettingsPanel, RectTransform joinServerBackground)
    {
        playerSettingsPanel.SetActive(true);

        RectTransform playerSettingsPanelTransform = (RectTransform)playerSettingsPanel.transform;
        playerSettingsPanelTransform.SetParent(joinServerBackground, false);
        playerSettingsPanelTransform.anchorMin = new Vector2(0f, 0f);
        playerSettingsPanelTransform.anchorMax = new Vector2(1f, 1f);
        playerSettingsPanelTransform.pivot = new Vector2(0.5f, 0.5f);
        playerSettingsPanelTransform.localScale = Vector3.one;
    }

    //The base tab is the outline surrounding the color picker, as well as teh "Player Color" label and associated "Selected Color" image.
    private static void InitializeBaseTabElement(GameObject playerSettingsPanel, RectTransform joinServerBackground)
    {
        GameObject baseTab = playerSettingsPanel.RequireGameObject("BaseTab");

        //Re-position the border
        RectTransform baseTabTransform = (RectTransform)baseTab.transform;
        baseTabTransform.anchoredPosition = new Vector2(0.5f, 0.5f);
        baseTabTransform.anchorMin = new Vector2(0.5f, 0.5f);
        baseTabTransform.anchorMax = new Vector2(0.5f, 0.5f);
        baseTabTransform.pivot = new Vector2(0.5f, 0.5f);

        //Resize the border to match the new parent. Capture the original width so we can make adjustments to child controls later
        //because the existing elements all have some really weird anchor settings the prevent them from moving automatically.
        float originalBaseTabWidth = baseTabTransform.rect.width;
        baseTabTransform.sizeDelta = new Vector2(joinServerBackground.rect.width, baseTabTransform.rect.height);

        //Move the SelectedColor element over to the right enough to match the shrinkage of the base tab.
        GameObject baseTabSelectedColor = baseTabTransform.RequireGameObject("SelectedColor");
        Image baseTabSelectedColorImage = baseTabSelectedColor.GetComponent<Image>();
        baseTabSelectedColorImage.rectTransform.anchoredPosition += new Vector2((originalBaseTabWidth - baseTabTransform.rect.width) / 2, 0);

        //Place the "Player Color" label to the right of the SelectedColor image and shrink it to fit the new tab region.
        GameObject baseTabTextGameObject = baseTabTransform.RequireGameObject("Text");
        RectTransform baseTabTextTransform = (RectTransform)baseTabTextGameObject.transform;
        baseTabTextTransform.anchorMin = new Vector2(0.5f, 0.5f);
        baseTabTextTransform.anchorMax = new Vector2(0.5f, 0.5f);
        baseTabTextTransform.pivot = new Vector2(0.5f, 0.5f);
        baseTabTextTransform.sizeDelta = new Vector2(80, 35);

        baseTabTextTransform.anchoredPosition += new Vector2(baseTabTextTransform.rect.width / 2f + 22f, 0);

        baseTabTextGameObject.GetComponent<TextMeshProUGUI>().text = Language.main.Get("Nitrox_PlayerColor");

        //This resizes the actual Image that outlines all of the UI elements.
        GameObject baseTabBackgroundGameObject = baseTabTransform.RequireGameObject("Background");
        Image baseTabBackgroundImage = baseTabBackgroundGameObject.GetComponent<Image>();
        baseTabBackgroundImage.rectTransform.sizeDelta = new Vector2(joinServerBackground.rect.width, baseTabTransform.rect.height);

        //This removes the extra "tabs" from the base texture.
        Texture2D newBaseTabTexture = baseTabBackgroundImage.sprite.texture.Clone();
        TextureBlock textureBlock = new TextureBlock(3, 3, 160, (int)(baseTabBackgroundImage.sprite.textureRect.height - 71f));
        IColorSwapStrategy alphaChannelSwapper = new AlphaChannelSwapper(0f);
        HsvSwapper baseTabBackgroundSwapper = new HsvSwapper(alphaChannelSwapper);
        baseTabBackgroundSwapper.SetHueRange(185f, 215f);
        baseTabBackgroundSwapper.SetAlphaRange(0f, 175f);
        newBaseTabTexture.SwapTextureColors(baseTabBackgroundSwapper, textureBlock);
        baseTabBackgroundImage.sprite = Sprite.Create(newBaseTabTexture, new Rect(baseTabBackgroundImage.sprite.textureRect), new Vector2(0f, 0f));
    }

    //The LowerDetail is the region that displays the current Server IP and the graphic that appears beneath it.
    private static void InitializeLowerDetailElement(GameObject playerSettingsPanel, out TextMeshProUGUI lowerDetailText)
    {
        GameObject lowerDetail = playerSettingsPanel.RequireGameObject("LowerDetail");

        //We use this as a reference point for positioning the LowerDetail element.
        RectTransform baseTabTextTransform = (RectTransform)playerSettingsPanel.RequireTransform("BaseTab/Text");

        RectTransform lowerDetailRectTransform = (RectTransform)lowerDetail.transform;
        lowerDetailRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        lowerDetailRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        lowerDetailRectTransform.pivot = new Vector2(0.5f, 0.5f);
        lowerDetailRectTransform.anchoredPosition = baseTabTextTransform.anchoredPosition - new Vector2(230f, -120f);

        //The text element is right-aligned by default and needs to be centered for our purposes
        GameObject lowerDetailTextGameObject = lowerDetailRectTransform.RequireGameObject("Text");
        lowerDetailText = lowerDetailTextGameObject.GetComponent<TextMeshProUGUI>();
        lowerDetailText.autoSizeTextContainer = true;
        lowerDetailText.alignment = TextAlignmentOptions.Center;

        RectTransform lowerDetailTextRectTransform = (RectTransform)lowerDetailTextGameObject.transform;
        lowerDetailTextRectTransform.anchorMin = new Vector2(0.5f, 0.5f);
        lowerDetailTextRectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        lowerDetailTextRectTransform.pivot = new Vector2(0.5f, 0.5f);
        lowerDetailTextRectTransform.anchoredPosition = new Vector2(0, 0);

        //Delete the pixels under the IP
        UnityEngine.Object.Destroy(lowerDetail.RequireGameObject("Pixels"));
    }

    //Player name text box
    private static void InitializePlayerNameInputElement(GameObject playerSettingsPanel, out uGUI_InputField playerNameInputField)
    {
        GameObject playerNameInputFieldGameObject = playerSettingsPanel.RequireGameObject("InputField");
        RectTransform inputFieldRectTransform = (RectTransform)playerNameInputFieldGameObject.transform;
        inputFieldRectTransform.anchoredPosition -= new Vector2(0, 15f);

        playerNameInputField = playerNameInputFieldGameObject.GetComponent<uGUI_InputField>();
        playerNameInputField.selectionColor = Color.white;

        GameObject inputFieldPlaceholder = inputFieldRectTransform.RequireGameObject("Placeholder");
        inputFieldPlaceholder.GetComponent<TextMeshProUGUI>().text = Language.main.Get("Nitrox_EnterName");
    }

    //This is the "service" that manages the click and drag events on the color picture RectTransform.
    private static void InitializeColorPickerComponent(GameObject playerSettingsPanel, out uGUI_ColorPicker colorPicker)
    {
        colorPicker = playerSettingsPanel.GetComponentInChildren<uGUI_ColorPicker>();
        colorPicker.onColorChange.RemoveAllListeners();

        //Don't let users apply a greyscale just yet. We have not quality tested the existing recoloring solution to know if it will behave as expected.
        colorPicker.saturationSlider.gameObject.SetActive(false);
        colorPicker.SetSaturation(1f);
    }

    //This the the actual color picker that renders on the screen.
    private static void InitializeColorPickerElement(GameObject playerSettingsPanel)
    {
        GameObject colorPickerGameObject = playerSettingsPanel.RequireGameObject("ColorPicker");
        RectTransform colorPickerGameObjectTransform = (RectTransform)colorPickerGameObject.transform;
        colorPickerGameObjectTransform.anchorMin = new Vector2(0f, 0f);
        colorPickerGameObjectTransform.anchorMax = new Vector2(1f, 1f);
        colorPickerGameObjectTransform.pivot = new Vector2(0.5f, 0.5f);
        colorPickerGameObjectTransform.anchoredPosition = new Vector2(40f, 0f);
    }
}
