using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ObjectPlacer {

	public Toggle[] ObjectButtons;
    public CanInspect PickedUpObject;
    private CanInspect UISelectedObject;
    public CanInspect _ObjectToPlace_ {
        get {
            return (PickedUpObject != null ? PickedUpObject : UISelectedObject); }
        set {
            if (PickedUpObject != null)
                PickedUpObject = value;
            else
                UISelectedObject = value;
        }
    }
    private CachedAssets.DoubleInt AssetBottom;
    private CachedAssets.DoubleInt AssetTop;
    private bool activeTemporarily = false;
    private int oldObjectButtonIndex = -1;
    private bool selectedObjectButtonHasChanged = false;




    [SerializeField] private Color Color_New = Color.white;
    [SerializeField] private Color Color_Remove = Color.red;
    [SerializeField] private Color Color_AlreadyExisting = Color.grey;
    [SerializeField] private Color Color_Blocked = (Color.yellow + Color.red) * 0.5f;

    [System.NonSerialized]
    public bool IsActive = false;

    private IEnumerator ghostRoutine;
    public class GhostInfo {
        public UVController BottomQuad;
        public UVController TopQuad;
        public Vector3 position { get; private set; }
        public Tile.Type Type;
        public Tile.TileOrientation Orientation;

        public GhostInfo(UVController _bottomQuad, UVController _topQuad) {
            BottomQuad = _bottomQuad;
            TopQuad = _topQuad;
            SetPosition(Vector2.zero);
            Type = Tile.Type.Empty;
            Orientation = Tile.TileOrientation.None;
        }

        private const float DEFAULT_OFFSET_Y = 0.5f;
        private Vector3 newPos;
        public void SetPosition(Vector3 _value) {
            newPos = new Vector3(Grid.Instance.grid[0, 0].WorldPosition.x + _value.x, Grid.Instance.grid[0, 0].WorldPosition.y + _value.y + DEFAULT_OFFSET_Y, Grid.WORLD_TOP_HEIGHT);
            BottomQuad.transform.position = newPos;
            TopQuad.transform.position = newPos;
            position = _value;
        }
        public void ChangeAssets(CachedAssets.DoubleInt _bottomIndices, CachedAssets.DoubleInt _topIndices) {
            BottomQuad.ChangeAsset(_bottomIndices);
            TopQuad.ChangeAsset(_topIndices);
        }
        public void SetColor(Color _color) {
            BottomQuad.ChangeColor(_color);
            TopQuad.ChangeColor(_color);
        }
        public void SetActive(bool _b) {
            BottomQuad.gameObject.SetActive(_b);
            TopQuad.gameObject.SetActive(_b);
        }
    }
    private GhostInfo Ghost;

    private Vector2 oldMouseGridPos;

    private Tile startTile;
    private Tile mouseTile;

    private bool isDeleting = false;
    private bool mouseGhostHasNewTile = false;
    private bool mouseGhostIsDirty = true;

    private List<Tile> selectedTiles = new List<Tile>();
    private List<Tile.Type> selectedTilesNewType = new List<Tile.Type>();
    private List<Tile.TileOrientation> selectedTilesNewOrientation = new List<Tile.TileOrientation>();


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

        UVController[] _allQuads = _transform.GetComponentsInChildren<UVController>(true);
        if (_allQuads.Length > 2)
            throw new System.NotImplementedException("ObjectPlacer currently only supports 2 UVControllers, but " + _allQuads.Length + " was found!");

        _allQuads[0].Setup();
        _allQuads[1].Setup();
        Ghost = new GhostInfo(_allQuads[0], _allQuads[1]);
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
            for (int i = 0; i < ObjectButtons.Length; i++) {
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

            UISelectedObject = Object.Instantiate(ObjectButtons[oldObjectButtonIndex].GetComponent<PrefabReference>().Prefab).GetComponent<CanInspect>();
            UISelectedObject.Setup();
            SetAssetBottomAndTop(UISelectedObject);
        }

        _ObjectToPlace_.PickUp();
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
        if (Mouse.StateRight == Mouse.MouseStateEnum.Release || _ObjectToPlace_ == null)
            FinishRound();
    }
    private void ControlMouseGhost() {
        // find current tile
        oldMouseGridPos = mouseTile == null ? Vector2.zero : new Vector2(mouseTile.GridX, mouseTile.GridY);
        mouseTile = Grid.Instance.GetTileFromWorldPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));

        mouseGhostHasNewTile = oldMouseGridPos.x != mouseTile.GridX || oldMouseGridPos.y != mouseTile.GridY;
        if (mouseGhostHasNewTile)
            mouseGhostIsDirty = true;

        // set ghost-transform
        Ghost.SetPosition(new Vector3(mouseTile.GridX, mouseTile.GridY, Grid.WORLD_BOTTOM_HEIGHT));
        Ghost.Orientation = TryRotateMouseGhost();

        if (mouseGhostIsDirty) {
            mouseGhostIsDirty = false;
            DetermineGhostPositions(_hasClicked: false, _snapToNeighbours: mouseGhostHasNewTile);
        }
    }
    private Tile.TileOrientation TryRotateMouseGhost() {
        // rotate diagonals with Q&E
        int _rotateDirection = 0;
        _rotateDirection += Input.GetKeyUp(KeyCode.E) ? -1 : 0;
        _rotateDirection += Input.GetKeyUp(KeyCode.Q) ? 1 : 0;
        if (_rotateDirection != 0) {
            mouseGhostIsDirty = true;

            if (Ghost.Type == Tile.Type.Diagonal) {
                switch (Ghost.Orientation) {
                    case Tile.TileOrientation.None:
                    case Tile.TileOrientation.BottomLeft:
                        return _rotateDirection > 0 ? Tile.TileOrientation.BottomRight : Tile.TileOrientation.TopLeft;
                    case Tile.TileOrientation.TopLeft:
                        return _rotateDirection > 0 ? Tile.TileOrientation.BottomLeft : Tile.TileOrientation.TopRight;
                    case Tile.TileOrientation.TopRight:
                        return _rotateDirection > 0 ? Tile.TileOrientation.TopLeft : Tile.TileOrientation.BottomRight;
                    case Tile.TileOrientation.BottomRight:
                        return _rotateDirection > 0 ? Tile.TileOrientation.TopRight : Tile.TileOrientation.BottomLeft;
                }
            }
            else {
                switch (Ghost.Orientation) {
                    case Tile.TileOrientation.None:
                    case Tile.TileOrientation.Bottom:
                        return _rotateDirection > 0 ? Tile.TileOrientation.Right : Tile.TileOrientation.Left;
                    case Tile.TileOrientation.Left:
                        return _rotateDirection > 0 ? Tile.TileOrientation.Bottom : Tile.TileOrientation.Top;
                    case Tile.TileOrientation.Top:
                        return _rotateDirection > 0 ? Tile.TileOrientation.Left : Tile.TileOrientation.Right;
                    case Tile.TileOrientation.Right:
                        return _rotateDirection > 0 ? Tile.TileOrientation.Top : Tile.TileOrientation.Bottom;
                }
            }
        }

        return Ghost.Orientation;
    }

    private void DetermineGhostPositions(bool _hasClicked, bool _snapToNeighbours) {

        // find current tile
        if (!_hasClicked)
            startTile = Grid.Instance.GetTileFromWorldPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));
        else
            mouseTile = Grid.Instance.GetTileFromWorldPoint(Camera.main.ScreenToWorldPoint(Input.mousePosition));

        // reset old stuff
        selectedTiles.Clear();
        selectedTilesNewType.Clear();
        selectedTilesNewOrientation.Clear();

        Ghost.SetActive(true);
        Ghost.SetPosition(new Vector2(mouseTile.GridX, mouseTile.GridY));
        SetGhostGraphics(mouseTile, _snapToNeighbours);
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

    private void SetGhostGraphics(Tile _tileUnderGhost, bool _snapToNeighbours) {
		Ghost.Type = Tile.Type.Empty;
        Ghost.Orientation = Tile.TileOrientation.None;
        Ghost.ChangeAssets (AssetBottom, AssetTop);

        // if a diagonal is below, sort ghost so the diagonal covers it in a pretty way
        if (_tileUnderGhost.ConnectedDiagonal_B != null) {
            Ghost.BottomQuad.SortCustom(_tileUnderGhost.BottomQuad.GetSortOrder() + 1);
            Ghost.TopQuad.SortCustom(_tileUnderGhost.TopQuad.GetSortOrder() + 1);
        }
        // otherwise just go on top
        else {
            Ghost.BottomQuad.SortCustom(_tileUnderGhost.TopQuad.GetSortOrder() + 1);
            Ghost.TopQuad.SortCustom(_tileUnderGhost.TopQuad.GetSortOrder() + 2);
        }
	}

    List<Tile> neighbours;
    private void Evaluate(){

		// deleting old tiles
		if (isDeleting) {
			// is building even allowed?
			if (!mouseTile._BuildingAllowed_ && mouseTile._FloorType_ != Tile.Type.Empty) { // empty tiles allowed for deletion bc it looks better
				ApplySettingsToGhost(false, Color_Blocked);
				return;
			}

			// is the tile being used for something currently?
			if (mouseTile.ThingsUsingThis > 0) {
				ApplySettingsToGhost(false, Color_Blocked);
				return;
			}

			ApplySettingsToGhost(true, Color_Remove);
			return;
		}


		// adding new tiles

		// is building even allowed?
		if (!mouseTile._BuildingAllowed_) {
			ApplySettingsToGhost(false, Color_Blocked);
			return;
		}
		// is the tile below covered by some kind of wall?
		if (mouseTile._WallType_ != Tile.Type.Empty) {
			ApplySettingsToGhost(false, Color_Blocked);
			return;
		}
		// is the tile below without floor?
		if (mouseTile._FloorType_ == Tile.Type.Empty) {
			ApplySettingsToGhost(false, Color_Blocked);
			return;
		}
        // is the tile below occupied by another object (or actor)?
		if (mouseTile.IsOccupiedByObject) {
            if (mouseTile.OccupyingInspectable != null) {
                ApplySettingsToGhost(false, Color.clear);
                return;
            }

            ApplySettingsToGhost(false, Color_Blocked);
			return;
		}
//		// is the tile below without floor?
//		if (_tileUnderGhost.IsOccupiedByObject) { // TODO: should check if the object is the same as the one being placed!
//			ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_AlreadyExisting);
//			return;
//		}

		// all's good
		ApplySettingsToGhost(true, Color_New);
	}
    private void ApplySettingsToGhost(bool _applyToGrid, Color _newColor) {
        // apply color and position
        Ghost.SetActive(true);
        Ghost.SetColor(_newColor);
        Ghost.SetPosition(new Vector2(mouseTile.GridX, mouseTile.GridY));

        // mark tile for changes
        if (_applyToGrid) {
            selectedTiles.Add(mouseTile);
            selectedTilesNewType.Add(Ghost.Type);
            selectedTilesNewOrientation.Add(Ghost.Orientation);
        }
    }

    private void ApplyCurrentTool() {
        if (selectedTiles.Count > 0) {
            if (selectedTiles[0].OccupyingInspectable != null) {
                CanInspect _formerlyPickedUp;
				TrySwitchComponents(selectedTiles[0].OccupyingInspectable, selectedTiles[0].OccupyingInspectable.MyTileObject.Parent, true, /*false, */out _formerlyPickedUp);
            }
            else
                PutDownPickedUp(selectedTiles[0], selectedTilesNewOrientation[0]);
        }
	
        // reset stuff
        selectedTiles.Clear();
        selectedTilesNewType.Clear();
        selectedTilesNewOrientation.Clear();
        Ghost.SetActive(false);
	}

    private void PutDownPickedUp(Tile _tile, Tile.TileOrientation _orientation) {
        if (_ObjectToPlace_ == null)
            return;

        _ObjectToPlace_.PutDown(_tile/**, _orientation*/);
        Mouse.Instance.TryDeselectSelectedObject();
        _ObjectToPlace_ = null;
    }
	public bool TrySwitchComponents(CanInspect _pickUpThis, TileObject _pickUpObjsParent, bool _hideOnPickup, out CanInspect _putThisDown) {
        _putThisDown = null;

        // can this thing even be picked up?
        if (_pickUpThis != null && !_pickUpThis.CanBePickedUp)
            return false;

        // cache the old object (need to pick up new one before putting old down)
        _putThisDown = _ObjectToPlace_;
        _ObjectToPlace_ = null;

        // pick up the new object
        if (_pickUpThis != null) {
            TryGetNewObjectToPlace(_pickUpThis);
            SetAssetBottomAndTop(_ObjectToPlace_);

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

    UVController[] uvc;
    private void SetAssetBottomAndTop(CanInspect _obj) {
        AssetBottom = null;
        AssetTop = null;
        if (_obj == null)
            return;

        uvc = _ObjectToPlace_.GetComponent<TileObject>().MyUVControllers;
        if (uvc.Length > 2)
            throw new System.NotImplementedException(_ObjectToPlace_ + " has " + uvc.Length + " UVControllers, but only 2 are supported currently!");

        for (int i = 0; i < Mathf.Min(2, uvc.Length); i++) {
            if (uvc[i].SortingLayer == UVController.SortingLayerEnum.Bottom) {
                if (AssetBottom != null)
                    throw new System.Exception(_ObjectToPlace_ + " had more than 1 UVController of the same SortingLayer!");
                AssetBottom = uvc[i].Coordinates;
            }
            else if (uvc[i].SortingLayer == UVController.SortingLayerEnum.Top) {
                if (AssetTop != null)
                    throw new System.Exception(_ObjectToPlace_ + " had more than 1 UVController of the same SortingLayer!");

                AssetTop = uvc[i].Coordinates;
            }
        }
    }
}