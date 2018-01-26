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

    protected override void ResetModifiedTiles(bool _includingMouse = false) {
		for (int i = 0; i < highlightedTiles.Count; i++) {
			highlightedTiles[i].SetTileType(Tile.Type.Empty, Tile.TileOrientation.None, _temporarily: true);
            highlightedTiles[i].ResetTempSettingsWall();
            highlightedTiles[i].ResetFloorColor();
            highlightedTiles[i].ResetWallColor();
		}

		base.ResetModifiedTiles (_includingMouse);
	}
	protected override void ResetSelectedTiles() {
		for (int i = 0; i < tilesToModify.Count; i++) {
			tilesToModify[i].SetTileType(Tile.Type.Empty, tilesToModify[i].TempOrientation, _temporarily: true);
            tilesToModify[i].ResetTempSettingsWall();
            tilesToModify[i].ResetFloorColor();
            tilesToModify[i].ResetWallColor();
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
                if (_tile._WallType_ == Tile.Type.Door) // (special exception)
                    return Tile.Type.Door;
                else if (_tile._WallType_ == Tile.Type.Airlock) // (special exception)
                    return Tile.Type.Airlock;
                else
                   return Tile.Type.Solid;
            case ModeEnum.Diagonal:
                if (_tile._WallType_ == Tile.Type.Door) // (special exception)
                    return Tile.Type.Door;
                else if (_tile._WallType_ == Tile.Type.Airlock) // (special exception)
                    return Tile.Type.Airlock;
                else
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
                // (special exception)
                if (_tile._WallType_ == Tile.Type.Door || _tile._WallType_ == Tile.Type.Airlock)
                    return _tile._Orientation_;
                else
                    return Tile.TileOrientation.None;
            case ModeEnum.Diagonal:
                // (special exception)
                if (_tile._WallType_ == Tile.Type.Door || _tile._WallType_ == Tile.Type.Airlock)
                    return _tile._Orientation_;

                if ((_snapToNeighbours && _tile.HasConnectable_L && _tile.HasConnectable_T) || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.TopLeft))
                    return Tile.TileOrientation.TopLeft;
                else if ((_snapToNeighbours && _tile.HasConnectable_T && _tile.HasConnectable_R) || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.TopRight))
                    return Tile.TileOrientation.TopRight;
                else if ((_snapToNeighbours && _tile.HasConnectable_R && _tile.HasConnectable_B) || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.BottomRight))
                    return Tile.TileOrientation.BottomRight;
                else if ((_snapToNeighbours && _tile.HasConnectable_B && _tile.HasConnectable_L) || (!_snapToNeighbours && _tile.TempOrientation == Tile.TileOrientation.BottomLeft))
                    return Tile.TileOrientation.BottomLeft;
                else
                    return /*_snapToNeighbours ? Tile.TileOrientation.TopLeft : */_tile.TempOrientation;
            case ModeEnum.Door:
                if ((_snapToNeighbours && (_tile.HasConnectable_L && _tile.HasConnectable_R && !_tile.HasConnectable_B && !_tile.HasConnectable_T)) || (!_snapToNeighbours && (_tile.TempOrientation == Tile.TileOrientation.Left || _tile.TempOrientation == Tile.TileOrientation.Right)))
                    return Tile.TileOrientation.Left; // left or right shouldn't matter...
                else if ((_snapToNeighbours && (!_tile.HasConnectable_L && !_tile.HasConnectable_R && _tile.HasConnectable_B && _tile.HasConnectable_T)) || (!_snapToNeighbours && (_tile.TempOrientation == Tile.TileOrientation.Bottom || _tile.TempOrientation == Tile.TileOrientation.Top)))
                    return Tile.TileOrientation.Bottom; // bottom or top shouldn't matter...
                else
                   return /*_snapToNeighbours ? Tile.TileOrientation.Left : */_tile.TempOrientation;
            case ModeEnum.Airlock:
                if ((_snapToNeighbours && (_tile.HasConnectable_L && _tile.HasConnectable_R && !_tile.HasConnectable_B && !_tile.HasConnectable_T)) || (!_snapToNeighbours && (_tile.TempOrientation == Tile.TileOrientation.Left || _tile.TempOrientation == Tile.TileOrientation.Right)))
                    return Tile.TileOrientation.Left; // left or right shouldn't matter...
                else if ((_snapToNeighbours && (!_tile.HasConnectable_L && !_tile.HasConnectable_R && _tile.HasConnectable_B && _tile.HasConnectable_T)) || (!_snapToNeighbours && (_tile.TempOrientation == Tile.TileOrientation.Bottom || _tile.TempOrientation == Tile.TileOrientation.Top)))
                    return Tile.TileOrientation.Bottom; // bottom or top shouldn't matter...
                else
                    return /*_snapToNeighbours ? Tile.TileOrientation.Left : */_tile.TempOrientation;

            default:
                throw new System.NotImplementedException(Mode.ToString() + " hasn't been properly implemented yet!");
        }
    }
    protected override void SetGhostGraphics(Tile _tile, bool _snapToNeighbours) {
        switch (Mode) {
            case ModeEnum.Default:
            case ModeEnum.Room:
			case ModeEnum.Fill:
            case ModeEnum.Diagonal:
            case ModeEnum.Door:
            case ModeEnum.Airlock:
                if (!isDeleting && (_tile._WallType_ != _tile.TempType || _tile._Orientation_ != _tile.TempOrientation)) {
                    _tile.ChangeWallGraphics(
						CachedAssets.Instance.GetWallAssetForTile(_tile.TempType, _tile.TempOrientation, 0, true, _tile.HasConnectableTemp_L, _tile.HasConnectableTemp_T, _tile.HasConnectableTemp_R, _tile.HasConnectableTemp_B),
						CachedAssets.Instance.GetWallAssetForTile(_tile.TempType, _tile.TempOrientation, 0, false, _tile.HasConnectableTemp_L, _tile.HasConnectableTemp_T, _tile.HasConnectableTemp_R, _tile.HasConnectableTemp_B),
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
		// is building even allowed?
		if (!_tile._BuildingAllowed_) {
			ApplySettingsToGhost(_tile, false, ColorIndex_Blocked);
			return false;
		}
        // is the tile occupied?
        if (_tile.IsOccupiedByObject) {
            ApplySettingsToGhost(_tile, false, ColorIndex_Blocked);
            return false;
        }

		// deleting old tiles
		if (isDeleting) {
            if (_tile._WallType_ == Tile.Type.Empty) { // empty tiles can't be deleted
                ApplySettingsToGhost(_tile, false, ColorIndex_AlreadyExisting);
                return false;
            }

            ApplySettingsToGhost(_tile, true, ColorIndex_Remove);
			return true;
		}

        // is the tile below a type of door and we're in a different mode currently? (special exception because nicer interaction)
        if ((Mode != ModeEnum.Door && _tile._WallType_ == Tile.Type.Door) || (Mode != ModeEnum.Airlock && _tile._WallType_ == Tile.Type.Airlock)) {
            ApplySettingsToGhost(_tile, false, ColorIndex_Blocked);
            return false;
        }

        // is there already an identical wall in place?
        if (_tile._WallType_ == _tile.TempType && _tile._Orientation_ == _tile.TempOrientation) {
            ApplySettingsToGhost(_tile, false, ColorIndex_AlreadyExisting);
            return false;
        }

        // all's good
        ApplySettingsToGhost(_tile, true, ColorIndex_New);
        return true;
	}

    protected override void ApplySettingsToGhost(Tile _tile, bool _applyToGrid, byte _newColorIndex) {
        _tile.SetFloorColor(ColorIndex_AlreadyExisting, true);
        _tile.SetWallColor(_newColorIndex, true);
        base.ApplySettingsToGhost(_tile, _applyToGrid, _newColorIndex);
    }

    protected override void ApplyCurrentTool() {
		for (int i = 0; i < tilesToModify.Count; i++) {
			tilesToModify[i].SetTileType(isDeleting ? Tile.Type.Empty : tilesToModify[i].TempType, isDeleting ? Tile.TileOrientation.None : tilesToModify[i].TempOrientation);
		}

		base.ApplyCurrentTool ();
	}
}