#pragma warning disable // Disable all warnings for copied file
// ReSharper disable InconsistentNaming

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
    // Token: 0x020006BA RID: 1722
    public static class MultiplayerBuilder
    {
        // Token: 0x17000299 RID: 665
        // (get) Token: 0x06002B93 RID: 11155 RVA: 0x0001E932 File Offset: 0x0001CB32
        public static Bounds aaBounds => MultiplayerBuilder._aaBounds;

        // Token: 0x1700029A RID: 666
        // (get) Token: 0x06002B94 RID: 11156 RVA: 0x0001E939 File Offset: 0x0001CB39
        public static bool isPlacing => MultiplayerBuilder.prefab != null;

        // Token: 0x1700029B RID: 667
        // (get) Token: 0x06002B95 RID: 11157 RVA: 0x0001E946 File Offset: 0x0001CB46
        // (set) Token: 0x06002B96 RID: 11158 RVA: 0x0001E94D File Offset: 0x0001CB4D
        public static bool canPlace
        {
            get;
            private set;
        }

        // Token: 0x06002B97 RID: 11159 RVA: 0x00103CE8 File Offset: 0x00101EE8
        private static void Initialize()
        {
            if (MultiplayerBuilder.initialized)
            {
                return;
            }

            MultiplayerBuilder.initialized = true;
            MultiplayerBuilder.placeLayerMask = ~(1 << LayerMask.NameToLayer("Player") | 1 << LayerMask.NameToLayer("Trigger"));
            MultiplayerBuilder.ghostStructureMaterial = new Material(Resources.Load<Material>("Materials/ghostmodel"));
        }

        // Token: 0x06002B98 RID: 11160 RVA: 0x0001E955 File Offset: 0x0001CB55
        public static bool Begin(GameObject modulePrefab)
        {
            MultiplayerBuilder.Initialize();
            if (modulePrefab == null)
            {
                Debug.LogWarning("Builder : Begin() : Module prefab is null!");
                return false;
            }

            if (modulePrefab != MultiplayerBuilder.prefab)
            {
                MultiplayerBuilder.End();
            }

            MultiplayerBuilder.prefab = modulePrefab;
            MultiplayerBuilder.Update();
            return true;
        }

        // Token: 0x06002B99 RID: 11161 RVA: 0x00103D44 File Offset: 0x00101F44
        public static void End()
        {
            MultiplayerBuilder.Initialize();
            MultiplayerBuilder.inputHandler.canHandleInput = false;
            if (MultiplayerBuilder.ghostModel != null)
            {
                ConstructableBase componentInParent = MultiplayerBuilder.ghostModel.GetComponentInParent<ConstructableBase>();
                if (componentInParent != null)
                {
                    UnityEngine.Object.Destroy(componentInParent.gameObject);
                }

                UnityEngine.Object.Destroy(MultiplayerBuilder.ghostModel);
            }

            MultiplayerBuilder.prefab = null;
            MultiplayerBuilder.ghostModel = null;
            MultiplayerBuilder.canPlace = false;
            MultiplayerBuilder.placementTarget = null;
            MultiplayerBuilder.additiveRotation = 0f;
        }

        // Token: 0x06002B9A RID: 11162
        public static void Update()
        {
            MultiplayerBuilder.Initialize();
            MultiplayerBuilder.canPlace = false;
            if (MultiplayerBuilder.prefab == null)
            {
                return;
            }

            if (MultiplayerBuilder.CreateGhost())
            {
                Log.Debug("Ghost Created!");
            }

            MultiplayerBuilder.canPlace = MultiplayerBuilder.UpdateAllowed();
            Transform expr_58 = MultiplayerBuilder.ghostModel.transform;
            expr_58.position = MultiplayerBuilder.placePosition + MultiplayerBuilder.placeRotation * MultiplayerBuilder.ghostModelPosition;
            expr_58.rotation = MultiplayerBuilder.placeRotation * MultiplayerBuilder.ghostModelRotation;
            expr_58.localScale = MultiplayerBuilder.ghostModelScale;
            Color color = (!MultiplayerBuilder.canPlace) ? MultiplayerBuilder.placeColorDeny : MultiplayerBuilder.placeColorAllow;
            MaterialExtensions.SetColor(MultiplayerBuilder.renderers, ShaderPropertyID._Tint, color);
        }

        // Token: 0x06002B9B RID: 11163 RVA: 0x00103EBC File Offset: 0x001020BC
        private static bool CreateGhost()
        {
            if (MultiplayerBuilder.ghostModel != null)
            {
                return false;
            }

            Constructable component = MultiplayerBuilder.prefab.GetComponent<Constructable>();
            MultiplayerBuilder.constructableTechType = component.techType;
            MultiplayerBuilder.placeMinDistance = component.placeMinDistance;
            MultiplayerBuilder.placeMaxDistance = component.placeMaxDistance;
            MultiplayerBuilder.placeDefaultDistance = component.placeDefaultDistance;
            MultiplayerBuilder.allowedSurfaceTypes = component.allowedSurfaceTypes;
            MultiplayerBuilder.forceUpright = component.forceUpright;
            MultiplayerBuilder.allowedInSub = component.allowedInSub;
            MultiplayerBuilder.allowedInBase = component.allowedInBase;
            MultiplayerBuilder.allowedOutside = component.allowedOutside;
            MultiplayerBuilder.allowedOnConstructables = component.allowedOnConstructables;
            MultiplayerBuilder.rotationEnabled = component.rotationEnabled;
            ConstructableBase component2 = MultiplayerBuilder.prefab.GetComponent<ConstructableBase>();
            if (component2 != null)
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(MultiplayerBuilder.prefab);
                component2 = gameObject.GetComponent<ConstructableBase>();
                MultiplayerBuilder.ghostModel = component2.model;
                BaseGhost component3 = MultiplayerBuilder.ghostModel.GetComponent<BaseGhost>();
                component3.SetupGhost();
                MultiplayerBuilder.ghostModelPosition = Vector3.zero;
                MultiplayerBuilder.ghostModelRotation = Quaternion.identity;
                MultiplayerBuilder.ghostModelScale = Vector3.one;
                MultiplayerBuilder.renderers = MaterialExtensions.AssignMaterial(MultiplayerBuilder.ghostModel, MultiplayerBuilder.ghostStructureMaterial);
                MultiplayerBuilder.InitBounds(MultiplayerBuilder.ghostModel);
            }
            else
            {
                MultiplayerBuilder.ghostModel = UnityEngine.Object.Instantiate<GameObject>(component.model);
                MultiplayerBuilder.ghostModel.SetActive(true);
                Transform component4 = component.GetComponent<Transform>();
                Transform component5 = component.model.GetComponent<Transform>();
                Quaternion quaternion = Quaternion.Inverse(component4.rotation);
                MultiplayerBuilder.ghostModelPosition = quaternion * (component5.position - component4.position);
                MultiplayerBuilder.ghostModelRotation = quaternion * component5.rotation;
                MultiplayerBuilder.ghostModelScale = component5.lossyScale;
                Collider[] componentsInChildren = MultiplayerBuilder.ghostModel.GetComponentsInChildren<Collider>();
                for (int i = 0; i < componentsInChildren.Length; i++)
                {
                    UnityEngine.Object.Destroy(componentsInChildren[i]);
                }

                MultiplayerBuilder.renderers = MaterialExtensions.AssignMaterial(MultiplayerBuilder.ghostModel, MultiplayerBuilder.ghostStructureMaterial);
                MultiplayerBuilder.SetupRenderers(MultiplayerBuilder.ghostModel, Player.main.IsInSub());
                MultiplayerBuilder.CreatePowerPreview(MultiplayerBuilder.constructableTechType, MultiplayerBuilder.ghostModel);
                MultiplayerBuilder.InitBounds(MultiplayerBuilder.prefab);
            }

            return true;
        }

        // Token: 0x06002B9C RID: 11164 RVA: 0x001040F8 File Offset: 0x001022F8
        private static bool UpdateAllowed()
        {
            // MultiplayerBuilder.SetDefaultPlaceTransform(ref MultiplayerBuilder.placePosition, ref MultiplayerBuilder.placeRotation);
            bool flag = false;
            ConstructableBase componentInParent = MultiplayerBuilder.ghostModel.GetComponentInParent<ConstructableBase>();
            bool flag2 = true;
            if (componentInParent != null)
            {
                Transform transform = componentInParent.transform;
                transform.position = MultiplayerBuilder.placePosition;
                transform.rotation = MultiplayerBuilder.placeRotation;

                flag2 = componentInParent.UpdateGhostModel(MultiplayerBuilder.GetAimTransform(), MultiplayerBuilder.ghostModel, default(RaycastHit), out flag, componentInParent);

                if (rotationMetadata.HasValue)
                {
                    ApplyRotationMetadata(MultiplayerBuilder.ghostModel, rotationMetadata.Value);
                }

                if (flag)
                {
                    MultiplayerBuilder.renderers = MaterialExtensions.AssignMaterial(MultiplayerBuilder.ghostModel, MultiplayerBuilder.ghostStructureMaterial);
                    MultiplayerBuilder.InitBounds(MultiplayerBuilder.ghostModel);
                }
            }

            if (flag2)
            {
                List<GameObject> list = new List<GameObject>();
                MultiplayerBuilder.GetObstacles(MultiplayerBuilder.placePosition, MultiplayerBuilder.placeRotation, MultiplayerBuilder.bounds, list);
                flag2 = list.Count == 0;
                list.Clear();
            }

            return flag2;
        }

        private static void ApplyRotationMetadata(GameObject ghostModel, RotationMetadata rotationMetadata)
        {
            BaseGhost component = ghostModel.GetComponent<BaseGhost>();

            if (component == null)
            {
                Log.Error("Was unable to apply rotation metadata - no BaseGhost found");
            }
            else if (component.GetType() != rotationMetadata.GhostType)
            {
                Log.Error("Was unable to apply rotation metadata - " + component.GetType() + " did not match " + rotationMetadata.GhostType);
            }
            else if (component is BaseAddCorridorGhost corridor)
            {
                Log.Info("Placing BaseAddCorridorGhost Rotation Metadata");

                CorridorRotationMetadata corridorRotationMetadata = (CorridorRotationMetadata)rotationMetadata;
                corridor.rotation = corridorRotationMetadata.Rotation;

                int corridorType = corridor.CalculateCorridorType();
                corridor.ghostBase.SetCorridor(Int3.zero, corridorType, corridor.isGlass);
                corridor.RebuildGhostGeometry();
            }
            else if (component is BaseAddMapRoomGhost mapRoom)
            {
                Log.Info("Placing MapRoomRotationMetadata Rotation Metadata");

                MapRoomRotationMetadata mapRoomRotationMetadata = (MapRoomRotationMetadata)rotationMetadata;
                mapRoom.cellType = (Base.CellType)mapRoomRotationMetadata.CellType;
                mapRoom.connectionMask = mapRoomRotationMetadata.ConnectionMask;

                mapRoom.ghostBase.SetCell(Int3.zero, (Base.CellType)mapRoomRotationMetadata.CellType);
                mapRoom.RebuildGhostGeometry();
            }
            else if (component is BaseAddModuleGhost ghost)
            {
                BaseModuleRotationMetadata baseModuleRotationMetadata = (BaseModuleRotationMetadata)rotationMetadata;

                ghost.anchoredFace = new Base.Face(baseModuleRotationMetadata.Cell.ToUnity(), (Base.Direction)baseModuleRotationMetadata.Direction);
                ghost.RebuildGhostGeometry();
            }
            else if (component is BaseAddFaceGhost faceGhost)
            {
                AnchoredFaceRotationMetadata baseModuleRotationMetadata = (AnchoredFaceRotationMetadata)rotationMetadata;
                Log.Info("Applying BaseAddFaceGhost " + baseModuleRotationMetadata);

                Base.Face face = new(baseModuleRotationMetadata.Cell.ToUnity(), (Base.Direction)baseModuleRotationMetadata.Direction);
                faceGhost.anchoredFace = face;
                
                Base.FaceType faceType = (Base.FaceType)baseModuleRotationMetadata.FaceType;
                faceGhost.ghostBase.SetFace(face, faceType);

                faceGhost.RebuildGhostGeometry();
            }
        }

        // Token: 0x06002B9D RID: 11165
        public static bool TryPlace()
        {
            MultiplayerBuilder.Initialize();
            if (MultiplayerBuilder.prefab == null || !MultiplayerBuilder.canPlace)
            {
                return false;
            }

            global::Utils.PlayEnvSound(MultiplayerBuilder.placeSound, MultiplayerBuilder.ghostModel.transform.position, 10f);
            ConstructableBase componentInParent = MultiplayerBuilder.ghostModel.GetComponentInParent<ConstructableBase>();
            if (componentInParent != null)
            {
                BaseGhost component = MultiplayerBuilder.ghostModel.GetComponent<BaseGhost>();
                component.GhostBase.transform.position = overridePosition;
                component.Place();

                componentInParent.transform.position = overridePosition;

                component.transform.position = overridePosition;
                component.transform.rotation = overrideQuaternion;
                if (component.TargetBase != null)
                {
                    componentInParent.transform.SetParent(component.TargetBase.transform, true);
                }

                componentInParent.SetState(false, true);

                component.GhostBase.transform.position = overridePosition;

                if (component.TargetBase != null)
                {
                    component.TargetBase.transform.position = overridePosition;
                }
            }
            else
            {
                GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(MultiplayerBuilder.prefab);
                bool flag = false;
                bool flag2 = false;
                SubRoot currentSub = Player.main.GetCurrentSub();
                if (currentSub != null)
                {
                    flag = currentSub.isBase;
                    flag2 = currentSub.isCyclops;
                    gameObject.transform.parent = currentSub.GetModulesRoot();
                }
                else if (MultiplayerBuilder.placementTarget != null && MultiplayerBuilder.allowedOutside)
                {
                    SubRoot componentInParent2 = MultiplayerBuilder.placementTarget.GetComponentInParent<SubRoot>();
                    if (componentInParent2 != null)
                    {
                        gameObject.transform.parent = componentInParent2.GetModulesRoot();
                    }
                }

                Transform expr_138 = gameObject.transform;
                expr_138.position = MultiplayerBuilder.placePosition;
                expr_138.rotation = MultiplayerBuilder.placeRotation;
                Constructable componentInParent3 = gameObject.GetComponentInParent<Constructable>();
                componentInParent3.SetState(false, true);
                global::Utils.SetLayerRecursively(gameObject, LayerMask.NameToLayer((!flag) ? "Interior" : "Default"), true, -1);
                componentInParent3.SetIsInside(flag | flag2);
                SkyEnvironmentChanged.Send(gameObject, currentSub);
                gameObject.transform.position = overridePosition;
                gameObject.transform.rotation = overrideQuaternion;
            }

            MultiplayerBuilder.ghostModel = null;
            MultiplayerBuilder.prefab = null;
            MultiplayerBuilder.canPlace = false;
            return true;
        }

        public static ConstructableBase TryPlaceBase(GameObject targetBaseGameObject)
        {
            MultiplayerBuilder.Initialize();
            global::Utils.PlayEnvSound(MultiplayerBuilder.placeSound, MultiplayerBuilder.ghostModel.transform.position, 10f);
            ConstructableBase componentInParent = MultiplayerBuilder.ghostModel.GetComponentInParent<ConstructableBase>();
            BaseGhost component = MultiplayerBuilder.ghostModel.GetComponent<BaseGhost>();
            component.GhostBase.transform.position = overridePosition;
            component.Place();

            componentInParent.transform.position = overridePosition;

            component.transform.position = overridePosition;
            component.transform.rotation = overrideQuaternion;

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

            componentInParent.SetState(false, true);

            component.GhostBase.transform.position = overridePosition;

            MultiplayerBuilder.ghostModel = null;
            MultiplayerBuilder.prefab = null;
            MultiplayerBuilder.canPlace = false;

            return componentInParent;
        }

        public static GameObject TryPlaceFurniture(SubRoot currentSub)
        {
            MultiplayerBuilder.Initialize();
            global::Utils.PlayEnvSound(MultiplayerBuilder.placeSound, MultiplayerBuilder.ghostModel.transform.position, 10f);

            GameObject gameObject = UnityEngine.Object.Instantiate<GameObject>(MultiplayerBuilder.prefab);
            bool flag = false;
            bool flag2 = false;
            if (currentSub != null)
            {
                flag = currentSub.isBase;
                flag2 = currentSub.isCyclops;
                gameObject.transform.parent = currentSub.GetModulesRoot();
            }
            else if (MultiplayerBuilder.placementTarget != null && MultiplayerBuilder.allowedOutside)
            {
                SubRoot componentInParent2 = MultiplayerBuilder.placementTarget.GetComponentInParent<SubRoot>();
                if (componentInParent2 != null)
                {
                    gameObject.transform.parent = componentInParent2.GetModulesRoot();
                }
            }

            Transform expr_138 = gameObject.transform;
            expr_138.position = MultiplayerBuilder.placePosition;
            expr_138.rotation = MultiplayerBuilder.placeRotation;
            Constructable componentInParent3 = gameObject.GetComponentInParent<Constructable>();
            componentInParent3.SetState(false, true);
            global::Utils.SetLayerRecursively(gameObject, LayerMask.NameToLayer((!flag) ? "Interior" : "Default"), true, -1);
            if (MultiplayerBuilder.ghostModel != null)
            {
                UnityEngine.Object.Destroy(MultiplayerBuilder.ghostModel);
            }

            componentInParent3.SetIsInside(flag || flag2);
            SkyEnvironmentChanged.Send(gameObject, currentSub);

            if (currentSub != null && currentSub.isCyclops)
            {
                gameObject.transform.localPosition = overridePosition;
                gameObject.transform.localRotation = overrideQuaternion;
            }
            else
            {
                gameObject.transform.position = overridePosition;
                gameObject.transform.rotation = overrideQuaternion;
            }

            MultiplayerBuilder.ghostModel = null;
            MultiplayerBuilder.prefab = null;
            MultiplayerBuilder.canPlace = false;

            return gameObject;
        }

        // Token: 0x06002B9E RID: 11166 RVA: 0x001043C0 File Offset: 0x001025C0
        public static void ShowRotationControlsHint()
        {
            string format = Language.main.GetFormat<string, string>("GhostRotateInputHint", uGUI.FormatButton(Builder.buttonRotateCW, true, ", "), uGUI.FormatButton(Builder.buttonRotateCCW, true, ", "));
            ErrorMessage.AddError(format);
        }

        // Token: 0x06002B9F RID: 11167 RVA: 0x001044D0 File Offset: 0x001026D0
        private static void InitBounds(GameObject gameObject)
        {
            Transform transform = gameObject.transform;
            MultiplayerBuilder.CacheBounds(transform, gameObject, MultiplayerBuilder.bounds, false);
            MultiplayerBuilder._aaBounds.center = Vector3.zero;
            MultiplayerBuilder._aaBounds.extents = Vector3.zero;
            int count = MultiplayerBuilder.bounds.Count;
            if (count > 0)
            {
                Vector3 vector = new Vector3(3.40282347E+38f, 3.40282347E+38f, 3.40282347E+38f);
                Vector3 a = new Vector3(-3.40282347E+38f, -3.40282347E+38f, -3.40282347E+38f);
                for (int i = 0; i < count; i++)
                {
                    OrientedBounds orientedBounds = MultiplayerBuilder.bounds[i];
                    Matrix4x4 boundsToLocalMatrix = OrientedBounds.TransformMatrix(orientedBounds.position, orientedBounds.rotation);
                    OrientedBounds.MinMaxBounds(boundsToLocalMatrix, Vector3.zero, orientedBounds.extents, ref vector, ref a);
                }

                MultiplayerBuilder._aaBounds.extents = (a - vector) * 0.5f;
                MultiplayerBuilder._aaBounds.center = vector + MultiplayerBuilder.aaBounds.extents;
            }
        }

        // Token: 0x06002BA0 RID: 11168 RVA: 0x001045D8 File Offset: 0x001027D8
        public static void OnDrawGizmos()
        {
            Matrix4x4 matrix = Gizmos.matrix;
            Color color = Gizmos.color;
            Gizmos.matrix = OrientedBounds.TransformMatrix(MultiplayerBuilder.placePosition, MultiplayerBuilder.placeRotation);
            Gizmos.color = new Color(0f, 0f, 1f, 0.5f);
            Gizmos.DrawCube(MultiplayerBuilder.aaBounds.center, MultiplayerBuilder.aaBounds.extents * 2f);
            Gizmos.matrix = matrix;
            Gizmos.color = color;
            MultiplayerBuilder.OnDrawGizmos();
        }

        // Token: 0x06002BA1 RID: 11169 RVA: 0x00104660 File Offset: 0x00102860
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
            for (int i = 0; i < componentsInChildren.Length; i++)
            {
                ConstructableBounds constructableBounds = componentsInChildren[i];
                OrientedBounds localBounds = constructableBounds.bounds;
                OrientedBounds orientedBounds = OrientedBounds.ToWorldBounds(constructableBounds.transform, localBounds);
                if (transform != null)
                {
                    orientedBounds = OrientedBounds.ToLocalBounds(transform, orientedBounds);
                }

                results.Add(orientedBounds);
            }
        }

        // Token: 0x06002BA2 RID: 11170 RVA: 0x001046D8 File Offset: 0x001028D8
        public static bool CheckSpace(Vector3 position, Quaternion rotation, Vector3 extents, int layerMask, Collider allowedCollider)
        {
            if (extents.x <= 0f || extents.y <= 0f || extents.z <= 0f)
            {
                return true;
            }

            int num = Physics.OverlapBoxNonAlloc(position, extents, MultiplayerBuilder.sColliders, rotation, layerMask, QueryTriggerInteraction.Ignore);
            return num == 0 || (num == 1 && MultiplayerBuilder.sColliders[0] == allowedCollider);
        }

        // Token: 0x06002BA3 RID: 11171 RVA: 0x00104750 File Offset: 0x00102950
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
                if (!MultiplayerBuilder.CheckSpace(orientedBounds.position, orientedBounds.rotation, orientedBounds.extents, layerMask, allowedCollider))
                {
                    return false;
                }
            }

            return true;
        }

        // Token: 0x06002BA4 RID: 11172 RVA: 0x00104800 File Offset: 0x00102A00
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

        // Token: 0x06002E52 RID: 11858 RVA: 0x00102B84 File Offset: 0x00100D84
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

        // Token: 0x06002BA6 RID: 11174 RVA: 0x0001E995 File Offset: 0x0001CB95
        public static void GetOverlappedObjects(Vector3 position, Quaternion rotation, Vector3 extents, List<GameObject> results)
        {
            MultiplayerBuilder.GetOverlappedColliders(position, rotation, extents, MultiplayerBuilder.sCollidersList);
            MultiplayerBuilder.GetRootObjects(MultiplayerBuilder.sCollidersList, results);
            MultiplayerBuilder.sCollidersList.Clear();
        }

        // Token: 0x06002BA7 RID: 11175 RVA: 0x001048CC File Offset: 0x00102ACC
        public static void GetObstacles(Vector3 position, Quaternion rotation, List<OrientedBounds> localBounds, List<GameObject> results)
        {
            results.Clear();
            if (rotation.IsDistinguishedIdentity())
            {
                rotation = Quaternion.identity;
            }

            List<GameObject> list = new List<GameObject>();
            for (int i = 0; i < localBounds.Count; i++)
            {
                OrientedBounds orientedBounds = localBounds[i];
                if (orientedBounds.rotation.IsDistinguishedIdentity())
                {
                    orientedBounds.rotation = Quaternion.identity;
                }

                orientedBounds.position = position + rotation * orientedBounds.position;
                orientedBounds.rotation = rotation * orientedBounds.rotation;
                MultiplayerBuilder.GetOverlappedColliders(orientedBounds.position, orientedBounds.rotation, orientedBounds.extents, MultiplayerBuilder.sCollidersList);
                MultiplayerBuilder.GetRootObjects(MultiplayerBuilder.sCollidersList, list);
                for (int j = list.Count - 1; j >= 0; j--)
                {
                    GameObject go = list[j];
                    if (!MultiplayerBuilder.IsObstacle(go))
                    {
                        list.RemoveAt(j);
                    }
                }

                for (int k = 0; k < MultiplayerBuilder.sCollidersList.Count; k++)
                {
                    Collider collider = MultiplayerBuilder.sCollidersList[k];
                    if (MultiplayerBuilder.IsObstacle(collider))
                    {
                        GameObject gameObject = collider.gameObject;
                        if (!list.Contains(gameObject))
                        {
                            list.Add(gameObject);
                        }
                    }
                }

                MultiplayerBuilder.sCollidersList.Clear();
                for (int l = 0; l < list.Count; l++)
                {
                    GameObject item = list[l];
                    if (!results.Contains(item))
                    {
                        results.Add(item);
                    }
                }
            }
        }

        // Token: 0x06002BA8 RID: 11176 RVA: 0x00104A5C File Offset: 0x00102C5C
        public static bool CanDestroyObject(GameObject go)
        {
            Player componentInParent = go.GetComponentInParent<Player>();
            if (componentInParent != null)
            {
                return false;
            }

            LargeWorldEntity component = go.GetComponent<LargeWorldEntity>();
            if (component != null && component.cellLevel >= LargeWorldEntity.CellLevel.Global)
            {
                return false;
            }

            SubRoot componentInParent2 = go.GetComponentInParent<SubRoot>();
            if (componentInParent2 != null)
            {
                return false;
            }

            Constructable componentInParent3 = go.GetComponentInParent<Constructable>();
            if (componentInParent3 != null)
            {
                return false;
            }

            IObstacle component2 = go.GetComponent<IObstacle>();
            if (component2 != null)
            {
                return false;
            }

            Pickupable component3 = go.GetComponent<Pickupable>();
            if (component3 != null && component3.attached)
            {
                return false;
            }

            PlaceTool component4 = go.GetComponent<PlaceTool>();
            return !(component4 != null);
        }

        // Token: 0x06002BA9 RID: 11177 RVA: 0x00104B18 File Offset: 0x00102D18
        public static bool IsObstacle(Collider collider)
        {
            if (collider != null)
            {
                GameObject gameObject = collider.gameObject;
                if (gameObject.layer == LayerID.TerrainCollider)
                {
                    return true;
                }
            }

            return false;
        }

        // Token: 0x06002BAA RID: 11178 RVA: 0x00104B4C File Offset: 0x00102D4C
        public static bool IsObstacle(GameObject go)
        {
            return go.GetComponent<IObstacle>() != null;
        }

        // Token: 0x06002BAB RID: 11179 RVA: 0x00002AA3 File Offset: 0x00000CA3
        public static Transform GetAimTransform()
        {
            return overrideTransform;
        }

        // Token: 0x06002BAC RID: 11180 RVA: 0x0001E9B9 File Offset: 0x0001CBB9
        public static GameObject GetGhostModel()
        {
            return MultiplayerBuilder.ghostModel;
        }

        // Token: 0x06002BAD RID: 11181 RVA: 0x00104B6C File Offset: 0x00102D6C
        private static bool CheckAsSubModule()
        {
            if (!Constructable.CheckFlags(MultiplayerBuilder.allowedInBase, MultiplayerBuilder.allowedInSub, MultiplayerBuilder.allowedOutside))
            {
                return false;
            }

            Transform aimTransform = MultiplayerBuilder.GetAimTransform();
            MultiplayerBuilder.placementTarget = null;
            RaycastHit hit;
            if (!Physics.Raycast(aimTransform.position, aimTransform.forward, out hit, MultiplayerBuilder.placeMaxDistance, MultiplayerBuilder.placeLayerMask.value, QueryTriggerInteraction.Ignore))
            {
                return false;
            }

            MultiplayerBuilder.placementTarget = hit.collider.gameObject;
            MultiplayerBuilder.SetPlaceOnSurface(hit, ref MultiplayerBuilder.placePosition, ref MultiplayerBuilder.placeRotation);
            if (!MultiplayerBuilder.CheckTag(hit.collider))
            {
                return false;
            }

            if (!MultiplayerBuilder.CheckSurfaceType(MultiplayerBuilder.GetSurfaceType(hit.normal)))
            {
                return false;
            }

            if (!MultiplayerBuilder.CheckDistance(hit.point, MultiplayerBuilder.placeMinDistance))
            {
                return false;
            }

            if (!MultiplayerBuilder.allowedOnConstructables && MultiplayerBuilder.HasComponent<Constructable>(hit.collider.gameObject))
            {
                return false;
            }

            if (!Player.main.IsInSub())
            {
                GameObject entityRoot = UWE.Utils.GetEntityRoot(MultiplayerBuilder.placementTarget);
                if (!entityRoot)
                {
                    entityRoot = MultiplayerBuilder.placementTarget;
                }

                if (!MultiplayerBuilder.ValidateOutdoor(entityRoot))
                {
                    return false;
                }
            }

            return MultiplayerBuilder.CheckSpace(MultiplayerBuilder.placePosition, MultiplayerBuilder.placeRotation, MultiplayerBuilder.bounds, MultiplayerBuilder.placeLayerMask.value, hit.collider);
        }

        // Token: 0x06002BAE RID: 11182 RVA: 0x0001E9C0 File Offset: 0x0001CBC0
        private static SurfaceType GetSurfaceType(Vector3 hitNormal)
        {
            if ((double)hitNormal.y < -0.33)
            {
                return SurfaceType.Ceiling;
            }

            if ((double)hitNormal.y < 0.33)
            {
                return SurfaceType.Wall;
            }

            return SurfaceType.Ground;
        }

        // Token: 0x06002BAF RID: 11183 RVA: 0x00104CB8 File Offset: 0x00102EB8
        private static bool CheckTag(Collider c)
        {
            if (c == null)
            {
                return false;
            }

            GameObject gameObject = c.gameObject;
            return !(gameObject == null) && !gameObject.CompareTag(MultiplayerBuilder.ignoreTag);
        }

        // Token: 0x06002BB0 RID: 11184 RVA: 0x0001E9F3 File Offset: 0x0001CBF3
        private static bool CheckSurfaceType(SurfaceType surfaceType)
        {
            return MultiplayerBuilder.allowedSurfaceTypes.Contains(surfaceType);
        }

        // Token: 0x06002BB1 RID: 11185 RVA: 0x00104CFC File Offset: 0x00102EFC
        private static bool CheckDistance(Vector3 worldPosition, float minDistance)
        {
            Transform aimTransform = MultiplayerBuilder.GetAimTransform();
            float magnitude = (worldPosition - aimTransform.position).magnitude;
            return magnitude >= minDistance;
        }

        // Token: 0x06002BB2 RID: 11186 RVA: 0x0001EA00 File Offset: 0x0001CC00
        private static bool HasComponent<T>(GameObject go) where T : Component
        {
            return go.GetComponentInParent<T>() != null;
        }

        // Token: 0x06002BB3 RID: 11187 RVA: 0x00104D2C File Offset: 0x00102F2C
        private static void SetDefaultPlaceTransform(ref Vector3 position, ref Quaternion rotation)
        {
            Transform aimTransform = MultiplayerBuilder.GetAimTransform();
            position = aimTransform.position + aimTransform.forward * MultiplayerBuilder.placeDefaultDistance;
            Vector3 forward;
            Vector3 up;
            if (MultiplayerBuilder.forceUpright)
            {
                forward = -aimTransform.forward;
                forward.y = 0f;
                forward.Normalize();
                up = Vector3.up;
            }
            else
            {
                forward = -aimTransform.forward;
                up = aimTransform.up;
            }

            rotation = Quaternion.LookRotation(forward, up);
            if (MultiplayerBuilder.rotationEnabled)
            {
                rotation = Quaternion.AngleAxis(MultiplayerBuilder.additiveRotation, up) * rotation;
            }

            if (MultiplayerBuilder.overridePosition != null)
            {
                position = MultiplayerBuilder.overridePosition;
            }

            if (MultiplayerBuilder.overrideQuaternion != null)
            {
                rotation = MultiplayerBuilder.overrideQuaternion;
            }
        }

        // Token: 0x06002BB4 RID: 11188 RVA: 0x00104DDC File Offset: 0x00102FDC
        private static void SetPlaceOnSurface(RaycastHit hit, ref Vector3 position, ref Quaternion rotation)
        {
            Transform aimTransform = MultiplayerBuilder.GetAimTransform();
            Vector3 vector = Vector3.forward;
            Vector3 vector2 = Vector3.up;
            if (MultiplayerBuilder.forceUpright)
            {
                vector = -aimTransform.forward;
                vector.y = 0f;
                vector.Normalize();
                vector2 = Vector3.up;
            }
            else
            {
                SurfaceType surfaceType = MultiplayerBuilder.GetSurfaceType(hit.normal);
                if (surfaceType != SurfaceType.Wall)
                {
                    if (surfaceType != SurfaceType.Ceiling)
                    {
                        if (surfaceType == SurfaceType.Ground)
                        {
                            vector2 = hit.normal;
                            vector = -aimTransform.forward;
                            vector.y -= Vector3.Dot(vector, vector2);
                            vector.Normalize();
                        }
                    }
                    else
                    {
                        vector = hit.normal;
                        vector2 = -aimTransform.forward;
                        vector2.y -= Vector3.Dot(vector2, vector);
                        vector2.Normalize();
                    }
                }
                else
                {
                    vector = hit.normal;
                    vector2 = Vector3.up;
                }
            }

            position = hit.point;
            rotation = Quaternion.LookRotation(vector, vector2);
            if (MultiplayerBuilder.rotationEnabled)
            {
                rotation = Quaternion.AngleAxis(MultiplayerBuilder.additiveRotation, vector2) * rotation;
            }
        }

        // Token: 0x06002BB5 RID: 11189 RVA: 0x00104F14 File Offset: 0x00103114
        private static void SetupRenderers(GameObject gameObject, bool interior)
        {
            int newLayer;
            if (interior)
            {
                newLayer = LayerMask.NameToLayer("Viewmodel");
            }
            else
            {
                newLayer = LayerMask.NameToLayer("Default");
            }

            global::Utils.SetLayerRecursively(gameObject, newLayer, true, -1);
        }

        // Token: 0x06002BB6 RID: 11190 RVA: 0x00104F4C File Offset: 0x0010314C
        public static bool ValidateOutdoor(GameObject hitObject)
        {
            Rigidbody component = hitObject.GetComponent<Rigidbody>();
            if (component && !component.isKinematic)
            {
                return false;
            }

            SubRoot component2 = hitObject.GetComponent<SubRoot>();
            Base component3 = hitObject.GetComponent<Base>();
            if (component2 != null && component3 == null)
            {
                return false;
            }

            Pickupable component4 = hitObject.GetComponent<Pickupable>();
            if (component4 != null)
            {
                return false;
            }

            LiveMixin component5 = hitObject.GetComponent<LiveMixin>();
            return !(component5 != null) || !component5.destroyOnDeath;
        }

        // Token: 0x06002BB7 RID: 11191 RVA: 0x00104FDC File Offset: 0x001031DC
        private static void CreatePowerPreview(TechType constructableTechType, GameObject ghostModel)
        {
            GameObject gameObject = null;
            string poweredPrefabName = CraftData.GetPoweredPrefabName(constructableTechType);
            if (poweredPrefabName != string.Empty)
            {
                gameObject = PrefabDatabase.GetPrefabForFilename(poweredPrefabName);
            }

            if (gameObject != null)
            {
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
        }

        public static Vector3 overridePosition;

        public static Quaternion overrideQuaternion;

        public static Transform overrideTransform;

        // Token: 0x04002A71 RID: 10865
        public static readonly float additiveRotationSpeed = 90f;

        // Token: 0x04002A72 RID: 10866
        public static readonly GameInput.Button buttonRotateCW = GameInput.Button.CyclePrev;

        // Token: 0x04002A73 RID: 10867
        public static readonly GameInput.Button buttonRotateCCW = GameInput.Button.CycleNext;

        // Token: 0x04002A74 RID: 10868
        private static readonly Vector3[] checkDirections = new Vector3[]
        {
        Vector3.up,
        Vector3.down,
        Vector3.left,
        Vector3.right
        };

        // Token: 0x04002A75 RID: 10869
        private static readonly Color placeColorAllow = new Color(0f, 1f, 0f, 1f);

        // Token: 0x04002A76 RID: 10870
        private static readonly Color placeColorDeny = new Color(1f, 0f, 0f, 1f);

        // Token: 0x04002A77 RID: 10871
        private static readonly string ignoreTag = "DenyBuilding";

        // Token: 0x04002A78 RID: 10872
        private static bool initialized;

        // Token: 0x04002A79 RID: 10873
        private static BuildModeInputHandler inputHandler = new BuildModeInputHandler();

        // Token: 0x04002A7A RID: 10874
        private static Collider[] sColliders = new Collider[2];

        // Token: 0x04002A7B RID: 10875
        private static List<Collider> sCollidersList = new List<Collider>();

        // Token: 0x04002A7C RID: 10876
        public static float additiveRotation;

        // Token: 0x04002A7D RID: 10877
        private static GameObject prefab;

        // Token: 0x04002A7E RID: 10878
        private static float placeMaxDistance;

        // Token: 0x04002A7F RID: 10879
        private static float placeMinDistance;

        // Token: 0x04002A80 RID: 10880
        private static float placeDefaultDistance;

        // Token: 0x04002A81 RID: 10881
        private static TechType constructableTechType;

        // Token: 0x04002A82 RID: 10882
        private static List<SurfaceType> allowedSurfaceTypes;

        // Token: 0x04002A83 RID: 10883
        private static bool forceUpright;

        // Token: 0x04002A84 RID: 10884
        private static bool allowedInSub;

        // Token: 0x04002A85 RID: 10885
        private static bool allowedInBase;

        // Token: 0x04002A86 RID: 10886
        private static bool allowedOutside;

        // Token: 0x04002A87 RID: 10887
        private static bool allowedOnConstructables;

        // Token: 0x04002A88 RID: 10888
        private static bool rotationEnabled;

        // Token: 0x04002A89 RID: 10889
        private static Renderer[] renderers;

        // Token: 0x04002A8A RID: 10890
        private static GameObject ghostModel;

        // Token: 0x04002A8B RID: 10891
        private static Vector3 ghostModelPosition;

        // Token: 0x04002A8C RID: 10892
        private static Quaternion ghostModelRotation;

        // Token: 0x04002A8D RID: 10893
        private static Vector3 ghostModelScale;

        // Token: 0x04002A8E RID: 10894
        private static List<OrientedBounds> bounds = new List<OrientedBounds>();

        // Token: 0x04002A8F RID: 10895
        private static Bounds _aaBounds = default(Bounds);

        // Token: 0x04002A90 RID: 10896
        public static Vector3 placePosition;

        // Token: 0x04002A91 RID: 10897
        public static Quaternion placeRotation;

        public static Optional<RotationMetadata> rotationMetadata;

        // Token: 0x04002A92 RID: 10898
        private static Material ghostStructureMaterial;

        // Token: 0x04002A93 RID: 10899
        private static LayerMask placeLayerMask;

        // Token: 0x04002A94 RID: 10900
        private static GameObject placementTarget;

        // Token: 0x04002A95 RID: 10901
        private static string placeSound = "event:/tools/builder/place";
    }
}
#pragma warning restore // Re-enable all warnings for copied file
