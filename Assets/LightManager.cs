﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

public class LightManager : MonoBehaviour {

	public class SinCosTable{
		public static float[] SinArray;
		public static float[] CosArray;

		public static void Init(){
			SinArray = new float[360];
			CosArray = new float[360];
			
			for(int i = 0; i < 360; i++){
				SinArray[i] = Mathf.Sin(i * Mathf.Deg2Rad);
				CosArray[i] = Mathf.Cos(i * Mathf.Deg2Rad);
			}
		}
	}

    public static List<CustomLight> AllLights = new List<CustomLight>();
	public const int MAX_LIGHTS_AFFECTING_VERTEX = 32;
	public const int MAX_LIGHTS_CASTING_SHADOWS = 4;
	public static Material 		GridMaterial;
	
	// vertmaps
	// public static VertMap<Color>	VGridMap_TotalColorNoBlur; 		// the current color of each vertex (without blur!)
	// // public static VertMap<Vector4>	VGridMap_DomLightIndices;
	// // public static VertMap<Vector4>	VGridMap_DomLightIntensities;
	// public static VertMap<int[]> 	VGridMap_LightsInRange;         // indices for each light that can reach each vertex
	public static VertexMap VertexInfoMap;

	// gridmaps (used to be [,] but was changed due to mysterious bug!)
	public static bool[][] 		GridMap_TilesAwaitingUpdate;	// bool for each tile, to track if they need updating or not


	void Awake(){
		SinCosTable.Init();
		Init();
	}
	private static void Init(){
		GridMaterial = CachedAssets.Instance.MaterialGrid;
		SpaceNavigator.SetupSizes();

		// Grid stuff
		GridMap_TilesAwaitingUpdate = new bool[Grid.GridSize.x][];
		for (int x = 0; x < Grid.GridSize.x; x++){
			GridMap_TilesAwaitingUpdate[x] = new bool[Grid.GridSize.y];
		}

		// VGridMap_LightsInRange = new int[Grid.GridSize.x][][];
		// for (int x = 0; x < Grid.GridSize.x; x++){
		// 	VGridMap_LightsInRange[x] = new int[Grid.GridSize.y][];
		// 	for (int y = 0; y < Grid.GridSize.y; y++){
		// 		VGridMap_LightsInRange[x][y] = new int[MAX_LIGHTS_AFFECTING_VERTEX];
		// 		for (int z = 0; z < MAX_LIGHTS_AFFECTING_VERTEX; z++){
		// 			VGridMap_LightsInRange[x][y][z] = -1;
		// 		}
		// 	}
		// }

		//VertMap_TotalColorNoBlur 	= new Color 	[vertMapSize.x, vertMapSize.y];
		// VertMap_DomLightIndices 	= new Vector4	[vertMapSize.x, vertMapSize.y];
		// VertMap_DomLightIntensities = new Vector4 	[vertMapSize.x, vertMapSize.y];

		// Vector2i _vertexGridSize = Grid.GridSize * UVControllerBasic.MESH_VERTICES_PER_EDGE;
		// VGridMap_TotalColorNoBlur = new VertMap<Color>(_vertexGridSize);
		// VGridMap_DomLightIndices 		= new VertMap<Vector4>(vertexGridSize);
		// VGridMap_DomLightIntensities 	= new VertMap<Vector4>(vertexGridSize);
		// int[] _lightsInRangeDefault = new int[MAX_LIGHTS_AFFECTING_VERTEX];
		// for (int i = 0; i < _lightsInRangeDefault.Length; i++){
		// 	_lightsInRangeDefault[i] = -1;
		// }
		// VGridMap_LightsInRange = new VertMap<int[]>(_vertexGridSize, _lightsInRangeDefault);

//		Vector4 _minusOne = new Vector4(-1, -1, -1, -1);
		// SpaceNavigator.IterateOverVertexGridAndSkipOverlaps((SpaceNavigator _spaces) => {
		// 	Vector2i _vGridPos = _spaces.GetVertexGridPos();
		// 	VGridMap_DomLightIndices.TrySetValue(_vGridPos, _minusOne);
		// 	VGridMap_DomLightIntensities.TrySetValue(_vGridPos, _minusOne);
		// });
		// VertMap<Vector4>.IterateOverVertMap(VGridMap_DomLightIndices, (Vector2i _mapPos) =>{
		// 	Debug.Log("Hmmmmmm");
		// 	VGridMap_DomLightIndices.SetValue(_mapPos, _minusOne);
		// 	VGridMap_DomLightIntensities.TrySetValue(_mapPos, _minusOne);
		// });
	}
	public static void OnLightInit(CustomLight _light) {
		int _newIndex = -1;
		while (true){
			_newIndex++;
			if (!AllLights.Find(x => x.LightIndex == _newIndex)) {
				break;
			}
		}

		AllLights.Add(_light);
		_light.SetLightIndex(_newIndex);
	}
	public static void OnLightDestroyed(CustomLight _light) {
		AllLights.Remove(_light);
	}

	private static Queue<Vector2i> tilesNeedingUpdate = new Queue<Vector2i>();
	public static void ScheduleUpdateLights(Vector2i _gGridPos){
		if (GridMap_TilesAwaitingUpdate[_gGridPos.x][_gGridPos.y]) 
			return;
		GridMap_TilesAwaitingUpdate[_gGridPos.x][_gGridPos.y] = true;
		tilesNeedingUpdate.Enqueue(_gGridPos);
	}
	public static void ScheduleRemoveLight(int _lightIndex){
		if (AllLights[_lightIndex].IsBeingRemoved)
			return;
		AllLights[_lightIndex].IsBeingRemoved = true;
		lightsToRemove.Add(_lightIndex);
	}
	private static List<int> lightsToUpdate = new List<int>();
	private static List<int> lightsToRemove = new List<int>();
	void LateUpdate(){
		while (tilesNeedingUpdate.Count > 0){
			Vector2i _gridPos = tilesNeedingUpdate.Dequeue();

			int _verticesInTileCount = UVControllerBasic.MESH_VERTICES_PER_EDGE * UVControllerBasic.MESH_VERTICES_PER_EDGE;
			Vector2i _vTilePos = new Vector2i();
			for (int i = 0; i < _verticesInTileCount; i++){ // TODO: maybe replace with an IterateOver-method=
				if (i > 0){
					_vTilePos.x++;
					if (_vTilePos.x == UVControllerBasic.MESH_VERTICES_PER_EDGE){
						_vTilePos.x = 0;
						_vTilePos.y++;
					}
				}

				Vector2i _vGridPosVertex = SpaceNavigator.ConvertToVertexGridSpace(_gridPos, _vTilePos);
				// int[] _lightsInRange = VGridMap_LightsInRange.TryGetValue(_vGridPosVertex);
				// for (int i2 = 0; i2 < _lightsInRange.Length; i2++){
				// 	int _index = _lightsInRange[i2];
				// 	if (_index == -1) break;
				// 	if (AllLights[_index].IsBeingRemoved) continue;

				// 	if(!lightsToUpdate.Contains(_index))
				// 		lightsToUpdate.Add(_index);
				// }

				VertexMap.VertexInfo _vInfo = VertexInfoMap.TryGetVertex(_vGridPosVertex);
				for (int i2 = 0; i2 < _vInfo.LightsInRange.Length; i2++){
					VertexMap.VertexInfo.LightInfo _lightInfo = _vInfo.LightsInRange[i2];
					if (_lightInfo.Index == -1) { 
						break;
					}
					if (AllLights[_lightInfo.Index].IsBeingRemoved) { 
						continue;
					}
					if (!lightsToUpdate.Contains(_lightInfo.Index)) { 
						lightsToUpdate.Add(_lightInfo.Index);
					}
				}

				GridMap_TilesAwaitingUpdate[_gridPos.x][_gridPos.y] = false;
			}

			// int[] _lightsInRange = VGridMap_LightsInRange[_gridPos.x][_gridPos.y];
			// for (int i = 0; i < _lightsInRange.Length; i++){
			// 	int _index = _lightsInRange[i];
			// 	if (_index == -1) break;
			// 	if (AllLights[_index].IsBeingRemoved) continue;

			// 	if(!lightsToUpdate.Contains(_index))
			// 		lightsToUpdate.Add(_index);
			// }

			// GridMap_TilesUpdated[_gridPos.x][_gridPos.y] = false;
		}

		for (int i = 0; i < lightsToUpdate.Count; i++){
			CustomLight _light = AllLights[lightsToUpdate[i]];
			if(_light.IsBeingRemoved) continue;
			_light.UpdateLight();
		}

		for (int i = 0; i < lightsToRemove.Count; i++){
			CustomLight _light = AllLights[lightsToRemove[i]];
			_light.RemoveLightsEffectOnGrid();
		}

		if (lightsToUpdate.Count > 0 || lightsToRemove.Count > 0){
			PostProcessLighting();
		}

		for (int i = 0; i < lightsToRemove.Count; i++){
			AllLights[lightsToRemove[i]].IsBeingRemoved = false;
		}
		lightsToUpdate.Clear();
		lightsToRemove.Clear();
	}

	void PostProcessLighting() {
		// Vector2i _vGridPos = new Vector2i();
		// Vector2i _vGridPosFirst = _vGridPos;
		// Vector2i _vGridSize = Grid.GridSize * UVControllerBasic.MESH_VERTICES_PER_EDGE;

		// mIterateVariables IterateExtraVariables = delegate (){
		// 	_vGridPos.x++;
		// 	if (_vGridPos.x >= _vGridSize.x){
		// 		_vGridPos.x = _vGridPosFirst.x;
		// 		_vGridPos.y++;
		// 	}
		// };

		// int _totalIterations = (int)(Grid.Instance.GridWorldSize.x * Grid.Instance.GridWorldSize.y * Mathf.Pow(UVControllerBasic.MESH_VERTICES_PER_EDGE, 2));
		// for (int i = 0; i < _totalIterations; i++, IterateExtraVariables()){
		// 	Vector2i _vTilePos = ConvertToVertexTileSpace(_vGridPos);
		// 	if (_vTilePos.x == 0 && _vGridPos.x > 0) continue;
		// 	if (_vTilePos.y == 0 && _vGridPos.y > 0) continue;
		// 	Vector2i _gGridPos = ConvertToGridSpace(_vGridPos);
		// 	if (GridMap_LightsInRange[_gGridPos.x, _gGridPos.y].Length == 0) continue;

		// 	// setup light dirs
		// 	Vector4 _lightDirs = GetLightDirections(_vGridPos, _gGridPos);
		// 	Grid.Instance.grid[_gGridPos.x, _gGridPos.y].MyUVController.LightDirections = _lightDirs; // debug
		// 	VertexSiblings.Setup(_vGridPos);
		// 	VertexSiblings.SetLightDirections(_lightDirs, _vGridPos);

		// 	// get blurred color
		// 	Color _blurredColor = Color.clear;
		// 	Vector2i[] _neighbors = GetSafeVertexNeighbors(_vGridPos, _vTilePos, _vGridSize);
		// 	for (int i2 = 0; i2 < _neighbors.Length; i2++){
		// 		Vector2i _neighborVGridPos = _neighbors[i2];
		// 		VertexSiblings.Setup(_neighborVGridPos);
		// 		_blurredColor += TryGetAdjacentTotalColor(_neighborVGridPos);
		// 	}
		// 	_blurredColor /= _neighbors.Length;
		// 	_blurredColor.a = 1;
		// 	VertexSiblings.Setup(_vGridPos);
		// 	VertexSiblings.SetVertexColor(_blurredColor, _vGridPos);
		// }

		SpaceNavigator.IterateOverVertexGridAndSkipOverlaps((SpaceNavigator _spaces) => {
			Vector2i _vGridPos = _spaces.GetVertexGridPos();
			if (VGridMap_LightsInRange.TryGetValue(_vGridPos)[0] == -1) return; awndawnd // continue here. Keep implementing the new vertexinfo-stuff and making the vertexmap better and stuff

			Vector2i _gridPos = _spaces.GetGridPos();
			Vector2i _vTilePos = _spaces.GetVertexTilePos();

			// setup light dirs
			Vector4 _lightDirs = GetLightDirections(_spaces);
			Grid.Instance.grid[_gridPos.x, _gridPos.y].MyUVController.LightDirections = _lightDirs; // debug
			VertexSiblings.Setup(_vGridPos);
			VertexSiblings.SetLightDirections(_lightDirs, _vGridPos);

			// get blurred color
			Color _blurredColor = Color.clear;
			Vector2i[] _neighbors = GetSafeVertexNeighbors(_vGridPos, _vTilePos, _spaces.GetVertexGridSize());
			for (int i2 = 0; i2 < _neighbors.Length; i2++){
				Vector2i _neighborVGridPos = _neighbors[i2];
				VertexSiblings.Setup(_neighborVGridPos);
				_blurredColor += GetTotalColorForVertex(new SpaceNavigator(_neighborVGridPos, null));
			}
			_blurredColor /= _neighbors.Length;
			_blurredColor.a = 1;
			VertexSiblings.Setup(_vGridPos);
			VertexSiblings.SetVertexColor(_blurredColor, _vGridPos);
		});

		for (int i = 0; i < AllLights.Count; i++){
			CustomLight _light = AllLights[i];
			if(_light.IsBeingRemoved)
				LightManager.AddToLightsInRangeMap(_light, false);
		}
	}

	private Vector4 GetLightDirections(SpaceNavigator _spaces){
		Vector2i _vGridPos = _spaces.GetVertexGridPos();
		Vector2 _worldPos = _spaces.GetWorldPos();
		Vector4 _lightDir = new Vector4();
		
		int[] _lightsInRange = LightManager.VGridMap_LightsInRange.TryGetValue(_vGridPos);
		//if(Time.timeSinceLevelLoad > 1) SuperDebug.Mark(_worldPos, Color.red, _lightsInRange[0], _lightsInRange[1], _lightsInRange[2], _lightsInRange[3]);
		for (int i = 0; i < _lightsInRange.Length; i++){
			int _lightIndex = _lightsInRange[i];
			if (_lightIndex == -1) break;

			CustomLight _otherLight = LightManager.AllLights[_lightIndex];
			if(_otherLight.IsBeingRemoved) continue;

			Vector2 _dir = (_otherLight.MyWorldPos - _worldPos).normalized;
			float _dotX = Vector2.Dot(Vector2.right, _dir);
			float _dotY = Vector2.Dot(Vector2.up, _dir);
			if (_dotX == 0 && _dotY == 0) { // light and vertex are at same position
				_lightDir = Vector4.one;
			}
			else{
				if (_dotY >= 0) _lightDir.x = 1;
				if (_dotX >= 0) _lightDir.y = 1;
				if (_dotY <= 0) _lightDir.z = 1;
				if (_dotX <= 0) _lightDir.w = 1;
			}
		}

		return _lightDir;
	}

	Vector2i vertexBottomLeft 	= new Vector2i();
	Vector2i vertexBottom 		= new Vector2i();
	Vector2i vertexBottomRight 	= new Vector2i();
	Vector2i vertexLeft 		= new Vector2i();
	Vector2i vertexThis 		= new Vector2i();
	Vector2i vertexRight 		= new Vector2i();
	Vector2i vertexTopLeft 		= new Vector2i();
	Vector2i vertexTop 			= new Vector2i();
	Vector2i vertexTopRight 	= new Vector2i();
	Vector2i[] GetSafeVertexNeighbors(Vector2i _vGridPos, Vector2i _vTilePos, Vector2i _vertexGridAxisLengths) {
		int _thisX = _vGridPos.x;
		int _thisY = _vGridPos.y;
		int _left = _thisX - 1;
		int _down = _thisY - 1;
		int _right = _thisX + (_vTilePos.x == 0 ? 1 : 2); // TODO: this can't be right. It should be == 1, right?
		int _up = _thisY + (_vTilePos.y == 0 ? 1 : 2);

		vertexThis.x 		= _thisX;	vertexThis.y 		= _thisY;
		vertexBottomLeft.x 	= _left;	vertexBottomLeft.y 	= _down;
		vertexBottom.x 		= _thisX;	vertexBottom.y 		= _down;
		vertexBottomRight.x = _right; 	vertexBottomRight.y = _down;
		vertexLeft.x 		= _left;	vertexLeft.y 		= _thisY;
		vertexRight.x 		= _right;	vertexRight.y 		= _thisY;
		vertexTopLeft.x 	= _left;	vertexTopLeft.y 	= _up;
		vertexTop.x 		= _thisX;	vertexTop.y 		= _up;
		vertexTopRight.x 	= _right;	vertexTopRight.y 	= _up;

		bool _canGoBottom 		= VertexIsWithinGrid(_thisX, 	_down, 	_vertexGridAxisLengths);
		bool _canGoLeft 		= VertexIsWithinGrid(_left, 	_thisY, _vertexGridAxisLengths);
		bool _canGoRight 		= VertexIsWithinGrid(_right, 	_thisY, _vertexGridAxisLengths);
		bool _canGoTop 			= VertexIsWithinGrid(_thisX, 	_up, 	_vertexGridAxisLengths);

		int _neighborCount = 1; // 1 because you can always go to _this_ vertex
		if (_canGoBottom) 					_neighborCount++;
		if (_canGoLeft) 					_neighborCount++;
		if (_canGoRight) 					_neighborCount++;
		if (_canGoTop) 						_neighborCount++;
		if (_canGoBottom && _canGoLeft) 	_neighborCount++;
		if (_canGoBottom && _canGoRight) 	_neighborCount++;
		if (_canGoTop && _canGoLeft) 		_neighborCount++;
		if (_canGoTop && _canGoRight) 		_neighborCount++;

		Vector2i[] _neighbors = new Vector2i[_neighborCount];

		_neighbors[0] = vertexThis;
		int _currentIndex = 1;
		if (_canGoBottom) 					{ _neighbors[_currentIndex] = vertexBottom; 		_currentIndex++; }
		if (_canGoLeft) 					{ _neighbors[_currentIndex] = vertexLeft; 			_currentIndex++; }
		if (_canGoRight) 					{ _neighbors[_currentIndex] = vertexRight; 			_currentIndex++; }
		if (_canGoTop) 						{ _neighbors[_currentIndex] = vertexTop; 			_currentIndex++; }
		if (_canGoBottom && _canGoLeft) 	{ _neighbors[_currentIndex] = vertexBottomLeft; 	_currentIndex++; }
		if (_canGoBottom && _canGoRight) 	{ _neighbors[_currentIndex] = vertexBottomRight; 	_currentIndex++; }
		if (_canGoTop && _canGoLeft) 		{ _neighbors[_currentIndex] = vertexTopLeft; 		_currentIndex++; }
		if (_canGoTop && _canGoRight) 		{ _neighbors[_currentIndex] = vertexTopRight; 		_currentIndex++; }

		return _neighbors;
	}
	static bool VertexIsWithinGrid(int _x, int _y, Vector2i _vertexGridAxisLengths) {
		return _x >= 0 && _y >= 0 && _x < _vertexGridAxisLengths.x && _y < _vertexGridAxisLengths.y;
	}

	Color GetTotalColorForVertex(SpaceNavigator _spaces){
		Vector2i _vGridPos = _spaces.GetVertexGridPos();
		Color _totalColor = VGridMap_TotalColorNoBlur.TryGetValue(_vGridPos);

		// no cached color, so create and cache
		if (!_totalColor.Any()) {
			int[] _lightsInRangeIndices = VGridMap_LightsInRange.TryGetValue(_vGridPos);
			for (int i = 0; i < _lightsInRangeIndices.Length; i++){
				int _index = _lightsInRangeIndices[i];
				if (_index == -1) break;

				CustomLight _light = AllLights[_index];
				if (_light.IsBeingRemoved) continue;

				_spaces.SetLightSpace(_light);
				Vector2i _vLightPos = _spaces.GetVertexLightPos();
				bool _hit = _light.VLightMap_Hit.TryGetValue(_vLightPos);
				if (!_hit) continue;

				float _intensity = _light.VLightMap_Intensity.TryGetValue(_vLightPos);
				Color _lightColor = _light.GetLightColor() * _intensity;
				if (_totalColor.r < _lightColor.r) _totalColor.r += _lightColor.r;
				if (_totalColor.g < _lightColor.g) _totalColor.g += _lightColor.g;
				if (_totalColor.b < _lightColor.b) _totalColor.b += _lightColor.b;
			}

			VertexSiblings.SetValueInVertexGridMap<Color>(VGridMap_TotalColorNoBlur, _totalColor, _vGridPos);
		}
	
		return _totalColor;
	}

	public static void AddToLightsInRangeMap(CustomLight _light, bool _add){
		CustomLight.TileReference[,] _tiles = _light.GetTilesInRange(_onlyWithColliders: false);

		// int _xMin = _light.MyGridCoord.x - _light.Radius;
		// int _xMax = _xMin + _tiles.GetLength(0);
		// int _xGrid = _xMin;
		// int _yGrid = _light.MyGridCoord.y - _light.Radius;
		// mIterateVariables IterateExtraVariables = delegate (){
		// 	_xGrid++;
		// 	if (_xGrid == _xMax){
		// 		_xGrid = _xMin;
		// 		_yGrid++;
		// 	}
		// };

		// for (int i = 0; i < _tiles.Length; i++, IterateExtraVariables()){
		// 	if(_yGrid >= Grid.GridSize.y)
		// 		break;
		// 	if(_yGrid < 0 || _xGrid < 0 || _xGrid >= Grid.GridSize.x)
		// 		continue;

		// 	for (int i2 = 0; i2 < GridMap_LightsInRange[_xGrid, _yGrid].Length; i2++){
		// 		int _lightIndex = GridMap_LightsInRange[_xGrid, _yGrid][i2];
		// 		if (_b && _lightIndex == _light.LightIndex){
		// 			break; // already exists for this tile
		// 		}
		// 		else if(_b && _lightIndex == -1){
		// 			Vector2 _pos = Grid.Instance.GetWorldPointFromTileCoord(new Vector2i(_xGrid, _yGrid));
		// 			GridMap_LightsInRange[_xGrid, _yGrid][i2] = _light.LightIndex;
		// 			break;
		// 		}
		// 		else if(!_b && _lightIndex == _light.LightIndex){
		// 			GridMap_LightsInRange[_xGrid, _yGrid].PseudoRemoveAt<int>(i2, _emptyValue: -1);
		// 			break;
		// 		}
		// 	}
		// }

		SpaceNavigator.IterateOverLightsVerticesOnVGridAndSkipOverlaps(_light, (SpaceNavigator _spaces) => {
			Vector2i _vGridPos = _spaces.GetVertexGridPos();
			int[] _lightIndices = VGridMap_LightsInRange.TryGetValue(_vGridPos);
			for (int i = 0; i < _lightIndices.Length; i++){
				int _lightIndex = _lightIndices[i];
				if (_lightIndex == _light.LightIndex){
					if (_add) {
						break; // already exists for this tile
					}
					else {
						_lightIndices.PseudoRemoveAt<int>(i, _emptyValue: -1);
						break;
					}
				}
				else if(_lightIndex == -1 && _add){
					if (_light.LightIndex == 3){
						Debug.Log(VGridMap_LightsInRange.TryGetValue(new Vector2i(_vGridPos.x, _vGridPos.y))[i].ToString().Color(Color.red));
						Debug.Log(VGridMap_LightsInRange.TryGetValue(new Vector2i(_vGridPos.x + 1, _vGridPos.y))[i].ToString().Color(Color.red));
						Debug.Log(VGridMap_LightsInRange.TryGetValue(new Vector2i(_vGridPos.x + 2, _vGridPos.y))[i].ToString().Color(Color.red));
						Debug.Log(VGridMap_LightsInRange.TryGetValue(new Vector2i(_vGridPos.x + 3, _vGridPos.y))[i].ToString().Color(Color.red));
						Debug.Log(VGridMap_LightsInRange.TryGetValue(new Vector2i(_vGridPos.x + 4, _vGridPos.y))[i].ToString().Color(Color.red));
					}
					_lightIndices[i] = _light.LightIndex;
					if (_light.LightIndex == 3){
						Debug.Log(VGridMap_LightsInRange.TryGetValue(new Vector2i(_vGridPos.x, _vGridPos.y))[i].ToString().Color(Color.cyan));
						Debug.Log(VGridMap_LightsInRange.TryGetValue(new Vector2i(_vGridPos.x + 1, _vGridPos.y))[i].ToString().Color(Color.cyan));
						Debug.Log(VGridMap_LightsInRange.TryGetValue(new Vector2i(_vGridPos.x + 2, _vGridPos.y))[i].ToString().Color(Color.cyan));
						Debug.Log(VGridMap_LightsInRange.TryGetValue(new Vector2i(_vGridPos.x + 3, _vGridPos.y))[i].ToString().Color(Color.cyan));
						Debug.Log(VGridMap_LightsInRange.TryGetValue(new Vector2i(_vGridPos.x + 4, _vGridPos.y))[i].ToString().Color(Color.cyan));
					}
					break;
				}
			}

			Grid.Instance.grid[_spaces.GetGridPos().x, _spaces.GetGridPos().y].MyUVController.LightIndices = _lightIndices;
			VGridMap_LightsInRange.TrySetValue(_vGridPos, _lightIndices);
		});
	}

	public static class VertexSiblings { // vertices sharing the same world pos
		public class Sibling {
			public bool Affected = false;

			// hard to replace these with SpaceNavigator, since VertexSiblings is also used for top-half vertices. Skipping for now.
			public Vector2i vGridPos;
			public Vector2i gGridPos;
			public Vector2i vTilePos;

			public void SetNewValues(bool _affected, int _gGridPosX, int _gGridPosY, int _vTilePosX, int _vTilePosY) {
				Affected = _affected;
				if(!_affected) return;

				gGridPos.x = _gGridPosX;
				gGridPos.y = _gGridPosY;
				vTilePos.x = _vTilePosX;
				vTilePos.y = _vTilePosY;
				vGridPos = SpaceNavigator.ConvertToVertexGridSpace(gGridPos, vTilePos);
			}

			public void SetLightDirections(Vector4 _lightDirs) {
				Grid.Instance.grid[gGridPos.x, gGridPos.y].MyUVController.SetLightDirections(vTilePos, _lightDirs);
			}
			public void SetVertexColor(Color _color){
				Grid.Instance.grid[gGridPos.x, gGridPos.y].MyUVController.SetVertexColor(vTilePos, _color);
			}
			public void SetValueInVertexLightMap<T>(VertMap<T> _vertexLightMap, T _value, CustomLight _light){
				Vector2i _vLightPos = SpaceNavigator.ConvertToVertexLightSpace(vGridPos, _light);
				_vertexLightMap.TrySetValue(_vLightPos, _value);
			}
		}
		public static Sibling Current 			= new Sibling();
		public static Sibling OwnTopHalf 		= new Sibling();
		public static Sibling OwnTopHalfLeft 	= new Sibling();
		public static Sibling OwnTopHalfRight 	= new Sibling();
		public static Sibling Top 				= new Sibling();
		public static Sibling Right 			= new Sibling();
		public static Sibling RightTopHalf 		= new Sibling();
		public static Sibling RightTopHalfLeft 	= new Sibling();
		public static Sibling TopRight 			= new Sibling();
		public static void Setup(Vector2i _vGridPos) {
			Vector2i _vTilePos = SpaceNavigator.ConvertToVertexTileSpace(_vGridPos);
			Vector2i _gGridPos = SpaceNavigator.ConvertToGridSpace(_vGridPos);
			bool _isOnLeftEdge = _vTilePos.x == 0;
			bool _isOnRightEdge = _vTilePos.x == UVControllerBasic.MESH_VERTICES_PER_EDGE - 1;
			bool _affectsTopHalf = _vTilePos.y == UVControllerBasic.MESH_VERTICES_PER_EDGE - 1;
			bool _canGoRight 	= _gGridPos.x + 1 < Grid.GridSize.x;
			bool _canGoUp 		= _gGridPos.y + 1 < Grid.GridSize.y;

			Current.SetNewValues(
				true,
				_gGridPos.x, _gGridPos.y,
				_vTilePos.x, _vTilePos.y
			);
			OwnTopHalf.SetNewValues(
				_affectsTopHalf,
				_gGridPos.x, _gGridPos.y,
				_vTilePos.x, _vTilePos.y + 1
			);
			OwnTopHalfLeft.SetNewValues(
				_affectsTopHalf && _isOnLeftEdge,
				_gGridPos.x, _gGridPos.y,
				0, _vTilePos.y + 2
			);
			OwnTopHalfRight.SetNewValues(
				_affectsTopHalf && _isOnRightEdge,
				_gGridPos.x, _gGridPos.y,
				1, _vTilePos.y + 2
			);
			Right.SetNewValues(
				_isOnRightEdge && _canGoRight,
				_gGridPos.x + 1, _gGridPos.y,
				0, _vTilePos.y
			);
			RightTopHalf.SetNewValues(
				_canGoRight && _isOnRightEdge && _affectsTopHalf,
				_gGridPos.x + 1, _gGridPos.y,
				0, _vTilePos.y + 1
			);
			RightTopHalfLeft.SetNewValues(
				_canGoRight && _isOnRightEdge && _affectsTopHalf,
				_gGridPos.x + 1, _gGridPos.y,
				0, _vTilePos.y + 2
			);
			Top.SetNewValues(
				_canGoUp && _affectsTopHalf,
				_gGridPos.x, _gGridPos.y + 1,
				_vTilePos.x, 0
			);
			TopRight.SetNewValues(
				_canGoUp && Top.Affected && Right.Affected,
				_gGridPos.x + 1, _gGridPos.y + 1,
				0, 0
			);
		}
		public static void SetLightDirections(Vector4 _lightDirs, Vector2i _currentVGridPos) {
			if(_currentVGridPos != Current.vGridPos) Debug.LogError("VertexSiblings appears to be outdated!");
			if (Current.Affected) 			Current.			SetLightDirections(_lightDirs);
			if (OwnTopHalf.Affected) 		OwnTopHalf.			SetLightDirections(_lightDirs);
			if (OwnTopHalfLeft.Affected) 	OwnTopHalfLeft.		SetLightDirections(_lightDirs);
			if (OwnTopHalfRight.Affected) 	OwnTopHalfRight.	SetLightDirections(_lightDirs);
			if (Top.Affected) 				Top.				SetLightDirections(_lightDirs);
			if (Right.Affected) 			Right.				SetLightDirections(_lightDirs);
			if (RightTopHalf.Affected) 		RightTopHalf.		SetLightDirections(_lightDirs);
			if (RightTopHalfLeft.Affected) 	RightTopHalfLeft.	SetLightDirections(_lightDirs);
			if (TopRight.Affected) 			TopRight.			SetLightDirections(_lightDirs);
		}
		public static void SetVertexColor(Color _color, Vector2i _currentVGridPos){
			if (_currentVGridPos != Current.vGridPos) Debug.LogError("VertexSiblings appears to be outdated!");
			if (Current.Affected) 			Current.			SetVertexColor(_color);
			if (OwnTopHalf.Affected) 		OwnTopHalf.			SetVertexColor(_color);
			if (OwnTopHalfLeft.Affected) 	OwnTopHalfLeft.		SetVertexColor(_color);
			if (OwnTopHalfRight.Affected) 	OwnTopHalfRight.	SetVertexColor(_color);
			if (Top.Affected) 				Top.				SetVertexColor(_color);
			if (Right.Affected) 			Right.				SetVertexColor(_color);
			if (RightTopHalf.Affected) 		RightTopHalf.		SetVertexColor(_color);
			if (RightTopHalfLeft.Affected) 	RightTopHalfLeft.	SetVertexColor(_color);
			if (TopRight.Affected) 			TopRight.			SetVertexColor(_color);
		}

		public static void SetValueInVertexGridMap<T>(VertMap<T> _vertexGridMap, T _value, Vector2i _currentVGridPos) {
			if (_currentVGridPos != Current.vGridPos) Debug.LogError("VertexSiblings appears to be outdated!");
			if (Current.Affected) 	_vertexGridMap.TrySetValue(Current.vGridPos, _value);
			if (Top.Affected) 		_vertexGridMap.TrySetValue(Top.vGridPos, _value);
			if (Right.Affected)		_vertexGridMap.TrySetValue(Right.vGridPos, _value);
			if (TopRight.Affected)	_vertexGridMap.TrySetValue(TopRight.vGridPos, _value);
		}
		public static void SetValueInVertexLightMap<T>(VertMap<T> _vertexLightMap, T _value, CustomLight _light, Vector2i _currentVGridPos){
			if (_currentVGridPos != Current.vGridPos) Debug.LogError("VertexSiblings appears to be outdated!");
			if (Current.Affected) 	Current.	SetValueInVertexLightMap<T>(_vertexLightMap, _value, _light);
			if (Top.Affected)		Top.		SetValueInVertexLightMap<T>(_vertexLightMap, _value, _light);
			if (Right.Affected)		Right.		SetValueInVertexLightMap<T>(_vertexLightMap, _value, _light);
			if (TopRight.Affected)	TopRight.	SetValueInVertexLightMap<T>(_vertexLightMap, _value, _light);
		}
	}

	// public static Vector2i ConvertToVertexGridSpace(Vector2i _vLightPos, CustomLight _light){
	// 	return _vLightPos + Vector2i.Scale(_light.MyGridCoord - _light.GetRadiusAsVector(), UVControllerBasic.MESH_VERTICES_PER_EDGE_AS_VECTOR);
	// }
	// public static Vector2i ConvertToVertexGridSpace(Vector2i _gGridPos, Vector2i _vTilePos){
	// 	return _gGridPos * UVControllerBasic.MESH_VERTICES_PER_EDGE + _vTilePos;
	// }
	// public static Vector2i ConvertToGridSpace(Vector2i _vGridPos){
	// 	return _vGridPos / UVControllerBasic.MESH_VERTICES_PER_EDGE;
	// }
	// public static Vector2i ConvertToLightSpace(Vector2i _vGridPos, CustomLight _light){
	// 	return ConvertToGridSpace(_vGridPos) - (_light.MyGridCoord - _light.GetRadiusAsVector());
	// }
	// public static Vector2i ConvertToVertexLightSpace(Vector2i _vGridPos, CustomLight _light){
	// 	return _vGridPos - Vector2i.Scale(_light.MyGridCoord - _light.GetRadiusAsVector(), UVControllerBasic.MESH_VERTICES_PER_EDGE_AS_VECTOR);
	// }
	// public static Vector2i ConvertToVertexTileSpace(Vector2i _vGridPos){ // WARNING: does not support top-half vertices! Confusing? Yes!
	// 	return _vGridPos - ConvertToGridSpace(_vGridPos) * UVControllerBasic.MESH_VERTICES_PER_EDGE;
	// }
	// public static Vector2 ConvertToWorldSpace(Vector2i _vGridPos){
	// 	Vector2i _gGridPos 	= ConvertToGridSpace(_vGridPos);
	// 	Vector2 _localPos 	= new Vector2(_vGridPos.x * UVControllerBasic.MESH_VERTEX_SEPARATION, _vGridPos.y * UVControllerBasic.MESH_VERTEX_SEPARATION);
	// 	Vector2 _correction = new Vector2(_gGridPos.x * UVControllerBasic.MESH_VERTEX_SEPARATION, _gGridPos.y * UVControllerBasic.MESH_VERTEX_SEPARATION); // discount every third vertex (except first) since they overlap
	// 	Vector2 _gridWorldPos = Grid.Instance.transform.position;
	// 	return _gridWorldPos - Grid.GridSizeHalf + _localPos - _correction;
    // }

	public class VertexMap {
		public class VertexInfo {
			public class LightInfo {
				public int Index;
				public bool Hit;
				public float Intensity;
				public LightInfo() {
					Index = -1;
					Hit = false;
					Intensity = 0;
				}
			}

			public Color ColorWithoutBlur;
			public LightInfo[] LightsInRange;
			public VertexInfo() {
				ColorWithoutBlur = Color.clear;
				LightsInRange = new LightInfo[MAX_LIGHTS_AFFECTING_VERTEX];
			}
		}
		private VertexInfo[,] map;
		private Vector2i lastIndex;
		private bool hasSetupConversionTables = false;
		

		public VertexMap(Vector2i _vertexCount) {
			Vector2i _mapVertexCount = ConvertVGridPosToVMapPos(_vertexCount);
			map = new VertexInfo[_mapVertexCount.x, _mapVertexCount.y];
			lastIndex = new Vector2i(_mapVertexCount.x - 1, _mapVertexCount.y - 1);

			if (!hasSetupConversionTables){
				// grid -> map
				VGridToVMapConversionTable = new Vector2i[_vertexCount.x, _vertexCount.y];
				for (int y = 0; y < _vertexCount.y; y++){
					for (int x = 0; x < _vertexCount.x; x++){
						VGridToVMapConversionTable[x, y] = ConvertVGridPosToVMapPos(new Vector2i(x, y), _useConversionTable: false);
					}
				}
				
				// map -> grid
				VMapToVGridConversionTable = new Vector2i[_mapVertexCount.x, _mapVertexCount.y];
				for (int y = 0; y < _mapVertexCount.y; y++){
					for (int x = 0; x < _mapVertexCount.x; x++){
						VMapToVGridConversionTable[x, y] = ConvertVMapPosToVGridPos(new Vector2i(x, y), _useConversionTable: false);
					}
				}

				hasSetupConversionTables = true;
			}
		}
		public VertexInfo TryGetVertex(Vector2i _vGridPos) {
			Vector2i _mapIndices = ConvertVGridPosToVMapPos(_vGridPos);
			if (_mapIndices.x < 0 || _mapIndices.x > lastIndex.x) return null;
			if (_mapIndices.y < 0 || _mapIndices.y > lastIndex.y) return null;
			return map[_mapIndices.x, _mapIndices.y];
		}
		public void TrySetVertex(Vector2i _vGridPos, VertexInfo _vertexInfo) {
			Vector2i _mapIndices = ConvertVGridPosToVMapPos(_vGridPos);
			if (_mapIndices.x < 0 || _mapIndices.x > lastIndex.x) return;
			if (_mapIndices.y < 0 || _mapIndices.y > lastIndex.y) return;
			map[_mapIndices.x, _mapIndices.y] = _vertexInfo;
		}
		public void SetVertex(Vector2i _mapPos, VertexInfo _vertexInfo) {
			map[_mapPos.x, _mapPos.y] = _vertexInfo;
		}
		public Vector2i GetLastIndex() {
			return lastIndex;
		}
	}
}
