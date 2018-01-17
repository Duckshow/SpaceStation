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
        rend.material.mainTexture = new Texture2D((int)Grid.Instance.GridWorldSize.x * Tile.RESOLUTION, (int)Grid.Instance.GridWorldSize.y * Tile.RESOLUTION, TextureFormat.RGBA32, false);
        rend.material.mainTexture.wrapMode = TextureWrapMode.Clamp;
        rend.material.mainTexture.filterMode = FilterMode.Point;
        rend.transform.localScale = new Vector3((int)Grid.Instance.GridWorldSize.x, (int)Grid.Instance.GridWorldSize.y, 1);
    }

    public void SetSelectSize(Tile _tile1, Tile _tile2) {
        min.x = Mathf.Min(_tile1.GridCoord.x, _tile2.GridCoord.x);
        min.y = Mathf.Min(_tile1.GridCoord.y, _tile2.GridCoord.y);
        max.x = Mathf.Max(_tile1.GridCoord.x, _tile2.GridCoord.x);
        max.y = Mathf.Max(_tile1.GridCoord.y, _tile2.GridCoord.y);

        selectMat.SetVector(propertyID1, min);
        selectMat.SetVector(propertyID2, max);
    }
}
