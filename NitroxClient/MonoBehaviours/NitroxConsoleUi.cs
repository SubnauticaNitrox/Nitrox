using System;
using System.Collections.Generic;
using NitroxClient.Unity.Helper;
using NitroxModel.Logger;
using NitroxModel.NitroxConsole;
using NitroxModel.NitroxConsole.Events;
using UnityEngine;

namespace NitroxClient.MonoBehaviours
{
    public class NitroxConsoleUI : MonoBehaviour
    {
        public bool IsRendering { get; private set; }
        /// <summary>
        /// Speed multiplier used when scrolling with the arrow keys (like <see cref="KeyCode.UpArrow"/> or <see cref="KeyCode.PageDown"/>).
        /// </summary>
        public float ArrowScrollSpeedMult { get; } = 500;

        /// <summary>
        /// Multiplicative multiplier on top of <see cref="ArrowScrollSpeedMult"/> when holding down shift.
        /// </summary>
        public float ArrowScrollSpeedShiftMult { get; } = 3;
        public Vector2 ScrollPosition
        {
            get { return scrollPosition; }
            set
            {
                if (value.x < 0)
                {
                    value.x = 0;
                }

                if (value.y < 0)
                {
                    value.y = 0;
                }

                scrollPosition = value;
            }
        }

        private string consoleInput;
        private Queue<string> consoleText = new Queue<string>();
        private Vector2 scrollPosition = Vector2.zero;
        private const string CONTROL_CONSOLEINPUT = "consoleinput";
        private const int MAX_CONSOLETEXT_LINES = 1000;
        private bool isReturnDown;
        private Rect consoleTextArea;

        public string ConsoleInput
        {
            get { return consoleInput ?? ""; }
            set
            {
                consoleInput = value;
            }
        }

        public void AddText(string text)
        {
            consoleText.Enqueue(text);
            while (consoleText.Count > MAX_CONSOLETEXT_LINES)
            {
                consoleText.Dequeue();
            }
        }

        public void AddErrorText(string errorText)
        {
            AddText($"<color=red>{errorText}</color>");
        }

        private void Update()
        {
            if (Input.GetKeyDown(KeyCode.BackQuote))
            {
                IsRendering = !IsRendering;
            }

            if (!IsRendering)
            {
                return;
            }

            if (isReturnDown)
            {
                isReturnDown = false;
                NitroxConsole.Main.Submit(ConsoleInput);
                ConsoleInput = null;
            }
        }

        private float GetFullScrollSpeedMult(Event evt)
        {
            if (evt.shift)
            {
                return ArrowScrollSpeedMult * ArrowScrollSpeedShiftMult * Time.deltaTime;
            }
            return ArrowScrollSpeedMult * Time.deltaTime;
        }

        private void OnEnable()
        {
            NitroxConsole.Main.CommandReceived += OnCommandReceived;
            NitroxConsole.Main.InvalidCommandReceived += OnInvalidCommandReceived;
        }

        private void OnDisable()
        {
            NitroxConsole.Main.CommandReceived -= OnCommandReceived;
            NitroxConsole.Main.InvalidCommandReceived -= OnInvalidCommandReceived;
        }

        private void OnInvalidCommandReceived(object sender, CommandCandidateEventArgs e)
        {
            AddErrorText($"Invalid console command: {e.Value}, Error: {e.State}{(!string.IsNullOrEmpty(e.StateMessage) ? $", Message: {e.StateMessage}" : "")}");
        }

        private void OnCommandReceived(object sender, CommandEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.HandlerMessage))
            {
                if (e.HasError)
                {
                    AddErrorText(e.HandlerMessage);
                }
                else
                {
                    AddText(e.HandlerMessage);
                }
                return;
            }
            AddText($"Command executed: {e.Value}");
        }

        private void OnGUI()
        {
            if (!IsRendering)
            {
                return;
            }

            Event evt = Event.current;
            if (evt.isKey)
            {
                if (evt.type == EventType.KeyDown)
                {
                    switch (evt.keyCode)
                    {
                        case KeyCode.BackQuote:
                            if (GUI.GetNameOfFocusedControl() == CONTROL_CONSOLEINPUT)
                            {
                                ConsoleInput = null;
                                NitroxConsole.Main.HistoryMoveAfterNow();
                                IsRendering = false;
                            }
                            break;
                        case KeyCode.Return:
                        case KeyCode.KeypadEnter:
                            isReturnDown = true;
                            break;
                        case KeyCode.PageUp:
                            ScrollPosition += Vector2.down * GetFullScrollSpeedMult(evt);
                            break;
                        case KeyCode.PageDown:
                            ScrollPosition += Vector2.up * GetFullScrollSpeedMult(evt);
                            break;
                        case KeyCode.LeftArrow:
                            ScrollPosition += Vector2.left * GetFullScrollSpeedMult(evt);
                            break;
                        case KeyCode.RightArrow:
                            ScrollPosition += Vector2.right * GetFullScrollSpeedMult(evt);
                            break;
                        case KeyCode.UpArrow:
                            CommandEventArgs argPrevious = NitroxConsole.Main.HistoryPrevious();
                            ConsoleInput = argPrevious.Value;
                            break;
                        case KeyCode.DownArrow:
                            CommandEventArgs argNext = NitroxConsole.Main.HistoryNext();
                            ConsoleInput = argNext.Value;
                            break;
                        case KeyCode.Home:
                            ScrollPosition = Vector2.zero;
                            break;
                        case KeyCode.End:
                            ScrollPosition = new Vector2(0, consoleTextArea.y);
                            break;
                    }
                }
            }

            // TODO: Rescaling on all resolutions.
            //Vector3 scale = new Vector3(Screen.width / originalSize.x, Screen.height / originalSize.y, 1);
            //Matrix4x4 matrix = Matrix4x4.TRS(Vector3.zero, Quaternion.identity, scale);

            GUISkinUtils.RenderWithSkin(GUISkinUtils.RegisterDerivedOnce("console", SetSkinStyle), Render);

            if (GUI.GetNameOfFocusedControl() != CONTROL_CONSOLEINPUT)
            {
                GUI.FocusControl(CONTROL_CONSOLEINPUT);
            }
        }

        private void Render()
        {
            using (new GUILayout.VerticalScope("Box", GUILayout.Width(Screen.width), GUILayout.Height(Screen.height / 4f)))
            {
                using (GUILayout.ScrollViewScope scroll = new GUILayout.ScrollViewScope(ScrollPosition, "fillscroll"))
                {
                    ScrollPosition = scroll.scrollPosition;
                    using (new GUILayout.VerticalScope("consoletextarea"))
                    {
                        foreach (string line in consoleText)
                        {
                            GUILayout.Label(line, "consoletext");
                        }
                    }

                    // Get the total size of the text area inside the console scrollview.
                    consoleTextArea = GUILayoutUtility.GetLastRect();
                }

                using (new GUILayout.HorizontalScope())
                {
                    GUILayout.Label(">", "input-prefix");
                    GUI.SetNextControlName(CONTROL_CONSOLEINPUT);
                    ConsoleInput = GUILayout.TextField(ConsoleInput, "input");
                }
            }
        }

        private void SetSkinStyle(GUISkin skin)
        {
            skin.SetCustomStyle("fillscroll", "scrollView", style =>
            {
                style.stretchWidth = true;
                style.stretchHeight = true;
            });

            skin.SetCustomStyle("consoletextarea", "box", style =>
            {
                style.stretchWidth = true;
                style.stretchHeight = true;
            });

            skin.SetCustomStyle("consoletext", "label", style =>
            {
                style.stretchWidth = true;
                style.margin = new RectOffset();
                style.padding = new RectOffset();
            });

            skin.SetCustomStyle("input-prefix", "label", style =>
            {
                style.fontSize = 20;
                style.normal.textColor = Color.cyan;
                style.fixedWidth = 20;
                style.margin.left = 20;
            });

            skin.SetCustomStyle("input", "textfield", style =>
            {
                style.fontSize = 20;
                style.fontStyle = FontStyle.Bold;
            });
        }
    }
}
