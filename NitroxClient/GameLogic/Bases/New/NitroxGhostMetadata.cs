using System.Collections;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Buildings.New.Metadata;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;
using UWE;

namespace NitroxClient.GameLogic.Bases.New;

public static class NitroxGhostMetadata
{
    public static T From<T>(BaseGhost baseGhost) where T : GhostMetadata, new()
    {
        T metadata = new();
        metadata.TargetOffset = baseGhost.targetOffset.ToDto();
        return metadata;
    }

    public static void ApplyTo(this GhostMetadata ghostMetadata, BaseGhost baseGhost)
    {
        baseGhost.targetOffset = ghostMetadata.TargetOffset.ToUnity();
    }

    public static GhostMetadata GetMetadataForGhost(BaseGhost baseGhost)
    {
        // Specific case in which a piece was deconstructed and resulted in a BaseDeconstructable with a normal BaseGhost
        if (BuildManager.IsUnderBaseDeconstructable(baseGhost))
        {
            return NitroxBaseDeconstructableGhostMetadata.From(baseGhost);
        }
        
        GhostMetadata metadata = baseGhost switch
        {
            BaseAddWaterPark or BaseAddPartitionDoorGhost or BaseAddModuleGhost or BaseAddFaceGhost => NitroxBaseAnchoredFaceGhostMetadata.From(baseGhost),
            BaseAddPartitionGhost => NitroxBaseAnchoredCellGhostMetadata.From(baseGhost),
            _ => From<BasicGhostMetadata>(baseGhost),
        };
        return metadata;
    }

    public static IEnumerator ApplyMetadataToGhost(BaseGhost baseGhost, EntityMetadata entityMetadata, Base @base)
    {
        if (entityMetadata is not GhostMetadata ghostMetadata)
        {
            Log.Error($"Trying to apply metadata to a ghost that is not of type {nameof(GhostMetadata)} : [{entityMetadata.GetType()}]");
            yield break;
        }
        if (BuildManager.IsUnderBaseDeconstructable(baseGhost, true) && entityMetadata is BaseDeconstructableGhostMetadata deconstructableMetadata)
        {
            yield return deconstructableMetadata.ApplyTo(baseGhost, @base);
            yield break;
        }

        switch (baseGhost)
        {
            case BaseAddWaterPark:
            case BaseAddPartitionDoorGhost:
            case BaseAddModuleGhost:
            case BaseAddFaceGhost:
                if (ghostMetadata is BaseAnchoredFaceGhostMetadata faceMetadata)
                {
                    faceMetadata.ApplyTo(baseGhost);
                }
                break;
            case BaseAddPartitionGhost:
                if (ghostMetadata is BaseAnchoredCellGhostMetadata cellMetadata)
                {
                    cellMetadata.ApplyTo(baseGhost);
                }
                break;
            default:
                ghostMetadata.ApplyTo(baseGhost);
                break;
        }
    }

    public static void LateApplyMetadataToGhost(BaseGhost baseGhost, EntityMetadata entityMetadata)
    {
        if (BuildManager.IsUnderBaseDeconstructable(baseGhost, true) && entityMetadata is BaseDeconstructableGhostMetadata deconstructableMetadata)
        {
            deconstructableMetadata.LateApplyTo(baseGhost);
        }
    }
}

public static class NitroxBaseDeconstructableGhostMetadata
{
    public static BaseDeconstructableGhostMetadata From(BaseGhost baseGhost)
    {
        BaseDeconstructableGhostMetadata metadata = NitroxGhostMetadata.From<BaseDeconstructableGhostMetadata>(baseGhost);
        if (baseGhost.TryGetComponentInParent(out ConstructableBase constructableBase) && constructableBase.moduleFace.HasValue)
        {
            metadata.ModuleFace = constructableBase.moduleFace.Value.ToDto();
            IBaseModule baseModule = baseGhost.targetBase.GetModule(constructableBase.moduleFace.Value);
            if (baseModule != null && (baseModule as MonoBehaviour).TryGetComponent(out PrefabIdentifier identifier))
            {
                metadata.ClassId = identifier.ClassId;
            }
        }
        return metadata;
    }

    public static IEnumerator ApplyTo(this BaseDeconstructableGhostMetadata ghostMetadata, BaseGhost baseGhost, Base @base)
    {
        NitroxGhostMetadata.ApplyTo(ghostMetadata, baseGhost);
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
                GameObject.Destroy(constructableBase.gameObject);
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
                GameObject.Destroy(constructableBase.gameObject);
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


public static class NitroxBaseAnchoredFaceGhostMetadata
{
    public static BaseAnchoredFaceGhostMetadata From(BaseGhost baseGhost)
    {
        BaseAnchoredFaceGhostMetadata metadata = NitroxGhostMetadata.From<BaseAnchoredFaceGhostMetadata>(baseGhost);
        metadata.AnchoredFace = baseGhost switch
        {
            BaseAddWaterPark ghost => ghost.anchoredFace?.ToDto(),
            BaseAddPartitionDoorGhost ghost => ghost.anchoredFace?.ToDto(),
            BaseAddModuleGhost ghost => ghost.anchoredFace?.ToDto(),
            BaseAddFaceGhost ghost => ghost.anchoredFace?.ToDto(),
            _ => null
        };
        return metadata;
    }

    public static void ApplyTo(this BaseAnchoredFaceGhostMetadata ghostMetadata, BaseGhost baseGhost)
    {
        NitroxGhostMetadata.ApplyTo(ghostMetadata, baseGhost);
        if (ghostMetadata.AnchoredFace.HasValue)
        {
            switch (baseGhost)
            {
                case BaseAddWaterPark ghost:
                    ghost.anchoredFace = ghostMetadata.AnchoredFace.Value.ToUnity();
                    break;
                case BaseAddPartitionDoorGhost ghost:
                    ghost.anchoredFace = ghostMetadata.AnchoredFace.Value.ToUnity();
                    break;
                case BaseAddModuleGhost ghost:
                    ghost.anchoredFace = ghostMetadata.AnchoredFace.Value.ToUnity();
                    break;
                case BaseAddFaceGhost ghost:
                    ghost.anchoredFace = ghostMetadata.AnchoredFace.Value.ToUnity();
                    break;
            }
        }
    }
}

public static class NitroxBaseAnchoredCellGhostMetadata
{
    public static BaseAnchoredCellGhostMetadata From(BaseGhost baseGhost)
    {
        BaseAnchoredCellGhostMetadata metadata = NitroxGhostMetadata.From<BaseAnchoredCellGhostMetadata>(baseGhost);
        if (baseGhost is BaseAddPartitionGhost ghost && ghost.anchoredCell.HasValue)
        {
            metadata.AnchoredCell = ghost.anchoredCell.Value.ToDto();
        }
        return metadata;
    }

    public static void ApplyTo(this BaseAnchoredCellGhostMetadata ghostMetadata, BaseGhost baseGhost)
    {
        NitroxGhostMetadata.ApplyTo(ghostMetadata, baseGhost);
        if (ghostMetadata.AnchoredCell.HasValue)
        {
            if (baseGhost is BaseAddPartitionGhost ghost)
            {
                ghost.anchoredCell = ghostMetadata.AnchoredCell.Value.ToUnity();
            }
        }
    }
}
