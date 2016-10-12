using UnityEngine;
using System.Collections;

public class ComponentManager : MonoBehaviour {
    public static ComponentManager Instance;

    [Header("-----Screws-----")]
    public GameObject Prefab_Screw_0;
    public GameObject Prefab_Screw_1;

    [Header("----Wires-----")]
    public GameObject Prefab_Wire_0;
    public GameObject Prefab_Wire_1;


    void OnEnable() {
        if (Instance)
            return;
        Instance = this;
    }
    void OnDisable() {
        Destroy(Instance);
    }

    public GameObject GetComponentPrefab(ComponentObject.ComponentType type, int id) {
        switch (type) {
            case ComponentObject.ComponentType.Screw:
                if (id == 0)
                    return Prefab_Screw_0;
                else if (id == 1)
                    return Prefab_Screw_1;
                break;
            case ComponentObject.ComponentType.Wire:
                if (id == 0)
                    return Prefab_Wire_0;
                else if (id == 1)
                    return Prefab_Wire_1;
                break;

            default:
                throw new System.NotImplementedException(type.ToString() +  " hasn't been properly implemented yet!");
        }

        return null;
    }
}
