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
	}

    protected override void ResetModifiedTiles(bool _includingMouse = false) {
		// for (int i = 0; i < highlightedTiles.Count; i++) {
        //     highlightedTiles[i].RemoveTemporaryGraphicsForSurroundingTiles();
		// 	highlightedTiles[i].RemoveTemporaryColor();
		// }

		base.ResetModifiedTiles (_includingMouse);
	}
	protected override void ResetSelectedTiles() {
		// for (int i = 0; i < tilesToModify.Count; i++) {
        //     tilesToModify[i].RemoveTemporaryGraphicsForSurroundingTiles();
		// 	tilesToModify[i].RemoveTemporaryColor();
		// }

		base.ResetSelectedTiles ();
	}

    List<Node> neighbours;
    int diffX;
    int diffY;
    bool isHorizontal = false;
    bool isVertical = false;
	protected override bool Evaluate(Node _node){
		// is building even allowed?
		if (!_node.IsBuildingAllowed) {
			ApplySettingsToGhost(_node, false, ColorManager.GetColorIndex(ColorManager.ColorUsage.Blocked));
			return false;
		}
        // is the tile occupied?
        if (_node.GetOccupyingTileObject() != null) {
            ApplySettingsToGhost(_node, false, ColorManager.GetColorIndex(ColorManager.ColorUsage.Blocked));
            return false;
        }

		// deleting old tiles
		if (isDeleting) {
            if (!_node.IsWall) { // empty tiles can't be deleted
                ApplySettingsToGhost(_node, false, ColorManager.GetColorIndex(ColorManager.ColorUsage.AlreadyExisting));
                return false;
            }

            ApplySettingsToGhost(_node, true, ColorManager.GetColorIndex(ColorManager.ColorUsage.Blocked));
			return true;
		}

        // all's good
        ApplySettingsToGhost(_node, true, ColorManager.GetColorIndex(ColorManager.ColorUsage.New));
        return true;
	}

    protected override void ApplySettingsToGhost(Node _node, bool _applyToGrid, byte _newColorIndex) {
        base.ApplySettingsToGhost(_node, _applyToGrid, _newColorIndex);
    }

    protected override void ApplyCurrentTool() {
		for (int i = 0; i < tilesToModify.Count; i++) {
			tilesToModify[i].SetIsWall(!isDeleting);
		}

		base.ApplyCurrentTool ();
	}
}