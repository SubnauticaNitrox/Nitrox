using NitroxModel.DataStructures.Unity;
using NitroxModel_Subnautica.DataStructures.Surrogates;
using NitroxServer.Serialization.Resources;
using UnityEngine;

namespace NitroxServer_Subnautica.Serialization.Resources;

public class SubnauticaProtoBufCellParser : ProtoBufCellParser
{
    public SubnauticaProtoBufCellParser(params string[] assemblies) : base(assemblies)
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
        Model.Add(typeof(GameObject), false).SetSurrogate(typeof(NitroxServer.Serialization.Resources.UnityStubs.GameObject));
    }
}
