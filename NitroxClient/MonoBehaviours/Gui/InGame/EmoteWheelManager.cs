using System.Collections.Generic;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.ChatUI;
using NitroxClient.GameLogic.Emotes;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.InGame;

public sealed class EmoteWheelManager : MonoBehaviour
{
    private const float MOUSE_DEAD_ZONE = 48f;
    private const float VIRTUAL_POINTER_RADIUS = 160f;
    private const float GAMEPAD_DEAD_ZONE = 0.35f;
    private const float LABEL_RADIUS = 192f;
    private const float WHEEL_SIZE = 560f;
    private const float OPEN_DURATION = 0.1f;
    private const float CLOSE_DURATION = 0.08f;
    private const float CLOSED_SCALE = 0.96f;

    private static readonly Color32 primaryText = new(218, 247, 255, 255);
    private static readonly Color32 secondaryText = new(142, 207, 219, 255);
    private static readonly Color32 recentText = new(65, 217, 255, 255);
    private static readonly Color32 selectedText = new(255, 190, 112, 255);

    private static EmoteWheelManager instance;

    private readonly EmoteWheelInputState inputState = new();
    private readonly List<TextMeshProUGUI> segmentLabels = new(PlayerEmoteCatalog.OrderedDefinitions.Count);

    private CanvasGroup canvasGroup;
    private EmoteWheelGraphic graphic;
    private TextMeshProUGUI centerLabel;
    private TextMeshProUGUI recentLabel;
    private RectTransform rootTransform;
    private RectTransform wheelTransform;
    private PlayerYells playerYells;
    private Vector2 selectionVector;
    private bool targetVisible;
    private bool usingGamepad;
    private float visibility;

    public static bool IsOpen => instance && instance.inputState.IsOpen;

    public static void BeginHold()
    {
        if (instance)
        {
            instance.BeginHoldInternal();
        }
    }

    public static void EndHold()
    {
        if (instance)
        {
            instance.EndHoldInternal();
        }
    }

    private void Awake()
    {
        if (instance)
        {
            Log.Error($"Tried to initialize a second {nameof(EmoteWheelManager)}");
            Destroy(this);
            return;
        }

        instance = this;
        playerYells = this.Resolve<PlayerYells>();
        Multiplayer.OnAfterMultiplayerEnd += OnMultiplayerEnded;
        EnsureView();
    }

    private void Update()
    {
        UpdateAnimation();

        if (!inputState.IsArmed && !inputState.IsOpen)
        {
            return;
        }

        if (!CanUseEmotes())
        {
            CancelInteraction();
            return;
        }

        if (inputState.IsArmed)
        {
            if (!inputState.TryOpen(Time.unscaledTime))
            {
                return;
            }

            if (!EnsureView())
            {
                Log.ErrorOnce($"[{nameof(EmoteWheelManager)}] Could not create the emote wheel UI");
                inputState.Cancel();
                return;
            }

            ShowWheel();
            return;
        }

        UpdateSelection();
    }

    private void OnDestroy()
    {
        Multiplayer.OnAfterMultiplayerEnd -= OnMultiplayerEnded;
        if (rootTransform)
        {
            Destroy(rootTransform.gameObject);
        }
        if (instance == this)
        {
            instance = null;
        }
    }

    private void BeginHoldInternal()
    {
        if (!CanUseEmotes())
        {
            return;
        }

        inputState.Begin(Time.unscaledTime);
    }

    private void EndHoldInternal()
    {
        EmoteWheelRelease release = inputState.Release(Time.unscaledTime);
        if (targetVisible)
        {
            HideWheel();
        }

        if (release.Kind != EmoteWheelReleaseKind.None && !CanUseEmotes())
        {
            return;
        }

        switch (release.Kind)
        {
            case EmoteWheelReleaseKind.PlayRecent:
                playerYells.TryYellRecent();
                break;
            case EmoteWheelReleaseKind.PlaySelected:
                playerYells.TryYell(release.SelectedGroup);
                break;
        }
    }

    private bool CanUseEmotes()
    {
        if (!Multiplayer.Joined || !playerYells.CanYell() || !Player.main || Player.main.cinematicModeActive)
        {
            return false;
        }

        if (!AvatarInputHandler.main || !AvatarInputHandler.main.IsEnabled() || PlayerChatManager.Instance.IsChatSelected)
        {
            return false;
        }

        return FPSInputModule.current == null || FPSInputModule.current.lastGroup == null;
    }

    private void CancelInteraction()
    {
        inputState.Cancel();
        if (targetVisible)
        {
            HideWheel();
        }
    }

    private void OnMultiplayerEnded()
    {
        CancelInteraction();
        if (rootTransform)
        {
            Destroy(rootTransform.gameObject);
            rootTransform = null;
            canvasGroup = null;
            wheelTransform = null;
            graphic = null;
            centerLabel = null;
            recentLabel = null;
            segmentLabels.Clear();
        }
    }

    private void ShowWheel()
    {
        selectionVector = Vector2.zero;
        usingGamepad = false;
        targetVisible = true;
        rootTransform.gameObject.SetActive(true);
        rootTransform.SetAsLastSibling();
        RefreshSelection(-1, 90f);
    }

    private void HideWheel()
    {
        targetVisible = false;
    }

    private void UpdateSelection()
    {
        Vector2 mouseDelta = Mouse.current != null ? Mouse.current.delta.ReadValue() : Vector2.zero;
        Vector2 rightStick = Gamepad.current != null ? Gamepad.current.rightStick.ReadValue() : Vector2.zero;

        if (mouseDelta.sqrMagnitude > 0.01f)
        {
            usingGamepad = false;
            selectionVector = Vector2.ClampMagnitude(selectionVector + mouseDelta, VIRTUAL_POINTER_RADIUS);
        }
        else if (usingGamepad || rightStick.sqrMagnitude >= GAMEPAD_DEAD_ZONE * GAMEPAD_DEAD_ZONE)
        {
            usingGamepad = true;
            selectionVector = rightStick * VIRTUAL_POINTER_RADIUS;
        }

        float deadZone = usingGamepad ? GAMEPAD_DEAD_ZONE * VIRTUAL_POINTER_RADIUS : MOUSE_DEAD_ZONE;
        if (selectionVector.magnitude < deadZone)
        {
            inputState.SetSelection(null);
            RefreshSelection(-1, 90f);
            return;
        }

        float angle = Mathf.Atan2(selectionVector.y, selectionVector.x) * Mathf.Rad2Deg;
        float clockwiseFromTop = Mathf.Repeat(90f - angle, 360f);
        int selectedIndex = Mathf.FloorToInt((clockwiseFromTop + 20f) / 40f) % PlayerEmoteCatalog.OrderedDefinitions.Count;
        inputState.SetSelection(PlayerEmoteCatalog.OrderedDefinitions[selectedIndex].Group);
        RefreshSelection(selectedIndex, angle);
    }

    private void RefreshSelection(int selectedIndex, float selectionAngle)
    {
        int recentIndex = GetDefinitionIndex(playerYells.RecentGroup);
        graphic.SetState(selectedIndex, recentIndex, selectionAngle);

        for (int index = 0; index < segmentLabels.Count; index++)
        {
            TextMeshProUGUI label = segmentLabels[index];
            bool selected = index == selectedIndex;
            label.color = selected ? selectedText : index == recentIndex ? recentText : primaryText;
            label.rectTransform.localScale = Vector3.one * (selected ? 1.06f : 1f);
        }

        centerLabel.text = selectedIndex >= 0
                               ? GetDisplayName(PlayerEmoteCatalog.OrderedDefinitions[selectedIndex])
                               : Language.main.Get("Nitrox_EmoteWheel_Cancel");

        string recentFormat = Language.main.Get("Nitrox_EmoteWheel_Last");
        recentLabel.text = recentFormat.Replace("{EMOTE}", GetDisplayName(PlayerEmoteCatalog.Get(playerYells.RecentGroup)));
    }

    private void UpdateAnimation()
    {
        if (!canvasGroup || !wheelTransform)
        {
            return;
        }

        float duration = targetVisible ? OPEN_DURATION : CLOSE_DURATION;
        visibility = Mathf.MoveTowards(visibility, targetVisible ? 1f : 0f, Time.unscaledDeltaTime / duration);
        float easedVisibility = 1f - Mathf.Pow(1f - visibility, 3f);
        canvasGroup.alpha = easedVisibility;
        wheelTransform.localScale = Vector3.one * Mathf.Lerp(CLOSED_SCALE, 1f, easedVisibility);

        if (!targetVisible && visibility <= 0f && rootTransform.gameObject.activeSelf)
        {
            rootTransform.gameObject.SetActive(false);
        }
    }

    private bool EnsureView()
    {
        if (rootTransform)
        {
            return true;
        }
        if (!uGUI.main || !uGUI.main.screenCanvas)
        {
            return false;
        }

        BuildView(uGUI.main.screenCanvas.transform);
        return true;
    }

    private void BuildView(Transform canvas)
    {
        GameObject root = new("NitroxEmoteWheel", typeof(RectTransform), typeof(CanvasGroup));
        rootTransform = root.GetComponent<RectTransform>();
        rootTransform.SetParent(canvas, false);
        Stretch(rootTransform);
        canvasGroup = root.GetComponent<CanvasGroup>();
        canvasGroup.alpha = 0f;
        canvasGroup.blocksRaycasts = false;
        canvasGroup.interactable = false;

        GameObject glassObject = new("AbyssGlass", typeof(RectTransform), typeof(Image));
        RectTransform glassTransform = glassObject.GetComponent<RectTransform>();
        glassTransform.SetParent(rootTransform, false);
        Stretch(glassTransform);
        Image glass = glassObject.GetComponent<Image>();
        glass.color = new Color32(2, 10, 17, 92);
        glass.raycastTarget = false;

        GameObject wheel = new("CommsSonar", typeof(RectTransform), typeof(EmoteWheelGraphic));
        wheelTransform = wheel.GetComponent<RectTransform>();
        wheelTransform.SetParent(rootTransform, false);
        wheelTransform.anchorMin = wheelTransform.anchorMax = new Vector2(0.5f, 0.5f);
        wheelTransform.sizeDelta = new Vector2(WHEEL_SIZE, WHEEL_SIZE);
        wheelTransform.anchoredPosition = Vector2.zero;
        graphic = wheel.GetComponent<EmoteWheelGraphic>();
        graphic.raycastTarget = false;

        TextMeshProUGUI title = AddText(rootTransform, "Title", Language.main.Get("Nitrox_EmoteWheel_Title"), new Vector2(0f, 324f), new Vector2(240f, 28f), 12f, FontStyles.Bold);
        title.color = secondaryText;
        title.characterSpacing = 4f;

        for (int index = 0; index < PlayerEmoteCatalog.OrderedDefinitions.Count; index++)
        {
            PlayerEmoteDefinition definition = PlayerEmoteCatalog.OrderedDefinitions[index];
            float angle = (90f - index * 40f) * Mathf.Deg2Rad;
            Vector2 labelPosition = new(Mathf.Cos(angle) * LABEL_RADIUS, Mathf.Sin(angle) * LABEL_RADIUS);
            TextMeshProUGUI label = AddText(wheelTransform, definition.Group.ToString(), GetDisplayName(definition), labelPosition, new Vector2(136f, 50f), 17f, FontStyles.Bold);
            label.color = primaryText;
            segmentLabels.Add(label);
        }

        centerLabel = AddText(wheelTransform, "Selection", Language.main.Get("Nitrox_EmoteWheel_Cancel"), new Vector2(0f, 10f), new Vector2(190f, 42f), 24f, FontStyles.Bold);
        centerLabel.color = primaryText;
        recentLabel = AddText(wheelTransform, "Recent", string.Empty, new Vector2(0f, -28f), new Vector2(190f, 24f), 11f, FontStyles.Bold);
        recentLabel.color = recentText;
        recentLabel.characterSpacing = 1.5f;

        TextMeshProUGUI hint = AddText(rootTransform, "Hint", Language.main.Get("Nitrox_EmoteWheel_Hint"), new Vector2(0f, -330f), new Vector2(620f, 30f), 14f, FontStyles.Normal);
        hint.color = secondaryText;

        visibility = 0f;
        targetVisible = false;
        wheelTransform.localScale = Vector3.one * CLOSED_SCALE;
        root.SetActive(false);
    }

    private static TextMeshProUGUI AddText(
        Transform parent,
        string name,
        string value,
        Vector2 position,
        Vector2 size,
        float fontSize,
        FontStyles style)
    {
        GameObject textObject = new(name, typeof(RectTransform), typeof(TextMeshProUGUI));
        RectTransform rectTransform = textObject.GetComponent<RectTransform>();
        rectTransform.SetParent(parent, false);
        rectTransform.anchorMin = rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
        rectTransform.anchoredPosition = position;
        rectTransform.sizeDelta = size;

        TextMeshProUGUI text = textObject.GetComponent<TextMeshProUGUI>();
        text.text = value;
        text.fontSize = fontSize;
        text.fontStyle = style;
        text.alignment = TextAlignmentOptions.Center;
        text.enableWordWrapping = true;
        text.overflowMode = TextOverflowModes.Overflow;
        text.raycastTarget = false;
        return text;
    }

    private static void Stretch(RectTransform rectTransform)
    {
        rectTransform.anchorMin = Vector2.zero;
        rectTransform.anchorMax = Vector2.one;
        rectTransform.offsetMin = Vector2.zero;
        rectTransform.offsetMax = Vector2.zero;
    }

    private static string GetDisplayName(PlayerEmoteDefinition definition) => Language.main.Get(definition.LanguageKey);

    private static int GetDefinitionIndex(PlayerEmoteGroup group)
    {
        for (int index = 0; index < PlayerEmoteCatalog.OrderedDefinitions.Count; index++)
        {
            if (PlayerEmoteCatalog.OrderedDefinitions[index].Group == group)
            {
                return index;
            }
        }
        return 0;
    }
}
