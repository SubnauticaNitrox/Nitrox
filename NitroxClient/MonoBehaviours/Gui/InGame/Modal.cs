using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UWE;

namespace NitroxClient.MonoBehaviours.Gui.InGame;

/// <summary>
/// Open a dialog box on the middle of the screen at any time.
/// The components also need to be added in Multiplayer::InitMonoBehaviours()
/// </summary>
public abstract class Modal : MonoBehaviour
{
    /// <summary>
    /// Get a Modal by its type at any time (static)
    /// </summary>
    public static Dictionary<Type, Modal> Modals = new();
    /// <summary>
    /// Current modal that is visible on the screen
    /// </summary>
    public static Modal CurrentModal;

    private GameObject modalSubWindow;
    private Text text;

    // All the properties that will be overriden by new instances that inherit this class
    public string SubWindowName { get; init; }
    public string ModalText { get; set; }

    /// <summary>
    /// Makes it possible to dismiss the modal by clicking outside of the modal or pressing escape (default false).
    /// </summary>
    public bool IsAvoidable { get; init; }
    public bool HideNoButton { get; init; }

    public string YesButtonText { get; init; }
    public string NoButtonText { get; init; }

    public bool FreezeGame { get; init; }

    // Is useful for calling IngameMenu::OnDeselect() from a modal class (in Hide() for example)
    public bool IsAvoidableBypass = false;

    public Modal(string yesButtonText = "YES", bool hideNoButton = true, string noButtonText = "NO", string modalText = "", bool isAvoidable = false, bool freezeGame = false)
    {
        Type type = GetType();
        if (Modals.ContainsKey(type))
        {
            throw new NotSupportedException($"You cannot set two modals to have the same Type");
        }

        SubWindowName = GetType().Name;
        YesButtonText = yesButtonText;
        HideNoButton = hideNoButton;
        NoButtonText = noButtonText;
        ModalText = modalText;
        IsAvoidable = isAvoidable;
        FreezeGame = freezeGame;

        Log.Debug($"Registered Modal {SubWindowName} of type {type}");
        Modals.Add(type, this);
    }

    /// <summary>
    /// Adds the Modal to the screen
    /// </summary>
    public void Show()
    {
        if (FreezeGame)
        {
            FreezeTime.Begin($"Nitrox{SubWindowName}Freeze");
        }
        CurrentModal?.Hide();
        CurrentModal = this;
        StartCoroutine(Show_Impl());
    }

    /// <summary>
    /// Removes the Modal from the screen
    /// </summary>
    public void Hide()
    {
        CurrentModal = null;
        if (FreezeGame)
        {
            FreezeTime.End($"Nitrox{SubWindowName}Freeze");
        }
        if (IsAvoidable)
        {
            IngameMenu.main.OnDeselect();
        }
        else
        {
            IsAvoidableBypass = true;
            IngameMenu.main.OnDeselect();
            IsAvoidableBypass = false;
        }
    }

    /// <summary>
    /// This creates the modal when showing it for the first time, you can't modify it afterwards
    /// </summary>
    private void InitSubWindow()
    {
        if (!IngameMenu.main)
        {
            throw new NotSupportedException($"Cannot show ingame subwindow {SubWindowName} because the ingame window does not exist.");
        }

        if (!modalSubWindow)
        {
            GameObject derivedSubWindow = IngameMenu.main.transform.Find("QuitConfirmation").gameObject;
            modalSubWindow = Instantiate(derivedSubWindow, IngameMenu.main.transform, false);
            modalSubWindow.name = SubWindowName;

            // Styling.
            RectTransform main = modalSubWindow.GetComponent<RectTransform>();
            main.sizeDelta = new Vector2(700, 195);

            RectTransform messageTransform = modalSubWindow.FindChild("Header").GetComponent<RectTransform>();
            messageTransform.sizeDelta = new Vector2(700, 195);
         }

        // Will happen either it's initialized or not
        UpdateModal();
    }

    /// <summary>
    /// Update the modal with informations that may change from one Show() to another
    /// </summary>
    private void UpdateModal()
    {
        text = modalSubWindow.FindChild("Header").GetComponent<Text>();
        text.text = ModalText;

        GameObject buttonYesObject = modalSubWindow.FindChild("ButtonYes");
        GameObject buttonNoObject = modalSubWindow.FindChild("ButtonNo");
        Button yesButton = buttonYesObject.GetComponent<Button>();

        // We need to reinitialize onClick to avoid keeping Persisted Events (which are set manually inside Unity's Editor)
        yesButton.onClick = new Button.ButtonClickedEvent();
        yesButton.onClick.AddListener(() => { ClickYes(); });
        buttonYesObject.GetComponentInChildren<Text>().text = YesButtonText;

        if (HideNoButton)
        {
            Destroy(buttonNoObject);
            buttonYesObject.transform.position = new Vector3(modalSubWindow.transform.position.x / 2, buttonYesObject.transform.position.y, buttonYesObject.transform.position.z); // Center Button
            return;
        }

        if (buttonNoObject)
        {
            Button noButton = buttonNoObject.GetComponent<Button>();
            noButton.onClick = new Button.ButtonClickedEvent();
            noButton.onClick.AddListener(() => { ClickNo(); });
            buttonNoObject.GetComponentInChildren<Text>().text = NoButtonText;
        }
    }

    public virtual void ClickYes() { }
    public virtual void ClickNo() { }

    private IEnumerator Show_Impl()
    {
        // Execute frame-by-frame to allow UI scripts to initialize.
        InitSubWindow();
        yield return new WaitForEndOfFrame();
        IngameMenu.main.Open();
        yield return new WaitForEndOfFrame();
        IngameMenu.main.ChangeSubscreen(SubWindowName);
    }

    /// <summary>
    /// Lets you get any existing Modal by its Type
    /// </summary>
    /// <typeparam name="T">The type of the modal to get</typeparam>
    /// <returns>An existing instance of the modal if it already exists, else, a new one</returns>
    public static T Get<T>() where T : Modal
    {
        if (Modals.TryGetValue(typeof(T), out Modal modal))
        {
            return (T)modal;
        }
        // No need to add entry in dictionary as it's done in constructor
        return Multiplayer.Main.gameObject.AddComponent<T>();
    }
}
