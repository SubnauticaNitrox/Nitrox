using System;
using System.Collections.Generic;
using System.Linq;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.Unity.Helper;
using NitroxModel.Logger;
using NitroxModel.Packets;
using UnityEngine;
#pragma warning disable 618

namespace NitroxClient.Debuggers
{
    public class SoundDebugger : BaseDebugger
    {
        private readonly Dictionary<string, SoundData> assetList;

        private readonly Dictionary<string, EventInstance> eventInstancesByPath = new Dictionary<string, EventInstance>();
        private Vector2 scrollPosition;
        private bool displayIsWhitelisted = true;
        private bool displayIsGlobal;
        private bool displayWithRadius;

        public SoundDebugger(FMODSystem fmodSystem) : base(650, null, KeyCode.S, true, false, true, GUISkinCreationOptions.DERIVEDCOPY)
        {
            assetList = fmodSystem.GetSoundDataList();
            ActiveTab = AddTab("All Sounds", RenderTabAllSounds);
            AddTab("Settings", RenderTabSettings);
        }

        protected override void OnSetSkin(GUISkin skin)
        {
            base.OnSetSkin(skin);

            skin.SetCustomStyle("middleLabel",
                                skin.label,
                                s =>
                                {
                                    s.alignment = TextAnchor.MiddleCenter;
                                });
        }

        private void RenderTabAllSounds()
        {
            using (new GUILayout.VerticalScope("Box"))
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(500f));

                foreach (KeyValuePair<string, SoundData> sound in assetList)
                {
                    if (displayIsWhitelisted && !sound.Value.IsWhitelisted ||
                        displayIsGlobal && !sound.Value.IsGlobal ||
                        displayWithRadius && sound.Value.SoundRadius == 0f)
                    {
                        continue;
                    }

                    using (new GUILayout.VerticalScope("Box", GUILayout.MaxHeight(13f)))
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label(sound.Key, "middleLabel", GUILayout.MaxWidth(250f));
                            GUILayout.FlexibleSpace();
                            GUILayout.Toggle(sound.Value.IsWhitelisted, "Is whitelisted", GUILayout.MaxHeight(13f));
                            GUILayout.Toggle(sound.Value.IsGlobal, "Is global", GUILayout.MaxHeight(13f));
                            GUILayout.Label($"Radius: {sound.Value.SoundRadius}", "middleLabel");
                            GUILayout.Space(10f);
                            if (GUILayout.Button("Play"))
                            {
                                PlaySound(sound.Key, Player.main ? Player.main.transform.position : Vector3.zero);
                            }
                            if (GUILayout.Button("Stop"))
                            {
                                StopSound(sound.Key);
                            }
                        }
                    }
                }

                GUILayout.EndScrollView();
            }
        }

        private void RenderTabSettings()
        {
            using (new GUILayout.VerticalScope("Box"))
            {
                displayIsWhitelisted = GUILayout.Toggle(displayIsWhitelisted, "Display whitelisted sounds");
                displayIsGlobal = GUILayout.Toggle(displayIsGlobal, "Display global sounds");
                displayWithRadius = GUILayout.Toggle(displayWithRadius, "Display sounds with radius");
            }
        }

        private void PlaySound(string eventPath, Vector3 position)
        {
            if (!eventInstancesByPath.TryGetValue(eventPath, out EventInstance instance))
            {
                instance = FMODUWE.GetEvent(eventPath);
            }
            instance.setVolume(1f);
            instance.set3DAttributes(position.To3DAttributes());
            instance.start();
            instance.release();
        }

        private void StopSound(string eventPath)
        {
            if (eventInstancesByPath.TryGetValue(eventPath, out EventInstance instance))
            {
                instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
            }
        }
    }
}
