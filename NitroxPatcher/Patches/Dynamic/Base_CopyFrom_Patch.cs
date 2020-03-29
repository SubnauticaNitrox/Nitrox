using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using System.Reflection;
using NitroxClient.MonoBehaviours;

namespace NitroxPatcher.Patches.Dynamic
{
    class Base_CopyFrom_Patch : NitroxPatch, IDynamicPatch
    {
        public readonly MethodInfo METHOD = typeof(Base).GetMethod(nameof(Base.CopyFrom), BindingFlags.Public | BindingFlags.Instance);

        public static void Prefix(Base __instance, Base sourceBase)
        {
            NitroxEntity entity = sourceBase.GetComponent<NitroxEntity>();

            if (!sourceBase.GetComponent<BaseRoot>())
            {
                entity = sourceBase.transform.parent.GetComponent<NitroxEntity>();
            }

            NitroxEntity.SetNewId(__instance.gameObject, entity.Id);
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, METHOD);
        }
    }
}
