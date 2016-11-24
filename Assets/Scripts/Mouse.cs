using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class Mouse : MonoBehaviour {

    public static Mouse Instance;

    [SerializeField]
    private WallBuilder WallBuilding;

    [System.Serializable]
    public enum ModeEnum { InspectAndMove, BuildWalls }
    public ModeEnum Mode = ModeEnum.InspectAndMove;

	public InteractiveObject SelectedObject;
    public InteractiveObject PickedUpObject;

    private InteractiveObject newLeftClickedObject;
    private InteractiveObject newRightClickedObject;

    private Vector2 screenPos;
    private bool leftClicked = false;
    private bool rightClicked = false;


    void Awake() {
        if (Instance) return;
        Instance = this;

        WallBuilding.Setup(transform);
     }
	
	void Update () {


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
        newLeftClickedObject = null;
        newRightClickedObject = null;

        InteractiveObject _io;
        for (int i = 0; i < InteractiveObject.AllInteractiveObjects.Count; i++) {
            _io = InteractiveObject.AllInteractiveObjects[i];
            if (_io.CanBePickedUp && !_io.CanBePickedUpCurrently)
                continue;

            Vector2 _ioScreenPos = Camera.main.WorldToScreenPoint(_io.transform.position); // optimization: can I track the mouse's worldpos rather than each object's screenpos?
            float magnitude = (screenPos - _ioScreenPos).magnitude * (Camera.main.orthographicSize / 10);
            bool withinRange = magnitude < _io.ClickableRange;

            if (!_io.PrevWasOutlined && withinRange) {
                _io.PrevWasOutlined = true;
                _io.ShowWithOutline(true);
                return;
            }
            else if (_io.PrevWasOutlined && withinRange) { // this object was clicked and is now selected
                if (leftClicked) {
                    newLeftClickedObject = _io;
                    break;
                }
                else if(rightClicked) {
                    newRightClickedObject = _io;
                    break;
                }
            }
            else if (_io.PrevWasOutlined && !withinRange) {
                _io.PrevWasOutlined = false;
                _io.ShowWithOutline(false);
            }
        }

        if (!leftClicked && !rightClicked) // Stop here if no input
            return;
        
        // left click
        if (leftClicked) {
            if (newLeftClickedObject == null) { // didn't click any object
                if (SelectedObject != null) {
                    GUIManager.Instance.CloseInfoWindow(SelectedObject);
                    SelectedObject = null;
                    return;
                }
            }

            if (newLeftClickedObject != null) {
                if(SelectedObject != null)
                Debug.Log(SelectedObject.name);
                // if already had something selected (except if it's the PickedUpObject), de-select it
                if (SelectedObject != null && SelectedObject != PickedUpObject) { // PickedUpObject can be SelectedObject if selecting, then picking it up (not sure if bad?)
                    GUIManager.Instance.CloseInfoWindow(SelectedObject);
                    bool _clickedSelectedObj = newLeftClickedObject == SelectedObject;
                    SelectedObject = null;

                    // if the new object is the old, don't continue
                    if (_clickedSelectedObj) {
                        Debug.Log("aha, aha");
                        return;
                    }
                }
                newLeftClickedObject.OnLeftClicked();
                SelectedObject = newLeftClickedObject;

                // if nothing's picked up, open the standard info window
                if (PickedUpObject == null) {
                    GUIManager.Instance.OpenNewWindow(SelectedObject, InteractiveObject.State.Default, SelectedObject.Type);
                }
                // else if something is picked up and a component was clicked, open the secondary info window
                else if(newLeftClickedObject.Type == GUIManager.WindowType.Component){
                    GUIManager.Instance.OpenNewWindow(SelectedObject, InteractiveObject.State.Default, GUIManager.WindowType.Component_SubWindow);
                }
                else { // else just open whatever
                    GUIManager.Instance.OpenNewWindow(SelectedObject, InteractiveObject.State.Default, SelectedObject.Type);
                }

                return;
            }

            return;
        }
        // right click
        else if (rightClicked) {

            // if clicked object, switch places
            if (newRightClickedObject != null) {
                InteractiveObject _receivedIO;
                if(TrySwitchComponents(newRightClickedObject, false, true, out _receivedIO))
                    newRightClickedObject.OnRightClicked();
                else
                    return;
            }
            // else if clicked ground, try putting object there
            else if(PickedUpObject != null) {
                Tile _tile = Grid.Instance.GetTileFromWorldPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
                if (_tile._Type_ != Tile.TileType.Empty)
                    return;
                if (_tile.IsOccupied)
                    return;

                PutDownPickedUp(_tile);

                // de-select SelectedObject, as it should only be a comp using the secondary comp-window, and this'll do stuff with PickedUpObject anyway
                if (SelectedObject != null) {
                    GUIManager.Instance.CloseInfoWindow(SelectedObject);
                    SelectedObject = null;
                }
            }

            return;
        }
    }

    void LateUpdate() {
        screenPos = Input.mousePosition;
    }

    bool toggled = false; // this should prevent the UI-toggle from going out-of-sync with CurrentMode if it's set through code
    public void ToggleMode_BuildWalls() {
        toggled = !toggled;

        if (toggled && Mode != ModeEnum.BuildWalls)
            SetMode(ModeEnum.BuildWalls);
        else if (!toggled && Mode != ModeEnum.InspectAndMove)
            SetMode(ModeEnum.InspectAndMove);
    }
    void SetMode(ModeEnum _mode) {
        if (Mode == _mode)
            return;

        Mode = _mode;
        switch (_mode) {
            case ModeEnum.InspectAndMove:
                WallBuilding.DeActivate();
                break;
            case ModeEnum.BuildWalls:
                WallBuilding.Activate();
                break;
            default:
                throw new System.NotImplementedException(_mode.ToString() + " hasn't been implemented yet!");
        }
    }
    
    public bool TrySwitchComponents(InteractiveObject _giveComponent, bool isVisibleOnPickUp, bool isVisibleOnPutDown, out InteractiveObject _receiveComponent) {
        _receiveComponent = null;

        if (_giveComponent != null && !_giveComponent.CanBePickedUp)
            return false;

        _receiveComponent = PickedUpObject;
        if (_receiveComponent != null) {

            _receiveComponent.transform.parent = (_giveComponent == null) ? null : _giveComponent.transform.parent;
            _receiveComponent.transform.localPosition = (_giveComponent == null) ? Vector3.zero : _giveComponent.transform.localPosition;
            GUIManager.Instance.CloseInfoWindow(_receiveComponent);
            if (SelectedObject != null && _receiveComponent == SelectedObject)
                SelectedObject = null;

            _receiveComponent.Hide(!isVisibleOnPutDown);
        }

        PickedUpObject = null;
        if (_giveComponent != null) {
            PickUp(_giveComponent);
            _giveComponent.Hide(!isVisibleOnPickUp);
        }

        return true;
    }


    public void PickUp(InteractiveObject _io) {
        if (PickedUpObject != null)
            Debug.LogError("Mouse already has a PickedUpObject!");


        //SavePreviousTransformInfo(_io);

        _io.transform.parent = null;
        _io.Hide(true);

        PickedUpObject = _io;
        GUIManager.Instance.OpenNewWindow(_io, InteractiveObject.State.PickedUp, _io.Type);
    }

    public void PutDownPickedUp(Tile _tile) {
        if (PickedUpObject == null)
            return;

        PickedUpObject.transform.position = _tile.DefaultPositionWorld;
        PickedUpObject.transform.parent = null;
        GUIManager.Instance.CloseInfoWindow(PickedUpObject);
        PickedUpObject.Hide(false);

        if (SelectedObject != null && SelectedObject == PickedUpObject)
            SelectedObject = null;
        PickedUpObject = null;
    }
}
