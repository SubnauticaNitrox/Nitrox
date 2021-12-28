using ZeroFormatter;

namespace NitroxModel.DataStructures
{
    [ZeroFormattable]
    public class SimulatedEntity
    {
        [Index(0)]
        public virtual bool ChangesPosition { get; protected set; }
        [Index(1)]
        public virtual NitroxId Id { get; protected set; }
        [Index(2)]
        public virtual ushort PlayerId { get; protected set; }
        [Index(3)]
        public virtual SimulationLockType LockType { get; protected set; }

        public SimulatedEntity() { }

        public SimulatedEntity(NitroxId id, ushort playerId, bool changesPosition, SimulationLockType lockType)
        {
            Id = id;
            PlayerId = playerId;
            ChangesPosition = changesPosition;
            LockType = lockType;
        }

        public override string ToString()
        {
            return $"[SimulatedEntity Id: '{Id}' PlayerId: {PlayerId} IsEntity: {ChangesPosition} LockType: {LockType}]";
        }
    }
}
