using System;
using System.Collections.Generic;
using Nitrox.Client.GameLogic.PlayerModel.Abstract;
using Nitrox.Client.GameLogic.PlayerModel.ColorSwap.Strategy;
using Nitrox.Client.Unity.Helper;
using Nitrox.Model.Subnautica.DataStructures;
using UnityEngine;
using static Nitrox.Client.GameLogic.PlayerModel.PlayerEquipmentConstants;

namespace Nitrox.Client.GameLogic.PlayerModel.ColorSwap
{
    public class ScubaTankColorSwapManager : IColorSwapManager
    {
        public Action<ColorSwapAsyncOperation> CreateColorSwapTask(INitroxPlayer nitroxPlayer)
        {
            GameObject playerModel = nitroxPlayer.PlayerModel;
            Color playerColor = nitroxPlayer.PlayerSettings.PlayerColor.ToUnity();
            IColorSwapStrategy colorSwapStrategy = new HueSwapper(playerColor);

            SkinnedMeshRenderer scubaTankRenderer = playerModel.GetRenderer(SCUBA_TANK_GAME_OBJECT_NAME);
            scubaTankRenderer.material.ApplyClonedTexture();

            Color[] texturePixels = scubaTankRenderer.material.GetMainTexturePixels();

            return operation =>
            {
                HsvSwapper scubaTankFilter = new HsvSwapper(colorSwapStrategy);
                scubaTankFilter.SetHueRange(0f, 30f);

                scubaTankFilter.SwapColors(texturePixels);

                operation.UpdateIndex(SCUBA_TANK_INDEX_KEY, texturePixels);
            };
        }

        public void ApplyPlayerColor(Dictionary<string, Color[]> pixelIndex, INitroxPlayer nitroxPlayer)
        {
            Color[] scubaTankPixelIndexes = pixelIndex[SCUBA_TANK_INDEX_KEY];

            GameObject playerModel = nitroxPlayer.PlayerModel;
            SkinnedMeshRenderer scubaTankRenderer = playerModel.GetRenderer(SCUBA_TANK_GAME_OBJECT_NAME);

            scubaTankRenderer.material.UpdateMainTextureColors(scubaTankPixelIndexes);
            scubaTankRenderer.material.SetTexture("_MainTex", scubaTankRenderer.material.mainTexture);
            scubaTankRenderer.material.SetTexture("_SpecTex", scubaTankRenderer.material.mainTexture);
        }
    }
}
