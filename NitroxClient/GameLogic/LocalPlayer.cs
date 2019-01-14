using System.Collections.Generic;
using NitroxClient.Communication.Abstract;
using NitroxClient.GameLogic.PlayerModel.Abstract;
using NitroxClient.MonoBehaviours;
using NitroxClient.Unity.Helper;
using NitroxModel.DataStructures.GameLogic;
using NitroxModel.DataStructures.Util;
using NitroxModel.MultiplayerSession;
using NitroxModel.Packets;
using UnityEngine;
using UnityEngine.Rendering;

using static NitroxClient.GameLogic.PlayerModel.PlayerEquipmentConstants;

namespace NitroxClient.GameLogic
{
    public class LocalPlayer : ILocalNitroxPlayer
    {
        private readonly IMultiplayerSession multiplayerSession;
        private readonly IPacketSender packetSender;

        public GameObject Body { get; }
        public GameObject PlayerModel { get; }
        public GameObject BodyPrototype { get; }
        public string PlayerName => multiplayerSession.AuthenticationContext.Username;
        public PlayerSettings PlayerSettings => multiplayerSession.PlayerSettings;

        public LocalPlayer(IMultiplayerSession multiplayerSession, IPacketSender packetSender)
        {
            this.multiplayerSession = multiplayerSession;
            this.packetSender = packetSender;

            Body = Player.main.RequireGameObject("body");
            PlayerModel = Body.RequireGameObject("player_view");

            PreparePlayerModelMaterials();
            BodyPrototype = CreateBodyPrototype();
        }

        private void PreparePlayerModelMaterials()
        {
            PrepareFinMaterials();
            PrepareRadiationTankMaterials();
            PrepareRadiationHelmetMaterials();
            PrepareRadiationSuitVestMaterials();
            PrepareRadiationSuitMaterials();
            PrepareReinforcedSuitMaterials();
            PrepareScubaTankMaterials();
            PrepareRebreatherMaterials();
            PrepareStillSuitMaterials();
            PrepareDiveSuitMaterials();
        }

        private void PrepareDiveSuitMaterials()
        {
            SkinnedMeshRenderer diveSuitRenderer = PlayerModel.GetRenderer(DIVE_SUIT_GAME_OBJECT_NAME);
            diveSuitRenderer.material.ApplyClonedTexture();
            diveSuitRenderer.materials[1].ApplyClonedTexture();
        }

        private void PrepareRebreatherMaterials()
        {
            SkinnedMeshRenderer rebreatherRenderer = PlayerModel.GetRenderer(REBREATHER_GAME_OBJECT_NAME);
            rebreatherRenderer.material.ApplyClonedTexture();

            Shader marmosetShader = PlayerModel.GetRenderer(NORMAL_HEAD_GAME_OBJECT_NAME).material.shader;
            rebreatherRenderer.material.shader = marmosetShader;
            rebreatherRenderer.material.SetOverrideTag("RenderType", "TransparentAdditive");
            rebreatherRenderer.material.SetOverrideTag("Queue", "Deferred");
            rebreatherRenderer.material.shaderKeywords = new List<string>
                {"MARMO_ALPHA", "MARMO_PREMULT_ALPHA", "MARMO_SIMPLE_GLASS", "UWE_DITHERALPHA", "MARMO_SPECMAP", "WBOIT", "_NORMALMAP", "_ZWRITE_ON"}.ToArray();

            rebreatherRenderer.material.SetTexture("_MainTex", rebreatherRenderer.material.mainTexture);
            rebreatherRenderer.material.SetTexture("_SpecTex", rebreatherRenderer.material.mainTexture);
            rebreatherRenderer.material.SetTexture("_BumpMap", rebreatherRenderer.material.GetTexture("player_mask_01_normal"));

            rebreatherRenderer.materials[2].shader = marmosetShader;
            rebreatherRenderer.materials[2].shaderKeywords = new List<string>
                {"MARMO_SPECMAP", "_ZWRITE_ON"}.ToArray();
            rebreatherRenderer.materials[2].SetTexture("_MainTex", rebreatherRenderer.materials[2].mainTexture);
            rebreatherRenderer.materials[2].SetTexture("_SpecTex", rebreatherRenderer.materials[2].mainTexture);
        }

        private void PrepareStillSuitMaterials()
        {
            SkinnedMeshRenderer stillSuitRenderer = PlayerModel.GetRenderer(STILL_SUIT_GAME_OBJECT_NAME);
            stillSuitRenderer.material.ApplyClonedTexture();
            stillSuitRenderer.material.SetTexture("_MainTex", stillSuitRenderer.material.mainTexture);
            stillSuitRenderer.material.SetTexture("_SpecTex", stillSuitRenderer.material.mainTexture);

            stillSuitRenderer.materials[1].ApplyClonedTexture();
            stillSuitRenderer.materials[1].SetTexture("_MainTex", stillSuitRenderer.materials[1].mainTexture);
            stillSuitRenderer.materials[1].SetTexture("_SpecTex", stillSuitRenderer.materials[1].mainTexture);
        }

        private void PrepareScubaTankMaterials()
        {
            SkinnedMeshRenderer scubaTankRenderer = PlayerModel.GetRenderer(SCUBA_TANK_GAME_OBJECT_NAME);
            scubaTankRenderer.material.ApplyClonedTexture();
            scubaTankRenderer.material.SetTexture("_MainTex", scubaTankRenderer.material.mainTexture);
            scubaTankRenderer.material.SetTexture("_SpecTex", scubaTankRenderer.material.mainTexture);
        }

        private void PrepareReinforcedSuitMaterials()
        {
            SkinnedMeshRenderer reinforcedSuitRenderer = PlayerModel.GetRenderer(REINFORCED_SUIT_GAME_OBJECT_NAME);
            reinforcedSuitRenderer.material.ApplyClonedTexture();
            reinforcedSuitRenderer.material.SetTexture("_MainTex", reinforcedSuitRenderer.material.mainTexture);
            reinforcedSuitRenderer.material.SetTexture("_SpecTex", reinforcedSuitRenderer.material.mainTexture);

            reinforcedSuitRenderer.materials[1].ApplyClonedTexture();
            reinforcedSuitRenderer.materials[1].SetTexture("_MainTex", reinforcedSuitRenderer.materials[1].mainTexture);
            reinforcedSuitRenderer.materials[1].SetTexture("_SpecTex", reinforcedSuitRenderer.materials[1].mainTexture);

            SkinnedMeshRenderer reinforcedSuitGlovesRenderer = PlayerModel.GetRenderer(REINFORCED_GLOVES_GAME_OBJECT_NAME);
            reinforcedSuitGlovesRenderer.material.ApplyClonedTexture();
        }

        private void PrepareRadiationSuitMaterials()
        {
            SkinnedMeshRenderer radiationSuitRenderer = PlayerModel.GetRenderer(RADIATION_SUIT_GAME_OBJECT_NAME);

            radiationSuitRenderer.material.ApplyClonedTexture();
            radiationSuitRenderer.material.SetTexture("_MainText", radiationSuitRenderer.material.mainTexture);
            radiationSuitRenderer.material.SetTexture("_SpecTex", radiationSuitRenderer.material.mainTexture);
            radiationSuitRenderer.materials[1].SetTexture("_MainText", radiationSuitRenderer.material.mainTexture);
            radiationSuitRenderer.materials[1].SetTexture("_SpecTex", radiationSuitRenderer.material.mainTexture);
        }

        private void PrepareRadiationSuitVestMaterials()
        {
            SkinnedMeshRenderer radiationVestRenderer = PlayerModel.GetRenderer(RADIATION_SUIT_VEST_GAME_OBJECT_NAME);
            radiationVestRenderer.material.ApplyClonedTexture();
        }

        private void PrepareRadiationHelmetMaterials()
        {
            SkinnedMeshRenderer radiationHelmetRenderer = PlayerModel.GetRenderer(RADIATION_HELMET_GAME_OBJECT_NAME);
            radiationHelmetRenderer.material.ApplyClonedTexture();

            SkinnedMeshRenderer radiationNeckCoverRenderer = PlayerModel.GetRenderer(RADIATION_SUIT_NECK_CLASP_GAME_OBJECT_NAME);
            radiationNeckCoverRenderer.material.ApplyClonedTexture();
        }

        private void PrepareRadiationTankMaterials()
        {
            SkinnedMeshRenderer radiationTankRenderer = PlayerModel.GetRenderer(RADIATION_TANK_GAME_OBJECT_NAME).gameObject.GetComponent<SkinnedMeshRenderer>();
            radiationTankRenderer.material.ApplyClonedTexture();
            radiationTankRenderer.material.SetTexture("_MainTex", radiationTankRenderer.material.mainTexture);
            radiationTankRenderer.material.SetTexture("_SpecTex", radiationTankRenderer.material.mainTexture);
        }

        private void PrepareFinMaterials()
        {
            SkinnedMeshRenderer basicFinRenderer = PlayerModel.GetRenderer(FINS_GAME_OBJECT_NAME);
            basicFinRenderer.material.SetTexture("_MainTex", basicFinRenderer.material.mainTexture);
            basicFinRenderer.material.SetTexture("_SpecTex", basicFinRenderer.material.mainTexture);

            SkinnedMeshRenderer chargedFinRenderer = PlayerModel.GetRenderer(CHARGED_FINS_GAME_OBJECT_NAME);
            chargedFinRenderer.material.SetTexture("_MainTex", basicFinRenderer.material.mainTexture);
            chargedFinRenderer.material.SetTexture("_SpecTex", basicFinRenderer.material.mainTexture);

            SkinnedMeshRenderer glideFinRenderer = PlayerModel.GetRenderer(GLIDE_FINS_GAME_OBJECT_NAME);
            glideFinRenderer.material.SetTexture("_MainTex", basicFinRenderer.material.mainTexture);
            glideFinRenderer.material.SetTexture("_SpecTex", basicFinRenderer.material.mainTexture);
        }

        public void BroadcastStats(float oxygen, float maxOxygen, float health, float food, float water)
        {
            PlayerStats playerStats = new PlayerStats(multiplayerSession.Reservation.PlayerId, oxygen, maxOxygen, health, food, water);
            packetSender.Send(playerStats);
        }

        public void UpdateLocation(Vector3 location, Vector3 velocity, Quaternion bodyRotation, Quaternion aimingRotation, Optional<VehicleMovementData> vehicle)
        {
            Movement movement;

            if (vehicle.IsPresent())
            {
                movement = new VehicleMovement(multiplayerSession.Reservation.PlayerId, vehicle.Get());
            }
            else
            {
                movement = new Movement(multiplayerSession.Reservation.PlayerId, location, velocity, bodyRotation, aimingRotation);
            }

            packetSender.Send(movement);
        }

        public void AnimationChange(AnimChangeType type, AnimChangeState state)
        {
            AnimationChangeEvent animEvent = new AnimationChangeEvent(multiplayerSession.Reservation.PlayerId, (int)type, (int)state);
            packetSender.Send(animEvent);
        }

        public void BroadcastDeath(Vector3 deathPosition)
        {
            PlayerDeathEvent playerDeath = new PlayerDeathEvent(multiplayerSession.AuthenticationContext.Username, deathPosition);
            packetSender.Send(playerDeath);
        }

        public void BroadcastSubrootChange(Optional<string> subrootGuid)
        {
            SubRootChanged packet = new SubRootChanged(multiplayerSession.Reservation.PlayerId, subrootGuid);
            packetSender.Send(packet);
        }

        private GameObject CreateBodyPrototype()
        {
            GameObject prototype = Body;

            // Cheap fix for showing head, much easier since male_geo contains many different heads
            prototype.GetComponentInParent<Player>().head.shadowCastingMode = ShadowCastingMode.On;
            GameObject clone = Object.Instantiate(prototype);
            prototype.GetComponentInParent<Player>().head.shadowCastingMode = ShadowCastingMode.ShadowsOnly;

            return clone;
        }
    }
}
