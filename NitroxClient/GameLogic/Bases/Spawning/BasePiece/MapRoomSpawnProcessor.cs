using System;
using NitroxClient.MonoBehaviours;
using NitroxModel.Core;
using NitroxModel.DataStructures;
using NitroxModel.DataStructures.GameLogic.Buildings.Metadata;
using NitroxModel.Logger;
using UnityEngine;

namespace NitroxClient.GameLogic.Bases.Spawning.BasePiece
{
    /*
     * When a map room is created, two objects are added to the base: a BaseMapRoom piece that is
     * entirely geometry and a MapRoomFunctionality game object in the base root.  This class sets
     * deterministic ids on non-geometry pieces, such as the map module upgrade storage area, so 
     * they can stay in sync during player interactions.
     */
    public class MapRoomSpawnProcessor : BasePieceSpawnProcessor
    {
        protected override TechType[] ApplicableTechTypes { get; } =
        {
            TechType.BaseMapRoom
        };
        private static Building building;

        protected override void SpawnPostProcess(Base latestBase, Int3 latestCell, GameObject finishedPiece, bool justConstructed)
        {
            building ??= NitroxServiceLocator.LocateService<Building>();

            NitroxId mapRoomGeometryPieceId = NitroxEntity.GetId(finishedPiece);
            GameObject mapRoomFunctionality = FindUntaggedMapRoomFunctionality(latestBase);

            NitroxId mapRoomFunctionalityId = mapRoomGeometryPieceId.Increment();
            NitroxEntity.SetNewId(mapRoomFunctionality, mapRoomFunctionalityId);

            GameObject mapRoomModules = mapRoomFunctionality.FindChild("MapRoomUpgrades");
            NitroxId mapRoomModulesId = mapRoomFunctionalityId.Increment();
            NitroxEntity.SetNewId(mapRoomModules, mapRoomModulesId);

            // Need to make sure that it already spawned
            PrefabSpawn fabricatorSpawn = mapRoomFunctionality.FindChild("MapRoomFabricatorSpawn").GetComponent<PrefabSpawn>();
            fabricatorSpawn.spawnType = SpawnType.Manual;
            GameObject mapRoomFabricator = fabricatorSpawn.SpawnManual();
            NitroxId mapRoomFabricatorId = mapRoomModulesId.Increment();
            NitroxEntity.SetNewId(mapRoomFabricator, mapRoomFabricatorId);

            GameObject mapRoomScreen = mapRoomFunctionality.transform.Find("screen/cameraScreen/input").gameObject;
            NitroxId mapRoomScreenId = mapRoomFabricatorId.Increment();
            NitroxEntity.SetNewId(mapRoomScreen, mapRoomScreenId);

            GameObject scannerUI = mapRoomFunctionality.transform.Find("screen/scannerUI").gameObject;
            NitroxId scannerUIId = mapRoomScreenId.Increment();
            NitroxEntity.SetNewId(scannerUI, scannerUIId);

            NitroxId dockingPoint1Id = scannerUIId.Increment();
            NitroxId dockingPoint2Id = dockingPoint1Id.Increment().Increment();

            Log.Debug($"Spawned MapRoomFunctionality: {mapRoomFunctionality.name}, justConstructed: {justConstructed}");
            foreach (MapRoomCameraDocking docking in mapRoomFunctionality.GetComponentsInChildren<MapRoomCameraDocking>())
            {
                // Simulate Start but don't call DockCamera to not start a Postfix
                // Log.Debug($"Docking found: [name: {docking.name}, camera: {docking.camera}, cameraDocked: {docking.cameraDocked}]");
                docking.cameraDocked = true;
                docking.deserialized = true;
                GameObject cameraGameObject = UnityEngine.Object.Instantiate(docking.cameraPrefab);
                CrafterLogic.NotifyCraftEnd(cameraGameObject, TechType.MapRoomCamera);
                docking.camera = cameraGameObject.GetComponent<MapRoomCamera>();
                docking.camera.transform.position = docking.dockingTransform.position;
                docking.camera.transform.rotation = docking.dockingTransform.rotation;
                docking.camera.SetDocked(docking);
                switch (docking.gameObject.name)
                {
                    case "dockingPoint1":
                        NitroxEntity.SetNewId(docking.gameObject, dockingPoint1Id);
                        NitroxEntity.SetNewId(cameraGameObject, dockingPoint1Id.Increment());
                        break;
                    case "dockingPoint2":
                        NitroxEntity.SetNewId(docking.gameObject, dockingPoint2Id);
                        NitroxEntity.SetNewId(cameraGameObject, dockingPoint2Id.Increment());
                        break;
                }
            }
            if (justConstructed)
            {
                building.MetadataChanged(mapRoomGeometryPieceId, mapRoomFunctionalityId, new MapRoomMetadata(mapRoomFunctionalityId, true, true, dockingPoint1Id.Increment(), dockingPoint2Id.Increment(), null));
            }
        }

        private static GameObject FindUntaggedMapRoomFunctionality(Base latestBase)
        {
            foreach (Transform child in latestBase.transform)
            {
                if (child.GetComponent<MapRoomFunctionality>() && !child.GetComponent<NitroxEntity>())
                {
                    return child.gameObject;
                }
            }

            throw new ArgumentException($"Unable to locate recently built map room with {latestBase}");
        }
    }
}
