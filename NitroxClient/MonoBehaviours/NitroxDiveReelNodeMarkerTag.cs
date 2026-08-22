using UnityEngine;

namespace NitroxClient.MonoBehaviours;

/// <summary>
/// Marks a GameObject as a visual-only DiveReel (Pathfinder Tool) node marker spawned by
/// DiveReelNodeMarkers to represent ANOTHER player's trail -- never the local player's own real
/// node. Both a real node (DiveReel.CreateNewNode) and a decorative marker
/// (DiveReelNodeMarkers.SpawnMarkerAsync) are instantiated from the exact same prefab with no
/// parent, so there is no structural way to tell them apart at DiveReelNode.Start() time
/// (Harmony patches the method itself, firing for every instance regardless of origin) without
/// an explicit tag like this one.
/// </summary>
public class NitroxDiveReelNodeMarkerTag : MonoBehaviour
{
}
