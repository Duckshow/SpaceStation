using System.Collections;
using System.Collections.Generic;
using System.Runtime.Remoting.Channels;
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
	// private const int UVCHANNEL_CHEM_AMOUNTS_AND_TEMPERATURE = 2;
	// private const int UVCHANNEL_CHEMCOLORINDICES = 3;

	private static readonly int PROPERTY_ID_CHEMAMOUNTSANDTEMPERATURETEX = Shader.PropertyToID("_ChemAmountsAndTemperatureTex");
	private static readonly int PROPERTY_ID_CHEMCOLORSTEX = Shader.PropertyToID("_ChemColorsTex");
	private static readonly int PROPERTY_ID_CHEMSTATESTEX = Shader.PropertyToID("_ChemStatesTex");

	private List<Color32> lightingData;
	// private List<Vector4> chemAmountsAndTemperature;
	// private List<Vector3> chemColorIndices;
	private byte[] appliedColorIndices;
	private List<Vector4> compressedColorIndices;
	// private List<Vector3> chemStates;

	private Texture2D chemAmountsAndTemperatureTex;
	private Texture2D chemColorsTex;
	private Texture2D debugTex;
	private Texture2D chemStatesTex;

	private Color[] chemAmountsAndTemperature;
	private Color[] chemColorIndices;
	private Color[] debugColors;
	private Color[] chemStates;

	[SerializeField] private Transform transform;
	[SerializeField] private bool shouldDisplayChemicals;

	public enum RenderMode { Walls, Interactives }
	private RenderMode renderMode;

	private Sorting sorting;
	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;
	private Mesh mesh;
	private List<Vector3> uvs = new List<Vector3>();

	private Queue<Int2> tilesToUpdateAssetFor = new Queue<Int2>();
	private Queue<Int2> nodesWithNewChemicalData = new Queue<Int2>();
	private bool isColorDirty = false;
	private bool isLightingDirty = false;
	private bool isUVOrChemicalsDirty = false;
	private bool isChemicalsDirty = false;

	public void Init(Sorting _sorting, RenderMode _renderMode) {
		sorting = _sorting;
		renderMode = _renderMode;

		meshFilter = transform.GetComponent<MeshFilter>();
		meshRenderer = transform.GetComponent<MeshRenderer>();

		int _verticesPerGrid = GameGrid.TILE_COUNT.x * GameGrid.TILE_COUNT.y * VERTICES_PER_TILE;

		lightingData = new List<Color32>(_verticesPerGrid);
		// chemAmountsAndTemperature = new List<Vector4>(_verticesPerGrid);
		// chemColorIndices = new List<Vector3>(_verticesPerGrid);
		// chemStates = new List<Vector3>(_verticesPerGrid);
		uvs = new List<Vector3>();

		chemAmountsAndTemperatureTex = new Texture2D(GameGrid.SIZE.x, GameGrid.SIZE.y, TextureFormat.RGBAFloat, mipmap : false);
		chemAmountsAndTemperatureTex.filterMode = FilterMode.Bilinear;
		chemAmountsAndTemperatureTex.wrapMode = TextureWrapMode.Clamp;

		chemColorsTex = new Texture2D(GameGrid.SIZE.x, GameGrid.SIZE.y, TextureFormat.RGBA32, mipmap : false);
		chemColorsTex.filterMode = FilterMode.Point;
		chemColorsTex.wrapMode = TextureWrapMode.Clamp;

		debugTex= new Texture2D(GameGrid.SIZE.x, GameGrid.SIZE.y, TextureFormat.RGBA32, mipmap : false);
		debugTex.filterMode = FilterMode.Point;
		debugTex.wrapMode   = TextureWrapMode.Clamp;

		chemStatesTex = new Texture2D(GameGrid.SIZE.x, GameGrid.SIZE.y, TextureFormat.RGBAFloat, mipmap : false);
		chemStatesTex.filterMode = FilterMode.Point;
		chemStatesTex.wrapMode = TextureWrapMode.Clamp;

		int _nodesPerGrid = GameGrid.SIZE.x * GameGrid.SIZE.y;
		chemAmountsAndTemperature = new Color[_nodesPerGrid];
		chemColorIndices = new Color[_nodesPerGrid];
		debugColors = new Color[_nodesPerGrid];
		chemStates = new Color[_nodesPerGrid];

		appliedColorIndices = new byte[GameGrid.TILE_COUNT.x * GameGrid.TILE_COUNT.y * ColorManager.COLOR_CHANNEL_COUNT];
		compressedColorIndices = new List<Vector4>(_verticesPerGrid);

		for(int i = 0; i < _verticesPerGrid; i++) {
			lightingData.Add(new Color32());
			// chemAmountsAndTemperature.Add(new Vector4());
			// chemColorIndices.Add(new Vector3());
			// chemStates.Add(new Vector4());
			uvs.Add(new Vector3());

			compressedColorIndices.Add(new Vector4());
		}
	}

	public void CreateMesh() {
		mesh = new Mesh();
		Vector3[] _verts = new Vector3[GameGrid.TILE_COUNT.x * GameGrid.TILE_COUNT.y * VERTICES_PER_TILE];
		int[] _tris = new int[GameGrid.TILE_COUNT.x * GameGrid.TILE_COUNT.y * TRIS_PER_TILE * 3];

		Vector3 _originWorldPos = transform.position - new Vector3(GameGrid.TILE_COUNT.x * GameGrid.TILE_DIAMETER * 0.5f, GameGrid.TILE_COUNT.y * GameGrid.TILE_DIAMETER * 0.5f, 0.0f);

		int _vertexIndex = 0;
		int _tileIndex = 0;
		for(int _y = 0; _y < GameGrid.TILE_COUNT.y; _y++) {
			for(int _x = 0; _x < GameGrid.TILE_COUNT.x; _x++) {
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
		// mesh.RecalculateNormals();
		meshFilter.mesh = mesh;

		meshRenderer.material = GridMaterial;
	}

	public void ClearTemporaryColor(Int2 _tileGridPos) {
		int _index =(_tileGridPos.y * GameGrid.TILE_COUNT.x + _tileGridPos.x) * ColorManager.COLOR_CHANNEL_COUNT;
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
			int _appliedColorIndex =(_tileGridPos.y * GameGrid.TILE_COUNT.x + _tileGridPos.x) * _colorIndices.Length;
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

		int _vertexIndex =(_tileGridPos.y * GameGrid.TILE_COUNT.x + _tileGridPos.x) * VERTICES_PER_TILE;
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
		return _tileGridPos.y * GameGrid.TILE_COUNT.x * VERTICES_PER_TILE + _tileGridPos.x * VERTICES_PER_TILE + _vertexIndex;
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

		while(shouldDisplayChemicals && nodesWithNewChemicalData.Count > 0) {
			isChemicalsDirty = true;
			CacheChemicalData(nodesWithNewChemicalData.Dequeue());
		}

		if(isUVOrChemicalsDirty) {
			isUVOrChemicalsDirty = false;
			mesh.SetUVs(UVCHANNEL_UV, uvs);
		}

		if(isColorDirty) {
			isColorDirty = false;
			mesh.SetUVs(UVCHANNEL_COLOR, compressedColorIndices);
		}

		if(isLightingDirty) {
			isLightingDirty = false;
			mesh.SetColors(lightingData);
		}

		if(isChemicalsDirty) {
			isChemicalsDirty = false;
			// CacheChemicalDataForAllCenterVertices();

			// mesh.SetUVs(UVCHANNEL_CHEM_AMOUNTS_AND_TEMPERATURE, chemAmountsAndTemperature);
			// mesh.SetUVs(UVCHANNEL_CHEMCOLORINDICES, chemColorIndices);
			// mesh.SetNormals(chemStates);

			chemAmountsAndTemperatureTex.SetPixels(chemAmountsAndTemperature);
			chemColorsTex.SetPixels(chemColorIndices);
			debugTex.SetPixels(debugColors);
			chemStatesTex.SetPixels(chemStates);

			chemAmountsAndTemperatureTex.Apply();
			chemColorsTex.Apply();
			debugTex.Apply();
			chemStatesTex.Apply();

			GridMaterial.SetTexture(PROPERTY_ID_CHEMAMOUNTSANDTEMPERATURETEX, chemAmountsAndTemperatureTex);
			GridMaterial.SetTexture(PROPERTY_ID_CHEMCOLORSTEX, chemColorsTex);
			GridMaterial.SetTexture("_DebugTex", debugTex);
			GridMaterial.SetTexture(PROPERTY_ID_CHEMSTATESTEX, chemStatesTex);
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

		int _uvStartIndex =(_tileGridPos.y * GameGrid.TILE_COUNT.x + _tileGridPos.x) * VERTICES_PER_TILE;

		Vector3 _uvBL = uvs[_uvStartIndex + 0];
		Vector3 _uvTL = uvs[_uvStartIndex + 1];
		Vector3 _uvTR = uvs[_uvStartIndex + 2];
		Vector3 _uvBR = uvs[_uvStartIndex + 3];
		Vector3 _uvM = uvs[_uvStartIndex + 4];

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

		uvs[_uvStartIndex + 0] = _uvBL;
		uvs[_uvStartIndex + 1] = _uvTL;
		uvs[_uvStartIndex + 2] = _uvTR;
		uvs[_uvStartIndex + 3] = _uvBR;
		uvs[_uvStartIndex + 4] = _uvM;

		isUVOrChemicalsDirty = true;
	}

	void CacheChemicalData(Int2 _nodeGridPos) {
		int _nodeIndex = _nodeGridPos.y * chemAmountsAndTemperatureTex.width + _nodeGridPos.x;

		// TODO: need to sort Contents
		ChemicalContainer _chemicalContainer = GameGrid.GetInstance().TryGetNode(_nodeGridPos).ChemicalContainer;
		Chemical.Blob _chem_0 = _chemicalContainer.Contents[0];
		Chemical.Blob _chem_1 = _chemicalContainer.Contents[1];
		Chemical.Blob _chem_2 = _chemicalContainer.Contents[2];

		// Vertex data
		// int _vertexIndexBL = ((_nodeGridPos.y) * GameGrid.TILE_COUNT.x + (_nodeGridPos.x)) * VERTICES_PER_TILE + VERTEX_INDEX_TOP_RIGHT;
		// int _vertexIndexBR = ((_nodeGridPos.y) * GameGrid.TILE_COUNT.x + (_nodeGridPos.x + 1)) * VERTICES_PER_TILE + VERTEX_INDEX_TOP_LEFT;
		// int _vertexIndexTL = ((_nodeGridPos.y + 1) * GameGrid.TILE_COUNT.x + (_nodeGridPos.x)) * VERTICES_PER_TILE + VERTEX_INDEX_BOTTOM_RIGHT;
		// int _vertexIndexTR = ((_nodeGridPos.y + 1) * GameGrid.TILE_COUNT.x + (_nodeGridPos.x + 1)) * VERTICES_PER_TILE + VERTEX_INDEX_BOTTOM_LEFT;

		float _amount_0 = _chem_0.Amount /(float)_chemicalContainer.MaxAmount;
		float _amount_1 = _chem_1.Amount /(float)_chemicalContainer.MaxAmount;
		float _amount_2 = _chem_2.Amount /(float)_chemicalContainer.MaxAmount;
		float _temperature = _chemicalContainer.Temperature /(float)Chemical.MAX_TEMPERATURE;

		// chemAmountsAndTemperature[_vertexIndexBL] = new Vector4(_amount_0, _amount_1, _amount_2, _temperature);
		// chemAmountsAndTemperature[_vertexIndexBR] = new Vector4(_amount_0, _amount_1, _amount_2, _temperature);
		// chemAmountsAndTemperature[_vertexIndexTL] = new Vector4(_amount_0, _amount_1, _amount_2, _temperature);
		// chemAmountsAndTemperature[_vertexIndexTR] = new Vector4(_amount_0, _amount_1, _amount_2, _temperature);

		float debugValue_0 = GameGrid.GetInstance().TryGetNode(_nodeGridPos).DebugChemicalContainer.Contents[0].Amount / (float)GameGrid.GetInstance().TryGetNode(_nodeGridPos).DebugChemicalContainer.MaxAmount;
		float debugValue_1 = GameGrid.GetInstance().TryGetNode(_nodeGridPos).DebugChemicalContainer.Contents[1].Amount / (float)GameGrid.GetInstance().TryGetNode(_nodeGridPos).DebugChemicalContainer.MaxAmount;
		float debugValue_2 = GameGrid.GetInstance().TryGetNode(_nodeGridPos).DebugChemicalContainer.Contents[2].Amount / (float)GameGrid.GetInstance().TryGetNode(_nodeGridPos).DebugChemicalContainer.MaxAmount;
		debugColors[_nodeIndex] = new Color(debugValue_0, debugValue_1, debugValue_2, 1);
		chemAmountsAndTemperature[_nodeIndex] = new Color(_amount_0, _amount_1, _amount_2, _temperature);

		// chemColorIndices[_vertexIndexBL] = new Vector3(_colorIndex_0, _colorIndex_1, _colorIndex_2);
		// chemColorIndices[_vertexIndexBR] = new Vector3(_colorIndex_0, _colorIndex_1, _colorIndex_2);
		// chemColorIndices[_vertexIndexTL] = new Vector3(_colorIndex_0, _colorIndex_1, _colorIndex_2);
		// chemColorIndices[_vertexIndexTR] = new Vector3(_colorIndex_0, _colorIndex_1, _colorIndex_2);
		float _colorIndex_0 = _chem_0.Chemical.GetColorIndex() / (float)ColorManager.COLOR_COUNT;
		float _colorIndex_1 = _chem_1.Chemical.GetColorIndex() / (float)ColorManager.COLOR_COUNT;
		float _colorIndex_2 = _chem_2.Chemical.GetColorIndex() / (float)ColorManager.COLOR_COUNT;
		chemColorIndices[_nodeIndex] = new Color(_colorIndex_0, _colorIndex_1, _colorIndex_2, 0.0f);
		//
		
		// Texture data
		int _stateCount = System.Enum.GetValues(typeof(Chemical.State)).Length;
		float _state_0 = _chem_0.GetStateAsFloat() /(float)(_stateCount - 1.0f);
		float _state_1 = _chem_1.GetStateAsFloat() /(float)(_stateCount - 1.0f);
		float _state_2 = _chem_2.GetStateAsFloat() /(float)(_stateCount - 1.0f);

		// Debug.Log(_chem_0.GetStateAsFloat() + ", " + _state_0);

		// chemStates[_vertexIndexBL] = new Vector3(_state_0, _state_1, _state_2);
		// chemStates[_vertexIndexBR] = new Vector3(_state_0, _state_1, _state_2);
		// chemStates[_vertexIndexTL] = new Vector3(_state_0, _state_1, _state_2);
		// chemStates[_vertexIndexTR] = new Vector3(_state_0, _state_1, _state_2);
		chemStates[_nodeIndex] = new Color(_state_0, _state_1, _state_2);
		//
		// if(_nodeGridPos.x == 30 && _nodeGridPos.y == 20) {
		// 	SuperDebug.MarkPoint(GameGrid.GetInstance().GetWorldPosFromNodeGridPos(_nodeGridPos), Color.cyan);
		//
		// 	float _stateR = GameGrid.GetInstance().TryGetNode(_nodeGridPos + Int2.Right).ChemicalContainer.Contents[0].GetStateAsFloat();
		//
		// 	Debug.Log("state = " +(_state_0 * 3.0f) + ", stateR = " + _stateR);
		// }

		if (_chemicalContainer.IsIncandescent() || _chemicalContainer.HasLostIncandescence()) {
 			LampManager.GetInstance().SetNodeIncandescenceDirty(_nodeGridPos);
		}
		
		// Color _incandescence_0 = _chem_0.GetIncandescence() * _amount_0;
		// Color _incandescence_1 = _chem_1.GetIncandescence() * _amount_1;
		// Color _incandescence_2 = _chem_2.GetIncandescence() * _amount_2;
		//
		// float _maxIncandescenceR = Mathf.Max(_incandescence_0.r, Mathf.Max(_incandescence_1.r, _incandescence_2.r));
		// float _maxIncandescenceG = Mathf.Max(_incandescence_0.g, Mathf.Max(_incandescence_1.g, _incandescence_2.g));
		// float _maxIncandescenceB = Mathf.Max(_incandescence_0.b, Mathf.Max(_incandescence_1.b, _incandescence_2.b));
		// float _maxIncandescenceA = Mathf.Max(_incandescence_0.a, Mathf.Max(_incandescence_1.a, _incandescence_2.a));
		//
		// Node _node = GameGrid.GetInstance().TryGetNode(_nodeGridPos);
		// Color32 _lighting = _node.GetLighting();
		//
		// _lighting.r = (byte)Mathf.Max(_lighting.r, _maxIncandescenceR * 255.0f);
		// _lighting.g = (byte)Mathf.Max(_lighting.g, _maxIncandescenceG * 255.0f);
		// _lighting.b = (byte)Mathf.Max(_lighting.b, _maxIncandescenceB * 255.0f);
		// _lighting.a = (byte)Mathf.Max(_lighting.a, _maxIncandescenceA * 255.0f);
		//
		// _node.SetLighting(_lighting);
	}

	// void CacheChemicalDataForAllCenterVertices() {
	// int _vertexCount = GameGrid.TILE_COUNT.x * GameGrid.TILE_COUNT.y * VERTICES_PER_TILE;
	// for(int i = 0; i < _vertexCount; i += VERTICES_PER_TILE) {
	// 	int _index = i + VERTEX_INDEX_CENTER;
	// 	int _indexBL = i + VERTEX_INDEX_BOTTOM_LEFT;
	// 	int _indexTL = i + VERTEX_INDEX_TOP_LEFT;
	// 	int _indexTR = i + VERTEX_INDEX_TOP_RIGHT;
	// 	int _indexBR = i + VERTEX_INDEX_BOTTOM_RIGHT;

	// 	Vector4 _amountsAndTemperatureBL = chemAmountsAndTemperature[_indexBL];
	// 	Vector4 _amountsAndTemperatureTL = chemAmountsAndTemperature[_indexTL];
	// 	Vector4 _amountsAndTemperatureTR = chemAmountsAndTemperature[_indexTR];
	// 	Vector4 _amountsAndTemperatureBR = chemAmountsAndTemperature[_indexBR];

	// 	float _sqrMagBL = _amountsAndTemperatureBL.sqrMagnitude;
	// 	float _sqrMagTL = _amountsAndTemperatureTL.sqrMagnitude;
	// 	float _sqrMagTR = _amountsAndTemperatureTR.sqrMagnitude;
	// 	float _sqrMagBR = _amountsAndTemperatureBR.sqrMagnitude;

	// 	if (_sqrMagBL.IsGreaterOrEqual(_sqrMagTL, _sqrMagTR, _sqrMagBR)) {
	// 		SetChemDataForVertex(_index, _amountsAndTemperatureBL, chemColorIndices[_indexBL], chemStates[_indexBL]);
	// 	}
	// 	else if (_sqrMagTL.IsGreaterOrEqual(_sqrMagBL, _sqrMagTR, _sqrMagBR)) {
	// 		SetChemDataForVertex(_index, _amountsAndTemperatureTL, chemColorIndices[_indexTL], chemStates[_indexTL]);
	// 	}
	// 	else if (_sqrMagTR.IsGreaterOrEqual(_sqrMagBL, _sqrMagTL, _sqrMagBR)) {
	// 		SetChemDataForVertex(_index, _amountsAndTemperatureTR, chemColorIndices[_indexTR], chemStates[_indexTR]);
	// 	}
	// 	else {
	// 		SetChemDataForVertex(_index, _amountsAndTemperatureBR, chemColorIndices[_indexBR], chemStates[_indexBR]);
	// 	}

	// }
	// }

	// void SetChemDataForVertex(int _vertexIndex, Vector4 _amountsAndTemperature, Vector3 _colorIndices, Vector3 _states) {
	// 	chemAmountsAndTemperature[_vertexIndex] = _amountsAndTemperature;
	// 	chemColorIndices[_vertexIndex] = _colorIndices;
	// 	chemStates[_vertexIndex] = _states;
	// }
}