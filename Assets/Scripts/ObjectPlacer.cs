using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ObjectPlacer {

	public Toggle[] ObjectButtons;
    public CanInspect PickedUpObject;
    private CanInspect UISelectedObject;

	public void SetObjectToPlace(CanInspect newObjectToPlace) {
		if (PickedUpObject != null) { 
			PickedUpObject = newObjectToPlace;
		}
		else { 
			UISelectedObject = newObjectToPlace;
		}
	}

	public CanInspect GetObjectToPlace() {
		return (PickedUpObject != null ? PickedUpObject : UISelectedObject);
	}



	private Int2 AssetBottom;
    private Int2 AssetTop;
    private bool activeTemporarily = false;
    private int oldObjectButtonIndex = -1;
    private bool selectedObjectButtonHasChanged = false;

    [SerializeField] private byte ColorIndex_New = ColoringTool.COLOR_WHITE;
    [SerializeField] private byte ColorIndex_AlreadyExisting = ColoringTool.COLOR_GREY;
    [SerializeField] private byte ColorIndex_Remove = ColoringTool.COLOR_RED;
    [SerializeField] private byte ColorIndex_Blocked = ColoringTool.COLOR_ORANGE;

    [System.NonSerialized]
    public bool IsActive = false;

    private IEnumerator ghostRoutine;
    public class GhostInfo {
        public UVController MyUVController;
        public Int2 posGrid { get; private set; }

        public GhostInfo(UVController _uvc) {
            MyUVController = _uvc;
            SetPosition(Int2.Zero);
        }

        private const float DEFAULT_OFFSET_Y = 0.5f;
        public void SetPosition(Int2 _posGrid) {
			Vector3 _posWorld = GameGrid.Instance.GetWorldPosFromGridPos(_posGrid);
			_posWorld.y += DEFAULT_OFFSET_Y;

			MyUVController.transform.position = _posWorld;
            posGrid = _posGrid;
        }
        public void ChangeAssets(Int2 _bottomIndices, Int2 _topIndices) {
            // MyUVController.ChangeAsset(MeshSorter.GridLayerEnum.Bottom, _bottomIndices, false);
			// MyUVController.ChangeAsset(MeshSorter.GridLayerEnum.Top, _topIndices, false);
		}
        public void SetColor(byte _colorIndex) {
            MyUVController.ChangeColor(_colorIndex, _temporary: true);
        }
        public void SetActive(bool _b) {
            MyUVController.gameObject.SetActive(_b);
        }
    }
    private GhostInfo Ghost;

    private Int2 oldMouseGridPos;

    private Node startTile;
    private Node mouseTile;

    private bool isDeleting = false;
    private bool mouseGhostHasNewTile = false;
    private bool mouseGhostIsDirty = true;

    private List<Node> selectedTiles = new List<Node>();


    private float timeActivated;
    public void Activate() {
        if (IsActive)
            return;
        IsActive = true;
        timeActivated = Time.time;
    }
    public void ActivateTemporarily() {
        if (IsActive)
            return;
        activeTemporarily = true;
        Activate();
    }
    public void DeActivate() {
        if (!IsActive)
            return;
        IsActive = false;

        Ghost.SetActive(false);

        if (PickedUpObject != null)
            throw new System.Exception("ObjectPlacer was deactivated but still had " + PickedUpObject.name + " as PickedUpObject!");
        if (UISelectedObject != null) {
            GUIManager.Instance.CloseInfoWindow(UISelectedObject);
            Object.Destroy(UISelectedObject.gameObject);
            UISelectedObject = null;
        }

		activeTemporarily = false;
    }

    public void Setup(Transform _transform) {
		ObjectButtons [0].group.SetAllTogglesOff ();
		ObjectButtons [0].isOn = true;

        // UVController[] _allQuads = _transform.GetComponentsInChildren<UVController>(true);
		// if (_allQuads.Length > 2)
		// 	throw new System.NotImplementedException("ObjectPlacer currently only supports 2 UVControllers, but " + _allQuads.Length + " was found!");

		// _allQuads[0].Setup();
		// _allQuads[1].Setup();
		// Ghost = new GhostInfo(_allQuads[0], _allQuads[1]);
		
		UVController _uvc = _transform.GetComponentInChildren<UVController>(true);
        _uvc.Setup();
        Ghost = new GhostInfo(_uvc);
	}

    private void TryGetNewObjectToPlace(CanInspect _newPickedUpObject = null) {
        if (_newPickedUpObject != null) {
            if (PickedUpObject != null)
                throw new System.Exception("Tried to assign PickedUpObject, but " + PickedUpObject.name + " was already assigned!");

            PickedUpObject = _newPickedUpObject;
        }
        else if (activeTemporarily) // shouldn't reload UISelectedObject when we're not planning on staying in this tool
            return;
        else if (PickedUpObject == null) {
            for (int i = 0; i < ObjectButtons.Length; i++) { // skipping 0 because it's currently the None-button
                if (ObjectButtons[i].isOn) {
                    selectedObjectButtonHasChanged = oldObjectButtonIndex != i;
                    oldObjectButtonIndex = i;
                    break;
                }
            }

            // return if didn't actually change object
            if (UISelectedObject != null && !selectedObjectButtonHasChanged)
                return;

            if (UISelectedObject != null) {
                GUIManager.Instance.CloseInfoWindow(UISelectedObject);
                Object.Destroy(UISelectedObject.gameObject);
                UISelectedObject = null;
            }

			PrefabReference prefabRef = ObjectButtons[oldObjectButtonIndex].GetComponent<PrefabReference>();
			if (prefabRef != null){
				UISelectedObject = Object.Instantiate(prefabRef.Prefab).GetComponent<CanInspect>();
				UISelectedObject.Setup();
				SetAssetBottomAndTop(UISelectedObject);
			}
        }

		CanInspect objectToPlace = GetObjectToPlace();
		if (objectToPlace != null){
			GetObjectToPlace().PickUp();
		}
    }


    float timeLastGhostUpdate = -1;
    public void Update() {
        // don't want this running the first frame (causes at least one bug where a pickedup object is immediately put down)
        if (Time.time == timeActivated)
            return;

        isDeleting = false;

        if (PickedUpObject == null)
            TryGetNewObjectToPlace();

        if ((Mouse.StateLeft == Mouse.MouseStateEnum.None || Mouse.StateLeft == Mouse.MouseStateEnum.Click) && (Mouse.StateRight == Mouse.MouseStateEnum.None || Mouse.StateRight == Mouse.MouseStateEnum.Click)) {
            // click
            if (Mouse.StateLeft == Mouse.MouseStateEnum.Click || isDeleting)
                mouseGhostIsDirty = true;

            // no click
            if ((Mouse.StateLeft == Mouse.MouseStateEnum.None && Mouse.StateRight == Mouse.MouseStateEnum.None) || mouseGhostIsDirty)
                ControlMouseGhost();
        }

        if (Mouse.IsOverGUI)
            return;

        bool skip = (!isDeleting && Mouse.StateLeft == Mouse.MouseStateEnum.Release) || (isDeleting && Mouse.StateRight == Mouse.MouseStateEnum.Release);
        if (!skip && Time.time - timeLastGhostUpdate < 0.01f)
            return;
        if (Mouse.StateLeft == Mouse.MouseStateEnum.Hold || Mouse.StateRight == Mouse.MouseStateEnum.Hold) {
            DetermineGhostPositions(_hasClicked: true, _snapToNeighbours: false);
            timeLastGhostUpdate = Time.time;
        }

        // click released
        if (Mouse.StateRight == Mouse.MouseStateEnum.Release || GetObjectToPlace() == null)
            FinishRound();
    }
    private void ControlMouseGhost() {
        // find current tile
        oldMouseGridPos = mouseTile == null ? Int2.Zero : mouseTile.GridPos;
        mouseTile = GameGrid.Instance.GetNodeFromWorldPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));

        mouseGhostHasNewTile = oldMouseGridPos != mouseTile.GridPos;
        if (mouseGhostHasNewTile)
            mouseGhostIsDirty = true;

        // set ghost-transform
        Ghost.SetPosition(mouseTile.GridPos);

        if (mouseGhostIsDirty) {
            mouseGhostIsDirty = false;
            DetermineGhostPositions(_hasClicked: false, _snapToNeighbours: mouseGhostHasNewTile);
        }
    }

    private void DetermineGhostPositions(bool _hasClicked, bool _snapToNeighbours) {

        // find current tile
        if (!_hasClicked)
            startTile = GameGrid.Instance.GetNodeFromWorldPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        else
            mouseTile = GameGrid.Instance.GetNodeFromWorldPos(Camera.main.ScreenToWorldPoint(Input.mousePosition));

        // reset old stuff
        selectedTiles.Clear();

        Ghost.SetActive(true);
        Ghost.SetPosition(mouseTile.GridPos);
        Evaluate();
    }

    private void FinishRound() {
        ApplyCurrentTool();
        if (activeTemporarily && PickedUpObject == null) {
            DeActivate();
            return;
        }

        mouseGhostIsDirty = true;
    }

    List<Node> neighbours;
    private void Evaluate(){

		// deleting old tiles
		if (isDeleting) {
			// is building even allowed?
			if (!mouseTile.IsBuildingAllowed) {
				ApplySettingsToGhost(false, ColorIndex_Blocked);
				return;
			}

			ApplySettingsToGhost(true, ColorIndex_Remove);
			return;
		}


		// adding new tiles

		// is building even allowed?
		if (!mouseTile.IsBuildingAllowed) {
			ApplySettingsToGhost(false, ColorIndex_Blocked);
			return;
		}

		// is the tile below covered by some kind of wall?
		if (mouseTile.IsWall) {
			ApplySettingsToGhost(false, ColorIndex_Blocked);
			return;
		}

        // is the tile below occupied by another object (or actor)?
        NodeObject _occupant = mouseTile.GetOccupyingTileObject();
		if (_occupant != null) {
            bool _isInspectable = _occupant.GetComponent<CanInspect>() != null;
            if (_isInspectable) {
                ApplySettingsToGhost(false, ColoringTool.COLOR_WHITE, hide: true);
                return;
            }

            ApplySettingsToGhost(false, ColorIndex_Blocked);
			return;
		}

		// all's good
		ApplySettingsToGhost(true, ColorIndex_New);
	}
    private void ApplySettingsToGhost(bool _applyToGrid, byte _newColorIndex, bool hide = false) {
        // apply color and position
        Ghost.SetPosition(mouseTile.GridPos);
        Ghost.SetActive(!hide);
        Ghost.SetColor(_newColorIndex);

        // mark tile for changes
        if (_applyToGrid) {
            selectedTiles.Add(mouseTile);
        }
    }

    private void ApplyCurrentTool() {
        if (selectedTiles.Count > 0) {
            NodeObject _occupant = selectedTiles[0].GetOccupyingTileObject();
            CanInspect _occupyingInspectable = _occupant != null ? _occupant.GetComponent<CanInspect>() : null;
            if (_occupyingInspectable != null) {
                CanInspect _formerlyPickedUp;
				TrySwitchComponents(_occupyingInspectable, _occupant.Parent, true, /*false, */out _formerlyPickedUp);
            }
            else
                PutDownPickedUp(selectedTiles[0]);
        }
	
        // reset stuff
        selectedTiles.Clear();
        Ghost.SetActive(false);
	}

    private void PutDownPickedUp(Node _node) {
        if (GetObjectToPlace() == null)
            return;

		GetObjectToPlace().PutDown(_node);
        Mouse.Instance.TryDeselectSelectedObject();
		SetObjectToPlace(null);
	}
	public bool TrySwitchComponents(CanInspect _pickUpThis, NodeObject _pickUpObjsParent, bool _hideOnPickup, out CanInspect _putThisDown) {
        _putThisDown = null;

        // can this thing even be picked up?
        if (_pickUpThis != null && !_pickUpThis.CanBePickedUp)
            return false;

        // cache the old object (need to pick up new one before putting old down)
        _putThisDown = GetObjectToPlace();
		SetObjectToPlace(null);

		// pick up the new object
		if (_pickUpThis != null) {
            TryGetNewObjectToPlace(_pickUpThis);
            SetAssetBottomAndTop(GetObjectToPlace());

            if (!IsActive)
                ActivateTemporarily();
        }
        else
            SetAssetBottomAndTop(null);

        // put down the old object
        if (_putThisDown != null) {
            if (_pickUpThis == null)
                _putThisDown.PutOffGrid(null, Vector3.zero, _hide: true); // used by ComponentSlots
            else {
				if (_pickUpObjsParent != null)
					_putThisDown.PutOffGrid(_pickUpObjsParent, Vector3.zero, _hide: true);
                else
                    _putThisDown.PutDown(_pickUpThis.MyTileObject.MyTile);
            }
        }

        return true;
    }

    private void SetAssetBottomAndTop(CanInspect _obj) {
        AssetBottom = CachedAssets.Instance.AssetSets[0].Empty;
        AssetTop = CachedAssets.Instance.AssetSets[0].Empty;
        if (_obj == null)
            return;

        UVController uvc = GetObjectToPlace().GetComponent<NodeObject>().MyUVController;
		AssetBottom = uvc.GridLayers[(int)MeshSorter.GridLayerEnum.Bottom].Coordinates;
		AssetTop 	= uvc.GridLayers[(int)MeshSorter.GridLayerEnum.Top].Coordinates;
    }
}