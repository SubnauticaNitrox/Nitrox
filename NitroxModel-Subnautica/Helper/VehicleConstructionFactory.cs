using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxModel_Subnautica.Helper
{
    public static class VehicleConstructionFactory
    {
        public static ConstructorBeginCrafting BuildFrom(VehicleModel vehicleModel, NitroxId constructorId = null, float duration = 3f)
        {
            return new ConstructorBeginCrafting(
                    constructorId ?? new NitroxId(),
                    vehicleModel.Id,
                    vehicleModel.TechType,
                    duration,
                    vehicleModel.InteractiveChildIdentifiers.ToList(),
                    vehicleModel.Position,
                    vehicleModel.Rotation,
                    vehicleModel.Name,
                    vehicleModel.HSB,
                    vehicleModel.Health
                );
        }
    }
}
