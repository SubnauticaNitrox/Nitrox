using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using UnityEngine;
using UnityEngine.UI;

namespace NitroxClient.MonoBehaviours.Gui.InGame
{
    public class LostConnectionModal
    {
        public static void Show()
        {
            IngameMenu.main.Open();
            IngameMenu.main.ChangeSubscreen("QuitConfirmation");

            RectTransform main = IngameMenu.main.currentScreen.GetComponent<RectTransform>();
            main.sizeDelta = new Vector2(700, 195);

            GameObject.DestroyImmediate(IngameMenu.main.currentScreen.FindChild("ButtonNo")); // Delete Button No

            GameObject Header = IngameMenu.main.currentScreen.FindChild("Header"); //Message Object
            Text messageText = Header.GetComponent<Text>();
            RectTransform MessageTransform = Header.GetComponent<UnityEngine.RectTransform>();
            MessageTransform.sizeDelta = new Vector2(700, 195);

            GameObject ButtonYes = IngameMenu.main.currentScreen.FindChild("ButtonYes"); //Button Yes Object
            ButtonYes.transform.position = new Vector3((IngameMenu.main.currentScreen.transform.position.x / 2), ButtonYes.transform.position.y, ButtonYes.transform.position.z); // Center Button

            Text messageTextbutton = ButtonYes.GetComponentInChildren<Text>(); //Get Button Text Component
            messageTextbutton.text = "OK";
 
            if (Language.main.GetCurrentLanguage() == "Spanish")
            {
                messageText.text = "Conexion con servidor perdida";
            }
            if (Language.main.GetCurrentLanguage() == "English")
            {
                messageText.text = "Lost Connection to Game Server";
            }
        }
    }
}
