using System.Collections.Generic;
using System.Globalization;
using NitroxClient.Communication.Abstract;
using NitroxClient.Properties;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.Unity;
using NitroxModel.Packets;

namespace NitroxClient.GameLogic.FMOD
{
    public class FMODSystem
    {
        private readonly Dictionary<string, SoundData> assetWhitelist = new();
        private readonly IPacketSender packetSender;

        public FMODSystem(IPacketSender packetSender)
        {
            this.packetSender = packetSender;
            string soundsWhitelist = Resources.soundsWhitelist;
            if (string.IsNullOrWhiteSpace(soundsWhitelist))
            {
                Log.Error("[FMODSystem]: soundsWhitelist.csv is null or whitespace");
            }

            foreach (string entry in soundsWhitelist.Split('\n'))
            {
                if (string.IsNullOrWhiteSpace(entry) || entry.StartsWith("#") || entry.StartsWith(";"))
                {
                    continue;
                }

                string[] keyValuePair = entry.Split(';');
                if (bool.TryParse(keyValuePair[1], out bool isWhitelisted) &&
                    bool.TryParse(keyValuePair[2], out bool isGlobal) &&
                    float.TryParse(keyValuePair[3], NumberStyles.Any, CultureInfo.InvariantCulture, out float soundRadius))
                {
                    assetWhitelist.Add(keyValuePair[0], new SoundData(isWhitelisted, isGlobal, soundRadius));
                }
                else
                {
                    Log.Error($"[FMODSystem]: Error while parsing soundsWhitelist.csv: {entry}");
                }
            }
        }

        public bool IsWhitelisted(string path)
        {
            return TryGetSoundData(path, out SoundData soundData) && soundData.IsWhitelisted;
        }

        public bool IsWhitelisted(string path, out bool isGlobal, out float radius)
        {
            if (TryGetSoundData(path, out SoundData soundData))
            {
                isGlobal = soundData.IsGlobal;
                radius = soundData.SoundRadius;
                return soundData.IsWhitelisted;
            }

            isGlobal = false;
            radius = -1f;
            return false;
        }

        public bool TryGetSoundData(string path, out SoundData soundData)
        {
            if (assetWhitelist.TryGetValue(path, out soundData))
            {
                return true;
            }
            soundData = default;
            return false;
        }

        public void PlayAsset(string path, NitroxVector3 position, float volume, float radius, bool isGlobal)
        {
            packetSender.Send(new PlayFMODAsset(path, position, volume, radius, isGlobal));
        }

        public void PlayCustomEmitter(NitroxId id, string assetPath, bool play)
        {
            packetSender.Send(new PlayFMODCustomEmitter(id, assetPath, play));
        }

        public void PlayCustomLoopingEmitter(NitroxId id, string assetPath)
        {
            packetSender.Send(new PlayFMODCustomLoopingEmitter(id, assetPath));
        }

        public void PlayStudioEmitter(NitroxId id, string assetPath, bool play, bool allowFadeout)
        {
            packetSender.Send(new PlayFMODStudioEmitter(id, assetPath, play, allowFadeout));
        }

        public void PlayEventInstance(NitroxId id, string assetPath, bool play, NitroxVector3 position, float volume, float radius, bool isGlobal)
        {
            packetSender.Send(new PlayFMODEventInstance(id, play, assetPath, position, volume, radius, isGlobal));
        }

        public Dictionary<string, SoundData> SoundDataList => assetWhitelist;
    }
}
