using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Mouse : Singleton<Mouse>{

	private const int SUPPORTED_MOUSE_BUTTON_COUNT = 2;
	private const float MIN_HOLD_TIME = 0.3f;

	public enum StateEnum { Idle, Click, Hold, Release }

	private StateEnum StateLMB;
	private StateEnum StateRMB;
	public StateEnum GetStateLMB() { return StateLMB; }
	public StateEnum GetStateRMB() { return StateRMB; }
	public bool DoesEitherButtonEqual(StateEnum state) { return StateLMB == state || StateRMB == state; }

	private Vector2 worldPos;
	private Int2 gridPos;
	private Int2 gridPosOld;
	public Int2 GetGridPos() { return gridPos; }
	public bool HasMoved() { return gridPos != gridPosOld; }

	private float[] timeAtClicks = new float[SUPPORTED_MOUSE_BUTTON_COUNT];
	private Int2[] posGridAtClicks = new Int2[SUPPORTED_MOUSE_BUTTON_COUNT];

	public static bool IsOverGUI { get; private set; }

	[SerializeField] private SetToolMode[] MainToolButtons;
   	[SerializeField] private Toggle[] AllButtons;

    public enum BuildModeEnum { None, Build, Coloring, PlaceObject }
    public BuildModeEnum BuildMode = BuildModeEnum.None;

	public CanInspect SelectedObject;
	private CanInspect inspectableInRange;

	public override bool IsUsingAwakeDefault() { return true; }
	public override void AwakeDefault(){
		base.AwakeDefault();
		SetToolMode.SetMode(BuildTool.ToolMode.None);
	}

	public override bool IsUsingUpdateEarly(){ return true; }
	public override void UpdateEarly(){
		base.UpdateEarly();

		worldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);

		gridPosOld = gridPos;
		gridPos = GameGrid.GetInstance().GetNodeGridPosFromWorldPos(worldPos);

		StateLMB = GetMouseButtonState(0);
		StateRMB = GetMouseButtonState(1);

		IsOverGUI = EventSystem.current.IsPointerOverGameObject();
	}

	public override bool IsUsingUpdateDefault() { return true; }
	public override void UpdateDefault() {
		base.UpdateDefault();

		if (Input.GetKeyUp(KeyCode.P)) {
            Debug.Break();
        }

		// disable UI-stuff
		// for (int i = 0; i < AllButtons.Length; i++) { 
		// 	AllButtons[i].interactable = (ObjectPlacing.PickedUpObject == null);
		// }

		// for testing purposes only
        if (Input.GetKeyUp(KeyCode.O)) {
            Actor[] _actor = FindObjectsOfType<Actor>();
            for (int i = 0; i < _actor.Length; i++) {
                _actor[i].enabled = !_actor[i].enabled;
            }
        }

		inspectableInRange = null;
		CanClick _clickable = null;
		for (int i = 0; i < CanClick.AllClickables.Count; i++) {
			_clickable = CanClick.AllClickables[i];
			
			if (!_clickable.Enabled) { 
				continue;
			}
			if (!_clickable.IsOnGUI && BuildMode != BuildModeEnum.None && BuildMode != BuildModeEnum.PlaceObject) { 
				continue;
			}
			if (!_clickable.IsOnGUI && IsOverGUI) {
				continue;
			 }
            if (!_clickable.IsVisible()) { 
                continue;
			}

			if (_clickable.IsMouseWithinRange()) {
				if (_clickable._OnWithinRange != null)
					_clickable._OnWithinRange ();
				
				inspectableInRange = _clickable.GetComponent<CanInspect> ();
				break;
			}
		}

		// stop here if no input
		if (StateLMB == StateEnum.Idle && StateRMB == StateEnum.Idle) { 
			return;
		}

        // left click
		switch (StateLMB) {
			case StateEnum.Click:
				return;
			case StateEnum.Hold:
				return;
			case StateEnum.Release:
				// if clicked nothing, with something selected, deselect
				if (inspectableInRange == null && !IsOverGUI) { 
					TryDeselectSelectedObject();
				}
				
				// else if clicked something, with something selected, switch selected object
				else if (inspectableInRange != null) {
					_clickable.OnLeftClickRelease ();

					if (!IsOverGUI) { 
						SelectObject(inspectableInRange);
					}
					return;
				}
				
				return;
		}
		switch (StateRMB) {
			case StateEnum.Click:
				return;
			case StateEnum.Hold:
				return;
			case StateEnum.Release:
				if (inspectableInRange != null) {
					_clickable.OnRightClickRelease ();

					if (!IsOverGUI) { // picking up stuff on GUI is currently handled by GUI-buttons, so not this
						if (!(SelectedObject != null && SelectedObject.GetComponent<ComponentHolder>() && _clickable.GetComponent<Component>())) { 
							TryDeselectSelectedObject();
						}
						
						// CanInspect _formerlyPickedUpObject;
						// ObjectPlacing.TrySwitchComponents (inspectableInRange, inspectableInRange.MyTileObject.Parent, true, /*false, */out _formerlyPickedUpObject);
					}
				}
				return;
		}
    }

	StateEnum GetMouseButtonState(int index) { 
		if (Input.GetMouseButtonDown(index)){
			timeAtClicks[index] = Time.time;
			posGridAtClicks[index] = gridPos;
			return StateEnum.Click;
		}

		bool hasPassedMinHoldTime = Time.time - timeAtClicks[index] > MIN_HOLD_TIME;
		bool hasChangedPosGrid = gridPos != posGridAtClicks[index];
		if (Input.GetMouseButton(index) && (hasPassedMinHoldTime || hasChangedPosGrid)){
			return StateEnum.Hold;
		}

		if (Input.GetMouseButtonUp(index)){
			return StateEnum.Release;
		}
		
		return StateEnum.Idle;
	}

    public bool TryDeselectSelectedObject(CanInspect _exception = null) {
		if (SelectedObject == null) { 
			return true;
		}
		if (SelectedObject == _exception) { 
			return false;
		}
    
	    GUIManager.Instance.CloseInfoWindow(SelectedObject);
        SelectedObject = null;
        return true;
    }
    public void SelectObject(CanInspect _object) {
		if (!TryDeselectSelectedObject()) { 
			return;
		}

        SelectedObject = _object.GetComponent<CanInspect>();

		// if something is picked up and a component was clicked, open the secondary info window
		// CanInspect objectToPlace = ObjectPlacing.GetObjectToPlace();
		// if (objectToPlace != null && ((ObjectPlacing.GetObjectToPlace().GetComponent<ComponentObject>() && SelectedObject.GetComponent<ComponentObject>()) || ObjectPlacing.GetObjectToPlace().GetComponent<ComponentHolder>())){
		// 	GUIManager.Instance.OpenNewWindow(SelectedObject, CanInspect.State.Default, GUIManager.WindowType.Basic_SecondWindow);
		// }
		// else{// else just open default window
		// 	GUIManager.Instance.OpenNewWindow(SelectedObject, CanInspect.State.Default, SelectedObject.Window_Inspector);
		// }
	}

	public void OnClickComponentSlot(ComponentHolder.ComponentSlot _slot){
		// float _efficiency = 0;
		// ComponentObject _pickedUpComponent = null;
		// if (ObjectPlacing.GetObjectToPlace() != null) {
		// 	_pickedUpComponent = ObjectPlacing.GetObjectToPlace().GetComponent<ComponentObject> ();
		// 	if (_pickedUpComponent == null) { 
		// 		return;
		// 	}
		// 	if (!ComponentHolder.DoesComponentFitInSlot(_slot, _pickedUpComponent, out _efficiency)) { 
		// 		return;
		// 	}
		// }

		// detach component from slot
		CanInspect _fromSlot = null;
		if (_slot.HeldComponent != null) {
			_fromSlot = _slot.HeldComponent.GetComponent<CanInspect>();
			_slot.HeldComponent = null;
			_slot.CurrentEfficiency = 0;
		}

		// pick up component and put the former PickedUpObject in the slot
		// CanInspect _formerPickedUpObject;
		// if (ObjectPlacing.TrySwitchComponents(_fromSlot, _slot.Owner.MyTileObject, true, /*true, */out _formerPickedUpObject)) {
		// 	if (_formerPickedUpObject == null)
		// 		return;
			
		// 	_slot.HeldComponent = _formerPickedUpObject.GetComponent<ComponentObject>();
		// 	_slot.CurrentEfficiency = _efficiency;
		// }
	}
}
