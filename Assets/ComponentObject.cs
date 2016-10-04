using UnityEngine;
using System.Collections.Generic;
using RUL;

public class ComponentObject : MonoBehaviour {

    public enum ComponentType { Screw, Wire }

    public ComponentType Type;
    public int TypeID = 0;

    [Header("**Generated")][SerializeField]
    private float StartQuality;

    [HideInInspector]
    public ComponentInfoManager.ComponentInfo StaticInfo;
    public float _Current_ { get { return current; } set { current = Mathf.Clamp(value, 0, StaticInfo.HP_Max); } }
    [SerializeField] private float current;


    void Awake() {
        StaticInfo = ComponentInfoManager.Instance.AllComponentsInfo[Type][TypeID];
    }
    void Start() {
        // optimization: this is pretty ugly and probably reeeaaally heavy
        // maybe I should handle deterioration through one manager instead?
        StartQuality = Rul.RandElement<float>(new float[] { 1f, 0.9f, 0.8f, 0.7f, 0.6f, 0.5f, 0.4f, 0.3f, 0.2f }, 0.9f, 0.05f, 0.025f);
        StartQuality -= Random.Range(0.001f, 0.009f);
        _Current_ = StaticInfo.HP_Max * StartQuality;
    }
}