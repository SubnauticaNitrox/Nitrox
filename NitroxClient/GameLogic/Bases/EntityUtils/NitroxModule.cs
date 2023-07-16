using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases.EntityUtils;

public static class NitroxModule
{
    public static void FillObject(this ModuleEntity moduleEntity, Constructable constructable)
    {
        moduleEntity.ClassId = constructable.GetComponent<PrefabIdentifier>().ClassId;

        if (NitroxEntity.TryGetEntityFrom(constructable.gameObject, out NitroxEntity entity))
        {
            moduleEntity.Id = entity.Id;
        }
        if (constructable.TryGetComponentInParent(out Base parentBase) &&
            NitroxEntity.TryGetEntityFrom(parentBase.gameObject, out NitroxEntity parentEntity))
        {
            moduleEntity.ParentId = parentEntity.Id;
        }
        moduleEntity.LocalPosition = constructable.transform.localPosition.ToDto();
        moduleEntity.LocalRotation = constructable.transform.localRotation.ToDto();
        moduleEntity.LocalScale = constructable.transform.localScale.ToDto();
        moduleEntity.TechType = constructable.techType.ToDto();
        moduleEntity.ConstructedAmount = constructable.constructedAmount;
        moduleEntity.IsInside = constructable.isInside;
    }

    public static void SetToModuleLocation(this Transform transform, ModuleEntity moduleEntity)
    {
        transform.localPosition = moduleEntity.LocalPosition.ToUnity();
        transform.localRotation = moduleEntity.LocalRotation.ToUnity();
        transform.localScale = moduleEntity.LocalScale.ToUnity();
    }

    public static ModuleEntity From(Constructable constructable)
    {
        ModuleEntity module = ModuleEntity.MakeEmpty();
        module.FillObject(constructable);
        return module;
    }
}
