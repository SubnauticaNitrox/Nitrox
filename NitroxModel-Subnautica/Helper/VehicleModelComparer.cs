using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxModel.DataStructures.GameLogic;

namespace NitroxModel_Subnautica.Helper
{
    public class VehicleModelComparer : IComparer<VehicleModel>
    {
        public int Compare(VehicleModel x, VehicleModel y)
        {
            if(x.TechType.Enum() == y.TechType.Enum())
            {
                return x.Guid.CompareTo(y.Guid);
            }
            else
            {
                return CompareTo(x.TechType.Enum(), y.TechType.Enum());
            }
        }
        private int CompareTo(TechType x, TechType y)
        {
            if(x > y)
            {
                return -1;
            } else
            {
                return 1;
            }
        }
    }
}
