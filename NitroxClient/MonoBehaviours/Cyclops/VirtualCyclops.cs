using System.Collections;
using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication;
using NitroxClient.GameLogic.Spawning.WorldEntities;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Cyclops;

/// <summary>
/// Script responsible for creating a virtual counterpart of every cyclops, which will always be horizontal and motionless so that simulated movement is always clear.
/// Contains a pawn for each player entering the regular cyclops.
/// </summary>
public class VirtualCyclops : MonoBehaviour
{
    public static VirtualCyclops Instance;
    public const string NAME = "VirtualCyclops";

    private static readonly Dictionary<TechType, GameObject> cacheColliderCopy = [];
    private readonly Dictionary<string, Openable> virtualOpenableByName = [];
    private readonly Dictionary<string, Openable> realOpenableByName = [];
    private readonly Dictionary<GameObject, GameObject> virtualConstructableByRealGameObject = [];
    public NitroxCyclops Cyclops;
    public Transform axis;

    private Rigidbody rigidbody;
    private Vector3 InitialPosition;
    private Quaternion InitialRotation;

    public static void Initialize()
    {
        CreateVirtualCyclops();
        Multiplayer.OnAfterMultiplayerEnd += Dispose;
    }

    public static void Dispose()
    {
        Destroy(Instance.gameObject);
        Instance = null;
        Multiplayer.OnAfterMultiplayerEnd -= Dispose;
    }

    /// <summary>
    /// Initializes the <see cref="Prefab"/> object with reduced utility to ensure the virtual cyclops won't be eating too much performance.
    /// </summary>
    public static void CreateVirtualCyclops()
    {
        if (Instance)
        {
            return;
        }

        LightmappedPrefabs.main.RequestScenePrefab("cyclops", (cyclopsPrefab) =>
        {
            SubConsoleCommand.main.OnSubPrefabLoaded(cyclopsPrefab);
            GameObject model = SubConsoleCommand.main.GetLastCreatedSub();
            model.name = NAME;
            Vector3 position = Vector3.up * 500;
            Quaternion rotation = Quaternion.identity;
            model.transform.position = position;
            model.transform.rotation = rotation;

            Instance = model.AddComponent<VirtualCyclops>();

            Instance.axis = model.GetComponent<SubRoot>().subAxis;

            GameObject.Destroy(model.GetComponent<EcoTarget>());
            GameObject.Destroy(model.GetComponent<PingInstance>());
            GameObject.Destroy(model.GetComponent<CyclopsDestructionEvent>());
            GameObject.Destroy(model.GetComponent<VFXConstructing>());
                        
            Instance.InitialPosition = position;
            Instance.InitialRotation = rotation;
            Instance.rigidbody = Instance.GetComponent<Rigidbody>();
            Instance.rigidbody.constraints = RigidbodyConstraints.FreezeAll;

            model.GetComponent<WorldForces>().enabled = false;
            model.GetComponent<WorldForces>().lockInterpolation = false;
            model.GetComponent<Stabilizer>().stabilizerEnabled = false;
            model.GetComponent<Rigidbody>().isKinematic = true;
            model.GetComponent<LiveMixin>().invincible = true;

            Instance.RegisterVirtualOpenables();
            Instance.ToggleRenderers(false);
            Instance.DisableBadComponents();

            model.SetActive(true);
        });
    }

    public static IEnumerator InitializeConstructablesCache()
    {
        List<TechType> constructableTechTypes = [];
        CraftData.GetBuilderGroupTech(TechGroup.InteriorModules, constructableTechTypes, true);
        CraftData.GetBuilderGroupTech(TechGroup.Miscellaneous, constructableTechTypes, true);

        TaskResult<GameObject> result = new();
        foreach (TechType techType in constructableTechTypes)
        {
            yield return DefaultWorldEntitySpawner.RequestPrefab(techType, result);
            if (result.value && result.value.GetComponent<Constructable>())
            {
                // We immediately destroy the copy because we only want to cache it for now
                Destroy(CreateColliderCopy(result.value, techType));
            }
        }
    }

    public void Populate()
    {
        foreach (Constructable constructable in Cyclops.GetComponentsInChildren<Constructable>(true))
        {
            ReplicateConstructable(constructable);
        }

        foreach (Openable openable in Cyclops.GetComponentsInChildren<Openable>(true))
        {
            openable.blocked = false;
            ReplicateOpening(openable, openable.isOpen);
            realOpenableByName.Add(openable.name, openable);
        }
    }

    public void Depopulate()
    {
        foreach (GameObject virtualObject in virtualConstructableByRealGameObject.Values)
        {
            Destroy(virtualObject);
        }
        virtualConstructableByRealGameObject.Clear();
        
        foreach (Openable openable in realOpenableByName.Values)
        {
            openable.blocked = false;
        }
        realOpenableByName.Clear();
    }

    public void SetCurrentCyclops(NitroxCyclops nitroxCyclops)
    {
        if (Cyclops)
        {
            Cyclops.Virtual = null;
            Depopulate();
            Cyclops = null;
        }

        Cyclops = nitroxCyclops;
        if (Cyclops)
        {
            Populate();
        }
    }

    public void Update()
    {
        transform.position = InitialPosition;
        transform.rotation = InitialRotation;
    }

    public void ToggleRenderers(bool toggled)
    {
        foreach (Renderer renderer in transform.GetComponentsInChildren<Renderer>(true))
        {
            renderer.enabled = toggled;
        }
    }

    private void RegisterVirtualOpenables()
    {
        foreach (Openable openable in transform.GetComponentsInChildren<Openable>(true))
        {
            virtualOpenableByName.Add(openable.name, openable);
        }
    }

    private void DisableBadComponents()
    {
        CyclopsLightingPanel cyclopsLightingPanel = GetComponentInChildren<CyclopsLightingPanel>(true);
        cyclopsLightingPanel.floodlightsOn = false;
        cyclopsLightingPanel.lightingOn = false;
        cyclopsLightingPanel.SetExternalLighting(false);
        cyclopsLightingPanel.cyclopsRoot.ForceLightingState(false);
        cyclopsLightingPanel.enabled = false;
        Destroy(cyclopsLightingPanel);

        // Disable a source of useless logs
        foreach (FMOD_CustomEmitter customEmitter in GetComponentsInChildren<FMOD_CustomEmitter>(true))
        {
            customEmitter.enabled = false;
        }
        foreach (PlayerCinematicController cinematicController in GetComponentsInChildren<PlayerCinematicController>(true))
        {
            cinematicController.enabled = false;
        }
    }

    public void ReplicateOpening(Openable openable, bool openState)
    {
        if (virtualOpenableByName.TryGetValue(openable.name, out Openable virtualOpenable))
        {
            using (PacketSuppressor<OpenableStateChanged>.Suppress())
            {
                virtualOpenable.PlayOpenAnimation(openState, virtualOpenable.animTime);
            }
        }
    }

    public void ReplicateBlock(Openable openable, bool blockState)
    {
        if (realOpenableByName.TryGetValue(openable.name, out Openable realOpenable))
        {
            realOpenable.blocked = blockState;
        }
    }

    public void ReplicateConstructable(Constructable constructable)
    {
        if (virtualConstructableByRealGameObject.ContainsKey(constructable.gameObject))
        {
            return;
        }
        GameObject colliderCopy = CreateColliderCopy(constructable.gameObject, constructable.techType);
        colliderCopy.transform.parent = transform;
        colliderCopy.transform.CopyLocals(constructable.transform);

        virtualConstructableByRealGameObject.Add(constructable.gameObject, colliderCopy);
    }

    /// <summary>
    /// Creates an empty shell simulating the presence of modules by copying its children containing a collider.
    /// </summary>
    public static GameObject CreateColliderCopy(GameObject realObject, TechType techType)
    {
        if (cacheColliderCopy.TryGetValue(techType, out GameObject colliderCopy))
        {
            return GameObject.Instantiate(colliderCopy);
        }
        colliderCopy = new GameObject($"{realObject.name}-collidercopy");
        // This will act as a prefab but will stay in the material world so we put it out of hands in the meantime
        colliderCopy.transform.position = Vector3.up * 1000 + Vector3.right * 10 * cacheColliderCopy.Count;

        Transform transform = realObject.transform;

        Dictionary<Transform, Transform> copiedTransformByRealTransform = [];
        copiedTransformByRealTransform[transform] = colliderCopy.transform;

        IEnumerable<GameObject> uniqueColliderObjects = realObject.GetComponentsInChildren<Collider>(true).Select(c => c.gameObject).Distinct();
        foreach (GameObject colliderObject in uniqueColliderObjects)
        {
            GameObject copiedColliderObject = new(colliderObject.name);
            copiedColliderObject.transform.CopyLocals(colliderObject.transform);
            foreach (Collider collider in colliderObject.GetComponents<Collider>())
            {
                collider.CopyComponent(copiedColliderObject);
            }

            // "child" is always a copied transform looking for its copied parent
            Transform child = copiedColliderObject.transform;
            // "parent" is always the real parent of the real transform corresponding to "child"
            Transform parent = colliderObject.transform.parent;

            while (!copiedTransformByRealTransform.ContainsKey(parent))
            {
                Transform copiedParent = copiedTransformByRealTransform[parent] = Instantiate(parent);

                child.SetParent(copiedParent, false);

                child = copiedParent;
                parent = parent.parent;
            }

            // At the top of the tree we can simply stick the latest child to the collider
            child.SetParent(colliderCopy.transform, false);
        }

        cacheColliderCopy.Add(techType, colliderCopy);
        return GameObject.Instantiate(colliderCopy);
    }

    public void UnregisterConstructable(GameObject realObject)
    {
        if (virtualConstructableByRealGameObject.TryGetValue(realObject, out GameObject virtualConstructable))
        {
            Destroy(virtualConstructable);
            virtualConstructableByRealGameObject.Remove(realObject);
        }
    }
}
