using UnityEngine;
using System.Linq;
using System.Collections.Generic;
public class UVControllerBasic : MeshSorter{

	public const int MESH_VERTEXCOUNT = 14;
	public static readonly int GRID_LAYER_COUNT = System.Enum.GetNames(typeof(GridLayerEnum)).Length;

	protected const int UVCHANNEL_UV0 = 0;
	protected const int UVCHANNEL_UV1 = 1;
	protected const int UVCHANNEL_COLOR = 2;
	protected const int UVCHANNEL_DOUBLEDOT = 3;

	public bool TotallySetTheBool = false;

	[System.Serializable]
	public class GridLayerClass{
		public Vector2i Coordinates = new Vector2i();
		public Vector2i TemporaryCoordinates = new Vector2i();
		public Tile.TileOrientation Orientation; // only used for Bottom and Floor currently
		public Vector2[] UVs = new Vector2[MESH_VERTEXCOUNT];
	}
	public GridLayerClass[] GridLayers = new GridLayerClass[GRID_LAYER_COUNT];
	public List<Vector4> CompressedUVs_0 = new List<Vector4>(MESH_VERTEXCOUNT);
	public List<Vector4> CompressedUVs_1 = new List<Vector4>(MESH_VERTEXCOUNT);
	protected bool shouldApplyChanges = false;

	[System.NonSerialized] public MeshFilter MyMeshFilter;

	protected static bool sHasSetShaderProperties = false;
	protected bool isHidden = false;

	[EasyButtons.Button]
	public void UpdateCurrentGraphics(){
		for (int i = 0; i < GridLayers.Length; i++)
			ChangeAsset((GridLayerEnum)i, GridLayers[i].Coordinates, false);
		ApplyAssetChanges();
	}

	void Awake(){
		SortingLayer = MeshSorter.SortingLayerEnum.Grid;
	}

	public override void Setup(){
		base.Setup();

		MyMeshFilter = GetComponent<MeshFilter>();
		if (MyMeshFilter.sharedMesh == null)
			MyMeshFilter.sharedMesh = GenerateMesh();

		if (!sHasSetShaderProperties){
			myRenderer.sharedMaterial.SetVectorArray("allColors", ColoringTool.sAllColorsForShaders);
			myRenderer.sharedMaterial.SetInt("TextureSizeX", CachedAssets.WallSet.TEXTURE_SIZE_X);
			myRenderer.sharedMaterial.SetInt("TextureSizeY", CachedAssets.WallSet.TEXTURE_SIZE_Y);
			sHasSetShaderProperties = true;
		}
	}
	Mesh GenerateMesh(){
		Mesh mesh = new Mesh();
		mesh.vertices = new Vector3[] { // imagine it upside down, and that's basically the mesh below
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
			new Vector2(1, 0.5f),   new Vector2(0.5f, 0.5f),    new Vector2(0, 0.5f),
			new Vector2(1, 1),                                  new Vector2(0, 1)
		};
		mesh.triangles = new int[]{
			6, 4, 3,
			3, 4, 0,
			0, 4, 1,
			1, 4, 2,
			2, 4, 5,
			5, 4, 8,
			8, 4, 7,
			7, 4, 6,

			6, 10, 9,
			9, 10, 8,
			8, 10, 11,
			11, 10, 13,
			13, 10, 12,
			12, 10, 9
		};

		return mesh;
	}

	public virtual void StopTempMode(){
		for (int i = 0; i < GridLayers.Length; i++){
			GridLayers[i].TemporaryCoordinates.x = 0;
			GridLayers[i].TemporaryCoordinates.y = 0;
			ChangeAsset((GridLayerEnum)i, GridLayers[i].Coordinates, false);
		}
	}
	private delegate void SetUV(int _index, float _x, float _y);
	public virtual void ChangeAsset(GridLayerEnum _layer, Vector2i _assetCoordinates, bool _temporary){
		if (!Application.isPlaying)
			return;
		if (_assetCoordinates == null)
			Debug.Log("_assetCoordinates was null!");

		int _layerIndex = (int)_layer;

		if (_temporary)
			GridLayers[_layerIndex].TemporaryCoordinates = _assetCoordinates;
		else{
			GridLayers[_layerIndex].Coordinates = _assetCoordinates;
		}

		float _uvL = Tile.RESOLUTION * _assetCoordinates.x;
		float _uvR = Tile.RESOLUTION * (_assetCoordinates.x + 1);
		float _uvB = Tile.RESOLUTION * _assetCoordinates.y;
		float _uvT = Tile.RESOLUTION * (_assetCoordinates.y + 2);

		float _halfX = (_uvR - _uvL) * 0.5f;
		float _halfY = (_uvT - _uvB) * 0.5f;
		float _quarterY = _halfY * 0.5f;

		SetUV _SetUV = delegate (int _index, float _x, float _y){
			GridLayers[_layerIndex].UVs[_index].x = _x;
			GridLayers[_layerIndex].UVs[_index].y = _y;
		};
		_SetUV(0, _uvL, _uvB); _SetUV(1, _uvL + _halfX, _uvB); _SetUV(2, _uvR, _uvB);
		_SetUV(3, _uvL, _uvB + _quarterY); _SetUV(4, _uvL + _halfX, _uvB + _quarterY); _SetUV(5, _uvR, _uvB + _quarterY);
		_SetUV(6, _uvL, _uvB + _halfY); _SetUV(7, _uvL + _halfX, _uvB + _halfY); _SetUV(8, _uvR, _uvB + _halfY);
		_SetUV(9, _uvL, _uvB + _halfY); _SetUV(10, _uvL + _halfX, _uvB + _halfY); _SetUV(11, _uvR, _uvB + _halfY);
		_SetUV(12, _uvL, _uvT); _SetUV(13, _uvR, _uvT);

		if (GridLayers.Length != 5)
			Debug.LogError("The amount of grid layers doesn't match the loop below! Not sure how to softcode this sadly!");
		CompressedUVs_0.Clear();
		CompressedUVs_1.Clear();
		Vector4 _compressed0 = new Vector4();
		Vector4 _compressed1 = new Vector4();
		for (int i = 0; i < MESH_VERTEXCOUNT; i++){
			_compressed0.x = BitCompressor.Int2ToInt((int)(GridLayers[0].UVs[i].x), (int)(GridLayers[0].UVs[i].y));
			_compressed0.y = BitCompressor.Int2ToInt((int)(GridLayers[1].UVs[i].x), (int)(GridLayers[1].UVs[i].y));
			_compressed0.z = BitCompressor.Int2ToInt((int)(GridLayers[2].UVs[i].x), (int)(GridLayers[2].UVs[i].y));
			_compressed0.w = BitCompressor.Int2ToInt((int)(GridLayers[3].UVs[i].x), (int)(GridLayers[3].UVs[i].y));

			_compressed1.x = BitCompressor.Int2ToInt((int)(GridLayers[4].UVs[i].x), (int)(GridLayers[4].UVs[i].y));
			_compressed1.y = BitCompressor.Int2ToInt(0, 0);
			_compressed1.z = BitCompressor.Int2ToInt(0, 0);
			_compressed1.w = BitCompressor.Int2ToInt(0, 0);

			CompressedUVs_0.Add(_compressed0);
			CompressedUVs_1.Add(_compressed1);
		}

		shouldApplyChanges = true;
	}
	void LateUpdate(){
		if (shouldApplyChanges){
			ApplyAssetChanges();
			shouldApplyChanges = false;
		}
	}
	protected virtual void ApplyAssetChanges(){
		MyMeshFilter.mesh.SetUVs(UVCHANNEL_UV0, CompressedUVs_0);
		MyMeshFilter.mesh.SetUVs(UVCHANNEL_UV1, CompressedUVs_1);
		myRenderer.enabled = !isHidden;
	}
	public void Hide(bool _b){
		if (!hasStarted)
			Setup();

		myRenderer.enabled = !_b;
		isHidden = _b;
	}
}