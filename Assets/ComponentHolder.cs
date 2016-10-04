using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(InteractiveObject))]
public class ComponentHolder : MonoBehaviour {

    [System.Serializable]
    public class ComponentSlot {
        public ComponentObject.ComponentType SlotType;
        public int SlotTypeID = 0;
        public bool IsRequired = false;
        public Button Button;
        public ComponentObject HeldComponent;

        [HideInInspector]
        public float CurrentEfficiency = 0;
    }
    [SerializeField]
    private bool FillSlotsOnStart = true;
    public List<ComponentSlot> ComponentSlots;

    [HideInInspector]
    public float CurrentEfficiency = 0;
    [HideInInspector]
    public InteractiveObject IO;


    void Awake() {
        IO = GetComponent<InteractiveObject>();
        if (IO == null) {
            throw new System.Exception(name + " is missing required stuff!");
        }
    }

    void Start() {
        if (FillSlotsOnStart) {
            for (int i = 0; i < ComponentSlots.Count; i++) {
                if (ComponentSlots[i].HeldComponent != null)
                    continue;

                GameObject _obj = Instantiate(ComponentManager.Instance.GetComponentPrefab(ComponentSlots[i].SlotType, ComponentSlots[i].SlotTypeID), Vector3.zero, Quaternion.identity) as GameObject;
                _obj.transform.parent = transform;
                _obj.GetComponent<InteractiveObject>().Hide(true);
                ComponentSlots[i].HeldComponent = _obj.GetComponent<ComponentObject>();
                ComponentSlots[i].CurrentEfficiency = 1;
            }
        }

        StartCoroutine(_Deteriorate());
        OnComponentsModified();
    }

    IEnumerator _Deteriorate() {
        while (true) {
            for (int i = 0; i < ComponentSlots.Count; i++) {
                if (ComponentSlots[i].HeldComponent == null)
                    continue;
                if (ComponentSlots[i].HeldComponent._Current_ == 0 || ComponentSlots[i].HeldComponent.StaticInfo.Deterioration_OverTime == 0)
                    continue;

                ComponentSlots[i].HeldComponent._Current_ -= ComponentSlots[i].HeldComponent.StaticInfo.Deterioration_OverTime;
                if (ComponentSlots[i].HeldComponent._Current_ == 0) {
                    ComponentSlots[i].CurrentEfficiency = 0;
                    OnComponentsModified();
                }
                yield return new WaitForSeconds(0.1f);
            }
            yield return null;
        }
    }

    public virtual bool CanUse() {
        for (int i = 0; i < ComponentSlots.Count; i++) {
            if (ComponentSlots[i].IsRequired && (ComponentSlots[i].HeldComponent == null || ComponentSlots[i].HeldComponent._Current_ == 0))
                return false;
        }

        for (int i = 0; i < ComponentSlots.Count; i++) {
            if (ComponentSlots[i].HeldComponent == null || ComponentSlots[i].HeldComponent.StaticInfo.Deterioration_PerUse == 0)
                continue;

            ComponentSlots[i].HeldComponent._Current_ -= ComponentSlots[i].HeldComponent.StaticInfo.Deterioration_PerUse;
        }

        return true;
    }

    public void OnComponentButtonClicked(int _index) {
        // determine if the mouse's component is sufficient and what efficiency it will have
        float _newComponentEfficiency = 0;
        ComponentObject _cObj = null;
        if (Mouse.Instance.PickedUpObject != null) {
            _cObj = Mouse.Instance.PickedUpObject.GetComponent<ComponentObject>();
            if (_cObj == null)
                return;
            if ((ComponentSlots[_index].SlotType != _cObj.Type && _cObj.StaticInfo.EfficiencyInOtherSlots.Find(x => x.Type == ComponentSlots[_index].SlotType) == null))
                return; // stop if the held component doesn't match the slot and can't be jury-rigged into the slot

            // determine how good a match the component is
            if (ComponentSlots[_index].SlotTypeID == _cObj.TypeID)
                _newComponentEfficiency = 1;
            else {
                int _effIndex = _cObj.StaticInfo.EfficiencyInOtherSlots.FindIndex(x => x.Type == ComponentSlots[_index].SlotType);
                if (_effIndex > -1)
                    _newComponentEfficiency = _cObj.StaticInfo.EfficiencyInOtherSlots[_effIndex].Efficiency;
            }

            if (_newComponentEfficiency == 0)
                return; // stop if the held component is as good as a non-match
        }

        // detach component from slot
        bool _changedComponents = false;
        InteractiveObject _oldIO = null;
        if (ComponentSlots[_index].HeldComponent != null) {
            _changedComponents = true;

            _oldIO = ComponentSlots[_index].HeldComponent.GetComponent<InteractiveObject>();
            ComponentSlots[_index].HeldComponent = null;
            ComponentSlots[_index].CurrentEfficiency = 0;
        }

        // switch mouse's component for slot's component
        InteractiveObject _newIO;
        if (Mouse.Instance.TrySwitchComponents(_oldIO, false, false, out _newIO) && _newIO != null) {
            _changedComponents = true;

            ComponentSlots[_index].HeldComponent = _newIO.GetComponent<ComponentObject>();
            ComponentSlots[_index].CurrentEfficiency = _newComponentEfficiency;
        }

        // tell everyone the great news
        if (_changedComponents)
            OnComponentsModified();
    }

    public void OnComponentsModified() {
        CalculateEfficiency();
        GUIManager.Instance.UpdateButtonGraphics(IO);
    }
    void CalculateEfficiency() {
        float _efficiency = 0;
        for (int i = 0; i < ComponentSlots.Count; i++)
            _efficiency += ComponentSlots[i].CurrentEfficiency;
        _efficiency /= ComponentSlots.Count;
        CurrentEfficiency = _efficiency;
    }
}
