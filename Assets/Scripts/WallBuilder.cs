using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WallBuilder : BuilderBase {

	private Color32[] AllColors;

	protected override void TryChangeMode(){
		base.TryChangeMode ();

		if (Input.GetKey(KeyCode.Alpha1))
			Mode = ModeEnum.Room;
		if (Input.GetKey(KeyCode.Alpha2))
			Mode = ModeEnum.Diagonal;
		if (Input.GetKey(KeyCode.Alpha3))
			Mode = ModeEnum.Door;
		if (Input.GetKey(KeyCode.Alpha4))
			Mode = ModeEnum.Airlock;
	}

    private static bool neighboursPassedEval = true;
    protected override bool AddGhostsForConnectedDiagonals(Tile _tile) {
        neighboursPassedEval = true;

        if (_tile.ConnectedDiagonal_B != null) {
            AddNextGhost(_tile.ConnectedDiagonal_B.GridX, _tile.ConnectedDiagonal_B.GridY, DetermineGhostType(_tile.ConnectedDiagonal_B), DetermineGhostOrientation(_tile.ConnectedDiagonal_B, false), false);
            SetGhostType(_tile.ConnectedDiagonal_B);
            SetGhostGraphics(_tile.ConnectedDiagonal_B, false);
            if (!Evaluate(_tile.ConnectedDiagonal_B))
                neighboursPassedEval = false;
        }
        if (_tile.ConnectedDiagonal_L != null) {
            AddNextGhost(_tile.ConnectedDiagonal_L.GridX, _tile.ConnectedDiagonal_L.GridY, DetermineGhostType(_tile.ConnectedDiagonal_L), DetermineGhostOrientation(_tile.ConnectedDiagonal_L, false), false);
            SetGhostType(_tile.ConnectedDiagonal_L);
            SetGhostGraphics(_tile.ConnectedDiagonal_L, false);
            if (!Evaluate(_tile.ConnectedDiagonal_L))
                neighboursPassedEval = false;
        }
        if (_tile.ConnectedDiagonal_T != null) {
            AddNextGhost(_tile.ConnectedDiagonal_T.GridX, _tile.ConnectedDiagonal_T.GridY, DetermineGhostType(_tile.ConnectedDiagonal_T), DetermineGhostOrientation(_tile.ConnectedDiagonal_T, false), false);
            SetGhostType(_tile.ConnectedDiagonal_T);
            SetGhostGraphics(_tile.ConnectedDiagonal_T, false);
            if (!Evaluate(_tile.ConnectedDiagonal_T))
                neighboursPassedEval = false;
        }
        if (_tile.ConnectedDiagonal_R != null) {
            AddNextGhost(_tile.ConnectedDiagonal_R.GridX, _tile.ConnectedDiagonal_R.GridY, DetermineGhostType(_tile.ConnectedDiagonal_R), DetermineGhostOrientation(_tile.ConnectedDiagonal_R, false), false);
            SetGhostType(_tile.ConnectedDiagonal_R);
            SetGhostGraphics(_tile.ConnectedDiagonal_R, false);
            if (!Evaluate(_tile.ConnectedDiagonal_R))
                neighboursPassedEval = false;
        }

        return neighboursPassedEval;
    }
    protected override bool AddGhostsForConnectedDoors(Tile _tile) {
        neighboursPassedEval = true;

        if (_tile.ConnectedDoorOrAirlock_B != null) {
            AddNextGhost(_tile.ConnectedDoorOrAirlock_B.GridX, _tile.ConnectedDoorOrAirlock_B.GridY, DetermineGhostType(_tile.ConnectedDoorOrAirlock_B), DetermineGhostOrientation(_tile.ConnectedDoorOrAirlock_B, false), false);
            SetGhostType(_tile.ConnectedDoorOrAirlock_B);
            SetGhostGraphics(_tile.ConnectedDoorOrAirlock_B, false);
            if (!Evaluate(_tile.ConnectedDoorOrAirlock_B))
                neighboursPassedEval = false;
        }
        if (_tile.ConnectedDoorOrAirlock_L != null) {
            AddNextGhost(_tile.ConnectedDoorOrAirlock_L.GridX, _tile.ConnectedDoorOrAirlock_L.GridY, DetermineGhostType(_tile.ConnectedDoorOrAirlock_L), DetermineGhostOrientation(_tile.ConnectedDoorOrAirlock_L, false), false);
            SetGhostType(_tile.ConnectedDoorOrAirlock_L);
            SetGhostGraphics(_tile.ConnectedDoorOrAirlock_L, false);
            if (!Evaluate(_tile.ConnectedDoorOrAirlock_L))
                neighboursPassedEval = false;
        }
        if (_tile.ConnectedDoorOrAirlock_T != null) {
            AddNextGhost(_tile.ConnectedDoorOrAirlock_T.GridX, _tile.ConnectedDoorOrAirlock_T.GridY, DetermineGhostType(_tile.ConnectedDoorOrAirlock_T), DetermineGhostOrientation(_tile.ConnectedDoorOrAirlock_T, false), false);
            SetGhostType(_tile.ConnectedDoorOrAirlock_T);
            SetGhostGraphics(_tile.ConnectedDoorOrAirlock_T, false);
            if (!Evaluate(_tile.ConnectedDoorOrAirlock_T))
                neighboursPassedEval = false;
        }
        if (_tile.ConnectedDoorOrAirlock_R != null) {
            AddNextGhost(_tile.ConnectedDoorOrAirlock_R.GridX, _tile.ConnectedDoorOrAirlock_R.GridY, DetermineGhostType(_tile.ConnectedDoorOrAirlock_R), DetermineGhostOrientation(_tile.ConnectedDoorOrAirlock_R, false), false);
            SetGhostType(_tile.ConnectedDoorOrAirlock_R);
            SetGhostGraphics(_tile.ConnectedDoorOrAirlock_R, false);
            if (!Evaluate(_tile.ConnectedDoorOrAirlock_R))
                neighboursPassedEval = false;
        }

        return neighboursPassedEval;
    }

    protected override void ResetModifiedTiles(bool _includingMouse = false) {
		for (int i = 0; i < modifiedTiles.Count; i++) {
			modifiedTiles[i].SetTileType(Tile.Type.Empty, Tile.TileOrientation.None, _temporarily: true);
            modifiedTiles[i].ResetTempSettingsWall();
            modifiedTiles[i].SetWallColor(Color.white);
            modifiedTiles[i].SetFloorColor(Color.white);
		}

		base.ResetModifiedTiles (_includingMouse);
	}
	protected override void ResetSelectedTiles() {
		for (int i = 0; i < selectedTiles.Count; i++) {
			selectedTiles[i].SetTileType(Tile.Type.Empty, selectedTiles[i].TempOrientation, _temporarily: true);
            selectedTiles[i].ResetTempSettingsWall();
            selectedTiles[i].SetWallColor(Color.white);
            selectedTiles[i].SetFloorColor(Color.white);
		}

		base.ResetSelectedTiles ();
	}

    protected override Tile.Type DetermineGhostType(Tile _tile) {
        if (isDeleting)
            return Tile.Type.Empty;
        switch (Mode) {
            case ModeEnum.Default:
            case ModeEnum.Room:
            case ModeEnum.Fill:
                return Tile.Type.Solid;
            case ModeEnum.Diagonal:
                return Tile.Type.Diagonal;
            case ModeEnum.Door:
                return Tile.Type.Door;
            case ModeEnum.Airlock:
                return Tile.Type.Airlock;
            case ModeEnum.ObjectPlacing:
                throw new System.Exception(Mode.ToString() + " does not apply to Walls!");
            default:
                throw new System.NotImplementedException(Mode.ToString() + " hasn't been properly implemented yet!");
        }
    }
    protected override Tile.TileOrientation DetermineGhostOrientation(Tile _tile, bool _snapToNeighbours) {
        switch (Mode) {
            case ModeEnum.Default:
            case ModeEnum.Room:
            case ModeEnum.Fill:
            case ModeEnum.ObjectPlacing:
                return Tile.TileOrientation.None;
            case ModeEnum.Diagonal:
                // diagonal top left
                if ((_snapToNeighbours && _tile.HasConnectable_L && _tile.HasConnectable_T) || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.TopLeft))
                    return Tile.TileOrientation.TopLeft;
                // diagonal top right
                else if ((_snapToNeighbours && _tile.HasConnectable_T && _tile.HasConnectable_R) || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.TopRight))
                    return Tile.TileOrientation.TopRight;
                // diagonal bottom right
                else if ((_snapToNeighbours && _tile.HasConnectable_R && _tile.HasConnectable_B) || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.BottomRight))
                    return Tile.TileOrientation.BottomRight;
                // diagonal bottom left
                else if ((_snapToNeighbours && _tile.HasConnectable_B && _tile.HasConnectable_L) || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.BottomLeft))
                    return Tile.TileOrientation.BottomLeft;
                else
                    return _snapToNeighbours ? Tile.TileOrientation.TopLeft : _tile.TempOrientation;
            case ModeEnum.Door:
                if ((_snapToNeighbours && (_tile.HasConnectable_L && _tile.HasConnectable_R && !_tile.HasConnectable_B && !_tile.HasConnectable_T)) || (!_snapToNeighbours && (_tile.TempOrientation == Tile.TileOrientation.Left || _tile.TempOrientation == Tile.TileOrientation.Right)))
                    return Tile.TileOrientation.Left; // left or right shouldn't matter...
                else if ((_snapToNeighbours && (!_tile.HasConnectable_L && !_tile.HasConnectable_R && _tile.HasConnectable_B && _tile.HasConnectable_T)) || (!_snapToNeighbours && (_tile.TempOrientation == Tile.TileOrientation.Bottom || _tile.TempOrientation == Tile.TileOrientation.Top)))
                    return Tile.TileOrientation.Bottom; // bottom or top shouldn't matter...
                else
                   return _snapToNeighbours ? Tile.TileOrientation.Left : _tile.TempOrientation;
            case ModeEnum.Airlock:
                if ((_snapToNeighbours && (_tile.HasConnectable_L && _tile.HasConnectable_R && !_tile.HasConnectable_B && !_tile.HasConnectable_T)) || (!_snapToNeighbours && (_tile.TempOrientation == Tile.TileOrientation.Left || _tile.TempOrientation == Tile.TileOrientation.Right)))
                    return Tile.TileOrientation.Left; // left or right shouldn't matter...
                else if ((_snapToNeighbours && (!_tile.HasConnectable_L && !_tile.HasConnectable_R && _tile.HasConnectable_B && _tile.HasConnectable_T)) || (!_snapToNeighbours && (_tile.TempOrientation == Tile.TileOrientation.Bottom || _tile.TempOrientation == Tile.TileOrientation.Top)))
                    return Tile.TileOrientation.Bottom; // bottom or top shouldn't matter...
                else
                    return _snapToNeighbours ? Tile.TileOrientation.Left : _tile.TempOrientation;

            default:
                throw new System.NotImplementedException(Mode.ToString() + " hasn't been properly implemented yet!");
        }
    }
    protected override void SetGhostGraphics(Tile _tile, bool _snapToNeighbours) {

		bool _hasConnection_L = (Tile.sTryTempCacheNeighbour_L(_tile.GridX, _tile.GridY) && Tile.sCachedNeighbour_L.TempType != Tile.Type.Empty && Tile.sCachedNeighbour_L.TempType != Tile.sCachedNeighbour_L._WallType_);
		bool _hasConnection_T = (Tile.sTryTempCacheNeighbour_T(_tile.GridX, _tile.GridY) && Tile.sCachedNeighbour_T.TempType != Tile.Type.Empty && Tile.sCachedNeighbour_T.TempType != Tile.sCachedNeighbour_T._WallType_);
		bool _hasConnection_R = (Tile.sTryTempCacheNeighbour_R(_tile.GridX, _tile.GridY) && Tile.sCachedNeighbour_R.TempType != Tile.Type.Empty && Tile.sCachedNeighbour_R.TempType != Tile.sCachedNeighbour_R._WallType_);
		bool _hasConnection_B = (Tile.sTryTempCacheNeighbour_B(_tile.GridX, _tile.GridY) && Tile.sCachedNeighbour_B.TempType != Tile.Type.Empty && Tile.sCachedNeighbour_B.TempType != Tile.sCachedNeighbour_B._WallType_);

        switch (Mode) {
            case ModeEnum.Default:
            case ModeEnum.Room:
			case ModeEnum.Fill:
                //if (isDeleting) {
                //    //_tile.ChangeWallGraphics(
                //    //    CachedAssets.Instance.GetWallAssetForTile(_tile._WallType_, _tile._Orientation_, 0, true, _tile.HasConnectable_L, _tile.HasConnectable_T, _tile.HasConnectable_R, _tile.HasConnectable_B),
                //    //    CachedAssets.Instance.GetWallAssetForTile(_tile._WallType_, _tile._Orientation_, 0, false, _tile.HasConnectable_L, _tile.HasConnectable_T, _tile.HasConnectable_R, _tile.HasConnectable_B),
                //    //    true
                //    //);
                //}
                if (!isDeleting && _tile._WallType_ != _tile.TempType) {
                    _tile.ChangeWallGraphics(
						CachedAssets.Instance.GetWallAssetForTile(_tile.TempType, _tile.TempOrientation, 0, true, _hasConnection_L, _hasConnection_T, _hasConnection_R, _hasConnection_B),
						CachedAssets.Instance.GetWallAssetForTile(_tile.TempType, _tile.TempOrientation, 0, false, _hasConnection_L, _hasConnection_T, _hasConnection_R, _hasConnection_B),
                        true
                    );
                }
                break;

            case ModeEnum.Diagonal:

                // default values 
                _tile.ChangeWallGraphics(
                   CachedAssets.WallSet.wall_Diagonal_TopLeft,
                   null,
                   true);

                
                // diagonal top left
                if ((_snapToNeighbours && _tile.HasConnectable_L && _tile.HasConnectable_T)
                || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.TopLeft)) {

                    _tile.ChangeWallGraphics(
                         CachedAssets.WallSet.wall_Diagonal_TopLeft,
                         null,
                         true);
                }

                // diagonal top right
                else if ((_snapToNeighbours && _tile.HasConnectable_T && _tile.HasConnectable_R)
                     || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.TopRight)) {

                    _tile.ChangeWallGraphics(
                        CachedAssets.WallSet.wall_Diagonal_TopRight,
                        null,
                        true);
                }

                // diagonal bottom right
                else if ((_snapToNeighbours && _tile.HasConnectable_R && _tile.HasConnectable_B)
                     || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.BottomRight)) {

                    _tile.ChangeWallGraphics(
                        CachedAssets.WallSet.wall_Diagonal_BottomRight,
                        null,
                        true);
                }

                // diagonal bottom left
                else if ((_snapToNeighbours && _tile.HasConnectable_B && _tile.HasConnectable_L)
                     || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.BottomLeft)) {

                    _tile.ChangeWallGraphics(
                       CachedAssets.WallSet.wall_Diagonal_BottomLeft,
                       null,
                       true);
                }

                break;

            case ModeEnum.Door:
                _tile.ChangeWallGraphics(
                       CachedAssets.WallSet.anim_DoorHorizontal_Open.GetBottomFirstFrame(),
                       CachedAssets.WallSet.anim_DoorHorizontal_Open.GetTopFirstFrame(),
                       true
                );

                if ((_snapToNeighbours && (_tile.HasConnectable_L && _tile.HasConnectable_R && !_tile.HasConnectable_B && !_tile.HasConnectable_T))
                || (!_snapToNeighbours && (_tile.TempOrientation == Tile.TileOrientation.Left || _tile.TempOrientation == Tile.TileOrientation.Right))) {
                    _tile.ChangeWallGraphics(
                              CachedAssets.WallSet.anim_DoorHorizontal_Open.GetBottomFirstFrame(),
                              CachedAssets.WallSet.anim_DoorHorizontal_Open.GetTopFirstFrame(),
                              true
                    );
                }
                else if ((_snapToNeighbours && (!_tile.HasConnectable_L && !_tile.HasConnectable_R && _tile.HasConnectable_B && _tile.HasConnectable_T))
                     || (!_snapToNeighbours && (_tile.TempOrientation == Tile.TileOrientation.Bottom || _tile.TempOrientation == Tile.TileOrientation.Top))) {
                    _tile.ChangeWallGraphics(
                          CachedAssets.WallSet.anim_DoorVertical_Open.GetBottomFirstFrame(),
                          CachedAssets.WallSet.anim_DoorVertical_Open.GetTopFirstFrame(),
                          true
                    );
                }

                break;
            case ModeEnum.Airlock:
                _tile.ChangeWallGraphics(
                       CachedAssets.WallSet.anim_AirlockHorizontal_OpenTop.Bottom[0],
                       CachedAssets.WallSet.anim_AirlockHorizontal_OpenTop.Top[0],
                       true
                );

                if ((_snapToNeighbours && (_tile.HasConnectable_L && _tile.HasConnectable_R && !_tile.HasConnectable_B && !_tile.HasConnectable_T))
                || (!_snapToNeighbours && (_tile.TempOrientation == Tile.TileOrientation.Left || _tile.TempOrientation == Tile.TileOrientation.Right))) {
                    _tile.ChangeWallGraphics(
                              CachedAssets.WallSet.anim_AirlockHorizontal_OpenTop.GetBottomFirstFrame(),
                              CachedAssets.WallSet.anim_AirlockHorizontal_OpenTop.GetTopFirstFrame(),
                              true
                    );
                }
                else if ((_snapToNeighbours && (!_tile.HasConnectable_L && !_tile.HasConnectable_R && _tile.HasConnectable_B && _tile.HasConnectable_T))
                     || (!_snapToNeighbours && (_tile.TempOrientation == Tile.TileOrientation.Bottom || _tile.TempOrientation == Tile.TileOrientation.Top))) {
                    _tile.ChangeWallGraphics(
                          CachedAssets.WallSet.anim_AirlockVertical_OpenLeft.GetBottomFirstFrame(),
                          CachedAssets.WallSet.anim_AirlockVertical_OpenLeft.GetTopFirstFrame(),
                          true
                    );
                }

                break;
            case ModeEnum.ObjectPlacing:
                throw new System.Exception(Mode.ToString() + " doesn't apply to Wallbuilding!");
            default:
                throw new System.NotImplementedException(Mode.ToString() + " hasn't been properly implemented yet!");
        }

		base.SetGhostGraphics (_tile, _snapToNeighbours);
    }

    List<Tile> neighbours;
    int diffX;
    int diffY;
    bool isHorizontal = false;
    bool isVertical = false;
	protected override bool Evaluate(Tile _tile){
        if (_tile.HasBeenEvaluated)
            return false; // not sure what to return here :s
        _tile.HasBeenEvaluated = true;

		// is building even allowed?
		if (!_tile._BuildingAllowed_) {
			ApplySettingsToGhost(_tile, false, Color_Blocked);
			return false;
		}

		// deleting old tiles
		if (isDeleting) {
            if (_tile._WallType_ == Tile.Type.Empty) { // empty tiles can't be deleted
                ApplySettingsToGhost(_tile, false, Color_Blocked);
                return false;
            }

            // is the tile occupied?
            if (_tile.IsOccupiedByObject) {
				ApplySettingsToGhost(_tile, false, Color_Blocked);
                return false;
            }

            // add ghosts for connected diagonals - but is any of them blocked from doing so?
            if (!AddGhostsForConnectedDiagonals(_tile)) {
                ApplySettingsToGhost(_tile, false, Color_Blocked);
                return false;
            }

            // add ghosts for connected doors and airlocks - but is any of them blocked from doing so?
            if (!AddGhostsForConnectedDoors(_tile)) {
                ApplySettingsToGhost(_tile, false, Color_Blocked);
                return false;
            }

            ApplySettingsToGhost(_tile, true, Color_Remove);
			return true;
		}


		// adding new tiles
		switch (Mode) {
			case ModeEnum.Diagonal:
				// is the tile below already a diagonal of the same orientation?
				if (_tile._WallType_ == Tile.Type.Diagonal && _tile._Orientation_ == _tile.TempOrientation) {
					ApplySettingsToGhost(_tile, false, Color_AlreadyExisting);
                    return false;
                }
                // is the tile below not cleared?
                if (_tile._WallType_ != Tile.Type.Empty || _tile.IsOccupiedByObject) {
					ApplySettingsToGhost(_tile, false, Color_Blocked);
                    return false;
                }

                // does the ghost's orientation match the neighbouring walls below?
                if ((_tile.TempOrientation == Tile.TileOrientation.TopLeft && !(_tile.HasConnectable_L && _tile.HasConnectable_T))
					|| (_tile.TempOrientation == Tile.TileOrientation.TopRight && !(_tile.HasConnectable_T && _tile.HasConnectable_R))
					|| (_tile.TempOrientation == Tile.TileOrientation.BottomRight && !(_tile.HasConnectable_R && _tile.HasConnectable_B))
					|| (_tile.TempOrientation == Tile.TileOrientation.BottomLeft && !(_tile.HasConnectable_B && _tile.HasConnectable_L))) {

					ApplySettingsToGhost(_tile, false, Color_Blocked);
                    return false;
                }
                break;

			case ModeEnum.Door:
			case ModeEnum.Airlock:

				// is the tile... living on the edge? B)
				if (_tile.GridX == 0 || _tile.GridX == Grid.Instance.GridSizeX - 1 || _tile.GridY == 0 || _tile.GridY == Grid.Instance.GridSizeY) {
					ApplySettingsToGhost(_tile, false, Color_Blocked);
                    return false;
                }

                isHorizontal = _tile.TempOrientation == Tile.TileOrientation.Left || _tile.TempOrientation == Tile.TileOrientation.Right;
				isVertical = _tile.TempOrientation == Tile.TileOrientation.Bottom || _tile.TempOrientation == Tile.TileOrientation.Top;

				// does the tile have adjacent walls for the door to be in?
				if (isHorizontal && (!_tile.HasConnectable_L || !_tile.HasConnectable_R)
					|| isVertical && (!_tile.HasConnectable_B || !_tile.HasConnectable_T)) {

					ApplySettingsToGhost(_tile, false, Color_Blocked);
                    return false;
                }

                // does the tile have space for door entrances?
                bool _failed = false;
				neighbours = Grid.Instance.GetNeighbours(_tile.GridX, _tile.GridY);
				for (int j = 0; j < neighbours.Count; j++) {
					diffX = neighbours[j].GridX - _tile.GridX;
					diffY = neighbours[j].GridY - _tile.GridY;

					if (((isHorizontal && (diffX == 0 && diffY != 0)) || (isVertical && (diffX != 0 && diffY == 0))) && neighbours[j]._WallType_ != Tile.Type.Empty) {
						ApplySettingsToGhost(_tile, false, Color_Blocked);
						_failed = true;
						break;
					}
				}
				if (_failed)
                    return false;

                // is there already a door?
                if (_tile._WallType_ == Tile.Type.Door) {
					ApplySettingsToGhost(_tile, false, Color_AlreadyExisting);
                    return false;
                }
                //}
                //else if (_type == Tile.TileType.Empty) { // door entrance should never write to grid
                //    ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_NewWall);
                //    return;
                //}

                // is the tile below not cleared?
                if (_tile.IsOccupiedByObject) {
					ApplySettingsToGhost(_tile, false, Color_Blocked);
                    return false;
                }
                break;

			case ModeEnum.Default:
			case ModeEnum.Room:
			case ModeEnum.Fill:
				// is the tile below already a wall?
				if (_tile._WallType_ == Tile.Type.Solid) {
					ApplySettingsToGhost(_tile, false, Color_AlreadyExisting);
                    return false;
                }
                // is the tile below not cleared?
                if (_tile._WallType_ != Tile.Type.Empty || _tile.IsOccupiedByObject) {
					ApplySettingsToGhost(_tile, false, Color_Blocked);
                    return false;
                }
                break;

            case ModeEnum.ObjectPlacing:
                throw new System.Exception(Mode.ToString() + "doesn't apply to Wallbuilding!");
			default:
				throw new System.NotImplementedException(Mode.ToString() + " hasn't been fully implemented yet!");
		}

		// all's good
		ApplySettingsToGhost(_tile, true, Color_New);
        return true;
	}

    protected override void ApplySettingsToGhost(Tile _tile, bool _applyToGrid, Color _newColor) {
        _tile.SetFloorColor(Color_AlreadyExisting);
        _tile.SetWallColor(_newColor);
        base.ApplySettingsToGhost(_tile, _applyToGrid, _newColor);
    }

    protected override void ApplyCurrentTool() {
		for (int i = 0; i < selectedTiles.Count; i++) {
			//if (selectedTiles [i].TempType == Tile.Type.Door || selectedTiles [i].TempType == Tile.Type.Airlock || selectedTiles [i].TempType == Tile.Type.Diagonal) {
			//	if (selectedTiles [i].TempOrientation == Tile.TileOrientation.None)
			//		Debug.LogError ("A " + selectedTiles[i].TempType + " can't be " + selectedTiles[i].TempOrientation + "!");
			//}

			selectedTiles[i].SetTileType(isDeleting ? Tile.Type.Empty : selectedTiles[i].TempType, isDeleting ? Tile.TileOrientation.None : selectedTiles[i].TempOrientation);
		}

		base.ApplyCurrentTool ();
	}
}