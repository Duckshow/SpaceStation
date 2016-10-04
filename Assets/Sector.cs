using UnityEngine;
using System.Collections.Generic;

public class Sector : MonoBehaviour {

    //public static List<Sector> AllSectors = new List<Sector>();
    //public const int SectorLayer = 10; // CORRECT THIS!!


    //private static bool hasStarted = false;

    //private BoxCollider borders;
    //private List<ResourceContainer> closestResources = new List<ResourceContainer>();


    //void OnEnable() {
    //    AllSectors.Add(this);
    //}
    //void OnDisable() {
    //    AllSectors.Remove(this);
    //}

    //void Awake() {
    //    borders = GetComponent<BoxCollider>();
    //}

    //void Start() {
    //    if (!hasStarted)
    //        StaticStart();
    //}

    //static void StaticStart() {
    //    hasStarted = true;

    //    // add the local resources to each sector (for easy access)
    //    for (int i = 0; i < ResourceContainer.AllResources.Count; i++) {
    //        if (!ResourceContainer.AllResources[i].CanBeTakenFrom)
    //            continue;

    //        GetSectorFromPos(ResourceContainer.AllResources[i].transform.position).closestResources.Add(ResourceContainer.AllResources[i]);
    //    }
    //}

    //public static Sector GetSectorFromPos(Vector3 pos) {
    //    for (int i = 0; i < AllSectors.Count; i++) {
    //        if (AllSectors[i].borders.bounds.Contains(pos))
    //            return AllSectors[i];
    //    }

    //    Debug.LogError("Couldn't find a sector at " + pos + "!");
    //    return null;
    //}

    //public ResourceContainer GetClosestAvailableResource(Unit pawn, ResourceManager.ResourceType type) {
    //    ResourceContainer closest = null;
    //    float shortestDist = Mathf.Infinity;

    //    Dictionary<ResourceContainer, KnowledgeBase.StateInfo> dictionary = pawn.Knowledge.GetResourceContainerStates(type);
    //    foreach (KeyValuePair<ResourceContainer, KnowledgeBase.StateInfo> kvp in dictionary) { // optimization: maybe don't look through ALL the resources?
    //        if (kvp.Value.State == false)
    //            continue;
    //        if (!kvp.Key.CanBeUsedByMany && kvp.Key.AmountUsingThis > 0)
    //            continue;

    //        float newDist = Vector3.Distance(kvp.Key.transform.position, pawn.transform.position);
    //        if (newDist < shortestDist) {
    //            shortestDist = newDist;
    //            closest = kvp.Key;
    //        }
    //    }

    //    return closest;
    //}
}
