﻿using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Mouse : MonoBehaviour {

    public static Mouse Instance;

	public enum MouseStateEnum { None, Click, Hold, Release }
	public static MouseStateEnum StateLeft;
	public static MouseStateEnum StateRight;
	public static bool IsOverGUI { get; private set; }

    [SerializeField] private WallBuilder WallBuilding;
	[SerializeField] private FloorBuilder FloorBuilding;
	[SerializeField] private ObjectPlacer ObjectPlacing;

	[SerializeField] private Toggle[] ModeButtons;

    [System.Serializable]
    public enum BuildModeEnum { None, BuildWalls, BuildFloor, PlaceObject }
    public BuildModeEnum BuildMode = BuildModeEnum.None;

	public CanInspect SelectedObject;

	private CanInspect inspectableInRange;

	private Vector2 mouseScreenPos;
    private Vector2 mouseWorldPos;


    void Awake() {
        if (Instance) return;
        Instance = this;

		ModeButtons [0].group.SetAllTogglesOff ();
		ModeButtons [0].isOn = true;

		BuilderBase.Setup(transform);
		ObjectPlacing.Setup ();
     }

	void OnEnable(){
		ModeButtons [0].onValueChanged.AddListener (OnModeButton0ValueChanged);
		ModeButtons [1].onValueChanged.AddListener (OnModeButton1ValueChanged);
		ModeButtons [2].onValueChanged.AddListener (OnModeButton2ValueChanged);
		ModeButtons [3].onValueChanged.AddListener (OnModeButton3ValueChanged);
	}
	void OnDisable(){
		ModeButtons [0].onValueChanged.RemoveListener (OnModeButton0ValueChanged);
		ModeButtons [1].onValueChanged.RemoveListener (OnModeButton1ValueChanged);
		ModeButtons [2].onValueChanged.RemoveListener (OnModeButton2ValueChanged);
		ModeButtons [3].onValueChanged.RemoveListener (OnModeButton3ValueChanged);
	}
	
	void Update () {

		// for testing purposes only
        if (Input.GetKeyDown(KeyCode.O)) {
            Actor[] _actor = FindObjectsOfType<Actor>();
            for (int i = 0; i < _actor.Length; i++) {
                _actor[i].enabled = !_actor[i].enabled;
            }
        }

		SetMouseState ();

		inspectableInRange = null;
		CanClick _clickable = null;
		for (int i = 0; i < CanClick.AllClickables.Count; i++) {
			_clickable = CanClick.AllClickables [i];
			if (!_clickable.Enabled)
				continue;
			if (!_clickable.IsOnGUI && BuildMode != BuildModeEnum.None && BuildMode != BuildModeEnum.PlaceObject)
				continue;
			if (!_clickable.IsOnGUI && IsOverGUI)
				continue;
            if (!_clickable.IsVisible())
                continue;

			if (_clickable.IsMouseWithinRange()) {
				if (_clickable._OnWithinRange != null)
					_clickable._OnWithinRange ();
				
				inspectableInRange = _clickable.GetComponent<CanInspect> ();
				break;
			}
		}

		// stop here if no input
		if (StateLeft == MouseStateEnum.None && StateRight == MouseStateEnum.None)
            return;

        // left click
		switch (StateLeft) {
			case MouseStateEnum.Click:
				return;
			case MouseStateEnum.Hold:
				return;
			case MouseStateEnum.Release:
				// if clicked nothing, with something selected, deselect
				if (inspectableInRange == null)
                    TryDeselectSelectedObject();
				
				// else if clicked something, with something selected, switch selected object
				else if (inspectableInRange != null) {
					_clickable.OnLeftClickRelease ();
                    SelectObject(inspectableInRange);
					return;
				}
				
				return;
		}
		switch (StateRight) {
			case MouseStateEnum.Click:
				return;
			case MouseStateEnum.Hold:
				return;
			case MouseStateEnum.Release:
				if (inspectableInRange != null) {
					_clickable.OnRightClickRelease ();

                    TryDeselectSelectedObject();

					CanInspect _formerlyPickedUpObject;
					ObjectPlacing.TrySwitchComponents (inspectableInRange, true, /*false, */out _formerlyPickedUpObject);
				}
				return;
		}
    }

    public bool TryDeselectSelectedObject(CanInspect _exception = null) {
        if (SelectedObject == null)
            return true;
        if (SelectedObject == _exception)
            return false;
        GUIManager.Instance.CloseInfoWindow(SelectedObject);
        SelectedObject = null;
        return true;
    }
    public void SelectObject(CanInspect _object) {
        if (!TryDeselectSelectedObject())
            return;

        SelectedObject = _object.GetComponent<CanInspect>();

        // if something is picked up and a component was clicked, open the secondary info window
        if (ObjectPlacing._ObjectToPlace_ != null && ObjectPlacing._ObjectToPlace_.GetComponent<ComponentHolder>())
            GUIManager.Instance.OpenNewWindow(SelectedObject, CanInspect.State.Default, GUIManager.WindowType.Basic_SecondWindow);
        else // else just open default window
            GUIManager.Instance.OpenNewWindow(SelectedObject, CanInspect.State.Default, SelectedObject.Window_Inspector);
    }

	bool _leftClickedOld;
	bool _leftClicked;
	bool _rightClickedOld;
	bool _rightClicked;
	private void SetMouseState(){
		_leftClickedOld = _leftClicked;
		_leftClicked = Input.GetMouseButton(0);
		_rightClickedOld = _rightClicked;
		_rightClicked = Input.GetMouseButton(1);

		if (_leftClicked && !_leftClickedOld)
			StateLeft = MouseStateEnum.Click;
		else if (_leftClicked && _leftClickedOld)
			StateLeft = MouseStateEnum.Hold;
		else if (!_leftClicked && _leftClickedOld)
			StateLeft = MouseStateEnum.Release;
		else
			StateLeft = MouseStateEnum.None;

		if (_rightClicked && !_rightClickedOld)
			StateRight = MouseStateEnum.Click;
		else if (_rightClicked && _rightClickedOld)
			StateRight = MouseStateEnum.Hold;
		else if (!_rightClicked && _rightClickedOld)
			StateRight = MouseStateEnum.Release;
		else
			StateRight = MouseStateEnum.None;

		IsOverGUI = EventSystem.current.IsPointerOverGameObject ();
	}

	public void OnClickComponentSlot(ComponentHolder.ComponentSlot _slot){
		float _efficiency = 0;
		ComponentObject _pickedUpComponent = null;
		if (ObjectPlacing._ObjectToPlace_ != null) {
			_pickedUpComponent = ObjectPlacing._ObjectToPlace_.GetComponent<ComponentObject> ();
			if (_pickedUpComponent == null)
				return;
			if (!ComponentHolder.DoesComponentFitInSlot (_slot, _pickedUpComponent, out _efficiency))
				return;
		}

		// detach component from slot
		CanInspect _fromSlot = null;
		if (_slot.HeldComponent != null) {
			_fromSlot = _slot.HeldComponent.GetComponent<CanInspect>();
			_slot.HeldComponent = null;
			_slot.CurrentEfficiency = 0;
		}

		// pick up component and put the former PickedUpObject in the slot
		CanInspect _formerPickedUpObject;
		if (ObjectPlacing.TrySwitchComponents(_fromSlot, true, /*true, */out _formerPickedUpObject)) {
			if (_formerPickedUpObject == null)
				return;
			
			_slot.HeldComponent = _formerPickedUpObject.GetComponent<ComponentObject>();
			_slot.CurrentEfficiency = _efficiency;
		}
	}

	

    void LateUpdate() {
        mouseScreenPos = Input.mousePosition;
        mouseWorldPos = Camera.main.ScreenToWorldPoint(mouseScreenPos);

		if (WallBuilding.IsActive)
			WallBuilding.Update ();
		if (FloorBuilding.IsActive)
			FloorBuilding.Update ();
		if (ObjectPlacing.IsActive)
			ObjectPlacing.Update ();

        // disable UI-stuff
        for (int i = 0; i < ModeButtons.Length; i++)
            ModeButtons[i].interactable = (ObjectPlacing.PickedUpObject == null);
        for (int i = 0; i < ObjectPlacing.ObjectButtons.Length; i++)
            ObjectPlacing.ObjectButtons[i].interactable = (ObjectPlacing.PickedUpObject == null);

        if (ObjectPlacing.PickedUpObject != null)
            return;
		
		if (Input.GetKeyUp(KeyCode.Tab)) {
			currentSelectedModeIndex++;
			if (currentSelectedModeIndex > ModeButtons.Length - 1)
				currentSelectedModeIndex = 0;

            TryDeselectSelectedObject();

			ModeButtons [currentSelectedModeIndex].isOn = true;
			OnModeButtonsNewActive (currentSelectedModeIndex);
		}
    }

	void OnModeButton0ValueChanged(bool b){
		if(b) OnModeButtonsNewActive (0);
	}
	void OnModeButton1ValueChanged(bool b){
		if(b) OnModeButtonsNewActive (1);
	}
	void OnModeButton2ValueChanged(bool b){
		if(b) OnModeButtonsNewActive (2);
	}
	void OnModeButton3ValueChanged(bool b){
		if(b) OnModeButtonsNewActive (3);
	}
	int currentSelectedModeIndex = 0;
	void OnModeButtonsNewActive(int selectedModeIndex){
		currentSelectedModeIndex = selectedModeIndex;
		switch (currentSelectedModeIndex) {
			case 0:
				SetMode (BuildModeEnum.None);
				break;
			case 1:
				SetMode (BuildModeEnum.BuildWalls);
				break;
			case 2:
				SetMode (BuildModeEnum.BuildFloor);
				break;
			case 3:
				SetMode (BuildModeEnum.PlaceObject);
				break;
			default:
				throw new System.IndexOutOfRangeException ("selectedModeIndex was out of range!");
		}
	}

    void SetMode(BuildModeEnum _mode) {
        if (BuildMode == _mode)
            return;

        BuildMode = _mode;
        switch (_mode) {
			case BuildModeEnum.None:
				WallBuilding.DeActivate ();
				FloorBuilding.DeActivate ();
				ObjectPlacing.DeActivate ();
				break;
			case BuildModeEnum.BuildWalls:
				FloorBuilding.DeActivate ();
				ObjectPlacing.DeActivate ();
				WallBuilding.Activate();
                break;
			case BuildModeEnum.BuildFloor:
				WallBuilding.DeActivate ();
				ObjectPlacing.DeActivate ();
				FloorBuilding.Activate ();
				break;
			case BuildModeEnum.PlaceObject:
				WallBuilding.DeActivate ();
				FloorBuilding.DeActivate ();
				ObjectPlacing.Activate ();
				break;
            default:
                throw new System.NotImplementedException(_mode.ToString() + " hasn't been implemented yet!");
        }
    }
    
}
