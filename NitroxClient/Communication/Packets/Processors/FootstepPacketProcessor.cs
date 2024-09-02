using NitroxClient.Communication.Packets.Processors.Abstract;
using NitroxModel.Packets;
using NitroxClient.GameLogic;
using FMOD;
using FMOD.Studio;
using FMODUnity;
using NitroxClient.GameLogic.FMOD;
using NitroxModel.Helper;
using System.Collections.Generic;
using System.Reflection.Emit;

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
        Log.Info("Processing footstep packet on client");
        var player = remotePlayerManager.Find(packet.playerID);
        if (!player.HasValue)
        {
            Log.Warn("Could not find player for footstep audio");
            return;
        }
        else
        {
            Log.Info($"Player found for footstep packet on client {player.Value.PlayerName}");
            FMODAsset asset;
            switch (packet.assetIndex)
            {
                case FootstepPacket.StepSounds.PRECURSOR_STEP_SOUND:
                    Log.Info("Precursor footstep sound identified");
                    asset = localFootstepSounds.precursorInteriorSound;
                    break;
                case FootstepPacket.StepSounds.METAL_STEP_SOUND:
                    Log.Info("Metal footstep sound identified");
                    asset = localFootstepSounds.metalSound;
                    break;
                case FootstepPacket.StepSounds.LAND_STEP_SOUND:
                    Log.Info("Land footstep sound identified");
                    asset = localFootstepSounds.landSound;
                    break;
                default:
                    asset = null;
                    Log.Warn("Weird asset index for footstep audio");
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
