using System;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using Nitrox.Model.DataStructures;
using Nitrox.Model.GameLogic.FMOD;
using Nitrox.Model.Subnautica.Packets;
using NitroxClient.Communication.Packets.Processors.Core;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.FMOD;

namespace NitroxClient.Communication.Packets.Processors;

internal sealed class FootstepPacketProcessor : IClientPacketProcessor<FootstepPacket>
{
    private const float FOOTSTEP_AUDIO_MAX_VOLUME = 0.5f;
    private readonly float footstepAudioRadius; // To modify this value, modify the last value in the SoundWhitelist_Subnautica.csv file
    private readonly Lazy<FootstepSounds> localFootstepSounds = new(() => Player.mainObject.GetComponent<FootstepSounds>());
    private readonly PlayerManager remotePlayerManager;
    private PARAMETER_ID fmodIndexSpeed = FMODUWE.invalidParameterId;

    public FootstepPacketProcessor(PlayerManager remotePlayerManager, FMODWhitelist whitelist)
    {
        this.remotePlayerManager = remotePlayerManager;
        whitelist.TryGetSoundData("event:/player/footstep_precursor_base", out SoundData soundData);
        footstepAudioRadius = soundData.Radius;
    }

    public Task Process(ClientProcessorContext context, FootstepPacket packet)
    {
        Optional<RemotePlayer> player = remotePlayerManager.Find(packet.SessionId);
        if (player.HasValue)
        {
            FMODAsset asset = packet.AssetIndex switch
            {
                FootstepPacket.StepSounds.PRECURSOR => localFootstepSounds.Value.precursorInteriorSound,
                FootstepPacket.StepSounds.METAL => localFootstepSounds.Value.metalSound,
                FootstepPacket.StepSounds.LAND => localFootstepSounds.Value.landSound,
                _ => null
            };
            EventInstance evt = FMODUWE.GetEvent(asset);
            if (evt.isValid())
            {
                if (FMODUWE.IsInvalidParameterId(fmodIndexSpeed))
                {
                    fmodIndexSpeed = FMODUWE.GetEventInstanceParameterIndex(evt, "speed");
                }
                ATTRIBUTES_3D attributes = player.Value.Body.To3DAttributes();
                evt.set3DAttributes(attributes);
                evt.setParameterValueByIndex(fmodIndexSpeed, player.Value.AnimationController.Velocity.magnitude);
                evt.setVolume(FMODSystem.CalculateVolume(Player.mainObject.transform.position, player.Value.Body.transform.position, footstepAudioRadius, FOOTSTEP_AUDIO_MAX_VOLUME));
                evt.start();
                evt.release();
            }
        }
        return Task.CompletedTask;
    }
}
