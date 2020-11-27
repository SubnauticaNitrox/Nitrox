using System;
using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.Logger;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic
{
    public class FMODSystem
    {
        private readonly Dictionary<string, bool[]> assetWhitelist = new Dictionary<string, bool[]>();

        private readonly IPacketSender packetSender;

        public FMODSystem(IPacketSender packetSender)
        {
            this.packetSender = packetSender;

            foreach (string entry in Properties.Resources.soundsWhitelist.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(entry) || entry.StartsWith("#"))
                {
                    continue;
                }

                try
                {
                    string[] keyValuePair = entry.Split(';');
                    if (bool.TryParse(keyValuePair[1], out bool isWhitelisted) && bool.TryParse(keyValuePair[2], out bool isGlobal))
                    {
                        assetWhitelist.Add(keyValuePair[0], new[] { isWhitelisted, isGlobal });
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, $"[FMODSystem]: Error while parsing soundsWhitelist.csv: {entry}");
                    throw;
                }
            }
        }

        public bool IsWhitelisted(string path)
        {
            return assetWhitelist.TryGetValue(path, out bool[] values) && values[0];
        }

        public bool IsWhitelisted(string path, out bool isGlobal)
        {
            bool isWhitelisted = assetWhitelist.TryGetValue(path, out bool[] values) && values[0];
            isGlobal = values[1];
            return isWhitelisted;
        }

        public void PlayFMODAsset(string path, NitroxVector3 position, float radius, bool isGlobal)
        {
            packetSender.Send(new PlayFMODAsset(path, position, radius, isGlobal));
        }

        public void PlayFMOD_CustomEmitter(NitroxId id, int componentNumber, bool play)
        {
            packetSender.Send(new PlayFMOD_CustomEmitter(id, componentNumber, play));
        }
    }
}
