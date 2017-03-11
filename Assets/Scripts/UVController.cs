using UnityEngine;
using System.Collections;

public class UVController : MonoBehaviour {

    public Tile.Type Type;
    public Tile.TileOrientation Orientation;
	public enum SortingLayerEnum { Floor, Bottom, Top }
	public SortingLayerEnum SortingLayer;

    private MeshFilter myMeshFilter;
    private MeshRenderer myRenderer;
    private Vector2[] myMeshUVs;

    private int cachedPropertyColor;
    private bool hasStarted = false;

    void Start() {
        if (!hasStarted)
            Setup();
    }

    public void Setup() {
        if (hasStarted)
            return;

        hasStarted = true;

        myMeshFilter = GetComponent<MeshFilter>();
        myRenderer = GetComponent<MeshRenderer>();
        myRenderer.sortingLayerName = "Grid";
		myMeshUVs = myMeshFilter.sharedMesh.uv;

        cachedPropertyColor = Shader.PropertyToID("_Color");
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

        myMeshFilter.mesh.uv = myMeshUVs;
        myRenderer.enabled = true;
    }

    public void ChangeColor(Color _color) {
        myRenderer.material.SetColor(cachedPropertyColor, _color);
    }

    public static int GetSortOrderFromGridY(int _gridY) { return (Grid.Instance.GridSizeY * 10) - (_gridY * 10); }
    public int GetSortOrder() { return (customSortOrder.HasValue ? (int)customSortOrder : regularSortOrder); }
    private int regularSortOrder = 0;
    public void Sort(int _gridY) {
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

    //public void ChangeAsset(Tile _tile, Vector2 _textureSize, Vector2 _coordinates, Vector2 _size) {

    //    meshUVs[0].x = Coordinates.x / TextureSize.x;
    //    meshUVs[0].y = Coordinates.y / TextureSize.y;

    //    meshUVs[1].x = (Coordinates.x + Size.x) / TextureSize.x;
    //    meshUVs[1].y = (Coordinates.y + Size.y) / TextureSize.y;

    //    meshUVs[2].x = meshUVs[1].x;
    //    meshUVs[2].y = meshUVs[0].y;

    //    meshUVs[3].x = meshUVs[0].x;
    //    meshUVs[3].y = meshUVs[1].y;

    //    mesh.uv = meshUVs;
    //    transform.localScale = Size.y > Grid.TILE_RESOLUTION ? SIZE_TALL : SIZE_DEFAULT;
    //    transform.localPosition = new Vector3(_tile.WorldPosition.x, _tile.WorldPosition.y + ((Size.y - SIZE_DEFAULT.y) * 0.25f), _tile.WorldPosition.z);

    //    //Debug.DrawLine((transform.position + (Vector3)meshUVs[0] - new Vector3(0.5f, 0.5f, 0)), (transform.position + (Vector3)meshUVs[2] - new Vector3(0.5f, 0.5f, 0)), Color.red);
    //    //Debug.DrawLine((transform.position + (Vector3)meshUVs[2] - new Vector3(0.5f, 0.5f, 0)), (transform.position + (Vector3)meshUVs[1] - new Vector3(0.5f, 0.5f, 0)), Color.red);
    //    //Debug.DrawLine((transform.position + (Vector3)meshUVs[1] - new Vector3(0.5f, 0.5f, 0)), (transform.position + (Vector3)meshUVs[3] - new Vector3(0.5f, 0.5f, 0)), Color.red);
    //    //Debug.DrawLine((transform.position + (Vector3)meshUVs[3] - new Vector3(0.5f, 0.5f, 0)), (transform.position + (Vector3)meshUVs[0] - new Vector3(0.5f, 0.5f, 0)), Color.red);
    //}
}
