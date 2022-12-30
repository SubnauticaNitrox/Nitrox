using NitroxClient.Unity.Helper;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu;

public static class JoinServerServerList
{
    //This method merges the cloned color picker element with the existing template for menus that appear in the "right side" region of the main menu.
    public static void InitializeServerList(GameObject saveGameMenuPrototype, out GameObject joinServerMenu, out RectTransform joinServerBackground)
    {
        MainMenuRightSide rightSideMainMenu = MainMenuRightSide.main;

        joinServerMenu = CloneSaveGameMenuPrototype(saveGameMenuPrototype);

        joinServerMenu.transform.SetParent(rightSideMainMenu.transform, false);
        rightSideMainMenu.groups.Add(joinServerMenu.GetComponent<MainMenuGroup>());

        //Not sure what is up with this menu, but we have to use the RectTransform of the Image component as the parent for our color picker panel.
        //Most of the UI elements seem to vanish behind this Image otherwise.
        joinServerBackground = joinServerMenu.GetComponent<Image>().rectTransform;
        joinServerBackground.anchorMin = new Vector2(0.5f, 0.5f);
        joinServerBackground.anchorMax = new Vector2(0.5f, 0.5f);
        joinServerBackground.pivot = new Vector2(0.5f, 0.5f);
        joinServerBackground.anchoredPosition = new Vector2(joinServerBackground.anchoredPosition.x, 5f);
    }

    private static GameObject CloneSaveGameMenuPrototype(GameObject saveGameMenuPrototype)
    {
        GameObject joinServerMenu = Object.Instantiate(saveGameMenuPrototype);
        Object.Destroy(joinServerMenu.RequireGameObject("Header"));
        Object.Destroy(joinServerMenu.RequireGameObject("Scroll View"));
        Object.Destroy(joinServerMenu.GetComponent<LayoutGroup>());
        Object.Destroy(joinServerMenu.GetComponent<MainMenuLoadPanel>());
        joinServerMenu.GetAllComponentsInChildren<LayoutGroup>().ForEach(Object.Destroy);

        //We cannot register click events on child transforms if they are being captured here.
        joinServerMenu.GetComponent<CanvasGroup>().blocksRaycasts = false;
        joinServerMenu.name = "Join Server";

        return joinServerMenu;
    }
}
