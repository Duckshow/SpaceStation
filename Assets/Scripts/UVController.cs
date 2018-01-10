using UnityEngine;
using System.Linq;
using System.Collections.Generic;
public class UVController : MeshSorter {

	public DoubleInt Coordinates;
    public DoubleInt TemporaryCoordinates;

    //public Tile.Type Type;
    public Tile.TileOrientation Orientation;

    private MeshFilter myMeshFilter;
    private List<Vector4> myMeshUVs = new List<Vector4>();

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
        myMeshFilter.mesh.GetUVs(0, myMeshUVs);

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
        mesh.vertices = new Vector3[] { // imagine it upside down, and that's the mesh below
            new Vector3(-0.5f, -1, 0),      new Vector3(0, -1, 0),      new Vector3(0.5f, -1, 0), 
            new Vector3(-0.5f, -0.5f, 0),   new Vector3(0, -0.5f, 0),   new Vector3(0.5f, -0.5f, 0),
            new Vector3(-0.5f, 0, 0),       new Vector3(0, 0, 0),       new Vector3(0.5f, 0, 0), 
            new Vector3(-0.5f, 0, 0),       new Vector3(0, 0, 0),       new Vector3(0.5f, 0, 0), 
            new Vector3(-0.5f, 1, 0),                                   new Vector3(0.5f, 1, 0)
        };
        mesh.uv = new Vector2[]{
            new Vector2(0, 0),      new Vector2(0.5f, 0),       new Vector2(1, 0),
            new Vector2(0, 0.25f),  new Vector2(0.5f, 0.25f),   new Vector2(1, 0.25f),
            new Vector2(0, 0.5f),   new Vector2(0.5f, 0.5f),    new Vector2(1, 0.5f), 
            new Vector2(0, 0.5f),   new Vector2(0.5f, 0.5f),    new Vector2(1, 0.5f), 
            new Vector2(0, 1),                                  new Vector2(1, 1)
        };
        mesh.triangles = new int[]{
            0, 4, 1,
            1, 4, 2,
            2, 4, 5,
            5, 4, 8,
            8, 4, 7,
            7, 4, 6,
            6, 4, 3,
            3, 4, 0,

            9,  10, 12,
            12, 10, 13,
            13, 10, 11
        };

        // mesh.vertices = new Vector3[] { // imagine it upside down, and that's the mesh below
        //     new Vector3(-0.5f, -1, 0),      new Vector3(0, -1, 0),      new Vector3(0.5f, -1, 0), 
        //     new Vector3(-0.5f, -0.5f, 0),                               new Vector3(0.5f, -0.5f, 0),
        //     new Vector3(-0.5f, 0, 0),       new Vector3(0, 0, 0),       new Vector3(0.5f, 0, 0), 
        //     new Vector3(-0.5f, 0, 0),       new Vector3(0, 0, 0),       new Vector3(0.5f, 0, 0), 
        //     new Vector3(-0.5f, 1, 0),                                   new Vector3(0.5f, 1, 0)
        // };
        // mesh.uv = new Vector2[]{
        //     new Vector2(0, 0),      new Vector2(0.5f, 0),       new Vector2(1, 0),
        //     new Vector2(0, 0.25f),                              new Vector2(1, 0.25f),
        //     new Vector2(0, 0.5f),   new Vector2(0.5f, 0.5f),    new Vector2(1, 0.5f), 
        //     new Vector2(0, 0.5f),   new Vector2(0.5f, 0.5f),    new Vector2(1, 0.5f), 
        //     new Vector2(0, 1),                                  new Vector2(1, 1)
        // };
        // mesh.triangles = new int[]{
        //     0, 3, 1,
        //     3, 4, 1,
        //     1, 4, 2,
        //     3, 5, 6,
        //     3, 6, 4,
        //     6, 7, 4,
        //     5, 8, 6,
        //     6, 8, 9,
        //     6, 9, 10,
        //     6, 10, 12,
        //     8, 11, 9,
        //     9, 11, 12,
        //     9, 12, 10
        // };

        return mesh;
    }

    [EasyButtons.Button]
    public void UpdateCurrentGraphics() {
        ChangeAsset(Coordinates, false);
    }
    public void StopTempMode() {
        TemporaryCoordinates = null;
        ChangeAsset(Coordinates, false);
    }
    delegate void SetUV(int _index, float _x, float _y);
    public void ChangeAsset(DoubleInt _assetIndices, bool _temporary) {
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

        float _uvL = (Tile.RESOLUTION * _assetIndices.X) / CachedAssets.WallSet.TEXTURE_SIZE_X;
        float _uvR = (Tile.RESOLUTION * (_assetIndices.X + 1)) / CachedAssets.WallSet.TEXTURE_SIZE_X;

        float _uvB = (Tile.RESOLUTION * _assetIndices.Y) / CachedAssets.WallSet.TEXTURE_SIZE_Y;
        float _uvT = (Tile.RESOLUTION * (_assetIndices.Y + 2)) / CachedAssets.WallSet.TEXTURE_SIZE_Y;

        float _halfX = (_uvR - _uvL) * 0.5f;
        float _halfY = (_uvT - _uvB) * 0.5f;
        float _quarterY = _halfY * 0.5f;

        //Debug.LogFormat("UVs:\n Left: {0}\nRight: {1}\nBottom: {2}\nTop: {3}\nHalfX: {4}\nHalfY: {5}\nQuarterY: {6}", _uvL, _uvR, _uvB, _uvT, _halfX, _halfY, _quarterY);

        SetUV _setUV = delegate (int _index, float _x, float _y){
            Vector4 _uv = new Vector4();
            _uv.x = _x;
            _uv.y = _y;
            _uv.z = myMeshUVs[_index].z;
            _uv.w = myMeshUVs[_index].w;
            myMeshUVs[_index] = _uv;
        };
        _setUV(0, _uvL, _uvB);               _setUV(1, _uvL + _halfX, _uvB);                _setUV(2, _uvR, _uvB);
        _setUV(3, _uvL, _uvB + _quarterY);   _setUV(4, _uvL + _halfX, _uvB + _quarterY);    _setUV(5, _uvR, _uvB + _quarterY);
        _setUV(6, _uvL, _uvB + _halfY);      _setUV(7, _uvL + _halfX, _uvB + _halfY);       _setUV(8, _uvR, _uvB + _halfY);
        _setUV(9, _uvL, _uvB + _halfY);      _setUV(10, _uvL + _halfX, _uvB + _halfY);      _setUV(11, _uvR, _uvB + _halfY);
        _setUV(12, _uvL, _uvT);                                                             _setUV(13, _uvR, _uvT);

        //myMeshFilter.mesh.uv = myMeshUVs;
        myMeshFilter.mesh.SetUVs(0, myMeshUVs);
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
    private static List<Vector2> sUVColors_01 = new List<Vector2>();
    private static List<Vector4> sUVColors_23 = new List<Vector4>();
    private static List<Vector4> sUVColors_45 = new List<Vector4>();
    private static Vector4 sUVColor_01 = new Vector4();
    private static Vector4 sUVColor_23 = new Vector4();
    private static Vector4 sUVColor_45 = new Vector4();
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

        sUVColors_01.Clear();
        sUVColors_23.Clear();
        sUVColors_45.Clear();
        // sUVColor_01.x = colorIndex_0; // don't assign x & y since they're used for actual uv-ing
        // sUVColor_01.y = colorIndex_1;
        sUVColor_01.z = colorIndex_0;
        sUVColor_01.w = colorIndex_1;
        sUVColor_23.x = colorIndex_2;
        sUVColor_23.y = colorIndex_3;
        sUVColor_23.z = colorIndex_4;
        sUVColor_23.w = colorIndex_5;
        sUVColor_45.x = colorIndex_6;
        sUVColor_45.y = colorIndex_7;
        sUVColor_45.z = colorIndex_8;
        sUVColor_45.w = colorIndex_9;

        for (int i = 0; i < myMeshFilter.mesh.uv.Length; i++) {

            // add the actual uvs to _01
            sUVColor_01.x = myMeshUVs[i].x;
            sUVColor_01.y = myMeshUVs[i].y;

            sUVColors_01.Add(sUVColor_01);
            sUVColors_23.Add(sUVColor_23);
            sUVColors_45.Add(sUVColor_45);
        }

        myMeshFilter.mesh.SetUVs(0, sUVColors_01);
        myMeshFilter.mesh.SetUVs(1, sUVColors_23);
        myMeshFilter.mesh.SetUVs(2, sUVColors_45);
    }
    public void ResetUVColor() {
        SetUVColor(setColorIndex_0, setColorIndex_1, setColorIndex_2, setColorIndex_3, setColorIndex_4, setColorIndex_5, setColorIndex_6, setColorIndex_7, setColorIndex_8, setColorIndex_9, false);
    }

    private static List<Color32> sVertexColors = new List<Color32>();
    public void SetVertexColor(int _specificVertex, Color32 _color){
        myMeshFilter.mesh.GetColors(sVertexColors);
        if(sVertexColors.Count != myMeshFilter.mesh.vertexCount)
            sVertexColors = new Color32[myMeshFilter.mesh.vertexCount].ToList();

        sVertexColors[_specificVertex] = _color;
        myMeshFilter.mesh.SetColors(sVertexColors);
    }

    private static List<Vector4> sUVDoubleDots = new List<Vector4>();
    public void SetUVDoubleDot(int _specificUV, float _doubleDot0, float _doubleDot1, float _doubleDot2, float _doubleDot3){
        myMeshFilter.mesh.GetUVs(3, sUVDoubleDots);
        if(sUVDoubleDots.Count != myMeshFilter.mesh.vertexCount)
            sUVDoubleDots = new Vector4[myMeshFilter.mesh.vertexCount].ToList();

        Vector4 _doubleDot = sUVDoubleDots[_specificUV];
        _doubleDot.x = _doubleDot0;
        _doubleDot.y = _doubleDot1;
        _doubleDot.z = _doubleDot2;
        _doubleDot.w = _doubleDot3;
		sUVDoubleDots[_specificUV] = _doubleDot;

        myMeshFilter.mesh.SetUVs(3, sUVDoubleDots);
    }
}
