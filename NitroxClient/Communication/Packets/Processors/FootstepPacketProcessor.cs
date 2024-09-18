using FMOD;
using FMOD.Studio;
using FMODUnity;
using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.FMOD;
using NitroxModel.Packets;

namespace NitroxClient.Communication.Packets.Processors;

public class FootstepPacketProcessor : ClientPacketProcessor<FootstepPacket>
{
    private readonly PlayerManager remotePlayerManager;
    private readonly FootstepSounds localFootstepSounds;
    private PARAMETER_ID fmodIndexSpeed = FMODUWE.invalidParameterId;
    private readonly float footstepAudioRadius = 20f;
    private readonly float footstepAudioMaxVolume = 0.5f;

    public FootstepPacketProcessor(PlayerManager remotePlayerManager)
    {
        this.remotePlayerManager = remotePlayerManager;
        this.localFootstepSounds = Player.mainObject.GetComponent<FootstepSounds>();
    }

    public override void Process(FootstepPacket packet)
    {
        var player = remotePlayerManager.Find(packet.playerID);
        if (!player.HasValue)
        {
            return;
        }
        else
        {
            FMODAsset asset;
            switch (packet.assetIndex)
            {
                case FootstepPacket.StepSounds.PRECURSOR_STEP_SOUND:
                    asset = localFootstepSounds.precursorInteriorSound;
                    break;
                case FootstepPacket.StepSounds.METAL_STEP_SOUND:
                    asset = localFootstepSounds.metalSound;
                    break;
                case FootstepPacket.StepSounds.LAND_STEP_SOUND:
                    asset = localFootstepSounds.landSound;
                    break;
                default:
                    asset = null;
                    break;
            }
            EventInstance @event = FMODUWE.GetEvent(asset);
            if (@event.isValid())
            {
                if (FMODUWE.IsInvalidParameterId(fmodIndexSpeed))
                {
                    fmodIndexSpeed = FMODUWE.GetEventInstanceParameterIndex(@event, "speed");
                }
                ATTRIBUTES_3D attributes = player.Value.Body.To3DAttributes();
                @event.set3DAttributes(attributes);
                @event.setParameterValueByIndex(fmodIndexSpeed, player.Value.AnimationController.Velocity.magnitude);
                @event.setVolume(FMODSystem.CalculateVolume(Player.mainObject.transform.position, player.Value.Body.transform.position, footstepAudioRadius, footstepAudioMaxVolume));
                @event.start();
                @event.release();
            }
        }
    }
}
