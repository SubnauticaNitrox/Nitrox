using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxModel.Core;
using NitroxModel.DataStructures.Util;
using NitroxModel.Logger;

namespace NitroxPatcher.Patches.Client
{
    public abstract class IHandTarget_OnHandClick_Generic_Patch<T> : IHandTarget_Generic_Patch<T>
        where T : IHandTarget
    {
        public static readonly MethodInfo TARGET_METHOD_OnHandClick = TARGET_CLASS.GetMethod("OnHandClick", BindingFlags.Public | BindingFlags.Instance);

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD_OnHandClick, "OnHandClick_Prefix");
            base.Patch(harmony);
        }
        public static bool OnHandClick_Prefix(T __instance, GUIHand hand)
        {
            return TryGetOwnershipBeforeProceeding(__instance, hand);
        }

        protected override void OnReceiveOwnership(T __instance, object data)
        {
            __instance.OnHandClick((GUIHand)data);
        }
    }

    public abstract class IHandTarget_Generic_Patch<T> : NitroxPatch
        where T : IHandTarget
    {
        protected static IHandTarget_Generic_Patch<T> Self { get; private set; }

        public static readonly Type TARGET_CLASS = typeof(T);
        public static readonly MethodInfo TARGET_METHOD_OnHandHover = TARGET_CLASS.GetMethod("OnHandHover", BindingFlags.Public | BindingFlags.Instance);

        public IHandTarget_Generic_Patch()
        {
            Self = this;
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, TARGET_METHOD_OnHandHover, "OnHandHover_Prefix");
        }

        // Or GetGameObject, have to see what suits best.
        protected abstract Optional<string> GetGuid(T instance);

        // Called when the call continues, because the player already has access.
        protected abstract void HasOwnership(T instance, string guid);

        // Called when the player received ownership; this is where the original function should be re-invoked.
        protected abstract void OnReceiveOwnership(T __instance, object data);

        protected abstract void OnHandDeny(T instance, string guid);

        public static bool TryGetOwnershipBeforeProceeding(T __instance, object data)
        {
            SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();

            Optional<string> opGuid = Self.GetGuid(__instance);
            if (opGuid.IsPresent())
            {
                string guid = opGuid.Get();

                if (simulationOwnership.HasOwnership(guid))
                {
                    Log.Debug($"Already have ownership over {nameof(T)}: {guid}");
                    Self.HasOwnership(__instance, guid);
                    return true;
                }

                simulationOwnership.TryToRequestOwnership(guid, () =>
                {
                    // When ownership is received, the patched function should be invoked again.
                    Log.Debug($"Received ownership over {nameof(T)}: {guid}. Re-invoking...");
                    Self.OnReceiveOwnership(__instance, data);
                });
                return false;
            }
            else
            {
                Log.Debug($"No GUID for handtarget {nameof(T)}! Ownership cannot be validated!");
            }

            return true;
        }

        public static bool OnHandHover_Prefix(T __instance, GUIHand hand)
        {
            SimulationOwnership simulationOwnership = NitroxServiceLocator.LocateService<SimulationOwnership>();

            // Log.Debug($"{nameof(T)} OnHandHover.");
            // Handle the case where another player is in the vehicle, playing the animation (as otherwise the player is not in the vehicle or the vehicle is not docked).
            // This patch is only to update the text and icon.

            Optional<string> opGuid = Self.GetGuid(__instance);
            if (opGuid.IsPresent())
            {
                string guid = opGuid.Get();
                if (!simulationOwnership.CanClaimOwnership(guid))
                {
                    // TODO: Pass owner as well? ("playername is currently in this ...!")
                    Self.OnHandDeny(__instance, guid);
                    return false;
                }
            }

            return true;
        }
    }
}
