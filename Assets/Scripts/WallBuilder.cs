using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class WallBuilder : BuilderBase {

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

    protected override void AddGhostsForConnectedDiagonals(Tile _tile) {
        if (_tile.ConnectedDiagonal_B != null)
            AddNextGhost(_tile.ConnectedDiagonal_B.GridX, _tile.ConnectedDiagonal_B.GridY, false);
        if (_tile.ConnectedDiagonal_L != null)
            AddNextGhost(_tile.ConnectedDiagonal_L.GridX, _tile.ConnectedDiagonal_L.GridY, false);
        if (_tile.ConnectedDiagonal_T != null)
            AddNextGhost(_tile.ConnectedDiagonal_T.GridX, _tile.ConnectedDiagonal_T.GridY, false);
        if (_tile.ConnectedDiagonal_R != null)
            AddNextGhost(_tile.ConnectedDiagonal_R.GridX, _tile.ConnectedDiagonal_R.GridY, false);
    }
	protected override void AddGhostsForConnectedDoors(Tile _tile) {
        if (_tile.ConnectedDoorOrAirlock_B != null)
            AddNextGhost(_tile.ConnectedDoorOrAirlock_B.GridX, _tile.ConnectedDoorOrAirlock_B.GridY, false);
        if (_tile.ConnectedDoorOrAirlock_L != null)
            AddNextGhost(_tile.ConnectedDoorOrAirlock_L.GridX, _tile.ConnectedDoorOrAirlock_L.GridY, false);
        if (_tile.ConnectedDoorOrAirlock_R != null)
            AddNextGhost(_tile.ConnectedDoorOrAirlock_R.GridX, _tile.ConnectedDoorOrAirlock_R.GridY, false);
        if (_tile.ConnectedDoorOrAirlock_T != null)
            AddNextGhost(_tile.ConnectedDoorOrAirlock_T.GridX, _tile.ConnectedDoorOrAirlock_T.GridY, false);
    }

    protected override void SetGhostGraphics(ref GhostInfo _ghost, Tile _tileUnderGhost, bool _snapToNeighbours) {

		bool _hasConnection_L = _ghost.HasNeighbourGhost_Left || _tileUnderGhost.HasConnectable_L;
		bool _hasConnection_R = _ghost.HasNeighbourGhost_Right || _tileUnderGhost.HasConnectable_R;
		bool _hasConnection_T = _ghost.HasNeighbourGhost_Top || _tileUnderGhost.HasConnectable_T;
		bool _hasConnection_B = _ghost.HasNeighbourGhost_Bottom || _tileUnderGhost.HasConnectable_B;

        switch (Mode) {

            case ModeEnum.Default:
            case ModeEnum.Room:
			case ModeEnum.Fill:
                _ghost.Type = Tile.Type.Solid;
                _ghost.Orientation = Tile.TileOrientation.None;

                if (isDeleting && _tileUnderGhost._WallType_ != Tile.Type.Empty) { // excluding empty because otherwise it doesn't get graphics
                    _ghost.ChangeAssets(
						CachedAssets.Instance.GetWallAssetForTile(_tileUnderGhost._WallType_, _tileUnderGhost._Orientation_, 0, true, _hasConnection_L, _hasConnection_T, _hasConnection_R, _hasConnection_B),
						CachedAssets.Instance.GetWallAssetForTile(_tileUnderGhost._WallType_, _tileUnderGhost._Orientation_, 0, false, _hasConnection_L, _hasConnection_T, _hasConnection_R, _hasConnection_B));
                }
                else {
                    _ghost.ChangeAssets(
						CachedAssets.Instance.GetWallAssetForTile(_ghost.Type, _ghost.Orientation, 0, true, _hasConnection_L, _hasConnection_T, _hasConnection_R, _hasConnection_B),
						CachedAssets.Instance.GetWallAssetForTile(_ghost.Type, _ghost.Orientation, 0, false, _hasConnection_L, _hasConnection_T, _hasConnection_R, _hasConnection_B));
                }
                break;

            case ModeEnum.Diagonal:

                // default values 
                _ghost.Type = Tile.Type.Diagonal;
                _ghost.Orientation = _snapToNeighbours ? Tile.TileOrientation.TopLeft : _ghost.Orientation;
                _ghost.ChangeAssets(
                   CachedAssets.WallSet.wall_Diagonal_TopLeft,
                   null);

                //_ghost.SetSprites(CachedAssets.Instance.WallSets[0].Diagonal_TopLeft.Diffuse, null);

                // diagonal top left
                if ((_snapToNeighbours && _tileUnderGhost.HasConnectable_L && _tileUnderGhost.HasConnectable_T)
                || (!_snapToNeighbours && ALL_GHOSTS[0].Orientation == Tile.TileOrientation.TopLeft)) {

                    _ghost.Orientation = Tile.TileOrientation.TopLeft;
                    _ghost.ChangeAssets(
                         CachedAssets.WallSet.wall_Diagonal_TopLeft,
                         null);
                    //_ghost.SetSprites(CachedAssets.Instance.WallSets[0].Diagonal_TopLeft.Diffuse, null);
                }

                // diagonal top right
                else if ((_snapToNeighbours && _tileUnderGhost.HasConnectable_T && _tileUnderGhost.HasConnectable_R)
                     || (!_snapToNeighbours && ALL_GHOSTS[0].Orientation == Tile.TileOrientation.TopRight)) {

                    _ghost.Orientation = Tile.TileOrientation.TopRight;
                    _ghost.ChangeAssets(
                        CachedAssets.WallSet.wall_Diagonal_TopRight,
                        null);
                    //_ghost.SetSprites(CachedAssets.Instance.WallSets[0].Diagonal_TopRight.Diffuse, null);
                }

                // diagonal bottom right
                else if ((_snapToNeighbours && _tileUnderGhost.HasConnectable_R && _tileUnderGhost.HasConnectable_B)
                     || (!_snapToNeighbours && ALL_GHOSTS[0].Orientation == Tile.TileOrientation.BottomRight)) {

                    _ghost.Orientation = Tile.TileOrientation.BottomRight;
                    _ghost.ChangeAssets(
                        CachedAssets.WallSet.wall_Diagonal_BottomRight,
                        null);
                    //_ghost.SetSprites(CachedAssets.Instance.WallSets[0].Diagonal_BottomRight.Diffuse, null);
                }

                // diagonal bottom left
                else if ((_snapToNeighbours && _tileUnderGhost.HasConnectable_B && _tileUnderGhost.HasConnectable_L)
                     || (!_snapToNeighbours && ALL_GHOSTS[0].Orientation == Tile.TileOrientation.BottomLeft)) {

                    _ghost.Orientation = Tile.TileOrientation.BottomLeft;
                    _ghost.ChangeAssets(
                       CachedAssets.WallSet.wall_Diagonal_BottomLeft,
                       null);
                    //_ghost.SetSprites(CachedAssets.Instance.WallSets[0].Diagonal_BottomLeft.Diffuse, null);
                }

                break;

            case ModeEnum.Door:

                //if (_ghost.Type == Tile.TileType.Door) {
                _ghost.Type = Tile.Type.Door;
                _ghost.Orientation = _snapToNeighbours ? Tile.TileOrientation.Left : _ghost.Orientation;
                _ghost.ChangeAssets(
                       CachedAssets.WallSet.anim_DoorHorizontal_Open.Bottom[0],
                       CachedAssets.WallSet.anim_DoorHorizontal_Open.Top[0]);

                if ((_snapToNeighbours && (_tileUnderGhost.HasConnectable_L && _tileUnderGhost.HasConnectable_R && !_tileUnderGhost.HasConnectable_B && !_tileUnderGhost.HasConnectable_T))
                || (!_snapToNeighbours && (ALL_GHOSTS[0].Orientation == Tile.TileOrientation.Left || ALL_GHOSTS[0].Orientation == Tile.TileOrientation.Right))) {
                    _ghost.Orientation = Tile.TileOrientation.Left; // left or right shouldn't matter...
                    _ghost.ChangeAssets(
                              CachedAssets.WallSet.anim_DoorHorizontal_Open.GetBottomFirstFrame(),
                              CachedAssets.WallSet.anim_DoorHorizontal_Open.GetTopFirstFrame());
                }
                else if ((_snapToNeighbours && (!_tileUnderGhost.HasConnectable_L && !_tileUnderGhost.HasConnectable_R && _tileUnderGhost.HasConnectable_B && _tileUnderGhost.HasConnectable_T))
                     || (!_snapToNeighbours && (ALL_GHOSTS[0].Orientation == Tile.TileOrientation.Bottom || ALL_GHOSTS[0].Orientation == Tile.TileOrientation.Top))) {
                    _ghost.Orientation = Tile.TileOrientation.Bottom; // bottom or top shouldn't matter...
                    _ghost.ChangeAssets(
                          CachedAssets.WallSet.anim_DoorVertical_Open.GetBottomFirstFrame(),
                          CachedAssets.WallSet.anim_DoorVertical_Open.GetTopFirstFrame());
                }

                break;
            case ModeEnum.Airlock:

                _ghost.Type = Tile.Type.Airlock;
                _ghost.Orientation = _snapToNeighbours ? Tile.TileOrientation.Left : _ghost.Orientation;
                _ghost.ChangeAssets(
                       CachedAssets.WallSet.anim_AirlockHorizontal_OpenTop.Bottom[0],
                       CachedAssets.WallSet.anim_AirlockHorizontal_OpenTop.Top[0]);

                if ((_snapToNeighbours && (_tileUnderGhost.HasConnectable_L && _tileUnderGhost.HasConnectable_R && !_tileUnderGhost.HasConnectable_B && !_tileUnderGhost.HasConnectable_T))
                || (!_snapToNeighbours && (ALL_GHOSTS[0].Orientation == Tile.TileOrientation.Left || ALL_GHOSTS[0].Orientation == Tile.TileOrientation.Right))) {
                    _ghost.Orientation = Tile.TileOrientation.Left; // left or right shouldn't matter...
                    _ghost.ChangeAssets(
                              CachedAssets.WallSet.anim_AirlockHorizontal_OpenTop.GetBottomFirstFrame(),
                              CachedAssets.WallSet.anim_AirlockHorizontal_OpenTop.GetTopFirstFrame());
                }
                else if ((_snapToNeighbours && (!_tileUnderGhost.HasConnectable_L && !_tileUnderGhost.HasConnectable_R && _tileUnderGhost.HasConnectable_B && _tileUnderGhost.HasConnectable_T))
                     || (!_snapToNeighbours && (ALL_GHOSTS[0].Orientation == Tile.TileOrientation.Bottom || ALL_GHOSTS[0].Orientation == Tile.TileOrientation.Top))) {
                    _ghost.Orientation = Tile.TileOrientation.Bottom; // bottom or top shouldn't matter...
                    _ghost.ChangeAssets(
                          CachedAssets.WallSet.anim_AirlockVertical_OpenLeft.GetBottomFirstFrame(),
                          CachedAssets.WallSet.anim_AirlockVertical_OpenLeft.GetTopFirstFrame());
                }

                break;

            default:
                throw new System.NotImplementedException(Mode.ToString() + " hasn't been properly implemented yet!");
        }

		base.SetGhostGraphics (ref _ghost, _tileUnderGhost, _snapToNeighbours);
    }

    List<Tile> neighbours;
    int diffX;
    int diffY;
    bool isHorizontal = false;
    bool isVertical = false;
	protected override void Evaluate(GhostInfo _ghost, Tile _tileUnderGhost, Tile.TileOrientation _orientation){

		// deleting old tiles
		if (isDeleting) {
			// is building even allowed?
			if (!_tileUnderGhost._BuildingAllowed_ && _tileUnderGhost._WallType_ != Tile.Type.Empty) { // empty tiles allowed for deletion bc it looks better
				ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_Blocked);
				return;
			}

			// is the tile occupied?
			if (_tileUnderGhost.IsOccupied) {
				ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_Blocked);
				return;
			}

			// all's good - but add connected diagonals and doors to be deleted as well!
			if (_tileUnderGhost._WallType_ != Tile.Type.Empty) {
				AddGhostsForConnectedDiagonals(_tileUnderGhost);
				AddGhostsForConnectedDoors(_tileUnderGhost);
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
		switch (Mode) {
			case ModeEnum.Diagonal:
				// is the tile below already a diagonal of the same orientation?
				if (_tileUnderGhost._WallType_ == Tile.Type.Diagonal && _tileUnderGhost._Orientation_ == _ghost.Orientation) {
					ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_AlreadyExisting);
					return;
				}
				// is the tile below not cleared?
				if (_tileUnderGhost._WallType_ != Tile.Type.Empty || _tileUnderGhost.IsOccupied) {
					ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_Blocked);
					return;
				}

				// does the ghost's orientation match the neighbouring walls below?
				if ((_ghost.Orientation == Tile.TileOrientation.TopLeft && !(_tileUnderGhost.HasConnectable_L && _tileUnderGhost.HasConnectable_T))
					|| (_ghost.Orientation == Tile.TileOrientation.TopRight && !(_tileUnderGhost.HasConnectable_T && _tileUnderGhost.HasConnectable_R))
					|| (_ghost.Orientation == Tile.TileOrientation.BottomRight && !(_tileUnderGhost.HasConnectable_R && _tileUnderGhost.HasConnectable_B))
					|| (_ghost.Orientation == Tile.TileOrientation.BottomLeft && !(_tileUnderGhost.HasConnectable_B && _tileUnderGhost.HasConnectable_L))) {

					ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_Blocked);
					return;
				}
				break;

			case ModeEnum.Door:
			case ModeEnum.Airlock:

				// is the tile... living on the edge? B)
				if (_tileUnderGhost.GridX == 0 || _tileUnderGhost.GridX == Grid.Instance.GridSizeX - 1 || _tileUnderGhost.GridY == 0 || _tileUnderGhost.GridY == Grid.Instance.GridSizeY) {
					ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_Blocked);
					return;
				}

				//if (_type == Tile.TileType.Door) {
				isHorizontal = _orientation == Tile.TileOrientation.Left || _orientation == Tile.TileOrientation.Right;
				isVertical = _orientation == Tile.TileOrientation.Bottom || _orientation == Tile.TileOrientation.Top;

				// does the tile have adjacent walls for the door to be in?
				if (isHorizontal && (!_tileUnderGhost.HasConnectable_L || !_tileUnderGhost.HasConnectable_R)
					|| isVertical && (!_tileUnderGhost.HasConnectable_B || !_tileUnderGhost.HasConnectable_T)) {

					ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_Blocked);
					return;
				}

				// does the tile have space for door entrances?
				bool _failed = false;
				neighbours = Grid.Instance.GetNeighbours(_tileUnderGhost.GridX, _tileUnderGhost.GridY);
				for (int j = 0; j < neighbours.Count; j++) {
					diffX = neighbours[j].GridX - _tileUnderGhost.GridX;
					diffY = neighbours[j].GridY - _tileUnderGhost.GridY;

					if (((isHorizontal && (diffX == 0 && diffY != 0)) || (isVertical && (diffX != 0 && diffY == 0))) && neighbours[j]._WallType_ != Tile.Type.Empty) {
						ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_Blocked);
						_failed = true;
						break;
					}
				}
				if (_failed)
					return;

				// is there already a door?
				if (_tileUnderGhost._WallType_ == Tile.Type.Door) {
					ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_AlreadyExisting);
					return;
				}
				//}
				//else if (_type == Tile.TileType.Empty) { // door entrance should never write to grid
				//    ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_NewWall);
				//    return;
				//}

				// is the tile below not cleared?
				if (_tileUnderGhost.IsOccupied) {
					ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_Blocked);
					return;
				}
				break;

			case ModeEnum.Default:
			case ModeEnum.Room:
			case ModeEnum.Fill:
				// is the tile below already a wall?
				if (_tileUnderGhost._WallType_ == Tile.Type.Solid) {
					ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_AlreadyExisting);
					return;
				}
				// is the tile below not cleared?
				if (_tileUnderGhost._WallType_ != Tile.Type.Empty || _tileUnderGhost.IsOccupied) {
					ApplySettingsToGhost(_ghost, _tileUnderGhost, false, Color_Blocked);
					return;
				}
				break;

			default:
				throw new System.NotImplementedException(Mode.ToString() + " hasn't been fully implemented yet!");
		}

		// all's good
		ApplySettingsToGhost(_ghost, _tileUnderGhost, true, Color_New);
	}

	protected override void ApplyCurrentTool() {
		for (int i = 0; i < selectedTiles.Count; i++)
			selectedTiles[i].SetTileType(isDeleting ? Tile.Type.Empty : selectedTilesNewType[i], isDeleting ? Tile.TileOrientation.None : selectedTilesNewOrientation[i]);

		base.ApplyCurrentTool ();
	}
}