using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Linq;

public class Mouse : MonoBehaviour {

    public static Mouse Instance;

    [SerializeField] private WallBuilder WallBuilding;
	[SerializeField] private FloorBuilder FloorBuilding;

	[SerializeField] private Toggle[] ModeButtons;

    [System.Serializable]
    public enum ModeEnum { InspectAndMove, BuildWalls, BuildFloor }
    public ModeEnum Mode = ModeEnum.InspectAndMove;

	public CanInspect SelectedObject;
    public CanInspect PickedUpObject;

	private bool leftClicked = false;
	private CanInspect newLeftClickedInspectable;

	private bool rightClicked = false;
	private CanInspect newRightClickedInspectable;

	private Vector2 screenPos;


    void Awake() {
        if (Instance) return;
        Instance = this;

		ModeButtons [0].group.SetAllTogglesOff ();
		ModeButtons [0].isOn = true;

        WallBuilding.Setup(transform);
     }

	void OnEnable(){
		ModeButtons [0].onValueChanged.AddListener (OnModeButton0ValueChanged);
		ModeButtons [1].onValueChanged.AddListener (OnModeButton1ValueChanged);
		ModeButtons [2].onValueChanged.AddListener (OnModeButton2ValueChanged);
	}
	void OnDisable(){
		ModeButtons [0].onValueChanged.RemoveListener (OnModeButton0ValueChanged);
		ModeButtons [1].onValueChanged.RemoveListener (OnModeButton1ValueChanged);
		ModeButtons [2].onValueChanged.RemoveListener (OnModeButton2ValueChanged);
	}
	
	void Update () {

		// for testing purposes only
        if (Input.GetKeyDown(KeyCode.O)) {
            Actor[] _actor = FindObjectsOfType<Actor>();
            for (int i = 0; i < _actor.Length; i++) {
                _actor[i].enabled = !_actor[i].enabled;
            }
        }

        // if is over UI
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(-1))
            return;
        if (Mode != ModeEnum.InspectAndMove)
            return;

		leftClicked = Input.GetMouseButtonUp(0);
		rightClicked = Input.GetMouseButtonUp(1);
		newLeftClickedInspectable = null;
		newRightClickedInspectable = null;
		CanClick _clickable = null;
		for (int i = 0; i < CanClick.AllClickables.Count; i++) {
			_clickable = CanClick.AllClickables [i];
			if (!_clickable.Enabled)
				continue;

			Vector2 _ioScreenPos = Camera.main.WorldToScreenPoint(_clickable.transform.position); // optimization: can I track the mouse's worldpos rather than each object's screenpos?
			float magnitude = (screenPos - _ioScreenPos).magnitude * (Camera.main.orthographicSize / 10);
			bool withinRange = magnitude < _clickable.ClickableRange;

			if (withinRange && _clickable.OnWithinRange != null) {
				_clickable.OnWithinRange ();

				if (leftClicked) {
					_clickable.OnLeftClickRelease ();
					newLeftClickedInspectable = _clickable.GetComponent<CanInspect>();
					break;
				} 
				else if (rightClicked) {
					_clickable.OnRightClickRelease ();
					newRightClickedInspectable = _clickable.GetComponent<CanInspect>();
					break;
				}
			}
		}

		// stop here if no input
		if (!leftClicked && !rightClicked)
            return;

        // left click
        if (leftClicked) {

			// if clicked nothing, with something selected, deselect
            if (newLeftClickedInspectable == null) {
                if (SelectedObject != null) {
                    GUIManager.Instance.CloseInfoWindow(SelectedObject);
                    SelectedObject = null;
                    return;
                }
            }

			// else if clicked something, with something selected, switch selected object
            else if (newLeftClickedInspectable != null) {
                if (SelectedObject != null) {
					if (newLeftClickedInspectable == SelectedObject)
						return;

                    GUIManager.Instance.CloseInfoWindow(SelectedObject);
                    SelectedObject = null;
                }

				SelectedObject = newLeftClickedInspectable.GetComponent<CanInspect>();

                // if something is picked up and a component was clicked, open the secondary info window
				if(PickedUpObject != null && SelectedObject.Type == GUIManager.WindowType.Component)
                    GUIManager.Instance.OpenNewWindow(SelectedObject, CanInspect.State.Default, GUIManager.WindowType.Component_SubWindow);
                else // else just open default window
                    GUIManager.Instance.OpenNewWindow(SelectedObject, CanInspect.State.Default, SelectedObject.Type);

                return;
            }

            return;
        }
        // right click
        else if (rightClicked) {

			// if right-clicked nothing, with something picked up, try putting the thing down and de-selecting whatever else is selected
			if (newRightClickedInspectable == null) {
				if (PickedUpObject != null) {
					Tile _tile = Grid.Instance.GetTileFromWorldPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
					if (_tile._FloorType_ == Tile.Type.Empty)
						return;
					if (_tile.IsOccupied)
						return;

					PutDownPickedUp(_tile);

					// de-select SelectedObject
					if (SelectedObject != null) {
						GUIManager.Instance.CloseInfoWindow(SelectedObject);
						SelectedObject = null;
					}
				}
			}

            // else if right-clicked something, switch places with what's held, if anything
            else if (newRightClickedInspectable != null) {

				if (SelectedObject != null) {
					GUIManager.Instance.CloseInfoWindow (SelectedObject);
					SelectedObject = null;
				}

				CanInspect _formerlyPickedUpObject;
				TrySwitchComponents (newRightClickedInspectable, true, false, out _formerlyPickedUpObject);
            }

            return;
        }
    }

	public void OnClickComponentSlot(ComponentHolder.ComponentSlot _slot){
		float _efficiency = 0;
		ComponentObject _pickedUpComponent = null;
		if (PickedUpObject != null) {
			_pickedUpComponent = PickedUpObject.GetComponent<ComponentObject> ();
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
		if (TrySwitchComponents(_fromSlot, true, true, out _formerPickedUpObject)) {
			if (_formerPickedUpObject == null)
				return;
			
			_slot.HeldComponent = _formerPickedUpObject.GetComponent<ComponentObject>();
			_slot.CurrentEfficiency = _efficiency;
		}
	}

	bool TrySwitchComponents(CanInspect _pickUpThis, bool _hideOnPickup, bool _hideOnPutDown, out CanInspect _putThisDown) {
		_putThisDown = null;

		// can this thing even be picked up?
		if (_pickUpThis != null && !_pickUpThis.CanBePickedUp)
			return false;

		// where should PickedUpObject go?
		if (PickedUpObject != null) {
			if (_pickUpThis == null)
				PickedUpObject.PutSomewhereElse (null, Vector3.zero, _hideOnPutDown); // used by ComponentSlots
			else
				PickedUpObject.PutSomewhereElse (_pickUpThis.transform.parent, _pickUpThis.transform.localPosition, _hideOnPutDown);

			_putThisDown = PickedUpObject;
			PickedUpObject = null;
		}

		if (_pickUpThis != null)
			PickUp(_pickUpThis);
		
		return true;
	}
	
	void PickUp(CanInspect _inspectable) {
		if (PickedUpObject != null)
			Debug.LogError("Mouse already has a PickedUpObject!");

		PickedUpObject = _inspectable;
		PickedUpObject.PickUp ();
	}
	
	void PutDownPickedUp(Tile _tile) {
		if (PickedUpObject == null)
			return;

		PickedUpObject.PutDown (_tile);
		
		if (SelectedObject != null && SelectedObject == PickedUpObject)
			SelectedObject = null;

		PickedUpObject = null;
	}

    void LateUpdate() {
        screenPos = Input.mousePosition;

		if (PickedUpObject != null)
			return;
		
		if (Input.GetKeyUp(KeyCode.Tab)) {
			currentSelectedModeIndex++;
			if (currentSelectedModeIndex > 2)
				currentSelectedModeIndex = 0;

			if (SelectedObject != null) {
				GUIManager.Instance.CloseInfoWindow (SelectedObject);
				SelectedObject = null;
			}

			ModeButtons [currentSelectedModeIndex].isOn = true;
			OnModeButtonsNewActive (currentSelectedModeIndex);
		}
    }

	void OnModeButton0ValueChanged(bool b){
		if(b)
			OnModeButtonsNewActive (0);
	}
	void OnModeButton1ValueChanged(bool b){
		if(b)
			OnModeButtonsNewActive (1);
	}
	void OnModeButton2ValueChanged(bool b){
		if(b)
			OnModeButtonsNewActive (2);
	}
	int currentSelectedModeIndex = 0;
	void OnModeButtonsNewActive(int selectedModeIndex){
		currentSelectedModeIndex = selectedModeIndex;
		switch (currentSelectedModeIndex) {
			case 0:
				SetMode (ModeEnum.InspectAndMove);
				break;
			case 1:
				SetMode (ModeEnum.BuildWalls);
				break;
			case 2:
				SetMode (ModeEnum.BuildFloor);
				break;
			default:
				throw new System.IndexOutOfRangeException ("selectedModeIndex was out of range!");
		}
	}

    void SetMode(ModeEnum _mode) {
        if (Mode == _mode)
            return;

        Mode = _mode;
        switch (_mode) {
			case ModeEnum.InspectAndMove:
				WallBuilding.DeActivate ();
				FloorBuilding.DeActivate ();
				break;
			case ModeEnum.BuildWalls:
				FloorBuilding.DeActivate ();
				WallBuilding.Activate();
                break;
			case ModeEnum.BuildFloor:
				WallBuilding.DeActivate ();
				FloorBuilding.Activate ();
				break;
            default:
                throw new System.NotImplementedException(_mode.ToString() + " hasn't been implemented yet!");
        }
    }
    
}
