using UnityEngine;
using System.Collections;

public class UVController : MonoBehaviour {

    public Tile.TileType Type;
    public Tile.TileOrientation Orientation;
    public bool IsBottom;

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
        hasStarted = true;

        myMeshFilter = GetComponent<MeshFilter>();
        myRenderer = GetComponent<MeshRenderer>();
        myMeshUVs = myMeshFilter.mesh.uv;

        cachedPropertyColor = Shader.PropertyToID("_Color");
    }

	public bool HaveChangedGraphics = false;
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
        //myMeshFilter.transform.localScale = _size.y > Grid.TILE_RESOLUTION ? SIZE_TALL : SIZE_DEFAULT;
        //myMeshFilter.transform.localPosition = new Vector3(WorldPosition.x, WorldPosition.y + ((_size.y - SIZE_DEFAULT.y) * 0.25f), WorldPosition.z);

        //Debug.DrawLine((transform.position + (Vector3)meshUVs[0] - new Vector3(0.5f, 0.5f, 0)), (transform.position + (Vector3)meshUVs[2] - new Vector3(0.5f, 0.5f, 0)), Color.red);
        //Debug.DrawLine((transform.position + (Vector3)meshUVs[2] - new Vector3(0.5f, 0.5f, 0)), (transform.position + (Vector3)meshUVs[1] - new Vector3(0.5f, 0.5f, 0)), Color.red);
        //Debug.DrawLine((transform.position + (Vector3)meshUVs[1] - new Vector3(0.5f, 0.5f, 0)), (transform.position + (Vector3)meshUVs[3] - new Vector3(0.5f, 0.5f, 0)), Color.red);
        //Debug.DrawLine((transform.position + (Vector3)meshUVs[3] - new Vector3(0.5f, 0.5f, 0)), (transform.position + (Vector3)meshUVs[0] - new Vector3(0.5f, 0.5f, 0)), Color.red);
    }

    public void ChangeColor(Color _color) {
        myRenderer.material.SetColor(cachedPropertyColor, _color);
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
