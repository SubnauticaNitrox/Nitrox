using System.Collections;
using NitroxClient.GameLogic.Spawning.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxModel.DataStructures.GameLogic.Entities.Bases;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.WorldEntities;

/// <summary>
/// Meant to be used only when a moonpool is the first thing built in a base.
/// Moonpools added to existing bases already spawn in correctly when UpdateBase is processed.
/// </summary>
public class MoonpoolEntitySpawner : EntitySpawner<MoonpoolEntity>
{
    protected override IEnumerator SpawnAsync(MoonpoolEntity entity, TaskResult<Optional<GameObject>> result)
    {
        // Wait until the PlaceBase packet gets processed
        Base @base;
        while (true)
        {
            Optional<GameObject> parent = NitroxEntity.GetObjectFrom(entity.ParentId);
            if (parent.HasValue)
            {
                @base = parent.Value.GetComponent<Base>();
                if (@base)
                {
                    break;
                }
            }

            yield return null;
        }

        MoonpoolManager moonpoolManager = @base.gameObject.EnsureComponent<MoonpoolManager>();
        yield return MoonpoolManager.RestoreMoonpools([..moonpoolManager.GetSavedMoonpools(), entity], @base);

        result.Set(Optional.OfNullable(moonpoolManager.FindGameObjectForMoonpool(entity.Cell.ToUnity())));
    }

    protected override bool SpawnsOwnChildren(MoonpoolEntity entity) => true;
}
