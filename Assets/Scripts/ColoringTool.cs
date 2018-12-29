using UnityEngine;
using System.Collections;
using System.Collections.Generic;

[System.Serializable]
public class ColoringTool : BuilderBase {

	public const byte COLOR_WHITE = 1; // actual white is 0, but soo bright
    public const byte COLOR_GREY = 8;
    public const byte COLOR_RED = 124;
    public const byte COLOR_ORANGE = 36;

    public Texture2D Palette;
	public Color[] AllColors = new Color[128];
	public static List<Vector4> sAllColorsForShaders = new List<Vector4> ();

	private static byte sColorIndex_0 = 5;
	private static byte sColorIndex_1 = 5;
	private static byte sColorIndex_2 = 5;
	private static byte sColorIndex_3 = 5;
    private static byte sColorIndex_4 = 5;
    private static byte sColorIndex_5 = 5;
    private static byte sColorIndex_6 = 5;
    private static byte sColorIndex_7 = 5;
    private static byte sColorIndex_8 = 5;
    private static byte sColorIndex_9 = 5;
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
        if (_channel == 6)
            sColorIndex_6 = _value;
        if (_channel == 7)
            sColorIndex_7 = _value;
        if (_channel == 8)
            sColorIndex_8 = _value;
        if (_channel == 9)
            sColorIndex_9 = _value;
    }

	public override void Setup(Transform transform) {
		base.Setup (transform);

		for (int i = 0; i < AllColors.Length; i++)
			sAllColorsForShaders.Add (new Vector4(AllColors[i].r, AllColors[i].g, AllColors[i].b, AllColors[i].a));
	}

	protected override void TryChangeMode(){
		base.TryChangeMode ();

		if (Input.GetKey(KeyCode.Alpha1)) { 
			Mode = ModeEnum.Room;
		}
	}

	protected override void ResetModifiedTiles(bool _includingMouse = false) {
		for (int i = 0; i < highlightedTiles.Count; i++) { 
			ResetColor (highlightedTiles[i]);
		}

		base.ResetModifiedTiles (_includingMouse);
	}
	protected override void ResetSelectedTiles() {
		for (int i = 0; i < tilesToModify.Count; i++) { 
			ResetColor (tilesToModify[i]);
		}

		base.ResetSelectedTiles ();
	}

	protected override bool Evaluate(Node _node){
		SetColor (_node, _temporarily: true);
		tilesToModify.Add (_node);
        return true;
	}

    protected override void ApplySettingsToGhost(Node _node, bool _applyToGrid, byte _newColorIndex) {
        base.ApplySettingsToGhost(_node, _applyToGrid, _newColorIndex);
    }

    protected override void ApplyCurrentTool() {
		for (int i = 0; i < tilesToModify.Count; i++) {
			if (isDeleting) { 
				ResetColor (tilesToModify[i]);
			}
			else { 
				SetColor (tilesToModify[i], false);
			}
		}
		highlightedTiles.Clear ();
		tilesToModify.Clear ();

		base.ApplyCurrentTool ();
	}

	public void SetColor(Node _node, bool _temporarily){
		// _node.MyUVController.ChangeColor(sColorIndex_0, sColorIndex_1, sColorIndex_2, sColorIndex_3, sColorIndex_4, sColorIndex_5, sColorIndex_6, sColorIndex_7, sColorIndex_8, sColorIndex_9, _temporarily);
	}
	public void ResetColor(Node _node){
		// _node.MyUVController.ResetColor();
	}
}