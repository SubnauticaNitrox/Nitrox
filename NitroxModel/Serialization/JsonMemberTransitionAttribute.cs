using System;

namespace NitroxModel.Serialization;

/// <summary>
/// This <see cref="Attribute"/> is mimicking the official Newtonsoft JsonMember attribute.
/// We can't use Newtonsoft directly because it is conflicting with an pact version inside an Oculus dll.
/// We also can't use <see cref="System.Runtime.Serialization.DataMember"/> as it's not supported in Subnautica/Unity.
/// This workaround is hopefully obsolete once the modpocalypse update hits.
/// </summary>
public class JsonMemberTransitionAttribute : Attribute
{
}
