using UnityEngine;
using System.Linq;
using System.Collections.Generic;
public class UVController : MeshSorter {

	public const float MESH_VERTEX_SEPARATION = 0.5f;
	public const int MESH_VERTEXCOUNT = 4;
	public const int MESH_VERTICES_PER_EDGE = 2;
	public static readonly int GRID_LAYER_COUNT = System.Enum.GetNames(typeof(GridLayerEnum)).Length;

	protected const int UVCHANNEL_UV0 = 0;
	protected const int UVCHANNEL_UV1 = 1;
	protected const int UVCHANNEL_COLOR = 2;

	public List<Vector4> CompressedUVs_0 = new List<Vector4>(MESH_VERTEXCOUNT);
	public List<Vector4> CompressedUVs_1 = new List<Vector4>(MESH_VERTEXCOUNT);
	protected bool shouldUpdateGraphics = false;

	[System.NonSerialized] public MeshFilter MyMeshFilter;

	protected static bool sHasSetShaderProperties = false;
	protected bool isHidden = false;

	private Int2 tileGridPos;

	public void SetTileGridPos(Int2 _tileGridPos) {
		tileGridPos = _tileGridPos;
	}

	private UVController MyTopUVC;

	private byte[] appliedColorChannelIndices = new byte[BuildTool.ToolSettingsColor.COLOR_CHANNEL_COUNT];

	private Int2[] uvsBottom = new Int2[MESH_VERTEXCOUNT];
	private Int2[] uvsTop = new Int2[MESH_VERTEXCOUNT];


	public override bool IsUsingAwakeDefault() { return true; }
	public override void AwakeDefault(){
		base.AwakeDefault();
		SortingLayer = MeshSorter.SortingLayerEnum.Grid;
	}

	public override void Setup(){
		base.Setup();

		MyMeshFilter = GetComponent<MeshFilter>();
		myRenderer.sharedMaterial = CachedAssets.Instance.MaterialGrid;

		if (!sHasSetShaderProperties){
			myRenderer.sharedMaterial.SetInt("TextureSizeX", CachedAssets.Instance.AssetSets[0].SpriteSheet.width);
			myRenderer.sharedMaterial.SetInt("TextureSizeY", CachedAssets.Instance.AssetSets[0].SpriteSheet.height);
			sHasSetShaderProperties = true;
		}

		ChangeColor(new byte[BuildTool.ToolSettingsColor.COLOR_CHANNEL_COUNT]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, false);
	}

	public void ScheduleUpdate() {
		shouldUpdateGraphics = true;
	}

	public override bool IsUsingUpdateLate() { return true; }
	public override void UpdateLate(){
		base.UpdateLate();

		if (shouldUpdateGraphics){
			UpdateGraphics();
			ApplyAssetChanges();

			shouldUpdateGraphics = false;
		}
	}

	void UpdateGraphics(){
		bool _isAnyWallTemporary, _isAnyUsingIsWallTemporary;
		Int2 _asset = CachedAssets.Instance.GetWallAsset(tileGridPos, out _isAnyWallTemporary, out _isAnyUsingIsWallTemporary);

		ChangeAsset(UVController.GridLayerEnum.Bottom, _asset, _isAnyWallTemporary);
		ChangeAsset(UVController.GridLayerEnum.Top, _asset, _isAnyWallTemporary);

		ColorManager.ColorUsage context = ColorManager.ColorUsage.Default;
		if (_isAnyUsingIsWallTemporary && _isAnyWallTemporary)	context = ColorManager.ColorUsage.New;
		if (_isAnyUsingIsWallTemporary && !_isAnyWallTemporary) context = ColorManager.ColorUsage.Delete;
		// if (!_isBuildingAllowed)	context = ColorManager.ColorUsage.Blocked;
		byte colorIndex = ColorManager.GetColorIndex(context);

		ChangeColor(colorIndex, _temporary: true);
	}

	void ChangeAsset(GridLayerEnum _layer, Int2 _assetPos, bool _temporary){
		if (!Application.isPlaying) { 
			return;
		}

		int _layerIndex = (int)_layer;

		int _uvT = Node.RESOLUTION * (_assetPos.y + 1);
		int _uvR = Node.RESOLUTION * (_assetPos.x + 1);
		int _uvB = Node.RESOLUTION * _assetPos.y;
		int _uvL = Node.RESOLUTION * _assetPos.x;

		switch (_layer){
			case GridLayerEnum.None:
				break;
			case GridLayerEnum.Bottom:
				uvsBottom[0] = new Int2(_uvL, _uvB);
				uvsBottom[1] = new Int2(_uvR, _uvT);
				uvsBottom[2] = new Int2(_uvR, _uvB);
				uvsBottom[3] = new Int2(_uvL, _uvT);
				break;
			case GridLayerEnum.Top:
				uvsTop[0] = new Int2(_uvL, _uvB);
				uvsTop[1] = new Int2(_uvR, _uvT);
				uvsTop[2] = new Int2(_uvR, _uvB);
				uvsTop[3] = new Int2(_uvL, _uvT);
				break;
			default:
				Debug.LogError(_layer + " hasn't been properly implemented yet!");
				break;
		}

		CompressedUVs_0.Clear();
		CompressedUVs_1.Clear();
		for (int i = 0; i < MESH_VERTEXCOUNT; i++){
			Int2 _uv0 = uvsBottom[i];
			Int2 _uv1 = uvsTop[i];

			Vector4 _compressed = new Vector4();
			_compressed.x = BitCompressor.Int2ToInt(_uv0.x, _uv0.y);
			_compressed.y = BitCompressor.Int2ToInt(_uv1.x, _uv1.y);
			_compressed.z = BitCompressor.Int2ToInt(0, 0);
			_compressed.w = BitCompressor.Int2ToInt(0, 0);
			CompressedUVs_0.Add(_compressed);

			_compressed.x = BitCompressor.Int2ToInt(0, 0);
			_compressed.y = BitCompressor.Int2ToInt(0, 0);
			_compressed.z = BitCompressor.Int2ToInt(0, 0);
			_compressed.w = BitCompressor.Int2ToInt(0, 0);
			CompressedUVs_1.Add(_compressed);
		}
	}

	public void ChangeColor(byte _colorIndex, bool _temporary) {
		if (!hasStarted) { 
			Setup();
		}

		byte[] _colorIndexAsArray = new byte[BuildTool.ToolSettingsColor.COLOR_CHANNEL_COUNT]{
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

		ChangeColor(_colorIndexAsArray, _temporary);
    }
	
	public void ResetColor() {
        ChangeColor(appliedColorChannelIndices, false);
    }

	private Vector4[] allColorIndices;
	public void ChangeColor(byte[] _colorChannelIndices, bool _temporarily) {
		if (_colorChannelIndices.Length != BuildTool.ToolSettingsColor.COLOR_CHANNEL_COUNT){
			Debug.LogError("_colorChannelIndices has a different length than what is supported!");
			return;
		}

        if (!_temporarily) {
			appliedColorChannelIndices = _colorChannelIndices;
		}

		Vector4 _indices = new Vector4();
		_indices.x = BitCompressor.Byte4ToInt(_colorChannelIndices[0], _colorChannelIndices[1], _colorChannelIndices[2], _colorChannelIndices[3]);
		_indices.y = BitCompressor.Byte4ToInt(_colorChannelIndices[4], _colorChannelIndices[5], _colorChannelIndices[6], _colorChannelIndices[7]);
		_indices.z = BitCompressor.Byte4ToInt(_colorChannelIndices[8], _colorChannelIndices[9], 0, 0);

		if (allColorIndices == null) {
			allColorIndices = new Vector4[MyMeshFilter.mesh.vertexCount];
		}
		for (int i = 0; i < MyMeshFilter.mesh.vertexCount; i++) {
			allColorIndices[i] = _indices;
		}

		shouldUpdateGraphics = true;
    }

	private List<Color32> vertexColors = new List<Color32>();
	public void SetVertexColor(int _vTilePosX, int _vTilePosY, Color32 _color){
		int _vertexIndex = _vTilePosY * MESH_VERTICES_PER_EDGE + _vTilePosX;

		// setup list for caching colors
		if (vertexColors.Count != MyMeshFilter.mesh.vertexCount){
			vertexColors = new List<Color32>(MyMeshFilter.mesh.vertexCount);
			for (int i = 0; i < MyMeshFilter.mesh.vertexCount; i++)
				vertexColors.Add(new Color32());
		}

		vertexColors[_vertexIndex] = _color;
		shouldUpdateGraphics = true;
	}

	protected void ApplyAssetChanges(){
		if (MyMeshFilter.mesh == null || MyMeshFilter.mesh.vertexCount == 0){
			return;
		}

		MyMeshFilter.mesh.SetUVs(UVCHANNEL_COLOR, allColorIndices.ToList());
		MyMeshFilter.mesh.SetColors(vertexColors);

		if (MyTopUVC != null){
			MyTopUVC.MyMeshFilter.mesh.SetUVs(UVCHANNEL_COLOR, allColorIndices.ToList());
			MyTopUVC.MyMeshFilter.mesh.SetColors(vertexColors);
		}

		MyMeshFilter.mesh.SetUVs(UVCHANNEL_UV0, CompressedUVs_0);
		MyMeshFilter.mesh.SetUVs(UVCHANNEL_UV1, CompressedUVs_1);
		myRenderer.enabled = !isHidden;
	}

	public void Hide(bool _b){
		if (!hasStarted) { 
			Setup();
		}

		myRenderer.enabled = !_b;
		isHidden = _b;
	}
}
