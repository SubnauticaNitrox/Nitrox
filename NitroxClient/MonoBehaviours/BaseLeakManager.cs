using System.Collections.Generic;
using NitroxClient.GameLogic;
using NitroxModel.DataStructures;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.MonoBehaviours;

public class BaseLeakManager : MonoBehaviour
{
    private Dictionary<Int3, NitroxId> idByRelativeCell;
    private Base @base;
    private BaseHullStrength baseHullStrength;
    private NitroxId baseId;

    public void Awake()
    {
        if (!TryGetComponent(out baseHullStrength))
        {
            Log.Error($"Tried adding a {nameof(BaseLeakManager)} to a GameObject that isn't a base, deleting it.");
            Destroy(this);
            return;
        }
        @base = baseHullStrength.baseComp;
        @base.TryGetNitroxId(out baseId);
        idByRelativeCell = new();
    }

    /// <summary>
    /// Either creates or updates existing leaks by modifying the base cell's health.
    /// Also registers the leak's id for further use.
    /// </summary>
    public void EnsureLeak(Int3 relativeCell, NitroxId cellId, float health)
    {
        Int3 absoluteCell = Absolute(relativeCell);
        Transform cellObject = @base.GetCellObject(absoluteCell);
        if (!cellObject)
        {
            return;
        }

        idByRelativeCell[relativeCell] = cellId;

        if (cellObject.TryGetComponent(out LiveMixin cellLiveMixin))
        {
            // Health goes from 0 to 100
            float deltaHealth = health - cellLiveMixin.health;
            if (Mathf.Abs(deltaHealth) > 1)
            {
                // Useful part of BaseHullStrength.CrushDamageUpdate
                this.Resolve<LiveMixinManager>().SyncRemoteHealth(cellLiveMixin, health, cellObject.position, DamageType.Pressure);
                
                // Only play noise if the leak lost health
                if (deltaHealth >= 0)
                {
                    return;
                }
                // Spawning multiple leaks would result in a big sounds when loading the game
                if (Multiplayer.Main && Multiplayer.Main.InitialSyncCompleted)
                {
                    // Code from BaseHullStrength.CrushDamageUpdate
                    int num = 0;
                    if (baseHullStrength.totalStrength <= -3f)
                    {
                        num = 2;
                    }
                    else if (baseHullStrength.totalStrength <= -2f)
                    {
                        num = 1;
                    }
                    if (baseHullStrength.crushSounds[num] != null)
                    {
                        // TODO: When #1780 is merged, change this accordingly
                        Utils.PlayFMODAsset(baseHullStrength.crushSounds[num], cellObject, 20f);
                    }
                    ErrorMessage.AddMessage(Language.main.GetFormat("BaseHullStrDamageDetected", baseHullStrength.totalStrength));
                }
            }
        }
    }

    public void HealLeakToMax(Int3 relativeCell)
    {
        Transform cellObject = @base.GetCellObject(Absolute(relativeCell));
        if (cellObject && cellObject.TryGetComponent(out LiveMixin liveMixin))
        {
            this.Resolve<LiveMixinManager>().SyncRemoteHealth(liveMixin, liveMixin.maxHealth);
            idByRelativeCell.Remove(relativeCell);
        }
    }

    public Int3 Absolute(Int3 relativeCell)
    {
        return relativeCell + @base.anchor;
    }

    public Int3 Relative(Int3 absoluteCell)
    {
        return absoluteCell - @base.anchor;
    }

    public LeakRepaired RemoveLeakByAbsoluteCell(Int3 absoluteCell)
    {
        Int3 relativeCell = Relative(absoluteCell);
        if (idByRelativeCell.TryGetValue(relativeCell, out NitroxId cellId))
        {
            idByRelativeCell.Remove(relativeCell);
            return new(baseId, cellId, relativeCell.ToDto());
        }
        return null;
    }

    public bool TryGetAbsoluteCellId(Int3 absoluteCell, out NitroxId cellId)
    {
        return idByRelativeCell.TryGetValue(Relative(absoluteCell), out cellId);
    }
}
