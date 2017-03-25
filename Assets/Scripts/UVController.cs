﻿using UnityEngine;
public class UVController : MonoBehaviour {

    //public Tile.Type Type;
    public Tile.TileOrientation Orientation;
	public enum SortingLayerEnum { Floor, Bottom, Top }
	public SortingLayerEnum SortingLayer;

    private MeshFilter myMeshFilter;
    private MeshRenderer myRenderer;
    private Vector2[] myMeshUVs;

    private int cachedPropertyColor;
    private bool hasStarted = false;
    private bool isHidden = false;


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

        cachedPropertyColor = Shader.PropertyToID("_Color");
        ChangeAsset(Coordinates);
    }

	public CachedAssets.DoubleInt Coordinates;

    public void ChangeAsset(CachedAssets.DoubleInt _assetIndices) {
        if (_assetIndices == null) {
            myRenderer.enabled = false;
            return;
        }
        
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

        //if(transform.parent != null)
        //    Debug.Log(transform.parent.name + "/" + transform.name + " was re-enabled!");
        //else
        //    Debug.Log(transform.name + " was re-enabled!");

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
        myRenderer.material.SetColor(cachedPropertyColor, _color);
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
            regularSortOrder += 7;

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
