using System;
using System.Reflection;
using Harmony;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.Helper;
using NitroxModel.Core;

namespace NitroxPatcher.Patches
{
    /// <summary>
    /// Hook onto <see cref="SubFire.CreateFire(SubFire.RoomFire)"/>. We do this on the fire creation method because unlike <see cref="SubRoot.OnTakeDamage(DamageInfo)"/>, fires
    /// can be created outside of <see cref="SubFire.OnTakeDamage(DamageInfo)"/>
    /// </summary>
    class SubFire_CreateFire_Patch : NitroxPatch
    {
        public static readonly Type TARGET_CLASS = typeof(SubFire);
        public static readonly MethodInfo TARGET_METHOD = TARGET_CLASS.GetMethod("CreateFire", BindingFlags.Instance | BindingFlags.Public);

        public static bool Prefix(SubFire __instance, SubFire.RoomFire startInRoom, out bool __state)
        {
            __state = NitroxServiceLocator.LocateService<SimulationOwnership>().HasAnyLockType(GuidHelper.GetGuid(__instance.subRoot.gameObject));

            // Block any new fires if this player is not the owner
            return __state;
        }

        public static void Postfix(SubFire __instance, SubFire.RoomFire startInRoom, bool __state)
        {
            if (__state)
            {
                NitroxServiceLocator.LocateService<Cyclops>().OnCreateFire(__instance, startInRoom);
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchMultiple(harmony, TARGET_METHOD, true, true, false);
        }
    }
}
