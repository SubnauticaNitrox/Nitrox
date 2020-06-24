using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Harmony;
using System.Reflection;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxClient.GameLogic.Bases;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;

namespace NitroxPatcher.Patches.Dynamic
{
    class Base_CopyFrom_Patch : NitroxPatch, IDynamicPatch
    {
        public readonly MethodInfo METHOD = typeof(Base).GetMethod(nameof(Base.CopyFrom), BindingFlags.Public | BindingFlags.Instance);

        public static void Prefix(Base __instance, Base sourceBase)
        {
            NitroxId sourceBaseId = NitroxEntity.GetIdNullable(sourceBase.gameObject);
            NitroxId targetBaseId = NitroxEntity.GetIdNullable(__instance.gameObject);

#if TRACE && BUILDING
            BaseRoot sourceBaseRoot = sourceBase.GetComponent<BaseRoot>();
            BaseRoot targetBaseRoot = targetBase.GetComponent<BaseRoot>();
            NitroxModel.Logger.Log.Debug("Base_CopyFrom_Pre - Base copy - sourceBase: " + sourceBase + " targetBase: " + targetBase + " targetBaseIsGhost: " + targetBase.isGhost + " sourceBaseId: " + sourceBaseId + " targetBaseId: " + targetBaseId + " sourceBaseRoot: " + sourceBaseRoot + " targetBaseRoot: " + targetBaseRoot);
#endif

            if (NitroxServiceLocator.LocateService<Building>().baseGhostsIDCache.ContainsKey(sourceBase.gameObject) && targetBaseId == null && !__instance.isGhost)
            {

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Base_CopyFrom_Pre - assigning cached Id from remote event or initial loading: " + baseGhostsIDCache[sourceBase.gameObject]);
#endif

                NitroxEntity.SetNewId(__instance.gameObject, NitroxServiceLocator.LocateService<Building>().baseGhostsIDCache[sourceBase.gameObject]);
            }
            // Transferring from a real base to a ghost base in case of beginning deconstruction of the last basepiece. Need this if player does not completely destroy 
            // last piece instead chooses to reconstruct this last piece.
            else if (sourceBaseId != null && !sourceBase.isGhost && !NitroxServiceLocator.LocateService<Building>().baseGhostsIDCache.ContainsKey(__instance.gameObject))
            {
                NitroxServiceLocator.LocateService<Building>().baseGhostsIDCache[__instance.gameObject] = sourceBaseId;

#if TRACE && BUILDING
                NitroxModel.Logger.Log.Debug("Base_CopyFrom_Pre - caching Base Id from deconstructing object: " + sourceBaseId);
#endif
            }
        }

        public override void Patch(HarmonyInstance harmony)
        {
            PatchPrefix(harmony, METHOD);
        }
    }
}
