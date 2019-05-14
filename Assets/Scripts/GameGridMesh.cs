using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class GameGridMesh {

	public static Material GridMaterial;

	private const int VERTICES_PER_TILE = 5;
	private const int TRIS_PER_TILE = 4;

	public const int VERTEX_INDEX_BOTTOM_LEFT = 0;
	public const int VERTEX_INDEX_TOP_LEFT = 1;
	public const int VERTEX_INDEX_TOP_RIGHT = 2;
	public const int VERTEX_INDEX_BOTTOM_RIGHT = 3;
	public const int VERTEX_INDEX_CENTER = 4;

	private const int UVCHANNEL_UV = 0;
	private const int UVCHANNEL_COLOR = 1;

	// TODO: why are these static?
	private static List<Color32> lightingData;
	private static List<Vector4> chemVertexData_0;
	private static List<Vector3> chemVertexData_1;
	private static byte[] appliedColorIndices;
	private static List<Vector4> compressedColorIndices;
	
	private Texture2D chemStatesTex;
	private Color[]   chemStatesTexPixels;

	[SerializeField] private Transform transform;

	public enum RenderMode { Walls, Interactives }
	private RenderMode renderMode;

	private Sorting sorting;
	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;
	private Mesh mesh;
	private List<Vector3> uvsAndChemicals = new List<Vector3>();

	private Queue<Int2> tilesToUpdateAssetFor = new Queue<Int2>();
	private Queue<Int2> nodesWithNewChemicalData= new Queue<Int2>();
	private bool isColorDirty = false;
	private bool isLightingDirty = false;
	private bool isUVOrChemicalsDirty = false;
	private bool isChemicalsDirty = false;


	public static void InitStatic() {
		int _verticesPerGrid = GameGrid.SIZE.x * GameGrid.SIZE.y * VERTICES_PER_TILE;
		
		lightingData = new List<Color32>(_verticesPerGrid);
		chemVertexData_0 = new List<Vector4>(_verticesPerGrid);
		chemVertexData_1 = new List<Vector3>(_verticesPerGrid);

		appliedColorIndices = new byte[GameGrid.SIZE.x * GameGrid.SIZE.y * ColorManager.COLOR_CHANNEL_COUNT];
		compressedColorIndices = new List<Vector4>(GameGrid.SIZE.x * GameGrid.SIZE.y * VERTICES_PER_TILE);
		
		for(int i = 0; i < _verticesPerGrid; i++) {
			lightingData.Add(new Color32());
			chemVertexData_0.Add(new Vector4());
			chemVertexData_1.Add(new Vector3());
			
			compressedColorIndices.Add(new Vector4());
		}
	}

	public void Init(Sorting _sorting, RenderMode _renderMode) {
		sorting = _sorting;
		renderMode = _renderMode;

		meshFilter = transform.GetComponent<MeshFilter>();
		meshRenderer = transform.GetComponent<MeshRenderer>();

		uvsAndChemicals = new List<Vector3>();
		for(int i = 0; i < GameGrid.SIZE.x * GameGrid.SIZE.y * VERTICES_PER_TILE; i++) {
			uvsAndChemicals.Add(new Vector3());
		}
		
		chemStatesTex = new Texture2D(GameGrid.SIZE.x, GameGrid.SIZE.y, TextureFormat.RHalf, mipmap: false);
		chemStatesTexPixels = new Color[chemStatesTex.width * chemStatesTex.height];
	}

	public void CreateMesh() {
		mesh = new Mesh();
		Vector3[] _verts = new Vector3[GameGrid.SIZE.x * GameGrid.SIZE.y * VERTICES_PER_TILE];
		int[] _tris = new int[GameGrid.SIZE.x * GameGrid.SIZE.y * TRIS_PER_TILE * 3];

		Vector3 _originWorldPos = transform.position - new Vector3(GameGrid.SIZE.x * GameGrid.TILE_DIAMETER * 0.5f, GameGrid.SIZE.y * GameGrid.TILE_DIAMETER * 0.5f, 0.0f);

		int _vertexIndex = 0;
		int _tileIndex = 0;
		for(int _y = 0; _y < GameGrid.SIZE.y; _y++) {
			for(int _x = 0; _x < GameGrid.SIZE.x; _x++) {
				int _vertexIndexBL = _vertexIndex + VERTEX_INDEX_BOTTOM_LEFT;
				int _vertexIndexTL = _vertexIndex + VERTEX_INDEX_TOP_LEFT;
				int _vertexIndexTR = _vertexIndex + VERTEX_INDEX_TOP_RIGHT;
				int _vertexIndexBR = _vertexIndex + VERTEX_INDEX_BOTTOM_RIGHT;
				int _vertexIndexM = _vertexIndex + VERTEX_INDEX_CENTER;

				_verts[_vertexIndexBL] = _originWorldPos + new Vector3(_x, _y, 0.0f);
				_verts[_vertexIndexTL] = _originWorldPos + new Vector3(_x, _y + GameGrid.TILE_DIAMETER, 0.0f);
				_verts[_vertexIndexTR] = _originWorldPos + new Vector3(_x + GameGrid.TILE_DIAMETER, _y + GameGrid.TILE_DIAMETER, 0.0f);
				_verts[_vertexIndexBR] = _originWorldPos + new Vector3(_x + GameGrid.TILE_DIAMETER, _y, 0.0f);
				_verts[_vertexIndexM] = _originWorldPos + new Vector3(_x + GameGrid.TILE_DIAMETER * 0.5f, _y + GameGrid.TILE_DIAMETER * 0.5f, 0.0f);

				_vertexIndex += VERTICES_PER_TILE;

				_tris[_tileIndex + 0] = _vertexIndexBL;
				_tris[_tileIndex + 1] = _vertexIndexTL;
				_tris[_tileIndex + 2] = _vertexIndexM;

				_tris[_tileIndex + 3] = _vertexIndexM;
				_tris[_tileIndex + 4] = _vertexIndexTL;
				_tris[_tileIndex + 5] = _vertexIndexTR;

				_tris[_tileIndex + 6] = _vertexIndexTR;
				_tris[_tileIndex + 7] = _vertexIndexBR;
				_tris[_tileIndex + 8] = _vertexIndexM;

				_tris[_tileIndex + 9] = _vertexIndexM;
				_tris[_tileIndex + 10] = _vertexIndexBR;
				_tris[_tileIndex + 11] = _vertexIndexBL;

				_tileIndex += TRIS_PER_TILE * 3;
			}
		}

		mesh.vertices = _verts;
		mesh.triangles = _tris;
		mesh.RecalculateBounds();
		mesh.RecalculateNormals();
		meshFilter.mesh = mesh;

		meshRenderer.material = GridMaterial;
	}

	public void ClearTemporaryColor(Int2 _tileGridPos) {
		int _index =(_tileGridPos.y * GameGrid.SIZE.x + _tileGridPos.x) * ColorManager.COLOR_CHANNEL_COUNT;
		byte[] _appliedColorIndicesForTile = new byte[ColorManager.COLOR_CHANNEL_COUNT] {
			appliedColorIndices[_index + 0],
				appliedColorIndices[_index + 1],
				appliedColorIndices[_index + 2],
				appliedColorIndices[_index + 3],
				appliedColorIndices[_index + 4],
				appliedColorIndices[_index + 5],
				appliedColorIndices[_index + 6],
				appliedColorIndices[_index + 7],
				appliedColorIndices[_index + 8],
				appliedColorIndices[_index + 9]
		};

		SetColor(_tileGridPos, _appliedColorIndicesForTile, _isPermanent : true);
	}

	public void SetColor(Int2 _tileGridPos, byte _colorIndex, bool _isPermanent) {
		byte[] _colorIndexAsArray = new byte[ColorManager.COLOR_CHANNEL_COUNT] {
			_colorIndex,
			_colorIndex,
			_colorIndex,
			_colorIndex,
			_colorIndex,
			_colorIndex,
			_colorIndex,
			_colorIndex,
			_colorIndex,
			_colorIndex,
		};

		SetColor(_tileGridPos, _colorIndexAsArray, _isPermanent);
	}

	public void SetColor(Int2 _tileGridPos, byte[] _colorIndices, bool _isPermanent) {
		if(_colorIndices.Length != ColorManager.COLOR_CHANNEL_COUNT) {
			Debug.LogError("_colorChannelIndices has a different length than what is supported!");
			return;
		}

		if(_isPermanent) {
			int _appliedColorIndex =(_tileGridPos.y * GameGrid.SIZE.x + _tileGridPos.x) * _colorIndices.Length;
			for(int i = 0; i < _colorIndices.Length; i++) {
				appliedColorIndices[_appliedColorIndex + i] = _colorIndices[i];
			}
		}

		// WARNING: using more than 3 bytes per int will cause instability when cast to float!
		Vector4 _newCompressedColorIndices = new Vector4(
			_newCompressedColorIndices.x = BitCompressor.ThreeBytesToFloat32(_colorIndices[0], _colorIndices[1], _colorIndices[2]),
			_newCompressedColorIndices.y = BitCompressor.ThreeBytesToFloat32(_colorIndices[3], _colorIndices[4], _colorIndices[5]),
			_newCompressedColorIndices.z = BitCompressor.ThreeBytesToFloat32(_colorIndices[6], _colorIndices[7], _colorIndices[8]),
			0
		);

		int _vertexIndex =(_tileGridPos.y * GameGrid.SIZE.x + _tileGridPos.x) * VERTICES_PER_TILE;
		for(int i = 0; i < VERTICES_PER_TILE; i++) {
			compressedColorIndices[_vertexIndex + i] = _newCompressedColorIndices;
		}

		isColorDirty = true;
	}

	public void SetLighting(Int2 _tileGridPos, int _vertexIndex, Color32 _lighting) {
		if(_vertexIndex == VERTEX_INDEX_CENTER) {
			Debug.LogError("Tried to set lighting for a tile's center vertex! This is done automatically!");
		}

		int _vertexIndexInGrid = GetVertexIndexInGrid(_tileGridPos, _vertexIndex);
		lightingData[_vertexIndexInGrid] = _lighting;

		int _centerVertexIndexInGrid = GetVertexIndexInGrid(_tileGridPos, VERTEX_INDEX_CENTER);
		Color32 _average = GetAverageColorInCornerVertices(_tileGridPos);
		lightingData[_centerVertexIndexInGrid] = _average;

		isLightingDirty = true;
	}

	int GetVertexIndexInGrid(Int2 _tileGridPos, int _vertexIndex) {
		return _tileGridPos.y * GameGrid.SIZE.x * VERTICES_PER_TILE + _tileGridPos.x * VERTICES_PER_TILE + _vertexIndex;
	}

	Color32 GetAverageColorInCornerVertices(Int2 _tileGridPos) {
		Color32 _colorBL = lightingData[GetVertexIndexInGrid(_tileGridPos, VERTEX_INDEX_BOTTOM_LEFT)];
		Color32 _colorTL = lightingData[GetVertexIndexInGrid(_tileGridPos, VERTEX_INDEX_TOP_LEFT)];
		Color32 _colorTR = lightingData[GetVertexIndexInGrid(_tileGridPos, VERTEX_INDEX_TOP_RIGHT)];
		Color32 _colorBR = lightingData[GetVertexIndexInGrid(_tileGridPos, VERTEX_INDEX_BOTTOM_RIGHT)];

		return new Color32(
			(byte)((_colorBL.r + _colorTL.r + _colorTR.r + _colorBR.r) / 4.0f),
			(byte)((_colorBL.g + _colorTL.g + _colorTR.g + _colorBR.g) / 4.0f),
			(byte)((_colorBL.b + _colorTL.b + _colorTR.b + _colorBR.b) / 4.0f),
			(byte)((_colorBL.a + _colorTL.a + _colorTR.a + _colorBR.a) / 4.0f)
		);
	}

	public void ScheduleUpdateForTileAsset(Int2 _tileGridPos) {
		tilesToUpdateAssetFor.Enqueue(_tileGridPos);
	}
	
	public void ScheduleCacheChemicalData(Int2 _nodeGridPos) {
		nodesWithNewChemicalData.Enqueue(_nodeGridPos);
	}

	public void TryUpdateVisuals() {
		while(tilesToUpdateAssetFor.Count > 0) {
			Int2 _tileGridPos = tilesToUpdateAssetFor.Dequeue();
			Int2 _asset = new Int2();

			switch(renderMode) {
				case RenderMode.Walls:
					_asset = CachedAssets.Instance.GetWallAsset(_tileGridPos, sorting);
					break;
				case RenderMode.Interactives:
					_asset = CachedAssets.Instance.GetInteractiveAsset(_tileGridPos, sorting);
					break;
				default:
					Debug.LogError(renderMode + " hasn't been properly implemented yet!");
					break;
			}

			SetAsset(_tileGridPos, _asset);
		}
		
		while(nodesWithNewChemicalData.Count > 0) {
			CacheChemicalData(nodesWithNewChemicalData.Dequeue());
		}

		if(isUVOrChemicalsDirty) {
			isUVOrChemicalsDirty = false;
			mesh.SetUVs(UVCHANNEL_UV, uvsAndChemicals);
		}

		if(isColorDirty) {
			isColorDirty = false;
			mesh.SetUVs(UVCHANNEL_COLOR, compressedColorIndices);
		}

		if(isLightingDirty) {
			isLightingDirty = false;
			mesh.SetColors(lightingData);
		}

		if (isChemicalsDirty) {
			isChemicalsDirty = false;
			CacheChemicalDataForAllCenterVertices();
			chemStatesTex.SetPixels(chemStatesTexPixels);
			mesh.SetTangents(chemVertexData_0);
			mesh.SetNormals(chemVertexData_1);
		}
	}

	void SetAsset(Int2 _tileGridPos, Int2 _assetPos) {
		Texture2D texture = CachedAssets.Instance.DefaultAssets.SpriteSheet;
		float _newUVB =(GameGrid.TILE_RESOLUTION *(_assetPos.y + 0.0f)) /(texture.height);
		float _newUVMY =(GameGrid.TILE_RESOLUTION *(_assetPos.y + 0.5f)) /(texture.height);
		float _newUVT =(GameGrid.TILE_RESOLUTION *(_assetPos.y + 1.0f)) /(texture.height);

		float _newUVL =(GameGrid.TILE_RESOLUTION *(_assetPos.x + 0.0f)) /(texture.width);
		float _newUVMX =(GameGrid.TILE_RESOLUTION *(_assetPos.x + 0.5f)) /(texture.width);
		float _newUVR =(GameGrid.TILE_RESOLUTION *(_assetPos.x + 1.0f)) /(texture.width);

		int _uvStartIndex =(_tileGridPos.y * GameGrid.SIZE.x + _tileGridPos.x) * VERTICES_PER_TILE;

		Vector3 _uvBL = uvsAndChemicals[_uvStartIndex + 0];
		Vector3 _uvTL = uvsAndChemicals[_uvStartIndex + 1];
		Vector3 _uvTR = uvsAndChemicals[_uvStartIndex + 2];
		Vector3 _uvBR = uvsAndChemicals[_uvStartIndex + 3];
		Vector3 _uvM = uvsAndChemicals[_uvStartIndex + 4];

		_uvBL.x = _newUVL;
		_uvBL.y = _newUVB;

		_uvTL.x = _newUVL;
		_uvTL.y = _newUVT;

		_uvTR.x = _newUVR;
		_uvTR.y = _newUVT;

		_uvBR.x = _newUVR;
		_uvBR.y = _newUVB;

		_uvM.x = _newUVMX;
		_uvM.y = _newUVMY;

		uvsAndChemicals[_uvStartIndex + 0] = _uvBL;
		uvsAndChemicals[_uvStartIndex + 1] = _uvTL;
		uvsAndChemicals[_uvStartIndex + 2] = _uvTR;
		uvsAndChemicals[_uvStartIndex + 3] = _uvBR;
		uvsAndChemicals[_uvStartIndex + 4] = _uvM;

		isUVOrChemicalsDirty = true;
	}

	void CacheChemicalData(Int2 _nodeGridPos) {
		// TODO: need to sort Contents
		ChemicalContainer _chemicalContainer = GameGrid.GetInstance().TryGetNode(_nodeGridPos).ChemicalContainer;
		Chemical.Blob _chem_0 = _chemicalContainer.Contents[0];
		Chemical.Blob _chem_1 = _chemicalContainer.Contents[1];
		Chemical.Blob _chem_2 = _chemicalContainer.Contents[2];
		
		// Vertex data
		int _vertexIndexBL = ((_nodeGridPos.y) * GameGrid.SIZE.x + (_nodeGridPos.x)) * VERTICES_PER_TILE + VERTEX_INDEX_TOP_RIGHT;
		int _vertexIndexBR = ((_nodeGridPos.y) * GameGrid.SIZE.x + (_nodeGridPos.x + 1)) * VERTICES_PER_TILE + VERTEX_INDEX_TOP_LEFT;
		int _vertexIndexTL = ((_nodeGridPos.y + 1) * GameGrid.SIZE.x + (_nodeGridPos.x)) * VERTICES_PER_TILE + VERTEX_INDEX_BOTTOM_RIGHT;
		int _vertexIndexTR = ((_nodeGridPos.y + 1) * GameGrid.SIZE.x + (_nodeGridPos.x + 1)) * VERTICES_PER_TILE + VERTEX_INDEX_BOTTOM_LEFT;

		float _amount_0 = _chem_0.Amount / _chemicalContainer.MaxAmount;
		float _amount_1 = _chem_1.Amount / _chemicalContainer.MaxAmount;
		float _amount_2 = _chem_2.Amount / _chemicalContainer.MaxAmount;
		float _temperature = _chemicalContainer.Temperature / Chemical.MAX_TEMPERATURE;
		
		chemVertexData_0[_vertexIndexBL].Set(_amount_0, _amount_1, _amount_2, _temperature);
		chemVertexData_0[_vertexIndexBR].Set(_amount_0, _amount_1, _amount_2, _temperature);
		chemVertexData_0[_vertexIndexTL].Set(_amount_0, _amount_1, _amount_2, _temperature);
		chemVertexData_0[_vertexIndexTR].Set(_amount_0, _amount_1, _amount_2, _temperature);
		
		chemVertexData_1[_vertexIndexBL].Set(_chem_0.GetCurrentColor(), _chem_1.GetCurrentColor(), _chem_2.GetCurrentColor());
		chemVertexData_1[_vertexIndexBR].Set(_chem_0.GetCurrentColor(), _chem_1.GetCurrentColor(), _chem_2.GetCurrentColor());
		chemVertexData_1[_vertexIndexTL].Set(_chem_0.GetCurrentColor(), _chem_1.GetCurrentColor(), _chem_2.GetCurrentColor());
		chemVertexData_1[_vertexIndexTR].Set(_chem_0.GetCurrentColor(), _chem_1.GetCurrentColor(), _chem_2.GetCurrentColor());
		//
		
		// Texture data
		byte _state_0 = (byte) _chem_0.GetState();
		byte _state_1 = (byte) _chem_1.GetState();
		byte _state_2 = (byte) _chem_2.GetState();
		Debug.Log((_nodeGridPos.y * GameGrid.SIZE.x + _nodeGridPos.x) + ", " + chemStatesTexPixels.Length);
		chemStatesTexPixels[_nodeGridPos.y * GameGrid.SIZE.x + _nodeGridPos.x] = new Color(BitCompressor.FourHalfBytesToFloat16(_state_0, _state_1, _state_2, 0), 0.0f, 0.0f, 0.0f);
		//
	}

	void CacheChemicalDataForAllCenterVertices() {
		int _vertexCount = GameGrid.SIZE.x * GameGrid.SIZE.y * VERTICES_PER_TILE;
		for(int i = 0; i < _vertexCount; i += VERTICES_PER_TILE) {
			int _index = i + VERTEX_INDEX_CENTER;
			int _indexBL = i + VERTEX_INDEX_BOTTOM_LEFT;
			int _indexTL = i + VERTEX_INDEX_TOP_LEFT;
			int _indexTR = i + VERTEX_INDEX_TOP_RIGHT;
			int _indexBR = i + VERTEX_INDEX_BOTTOM_RIGHT;
			
			Vector4 _amountAndTemperatureBL = chemVertexData_0[_indexBL];
			Vector4 _amountAndTemperatureTL = chemVertexData_0[_indexTL];
			Vector4 _amountAndTemperatureTR = chemVertexData_0[_indexTR];
			Vector4 _amountAndTemperatureBR = chemVertexData_0[_indexBR];
			chemVertexData_0[_index] = new Vector4(
				(_amountAndTemperatureBL.x + _amountAndTemperatureTL.x + _amountAndTemperatureTR.x + _amountAndTemperatureBR.x) * 0.25f,
				(_amountAndTemperatureBL.y + _amountAndTemperatureTL.y + _amountAndTemperatureTR.y + _amountAndTemperatureBR.y) * 0.25f,
				(_amountAndTemperatureBL.z + _amountAndTemperatureTL.z + _amountAndTemperatureTR.z + _amountAndTemperatureBR.z) * 0.25f,
				(_amountAndTemperatureBL.w + _amountAndTemperatureTL.w + _amountAndTemperatureTR.w + _amountAndTemperatureBR.w) * 0.25f
			);
			
			Vector3 _colorBL = chemVertexData_1[_indexBL];
			Vector3 _colorTL = chemVertexData_1[_indexTL];
			Vector3 _colorTR = chemVertexData_1[_indexTR];
			Vector3 _colorBR = chemVertexData_1[_indexBR];
			chemVertexData_1[_index] = new Vector3(
				(_colorBL.x + _colorTL.x + _colorTR.x + _colorBR.x) * 0.25f,
				(_colorBL.y + _colorTL.y + _colorTR.y + _colorBR.y) * 0.25f,
				(_colorBL.z + _colorTL.z + _colorTR.z + _colorBR.z) * 0.25f
			);
		}
	}
}