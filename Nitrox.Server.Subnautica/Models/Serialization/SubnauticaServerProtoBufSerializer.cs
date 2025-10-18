using Nitrox.Model.DataStructures.Unity;
using Nitrox.Model.Subnautica.DataStructures.Surrogates;
using UnityEngine;

namespace Nitrox.Server.Subnautica.Models.Serialization
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
            Model.Add(typeof(GameObject), false).SetSurrogate(typeof(Models.UnityStubs.GameObject));
        }
    }
}
