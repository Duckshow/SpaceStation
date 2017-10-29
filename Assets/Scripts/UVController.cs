using UnityEngine;
using System.Linq;
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


    void Awake() {
        SortingLayer = MeshSorter.SortingLayerEnum.Grid;
    }

    public override void Setup() {
        base.Setup();

        myMeshFilter = GetComponent<MeshFilter>();
        if(myMeshFilter.sharedMesh == null)
            myMeshFilter.sharedMesh = GenerateMesh();
        myMeshUVs = myMeshFilter.sharedMesh.uv;

		if(sCachedPropertyColor == -1)
	        sCachedPropertyColor = Shader.PropertyToID("_Color");
		ChangeAsset(Coordinates, false);

		if (sCachedPropertyAllColors == -1) { // this is only to prevent array being sent more than once
			sCachedPropertyAllColors = Shader.PropertyToID("_allColors");
			myRenderer.sharedMaterial.SetVectorArray (sCachedPropertyAllColors, ColoringTool.sAllColorsForShaders);
		}

		SetUVColor (0, 0, 0, 0, 0, 0, 0, 0, 0, 0, false);
    }

    Mesh GenerateMesh(){
        Mesh mesh = new Mesh();
        mesh.vertices = new Vector3[] {
            new Vector3(-0.5f, -1, 0),
            new Vector3(-0.5f, 0, 0),
            new Vector3(-0.5f, 0, 0), // duplicate to stop vertex color spilling into tile above
            new Vector3(-0.5f, 1, 0),
            new Vector3(0.5f, 1, 0),
            new Vector3(0.5f, 0, 0),
            new Vector3(0.5f, 0, 0), // duplicate again
            new Vector3(0.5f, -1, 0)
        };
        mesh.uv = new Vector2[]{
            new Vector2(0, 0),
            new Vector2(0, 0.5f),
            new Vector2(0, 0.5f),
            new Vector2(0, 1),
            new Vector2(1, 1),
            new Vector2(1, 0.5f),
            new Vector2(1, 0.5f),
            new Vector2(1, 0)
        };
        mesh.triangles = new int[]{
            0,
            6,
            7,

            0,
            1,
            6,

            2,
            4,
            5,

            2,
            3,
            4
        };
        return mesh;
    }

    public void StopTempMode() {
        TemporaryCoordinates = null;
        ChangeAsset(Coordinates, false);
    }
    public void ChangeAsset(CachedAssets.DoubleInt _assetIndices, bool _temporary) {
        if(!Application.isPlaying)
            return;

        if (_temporary)
            TemporaryCoordinates = _assetIndices;
        else
	    	Coordinates = _assetIndices;

        if (_assetIndices == null) {
            myRenderer.enabled = false;
            return;
        }

        // BL
        myMeshUVs[0].x = (Tile.RESOLUTION * _assetIndices.X) / CachedAssets.WallSet.TEXTURE_SIZE_X;
        myMeshUVs[0].y = (Tile.RESOLUTION * _assetIndices.Y) / CachedAssets.WallSet.TEXTURE_SIZE_Y;

        // TR
        myMeshUVs[4].x = (Tile.RESOLUTION * (_assetIndices.X + 1)) / CachedAssets.WallSet.TEXTURE_SIZE_X;
        myMeshUVs[4].y = (Tile.RESOLUTION * (_assetIndices.Y + 2)) / CachedAssets.WallSet.TEXTURE_SIZE_Y;

        // L
        myMeshUVs[1].x = myMeshUVs[0].x;
        myMeshUVs[1].y = (Tile.RESOLUTION * (_assetIndices.Y + 1)) / CachedAssets.WallSet.TEXTURE_SIZE_Y;
        myMeshUVs[2] = myMeshUVs[1];

        // TL
        myMeshUVs[3].x = myMeshUVs[0].x;
        myMeshUVs[3].y = myMeshUVs[4].y;

        // R
        myMeshUVs[5].x = myMeshUVs[4].x;
        myMeshUVs[5].y = myMeshUVs[1].y;
        myMeshUVs[6] = myMeshUVs[5];

        // BR
        myMeshUVs[7].x = myMeshUVs[4].x;
        myMeshUVs[7].y = myMeshUVs[0].y;

        myMeshFilter.mesh.uv = myMeshUVs;
        if(!isHidden)
            myRenderer.enabled = true;
    }
    public void Hide(bool _b) {
        if (!hasStarted)
            Setup();

        myRenderer.enabled = !_b;
        isHidden = _b;
    }

    public void ChangeColor(byte _colorIndex) {
        if (!hasStarted)
            Setup();

        SetUVColor(_colorIndex, _colorIndex, _colorIndex, _colorIndex, _colorIndex, _colorIndex, _colorIndex, _colorIndex, _colorIndex, _colorIndex, true);
    }

    // skip 0 as it's used as actual UV
    private static List<Vector4> sUVColors_12 = new List<Vector4>();
    private static List<Vector4> sUVColors_34 = new List<Vector4>();
    private static List<Vector2> sUVColors_5 = new List<Vector2>();
    private static Vector4 sUVColor_12 = new Vector4();
    private static Vector4 sUVColor_34 = new Vector4();
    private static Vector2 sUVColor_5 = new Vector2();
    public void SetUVColor(byte _color0, byte _color1, byte _color2, byte _color3, byte _color4, byte _color5, byte _color6, byte _color7, byte _color8, byte _color9, bool _temporarily) {
        if (!_temporarily) {
			setColorIndex_0 = _color0;
			setColorIndex_1 = _color1;
			setColorIndex_2 = _color2;
			setColorIndex_3 = _color3;
			setColorIndex_4 = _color4;
			setColorIndex_5 = _color5;
            setColorIndex_6 = _color6;
            setColorIndex_7 = _color7;
            setColorIndex_8 = _color8;
            setColorIndex_9 = _color9;
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

        sUVColors_12.Clear();
        sUVColors_34.Clear();
        sUVColors_5.Clear();
        sUVColor_12.x = colorIndex_0; // non-alloc, instead of doing new Vector2() all the time
        sUVColor_12.y = colorIndex_1;
        sUVColor_12.z = colorIndex_2;
        sUVColor_12.w = colorIndex_3;
        sUVColor_34.x = colorIndex_4;
        sUVColor_34.y = colorIndex_5;
        sUVColor_34.z = colorIndex_6;
        sUVColor_34.w = colorIndex_7;
        sUVColor_5.x = colorIndex_8;
        sUVColor_5.y = colorIndex_9;
        for (int i = 0; i < myMeshFilter.mesh.uv.Length; i++) {
            sUVColors_12.Add(sUVColor_12);
            sUVColors_34.Add(sUVColor_34);
            sUVColors_5.Add(sUVColor_5);
        }

        myMeshFilter.mesh.SetUVs(1, sUVColors_12);
        myMeshFilter.mesh.SetUVs(2, sUVColors_34);
        myMeshFilter.mesh.SetUVs(3, sUVColors_5);
    }
    public void ResetUVColor() {
        SetUVColor(setColorIndex_0, setColorIndex_1, setColorIndex_2, setColorIndex_3, setColorIndex_4, setColorIndex_5, setColorIndex_6, setColorIndex_7, setColorIndex_8, setColorIndex_9, false);
    }

    private static Color32[] sVertexColors;
    public void SetVertexColor(byte _specificVertex, Color32 _color){
        sVertexColors = myMeshFilter.mesh.colors32.Length > 0 ? myMeshFilter.mesh.colors32 : new Color32[myMeshFilter.mesh.vertices.Length];
        sVertexColors[_specificVertex] = _color;
        myMeshFilter.mesh.colors32 = sVertexColors;
    }
}
