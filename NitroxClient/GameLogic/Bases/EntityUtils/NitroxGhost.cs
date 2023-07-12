using NitroxClient.GameLogic.Bases.MetadataUtils;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxClient.GameLogic.Bases.EntityUtils;

public static class NitroxGhost
{
    public static GhostEntity From(ConstructableBase constructableBase)
    {
        GhostEntity ghost = GhostEntity.MakeEmpty();

        ghost.FillObject(constructableBase);
        if (constructableBase.moduleFace.HasValue)
        {
            ghost.BaseFace = constructableBase.moduleFace.Value.ToDto();
        }

        ghost.SavedBase = NitroxBase.From(constructableBase.model.GetComponent<Base>());
        if (constructableBase.name.Equals("BaseDeconstructable(Clone)"))
        {
            ghost.TechType = constructableBase.techType.ToDto();
        }

        if (constructableBase.TryGetComponentInChildren(out BaseGhost baseGhost, true))
        {
            ghost.Metadata = NitroxGhostMetadata.GetMetadataForGhost(baseGhost);
        }

        return ghost;
    }

    public static string ToString(this GhostEntity ghostEntity)
    {
        return $"SavedGhost [ClassId: {ghostEntity.ClassId}, NitroxId: {ghostEntity.Id}, ParentId: {ghostEntity.ParentId}, LocalPosition: {ghostEntity.LocalPosition}, LocalRotation: {ghostEntity.LocalRotation}, LocalScale: {ghostEntity.LocalScale}, ConstructedAmount: {ghostEntity.ConstructedAmount}, IsInside: {ghostEntity.IsInside}, ModuleFace: [{ghostEntity.BaseFace}]]";
    }
}
