using System;
using System.Collections;
using System.Linq;
using NitroxClient.Unity.Helper;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.ResourceManagement.AsyncOperations;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu;

public class MainMenuNotificationPanel : MonoBehaviour, uGUI_INavigableIconGrid, uGUI_IButtonReceiver
{
    public const string NAME = "MultiplayerNotification";

    private static MainMenuNotificationPanel instance;

    private Image loadingCircle;
    private TextMeshProUGUI text;
    private GameObject confirmObject;
    private Button confirmButton;
    private mGUI_Change_Legend_On_Select confirmButtonLegend;
    private LegendButtonData[] savedLegendData;

    private string returningMenuPanel;
    private Action continuationAction;

    public static void ShowLoading()
    {
        if (!instance)
        {
            Log.Error($"Tried to use {nameof(ShowLoading)} while {nameof(MainMenuNotificationPanel)} was not ready");
            return;
        }

        instance.confirmObject.SetActive(false);
        instance.loadingCircle.gameObject.SetActive(true);
        instance.text.text = Language.main.Get("Nitrox_Loading");
        uGUI_MainMenu.main.ShowPrimaryOptions(true);
        MainMenuRightSide.main.OpenGroup(NAME);
        instance.confirmButtonLegend.legendButtonConfiguration = [];
    }

    public static void ShowMessage(string message, string returningMenuPanel, Action continuationAction = null)
    {
        if (!instance)
        {
            Log.Error("Tried to use ShowMessage() while MainMenuJoinServerNotificationPanel was not ready");
            return;
        }

        instance.text.text = message;
        instance.returningMenuPanel = returningMenuPanel;
        instance.continuationAction = continuationAction;
        instance.confirmObject.SetActive(true);
        instance.loadingCircle.gameObject.SetActive(false);
        uGUI_MainMenu.main.ShowPrimaryOptions(true);
        MainMenuRightSide.main.OpenGroup(NAME);
        instance.confirmButtonLegend.legendButtonConfiguration = instance.savedLegendData;
    }

    public void Setup(GameObject savedGamesRef)
    {
        instance = this;
        Destroy(transform.RequireGameObject("Scroll View"));
        Destroy(GetComponentInChildren<TranslationLiveUpdate>());

        text = GetComponentInChildren<TextMeshProUGUI>();
        text.horizontalAlignment = HorizontalAlignmentOptions.Center;
        text.verticalAlignment = VerticalAlignmentOptions.Top;
        text.transform.localPosition = new Vector3(-375, 350, 0);
        text.GetComponent<RectTransform>().sizeDelta = new Vector2(350, 280);

        GameObject multiplayerButtonRef = savedGamesRef.RequireGameObject("Scroll View/Viewport/SavedGameAreaContent/NewGame");

        confirmObject = Instantiate(multiplayerButtonRef, transform, false);
        confirmObject.transform.localPosition = new Vector3(-200, 50, 0);
        confirmObject.transform.localScale = new Vector3(1.25f, 1.25f, 1.25f);
        confirmObject.transform.GetChild(0).GetComponent<RectTransform>().sizeDelta = new Vector2(200, 40);
        confirmObject.GetComponentInChildren<TextMeshProUGUI>().text = Language.main.Get("Nitrox_OK");
        confirmButton = confirmObject.RequireTransform("NewGameButton").GetComponent<Button>();
        confirmButton.onClick = new Button.ButtonClickedEvent();
        confirmButton.onClick.AddListener(() =>
        {
            continuationAction?.Invoke();
            if (!string.IsNullOrEmpty(returningMenuPanel))
            {
                MainMenuRightSide.main.OpenGroup(returningMenuPanel);
            }
        });

        confirmButtonLegend = confirmButton.GetComponent<mGUI_Change_Legend_On_Select>();
        savedLegendData = confirmButtonLegend.legendButtonConfiguration.Take(1).ToArray();

        GameObject loadingCircleObject = new("LoadingCircle");
        loadingCircle = loadingCircleObject.AddComponent<Image>();
        loadingCircleObject.transform.SetParent(transform);
        loadingCircleObject.transform.localPosition = new Vector3(-200, 180, 0);
        loadingCircleObject.transform.localRotation = Quaternion.identity;
        loadingCircleObject.transform.localScale = Vector3.one;
    }

    private IEnumerator Start()
    {
        AsyncOperationHandle<Texture2D> request = AddressablesUtility.LoadAsync<Texture2D>("Assets/uGUI/Sources/Sprites/HUD/Progress.png");
        yield return request;

        Texture2D tex = request.Result;
        loadingCircle.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        loadingCircle.type = Image.Type.Filled;
    }

    private void Update()
    {
        if (loadingCircle)
        {
            loadingCircle.transform.localRotation = Quaternion.Euler(0, 0, Time.time % 360 * 250); // 250 is the speed
            loadingCircle.fillAmount = Mathf.Lerp(0.05f, 0.95f, Math.Abs(Time.time % 6 - 3) * 0.333f); // Lerps t fades from 0 to 1 and back to 0
            uGUI_LegendBar.ClearButtons();
        }
    }

    bool uGUI_INavigableIconGrid.ShowSelector => false;
    bool uGUI_INavigableIconGrid.EmulateRaycast => false;
    bool uGUI_INavigableIconGrid.SelectItemClosestToPosition(Vector3 worldPos) => false;
    uGUI_INavigableIconGrid uGUI_INavigableIconGrid.GetNavigableGridInDirection(int dirX, int dirY) => null;
    Graphic uGUI_INavigableIconGrid.GetSelectedIcon() => null;

    object uGUI_INavigableIconGrid.GetSelectedItem() => confirmObject ? confirmObject : null;

    public bool SelectItemInDirection(int dirX, int dirY) => SelectFirstItem();

    public bool SelectFirstItem()
    {
        if (confirmObject)
        {
            SelectItem(confirmObject);
            return true;
        }

        return false;
    }

    public void SelectItem(object item)
    {
        DeselectItem();
        GameObject selectedItem = item as GameObject;

        if (selectedItem && selectedItem == confirmObject)
        {
            confirmButton.Select();
            confirmButtonLegend.SyncLegendBarToGUISelection();
        }
    }

    public void DeselectItem()
    {
        if (confirmObject)
        {
            EventSystem.current.SetSelectedGameObject(null);
            confirmObject.GetComponentInChildren<uGUI_BasicColorSwap>().makeTextWhite();
        }
        uGUI_LegendBar.ClearButtons();
    }

    public bool OnButtonDown(GameInput.Button button)
    {
        switch (button)
        {
            case GameInput.Button.UISubmit:
            case GameInput.Button.UICancel:
                if (confirmObject.activeSelf)
                {
                    confirmButton.onClick.Invoke();
                }
                return true;
            default:
                return false;
        }
    }
}
