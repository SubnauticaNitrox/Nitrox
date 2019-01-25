using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxServer.UnityStubs;

namespace NitroxServer.GameLogic
{
    public class Prefab
    {
        public string ClassId { get; private set; }
        public Transform Transform { get; private set; }
        public string Guid { get; set; }
        public int CellLevel { get; set; }
        public NitroxModel.DataStructures.TechType TechType { get; set; }

        public Prefab(string classId, Transform transform)
        {
            ClassId = classId;
            Transform = transform;
        }
    }
}
