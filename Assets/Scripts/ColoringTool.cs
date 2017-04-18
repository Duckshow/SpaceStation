using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ColoringTool : BuilderBase {

	public Color[] AllColors = new Color[32];
	public static List<Vector4> sAllColorsForShaders = new List<Vector4> ();

	[SerializeField] private byte Color_0;
	[SerializeField] private byte Color_1;
	[SerializeField] private byte Color_2;
	[SerializeField] private byte Color_3;


	public override void Setup(Transform transform) {
		base.Setup (transform);

		for (int i = 0; i < AllColors.Length; i++)
			sAllColorsForShaders.Add (new Vector4(AllColors[i].r, AllColors[i].g, AllColors[i].b, AllColors[i].a));
	}

	protected override void TryChangeMode(){
		base.TryChangeMode ();

		if (Input.GetKey(KeyCode.Alpha1))
			Mode = ModeEnum.Fill;
		if (Input.GetKey(KeyCode.Alpha2))
			Mode = ModeEnum.Room;
	}

	protected override void ResetModifiedTiles(bool _includingMouse = false) {
		for (int i = 0; i < modifiedTiles.Count; i++)
			ResetColor (modifiedTiles[i]);

		base.ResetModifiedTiles (_includingMouse);
	}
	protected override void ResetSelectedTiles() {
		for (int i = 0; i < selectedTiles.Count; i++) {
			ResetColor (selectedTiles[i]);
		}

		base.ResetSelectedTiles ();
	}

    protected override Tile.Type DetermineGhostType(Tile _tile) {
		if (_tile._WallType_ != Tile.Type.Empty)
			return _tile._WallType_;
		else
			return _tile._FloorType_;
    }
    protected override Tile.TileOrientation DetermineGhostOrientation(Tile _tile, bool _snapToNeighbours) {
		if (_tile._WallType_ != Tile.Type.Empty)
			return _tile._Orientation_;
		else
			return _tile._FloorOrientation_;
	}

	protected override void Evaluate(Tile _tile){
		SetColor (_tile, true);
		selectedTiles.Add (_tile);
	}

	protected override void ApplyCurrentTool() {
		for (int i = 0; i < selectedTiles.Count; i++) {
			if (isDeleting)
				ResetColor (selectedTiles [i]);
			else
				SetColor (selectedTiles[i], false);
		}
		modifiedTiles.Clear ();
		selectedTiles.Clear ();

		base.ApplyCurrentTool ();
	}

	public void SetColor(Tile _tile, bool _temporarily){
		_tile.FloorQuad.SetVertexColor (Color_0, Color_1, Color_2, Color_3, _temporarily);
		_tile.FloorCornerHider.SetVertexColor (Color_0, Color_1, Color_2, Color_3, _temporarily);
		_tile.BottomQuad.SetVertexColor (Color_0, Color_1, Color_2, Color_3, _temporarily);
		_tile.TopQuad.SetVertexColor (Color_0, Color_1, Color_2, Color_3, _temporarily);
		_tile.WallCornerHider.SetVertexColor (Color_0, Color_1, Color_2, Color_3, _temporarily);
	}
	public void ResetColor(Tile _tile){
		_tile.FloorQuad.ResetVertexColor ();
		_tile.FloorCornerHider.ResetVertexColor ();
		_tile.BottomQuad.ResetVertexColor ();
		_tile.TopQuad.ResetVertexColor ();
		_tile.WallCornerHider.ResetVertexColor ();
	}
}