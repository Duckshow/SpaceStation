using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class PlaceObjects : BuilderBase {

	[SerializeField] private Toggle[] ObjectButtons;
	private PrefabReference ObjectToPlace;


	public override void Setup() {
		ObjectButtons [0].group.SetAllTogglesOff ();
		ObjectButtons [0].isOn = true;
		ObjectToPlace = ObjectButtons [0].GetComponent<PrefabReference> ();
		base.Setup ();
	}
	protected override void InheritedUpdate() {
		for (int i = 0; i < ObjectButtons.Length; i++) {
			if (ObjectButtons[i].isOn) {
				ObjectToPlace = ObjectButtons [i].GetComponent<PrefabReference> ();
				break;
			}
		}

		base.InheritedUpdate ();
	}

	protected override void SetGhostGraphics(ref GhostInfo _ghost, Tile _tileUnderGhost, bool _snapToNeighbours) {
		_ghost.Type = Tile.Type.Empty;
		_ghost.Orientation = Tile.TileOrientation.None;
		_ghost.ChangeAssets (ObjectToPlace.AssetBottom, ObjectToPlace.AssetTop);

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
		if(selectedTiles.Count > 0)
			Object.Instantiate (ObjectToPlace.Prefab, selectedTiles[0].CharacterPositionWorld, Quaternion.Euler(GetEulerFromOrientation(selectedTilesNewOrientation[0])));
	
		base.ApplyCurrentTool ();
	}

	private Vector3 GetEulerFromOrientation(Tile.TileOrientation _orientation){
		switch (selectedTiles[0]._Orientation_) {
			case Tile.TileOrientation.None:
			case Tile.TileOrientation.Bottom:
				return new Vector3 (0, 0, 0);
			case Tile.TileOrientation.Left:
				return new Vector3 (0, 0, -90);
			case Tile.TileOrientation.Top:
				return new Vector3 (0, 0, 180);
			case Tile.TileOrientation.Right:
				return new Vector3 (0, 0, 90);
			default:
				throw new System.Exception (selectedTiles[0]._Orientation_ + " is an invalid orientation!");
		}
	}
}