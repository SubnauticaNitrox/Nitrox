using System.Collections.Generic;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Logger;
using NitroxModel.Packets;
using NitroxModel_Subnautica.DataStructures;
using UnityEngine;

namespace NitroxClient.Communication.Packets.Processors
{
    public class PlayFMODAssetProcessor : ClientPacketProcessor<PlayFMODAsset>
    {
        private readonly Dictionary<string, FMODAsset> assetsById = new Dictionary<string, FMODAsset>();

        public PlayFMODAssetProcessor()
        {
            foreach (FMODAsset asset in Resources.FindObjectsOfTypeAll<FMODAsset>())
            {
                if (!assetsById.ContainsKey(asset.id))
                {
                    assetsById.Add(asset.id, asset);
                }
            }
        }

        public override void Process(PlayFMODAsset packet)
        {
            if (assetsById.TryGetValue(packet.Id, out FMODAsset fmodAsset))
            {
                FMODUWE.PlayOneShot(fmodAsset, packet.Position.ToUnity(), packet.Volume);
            }
            else
            {
                Log.Error($"assetsById[{assetsById.Count}] does not contain {packet.Id}.");
            }
        }
    }
}
