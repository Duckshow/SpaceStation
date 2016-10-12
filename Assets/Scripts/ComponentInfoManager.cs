using UnityEngine;
using System.Collections.Generic;

public class ComponentInfoManager : MonoBehaviour {

    public static ComponentInfoManager Instance;

    [System.Serializable]
    public class ComponentInfo {
        public ComponentObject.ComponentType Type;
        //public int TypeID;
        public Sprite Image;
        public string Name;
        public string Description;
        public float Deterioration_OverTime = 0.001f;
        public float Deterioration_PerUse = 0.1f;
        public float HP_Max;
        public List<OtherTypeEfficiency> EfficiencyInOtherSlots;
    }
    [System.Serializable]
    public class OtherTypeEfficiency {
        public ComponentObject.ComponentType Type;
        [Range(0, 1)]
        public float Efficiency = 0.8f;
    }
    [Header("The order of components within a type is crucial because TypeID!")][SerializeField]
    private ComponentInfo[] InfoList;

    public Dictionary<ComponentObject.ComponentType, List<ComponentInfo>> AllComponentsInfo = new Dictionary<ComponentObject.ComponentType, List<ComponentInfo>>();

    void Awake() {
        if (Instance)
            Destroy(this);
        Instance = this;

        for (int i = 0; i < InfoList.Length; i++) {
            if (!AllComponentsInfo.ContainsKey(InfoList[i].Type))
                AllComponentsInfo.Add(InfoList[i].Type, new List<ComponentInfo>());

            AllComponentsInfo[InfoList[i].Type].Add(InfoList[i]);
        }
    }
}
