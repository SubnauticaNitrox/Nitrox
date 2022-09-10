using System.Collections.Generic;
using NitroxModel.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel.DataStructures.Util;
using NitroxModel_Subnautica.DataStructures;
using NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation;
using NitroxModel_Subnautica.DataStructures.GameLogic.Buildings.Rotation.Metadata;
using UnityEngine;
using UWE;

namespace NitroxClient.MonoBehaviours.Overrides
{
    public static class MultiplayerBuilder
    {
        public static Bounds AABounds => aaBounds;

        public static bool IsPlacing => prefab != null;

        internal static GameObject ParentBase = null;
        
        public static void PlaceBasePiece(GameObject modulePrefab)
        {
            if (ghostModel != null)
            {
                ConstructableBase componentInParent = ghostModel.GetComponentInParent<ConstructableBase>();
                if (componentInParent != null)
                {
                    Object.Destroy(componentInParent.gameObject);
                }

                Object.Destroy(ghostModel);
            }

            prefab = modulePrefab;
            placementTarget = null;
            
            CreateGhost();
            UpdatePosition();
            MaterialExtensions.SetColor(renderers, ShaderPropertyID._Tint, placeColorAllow);
        }

        private static void CreateGhost()
        {
            Constructable component = prefab.GetComponent<Constructable>();
            constructableTechType = component.techType;
            allowedOutside = component.allowedOutside;
            ConstructableBase component2 = prefab.GetComponent<ConstructableBase>();
            if (component2 != null)
            {
                GameObject gameObject = Object.Instantiate<GameObject>(prefab);
                component2 = gameObject.GetComponent<ConstructableBase>();
                ghostModel = component2.model;
                BaseGhost component3 = ghostModel.GetComponent<BaseGhost>();
                component3.SetupGhost();
                ghostModelPosition = Vector3.zero;
                ghostModelRotation = Quaternion.identity;
                ghostModelScale = Vector3.one;
                renderers = MaterialExtensions.AssignMaterial(ghostModel, ghostStructureMaterial);
                InitBounds(ghostModel);
            }
            else
            {
                ghostModel = Object.Instantiate<GameObject>(component.model);
                ghostModel.SetActive(true);
                Transform component4 = component.GetComponent<Transform>();
                Transform component5 = component.model.GetComponent<Transform>();
                Quaternion quaternion = Quaternion.Inverse(component4.rotation);
                ghostModelPosition = quaternion * (component5.position - component4.position);
                ghostModelRotation = quaternion * component5.rotation;
                ghostModelScale = component5.lossyScale;
                Collider[] componentsInChildren = ghostModel.GetComponentsInChildren<Collider>();
                foreach (Collider collider in componentsInChildren)
                {
                    Object.Destroy(collider);
                }

                renderers = MaterialExtensions.AssignMaterial(ghostModel, ghostStructureMaterial);
                SetupRenderers(ghostModel, Player.main.IsInSub());
                CreatePowerPreview();
                InitBounds(prefab);
            }
        }

        private static void UpdatePosition()
        {
            ConstructableBase componentInParent = ghostModel.GetComponentInParent<ConstructableBase>();
            if (componentInParent != null)
            {
                Transform transform = componentInParent.transform;
                transform.position = PlacePosition;
                transform.rotation = PlaceRotation;

                bool geometryChanged = false;
                bool result = false;
                bool positionFound = false;

                BaseGhost baseGhost = ghostModel.GetComponent<BaseGhost>();

                // When you try to build a hatch onto a MapRoom when it is already linked to something (on the other side), it will end up in a bug because this room works weirdly for parts directly built on it
                // Therefore, this is a fix for both this issue and any other similar issue that would occur anywhere else.
                // NB: This can only be a fix and works as a backup plan if the placement didn't work at first 
                if (RotationMetadata.HasValue && RotationMetadata.Value is AnchoredFaceBuilderMetadata metadata && baseGhost is BaseAddFaceGhost faceGhost)
                {
                    // The issue happens inside BaseAddFaceGhost.UpdatePlacement, the face is wrongly detected by "targetBase.PickFace(camera, out face)" and it can't adjust itself with the following lines
                    // Therefore, what we do is skip the whole "targetBase.CanSetFace" part because we consider that if the information of building this piece here was issued, then it is in fact possible

                    // First we force the detection of targetBase which is only set when using UpdatePlacement
                    result = UpdatePlacement(baseGhost, GetAimTransform(), componentInParent.placeMaxDistance, out positionFound, out geometryChanged, componentInParent);
                    // We only want to apply the fix if the placement is not already valid and if we have a valid base
                    if (!result)
                    {
                        if (faceGhost.targetBase)
                        {
                            Base.Face face = new(metadata.Cell.ToUnity(), (Base.Direction)metadata.Direction);
                            face.cell += metadata.Anchor.ToUnity();
                            // We just skip the first part of the code and run the rest, which is a part that can't be ignored
                            result = ForceFaceGhostUpdatePlacement(faceGhost, face, out positionFound, out geometryChanged, componentInParent);
                        }
                        else
                        {
                            // With future plans, we may want to delete the part from here or try more things so that it doesn't corrupt the whole base
                            Log.Warn("Couldn't find a valid target base for a ghost, it won't be placed correctly");
                        }
                    }
                }

                // We don't need to check it if we already know the placement is valid
                if (!result)
                {
                    result = UpdatePlacement(baseGhost,GetAimTransform(), componentInParent.placeMaxDistance, out positionFound, out geometryChanged, componentInParent);
                }
                componentInParent.SetGhostVisible(positionFound);

                if (result && RotationMetadata.HasValue)
                {
                    ApplyRotationMetadata(baseGhost, RotationMetadata.Value);
                }
                if (geometryChanged)
                {
                    renderers = MaterialExtensions.AssignMaterial(ghostModel, ghostStructureMaterial);
                    InitBounds(ghostModel);
                }
            }
            Transform ghostModelTransform = ghostModel.transform;
            ghostModelTransform.position = PlacePosition + PlaceRotation * ghostModelPosition;
            ghostModelTransform.rotation = PlaceRotation * ghostModelRotation;
            ghostModelTransform.localScale = ghostModelScale;
        }

        /// <summary>
        /// BaseAddFaceGhost.UpdatePlacement's code after the "targetBase.CanSetFace" block
        /// </summary>
        private static bool ForceFaceGhostUpdatePlacement(BaseAddFaceGhost faceGhost, Base.Face face, out bool positionFound, out bool geometryChanged, ConstructableBase ghostModelParentConstructableBase)
        {
            Int3 @int = faceGhost.targetBase.NormalizeCell(face.cell);
            Base.CellType cell = faceGhost.targetBase.GetCell(@int);
            Int3 v = Base.CellSize[(int)cell];
            Int3.Bounds a = new Int3.Bounds(face.cell, face.cell);
            Int3.Bounds b = new Int3.Bounds(@int, @int + v - 1);
            Int3.Bounds bounds = Int3.Bounds.Union(a, b);
            geometryChanged = faceGhost.UpdateSize(bounds.size);
            Base.Face face2 = new Base.Face(face.cell - faceGhost.targetBase.GetAnchor(), face.direction);
            if (faceGhost.anchoredFace == null || faceGhost.anchoredFace.Value != face2)
            {
                faceGhost.anchoredFace = new Base.Face?(face2);
                faceGhost.ghostBase.CopyFrom(faceGhost.targetBase, bounds, bounds.mins * -1);
                faceGhost.ghostBase.ClearMasks();
                Int3 cell2 = face.cell - @int;
                Base.Face face3 = new Base.Face(cell2, face.direction);
                faceGhost.ghostBase.SetFaceMask(face3, true);
                faceGhost.ghostBase.SetFace(face3, faceGhost.faceType);
                faceGhost.RebuildGhostGeometry();
                geometryChanged = true;
            }
            ghostModelParentConstructableBase.transform.position = faceGhost.targetBase.GridToWorld(@int);
            ghostModelParentConstructableBase.transform.rotation = faceGhost.targetBase.transform.rotation;
            positionFound = true;
            foreach (Int3 cell3 in bounds)
            {
                if (faceGhost.targetBase.IsCellUnderConstruction(cell3))
                {
                    return false;
                }
            }
            return true;
        }

        private static bool UpdatePlacement(BaseGhost baseGhost, Transform camera, float placeMaxDistance, out bool positionFound, out bool flag, ConstructableBase componentInParent)
        {
            bool flag2;
            switch (baseGhost)
            {
                default:
                    flag2 = baseGhost.UpdatePlacement(camera, placeMaxDistance, out positionFound, out flag, componentInParent);
                    Log.Debug($"faceGhost: {positionFound}, {flag}");
                    break;
            }

            Log.Debug($"Returned: {flag2}");
            return flag2;
        }

        private static void ApplyRotationMetadata(BaseGhost baseGhost, BuilderMetadata builderMetadata)
        {
            Log.Debug($"Found BaseGhost of type: {baseGhost.GetType()}, {baseGhost}");
            switch (baseGhost)
            {
                case BaseAddCorridorGhost corridor when builderMetadata is CorridorBuilderMetadata corridorRotationMetadata:
                {
                    corridor.rotation = corridorRotationMetadata.Rotation;
                    int corridorType = corridor.CalculateCorridorType();
                    corridor.ghostBase.SetCorridor(Int3.zero, corridorType, corridor.isGlass);
                    corridor.RebuildGhostGeometry();
                    break;
                }
                case BaseAddMapRoomGhost mapRoom when builderMetadata is MapRoomBuilderMetadata mapRoomRotationMetadata:
                {
                    mapRoom.cellType = (Base.CellType)mapRoomRotationMetadata.CellType;
                    mapRoom.connectionMask = mapRoomRotationMetadata.ConnectionMask;

                    mapRoom.ghostBase.SetCell(Int3.zero, (Base.CellType)mapRoomRotationMetadata.CellType);
                    mapRoom.RebuildGhostGeometry();
                    break;
                }
                case BaseAddModuleGhost ghost when builderMetadata is BaseModuleBuilderMetadata baseModuleRotationMetadata:
                {
                    ghost.anchoredFace = new Base.Face(baseModuleRotationMetadata.Cell.ToUnity(), (Base.Direction)baseModuleRotationMetadata.Direction);
                    ghost.RebuildGhostGeometry();
                    break;
                }
                case BaseAddFaceGhost faceGhost when builderMetadata is AnchoredFaceBuilderMetadata baseModuleRotationMetadata:
                {
                    Log.Debug($"Applying AnchoredFaceBuilderMetadata: {baseModuleRotationMetadata}");
                    Base.Face face = new(baseModuleRotationMetadata.Cell.ToUnity(), (Base.Direction)baseModuleRotationMetadata.Direction);
                    if (faceGhost.targetBase.GetAnchor() != baseModuleRotationMetadata.Anchor.ToUnity())
                    {
                        face.cell += baseModuleRotationMetadata.Anchor.ToUnity();
                    }
                    faceGhost.anchoredFace = face;
                
                    Base.FaceType faceType = (Base.FaceType)baseModuleRotationMetadata.FaceType;
                    faceGhost.ghostBase.SetFace(face, faceType);

                    faceGhost.RebuildGhostGeometry();
                    break;
                }
                default:
                    Log.Error($"Was unable to apply rotation metadata for {baseGhost.GetType()} and {builderMetadata}");
                    break;
            }
        }

        public static ConstructableBase TryPlaceBase(GameObject targetBaseGameObject)
        {
#pragma warning disable CS0618
            //Disabling warning as we dont have the FModAsset to use instead.
            Utils.PlayEnvSound(PLACE_SOUND, ghostModel.transform.position);
#pragma warning restore CS0618
            ConstructableBase componentInParent = ghostModel.GetComponentInParent<ConstructableBase>();
            BaseGhost component = ghostModel.GetComponent<BaseGhost>();
            component.GhostBase.transform.position = OverridePosition;
            component.Place();

            componentInParent.transform.position = OverridePosition;

            Transform transform = component.transform;
            transform.position = OverridePosition;
            transform.rotation = OverrideQuaternion;

            if (targetBaseGameObject)
            {
                Base targetBase = targetBaseGameObject.GetComponent<Base>();

                if (targetBase != null)
                {
                    component.targetBase = targetBase;
                    componentInParent.transform.SetParent(targetBase.transform, true);
                }
                else
                {
                    Log.Error("Could not find base component on the given game object: " + targetBaseGameObject.name);
                }
            }

            componentInParent.SetState(false);

            component.GhostBase.transform.position = OverridePosition;

            ghostModel = null;
            prefab = null;
            RotationMetadata = Optional.Empty;

            return componentInParent;
        }

        public static GameObject TryPlaceFurniture(SubRoot currentSub)
        {
#pragma warning disable CS0618
            //Disabling warning as we dont have the FModAsset to use instead.
            Utils.PlayEnvSound(PLACE_SOUND, ghostModel.transform.position);
#pragma warning restore CS0618

            GameObject gameObject = Object.Instantiate<GameObject>(prefab);
            bool flag = false;
            bool flag2 = false;
            if (currentSub != null)
            {
                flag = currentSub.isBase;
                flag2 = currentSub.isCyclops;
                gameObject.transform.parent = currentSub.GetModulesRoot();
            }
            else if (placementTarget != null && allowedOutside)
            {
                SubRoot componentInParent2 = placementTarget.GetComponentInParent<SubRoot>();
                if (componentInParent2 != null)
                {
                    gameObject.transform.parent = componentInParent2.GetModulesRoot();
                }
            }

            Transform expr138 = gameObject.transform;
            expr138.position = PlacePosition;
            expr138.rotation = PlaceRotation;
            Constructable componentInParent3 = gameObject.GetComponentInParent<Constructable>();
            componentInParent3.SetState(false);
            Utils.SetLayerRecursively(gameObject, LayerMask.NameToLayer((!flag) ? "Interior" : "Default"));
            if (ghostModel != null)
            {
                Object.Destroy(ghostModel);
            }

            componentInParent3.SetIsInside(flag || flag2);
            SkyEnvironmentChanged.Send(gameObject, currentSub);

            if (currentSub != null && currentSub.isCyclops)
            {
                gameObject.transform.localPosition = OverridePosition;
                gameObject.transform.localRotation = OverrideQuaternion;
            }
            else
            {
                gameObject.transform.position = OverridePosition;
                gameObject.transform.rotation = OverrideQuaternion;
            }

            ghostModel = null;
            prefab = null;
            RotationMetadata = Optional.Empty;

            return gameObject;
        }

        private static void InitBounds(GameObject gameObject)
        {
            Transform transform = gameObject.transform;
            CacheBounds(transform, gameObject, bounds);
            aaBounds.center = Vector3.zero;
            aaBounds.extents = Vector3.zero;
            int count = bounds.Count;
            if (count > 0)
            {
                Vector3 vector = new Vector3(3.40282347E+38f, 3.40282347E+38f, 3.40282347E+38f);
                Vector3 a = new Vector3(-3.40282347E+38f, -3.40282347E+38f, -3.40282347E+38f);
                for (int i = 0; i < count; i++)
                {
                    OrientedBounds orientedBounds = bounds[i];
                    Matrix4x4 boundsToLocalMatrix = OrientedBounds.TransformMatrix(orientedBounds.position, orientedBounds.rotation);
                    OrientedBounds.MinMaxBounds(boundsToLocalMatrix, Vector3.zero, orientedBounds.extents, ref vector, ref a);
                }

                aaBounds.extents = (a - vector) * 0.5f;
                aaBounds.center = vector + AABounds.extents;
            }
        }

        public static void CacheBounds(Transform transform, GameObject target, List<OrientedBounds> results, bool append = false)
        {
            if (!append)
            {
                results.Clear();
            }

            if (target == null)
            {
                return;
            }

            ConstructableBounds[] componentsInChildren = target.GetComponentsInChildren<ConstructableBounds>();
            foreach (ConstructableBounds constructableBounds in componentsInChildren)
            {
                OrientedBounds localBounds = constructableBounds.bounds;
                OrientedBounds orientedBounds = OrientedBounds.ToWorldBounds(constructableBounds.transform, localBounds);
                if (transform != null)
                {
                    orientedBounds = OrientedBounds.ToLocalBounds(transform, orientedBounds);
                }

                results.Add(orientedBounds);
            }
        }

        public static bool CheckSpace(Vector3 position, Quaternion rotation, Vector3 extents, int layerMask, Collider allowedCollider)
        {
            if (extents.x <= 0f || extents.y <= 0f || extents.z <= 0f)
            {
                return true;
            }

            int num = Physics.OverlapBoxNonAlloc(position, extents, sColliders, rotation, layerMask, QueryTriggerInteraction.Ignore);
            return num == 0 || (num == 1 && sColliders[0] == allowedCollider);
        }

        public static bool CheckSpace(Vector3 position, Quaternion rotation, List<OrientedBounds> localBounds, int layerMask, Collider allowedCollider)
        {
            if (rotation.IsDistinguishedIdentity())
            {
                rotation = Quaternion.identity;
            }

            for (int i = 0; i < localBounds.Count; i++)
            {
                OrientedBounds orientedBounds = localBounds[i];
                if (orientedBounds.rotation.IsDistinguishedIdentity())
                {
                    orientedBounds.rotation = Quaternion.identity;
                }

                orientedBounds.position = position + rotation * orientedBounds.position;
                orientedBounds.rotation = rotation * orientedBounds.rotation;
                if (!CheckSpace(orientedBounds.position, orientedBounds.rotation, orientedBounds.extents, layerMask, allowedCollider))
                {
                    return false;
                }
            }

            return true;
        }

        public static void GetOverlappedColliders(Vector3 position, Quaternion rotation, Vector3 extents, List<Collider> results)
        {
            results.Clear();
            int num = UWE.Utils.OverlapBoxIntoSharedBuffer(position, extents, rotation, -1, QueryTriggerInteraction.Collide);
            for (int i = 0; i < num; i++)
            {
                Collider collider = UWE.Utils.sharedColliderBuffer[i];
                GameObject gameObject = collider.gameObject;
                if (!collider.isTrigger || gameObject.layer == LayerID.Useable)
                {
                    results.Add(collider);
                }
            }
        }

        public static void GetRootObjects(List<Collider> colliders, List<GameObject> results)
        {
            results.Clear();
            for (int i = 0; i < colliders.Count; i++)
            {
                GameObject gameObject = colliders[i].gameObject;
                GameObject gameObject2 = UWE.Utils.GetEntityRoot(gameObject);
                if (gameObject2 == null)
                {
                    SceneObjectIdentifier componentInParent = gameObject.GetComponentInParent<SceneObjectIdentifier>();
                    if (componentInParent != null)
                    {
                        gameObject2 = componentInParent.gameObject;
                    }
                }
                gameObject = ((gameObject2 != null) ? gameObject2 : gameObject);
                if (!results.Contains(gameObject))
                {
                    results.Add(gameObject);
                }
            }
        }

        public static Transform GetAimTransform()
        {
            return OverrideTransform;
        }

        private static void SetupRenderers(GameObject gameObject, bool interior)
        {
            int newLayer = LayerMask.NameToLayer(interior ? "Viewmodel" : "Default");
            Utils.SetLayerRecursively(gameObject, newLayer);
        }

        private static void CreatePowerPreview()
        {
            GameObject gameObject = null;
            string poweredPrefabName = CraftData.GetPoweredPrefabName(constructableTechType);
            if (poweredPrefabName != string.Empty)
            {
#pragma warning disable CS0618
                //Ignore warning to use an async method when we need sync.
                gameObject = PrefabDatabase.GetPrefabForFilename(poweredPrefabName);
#pragma warning restore CS0618
            }

            if (gameObject == null)
            {
                return;
            }

            PowerRelay component = gameObject.GetComponent<PowerRelay>();
            if (component.powerFX != null && component.powerFX.attachPoint != null)
            {
                PowerFX powerFX = ghostModel.AddComponent<PowerFX>();
                powerFX.attachPoint = new GameObject
                {
                    transform =
                    {
                        parent = ghostModel.transform,
                        localPosition = component.powerFX.attachPoint.localPosition
                    }
                }.transform;
            }

            PowerRelay powerRelay = ghostModel.AddComponent<PowerRelay>();
            powerRelay.maxOutboundDistance = component.maxOutboundDistance;
            powerRelay.dontConnectToRelays = component.dontConnectToRelays;
            if (component.internalPowerSource != null)
            {
                powerRelay.internalPowerSource = ghostModel.AddComponent<PowerSource>();
            }
        }

        public static Vector3 OverridePosition;

        public static Quaternion OverrideQuaternion;

        public static Transform OverrideTransform;

        private static readonly Color placeColorAllow = new Color(0f, 1f, 0f, 1f);

        private static readonly Collider[] sColliders = new Collider[2];

        private static GameObject prefab;

        private static TechType constructableTechType;

        private static bool allowedOutside;

        private static Renderer[] renderers;

        private static GameObject ghostModel;

        private static Vector3 ghostModelPosition;

        private static Quaternion ghostModelRotation;

        private static Vector3 ghostModelScale;

        private static readonly List<OrientedBounds> bounds = new List<OrientedBounds>();

        private static Bounds aaBounds = default(Bounds);

        public static Vector3 PlacePosition;

        public static Quaternion PlaceRotation;

        public static Optional<BuilderMetadata> RotationMetadata;

        private static readonly Material ghostStructureMaterial = new Material(Resources.Load<Material>("Materials/ghostmodel"));

        private static GameObject placementTarget;

        private const string PLACE_SOUND = "event:/tools/builder/place";
    }
}
