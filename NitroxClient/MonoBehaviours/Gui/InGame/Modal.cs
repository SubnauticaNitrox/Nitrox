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
    /// Get a Modal by its SubWindowName at any time (static)
    /// </summary>
    public static Dictionary<string, Modal> ModalsPerSubWindowName = new();

    private GameObject modalSubWindow;
    private Text text;

    // All the properties that will be overriden by new instances that inherit this class
    public abstract string SubWindowName { get; }
    public abstract string ModalText { get; set; }

    /// <summary>
    /// By default (true), clicking outside of the modal or pressing escape makes it possible to dismiss it
    /// </summary>
    public abstract bool IsAvoidable { get; }
    public abstract bool HideNoButton { get; }

    public abstract string YesButtonText { get; }
    public abstract string NoButtonText { get; }

    public abstract bool FreezeGame { get; }

    // Is useful for calling IngameMenu::OnDeselect() from a modal class (in Hide() for example)
    public bool IsAvoidableBypass = false;

    public Modal()
    {
        Type type = GetType();
        if (Modals.ContainsKey(type))
        {
            throw new NotSupportedException($"You cannot set two modals to have the same Type");
        }
        Log.Debug($"Registered Modal {SubWindowName} of type {type}");
        Modals.Add(type, this);
        ModalsPerSubWindowName[SubWindowName] = this;
    }

    /// <summary>
    /// Adds the Modal to the screen
    /// </summary>
    public void Show()
    {
        FreezeTime.Begin($"Nitrox{SubWindowName}");
        StartCoroutine(Show_Impl());
    }

    /// <summary>
    /// Removes the Modal from the screen
    /// </summary>
    public void Hide()
    {
        FreezeTime.End($"Nitrox{SubWindowName}");
        if (!IsAvoidable)
        {
            IsAvoidableBypass = true;
            IngameMenu.main.OnDeselect();
            IsAvoidableBypass = false;
        }
        else
        {
            IngameMenu.main.OnDeselect();
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

        if (HideNoButton && buttonNoObject)
        {
            DestroyImmediate(buttonNoObject);
            buttonYesObject.transform.position = new Vector3(modalSubWindow.transform.position.x / 2, buttonYesObject.transform.position.y, buttonYesObject.transform.position.z); // Center Button
            return;
        }

        Button noButton = buttonNoObject.GetComponent<Button>();
        noButton.onClick = new Button.ButtonClickedEvent();
        noButton.onClick.AddListener(() => { ClickNo(); });
        buttonNoObject.GetComponentInChildren<Text>().text = NoButtonText;
    }

    public abstract void ClickYes();
    public abstract void ClickNo();

    private IEnumerator Show_Impl()
    {
        // Execute frame-by-frame to allow UI scripts to initialize.
        InitSubWindow();
        yield return null;
        IngameMenu.main.Open();
        yield return null;
        IngameMenu.main.ChangeSubscreen(SubWindowName);
    }

    /// <summary>
    /// Lets you get any existing Modal that was registered by its Type
    /// </summary>
    /// <typeparam name="T">The type of the modal to get</typeparam>
    /// <returns>The class of the modal or null if it wasn't found</returns>
    public static T Get<T>() where T : Modal
    {
        if (Modals.TryGetValue(typeof(T), out Modal modal))
        {
            return (T)modal;
        }
        return null;
    }
}
