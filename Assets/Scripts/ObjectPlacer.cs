using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

[System.Serializable]
public class ObjectPlacer : BuilderBase {

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


    public void ActivateTemporarily() {
        if (IsActive)
            return;
        activeTemporarily = true;
        Activate();
    }
    public override void DeActivate() {
        base.DeActivate();
        if (PickedUpObject != null)
            throw new System.Exception("ObjectPlacer was deactivated but still had " + PickedUpObject.name + " as PickedUpObject!");
        if (UISelectedObject != null) {
            GUIManager.Instance.CloseInfoWindow(UISelectedObject);
            Object.Destroy(UISelectedObject.gameObject);
            UISelectedObject = null;
        }

		activeTemporarily = false;
    }

    public override void Setup() {
		ObjectButtons [0].group.SetAllTogglesOff ();
		ObjectButtons [0].isOn = true;
		base.Setup ();
	}

    protected override void OnNewRound() {
        //GetNewObjectToPlace();
        base.OnNewRound();
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

    protected override void InheritedUpdate() {
        isDeleting = false;

        if (PickedUpObject == null)
            TryGetNewObjectToPlace();

        base.InheritedUpdate ();
	}

    protected override void TryChangeMode() {
        Mode = ModeEnum.ObjectPlacing; // needed to override being able to drag out ghosts
    }
    protected override bool ShouldFinish() {
        //Debug.Log((PickedUpObject == null) + ", " + (UISelectedObject == null)); // how about some bool in TrySwitch? :/
        //return PickedUpObject == null && UISelectedObject == null;
        return Mouse.StateRight == Mouse.MouseStateEnum.Release || _ObjectToPlace_ == null;
    }
    protected override void FinishRound() {
        ApplyCurrentTool();
        if (activeTemporarily && PickedUpObject == null) {
            DeActivate();
            return;
        }

        mouseGhostIsDirty = true;
        OnNewRound();
    }

    protected override void SetGhostGraphics(ref GhostInfo _ghost, Tile _tileUnderGhost, bool _snapToNeighbours) {
		_ghost.Type = Tile.Type.Empty;
		_ghost.Orientation = Tile.TileOrientation.None;
		_ghost.ChangeAssets (AssetBottom, AssetTop);

        base.SetGhostGraphics (ref _ghost, _tileUnderGhost, _snapToNeighbours);
	}

	List<Tile> neighbours;
	int diffX;
	int diffY;
	protected override void Evaluate(GhostInfo _ghost, Tile _tileUnderGhost, Tile.TileOrientation _orientation){

		// deleting old tiles
		if (isDeleting) {
			// is building even allowed?
			if (!_tileUnderGhost._BuildingAllowed_ && _tileUnderGhost._FloorType_ != Tile.Type.Empty) { // empty tiles allowed for deletion bc it looks better
				ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_Blocked);
				return;
			}

			// is the tile being used for something currently?
			if (_tileUnderGhost.ThingsUsingThis > 0) {
				ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_Blocked);
				return;
			}

			ApplySettingsToGhost(_ghost, _tileUnderGhost, true, Color_Remove);
			return;
		}


		// adding new tiles

		// is building even allowed?
		if (!_tileUnderGhost._BuildingAllowed_) {
			ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_Blocked);
			return;
		}
		// is the tile below covered by some kind of wall?
		if (_tileUnderGhost._WallType_ != Tile.Type.Empty) {
			ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_Blocked);
			return;
		}
		// is the tile below without floor?
		if (_tileUnderGhost._FloorType_ == Tile.Type.Empty) {
			ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_Blocked);
			return;
		}
        // is the tile below occupied by another object (or actor)?
		if (_tileUnderGhost.IsOccupiedByObject) {
            if (_tileUnderGhost.OccupyingInspectable != null) {
                ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color.clear);
                return;
            }

            ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_Blocked);
			return;
		}
//		// is the tile below without floor?
//		if (_tileUnderGhost.IsOccupiedByObject) { // TODO: should check if the object is the same as the one being placed!
//			ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_AlreadyExisting);
//			return;
//		}

		// all's good
		ApplySettingsToGhost(_ghost, _tileUnderGhost, true, Color_New);
	}

	protected override void ApplyCurrentTool() {
        if (selectedTiles.Count > 0) {
            if (selectedTiles[0].OccupyingInspectable != null) {
                CanInspect _formerlyPickedUp;
				TrySwitchComponents(selectedTiles[0].OccupyingInspectable, selectedTiles[0].OccupyingInspectable.MyTileObject.Parent, true, /*false, */out _formerlyPickedUp);
            }
            else
                PutDownPickedUp(selectedTiles[0], selectedTilesNewOrientation[0]);
        }
	
		base.ApplyCurrentTool ();
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