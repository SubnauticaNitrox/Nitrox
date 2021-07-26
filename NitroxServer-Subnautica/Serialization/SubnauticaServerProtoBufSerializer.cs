using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation;
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
            vehicleModel.AddSubType(200, typeof(SeamothModel));
            vehicleModel.AddSubType(300, typeof(CyclopsModel));

            MetaType movementData = Model.Add(typeof(VehicleMovementData), false);
            movementData.AddSubType(100, typeof(ExosuitMovementData));
            
            MetaType rotationData = Model.Add(typeof(RotationMetadata), false);
            rotationData.AddSubType(50, typeof(CorridorRotationMetadata));
            rotationData.AddSubType(60, typeof(MapRoomRotationMetadata));
            rotationData.AddSubType(70, typeof(BaseModuleRotationMetadata));
        }
    }
}
