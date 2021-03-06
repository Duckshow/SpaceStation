﻿using UnityEngine;
using System.Linq;
using System.Collections.Generic;
public class UVController : MeshSorter {

	// public const float MESH_VERTEX_SEPARATION = 0.5f;
	// public const int MESH_VERTEXCOUNT = 4;
	// public const int MESH_VERTICES_PER_EDGE = 2;
	// public static readonly int GRID_LAYER_COUNT = System.Enum.GetNames(typeof(GridLayerEnum)).Length;

	// protected const int UVCHANNEL_UV0 = 0;
	// // protected const int UVCHANNEL_UV1 = 1;
	// protected const int UVCHANNEL_COLOR = 1;

	// public List<Vector4> CompressedUVs_0 = new List<Vector4>(MESH_VERTEXCOUNT);
	// // public List<Vector4> CompressedUVs_1 = new List<Vector4>(MESH_VERTEXCOUNT);
	// protected bool shouldUpdateGraphics = false;

	// [System.NonSerialized] public MeshFilter MyMeshFilter;

	// protected static bool sHasSetShaderProperties = false;
	// protected bool isHidden = false;

	// private Int2 tileGridPos;

	// public void SetTileGridPos(Int2 _tileGridPos) {
	// 	tileGridPos = _tileGridPos;
	// }

	// private UVController MyTopUVC;

	// private byte[] appliedColorChannelIndices = new byte[BuildTool.ToolSettingsColor.COLOR_CHANNEL_COUNT];

	// private Int2[] uvsBottom = new Int2[MESH_VERTEXCOUNT];
	// private Int2[] uvsTop = new Int2[MESH_VERTEXCOUNT];

	// private Vector4[] allColorIndices;
	// private List<Color32> vertexColors = new List<Color32>();


	// public override bool IsUsingAwakeDefault() { return true; }
	// public override void AwakeDefault(){
	// 	base.AwakeDefault();
	// 	SortingLayer = MeshSorter.SortingLayerEnum.Grid;
	// }

	// public override void Setup(){
	// 	base.Setup();

	// 	MyMeshFilter = GetComponent<MeshFilter>();
	// 	myRenderer.sharedMaterial = CachedAssets.Instance.MaterialGrid;

	// 	if (!sHasSetShaderProperties){
	// 		myRenderer.sharedMaterial.SetInt("TextureSizeX", CachedAssets.Instance.AssetSets[0].SpriteSheet.width);
	// 		myRenderer.sharedMaterial.SetInt("TextureSizeY", CachedAssets.Instance.AssetSets[0].SpriteSheet.height);
	// 		sHasSetShaderProperties = true;
	// 	}

	// 	vertexColors = new List<Color32>(MyMeshFilter.mesh.vertexCount);
	// 	for (int i = 0; i < MyMeshFilter.mesh.vertexCount; i++){
	// 		vertexColors.Add(new Color32());
	// 	}

	// 	SetColor(new byte[BuildTool.ToolSettingsColor.COLOR_CHANNEL_COUNT]{ 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 }, _isPermanent: true);
	// }

	// public void ScheduleUpdate() {
	// 	shouldUpdateGraphics = true;
	// }

	// public override bool IsUsingUpdateLate() { return true; }
	// public override void UpdateLate(){
	// 	base.UpdateLate();

	// 	if (shouldUpdateGraphics){
	// 		UpdateAssets();
	// 		ApplyAssetChanges();

	// 		shouldUpdateGraphics = false;
	// 	}
	// }

	// void UpdateAssets(){
	// 	bool _isAnyWallTemporary, _isAnyUsingIsWallTemporary;
	// 	Int2 _asset = CachedAssets.Instance.GetWallAsset(tileGridPos, out _isAnyWallTemporary, out _isAnyUsingIsWallTemporary);

	// 	SetAsset(UVController.GridLayerEnum.Bottom, _asset);
	// 	SetAsset(UVController.GridLayerEnum.Top, _asset);
	// }

	// void SetAsset(GridLayerEnum _layer, Int2 _assetPos){
	// 	if (!Application.isPlaying) { 
	// 		return;
	// 	}

	// 	int _layerIndex = (int)_layer;

	// 	int _uvT = GameGrid.TILE_RESOLUTION * (_assetPos.y + 1);
	// 	int _uvR = GameGrid.TILE_RESOLUTION * (_assetPos.x + 1);
	// 	int _uvB = GameGrid.TILE_RESOLUTION * _assetPos.y;
	// 	int _uvL = GameGrid.TILE_RESOLUTION * _assetPos.x;

	// 	switch (_layer){
	// 		case GridLayerEnum.None:
	// 			break;
	// 		case GridLayerEnum.Bottom:
	// 			uvsBottom[0] = new Int2(_uvL, _uvB);
	// 			uvsBottom[1] = new Int2(_uvR, _uvT);
	// 			uvsBottom[2] = new Int2(_uvR, _uvB);
	// 			uvsBottom[3] = new Int2(_uvL, _uvT);
	// 			break;
	// 		case GridLayerEnum.Top:
	// 			uvsTop[0] = new Int2(_uvL, _uvB);
	// 			uvsTop[1] = new Int2(_uvR, _uvT);
	// 			uvsTop[2] = new Int2(_uvR, _uvB);
	// 			uvsTop[3] = new Int2(_uvL, _uvT);
	// 			break;
	// 		default:
	// 			Debug.LogError(_layer + " hasn't been properly implemented yet!");
	// 			break;
	// 	}

	// 	CompressedUVs_0.Clear();
	// 	// CompressedUVs_1.Clear();
	// 	for (int i = 0; i < MESH_VERTEXCOUNT; i++){
	// 		Int2 _uv0 = uvsBottom[i];
	// 		Int2 _uv1 = uvsTop[i];

	// 		Vector4 _compressed = new Vector4();
	// 		_compressed.x = BitCompressor.Int2ToInt(_uv0.x, _uv0.y);
	// 		_compressed.y = BitCompressor.Int2ToInt(_uv1.x, _uv1.y);
	// 		// _compressed.z = BitCompressor.Int2ToInt(0, 0);
	// 		// _compressed.w = BitCompressor.Int2ToInt(0, 0);
	// 		CompressedUVs_0.Add(_compressed);

	// 		// _compressed.x = BitCompressor.Int2ToInt(0, 0);
	// 		// _compressed.y = BitCompressor.Int2ToInt(0, 0);
	// 		// _compressed.z = BitCompressor.Int2ToInt(0, 0);
	// 		// _compressed.w = BitCompressor.Int2ToInt(0, 0);
	// 		// CompressedUVs_1.Add(_compressed);
	// 	}
	// }

	// public void SetColor(byte _colorIndex, bool _isPermanent) {
	// 	if (!hasStarted) { 
	// 		Setup();
	// 	}

	// 	byte[] _colorIndexAsArray = new byte[BuildTool.ToolSettingsColor.COLOR_CHANNEL_COUNT]{
	// 		_colorIndex,
	// 		_colorIndex,
	// 		_colorIndex,
	// 		_colorIndex,
	// 		_colorIndex,
	// 		_colorIndex,
	// 		_colorIndex,
	// 		_colorIndex,
	// 		_colorIndex,
	// 		_colorIndex,
	// 	};

	// 	SetColor(_colorIndexAsArray, _isPermanent);
    // }

	// public void SetColor(byte[] _colorChannelIndices, bool _isPermanent) {
	// 	if (_colorChannelIndices.Length != BuildTool.ToolSettingsColor.COLOR_CHANNEL_COUNT){
	// 		Debug.LogError("_colorChannelIndices has a different length than what is supported!");
	// 		return;
	// 	}
	// 	if (!hasStarted){
	// 		Setup();
	// 	}

	// 	if (_isPermanent) {
	// 		_colorChannelIndices.CopyTo(appliedColorChannelIndices, 0);
	// 	}

	// 	Vector4 _indices = new Vector4();
		
	// 	// WARNING: using the more than 3 bytes per int will cause instability when cast to float!
	// 	_indices.x = BitCompressor.Byte4ToInt(_colorChannelIndices[0], _colorChannelIndices[1], _colorChannelIndices[2], 0);
	// 	_indices.y = BitCompressor.Byte4ToInt(_colorChannelIndices[3], _colorChannelIndices[4], _colorChannelIndices[5], 0);
	// 	_indices.z = BitCompressor.Byte4ToInt(_colorChannelIndices[6], _colorChannelIndices[7], _colorChannelIndices[8], 0);

	// 	if (allColorIndices == null || allColorIndices.Length == 0) {
	// 		allColorIndices = new Vector4[MyMeshFilter.mesh.vertexCount];
	// 	}
	// 	for (int i = 0; i < MyMeshFilter.mesh.vertexCount; i++) {
	// 		allColorIndices[i] = _indices;
	// 	}

	// 	shouldUpdateGraphics = true;
	// }

	// public void ClearTemporaryColor() {
	// 	SetColor(appliedColorChannelIndices, _isPermanent: true);
	// }

	// public void SetLighting(int _vertexIndex, Color32 _lighting) {
	// 	vertexColors[_vertexIndex] = _lighting;
	// 	shouldUpdateGraphics = true;
	// }

	// protected void ApplyAssetChanges(){
	// 	if (MyMeshFilter.mesh == null || MyMeshFilter.mesh.vertexCount == 0){
	// 		return;
	// 	}

	// 	MyMeshFilter.mesh.SetUVs(UVCHANNEL_COLOR, allColorIndices.ToList());
	// 	MyMeshFilter.mesh.SetColors(vertexColors);

	// 	if (MyTopUVC != null){
	// 		MyTopUVC.MyMeshFilter.mesh.SetUVs(UVCHANNEL_COLOR, allColorIndices.ToList());
	// 		MyTopUVC.MyMeshFilter.mesh.SetColors(vertexColors);
	// 	}

	// 	MyMeshFilter.mesh.SetUVs(UVCHANNEL_UV0, CompressedUVs_0);
	// 	// MyMeshFilter.mesh.SetUVs(UVCHANNEL_UV1, CompressedUVs_1);
	// 	myRenderer.enabled = !isHidden;
	// }

	// public void Hide(bool _b){
	// 	if (!hasStarted) { 
	// 		Setup();
	// 	}

	// 	myRenderer.enabled = !_b;
	// 	isHidden = _b;
	// }
}
