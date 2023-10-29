using System.Linq;
using NitroxClient.Communication;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic.Entities.Metadata;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Abstract;

public abstract class NamedColoredMetadataProcessor<T> : GenericEntityMetadataProcessor<T> where T : NamedColoredMetadata
{
    public override void ProcessMetadata(GameObject gameObject, T metadata)
    {
        using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
        {
            SetNameAndColors(gameObject, metadata.Name, metadata.Colors);
        }
    }

    protected void SetNameAndColors(GameObject gameObject, string text, NitroxVector3[] nitroxColor)
    {
        SubName subName = gameObject.RequireComponent<SubName>();
        if (!string.IsNullOrEmpty(text))
        {
            subName.DeserializeName(text);
        }
        if (nitroxColor != null)
        {
            Vector3[] colors = nitroxColor.Select(c => c.ToUnity()).ToArray();
            subName.DeserializeColors(colors);
        }
    }
}
