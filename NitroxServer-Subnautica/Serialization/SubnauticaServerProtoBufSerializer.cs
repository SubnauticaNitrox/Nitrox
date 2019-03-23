using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxServer.Serialization;
using ProtoBufNet.Meta;

namespace NitroxServer_Subnautica.Serialization
{
    class SubnauticaServerProtobufSerializer : ServerProtobufSerializer
    {
        public SubnauticaServerProtobufSerializer(params string[] assemblies) : base(assemblies)
        {
            RegisterHardCodedTypes();
        }

        // Register here all hard coded types, that come from NitroxModel-Subnautica or NitroxServer-Subnautica
        private void RegisterHardCodedTypes()
        {
            MetaType vehicleModel = Model.Add(typeof(VehicleModel), false);
            vehicleModel.AddSubType(100, typeof(ExosuitModel));

            MetaType movementData = Model.Add(typeof(VehicleMovementData), false);
            movementData.AddSubType(100, typeof(ExosuitMovementData));
        }
    }
}
