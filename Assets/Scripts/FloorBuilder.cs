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
		if (_tile.ConnectedDiagonalFloor_B != null)
			AddNextGhost(_tile.ConnectedDiagonalFloor_B.GridX, _tile.ConnectedDiagonalFloor_B.GridY, false);
		if (_tile.ConnectedDiagonalFloor_L != null)
			AddNextGhost(_tile.ConnectedDiagonalFloor_L.GridX, _tile.ConnectedDiagonalFloor_L.GridY, false);
		if (_tile.ConnectedDiagonalFloor_T != null)
			AddNextGhost(_tile.ConnectedDiagonalFloor_T.GridX, _tile.ConnectedDiagonalFloor_T.GridY, false);
		if (_tile.ConnectedDiagonalFloor_R != null)
			AddNextGhost(_tile.ConnectedDiagonalFloor_R.GridX, _tile.ConnectedDiagonalFloor_R.GridY, false);
	}

	protected override void SetGhostGraphics(ref GhostInfo _ghost, Tile _tileUnderGhost, bool _snapToNeighbours) {

		bool _hasConnection_Left = _ghost.HasNeighbourGhost_Left || _tileUnderGhost.HasConnectableFloor_L;
		bool _hasConnection_Right = _ghost.HasNeighbourGhost_Right || _tileUnderGhost.HasConnectableFloor_R;
		bool _hasConnection_Top = _ghost.HasNeighbourGhost_Top || _tileUnderGhost.HasConnectableFloor_T;
		bool _hasConnection_Bottom = _ghost.HasNeighbourGhost_Bottom || _tileUnderGhost.HasConnectableFloor_B;

		switch (Mode) {

			case ModeEnum.Default:
			case ModeEnum.Room:
			case ModeEnum.Fill:
				_ghost.Type = Tile.Type.Solid;
				_ghost.Orientation = Tile.TileOrientation.None;

				if (isDeleting && _tileUnderGhost._FloorType_ != Tile.Type.Empty) { // excluding empty because otherwise it doesn't get graphics
					_ghost.ChangeAssets(
						CachedAssets.Instance.GetFloorAssetForTile(_tileUnderGhost._FloorType_, _tileUnderGhost._FloorOrientation_, 0, _hasConnection_Left, _hasConnection_Top, _hasConnection_Right, _hasConnection_Bottom),
						null);
				}
				else {
					_ghost.ChangeAssets(
						CachedAssets.Instance.GetFloorAssetForTile(_ghost.Type, _ghost.Orientation, 0, _hasConnection_Left, _hasConnection_Top, _hasConnection_Right, _hasConnection_Bottom),
						null);
				}
				break;

			case ModeEnum.Diagonal:

				// default values 
				_ghost.Type = Tile.Type.Diagonal;
				_ghost.Orientation = _snapToNeighbours ? Tile.TileOrientation.TopLeft : _ghost.Orientation;
				_ghost.ChangeAssets(
					CachedAssets.WallSet.floor_Diagonal_TopLeft,
					null);


				// diagonal top left
				if ((_snapToNeighbours && _tileUnderGhost.HasConnectableFloor_L && _tileUnderGhost.HasConnectableFloor_T)
					|| (!_snapToNeighbours && ALL_GHOSTS[0].Orientation == Tile.TileOrientation.TopLeft)) {

					_ghost.Orientation = Tile.TileOrientation.TopLeft;
					_ghost.ChangeAssets(
						CachedAssets.WallSet.floor_Diagonal_TopLeft,
						null);
				}

				// diagonal top right
				else if ((_snapToNeighbours && _tileUnderGhost.HasConnectableFloor_T && _tileUnderGhost.HasConnectableFloor_R)
					|| (!_snapToNeighbours && ALL_GHOSTS[0].Orientation == Tile.TileOrientation.TopRight)) {

					_ghost.Orientation = Tile.TileOrientation.TopRight;
					_ghost.ChangeAssets(
						CachedAssets.WallSet.floor_Diagonal_TopRight,
						null);
				}

				// diagonal bottom right
				else if ((_snapToNeighbours && _tileUnderGhost.HasConnectableFloor_R && _tileUnderGhost.HasConnectableFloor_B)
					|| (!_snapToNeighbours && ALL_GHOSTS[0].Orientation == Tile.TileOrientation.BottomRight)) {

					_ghost.Orientation = Tile.TileOrientation.BottomRight;
					_ghost.ChangeAssets(
						CachedAssets.WallSet.floor_Diagonal_BottomRight,
						null);
				}

				// diagonal bottom left
				else if ((_snapToNeighbours && _tileUnderGhost.HasConnectableFloor_B && _tileUnderGhost.HasConnectableFloor_L)
					|| (!_snapToNeighbours && ALL_GHOSTS[0].Orientation == Tile.TileOrientation.BottomLeft)) {

					_ghost.Orientation = Tile.TileOrientation.BottomLeft;
					_ghost.ChangeAssets(
						CachedAssets.WallSet.floor_Diagonal_BottomLeft,
						null);
				}

				break;

			case ModeEnum.Door:
			case ModeEnum.Airlock:
            case ModeEnum.ObjectPlacing:
				throw new System.Exception (Mode.ToString() + " does not apply to Floor!");
			default:
				throw new System.NotImplementedException(Mode.ToString() + " hasn't been properly implemented yet!");
		}

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

//			// is the tile occupied?
//			if (_tileUnderGhost.IsOccupied) {
//				ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_Blocked);
//				return;
//			}

			// all's good - but add connected diagonals to be deleted as well!
			if (_tileUnderGhost._FloorType_ != Tile.Type.Empty)
				AddGhostsForConnectedDiagonals(_tileUnderGhost);

			ApplySettingsToGhost(_ghost, _tileUnderGhost, true, Color_Remove);
			return;
		}


		// adding new tiles

		// is building even allowed?
		if (!_tileUnderGhost._BuildingAllowed_) {
			ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_Blocked);
			return;
		}
		switch (Mode) {
			case ModeEnum.Diagonal:
				// is the tile below not free of walls?
				if (_tileUnderGhost._WallType_ != Tile.Type.Empty && _tileUnderGhost._WallType_ != Tile.Type.Diagonal) {
					ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_Blocked);
					return;
				}

				// is the tile below already a diagonal of the same orientation?
				if (_tileUnderGhost._FloorType_ == Tile.Type.Diagonal && _tileUnderGhost._FloorOrientation_ == _ghost.Orientation) {
					ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_AlreadyExisting);
					return;
				}
				// is the tile below not cleared?
				if (_tileUnderGhost._FloorType_ != Tile.Type.Empty || _tileUnderGhost.IsOccupiedByObject) {
					ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_Blocked);
					return;
				}

				// does the ghost's orientation match the neighbouring walls below?
				if ((_ghost.Orientation == Tile.TileOrientation.TopLeft && !(_tileUnderGhost.HasConnectableFloor_L && _tileUnderGhost.HasConnectableFloor_T))
					|| (_ghost.Orientation == Tile.TileOrientation.TopRight && !(_tileUnderGhost.HasConnectableFloor_T && _tileUnderGhost.HasConnectableFloor_R))
					|| (_ghost.Orientation == Tile.TileOrientation.BottomRight && !(_tileUnderGhost.HasConnectableFloor_R && _tileUnderGhost.HasConnectableFloor_B))
					|| (_ghost.Orientation == Tile.TileOrientation.BottomLeft && !(_tileUnderGhost.HasConnectableFloor_B && _tileUnderGhost.HasConnectableFloor_L))) {

					ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_Blocked);
					return;
				}
				break;
			case ModeEnum.Default:
			case ModeEnum.Room:
			case ModeEnum.Fill:
				// is the tile below covered by some kind of wall?
				if (_tileUnderGhost._WallType_ != Tile.Type.Empty) {
					ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_Blocked);
					return;
				}
				// is the tile below already a solid?
				if (_tileUnderGhost._FloorType_ == Tile.Type.Solid) {
					ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_AlreadyExisting);
					return;
				}
				// is the tile below not cleared?
				if (_tileUnderGhost._FloorType_ != Tile.Type.Empty/* || _tileUnderGhost.IsOccupied*/) {
					ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_Blocked);
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
		ApplySettingsToGhost(_ghost, _tileUnderGhost, true, Color_New);
	}

	protected override void ApplyCurrentTool() {
		for (int i = 0; i < selectedTiles.Count; i++)
			selectedTiles [i].SetFloorType (isDeleting ? Tile.Type.Empty : selectedTilesNewType [i], isDeleting ? Tile.TileOrientation.None : selectedTilesNewOrientation [i]);
		base.ApplyCurrentTool ();
	}
}