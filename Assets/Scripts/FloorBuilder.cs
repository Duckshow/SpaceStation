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

	protected override void AddGhostsForConnectedDiagonals(Tile _tile) {
		if (_tile.ConnectedDiagonalFloor_B != null) {
			AddNextGhost(_tile.ConnectedDiagonalFloor_B.GridX, _tile.ConnectedDiagonalFloor_B.GridY, false);
			AddNextGhostGraphics(_tile.ConnectedDiagonalFloor_B.GridX, _tile.ConnectedDiagonalFloor_B.GridY, false);
		}
		if (_tile.ConnectedDiagonalFloor_L != null) {
			AddNextGhost(_tile.ConnectedDiagonalFloor_L.GridX, _tile.ConnectedDiagonalFloor_L.GridY, false);
			AddNextGhostGraphics(_tile.ConnectedDiagonalFloor_L.GridX, _tile.ConnectedDiagonalFloor_L.GridY, false);
		}
		if (_tile.ConnectedDiagonalFloor_T != null) {
			AddNextGhost(_tile.ConnectedDiagonalFloor_T.GridX, _tile.ConnectedDiagonalFloor_T.GridY, false);
			AddNextGhostGraphics(_tile.ConnectedDiagonalFloor_T.GridX, _tile.ConnectedDiagonalFloor_T.GridY, false);
		}
		if (_tile.ConnectedDiagonalFloor_R != null) {
			AddNextGhost(_tile.ConnectedDiagonalFloor_R.GridX, _tile.ConnectedDiagonalFloor_R.GridY, false);
			AddNextGhostGraphics(_tile.ConnectedDiagonalFloor_R.GridX, _tile.ConnectedDiagonalFloor_R.GridY, false);
		}
	}

	protected override void SetGhostType(Tile _tile, bool _snapToNeighbours){
		switch (Mode) {
			case ModeEnum.Default:
			case ModeEnum.Room:
			case ModeEnum.Fill:
				_tile.TempType = Tile.Type.Solid;
				_tile.TempOrientation = Tile.TileOrientation.None;
				break;

			case ModeEnum.Diagonal:

				// default values 
				_tile.TempType = Tile.Type.Diagonal;
				_tile.TempOrientation = _snapToNeighbours ? Tile.TileOrientation.TopLeft : _tile.TempOrientation;

				// diagonal top left
				if (	(_snapToNeighbours && _tile.HasConnectableFloor_L && _tile.HasConnectableFloor_T)
					|| (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.TopLeft)) {

					_tile.TempOrientation = Tile.TileOrientation.TopLeft;
				}

				// diagonal top right
				else if ((_snapToNeighbours && _tile.HasConnectableFloor_T && _tile.HasConnectableFloor_R)
					 || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.TopRight)) {

					_tile.TempOrientation = Tile.TileOrientation.TopRight;
				}

				// diagonal bottom right
				else if ((_snapToNeighbours && _tile.HasConnectableFloor_R && _tile.HasConnectableFloor_B)
					 || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.BottomRight)) {

					_tile.TempOrientation = Tile.TileOrientation.BottomRight;
				}

				// diagonal bottom left
				else if ((_snapToNeighbours && _tile.HasConnectableFloor_B && _tile.HasConnectableFloor_L)
					 || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.BottomLeft)) {

					_tile.TempOrientation = Tile.TileOrientation.BottomLeft;
				}

				break;

			case ModeEnum.Door:
			case ModeEnum.Airlock:
			case ModeEnum.ObjectPlacing:
				throw new System.Exception (Mode.ToString() + " does not apply to Floor!");
			default:
				throw new System.NotImplementedException(Mode.ToString() + " hasn't been properly implemented yet!");
		}

		base.SetGhostType (_tile, _snapToNeighbours);
	}
	protected override void SetGhostGraphics(Tile _tile, bool _snapToNeighbours) {

		bool _hasConnection_L = (Tile.sTryTempCacheNeighbour_L(_tile.GridX, _tile.GridY) && Tile.sCachedNeighbour_L.TempType != Tile.Type.Empty) || _tile.HasConnectableFloor_L;
		bool _hasConnection_R = (Tile.sTryTempCacheNeighbour_R(_tile.GridX, _tile.GridY) && Tile.sCachedNeighbour_R.TempType != Tile.Type.Empty) || _tile.HasConnectableFloor_R;
		bool _hasConnection_T = (Tile.sTryTempCacheNeighbour_T(_tile.GridX, _tile.GridY) && Tile.sCachedNeighbour_T.TempType != Tile.Type.Empty) || _tile.HasConnectableFloor_T;
		bool _hasConnection_B = (Tile.sTryTempCacheNeighbour_B(_tile.GridX, _tile.GridY) && Tile.sCachedNeighbour_B.TempType != Tile.Type.Empty) || _tile.HasConnectableFloor_B;

		switch (Mode) {
			case ModeEnum.Default:
			case ModeEnum.Room:
			case ModeEnum.Fill:
				//if (isDeleting && _tile._FloorType_ != Tile.Type.Empty) { // excluding empty because otherwise it doesn't get graphics
				//	//_ghost.ChangeAssets(
				//	//	CachedAssets.Instance.GetFloorAssetForTile(_tile._FloorType_, _tile._FloorOrientation_, 0, _hasConnection_Left, _hasConnection_Top, _hasConnection_Right, _hasConnection_Bottom),
				//	//	null);
				//}
				if(!isDeleting) { 
					_tile.ChangeFloorGraphics(
						CachedAssets.Instance.GetFloorAssetForTile(_tile.TempType, _tile.TempOrientation, 0, _hasConnection_L, _hasConnection_T, _hasConnection_R, _hasConnection_B),
                        true);
				} 
                // else if deleting, don't change the graphics (use the current)
				break;

			case ModeEnum.Diagonal:

				// default values 
				_tile.ChangeFloorGraphics(
					CachedAssets.WallSet.floor_Diagonal_TopLeft,
					true);

				// diagonal top left
				if 	((_snapToNeighbours && _tile.HasConnectableFloor_L && _tile.HasConnectableFloor_T)
				 || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.TopLeft)) {
					_tile.ChangeFloorGraphics(
						CachedAssets.WallSet.floor_Diagonal_TopLeft,
						true);
				}

				// diagonal top right
				else if ((_snapToNeighbours && _tile.HasConnectableFloor_T && _tile.HasConnectableFloor_R)
					 || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.TopRight)) {
					_tile.ChangeFloorGraphics(
						CachedAssets.WallSet.floor_Diagonal_TopRight,
						true);
				}

				// diagonal bottom right
				else if ((_snapToNeighbours && _tile.HasConnectableFloor_R && _tile.HasConnectableFloor_B)
					 || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.BottomRight)) {
					_tile.ChangeFloorGraphics(
						CachedAssets.WallSet.floor_Diagonal_BottomRight,
						true);
				}

				// diagonal bottom left
				else if ((_snapToNeighbours && _tile.HasConnectableFloor_B && _tile.HasConnectableFloor_L)
					 || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.BottomLeft)) {
					_tile.ChangeFloorGraphics(
						CachedAssets.WallSet.floor_Diagonal_BottomLeft,
						true);
				}

				break;

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
	protected override void Evaluate(Tile _tile){

		// deleting old tiles
		if (isDeleting) {
			// is building even allowed?
			if (!_tile._BuildingAllowed_ && _tile._FloorType_ != Tile.Type.Empty) { // empty tiles allowed for deletion bc it looks better
				ApplySettingsToGhost(_tile, false, Color_Blocked);
				return;
			}

//			// is the tile occupied?
//			if (_tileUnderGhost.IsOccupied) {
//				ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_Blocked);
//				return;
//			}

			// all's good - but add connected diagonals to be deleted as well!
			if (_tile._FloorType_ != Tile.Type.Empty)
				AddGhostsForConnectedDiagonals(_tile);

			ApplySettingsToGhost(_tile, true, Color_Remove);
			return;
		}


		// adding new tiles

		// is building even allowed?
		if (!_tile._BuildingAllowed_) {
			ApplySettingsToGhost(_tile, false, Color_Blocked);
			return;
		}
		switch (Mode) {
			case ModeEnum.Diagonal:
				// is the tile below not free of walls?
				if (_tile._WallType_ != Tile.Type.Empty && _tile._WallType_ != Tile.Type.Diagonal) {
					ApplySettingsToGhost(_tile, false, Color_Blocked);
					return;
				}

				// is the tile below already a diagonal of the same orientation?
				if (_tile._FloorType_ == Tile.Type.Diagonal && _tile._FloorOrientation_ == _tile.TempOrientation) {
					ApplySettingsToGhost(_tile, false, Color_AlreadyExisting);
					return;
				}
				// is the tile below not cleared?
				if (_tile._FloorType_ != Tile.Type.Empty || _tile.IsOccupiedByObject) {
					ApplySettingsToGhost(_tile, false, Color_Blocked);
					return;
				}

				// does the ghost's orientation match the neighbouring walls below?
				if (   (_tile.TempOrientation == Tile.TileOrientation.TopLeft && !(_tile.HasConnectableFloor_L && _tile.HasConnectableFloor_T))
					|| (_tile.TempOrientation == Tile.TileOrientation.TopRight && !(_tile.HasConnectableFloor_T && _tile.HasConnectableFloor_R))
					|| (_tile.TempOrientation == Tile.TileOrientation.BottomRight && !(_tile.HasConnectableFloor_R && _tile.HasConnectableFloor_B))
					|| (_tile.TempOrientation == Tile.TileOrientation.BottomLeft && !(_tile.HasConnectableFloor_B && _tile.HasConnectableFloor_L))) {

					ApplySettingsToGhost(_tile, false, Color_Blocked);
					return;
				}
				break;
			case ModeEnum.Default:
			case ModeEnum.Room:
			case ModeEnum.Fill:
				// is the tile below covered by some kind of wall?
				if (_tile._WallType_ != Tile.Type.Empty) {
					ApplySettingsToGhost(_tile, false, Color_Blocked);
					return;
				}
				// is the tile below already a solid?
				if (_tile._FloorType_ == Tile.Type.Solid) {
					ApplySettingsToGhost(_tile, false, Color_AlreadyExisting);
					return;
				}
				// is the tile below not cleared?
				if (_tile._FloorType_ != Tile.Type.Empty/* || _tileUnderGhost.IsOccupied*/) {
					ApplySettingsToGhost(_tile, false, Color_Blocked);
					return;
				}
				break;
			case ModeEnum.Door:
			case ModeEnum.Airlock:
            case ModeEnum.ObjectPlacing:
				throw new System.Exception (Mode.ToString() + " is not applicable to Floor!");

			default:
				throw new System.NotImplementedException(Mode.ToString() + " hasn't been fully implemented yet!");
		}

		// all's good
		ApplySettingsToGhost(_tile, true, Color_New);
	}

	protected override void ApplyCurrentTool() {
		for (int i = 0; i < selectedTiles.Count; i++)
			selectedTiles [i].SetFloorType (isDeleting ? Tile.Type.Empty : selectedTiles[i].TempType, isDeleting ? Tile.TileOrientation.None : selectedTiles[i].TempOrientation);
		base.ApplyCurrentTool ();
	}
}