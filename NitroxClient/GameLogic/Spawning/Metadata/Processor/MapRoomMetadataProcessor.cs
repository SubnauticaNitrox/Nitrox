using Nitrox.Model.Logger;
using Nitrox.Model.Subnautica.DataStructures.GameLogic.Entities.Metadata;
using Nitrox.Model.Subnautica.Extensions;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication;
using NitroxClient.GameLogic.Spawning.Metadata.Processor.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.Spawning.Metadata.Processor;

public class MapRoomMetadataProcessor : EntityMetadataProcessor<MapRoomMetadata>
{
	public override void ProcessMetadata(GameObject gameObject, MapRoomMetadata metadata)
	{
		if (!gameObject.TryGetComponent<MapRoomFunctionality>(out var component))
		{
			Log.Error("Could not find MapRoomFunctionality on " + gameObject.name);
			return;
		}
		TechType techType = metadata.TypeToScan.ToUnity();
		bool flag = component.typeToScan != techType;
		using (PacketSuppressor<EntityMetadataUpdate>.Suppress())
		{
			if (flag)
			{
				component.StartScanning(techType);
			}
			component.numNodesScanned = metadata.NumNodesScanned;
		}
		if (flag)
		{
			uGUI_MapRoomScanner componentInChildren = component.GetComponentInChildren<uGUI_MapRoomScanner>(includeInactive: true);
			if ((bool)componentInChildren)
			{
				componentInChildren.UpdateGUIState();
			}
		}
	}
}


