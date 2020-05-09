using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel_Subnautica.Helper
{
    public class VehicleModelComparer : IComparer<VehicleModel>
    {
        public int Compare(VehicleModel x, VehicleModel y)
        {
            return (x.TechType.Enum() == y.TechType.Enum()) ? 0 : CompareTo(x.TechType.Enum(), y.TechType.Enum());
        }

        private int CompareTo(TechType x, TechType y)
        {
            return (x > y) ? -1 : 1;
        }
    }
}
