using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class FloorBuilder : BuilderBase {

	protected override void TryChangeMode(){
		base.TryChangeMode ();

		if (Input.GetKey(KeyCode.Alpha1))
			Mode = ModeEnum.Fill;
		if (Input.GetKey(KeyCode.Alpha2))
			Mode = ModeEnum.Diagonal;
	}

	protected override void ResetModifiedTiles(bool _includingMouse = false) {
		for (int i = 0; i < modifiedTiles.Count; i++) {
			modifiedTiles[i].SetFloorType(Tile.Type.Empty, Tile.TileOrientation.None, _temporarily: true);
            modifiedTiles[i].ResetTempSettingsFloor();
            modifiedTiles[i].ResetFloorColor();
            modifiedTiles[i].ResetWallColor();
        }

        base.ResetModifiedTiles ();
	}
	protected override void ResetSelectedTiles() {
		for (int i = 0; i < selectedTiles.Count; i++) {
			selectedTiles[i].SetFloorType(Tile.Type.Empty, selectedTiles[i].TempOrientation, _temporarily: true);
            selectedTiles[i].ResetTempSettingsFloor();
			selectedTiles[i].ResetFloorColor();
            selectedTiles[i].ResetWallColor();
        }

        base.ResetSelectedTiles ();
	}

 //   private static bool neighboursPassedEval = true;
 //   protected override bool AddGhostsForConnectedDiagonals(Tile _tile) {
 //       neighboursPassedEval = true;

	//	if (_tile.ConnectedDiagonalFloor_B != null) {
	//		AddNextGhost(_tile.ConnectedDiagonalFloor_B.GridX, _tile.ConnectedDiagonalFloor_B.GridY, DetermineGhostType(_tile.ConnectedDiagonalFloor_B), DetermineGhostOrientation(_tile.ConnectedDiagonalFloor_B, false), false);
 //           SetGhostType(_tile.ConnectedDiagonalFloor_B);
 //           SetGhostGraphics(_tile.ConnectedDiagonalFloor_B, false);
 //           if (!Evaluate(_tile.ConnectedDiagonalFloor_B))
 //               neighboursPassedEval = false;
 //       }
	//	if (_tile.ConnectedDiagonalFloor_L != null) {
 //           AddNextGhost(_tile.ConnectedDiagonalFloor_L.GridX, _tile.ConnectedDiagonalFloor_L.GridY, DetermineGhostType(_tile.ConnectedDiagonalFloor_L), DetermineGhostOrientation(_tile.ConnectedDiagonalFloor_L, false), false);
 //           SetGhostType(_tile.ConnectedDiagonalFloor_L);
 //           SetGhostGraphics(_tile.ConnectedDiagonalFloor_L, false);
 //           if (!Evaluate(_tile.ConnectedDiagonalFloor_L))
 //               neighboursPassedEval = false;
 //       }
	//	if (_tile.ConnectedDiagonalFloor_T != null) {
 //           AddNextGhost(_tile.ConnectedDiagonalFloor_T.GridX, _tile.ConnectedDiagonalFloor_T.GridY, DetermineGhostType(_tile.ConnectedDiagonalFloor_T), DetermineGhostOrientation(_tile.ConnectedDiagonalFloor_T, false), false);
 //           SetGhostType(_tile.ConnectedDiagonalFloor_T);
 //           SetGhostGraphics(_tile.ConnectedDiagonalFloor_T, false);
 //           if (!Evaluate(_tile.ConnectedDiagonalFloor_T))
 //               neighboursPassedEval = false;
 //       }
	//	if (_tile.ConnectedDiagonalFloor_R != null) {
 //           AddNextGhost(_tile.ConnectedDiagonalFloor_R.GridX, _tile.ConnectedDiagonalFloor_R.GridY, DetermineGhostType(_tile.ConnectedDiagonalFloor_R), DetermineGhostOrientation(_tile.ConnectedDiagonalFloor_R, false), false);
 //           SetGhostType(_tile.ConnectedDiagonalFloor_R);
 //           SetGhostGraphics(_tile.ConnectedDiagonalFloor_R, false);
 //           if (!Evaluate(_tile.ConnectedDiagonalFloor_R))
 //               neighboursPassedEval = false;
 //       }

 //       return neighboursPassedEval;
	//}

	protected override Tile.Type DetermineGhostType(Tile _tile){
		switch (Mode) {
			case ModeEnum.Default:
			case ModeEnum.Room:
			case ModeEnum.Fill:
                if (_tile._WallType_ == Tile.Type.Diagonal) // because forcing this to be diagonal
                    return Tile.Type.Diagonal;
                else if (isDeleting)
                    return Tile.Type.Empty;
                else
                    return Tile.Type.Solid;
			case ModeEnum.Diagonal:
                return Tile.Type.Diagonal;
			case ModeEnum.Door:
			case ModeEnum.Airlock:
			case ModeEnum.ObjectPlacing:
				throw new System.Exception (Mode.ToString() + " does not apply to Floor!");
			default:
				throw new System.NotImplementedException(Mode.ToString() + " hasn't been properly implemented yet!");
		}
	}
    protected override Tile.TileOrientation DetermineGhostOrientation(Tile _tile, bool _snapToNeighbours) {
        switch (Mode) {
            case ModeEnum.Default:
            case ModeEnum.Room:
            case ModeEnum.Fill:
            case ModeEnum.Door:
            case ModeEnum.Airlock:
            case ModeEnum.ObjectPlacing:
                if (_tile._WallType_ == Tile.Type.Diagonal) // because forcing this to be diagonal
                    return Tile.GetReverseDirection(_tile._Orientation_);

                return Tile.TileOrientation.None;
            case ModeEnum.Diagonal:
                if (_tile._WallType_ == Tile.Type.Diagonal)
                    return Tile.GetReverseDirection(_tile._Orientation_);

                // diagonal top left
                if ((_snapToNeighbours && _tile.HasConnectableFloor_L && _tile.HasConnectableFloor_T) || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.TopLeft))
                    return Tile.TileOrientation.TopLeft;
                // diagonal top right
                else if ((_snapToNeighbours && _tile.HasConnectableFloor_T && _tile.HasConnectableFloor_R) || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.TopRight))
                    return Tile.TileOrientation.TopRight;
                // diagonal bottom right
                else if ((_snapToNeighbours && _tile.HasConnectableFloor_R && _tile.HasConnectableFloor_B) || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.BottomRight))
                    return Tile.TileOrientation.BottomRight;
                // diagonal bottom left
                else if ((_snapToNeighbours && _tile.HasConnectableFloor_B && _tile.HasConnectableFloor_L) || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.BottomLeft))
                    return Tile.TileOrientation.BottomLeft;
                else
                    return /*_snapToNeighbours ? Tile.TileOrientation.TopLeft : */_tile.TempOrientation;
            default:
                throw new System.NotImplementedException(Mode.ToString() + " hasn't been properly implemented yet!");
        }
    }
    protected override void SetGhostGraphics(Tile _tile, bool _snapToNeighbours) {

		//bool _hasConnection_L = (Tile.sTryTempCacheNeighbour_L(_tile.GridX, _tile.GridY) && Tile.sCachedNeighbour_L.TempType != Tile.Type.Empty && Tile.sCachedNeighbour_L.TempType != Tile.sCachedNeighbour_L._FloorType_);
		//bool _hasConnection_T = (Tile.sTryTempCacheNeighbour_T(_tile.GridX, _tile.GridY) && Tile.sCachedNeighbour_T.TempType != Tile.Type.Empty && Tile.sCachedNeighbour_T.TempType != Tile.sCachedNeighbour_T._FloorType_);
		//bool _hasConnection_R = (Tile.sTryTempCacheNeighbour_R(_tile.GridX, _tile.GridY) && Tile.sCachedNeighbour_R.TempType != Tile.Type.Empty && Tile.sCachedNeighbour_R.TempType != Tile.sCachedNeighbour_R._FloorType_);
		//bool _hasConnection_B = (Tile.sTryTempCacheNeighbour_B(_tile.GridX, _tile.GridY) && Tile.sCachedNeighbour_B.TempType != Tile.Type.Empty && Tile.sCachedNeighbour_B.TempType != Tile.sCachedNeighbour_B._FloorType_);

		switch (Mode) {
			case ModeEnum.Default:
			case ModeEnum.Room:
			case ModeEnum.Fill:
            case ModeEnum.Diagonal:
				//if (isDeleting && _tile._FloorType_ != Tile.Type.Empty) { // excluding empty because otherwise it doesn't get graphics
				//	//_ghost.ChangeAssets(
				//	//	CachedAssets.Instance.GetFloorAssetForTile(_tile._FloorType_, _tile._FloorOrientation_, 0, _hasConnection_Left, _hasConnection_Top, _hasConnection_Right, _hasConnection_Bottom),
				//	//	null);
				//}
				if(!isDeleting && (_tile._FloorType_ != _tile.TempType || _tile._FloorOrientation_ != _tile.TempOrientation)) {
					_tile.ChangeFloorGraphics(
						CachedAssets.Instance.GetFloorAssetForTile(_tile.TempType, _tile.TempOrientation, 0, _tile.HasConnectableTempFloor_L, _tile.HasConnectableTempFloor_T, _tile.HasConnectableTempFloor_R, _tile.HasConnectableTempFloor_B),
                        true);
				} 
                // else if deleting, don't change the graphics (use the current)
				break;

			//case ModeEnum.Diagonal:

			//	// default values 
			//	_tile.ChangeFloorGraphics(
			//		CachedAssets.WallSet.floor_Diagonal_TopLeft,
			//		true);

			//	// diagonal top left
			//	if 	((_snapToNeighbours && _tile.HasConnectableFloor_L && _tile.HasConnectableFloor_T)
			//	 || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.TopLeft)) {
			//		_tile.ChangeFloorGraphics(
			//			CachedAssets.WallSet.floor_Diagonal_TopLeft,
			//			true);
			//	}

			//	// diagonal top right
			//	else if ((_snapToNeighbours && _tile.HasConnectableFloor_T && _tile.HasConnectableFloor_R)
			//		 || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.TopRight)) {
			//		_tile.ChangeFloorGraphics(
			//			CachedAssets.WallSet.floor_Diagonal_TopRight,
			//			true);
			//	}

			//	// diagonal bottom right
			//	else if ((_snapToNeighbours && _tile.HasConnectableFloor_R && _tile.HasConnectableFloor_B)
			//		 || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.BottomRight)) {
			//		_tile.ChangeFloorGraphics(
			//			CachedAssets.WallSet.floor_Diagonal_BottomRight,
			//			true);
			//	}

			//	// diagonal bottom left
			//	else if ((_snapToNeighbours && _tile.HasConnectableFloor_B && _tile.HasConnectableFloor_L)
			//		 || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.BottomLeft)) {
			//		_tile.ChangeFloorGraphics(
			//			CachedAssets.WallSet.floor_Diagonal_BottomLeft,
			//			true);
			//	}

			//	break;

			case ModeEnum.Door:
			case ModeEnum.Airlock:
            case ModeEnum.ObjectPlacing:
				throw new System.Exception (Mode.ToString() + " does not apply to Floor!");
			default:
				throw new System.NotImplementedException(Mode.ToString() + " hasn't been properly implemented yet!");
		}

		base.SetGhostGraphics (_tile, _snapToNeighbours);
	}

	List<Tile> neighbours;
	int diffX;
	int diffY;
	protected override bool Evaluate(Tile _tile){
        //if (_tile.HasBeenEvaluated)
        //    return false; // not sure what to return here :s
        //_tile.HasBeenEvaluated = true;

		// is building even allowed?
		if (!_tile._BuildingAllowed_) {
			ApplySettingsToGhost(_tile, false, ColorIndex_Blocked);
            return false;
		}

        // deleting old tiles
        if (isDeleting) {
            if (_tile._FloorType_ == Tile.Type.Empty) { // can't delete empty tiles
                ApplySettingsToGhost(_tile, false, ColorIndex_AlreadyExisting);
                return false;
            }

            //			// is the tile occupied?
            //			if (_tileUnderGhost.IsOccupied) {
            //				ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_Blocked);
            //				return;
            //			}


            // add ghosts for connected diagonals - but is any of them blocked from doing so?
            //if (!AddGhostsForConnectedDiagonals(_tile)) {
            //    ApplySettingsToGhost(_tile, false, Color_Blocked);
            //    return false;
            //}

			ApplySettingsToGhost(_tile, true, ColorIndex_Remove);
			return true;
		}

        // is the tile below not free of walls?
        if (_tile._WallType_ != Tile.Type.Empty && _tile._WallType_ != Tile.Type.Diagonal || _tile._Orientation_ != Tile.GetReverseDirection(_tile.TempOrientation)) {
            ApplySettingsToGhost(_tile, false, ColorIndex_Blocked);
            return false;
        }

        //// adding new tiles
        //switch (Mode) {
        //	case ModeEnum.Diagonal:
        //		// is the tile below not free of walls?
        //		if (_tile._WallType_ != Tile.Type.Empty && _tile._WallType_ != Tile.Type.Diagonal) {
        //			ApplySettingsToGhost(_tile, false, Color_Blocked);
        //                  return false;
        //		}

        //		// is the tile below already a diagonal of the same orientation?
        //		if (_tile._FloorType_ == Tile.Type.Diagonal && _tile._FloorOrientation_ == _tile.TempOrientation) {
        //			ApplySettingsToGhost(_tile, false, Color_AlreadyExisting);
        //                  return false;
        //		}
        //		// is the tile below not cleared?
        //		if (_tile._FloorType_ != Tile.Type.Empty || _tile.IsOccupiedByObject) {
        //			ApplySettingsToGhost(_tile, false, Color_Blocked);
        //                  return false;
        //		}

        //		// does the ghost's orientation match the neighbouring walls below?
        //		if (   (_tile.TempOrientation == Tile.TileOrientation.TopLeft && !(_tile.HasConnectableFloor_L && _tile.HasConnectableFloor_T))
        //			|| (_tile.TempOrientation == Tile.TileOrientation.TopRight && !(_tile.HasConnectableFloor_T && _tile.HasConnectableFloor_R))
        //			|| (_tile.TempOrientation == Tile.TileOrientation.BottomRight && !(_tile.HasConnectableFloor_R && _tile.HasConnectableFloor_B))
        //			|| (_tile.TempOrientation == Tile.TileOrientation.BottomLeft && !(_tile.HasConnectableFloor_B && _tile.HasConnectableFloor_L))) {

        //			ApplySettingsToGhost(_tile, false, Color_Blocked);
        //                  return false;
        //		}
        //		break;
        //	case ModeEnum.Default:
        //	case ModeEnum.Room:
        //	case ModeEnum.Fill:
        //		// is the tile below covered by some kind of wall?
        //		if (_tile._WallType_ != Tile.Type.Empty) {
        //			ApplySettingsToGhost(_tile, false, Color_Blocked);
        //                  return false;
        //		}
        //		// is the tile below already a solid?
        //		if (_tile._FloorType_ == Tile.Type.Solid) {
        //			ApplySettingsToGhost(_tile, false, Color_AlreadyExisting);
        //                  return false;
        //		}
        //		// is the tile below not cleared?
        //		if (_tile._FloorType_ != Tile.Type.Empty/* || _tileUnderGhost.IsOccupied*/) {
        //			ApplySettingsToGhost(_tile, false, Color_Blocked);
        //                  return false;
        //		}
        //		break;
        //	case ModeEnum.Door:
        //	case ModeEnum.Airlock:
        //          case ModeEnum.ObjectPlacing:
        //		throw new System.Exception (Mode.ToString() + " is not applicable to Floor!");

        //	default:
        //		throw new System.NotImplementedException(Mode.ToString() + " hasn't been fully implemented yet!");
        //}

        // is there already an identical floor in place?
        if (_tile._FloorType_ == _tile.TempType && _tile._FloorOrientation_ == _tile.TempOrientation) {
            ApplySettingsToGhost(_tile, false, ColorIndex_AlreadyExisting);
            return false;
        }

        // all's good
        ApplySettingsToGhost(_tile, true, ColorIndex_New);
        return true;
    }

    protected override void ApplySettingsToGhost(Tile _tile, bool _applyToGrid, byte _newColorIndex) {
        _tile.SetFloorColor(_newColorIndex, true);
        _tile.SetWallColor(ColorIndex_AlreadyExisting, true);
        base.ApplySettingsToGhost(_tile, _applyToGrid, _newColorIndex);
    }

    protected override void ApplyCurrentTool() {
		for (int i = 0; i < selectedTiles.Count; i++)
			selectedTiles [i].SetFloorType (isDeleting ? Tile.Type.Empty : selectedTiles[i].TempType, isDeleting ? Tile.TileOrientation.None : selectedTiles[i].TempOrientation);
		base.ApplyCurrentTool ();
	}
}