using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NitroxModel.DataStructures.ServerModel
{
    public class VehicleModel
    {
        public String TechType { get; set; }
        public String Guid { get; set; }
        public Quaternion Rotation { get; set; }

        public VehicleModel(String techType, String guid, Quaternion rotation)
        {
            this.TechType = techType;
            this.Guid = guid;
            this.Rotation = rotation;
        }
    }
}
