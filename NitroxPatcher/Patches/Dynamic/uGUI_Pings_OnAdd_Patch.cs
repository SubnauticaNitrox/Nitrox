using System;
using System.Reflection;
using HarmonyLib;
using NitroxClient.MonoBehaviours;
using UnityEngine;

namespace NitroxPatcher.Patches.Dynamic;

/// <summary>
/// Configures the pooled HUD ping used by remote players with a prominent off-screen chevron.
/// </summary>
public sealed class uGUI_Pings_OnAdd_Patch : NitroxPatch, IDynamicPatch
{
    public static readonly MethodInfo TARGET_METHOD = Reflect.Method((uGUI_Pings pings) => pings.OnAdd(default));

    private static readonly Type remotePlayerPingIdentifierType = RequireClientType("NitroxClient.GameLogic.PlayerLogic.PlayerModel.RemotePlayerPingIdentifier");
    private static readonly Type remotePlayerPingChevronType = RequireClientType("NitroxClient.MonoBehaviours.Gui.InGame.RemotePlayerPingChevron");
    private static readonly MethodInfo configureRemotePlayerPingChevronMethod = RequireClientMethod(remotePlayerPingChevronType, "Configure");

    private static Type RequireClientType(string fullName) => typeof(Multiplayer).Assembly.GetType(fullName) ?? throw new TypeLoadException(fullName);
    private static MethodInfo RequireClientMethod(Type type, string methodName) =>
        type.GetMethod(methodName, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic) ?? throw new MissingMethodException(type.FullName, methodName);

    public static void Postfix(uGUI_Pings __instance, PingInstance __0)
    {
        if (!__0 || !__instance.pings.TryGetValue(__0.Id, out uGUI_Ping hudPing))
        {
            return;
        }

        Component remotePlayerIdentifier = __0.GetComponent(remotePlayerPingIdentifierType);
        bool isRemotePlayer = remotePlayerIdentifier;
        Behaviour chevron = hudPing.GetComponent(remotePlayerPingChevronType) as Behaviour;
        if (!chevron)
        {
            if (!isRemotePlayer)
            {
                return;
            }

            chevron = (Behaviour)hudPing.gameObject.AddComponent(remotePlayerPingChevronType);
        }

        configureRemotePlayerPingChevronMethod.Invoke(chevron, [remotePlayerIdentifier]);
        chevron.enabled = isRemotePlayer;
    }

    public override void Patch(Harmony harmony)
    {
        PatchPostfix(harmony, TARGET_METHOD, ((Action<uGUI_Pings, PingInstance>)Postfix).Method);
    }
}
