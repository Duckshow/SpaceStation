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

	private static List<Color32> vertexColors;
	private static List<Vector4> compressedColorIndices;
	private static byte[] appliedColorIndices;

	[SerializeField] private Transform transform;

	public enum RenderMode { Walls, Interactives }
	private RenderMode renderMode;

	private Sorting sorting;
	private MeshFilter meshFilter;
	private MeshRenderer meshRenderer;
	private Mesh mesh;
	private List<Vector3> uvsAndChemicals = new List<Vector3>();

	private Queue<Int2> tilesToUpdate = new Queue<Int2>();
	private bool isColorDirty = false;
	private bool isLightingDirty = false;
	private bool isUVOrChemicalsDirty = false;

	public static void InitStatic() {
		int _verticesPerGrid = GameGrid.SIZE.x * GameGrid.SIZE.y * VERTICES_PER_TILE;
		vertexColors = new List<Color32>(_verticesPerGrid);
		compressedColorIndices = new List<Vector4>(_verticesPerGrid);
		for (int i = 0; i < _verticesPerGrid; i++) {
			vertexColors.Add(new Color32());
			compressedColorIndices.Add(new Vector4());
		}

		appliedColorIndices = new byte[GameGrid.SIZE.x * GameGrid.SIZE.y * ColorManager.COLOR_CHANNEL_COUNT];
	}

	public void Init(Sorting _sorting, RenderMode _renderMode) {
		sorting = _sorting;
		renderMode = _renderMode;

		meshFilter = transform.GetComponent<MeshFilter>();
		meshRenderer = transform.GetComponent<MeshRenderer>();

		uvsAndChemicals = new List<Vector3>();
		for (int i = 0; i < GameGrid.SIZE.x * GameGrid.SIZE.y * VERTICES_PER_TILE; i++) {
			uvsAndChemicals.Add(new Vector3());
		}
	}

	public void CreateMesh() {
		mesh = new Mesh();
		Vector3[] _verts = new Vector3[GameGrid.SIZE.x * GameGrid.SIZE.y * VERTICES_PER_TILE];
		int[] _tris = new int[GameGrid.SIZE.x * GameGrid.SIZE.y * TRIS_PER_TILE * 3];

		Vector3 _originWorldPos = transform.position - new Vector3(GameGrid.SIZE.x * GameGrid.TILE_DIAMETER * 0.5f, GameGrid.SIZE.y * GameGrid.TILE_DIAMETER * 0.5f, 0.0f);

		int _vertexIndex = 0;
		int _tileIndex = 0;
		for (int _y = 0; _y < GameGrid.SIZE.y; _y++) {
			for (int _x = 0; _x < GameGrid.SIZE.x; _x++) {
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
		int _index = (_tileGridPos.y * GameGrid.SIZE.x + _tileGridPos.x) * ColorManager.COLOR_CHANNEL_COUNT;
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
		if (_colorIndices.Length != ColorManager.COLOR_CHANNEL_COUNT) {
			Debug.LogError("_colorChannelIndices has a different length than what is supported!");
			return;
		}

		if (_isPermanent) {
			int _appliedColorIndex = (_tileGridPos.y * GameGrid.SIZE.x + _tileGridPos.x) * _colorIndices.Length;
			for (int i = 0; i < _colorIndices.Length; i++) {
				appliedColorIndices[_appliedColorIndex + i] = _colorIndices[i];
			}
		}

		// WARNING: using more than 3 bytes per int will cause instability when cast to float!
		Vector4 _newCompressedColorIndices = new Vector4(
			_newCompressedColorIndices.x = BitCompressor.Byte4ToInt(_colorIndices[0], _colorIndices[1], _colorIndices[2], 0),
			_newCompressedColorIndices.y = BitCompressor.Byte4ToInt(_colorIndices[3], _colorIndices[4], _colorIndices[5], 0),
			_newCompressedColorIndices.z = BitCompressor.Byte4ToInt(_colorIndices[6], _colorIndices[7], _colorIndices[8], 0),
			0
		);

		int _vertexIndex = (_tileGridPos.y * GameGrid.SIZE.x + _tileGridPos.x) * VERTICES_PER_TILE;
		for (int i = 0; i < VERTICES_PER_TILE; i++) {
			compressedColorIndices[_vertexIndex + i] = _newCompressedColorIndices;
		}

		isColorDirty = true;
	}

	public void SetLighting(Int2 _tileGridPos, int _vertexIndex, Color32 _lighting) {
		if (_vertexIndex == VERTEX_INDEX_CENTER) {
			Debug.LogError("Tried to set lighting for a tile's center vertex! This is done automatically!");
		}

		int _vertexIndexInGrid = GetVertexIndexInGrid(_tileGridPos, _vertexIndex);
		vertexColors[_vertexIndexInGrid] = _lighting;

		int _centerVertexIndexInGrid = GetVertexIndexInGrid(_tileGridPos, VERTEX_INDEX_CENTER);
		Color32 _average = GetAverageColorInCornerVertices(_tileGridPos);
		vertexColors[_centerVertexIndexInGrid] = _average;

		isLightingDirty = true;
	}

	int GetVertexIndexInGrid(Int2 _tileGridPos, int _vertexIndex) {
		return _tileGridPos.y * GameGrid.SIZE.x * VERTICES_PER_TILE + _tileGridPos.x * VERTICES_PER_TILE + _vertexIndex;
	}

	Color32 GetAverageColorInCornerVertices(Int2 _tileGridPos) {
		Color32 _colorBL = vertexColors[GetVertexIndexInGrid(_tileGridPos, VERTEX_INDEX_BOTTOM_LEFT)];
		Color32 _colorTL = vertexColors[GetVertexIndexInGrid(_tileGridPos, VERTEX_INDEX_TOP_LEFT)];
		Color32 _colorTR = vertexColors[GetVertexIndexInGrid(_tileGridPos, VERTEX_INDEX_TOP_RIGHT)];
		Color32 _colorBR = vertexColors[GetVertexIndexInGrid(_tileGridPos, VERTEX_INDEX_BOTTOM_RIGHT)];

		return new Color32(
			(byte) ((_colorBL.r + _colorTL.r + _colorTR.r + _colorBR.r) / 4.0f),
			(byte) ((_colorBL.g + _colorTL.g + _colorTR.g + _colorBR.g) / 4.0f),
			(byte) ((_colorBL.b + _colorTL.b + _colorTR.b + _colorBR.b) / 4.0f),
			(byte) ((_colorBL.a + _colorTL.a + _colorTR.a + _colorBR.a) / 4.0f)
		);
	}

	public void ScheduleUpdateForTile(Int2 _tileGridPos) {
		tilesToUpdate.Enqueue(_tileGridPos);
	}

	public void TryUpdateVisuals() {
		if (tilesToUpdate.Count > 0) {
			while (tilesToUpdate.Count > 0) {
				Int2 _tileGridPos = tilesToUpdate.Dequeue();
				Int2 _asset = new Int2();

				switch (renderMode) {
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

		}

		if (isUVOrChemicalsDirty) {
			isUVOrChemicalsDirty = false;
			SetChemicalAmountForCenterVertices();
			mesh.SetUVs(UVCHANNEL_UV, uvsAndChemicals);
		}

		if (isColorDirty) {
			isColorDirty = false;
			mesh.SetUVs(UVCHANNEL_COLOR, compressedColorIndices);
		}

		if (isLightingDirty) {
			isLightingDirty = false;
			mesh.SetColors(vertexColors);
		}
	}

	void SetAsset(Int2 _tileGridPos, Int2 _assetPos) {
		Texture2D texture = CachedAssets.Instance.DefaultAssets.SpriteSheet;
		float _uvB = (GameGrid.TILE_RESOLUTION * (_assetPos.y + 0.0f)) / (texture.height);
		float _uvMY = (GameGrid.TILE_RESOLUTION * (_assetPos.y + 0.5f)) / (texture.height);
		float _uvT = (GameGrid.TILE_RESOLUTION * (_assetPos.y + 1.0f)) / (texture.height);

		float _uvL = (GameGrid.TILE_RESOLUTION * (_assetPos.x + 0.0f)) / (texture.width);
		float _uvMX = (GameGrid.TILE_RESOLUTION * (_assetPos.x + 0.5f)) / (texture.width);
		float _uvR = (GameGrid.TILE_RESOLUTION * (_assetPos.x + 1.0f)) / (texture.width);

		int _uvStartIndex = (_tileGridPos.y * GameGrid.SIZE.x + _tileGridPos.x) * VERTICES_PER_TILE;

		Vector3 _uvBL = uvsAndChemicals[_uvStartIndex + 0];
		Vector3 _uvTL = uvsAndChemicals[_uvStartIndex + 1];
		Vector3 _uvTR = uvsAndChemicals[_uvStartIndex + 2];
		Vector3 _uvBR = uvsAndChemicals[_uvStartIndex + 3];
		Vector3 _uvM = uvsAndChemicals[_uvStartIndex + 4];

		_uvBL.x = _uvL;
		_uvBL.y = _uvB;

		_uvTL.x = _uvL;
		_uvTL.y = _uvT;

		_uvTR.x = _uvR;
		_uvTR.y = _uvT;

		_uvBR.x = _uvR;
		_uvBR.y = _uvB;

		_uvM.x = _uvMX;
		_uvM.y = _uvMY;

		uvsAndChemicals[_uvStartIndex + 0] = _uvBL;
		uvsAndChemicals[_uvStartIndex + 1] = _uvTL;
		uvsAndChemicals[_uvStartIndex + 2] = _uvTR;
		uvsAndChemicals[_uvStartIndex + 3] = _uvBR;
		uvsAndChemicals[_uvStartIndex + 4] = _uvM;

		// compressedUVs.Clear();
		// for (int i = 0; i < VERTICES_PER_TILE; i++){
		// 	Int2 _uv = uvs[_uvStartIndex + i];

		// 	Vector4 _compressed = new Vector4();
		// 	_compressed.x = BitCompressor.Int2ToInt(_uv.x, _uv.y);
		// 	// _compressed.y = BitCompressor.Int2ToInt(0, 0);
		// 	// _compressed.z = BitCompressor.Int2ToInt(0, 0);
		// 	// _compressed.w = BitCompressor.Int2ToInt(0, 0);
		// 	compressedUVs.Add(_compressed);
		// }

		isUVOrChemicalsDirty = true;
	}

	public void SetChemicalAmount(Int2 _nodeGridPos, float _amount) {
		isUVOrChemicalsDirty = true;

		int _vertexIndexTR = (_nodeGridPos.y * GameGrid.SIZE.x + _nodeGridPos.x) * VERTICES_PER_TILE;
		SetChemicalAmount(_vertexIndexTR, _amount);

		if (_nodeGridPos.x > 0) {
			int _vertexIndexTL = VERTEX_INDEX_BOTTOM_RIGHT + (_nodeGridPos.y * GameGrid.SIZE.x + (_nodeGridPos.x - 1)) * VERTICES_PER_TILE;
			SetChemicalAmount(_vertexIndexTL, _amount);
		}

		if (_nodeGridPos.y > 0) {
			int _vertexIndexBR = VERTEX_INDEX_TOP_LEFT + ((_nodeGridPos.y - 1) * GameGrid.SIZE.x + _nodeGridPos.x) * VERTICES_PER_TILE;
			SetChemicalAmount(_vertexIndexBR, _amount);
		}

		if (_nodeGridPos.x > 0 && _nodeGridPos.y > 0) {
			int _vertexIndexBL = VERTEX_INDEX_TOP_RIGHT + ((_nodeGridPos.y - 1) * GameGrid.SIZE.x + (_nodeGridPos.x - 1)) * VERTICES_PER_TILE;
			SetChemicalAmount(_vertexIndexBL, _amount);
		}
	}

	void SetChemicalAmount(int _index, float _amount) {
		Vector3 _uvsAndChemical = uvsAndChemicals[_index];
		_uvsAndChemical.z = _amount * 0.1f;
		uvsAndChemicals[_index] = _uvsAndChemical;
	}

	void SetChemicalAmountForCenterVertices() {
		for (int i = 0; i < uvsAndChemicals.Count; i += VERTICES_PER_TILE) {
			float _amountBL = uvsAndChemicals[i + VERTEX_INDEX_BOTTOM_LEFT].z;
			float _amountTL = uvsAndChemicals[i + VERTEX_INDEX_TOP_LEFT].z;
			float _amountTR = uvsAndChemicals[i + VERTEX_INDEX_TOP_RIGHT].z;
			float _amountBR = uvsAndChemicals[i + VERTEX_INDEX_BOTTOM_RIGHT].z;
			float _average = (_amountBL + _amountTL + _amountTR + _amountBR) * 0.25f;

			Vector3 _uvsAndChemical = uvsAndChemicals[i + VERTEX_INDEX_CENTER];
			_uvsAndChemical.z = _average;
			uvsAndChemicals[i + VERTEX_INDEX_CENTER] = _uvsAndChemical;

		}
	}
}