using NitroxModel.DataStructures.GameLogic;
using NitroxServer.UnityStubs;

namespace NitroxServer.GameLogic.Entities.Spawning
{
    public class EntitySpawnPoint
    {
        public AbsoluteEntityCell AbsoluteEntityCell { get; private set; }
        public UnityEngine.Vector3 Position { get; private set; }
        public UnityEngine.Quaternion Rotation { get; private set; }
        public string ClassId { get; private set; }
        public string Guid { get; private set; }
        public BiomeType BiomeType { get; private set; }
        public float Density { get; private set; }
        public bool CanSpawnCreature { get; private set; }

        public static EntitySpawnPoint From(Int3 batchId, GameObject go, CellManager.CellHeader cellHeader)
        {
            // Why is this not a constructor?
            EntitySpawnPoint esp = new EntitySpawnPoint
            {
                AbsoluteEntityCell = new AbsoluteEntityCell(batchId, cellHeader.cellId, cellHeader.level),
                ClassId = go.ClassId,
                Density = 1
            };

            EntitySlot entitySlot = go.GetComponent<EntitySlot>();

            if (!ReferenceEquals(entitySlot, null))
            {
                esp.BiomeType = entitySlot.biomeType;
                esp.Density = entitySlot.density;
                esp.CanSpawnCreature = entitySlot.IsCreatureSlot();
            }

            esp.Rotation = go.GetComponent<Transform>().Rotation;

            UnityEngine.Vector3 localPosition = go.GetComponent<Transform>().Position;
            esp.Position = esp.AbsoluteEntityCell.Center + localPosition;

            return esp;
        }

        public override string ToString() => $"[EntitySpawnPoint - {AbsoluteEntityCell}, Position: {Position}, Rotation: {Rotation}, ClassId: {ClassId}, Guid: {Guid}, BiomeType: {BiomeType}, Density: {Density}, CanSpawnCreature: {CanSpawnCreature}]";
    }
}
