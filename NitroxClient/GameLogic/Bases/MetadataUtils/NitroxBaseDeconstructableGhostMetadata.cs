using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Bases.Metadata;
using NitroxModel_Subnautica.DataStructures;
using System.Collections;
using UnityEngine;
using UWE;

namespace NitroxClient.GameLogic.Bases.MetadataUtils;

public static class NitroxBaseDeconstructableGhostMetadata
{
    public static BaseDeconstructableGhostMetadata From(BaseGhost baseGhost)
    {
        BaseDeconstructableGhostMetadata metadata = NitroxGhostMetadata.From<BaseDeconstructableGhostMetadata>(baseGhost);
        if (baseGhost.TryGetComponentInParent(out ConstructableBase constructableBase) && constructableBase.moduleFace.HasValue)
        {
            Base.Face moduleFace = constructableBase.moduleFace.Value;
            metadata.ModuleFace = moduleFace.ToDto();
            moduleFace.cell += baseGhost.targetBase.GetAnchor();
            IBaseModule baseModule = baseGhost.targetBase.GetModule(moduleFace);
            if (baseModule != null && (baseModule as MonoBehaviour).TryGetComponent(out PrefabIdentifier identifier))
            {
                metadata.ClassId = identifier.ClassId;
            }
        }
        return metadata;
    }

    public static IEnumerator ApplyTo(this BaseDeconstructableGhostMetadata ghostMetadata, BaseGhost baseGhost, Base @base)
    {
        ghostMetadata.ApplyTo(baseGhost);
        baseGhost.DisableGhostModelScripts();
        if (ghostMetadata.ModuleFace.HasValue)
        {
            if (!baseGhost.TryGetComponentInParent(out ConstructableBase constructableBase))
            {
                Log.Error($"Couldn't find an interior piece's parent ConstructableBase to apply a {nameof(BaseDeconstructableGhostMetadata)} to.");
                yield break;
            }
            constructableBase.moduleFace = ghostMetadata.ModuleFace.Value.ToUnity();

            IPrefabRequest request = PrefabDatabase.GetPrefabAsync(ghostMetadata.ClassId);
            yield return request;
            if (!request.TryGetPrefab(out GameObject prefab))
            {
                // Without its module, the ghost will be useless, so we delete it (like in base game)
                Object.Destroy(constructableBase.gameObject);
                Log.Error($"Couldn't find a prefab for module of interior piece of ClassId: {ghostMetadata.ClassId}");
                yield break;
            }
            if (!baseGhost.targetBase)
            {
                baseGhost.targetBase = @base;
            }
            Base.Face face = ghostMetadata.ModuleFace.Value.ToUnity();
            face.cell += baseGhost.targetBase.GetAnchor();
            GameObject moduleObject = baseGhost.targetBase.SpawnModule(prefab, face);
            if (!moduleObject)
            {
                Object.Destroy(constructableBase.gameObject);
                Log.Error("Module couldn't be spawned for interior piece");
                yield break;
            }
            if (moduleObject.TryGetComponent(out IBaseModule baseModule))
            {
                baseModule.constructed = constructableBase.constructedAmount;
            }
        }
    }

    public static void LateApplyTo(this BaseDeconstructableGhostMetadata _, BaseGhost baseGhost)
    {
        baseGhost.DisableGhostModelScripts();
    }
}
