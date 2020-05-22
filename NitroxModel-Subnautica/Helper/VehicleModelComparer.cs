using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel_Subnautica.DataStructures;

namespace NitroxModel_Subnautica.Helper
{
    public class VehicleModelComparer : IComparer<VehicleModel>
    {
        public int Compare(VehicleModel x, VehicleModel y)
        {
            return (x.TechType.ToUnity() == y.TechType.ToUnity()) ? 0 : CompareTo(x.TechType.ToUnity(), y.TechType.ToUnity());
        }

        private int CompareTo(TechType x, TechType y)
        {
            return (x > y) ? -1 : 1;
        }
    }
}
