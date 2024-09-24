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
    private readonly FootstepSounds localFootstepSounds;
    private PARAMETER_ID fmodIndexSpeed = FMODUWE.invalidParameterId;
    private readonly float footstepAudioRadius; // To modify this value, modify the last value in the SoundWhitelist_Subnautica.csv file
    private const float footstepAudioMaxVolume = 0.5f;

    public FootstepPacketProcessor(PlayerManager remotePlayerManager, FMODWhitelist whitelist)
    {
        this.remotePlayerManager = remotePlayerManager;
        localFootstepSounds = Player.mainObject.GetComponent<FootstepSounds>();
        whitelist.TryGetSoundData("event:/player/footstep_precursor_base", out SoundData soundData);
        footstepAudioRadius = soundData.Radius;
    }

    public override void Process(FootstepPacket packet)
    {
        Optional<RemotePlayer> player = remotePlayerManager.Find(packet.PlayerID);
        if (player.HasValue)
        {
            FMODAsset asset = packet.AssetIndex switch
            {
                FootstepPacket.StepSounds.PRECURSOR_STEP_SOUND => localFootstepSounds.precursorInteriorSound,
                FootstepPacket.StepSounds.METAL_STEP_SOUND => localFootstepSounds.metalSound,
                FootstepPacket.StepSounds.LAND_STEP_SOUND => localFootstepSounds.landSound,
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
                evt.setVolume(FMODSystem.CalculateVolume(Player.mainObject.transform.position, player.Value.Body.transform.position, footstepAudioRadius, footstepAudioMaxVolume));
                evt.start();
                evt.release();
            }
        }
    }
}
