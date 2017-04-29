using UnityEngine;
using System.Collections.Generic;
public class UVController : MonoBehaviour {

	public CachedAssets.DoubleInt Coordinates;
    public CachedAssets.DoubleInt TemporaryCoordinates;

    //public Tile.Type Type;
    public Tile.TileOrientation Orientation;
	public enum SortingLayerEnum { Floor, Bottom, Top }
	public SortingLayerEnum SortingLayer;

    private MeshFilter myMeshFilter;
    private MeshRenderer myRenderer;
    private Vector2[] myMeshUVs;

	private static int sCachedPropertyColor = -1;
	private static int sCachedPropertyAllColors = -1;
    private bool hasStarted = false;
    private bool isHidden = false;

	//private Color32 oldVertexColor;
	//private Color32 vertexColor;

    private byte uvColorIndex_0;
    private byte uvColorIndex_1;
    private byte uvColorIndex_2;
    private byte uvColorIndex_3;
    private byte uvColorIndex_4;
    private byte uvColorIndex_5;
    private byte oldUvColorIndex_0;
    private byte oldUvColorIndex_1;
    private byte oldUvColorIndex_2;
    private byte oldUvColorIndex_3;
    private byte oldUvColorIndex_4;
    private byte oldUvColorIndex_5;



    void Start() {
        if (!hasStarted)
            Setup();
    }

    public void Setup() {
        if (hasStarted && Application.isPlaying)
            return;

        hasStarted = true;

        myMeshFilter = GetComponent<MeshFilter>();
        myRenderer = GetComponent<MeshRenderer>();
        myRenderer.sortingLayerName = "Grid";
		myMeshUVs = myMeshFilter.sharedMesh != null ? myMeshFilter.sharedMesh.uv : myMeshFilter.mesh.uv;

		if(sCachedPropertyColor == -1)
	        sCachedPropertyColor = Shader.PropertyToID("_Color");
		ChangeAsset(Coordinates, false);

		if (sCachedPropertyAllColors == -1) {
			sCachedPropertyAllColors = Shader.PropertyToID("_allColors");
			myRenderer.sharedMaterial.SetVectorArray (sCachedPropertyAllColors, ColoringTool.sAllColorsForShaders);
		}

		SetUVColor (1, 2, 2, 2, 2, 2, false);
    }

    public void ChangeAsset(CachedAssets.DoubleInt _assetIndices, bool _temporary) {
        if (_assetIndices == null) {
            if (_temporary) {
                TemporaryCoordinates = null;
                ChangeAsset(Coordinates, false);
                return;
            }

            Coordinates = null;
            myRenderer.enabled = false;
            return;
        }

        if (_temporary)
            TemporaryCoordinates = _assetIndices;
        else
	    	Coordinates = _assetIndices;

        myMeshUVs[0].x = (Grid.TILE_RESOLUTION * _assetIndices.X) / CachedAssets.WallSet.TEXTURE_SIZE_X;
        myMeshUVs[0].y = (Grid.TILE_RESOLUTION * _assetIndices.Y) / CachedAssets.WallSet.TEXTURE_SIZE_Y;

        myMeshUVs[1].x = (Grid.TILE_RESOLUTION * (_assetIndices.X + 1)) / CachedAssets.WallSet.TEXTURE_SIZE_X;
        myMeshUVs[1].y = (Grid.TILE_RESOLUTION * (_assetIndices.Y + 2)) / CachedAssets.WallSet.TEXTURE_SIZE_Y;

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
        myRenderer.material.SetColor(sCachedPropertyColor, _color);
    }
	//private static List<Color32> sVertexColors = new List<Color32> ();
	//public void SetVertexColor(byte _color0, byte _color1, byte _color2, byte _color3, bool _temporarily){
	//	if(oldVertexColor.Equals(vertexColor))
	//		oldVertexColor = vertexColor;

	//	vertexColor.r = _color0;
	//	vertexColor.g = _color1;
	//	vertexColor.b = _color2;
	//	vertexColor.a = _color3;

	//	if (!_temporarily)
	//		oldVertexColor = vertexColor;

	//	sVertexColors.Clear ();
 //       for (int i = 0; i < myMeshFilter.mesh.vertexCount; i++)
 //           sVertexColors.Add(vertexColor);
	//	myMeshFilter.mesh.SetColors (sVertexColors);
	//}
	//public void ResetVertexColor(){
	//	SetVertexColor (oldVertexColor.r, oldVertexColor.g, oldVertexColor.b, oldVertexColor.a, false);
	//}

    private static List<Vector2> sUVColors_0 = new List<Vector2>();
    private static List<Vector2> sUVColors_1 = new List<Vector2>();
    private static List<Vector2> sUVColors_2 = new List<Vector2>();
    private static Vector2 sUVColor_0 = new Vector2();
    private static Vector2 sUVColor_1 = new Vector2();
    private static Vector2 sUVColor_2 = new Vector2();
    public void SetUVColor(byte _color0, byte _color1, byte _color2, byte _color3, byte _color4, byte _color5, bool _temporarily, bool _resetting = false) {
        if (!_resetting && !_temporarily && (uvColorIndex_0 != _color0 || uvColorIndex_1 != _color1 || uvColorIndex_2 != _color2 || uvColorIndex_3 != _color3 || uvColorIndex_4 != _color4 || uvColorIndex_5 != _color5)) {
            oldUvColorIndex_0 = uvColorIndex_0;
            oldUvColorIndex_1 = uvColorIndex_1;
            oldUvColorIndex_2 = uvColorIndex_2;
            oldUvColorIndex_3 = uvColorIndex_3;
            oldUvColorIndex_4 = uvColorIndex_4;
            oldUvColorIndex_5 = uvColorIndex_5;
        }

        uvColorIndex_0 = _color0;
        uvColorIndex_1 = _color1;
        uvColorIndex_2 = _color2;
        uvColorIndex_3 = _color3;
        uvColorIndex_4 = _color4;
        uvColorIndex_5 = _color5;
        if(transform.parent == null)
            Debug.Log(transform.name + " (" + _temporarily + "): " + _color0 + ", " + _color1 + ", " + _color2 + ", " + _color3 + ", " + _color4 + ", " + _color5);

        sUVColors_0.Clear();
        sUVColors_1.Clear();
        sUVColors_2.Clear();
        sUVColor_0.x = uvColorIndex_0;
        sUVColor_0.y = uvColorIndex_1;
        sUVColor_1.x = uvColorIndex_2;
        sUVColor_1.y = uvColorIndex_3;
        sUVColor_2.x = uvColorIndex_4;
        sUVColor_2.y = uvColorIndex_5;
        for (int i = 0; i < myMeshFilter.mesh.uv.Length; i++) {
            sUVColors_0.Add(sUVColor_0);
            sUVColors_1.Add(sUVColor_1);
            sUVColors_2.Add(sUVColor_2);
        }
        myMeshFilter.mesh.SetUVs(1, sUVColors_0);
        myMeshFilter.mesh.SetUVs(2, sUVColors_1);
        myMeshFilter.mesh.SetUVs(3, sUVColors_2);
    }
    public void ResetUVColor() {
        SetUVColor(oldUvColorIndex_0, oldUvColorIndex_1, oldUvColorIndex_2, oldUvColorIndex_3, oldUvColorIndex_4, oldUvColorIndex_5, false, true);
    }

    public static int GetSortOrderFromGridY(int _gridY) { return (Grid.Instance.GridSizeY * 10) - (_gridY * 10); }
    public int GetSortOrder() { return (customSortOrder.HasValue ? (int)customSortOrder : regularSortOrder); }
    private int regularSortOrder = 0;
    public void Sort(int _gridY) {
        if (!hasStarted)
            Setup();

        regularSortOrder = GetSortOrderFromGridY(_gridY);
		if (SortingLayer == SortingLayerEnum.Floor)
			regularSortOrder -= 1;
		else if (SortingLayer == SortingLayerEnum.Top)
            regularSortOrder += 7; // hack to account for transforms in an actor

        if(!customSortOrder.HasValue)
            myRenderer.sortingOrder = regularSortOrder;
    }
    private int? customSortOrder = null;
    public void SortCustom(int _customSortOrder) {
        customSortOrder = _customSortOrder;
        myRenderer.sortingOrder = _customSortOrder;
    }
    public void RemoveCustomSort() {
        customSortOrder = null;
        myRenderer.sortingOrder = regularSortOrder;
    }
}
