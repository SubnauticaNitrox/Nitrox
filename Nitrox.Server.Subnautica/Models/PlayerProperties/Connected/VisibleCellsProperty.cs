using System.Collections.Generic;
using Nitrox.Model.DataStructures;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities;
using Nitrox.Server.Subnautica.Models.PlayerProperties.Core;

namespace Nitrox.Server.Subnautica.Models.PlayerProperties.Connected;

internal sealed class VisibleCellsProperty(OutOfCellVisibleEntitiesProperty outOfCellVisibleEntitiesProperty) : IConnectedPlayerProperty<ThreadSafeSet<AbsoluteEntityCell>, VisibleCellsProperty>
{
    private readonly OutOfCellVisibleEntitiesProperty outOfCellVisibleEntitiesProperty = outOfCellVisibleEntitiesProperty;

    public VisibleCellsProperty() : this(new OutOfCellVisibleEntitiesProperty())
    {
    }

    public ThreadSafeSet<AbsoluteEntityCell> Value
    {
        get => Interlocked.CompareExchange(ref field, null, null);
        set => Interlocked.Exchange(ref field, value);
    } = [];

    public Task ResetAsync()
    {
        Value.Clear();
        return Task.CompletedTask;
    }

    public void AddCells(IEnumerable<AbsoluteEntityCell> cells)
    {
        ThreadSafeSet<AbsoluteEntityCell> value = Value;
        foreach (AbsoluteEntityCell cell in cells)
        {
            value.Add(cell);
        }
    }

    public void RemoveCells(IEnumerable<AbsoluteEntityCell> cells)
    {
        ThreadSafeSet<AbsoluteEntityCell> value = Value;
        foreach (AbsoluteEntityCell cell in cells)
        {
            value.Remove(cell);
        }
    }

    public bool HasCellLoaded(AbsoluteEntityCell cell) => Value.Contains(cell);

    public bool CanSee(Entity entity)
    {
        if (entity is WorldEntity worldEntity)
        {
            return worldEntity is GlobalRootEntity || HasCellLoaded(worldEntity.AbsoluteEntityCell) ||
                   outOfCellVisibleEntitiesProperty.Value.Contains(entity.Id);
        }

        return true;
    }

    /// <summary>
    ///     Returns a <b>new</b> list from the original set. To use the original set, use <see cref="AddCells" />,
    ///     <see cref="RemoveCells" /> and <see cref="HasCellLoaded" />.
    /// </summary>
    internal List<AbsoluteEntityCell> GetVisibleCells() => [.. Value];
}
