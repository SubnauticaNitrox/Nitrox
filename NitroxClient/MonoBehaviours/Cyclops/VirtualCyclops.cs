using System.Collections.Generic;
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
    public readonly Dictionary<INitroxPlayer, CyclopsPawn> Pawns = [];
    public NitroxCyclops Cyclops;
    public Transform axis;

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

            GameObject instance = GameObject.Instantiate(Prefab, position, Quaternion.identity, false);
            instance.name = NAME;
            LargeWorldEntity.Register(instance);
            virtualCyclops = instance.GetComponent<VirtualCyclops>();
            virtualCyclops.Cyclops = cyclopsObject.GetComponent<NitroxCyclops>();
            virtualCyclops.axis = instance.GetComponent<SubRoot>().subAxis;
            virtualCyclops.RegisterOpenables();

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
}
