using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Unity;
using NitroxModel_Subnautica.DataStructures.GameLogic;
using NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata;
using NitroxModel_Subnautica.DataStructures.Surrogates;
using NitroxServer.Serialization;
using ProtoBufNet.Meta;
using UnityEngine;

namespace NitroxServer_Subnautica.Serialization
{
    class SubnauticaServerProtoBufSerializer : ServerProtoBufSerializer
    {
        public SubnauticaServerProtoBufSerializer(params string[] assemblies) : base(assemblies)
        {
            RegisterHardCodedTypes();
        }

        // Register here all hard coded types, that come from NitroxModel-Subnautica or NitroxServer-Subnautica
        private void RegisterHardCodedTypes()
        {
            Model.Add(typeof(Light), true);
            Model.Add(typeof(BoxCollider), true);
            Model.Add(typeof(SphereCollider), true);
            Model.Add(typeof(MeshCollider), true);
            Model.Add(typeof(Vector3), false).SetSurrogate(typeof(Vector3Surrogate));
            Model.Add(typeof(NitroxVector3), false).SetSurrogate(typeof(Vector3Surrogate));
            Model.Add(typeof(Quaternion), false).SetSurrogate(typeof(QuaternionSurrogate));
            Model.Add(typeof(NitroxQuaternion), false).SetSurrogate(typeof(QuaternionSurrogate));
            Model.Add(typeof(Transform), false).SetSurrogate(typeof(NitroxTransform));
            Model.Add(typeof(GameObject), false).SetSurrogate(typeof(NitroxServer.UnityStubs.GameObject));

            MetaType vehicleModel = Model.Add(typeof(VehicleModel), false);
            vehicleModel.AddSubType(100, typeof(ExosuitModel));
            vehicleModel.AddSubType(200, typeof(SeamothModel));
            vehicleModel.AddSubType(300, typeof(CyclopsModel));
            vehicleModel.AddSubType(400, typeof(NeptuneRocketModel));

            MetaType movementData = Model.Add(typeof(VehicleMovementData), false);
            movementData.AddSubType(100, typeof(ExosuitMovementData));

            MetaType builderMetadata = Model.Add(typeof(BuilderMetadata), false);
            rotationData.AddSubType(50, typeof(CorridorBuilderMetadata));
            rotationData.AddSubType(60, typeof(MapRoomBuilderMetadata));
            rotationData.AddSubType(70, typeof(BaseModuleBuilderMetadata));
            rotationData.AddSubType(80, typeof(AnchoredFaceBuilderMetadata));
        }
    }
}
