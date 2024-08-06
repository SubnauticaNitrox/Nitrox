using System.Collections.Generic;
using System.Linq;
using NitroxClient.Communication;
using NitroxClient.GameLogic;
using NitroxClient.GameLogic.PlayerLogic.PlayerModel.Abstract;
using NitroxModel.Packets;
using UnityEngine;

namespace NitroxClient.MonoBehaviours.Cyclops;

/// <summary>
/// Script responsible for creating a virtual counterpart of every cyclops, which will always be horizontal and motionless so that simulated movement is always clear.
/// Contains a pawn for each player entering the regular cyclops.
/// </summary>
public class VirtualCyclops : MonoBehaviour
{
    private static GameObject Prefab;
    private static float Offset;
    public const string NAME = "VirtualCyclops";

    public static readonly Dictionary<GameObject, VirtualCyclops> VirtualCyclopsByObject = [];

    private readonly Dictionary<string, Openable> openableByName = [];
    private readonly Dictionary<GameObject, GameObject> virtualConstructableByRealGameObject = [];
    private readonly Dictionary<TechType, GameObject> cacheColliderCopy = [];
    public readonly Dictionary<INitroxPlayer, CyclopsPawn> Pawns = [];
    public NitroxCyclops Cyclops;
    public Transform axis;

    private Rigidbody rigidbody;
    private Vector3 InitialPosition;
    private Quaternion InitialRotation;

    public static void Initialize()
    {
        CreateVirtualPrefab();
    }

    /// <summary>
    /// Initializes the <see cref="Prefab"/> object with reduced utility to ensure the virtual cyclops won't be eating too much performance.
    /// </summary>
    public static void CreateVirtualPrefab()
    {
        if (Prefab)
        {
            return;
        }

        LightmappedPrefabs.main.RequestScenePrefab("cyclops", (cyclopsPrefab) =>
        {
            SubConsoleCommand.main.OnSubPrefabLoaded(cyclopsPrefab);
            Prefab = SubConsoleCommand.main.GetLastCreatedSub();
            Prefab.name = NAME;
            Prefab.GetComponent<LargeWorldEntity>().enabled = false;
            Prefab.transform.parent = null;

            GameObject.Destroy(Prefab.GetComponent<EcoTarget>());
            GameObject.Destroy(Prefab.GetComponent<PingInstance>());
            Prefab.AddComponent<VirtualCyclops>();
            Prefab.SetActive(false);
        });
    }

    /// <summary>
    /// Instantiates the <see cref="Prefab"/> object associated with a regular cyclops and links references where required.
    /// </summary>
    public static VirtualCyclops CreateVirtualInstance(GameObject cyclopsObject)
    {
        if (!VirtualCyclopsByObject.TryGetValue(cyclopsObject, out VirtualCyclops virtualCyclops))
        {
            Vector3 position = Vector3.up * 500 + Vector3.right * (Offset - 100);
            Offset += 10;
            Quaternion rotation = Quaternion.identity;

            GameObject instance = GameObject.Instantiate(Prefab, position, rotation, false);
            instance.name = NAME;
            LargeWorldEntity.Register(instance);
            virtualCyclops = instance.GetComponent<VirtualCyclops>();
            virtualCyclops.Cyclops = cyclopsObject.GetComponent<NitroxCyclops>();
            virtualCyclops.axis = instance.GetComponent<SubRoot>().subAxis;
            virtualCyclops.RegisterOpenables();
            virtualCyclops.InitialPosition = position;
            virtualCyclops.InitialRotation = rotation;
            virtualCyclops.rigidbody = instance.GetComponent<Rigidbody>();
            virtualCyclops.rigidbody.constraints = RigidbodyConstraints.FreezeAll;

            instance.GetComponent<WorldForces>().enabled = false;
            instance.GetComponent<WorldForces>().lockInterpolation = false;
            instance.GetComponent<Stabilizer>().stabilizerEnabled = false;
            instance.GetComponent<Rigidbody>().isKinematic = true;

            instance.SetActive(true);
            virtualCyclops.ToggleRenderers(false);
            VirtualCyclopsByObject.Add(cyclopsObject, virtualCyclops);
        }
        return virtualCyclops;
    }

    /// <summary>
    /// Destroys the virtual cyclops instance associated with a regular cyclops object.
    /// </summary>
    public static void Terminate(GameObject cyclopsObject)
    {
        if (VirtualCyclopsByObject.TryGetValue(cyclopsObject, out VirtualCyclops associatedVirtualCyclops))
        {
            Destroy(associatedVirtualCyclops.gameObject);
            VirtualCyclopsByObject.Remove(cyclopsObject);
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

    public CyclopsPawn AddPawnForPlayer(INitroxPlayer player)
    {
        if (!Pawns.TryGetValue(player, out CyclopsPawn pawn))
        {
            pawn = new(player, this, Cyclops.transform);
            Pawns.Add(player, pawn);
        }
        return pawn;
    }

    public void RemovePawnForPlayer(INitroxPlayer player)
    {
        if (Pawns.TryGetValue(player, out CyclopsPawn pawn))
        {
            pawn.Terminate();
        }
        Pawns.Remove(player);
    }

    public void RegisterOpenables()
    {
        foreach (Openable openable in transform.GetComponentsInChildren<Openable>(true))
        {
            openableByName.Add(openable.name, openable);
        }
    }

    public void ReplicateOpening(Openable openable, bool openState)
    {
        if (openableByName.TryGetValue(openable.name, out Openable virtualOpenable))
        {
            using (PacketSuppressor<OpenableStateChanged>.Suppress())
            {
                virtualOpenable.PlayOpenAnimation(openState, virtualOpenable.animTime);
            }
        }
    }

    public void ReplicateConstructable(Constructable constructable)
    {
        if (virtualConstructableByRealGameObject.ContainsKey(constructable.gameObject))
        {
            return;
        }
        GameObject colliderCopy = CreateColliderCopy(constructable.gameObject, constructable.techType);
        // WorldPositionStays is set to false so we keep the same local parameters
        colliderCopy.transform.parent = transform;
        colliderCopy.transform.localPosition = constructable.transform.localPosition;
        colliderCopy.transform.localRotation = constructable.transform.localRotation;
        colliderCopy.transform.localScale = constructable.transform.localScale;

        virtualConstructableByRealGameObject.Add(constructable.gameObject, colliderCopy);
    }

    /// <summary>
    /// Creates an empty shell simulating the presence of modules by copying its children containing a collider.
    /// </summary>
    public GameObject CreateColliderCopy(GameObject realObject, TechType techType)
    {
        if (cacheColliderCopy.TryGetValue(techType, out GameObject colliderCopy))
        {
            return GameObject.Instantiate(colliderCopy);
        }
        colliderCopy = new GameObject($"{realObject.name}-collidercopy");

        Transform transform = realObject.transform;

        Dictionary<Transform, Transform> copiedTransformByRealTransform = [];
        copiedTransformByRealTransform[transform] = colliderCopy.transform;

        IEnumerable<GameObject> uniqueColliderObjects = realObject.GetComponentsInChildren<Collider>(true).Select(c => c.gameObject).Distinct();
        foreach (GameObject colliderObject in uniqueColliderObjects)
        {
            GameObject copiedCollider = Instantiate(colliderObject);
            copiedCollider.name = colliderObject.name;

            // "child" is always a copied transform looking for its copied parent
            Transform child = copiedCollider.transform;
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
