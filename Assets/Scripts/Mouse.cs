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
    private bool inputWasReceived = false;
    private bool leftClicked = false;
    private bool rightClicked = false;


    void Awake() {
        if (Instance) return;
        Instance = this;

        WallBuilding.Setup(transform);
     }
	
	void Update () {
        // if is over UI
        if (UnityEngine.EventSystems.EventSystem.current.IsPointerOverGameObject(-1))
            return;
        if (Mode != ModeEnum.InspectAndMove)
            return;

        leftClicked = Input.GetMouseButtonUp(0);
        rightClicked = Input.GetMouseButtonUp(1);
        newLeftClickedObject = null;
        newRightClickedObject = null;

        //if (leftClicked)
        //    SelectedObject = null;

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
                //if (PickedUpObject != null)
                //    if (!TryReturnPickedUpToPreviousParent())
                //        return;

                if (SelectedObject != null) {
                    GUIManager.Instance.CloseInfoWindow(SelectedObject);
                    SelectedObject = null;
                    return;
                }
            }

            if (newLeftClickedObject != null) {
                //if (PickedUpObject != null && newLeftClickedObject.Type == GUIManager.WindowType.Component) // don't wanna drop object if we're opening a componentholder
                //    if (!TryReturnPickedUpToPreviousParent())
                //        return;
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
            //if(newRightClickedObject == null && PickedUpObject == null) {
            //    GUIManager.Instance.CloseAllWindows();
            //    return;
            //}


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


            //if (newRightClickedObject != null) {
            //    if (!newRightClickedObject.CanBePickedUp)
            //        return;

            //    if (PickedUpObject != null) {
            //        if (newRightClickedObject != PickedUpObject)
            //            TrySwitchObjects<ComponentObject>(_giveComponent: newRightClickedObject, isVisibleOnPickUp: true, isVisibleOnPutDown: true);
            //        return;
            //    }
            //    else {
            //        if (newRightClickedObject.CanBePickedUp)
            //            PickUp(newRightClickedObject);

            //        newRightClickedObject.OnRightClicked();
            //        GUIManager.Instance.OpenNewWindow(newRightClickedObject, InteractiveObject.State.PickedUp, newRightClickedObject.Type);
            //        PickedUpObject = newRightClickedObject;
            //        return;
            //    }
            //}
            //else {
            //    if (PickedUpObject != null) {
            //        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            //        RaycastHit rayHit;
            //        int layerMask = 1 << 9; // walkable layer
            //        if (Physics.Raycast(ray, out rayHit, 1000, layerMask)) {
            //            PutDownPickedUp(rayHit.point);
            //            return;
            //        }
            //    }
            //}

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
            //SavePreviousTransformInfo(oldPickedUp);

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
    //public bool TryReturnPickedUpToPreviousParent() {
    //    if (PickedUpObject == null)
    //        return true;

    //    ComponentObject _component = PickedUpObject.GetComponent<ComponentObject>();
    //    ComponentHolder _parentCH = PickedUpObject.PreviousParent.GetComponent<ComponentHolder>();

    //    //Button _parentButton = PickedUpObject.PreviousParent.GetComponent<Button>();
    //    Debug.Log(PickedUpObject.PreviousComponentSlotIndex);
    //    if (_parentCH.ComponentSlots[PickedUpObject.PreviousComponentSlotIndex].HeldComponent != null) {
    //        //throw new System.Exception("Tried re-adding a component to its slot, but it was filled somehow!");
    //        Debug.LogWarning("Can't cancel because the component's slot has already been filled!");
    //        return false;
    //    }

    //    PickedUpObject.transform.parent = PickedUpObject.PreviousParent;
    //    PickedUpObject.transform.localPosition = PickedUpObject.PreviousPosition;

    //    if (_parentCH == null) {
    //        GUIManager.Instance.CloseCurrentInfoWindow();
    //        PickedUpObject = null;
    //        return true;
    //    }

    //    _parentCH.ComponentSlots[PickedUpObject.PreviousComponentSlotIndex].HeldComponent = _component;
    //    _parentCH.OnComponentsModified();
    //    GUIManager.Instance.OpenNewWindow(PickedUpObject, InteractiveObject.State.Contained, PickedUpObject.Type);
    //    PickedUpObject = null;

    //    return true;
    //}
    //void SavePreviousTransformInfo(InteractiveObject _io) {
    //    _io.PreviousParent = _io.transform.parent;
    //    _io.PreviousPosition = _io.transform.localPosition;

    //    ComponentHolder _holder = _io.GetComponentInParent<ComponentHolder>();
    //    if (_holder != null)
    //        _io.PreviousComponentSlotIndex = _holder.ComponentSlots.FindIndex(x => x.HeldComponent == _io.GetComponent<ComponentObject>());
    //    else
    //        _io.PreviousComponentSlotIndex = -1;
    //}
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
