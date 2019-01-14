using System;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.GameLogic.PlayerModel.Abstract;
using UnityEngine;

namespace NitroxClient.GameLogic.PlayerModel
{
    /// <summary>
    ///     Stores an index of the pixels that need to be replaced for each texture associated with a piece of wearable player
    ///     equipment
    /// </summary>
    public class PlayerColorApplicatorPixelIndex : IPixelIndexer
    {
        private readonly Dictionary<string, List<int>> texturePixelIndexes = new Dictionary<string, List<int>>();

        public List<int> this[string indexKey] => texturePixelIndexes[indexKey];

        public IndexPixelsAsyncOperation IndexPixelsAsync(GameObject playerModel)
        {
            Action<IndexPixelsAsyncOperation> indexFinTexturesTask = CreateFinTextureIndexer(playerModel);
            Action<IndexPixelsAsyncOperation> indexRadiationTankTexturesTask = CreateRadiationTankTextureIndexer(playerModel);
            Action<IndexPixelsAsyncOperation> indexRadiationHelmetTexturesTask = CreateRadiationHelmetTextureIndexer(playerModel);
            Action<IndexPixelsAsyncOperation> indexRadiationSuitVestTexturesTask = CreateRadiationSuitVestTextureIndexer(playerModel);
            Action<IndexPixelsAsyncOperation> indexRadiationSuitTexturesTask = CreateRadiationSuitTextureIndexer(playerModel);
            Action<IndexPixelsAsyncOperation> indexReinforcedSuitTexturesTask = CreateReinforcedSuitTextureIndexer(playerModel);
            Action<IndexPixelsAsyncOperation> indexScubaTankTexturesTask = CreateScubaTankTextureIndexer(playerModel);
            Action<IndexPixelsAsyncOperation> indexRebreatherTexturesTask = CreateRebreatherTextureIndexer(playerModel);
            Action<IndexPixelsAsyncOperation> indexStillSuitTexturesTask = CreateStillSuitTextureIndexer(playerModel);
            Action<IndexPixelsAsyncOperation> indexDiveSuitTexturesTask = CreateDiveSuitTextureIndexer(playerModel);

            IndexPixelsAsyncOperation operation = new IndexPixelsAsyncOperation(texturePixelIndexes);

            return operation.Start(indexFinTexturesTask,
                indexRadiationTankTexturesTask,
                indexRadiationHelmetTexturesTask,
                indexRadiationSuitVestTexturesTask,
                indexRadiationSuitTexturesTask,
                indexReinforcedSuitTexturesTask,
                indexReinforcedSuitTexturesTask,
                indexScubaTankTexturesTask,
                indexRebreatherTexturesTask,
                indexStillSuitTexturesTask,
                indexDiveSuitTexturesTask);
        }

        public Action<IndexPixelsAsyncOperation> CreateDiveSuitTextureIndexer(GameObject playerModel)
        {
            SkinnedMeshRenderer diveSuitRenderer = playerModel.GetRenderer(PlayerEquipmentConstants.DIVE_SUIT_GAME_OBJECT_NAME);
            Color[] texturePixels = diveSuitRenderer.material.GetMainTexturePixels();

            return operation =>
            {
                HsvColorFilter diveSuitFilter = new HsvColorFilter();
                diveSuitFilter.SetHueRange(5f, 45f);
                diveSuitFilter.SetSaturationRange(50f, 100f);
                diveSuitFilter.SetVibrancyRange(15f, 100f);

                List<int> pixelIndexes = diveSuitFilter.GetPixelIndexes(texturePixels).ToList();

                operation.UpdateIndex(PlayerEquipmentConstants.DIVE_SUIT_INDEX_KEY, pixelIndexes);
            };
        }

        private Action<IndexPixelsAsyncOperation> CreateStillSuitTextureIndexer(GameObject playerModel)
        {
            SkinnedMeshRenderer stillSuitRenderer = playerModel.GetRenderer(PlayerEquipmentConstants.STILL_SUIT_GAME_OBJECT_NAME);
            Color[] texturePixels = stillSuitRenderer.material.GetMainTexturePixels();

            return operation =>
            {
                HsvColorFilter stillSuitFilter = new HsvColorFilter();
                stillSuitFilter.SetHueRange(0f, 75f);

                List<int> pixelIndexes = stillSuitFilter.GetPixelIndexes(texturePixels).ToList();

                operation.UpdateIndex(PlayerEquipmentConstants.STILL_SUIT_INDEX_KEY, pixelIndexes);
            };
        }

        private Action<IndexPixelsAsyncOperation> CreateRebreatherTextureIndexer(GameObject playerModel)
        {
            SkinnedMeshRenderer rebreatherRenderer = playerModel.GetRenderer(PlayerEquipmentConstants.REBREATHER_GAME_OBJECT_NAME);
            Color[] texturePixels = rebreatherRenderer.material.GetMainTexturePixels();

            return operation =>
            {
                HsvColorFilter rebreatherFilter = new HsvColorFilter();
                rebreatherFilter.SetHueRange(0f, 25f);

                List<int> pixelIndexes = rebreatherFilter.GetPixelIndexes(texturePixels).ToList();

                operation.UpdateIndex(PlayerEquipmentConstants.REBREATHER_INDEX_KEY, pixelIndexes);
            };
        }

        private Action<IndexPixelsAsyncOperation> CreateScubaTankTextureIndexer(GameObject playerModel)
        {
            SkinnedMeshRenderer scubaTankRenderer = playerModel.GetRenderer(PlayerEquipmentConstants.SCUBA_TANK_GAME_OBJECT_NAME);
            Color[] texturePixels = scubaTankRenderer.material.GetMainTexturePixels();

            return operation =>
            {
                HsvColorFilter scubaTankFilter = new HsvColorFilter();
                scubaTankFilter.SetHueRange(0f, 30f);

                List<int> pixelIndexes = scubaTankFilter.GetPixelIndexes(texturePixels).ToList();

                operation.UpdateIndex(PlayerEquipmentConstants.SCUBA_TANK_INDEX_KEY, pixelIndexes);
            };
        }

        private Action<IndexPixelsAsyncOperation> CreateReinforcedSuitTextureIndexer(GameObject playerModel)
        {
            SkinnedMeshRenderer reinforcedSuitRenderer = playerModel.GetRenderer(PlayerEquipmentConstants.REINFORCED_SUIT_GAME_OBJECT_NAME);
            Color[] texturePixels = reinforcedSuitRenderer.material.GetMainTexturePixels();

            return operation =>
            {
                HsvColorFilter reinforcedSuitFilter = new HsvColorFilter();
                reinforcedSuitFilter.SetHueRange(0f, 20f);
                reinforcedSuitFilter.SetSaturationRange(45f, 100f);

                List<int> pixelIndexes = reinforcedSuitFilter.GetPixelIndexes(texturePixels).ToList();

                operation.UpdateIndex(PlayerEquipmentConstants.REINFORCED_SUIT_INDEX_KEY, pixelIndexes);
            };
        }

        private Action<IndexPixelsAsyncOperation> CreateRadiationSuitTextureIndexer(GameObject playerModel)
        {
            SkinnedMeshRenderer radiationVestRenderer = playerModel.GetRenderer(PlayerEquipmentConstants.RADIATION_SUIT_GAME_OBJECT_NAME);

            Color[] legPixelBlock = radiationVestRenderer.material.GetMainTexturePixelBlock(700, 484, 130, 155);
            Color[] feetPixelBlock = radiationVestRenderer.material.GetMainTexturePixelBlock(525, 324, 250, 325);
            Color[] beltPixelBlock = radiationVestRenderer.material.GetMainTexturePixelBlock(570, 0, 454, 1024);

            return operation =>
            {
                HsvColorFilter radiationSuitLegFilter = new HsvColorFilter();
                radiationSuitLegFilter.SetSaturationRange(0f, 35f);
                radiationSuitLegFilter.SetVibrancyRange(40f, 100f);

                HsvColorFilter radiationSuitArmAndFeetFilter = new HsvColorFilter();
                radiationSuitArmAndFeetFilter.SetHueRange(0f, 100f);
                radiationSuitArmAndFeetFilter.SetVibrancyRange(30f, 100f);

                HsvColorFilter radiationSuitBeltFilter = new HsvColorFilter();
                radiationSuitBeltFilter.SetVibrancyRange(3f, 100f);
                radiationSuitBeltFilter.SetHueRange(0f, 90f);

                List<int> legPixelIndexes = radiationSuitLegFilter.GetPixelIndexes(legPixelBlock).ToList();
                List<int> feetPixelIndexes = radiationSuitArmAndFeetFilter.GetPixelIndexes(feetPixelBlock).ToList();
                List<int> beltPixelIndexes = radiationSuitBeltFilter.GetPixelIndexes(beltPixelBlock).ToList();

                operation.UpdateIndex(PlayerEquipmentConstants.RADIATION_SUIT_LEG_INDEX_KEY, legPixelIndexes);
                operation.UpdateIndex(PlayerEquipmentConstants.RADIATION_SUIT_FEET_INDEX_KEY, feetPixelIndexes);
                operation.UpdateIndex(PlayerEquipmentConstants.RADIATION_SUIT_BELT_INDEX_KEY, beltPixelIndexes);
            };
        }

        private Action<IndexPixelsAsyncOperation> CreateRadiationSuitVestTextureIndexer(GameObject playerModel)
        {
            SkinnedMeshRenderer radiationVestRenderer = playerModel.GetRenderer(PlayerEquipmentConstants.RADIATION_SUIT_VEST_GAME_OBJECT_NAME);
            Color[] texturePixels = radiationVestRenderer.material.GetMainTexturePixels();

            return operation =>
            {
                HsvColorFilter radiationSuitVestFilter = new HsvColorFilter();
                radiationSuitVestFilter.SetSaturationRange(0f, 35f);
                radiationSuitVestFilter.SetVibrancyRange(12f, 100f);

                List<int> pixelIndexes = radiationSuitVestFilter.GetPixelIndexes(texturePixels).ToList();

                operation.UpdateIndex(PlayerEquipmentConstants.RADIATION_SUIT_VEST_INDEX_KEY, pixelIndexes);
            };
        }

        private Action<IndexPixelsAsyncOperation> CreateRadiationHelmetTextureIndexer(GameObject playerModel)
        {
            SkinnedMeshRenderer radiationHelmetRenderer = playerModel.GetRenderer(PlayerEquipmentConstants.RADIATION_HELMET_GAME_OBJECT_NAME);
            SkinnedMeshRenderer radiationSuitNeckClaspRenderer = playerModel.GetRenderer(PlayerEquipmentConstants.RADIATION_SUIT_NECK_CLASP_GAME_OBJECT_NAME);

            Color[] helmetPixels = radiationHelmetRenderer.material.GetMainTexturePixels();
            Color[] neckClaspPixels = radiationSuitNeckClaspRenderer.material.GetMainTexturePixels();

            return operation =>
            {
                HsvColorFilter radiationHelmetFilter = new HsvColorFilter();
                radiationHelmetFilter.SetSaturationRange(0f, 35f);
                radiationHelmetFilter.SetVibrancyRange(30f, 100f);

                List<int> helmetPixelIndexes = radiationHelmetFilter.GetPixelIndexes(helmetPixels).ToList();
                List<int> neckClaspPixelIndexes = radiationHelmetFilter.GetPixelIndexes(neckClaspPixels).ToList();

                operation.UpdateIndex(PlayerEquipmentConstants.RADIATION_HELMET_INDEX_KEY, helmetPixelIndexes);
                operation.UpdateIndex(PlayerEquipmentConstants.RADIATION_SUIT_NECK_CLASP_INDEX_KEY, neckClaspPixelIndexes);
            };
        }

        private Action<IndexPixelsAsyncOperation> CreateRadiationTankTextureIndexer(GameObject playerModel)
        {
            SkinnedMeshRenderer radiationTankRenderer = playerModel.GetRenderer(PlayerEquipmentConstants.RADIATION_TANK_GAME_OBJECT_NAME);
            Color[] texturePixels = radiationTankRenderer.material.GetMainTexturePixels();

            return operation =>
            {
                HsvColorFilter radiationTankFilter = new HsvColorFilter();
                radiationTankFilter.SetHueRange(0f, 85f);

                List<int> pixelIndexes = radiationTankFilter.GetPixelIndexes(texturePixels).ToList();
                operation.UpdateIndex(PlayerEquipmentConstants.RADIATION_SUIT_TANK_INDEX_KEY, pixelIndexes);
            };
        }

        private Action<IndexPixelsAsyncOperation> CreateFinTextureIndexer(GameObject playerModel)
        {
            SkinnedMeshRenderer basicFinRenderer = playerModel.GetRenderer(PlayerEquipmentConstants.FINS_GAME_OBJECT_NAME);
            Color[] texturePixels = basicFinRenderer.material.GetMainTexturePixels();

            return operation =>
            {
                HsvColorFilter finFilter = new HsvColorFilter();
                finFilter.SetHueRange(0f, 35f);

                List<int> pixelIndexes = finFilter.GetPixelIndexes(texturePixels).ToList();
                operation.UpdateIndex(PlayerEquipmentConstants.FINS_INDEX_EY, pixelIndexes);
            };
        }
    }
}
