using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class Mouse : MonoBehaviour {

    public static Mouse Instance;

	public enum MouseStateEnum { None, Click, Hold, Release }
	public static MouseStateEnum StateLeft;
	public static MouseStateEnum StateRight;
	public static bool IsOverGUI { get; private set; }

    [SerializeField] private WallBuilder WallBuilding;
	[SerializeField] public ColoringTool Coloring;
	[SerializeField] private ObjectPlacer ObjectPlacing;

	[SerializeField] private SetToolMode[] MainToolButtons;
   	[SerializeField] private Toggle[] AllButtons;

    [System.Serializable]
    public enum BuildModeEnum { None, Build, Coloring, PlaceObject }
    public BuildModeEnum BuildMode = BuildModeEnum.None;

	public CanInspect SelectedObject;

	private CanInspect inspectableInRange;

	//private Vector2 mouseScreenPos;
    //private Vector2 mouseWorldPos;


    void Awake() {
        if (Instance) return;
        Instance = this;

		MainToolButtons [0].MyToggle.group.SetAllTogglesOff ();
		MainToolButtons [0].MyToggle.isOn = true;

		Coloring.Setup(transform);
		ObjectPlacing.Setup (transform);
     }

    void Update () {
		// for testing purposes only
        if (Input.GetKeyUp(KeyCode.O)) {
            Actor[] _actor = FindObjectsOfType<Actor>();
            for (int i = 0; i < _actor.Length; i++) {
                _actor[i].enabled = !_actor[i].enabled;
            }
        }

        if (Input.GetKeyUp(KeyCode.P)) {
            Debug.Break();
        }

        if (Input.GetKeyUp(KeyCode.Tab)) {
            currentSelectedModeIndex++;
            if (currentSelectedModeIndex > MainToolButtons.Length - 1)
                currentSelectedModeIndex = 0;

            MainToolButtons[currentSelectedModeIndex].MyToggle.isOn = true;
            OnBuildModeChange(MainToolButtons[currentSelectedModeIndex].ToolIndex, MainToolButtons[currentSelectedModeIndex].ToggleMode);
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
				if (inspectableInRange == null && !IsOverGUI)
                    TryDeselectSelectedObject();
				
				// else if clicked something, with something selected, switch selected object
				else if (inspectableInRange != null) {
					_clickable.OnLeftClickRelease ();

					if(!IsOverGUI)
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

					if (!IsOverGUI) { // picking up stuff on GUI is currently handled by GUI-buttons, so not this
						if(!(SelectedObject != null && SelectedObject.GetComponent<ComponentHolder>() && _clickable.GetComponent<Component>()))
							TryDeselectSelectedObject();
						
						CanInspect _formerlyPickedUpObject;
						ObjectPlacing.TrySwitchComponents (inspectableInRange, inspectableInRange.MyTileObject.Parent, true, /*false, */out _formerlyPickedUpObject);
					}
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
		CanInspect objectToPlace = ObjectPlacing.GetObjectToPlace();
		if (objectToPlace != null && ((ObjectPlacing.GetObjectToPlace().GetComponent<ComponentObject>() && SelectedObject.GetComponent<ComponentObject>()) || ObjectPlacing.GetObjectToPlace().GetComponent<ComponentHolder>())){
			GUIManager.Instance.OpenNewWindow(SelectedObject, CanInspect.State.Default, GUIManager.WindowType.Basic_SecondWindow);
		}
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
		if (ObjectPlacing.GetObjectToPlace() != null) {
			_pickedUpComponent = ObjectPlacing.GetObjectToPlace().GetComponent<ComponentObject> ();
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
		if (ObjectPlacing.TrySwitchComponents(_fromSlot, _slot.Owner.MyTileObject, true, /*true, */out _formerPickedUpObject)) {
			if (_formerPickedUpObject == null)
				return;
			
			_slot.HeldComponent = _formerPickedUpObject.GetComponent<ComponentObject>();
			_slot.CurrentEfficiency = _efficiency;
		}
	}

    void LateUpdate() {
		if (WallBuilding.IsActive) { 
			WallBuilding.Update ();
		}
		if (Coloring.IsActive) { 
			Coloring.Update ();
		}
		if (ObjectPlacing.IsActive) { 
			ObjectPlacing.Update ();
		}

        // disable UI-stuff
        for (int i = 0; i < AllButtons.Length; i++)
            AllButtons[i].interactable = (ObjectPlacing.PickedUpObject == null);

        if (ObjectPlacing.PickedUpObject != null)
            return;
    }

	int currentSelectedModeIndex = 0;
    public void OnBuildModeChange(int _toolIndex, BuildModeEnum _mode) {
        TryDeselectSelectedObject();
        if(_toolIndex > -1)
            currentSelectedModeIndex = _toolIndex;
        SetMode(_mode);
    }

    void SetMode(BuildModeEnum _mode) {
        if (BuildMode == _mode)
            return;

        BuildMode = _mode;
        switch (_mode) {
			case BuildModeEnum.None:
				WallBuilding.DeActivate ();
				Coloring.DeActivate ();
				ObjectPlacing.DeActivate ();
				break;
			case BuildModeEnum.Build:
				Coloring.DeActivate ();
				ObjectPlacing.DeActivate ();
				WallBuilding.Activate();
                break;
			case BuildModeEnum.Coloring:
				WallBuilding.DeActivate ();
				ObjectPlacing.DeActivate ();
				Coloring.Activate ();
				break;
			case BuildModeEnum.PlaceObject:
				WallBuilding.DeActivate ();
				Coloring.DeActivate ();
				ObjectPlacing.Activate ();
				break;
            default:
                throw new System.NotImplementedException(_mode.ToString() + " hasn't been implemented yet!");
        }
    }
    
}
