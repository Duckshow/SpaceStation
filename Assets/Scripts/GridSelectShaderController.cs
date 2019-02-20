using UnityEngine;
using System.Collections;

public class GridSelectShaderController : MonoBehaviour {

    public static GridSelectShaderController Instance;
    private MeshRenderer rend;
    private Vector4 min;
    private Vector4 max;
    private Material selectMat;
    private int propertyID1;
    private int propertyID2;


    void Awake() {
        Instance = this;
        rend = GetComponent<MeshRenderer>();
        selectMat = rend.material;

        propertyID1 = Shader.PropertyToID("_Pos1");
        propertyID2 = Shader.PropertyToID("_Pos2");
        min = new Vector4();
        max = new Vector4();
    }

    void Start() {
		Int2 resolution = new Int2();
		resolution.x = GameGrid.SIZE.x * GameGrid.TILE_RESOLUTION;
		resolution.y = GameGrid.SIZE.y * GameGrid.TILE_RESOLUTION;

		rend.material.mainTexture = new Texture2D(resolution.x, resolution.y, TextureFormat.RGBA32, false);
        rend.material.mainTexture.wrapMode = TextureWrapMode.Clamp;
        rend.material.mainTexture.filterMode = FilterMode.Point;
        rend.transform.localScale = new Vector3(GameGrid.SIZE.x, GameGrid.SIZE.y, 1);
    }

    public void SetSelectSize(Node _tile1, Node _tile2) {
        min.x = Mathf.Min(_tile1.GridPos.x, _tile2.GridPos.x);
        min.y = Mathf.Min(_tile1.GridPos.y, _tile2.GridPos.y);
        max.x = Mathf.Max(_tile1.GridPos.x, _tile2.GridPos.x);
        max.y = Mathf.Max(_tile1.GridPos.y, _tile2.GridPos.y);

        selectMat.SetVector(propertyID1, min);
        selectMat.SetVector(propertyID2, max);
    }
}
