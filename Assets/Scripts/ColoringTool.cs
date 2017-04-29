using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ColoringTool : BuilderBase {

    public Texture2D Palette;
	public Color[] AllColors = new Color[128];
	public static List<Vector4> sAllColorsForShaders = new List<Vector4> ();

	private static byte sColorIndex_0 = 0;
	private static byte sColorIndex_1 = 0;
	private static byte sColorIndex_2 = 0;
	private static byte sColorIndex_3 = 0;
    private static byte sColorIndex_4 = 0;
    private static byte sColorIndex_5 = 0;
    public static void AssignColorIndex(int _channel, byte _value){
		if (_channel == 0)
			sColorIndex_0 = _value;
		if (_channel == 1)
			sColorIndex_1 = _value;
		if (_channel == 2)
			sColorIndex_2 = _value;
		if (_channel == 3)
			sColorIndex_3 = _value;
        if (_channel == 4)
            sColorIndex_4 = _value;
        if (_channel == 5)
            sColorIndex_5 = _value;
    }

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
		for (int i = 0; i < selectedTiles.Count; i++)
			ResetColor (selectedTiles[i]);

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
		_tile.FloorQuad.SetUVColor (sColorIndex_0, sColorIndex_1, sColorIndex_2, sColorIndex_3, sColorIndex_4, sColorIndex_5, _temporarily);
		_tile.FloorCornerHider.SetUVColor (sColorIndex_0, sColorIndex_1, sColorIndex_2, sColorIndex_3, sColorIndex_4, sColorIndex_5, _temporarily);
		_tile.BottomQuad.SetUVColor (sColorIndex_0, sColorIndex_1, sColorIndex_2, sColorIndex_3, sColorIndex_4, sColorIndex_5, _temporarily);
		_tile.TopQuad.SetUVColor (sColorIndex_0, sColorIndex_1, sColorIndex_2, sColorIndex_3, sColorIndex_4, sColorIndex_5, _temporarily);
		_tile.WallCornerHider.SetUVColor (sColorIndex_0, sColorIndex_1, sColorIndex_2, sColorIndex_3, sColorIndex_4, sColorIndex_5, _temporarily);
	}
	public void ResetColor(Tile _tile){
		_tile.FloorQuad.ResetUVColor ();
		_tile.FloorCornerHider.ResetUVColor ();
		_tile.BottomQuad.ResetUVColor ();
		_tile.TopQuad.ResetUVColor ();
		_tile.WallCornerHider.ResetUVColor ();
	}
}