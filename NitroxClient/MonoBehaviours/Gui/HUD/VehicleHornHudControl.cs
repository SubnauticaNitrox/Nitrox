using NitroxClient.GameLogic;
using NitroxClient.MonoBehaviours.Gui.Input;
using NitroxClient.MonoBehaviours.Gui.Input.KeyBindings.Actions;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.HUD;

[DisallowMultipleComponent]
public sealed class VehicleHornHudControl : MonoBehaviour
{
    private static readonly Color BackgroundColor = new(0.055f, 0.38f, 0.48f, 0.88f);
    private static readonly Color AccentColor = new(0.227f, 0.949f, 0.969f, 1f);
    private static readonly Color TextColor = new(0.82f, 0.98f, 1f, 1f);
    private static readonly GameInput.Button HornButton = KeyBindingManager.GetButton<VehicleHornKeyBindingAction>();

    private static VehicleHornHudControl instance;

    private CanvasGroup canvasGroup;
    private GameObject control;
    private TextMeshProUGUI label;
    private string lastLabelText;
    private VehicleHorns vehicleHorns;

    public static void EnsureAttached(uGUI_SeamothHUD seamothHud)
    {
        if (instance || !uGUI.main || !uGUI.main.quickSlots || !seamothHud.textPower)
        {
            return;
        }

        instance = uGUI.main.quickSlots.gameObject.AddComponent<VehicleHornHudControl>();
        instance.Initialize(uGUI.main.quickSlots, seamothHud.textPower);
    }

    private void Initialize(uGUI_QuickSlots quickSlots, TextMeshProUGUI styleSource)
    {
        vehicleHorns = this.Resolve<VehicleHorns>();

        control = new GameObject("NitroxVehicleHornControl", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(CanvasGroup));
        control.transform.SetParent(quickSlots.rectTransform, false);

        RectTransform controlRect = (RectTransform)control.transform;
        controlRect.anchorMin = new Vector2(1f, 0.5f);
        controlRect.anchorMax = new Vector2(1f, 0.5f);
        controlRect.pivot = new Vector2(0f, 0.5f);
        controlRect.anchoredPosition = new Vector2(18f, 0f);
        controlRect.sizeDelta = new Vector2(150f, 44f);

        Image background = control.GetComponent<Image>();
        background.sprite = quickSlots.spriteCenter ? quickSlots.spriteCenter : quickSlots.spriteNormal;
        background.material = quickSlots.materialBackground;
        background.type = Image.Type.Sliced;
        background.color = BackgroundColor;
        background.raycastTarget = false;

        canvasGroup = control.GetComponent<CanvasGroup>();
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        GameObject accent = new("Accent", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
        accent.transform.SetParent(controlRect, false);
        RectTransform accentRect = (RectTransform)accent.transform;
        accentRect.anchorMin = Vector2.zero;
        accentRect.anchorMax = new Vector2(0f, 1f);
        accentRect.pivot = new Vector2(0f, 0.5f);
        accentRect.anchoredPosition = Vector2.zero;
        accentRect.sizeDelta = new Vector2(4f, 0f);
        Image accentImage = accent.GetComponent<Image>();
        accentImage.color = AccentColor;
        accentImage.raycastTarget = false;

        GameObject labelObject = new("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(TextMeshProUGUI));
        labelObject.transform.SetParent(controlRect, false);
        RectTransform labelRect = (RectTransform)labelObject.transform;
        labelRect.anchorMin = Vector2.zero;
        labelRect.anchorMax = Vector2.one;
        labelRect.offsetMin = new Vector2(12f, 2f);
        labelRect.offsetMax = new Vector2(-6f, -2f);

        label = labelObject.GetComponent<TextMeshProUGUI>();
        label.font = styleSource.font;
        label.fontSize = 15f;
        label.fontStyle = FontStyles.Bold;
        label.alignment = TextAlignmentOptions.Center;
        label.color = TextColor;
        label.enableWordWrapping = false;
        label.overflowMode = TextOverflowModes.Ellipsis;
        label.raycastTarget = false;

        control.SetActive(false);
    }

    private void Update()
    {
        if (!control || vehicleHorns == null)
        {
            return;
        }

        bool visible = VehicleHorns.TryGetPilotedVehicle(out _) && !IsPdaOpen();
        if (control.activeSelf != visible)
        {
            control.SetActive(visible);
        }

        if (!visible)
        {
            return;
        }

        canvasGroup.alpha = Mathf.MoveTowards(canvasGroup.alpha, vehicleHorns.IsCurrentVehicleReady() ? 1f : 0.45f, Time.unscaledDeltaTime * 5f);

        string labelText = $"{GameInput.FormatButton(HornButton)}  {Language.main.Get("CyclopsHorn")}";
        if (lastLabelText != labelText)
        {
            lastLabelText = labelText;
            label.SetText(labelText);
        }
    }

    private static bool IsPdaOpen()
    {
        PDA pda = Player.main ? Player.main.GetPDA() : null;
        return pda && pda.isInUse;
    }

    private void OnDestroy()
    {
        if (instance == this)
        {
            instance = null;
        }
    }
}
