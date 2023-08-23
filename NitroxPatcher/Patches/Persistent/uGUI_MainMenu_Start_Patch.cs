using System;
using System.Collections.Generic;
using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours.Gui.MainMenu;
using NitroxModel.Helper;
using UnityEngine;

namespace NitroxPatcher.Patches.Persistent;

public sealed partial class uGUI_MainMenu_Start_Patch : NitroxPatch, IPersistentPatch
{
    private static readonly MethodInfo TARGET_METHOD_ENUMERATOR = Reflect.Method((uGUI_MainMenu t) => t.Start());
    public static readonly MethodInfo TARGET_METHOD = AccessTools.EnumeratorMoveNext(TARGET_METHOD_ENUMERATOR);

    private static bool Applied;

    private static List<Color> Colors = new()
    {
        Color.red, Color.green, Color.blue, Color.yellow, Color.magenta
    };

    public static void Postfix()
    {
        if (Applied)
        {
            return;
        }
        Applied = true;

        foreach (string arg in Environment.GetCommandLineArgs())
        {
            if (arg.StartsWith("-specialDevLaunch"))
            {
                int id = int.Parse(arg[^1].ToString());
                Log.Info($"Detected special dev launch, {arg} connecting to 127.0.0.1:11000 with id {id}");
                _ = MainMenuMultiplayerPanel.OpenJoinServerMenuAsyncAndJoin("127.0.0.1", "11000", id, Colors[id]);
                break;
            }
        }
    }
}
