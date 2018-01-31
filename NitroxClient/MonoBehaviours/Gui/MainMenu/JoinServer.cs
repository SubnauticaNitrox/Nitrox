using NitroxClient.Unity.Helper;
using System.Collections;
using NitroxModel.Helper;
using UnityEngine;
using NitroxModel.Logger;
using NitroxModel;
using System.ComponentModel;

namespace NitroxClient.MonoBehaviours.Gui.MainMenu
{
    public class JoinServer : MonoBehaviour
    {
        public string ServerIp = "";
        Rect joinServerWindowRect = new Rect(Screen.width / 2 - 250, 200, 500, 150);
        Rect unableToJoinWindowRect = new Rect(Screen.width / 2 - 250, 200, 500, 150);
        string username = "username";
        bool joiningServer = false;
        bool notifyingUnableToJoin = false;
        bool shouldFocus;

        private GameObject multiplayerClient;

        public void Awake()
        {
            DontDestroyOnLoad(gameObject);
            
        }

        public void Start()
        {
            joiningServer = true;
            notifyingUnableToJoin = false;
            shouldFocus = true;
        }

        public void OnGUI()
        {
            if (joiningServer)
            {
                joinServerWindowRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Keyboard), joinServerWindowRect, RenderJoinServerDialog, "Join server");
            }

            if (notifyingUnableToJoin)
            {
                unableToJoinWindowRect = GUILayout.Window(GUIUtility.GetControlID(FocusType.Keyboard), joinServerWindowRect, RenderUnableToJoinDialog, "Unable to Join Session");
            }
        }

        private IEnumerator NegotiateSession(string serverIp)
        {
            Log.InGame("Negotiating session...");            

            if(Multiplayer.Main == null)
            {
                Log.InGame("Critical error, Multiplayer main unset.");
            }
            Multiplayer.Main.NegotiatePlayerSlotReservation(serverIp, username);

            Log.InGame("Waiting for reservation...");
            yield return new WaitUntil(() => Multiplayer.Logic.ClientBridge.CurrentState != Communication.ClientBridgeState.WaitingForRerservation);

            switch (Multiplayer.Logic.ClientBridge.CurrentState)
            {
                case Communication.ClientBridgeState.Reserved:
                    Log.InGame("Launching game...");
                    StartCoroutine(LaunchSession());
                    break;
                case Communication.ClientBridgeState.ReservationRejected:
                    Log.InGame("Reservation rejected...");
                    notifyingUnableToJoin = true;
                    break;
                default:
                    break;
            }
        }

        private IEnumerator LaunchSession()
        {
            Log.InGame("Launching game...");
			
#pragma warning disable CS0618 // Type or member is obsolete
            IEnumerator startNewGame = (IEnumerator)uGUI_MainMenu.main.ReflectionCall("StartNewGame", false, false, GameMode.Survival);
            StartCoroutine(startNewGame);

            Log.InGame("Waiting for game to load...");
            //Wait until game starts
            yield return new WaitUntil(() => LargeWorldStreamer.main != null);
            yield return new WaitUntil(() => LargeWorldStreamer.main.IsReady() || LargeWorldStreamer.main.IsWorldSettled());
            yield return new WaitUntil(() => !PAXTerrainController.main.isWorking);

            Log.InGame("Joining Multiplayer Session...");
            Multiplayer.Main.JoinSession();

            multiplayerClient = null;
            Destroy(gameObject);
        }

        private GUISkin GetGUISkin(string skinName, int labelWidth)
        {
            return GUISkinUtils.RegisterDerivedOnce(skinName, s =>
            {
                s.textField.fontSize = 14;
                s.textField.richText = false;
                s.textField.alignment = TextAnchor.MiddleLeft;
                s.textField.wordWrap = true;
                s.textField.stretchHeight = true;
                s.textField.padding = new RectOffset(10, 10, 5, 5);

                s.label.fontSize = 14;
                s.label.alignment = TextAnchor.MiddleRight;
                s.label.stretchHeight = true;
                s.label.fixedWidth = labelWidth; 

                s.button.fontSize = 14;
                s.button.stretchHeight = true;
            });
        }

        private void RenderJoinServerDialog(int windowId)
        {
            Event e = Event.current;
            if (e.isKey)
            {
                switch (e.keyCode)
                {
                    case KeyCode.Return:
                        StartMultiplayerClient();
                        break;
                    case KeyCode.Escape:
                        StopMultiplayerClient();
                        break;
                }
            }

            GUISkinUtils.RenderWithSkin(GetGUISkin("menus.server", 80), () => 
            {
                using (new GUILayout.VerticalScope("Box"))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        GUILayout.Label("Username:");
                        GUI.SetNextControlName("usernameField");
                        username = GUILayout.TextField(username);
                    }


					if (GUILayout.Button("Join"))
					{
						StartMultiplayerClient();
					}

					if (GUILayout.Button("Cancel"))
					{
						StopMultiplayerClient();
					}
				}
            });

            if (shouldFocus)
            {
                GUI.FocusControl("usernameField");
                shouldFocus = false;
            }
        }

        private void StartMultiplayerClient()
        {
            if (multiplayerClient == null)
            {
                multiplayerClient = new GameObject();
                multiplayerClient.AddComponent<Multiplayer>();
            }

            StartCoroutine(NegotiateSession(ServerIp));
            joiningServer = false;
        }

        private void StopMultiplayerClient()
        {
            if (multiplayerClient != null)
            {
                Destroy(multiplayerClient);
                multiplayerClient = null;
            }

            joiningServer = false;
        }

        private void RenderUnableToJoinDialog(int windowId)
        {
            Event e = Event.current;
            if (e.isKey)
            {
                switch (e.keyCode)
                {
                    case KeyCode.Return:
                        joiningServer = true;
                        notifyingUnableToJoin = false;
                        break;
                    case KeyCode.Escape:
                        joiningServer = true;
                        notifyingUnableToJoin = false;
                        break;
                }
            }
			
			GUISkinUtils.RenderWithSkin(GetGUISkin("dialogs.server.rejected", 490), () => 
            {
                using (new GUILayout.VerticalScope("Box"))
                {
                    using (new GUILayout.HorizontalScope())
                    {
                        var rejectionReason = Multiplayer.Logic.ClientBridge.ReservationRejectionReason;
						var descriptionAttribute = rejectionReason.GetAttribute<DescriptionAttribute>();

						GUILayout.Label(descriptionAttribute.Description);
                    }

					if (GUILayout.Button("OK"))
					{
						joiningServer = true;
						notifyingUnableToJoin = false;
					}
				}
            });
        }
    }
}
