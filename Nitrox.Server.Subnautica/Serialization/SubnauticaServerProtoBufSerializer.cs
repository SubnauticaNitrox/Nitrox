using Nitrox.Model.DataStructures.GameLogic;
using Nitrox.Model.DataStructures.GameLogic.Buildings.Rotation;
using Nitrox.Model.Subnautica.DataStructures.GameLogic;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata;
using Nitrox.Model.Subnautica.DataStructures.Surrogates;
using Nitrox.Server.Serialization;
using ProtoBufNet.Meta;
using UnityEngine;

namespace Nitrox.Server.Subnautica.Serialization
{
    class SubnauticaServerProtoBufSerializer : ServerProtoBufSerializer
    {
        public SubnauticaServerProtoBufSerializer(params string[] assemblies) : base(assemblies)
        {
            RegisterHardCodedTypes();
        }

        // Register here all hard coded types, that come from Nitrox.Model.Subnautica or Nitrox.Server.Subnautica
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
            Model.Add(typeof(GameObject), false).SetSurrogate(typeof(UnityStubs.GameObject));

            MetaType vehicleModel = Model.Add(typeof(VehicleModel), false);
            vehicleModel.AddSubType(100, typeof(ExosuitModel));
            vehicleModel.AddSubType(200, typeof(SeamothModel));
            vehicleModel.AddSubType(300, typeof(CyclopsModel));
            vehicleModel.AddSubType(400, typeof(NeptuneRocketModel));

            MetaType movementData = Model.Add(typeof(VehicleMovementData), false);
            movementData.AddSubType(100, typeof(ExosuitMovementData));

            MetaType rotationData = Model.Add(typeof(RotationMetadata), false);
            rotationData.AddSubType(50, typeof(CorridorRotationMetadata));
            rotationData.AddSubType(60, typeof(MapRoomRotationMetadata));
            rotationData.AddSubType(70, typeof(BaseModuleRotationMetadata));
            rotationData.AddSubType(80, typeof(AnchoredFaceRotationMetadata));
        }
    }
}
