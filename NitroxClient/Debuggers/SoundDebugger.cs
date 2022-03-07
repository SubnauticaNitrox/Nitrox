using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using FMOD.Studio;
using FMODUnity;
using NitroxClient.GameLogic.FMOD;
using NitroxClient.Unity.Helper;
using UnityEngine;
#pragma warning disable 618

namespace NitroxClient.Debuggers
{
    [ExcludeFromCodeCoverage]
    public class SoundDebugger : BaseDebugger
    {
        private readonly Dictionary<string, SoundData> assetList;
        private readonly Dictionary<string, EventInstance> eventInstancesByPath = new Dictionary<string, EventInstance>();
        private Vector2 scrollPosition;
        private string searchText;
        private string searchCategory;
        private float volume = 1f;
        private float distance;
        private bool displayIsWhitelisted = true;
        private bool displayIsGlobal;
        private bool displayWithRadius;

        public SoundDebugger(FMODSystem fmodSystem) : base(700, null, KeyCode.F, true, false, false, GUISkinCreationOptions.DERIVEDCOPY)
        {
            assetList = fmodSystem.SoundDataList;
            ActiveTab = AddTab("Sounds", RenderTabAllSounds);
        }

        protected override void OnSetSkin(GUISkin skin)
        {
            base.OnSetSkin(skin);

            skin.SetCustomStyle("soundKeyLabel",
                                skin.label,
                                s =>
                                {
                                    s.fontSize = 15;
                                });
            skin.SetCustomStyle("enabled",
                                skin.label,
                                s =>
                                {
                                    s.normal = new GUIStyleState { textColor = Color.green };
                                });
            skin.SetCustomStyle("disabled",
                                skin.label,
                                s =>
                                {
                                    s.normal = new GUIStyleState { textColor = Color.red };
                                });
        }

        private void RenderTabAllSounds()
        {
            using (new GUILayout.VerticalScope("Box"))
            {
                using (new GUILayout.VerticalScope("Box"))
                {
                    using (new GUILayout.HorizontalScope("Box"))
                    {
                        searchText = GUILayout.TextField(searchText);

                        if (GUILayout.Button("Stop all", GUILayout.MaxWidth(60f)))
                        {
                            eventInstancesByPath.Values.ForEach(evt => evt.stop(FMOD.Studio.STOP_MODE.IMMEDIATE));
                            eventInstancesByPath.Clear();
                        }
                    }

                    using (new GUILayout.HorizontalScope())
                    {
                        if (GUILayout.Button("Creature"))
                        {
                            searchCategory = "event:/creature";
                        }
                        if (GUILayout.Button("Environment"))
                        {
                            searchCategory = "event:/env";
                        }
                        if (GUILayout.Button("Player"))
                        {
                            searchCategory = "event:/player";
                        }
                        if (GUILayout.Button("Vehicles"))
                        {
                            searchCategory = "event:/sub";
                        }
                        if (GUILayout.Button("Tools"))
                        {
                            searchCategory = "event:/tools";
                        }
                        if (GUILayout.Button("Clear"))
                        {
                            searchCategory = string.Empty;
                        }
                    }
                }

                using (new GUILayout.HorizontalScope("Box"))
                {
                    using (new GUILayout.VerticalScope(GUILayout.Width(325f)))
                    {
                        displayIsWhitelisted = GUILayout.Toggle(displayIsWhitelisted, "Whitelisted sounds");
                        displayWithRadius = GUILayout.Toggle(displayWithRadius, "Sounds with radius");
                        displayIsGlobal = GUILayout.Toggle(displayIsGlobal, "Global sounds");
                    }

                    using (new GUILayout.VerticalScope(GUILayout.Width(325f)))
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label($"Volume: {NitroxModel.Helper.Mathf.Round(volume, 2)}");
                            volume = GUILayout.HorizontalSlider(volume, 0f, 1f, GUILayout.Width(240f));
                        }
                        GUILayout.Space(5f);
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label($"Distance: {NitroxModel.Helper.Mathf.Round(distance)}");
                            distance = GUILayout.HorizontalSlider(distance, 0f, 500f, GUILayout.Width(240f));
                        }
                    }
                }
            }

            using (new GUILayout.VerticalScope("Box"))
            {
                scrollPosition = GUILayout.BeginScrollView(scrollPosition, GUILayout.Height(500f));


                foreach (KeyValuePair<string, SoundData> sound in assetList)
                {
                    if (displayIsWhitelisted && !sound.Value.IsWhitelisted ||
                        displayIsGlobal && !sound.Value.IsGlobal ||
                        displayWithRadius && sound.Value.SoundRadius <= 0.1f ||
                        !string.IsNullOrWhiteSpace(searchText) && sound.Key.IndexOf(searchText, StringComparison.OrdinalIgnoreCase) == -1 ||
                        !string.IsNullOrWhiteSpace(searchCategory) && !sound.Key.StartsWith(searchCategory))
                    {
                        continue;
                    }

                    using (new GUILayout.VerticalScope("Box", GUILayout.MaxHeight(16f)))
                    {
                        using (new GUILayout.HorizontalScope())
                        {
                            GUILayout.Label(sound.Key, "soundKeyLabel", GUILayout.MaxWidth(370f));
                            GUILayout.FlexibleSpace();
                            GUILayout.Label("Whitelisted", sound.Value.IsWhitelisted ? "enabled" : "disabled");
                            GUILayout.Space(7f);
                            GUILayout.Label("Global", sound.Value.IsGlobal ? "enabled" : "disabled");
                            GUILayout.Space(7f);
                            GUILayout.Label($"Radius: {sound.Value.SoundRadius}", GUILayout.Width(70f));
                            if (GUILayout.Button("Play"))
                            {
                                PlaySound(sound.Key);
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

        private void PlaySound(string eventPath)
        {
            Camera camera = Camera.main;
            if (!camera)
            {
                Log.InGame("Camera.main not found");
                return;
            }
            Vector3 position = camera.transform.position + new Vector3(distance, 0, 0);

            if (!eventInstancesByPath.TryGetValue(eventPath, out EventInstance instance))
            {
                instance = FMODUWE.GetEvent(eventPath);
                eventInstancesByPath.Add(eventPath, instance);
            }
            else
            {
                instance.stop(FMOD.Studio.STOP_MODE.IMMEDIATE);
            }

            instance.setVolume(volume);
            instance.set3DAttributes(position.To3DAttributes());
            instance.setProperty(EVENT_PROPERTY.MINIMUM_DISTANCE, 0f);
            instance.setProperty(EVENT_PROPERTY.MAXIMUM_DISTANCE, 100f);
            instance.start();
            instance.release();
        }

        private void StopSound(string eventPath)
        {
            if (eventInstancesByPath.TryGetValue(eventPath, out EventInstance instance))
            {
                instance.stop(FMOD.Studio.STOP_MODE.ALLOWFADEOUT);
                eventInstancesByPath.Remove(eventPath);
            }
        }
    }
}
