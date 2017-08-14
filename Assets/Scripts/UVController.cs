using UnityEngine;
using System.Collections.Generic;
public class UVController : MeshSorter {

	public CachedAssets.DoubleInt Coordinates;
    public CachedAssets.DoubleInt TemporaryCoordinates;

    //public Tile.Type Type;
    public Tile.TileOrientation Orientation;

    private MeshFilter myMeshFilter;
    private Vector2[] myMeshUVs;

	private static int sCachedPropertyColor = -1;
	private static int sCachedPropertyAllColors = -1;
    private bool isHidden = false;

    private byte colorIndex_0;
    private byte colorIndex_1;
    private byte colorIndex_2;
    private byte colorIndex_3;
    private byte colorIndex_4;
    private byte colorIndex_5;
    private byte colorIndex_6;
    private byte colorIndex_7;
    private byte colorIndex_8;
    private byte colorIndex_9;
    private byte setColorIndex_0;
	private byte setColorIndex_1;
	private byte setColorIndex_2;
	private byte setColorIndex_3;
	private byte setColorIndex_4;
	private byte setColorIndex_5;
    private byte setColorIndex_6;
    private byte setColorIndex_7;
    private byte setColorIndex_8;
    private byte setColorIndex_9;

	private Color32 setVertexColor;
	private Color32 myVertexColor;


    void Awake() {
        SortingLayer = MeshSorter.SortingLayerEnum.Grid;
    }

    public override void Setup() {
        base.Setup();

        myMeshFilter = GetComponent<MeshFilter>();
        myMeshUVs = myMeshFilter.sharedMesh != null ? myMeshFilter.sharedMesh.uv : myMeshFilter.mesh.uv;

		if(sCachedPropertyColor == -1)
	        sCachedPropertyColor = Shader.PropertyToID("_Color");
		ChangeAsset(Coordinates, false);

		if (sCachedPropertyAllColors == -1) {
			sCachedPropertyAllColors = Shader.PropertyToID("_allColors");
			myRenderer.sharedMaterial.SetVectorArray (sCachedPropertyAllColors, ColoringTool.sAllColorsForShaders);
		}

		SetUVColor (1, 2, 2, 2, 2, 2, 2, 2, 2, 2, false);
    }

    public void StopTempMode() {
        TemporaryCoordinates = null;
        ChangeAsset(Coordinates, false);
    }
    public void ChangeAsset(CachedAssets.DoubleInt _assetIndices, bool _temporary) {

        if (_temporary)
            TemporaryCoordinates = _assetIndices;
        else
	    	Coordinates = _assetIndices;

        if (_assetIndices == null) {
            //if (_temporary) {
            //    TemporaryCoordinates = null;
            //    _assetIndices = Coordinates;
            //    _temporary = false;
            //}

            //if (_assetIndices == null && !_temporary) {
            //    Coordinates = null;
            //    myRenderer.enabled = false;
            //    return;
            //}

            myRenderer.enabled = false;
            return;
        }

        myMeshUVs[0].x = (Tile.RESOLUTION * _assetIndices.X) / CachedAssets.WallSet.TEXTURE_SIZE_X;
        myMeshUVs[0].y = (Tile.RESOLUTION * _assetIndices.Y) / CachedAssets.WallSet.TEXTURE_SIZE_Y;

        myMeshUVs[1].x = (Tile.RESOLUTION * (_assetIndices.X + 1)) / CachedAssets.WallSet.TEXTURE_SIZE_X;
        myMeshUVs[1].y = (Tile.RESOLUTION * (_assetIndices.Y + 2)) / CachedAssets.WallSet.TEXTURE_SIZE_Y;

        myMeshUVs[2].x = myMeshUVs[1].x;
        myMeshUVs[2].y = myMeshUVs[0].y;

        myMeshUVs[3].x = myMeshUVs[0].x;
        myMeshUVs[3].y = myMeshUVs[1].y;

        if(Application.isPlaying)
            myMeshFilter.mesh.uv = myMeshUVs;
        else
            myMeshFilter.sharedMesh.uv = myMeshUVs;

        if(!isHidden)
            myRenderer.enabled = true;
    }
    public void Hide(bool _b) {
        if (!hasStarted)
            Setup();

        myRenderer.enabled = !_b;
        isHidden = _b;
    }

    public void ChangeColor(Color _color) {
        if (!hasStarted)
            Setup();

        myRenderer.material.SetColor(sCachedPropertyColor, _color);
    }

    private static List<Vector2> sUVColors_0 = new List<Vector2>();
    private static List<Vector2> sUVColors_1 = new List<Vector2>();
    private static List<Vector2> sUVColors_2 = new List<Vector2>();
    private static List<Color32> sVertexColors = new List<Color32>();
    private static Vector2 sUVColor_0 = new Vector2();
    private static Vector2 sUVColor_1 = new Vector2();
    private static Vector2 sUVColor_2 = new Vector2();
    public void SetUVColor(byte _color0, byte _color1, byte _color2, byte _color3, byte _color4, byte _color5, byte _color6, byte _color7, byte _color8, byte _color9, bool _temporarily) {
        if (!_temporarily) {
			setColorIndex_0 = _color0;
			setColorIndex_1 = _color1;
			setColorIndex_2 = _color2;
			setColorIndex_3 = _color3;
			setColorIndex_4 = _color4;
			setColorIndex_5 = _color5;

            setVertexColor.r = _color6;
            setVertexColor.g = _color7;
            setVertexColor.b = _color8;
            setVertexColor.a = _color9;
        }

        colorIndex_0 = _color0;
        colorIndex_1 = _color1;
        colorIndex_2 = _color2;
        colorIndex_3 = _color3;
        colorIndex_4 = _color4;
        colorIndex_5 = _color5;
        colorIndex_6 = _color6;
        colorIndex_7 = _color7;
        colorIndex_8 = _color8;
        colorIndex_9 = _color9;

        sUVColors_0.Clear();
        sUVColors_1.Clear();
        sUVColors_2.Clear();
        sUVColor_0.x = colorIndex_0;
        sUVColor_0.y = colorIndex_1;
        sUVColor_1.x = colorIndex_2;
        sUVColor_1.y = colorIndex_3;
        sUVColor_2.x = colorIndex_4;
        sUVColor_2.y = colorIndex_5;
        for (int i = 0; i < myMeshFilter.mesh.uv.Length; i++) {
            sUVColors_0.Add(sUVColor_0);
            sUVColors_1.Add(sUVColor_1);
            sUVColors_2.Add(sUVColor_2);
        }
        myMeshFilter.mesh.SetUVs(1, sUVColors_0);
        myMeshFilter.mesh.SetUVs(2, sUVColors_1);
        myMeshFilter.mesh.SetUVs(3, sUVColors_2);

        myVertexColor.r = colorIndex_6;
        myVertexColor.g = colorIndex_7;
        myVertexColor.b = colorIndex_8;
        myVertexColor.a = colorIndex_9;
        sVertexColors.Clear();
        for (int i = 0; i < myMeshFilter.mesh.vertexCount; i++)
            sVertexColors.Add(myVertexColor);
        myMeshFilter.mesh.SetColors(sVertexColors);
    }
    public void ResetUVColor() {
        SetUVColor(setColorIndex_0, setColorIndex_1, setColorIndex_2, setColorIndex_3, setColorIndex_4, setColorIndex_5, setVertexColor.r, setVertexColor.g, setVertexColor.b, setVertexColor.a, false);
    }
}
