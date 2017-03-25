using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using System.Collections;
using System.Collections.Generic;

[RequireComponent(typeof(CanInspect))] [RequireComponent(typeof(TileObject))]
public class ComponentHolder : MonoBehaviour {

    [System.Serializable]
    public class ComponentSlot {
        public ComponentObject.ComponentType SlotType;
        public int SlotTypeID = 0;
        public bool IsRequired = false;
        public Button Button;
        public ComponentObject HeldComponent;

        [HideInInspector] public float CurrentEfficiency = 0;
		[HideInInspector] public ComponentHolder Owner;
    }
    [SerializeField]
    private bool FillSlotsOnStart = true;
    public List<ComponentSlot> ComponentSlots;

    [HideInInspector] public float CurrentEfficiency = 0;
    [HideInInspector] public CanInspect Inspectable;
    [HideInInspector] public TileObject MyTileObject;


    void Awake() {
        Inspectable = GetComponent<CanInspect>();
        MyTileObject = GetComponent<TileObject>();
    }

    void Start() {
        if (FillSlotsOnStart) {
            for (int i = 0; i < ComponentSlots.Count; i++) {
                if (ComponentSlots[i].HeldComponent != null)
                    continue;

                GameObject _obj = Instantiate(ComponentManager.Instance.GetComponentPrefab(ComponentSlots[i].SlotType, ComponentSlots[i].SlotTypeID), Vector3.zero, Quaternion.identity) as GameObject;
                _obj.GetComponent<CanInspect>().Setup();
                _obj.GetComponent<CanInspect> ().PutOffGrid (MyTileObject, Vector3.zero, true);
				ComponentSlots[i].Owner = this;
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

    public void OnClickComponentSlot(int _index) {
		Mouse.Instance.OnClickComponentSlot (ComponentSlots[_index]);

        // tell everyone the great news
        OnComponentsModified();
    }

	public static bool DoesComponentFitInSlot(ComponentSlot _slot, ComponentObject _comp, out float _efficiency){
		_efficiency = 0;

		if ((_slot.SlotType != _comp.Type && _comp.StaticInfo.EfficiencyInOtherSlots.Find(x => x.Type == _slot.SlotType) == null))
			return false; // stop if the held component doesn't match the slot and can't be jury-rigged into the slot

		// determine how good a match the component is
		if (_slot.SlotTypeID == _comp.TypeID)
			_efficiency = 1;
		else {
			int _effIndex = _comp.StaticInfo.EfficiencyInOtherSlots.FindIndex(x => x.Type == _slot.SlotType);
			if (_effIndex > -1)
				_efficiency = _comp.StaticInfo.EfficiencyInOtherSlots[_effIndex].Efficiency;
		}

		if (_efficiency == 0)
			return false; // stop if the held component is as good as a non-match

		return true;
	}

    public void OnComponentsModified() {
        CalculateEfficiency();
        GUIManager.Instance.UpdateButtonGraphics(Inspectable);
    }
    void CalculateEfficiency() {
        float _efficiency = 0;
        for (int i = 0; i < ComponentSlots.Count; i++)
            _efficiency += ComponentSlots[i].CurrentEfficiency;
        _efficiency /= ComponentSlots.Count;
        CurrentEfficiency = _efficiency;
    }
}