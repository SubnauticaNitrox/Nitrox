using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Bases;
using NitroxModel.DataStructures.GameLogic.Entities;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases.EntityUtils;

public static class NitroxInteriorPiece
{
    public static InteriorPieceEntity From(IBaseModule module)
    {
        InteriorPieceEntity interiorPiece = InteriorPieceEntity.MakeEmpty();
        GameObject gameObject = (module as Component).gameObject;
        if (gameObject && gameObject.TryGetComponent(out PrefabIdentifier identifier))
        {
            interiorPiece.ClassId = identifier.ClassId;
        }
        else
        {
            Log.Warn($"Couldn't find an identifier for the interior piece {module.GetType()}");
        }

        if (NitroxEntity.TryGetEntityFrom(gameObject, out NitroxEntity entity))
        {
            interiorPiece.Id = entity.Id;
        }
        else
        {
            Log.Warn($"Couldn't find a NitroxEntity for the interior piece {module.GetType()}");
        }
        if (gameObject.TryGetComponentInParent(out Base parentBase) &&
            NitroxEntity.TryGetEntityFrom(parentBase.gameObject, out NitroxEntity parentEntity))
        {
            interiorPiece.ParentId = parentEntity.Id;
        }

        switch (module)
        {
            case LargeRoomWaterPark:
                PlanterEntity leftPlanter = new(interiorPiece.Id.Increment(), interiorPiece.Id);
                PlanterEntity rightPlanter = new(leftPlanter.Id.Increment(), interiorPiece.Id);
                interiorPiece.ChildEntities.Add(leftPlanter);
                interiorPiece.ChildEntities.Add(rightPlanter);
                break;
            // When you deconstruct (not entirely) then construct back those pieces, they keep their inventories
            case BaseNuclearReactor baseNuclearReactor:
                interiorPiece.ChildEntities.AddRange(Items.GetEquipmentModuleEntities(baseNuclearReactor.equipment, entity.Id));
                break;
            case BaseBioReactor baseBioReactor:
                foreach (ItemsContainer.ItemGroup itemGroup in baseBioReactor.container._items.Values)
                {
                    foreach (InventoryItem item in itemGroup.items)
                    {
                        interiorPiece.ChildEntities.Add(Items.ConvertToInventoryItemEntity(item.item.gameObject));
                    }
                }
                break;
        }

        interiorPiece.BaseFace = module.moduleFace.ToDto();

        return interiorPiece;
    }

    public static string ToString(this SavedInteriorPiece savedInteriorPiece)
    {
        return $"SavedInteriorPiece [ClassId: {savedInteriorPiece.ClassId}, Face: [{savedInteriorPiece.BaseFace.Cell};{savedInteriorPiece.BaseFace.Direction}], Constructed: {savedInteriorPiece.Constructed}]";
    }
}
