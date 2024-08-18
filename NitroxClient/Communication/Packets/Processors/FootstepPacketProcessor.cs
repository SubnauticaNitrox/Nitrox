using System;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.FMOD;
using NitroxModel.DataStructures.Util;
using NitroxModel.GameLogic.FMOD;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class FootstepPacketProcessor : ClientPacketProcessor<FootstepPacket>
{
    private readonly PlayerManager remotePlayerManager;
    private readonly Lazy<FootstepSounds> localFootstepSounds = new(() => Player.mainObject.GetComponent<FootstepSounds>());
    private PARAMETER_ID fmodIndexSpeed = FMODUWE.invalidParameterId;
    private readonly float footstepAudioRadius; // To modify this value, modify the last value in the SoundWhitelist_Subnautica.csv file
    private const float FOOTSTEP_AUDIO_MAX_VOLUME = 0.5f;

    public FootstepPacketProcessor(PlayerManager remotePlayerManager, FMODWhitelist whitelist)
    {
        this.remotePlayerManager = remotePlayerManager;
        whitelist.TryGetSoundData("event:/player/footstep_precursor_base", out SoundData soundData);
        footstepAudioRadius = soundData.Radius;
    }

    public override void Process(FootstepPacket packet)
    {
        Optional<RemotePlayer> player = remotePlayerManager.Find(packet.PlayerID);
        if (player.HasValue)
        {
#if SUBNAUTICA
            FMODAsset asset = packet.AssetIndex switch
            {
                FootstepPacket.StepSounds.PRECURSOR => localFootstepSounds.Value.precursorInteriorSound,
                FootstepPacket.StepSounds.METAL => localFootstepSounds.Value.metalSound,
                FootstepPacket.StepSounds.LAND => localFootstepSounds.Value.landSound,
                _ => null
            };
#elif BELOWZERO
            FMODAsset asset = packet.AssetIndex switch
            {
                FootstepPacket.StepSounds.LAND => localFootstepSounds.Value.footStepSound,
                _ => null
            };
#endif
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
    }
}
