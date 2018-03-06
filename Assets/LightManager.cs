using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
	public static bool[,] 		GridMap_TilesUpdated;          	// bool for each tile, to track if they need updating or not
	public static int[,][] 		GridMap_LightsInRange;          // indices for each light that can reach each vertex
	//public static float[,][] 	VertMap_LightReceived;          // how much light every vertex receives from every light in range
	public static Vector4[,] 	VertMap_DomLightIndices; 		// the four most dominant lights' indices for each vertex
	public static Vector4[,] 	VertMap_DomLightIntensities; 	// how strongly the dominant lights hit each vertex
	public static Color[,] 		VertMap_TotalColorNoBlur; 			// the current color of each vertex (without blur!)

	private delegate void mIterateVariables();


	void Awake(){
		SinCosTable.Init();
		Init();
	}
	private static void Init(){
		GridMaterial = CachedAssets.Instance.MaterialGrid;

		// Grid stuff
		GridMap_TilesUpdated = new bool[Grid.GridSize.x, Grid.GridSize.y];
		GridMap_LightsInRange = new int[Grid.GridSize.x, Grid.GridSize.y][];
		int _xGrid = 0, _yGrid = 0;
		mIterateVariables IterateGridVariables = delegate (){
			_xGrid++;
			if (_xGrid == Grid.GridSize.x){
				_xGrid = 0;
				_yGrid++;
			}
		};
		int _totalIterations = Grid.GridSize.x * Grid.GridSize.y;
		for (int i = 0; i < _totalIterations; i++, IterateGridVariables()){
			int[] _lightIndices = new int[MAX_LIGHTS_AFFECTING_VERTEX];
			for (int i2 = 0; i2 < _lightIndices.Length; i2++){
				_lightIndices[i2] = -1;
			}

			GridMap_LightsInRange[_xGrid, _yGrid] = _lightIndices;
		}

		// Vertex stuff
		int _mapSizeX = Grid.GridSize.x * UVControllerBasic.MESH_VERTICES_PER_EDGE;
		int _mapSizeY = Grid.GridSize.y * UVControllerBasic.MESH_VERTICES_PER_EDGE;
		//VertMap_LightReceived 			= new float[_mapSizeX, _mapSizeY][];
		VertMap_DomLightIndices 		= new Vector4	[_mapSizeX, _mapSizeY];
		VertMap_DomLightIntensities 	= new Vector4	[_mapSizeX, _mapSizeY];
		VertMap_TotalColorNoBlur 			= new Color		[_mapSizeX, _mapSizeY];
		int _vxGrid = 0, _vyGrid = 0;
		mIterateVariables IterateVertexVariables = delegate (){
			_vxGrid++;
			if (_vxGrid == _mapSizeX){
				_vxGrid = 0;
				_vyGrid++;
			}
		};
		_totalIterations = _mapSizeX * _mapSizeY;
		for (int i = 0; i < _totalIterations; i++, IterateVertexVariables()){
			//VertMap_LightReceived[_vxGrid, _vyGrid] = new float[MAX_LIGHTS_AFFECTING_VERTEX];
			
			VertMap_DomLightIndices[_vxGrid, _vyGrid].x = -1;
			VertMap_DomLightIndices[_vxGrid, _vyGrid].y = -1;
			VertMap_DomLightIndices[_vxGrid, _vyGrid].z = -1;
			VertMap_DomLightIndices[_vxGrid, _vyGrid].w = -1;
		}
	}

	private static Queue<Vector2i> tilesNeedingUpdate = new Queue<Vector2i>();
	private static Queue<int> lightsNeedingRemoval = new Queue<int>();
	public static void ScheduleUpdateLights(Vector2i _posGrid){
		if (GridMap_TilesUpdated[_posGrid.x, _posGrid.y]) 
			return;
		GridMap_TilesUpdated[_posGrid.x, _posGrid.y] = true;
		tilesNeedingUpdate.Enqueue(_posGrid);
	}
	public static void ScheduleRemoveLight(int _lightIndex){
		if (AllLights[_lightIndex].IsBeingRemoved)
			return;
		AllLights[_lightIndex].IsBeingRemoved = true;
		lightsNeedingRemoval.Enqueue(_lightIndex);
	}
	List<int> lightsToUpdate = new List<int>();
	void LateUpdate(){
		while (tilesNeedingUpdate.Count > 0){
			Vector2i _posGrid = tilesNeedingUpdate.Dequeue();
			int[] _lightsInRange = GridMap_LightsInRange[_posGrid.x, _posGrid.y];
			for (int i = 0; i < _lightsInRange.Length; i++){
				int _index = _lightsInRange[i];
				if (_index == -1)
					break;

				if(!lightsToUpdate.Contains(_index))
					lightsToUpdate.Add(_index);
			}

			GridMap_TilesUpdated[_posGrid.x, _posGrid.y] = false;
		}

		if (lightsToUpdate.Count > 0){
			for (int i = 0; i < lightsToUpdate.Count; i++){
				CustomLight _light = AllLights[lightsToUpdate[i]];
				if(_light.IsBeingRemoved)
					continue;

				_light.UpdateLight();
			}

			lightsToUpdate.Clear();
		}

		while (lightsNeedingRemoval.Count > 0){
			int _index = lightsNeedingRemoval.Dequeue();
			AllLights[_index].RemoveLightsEffectOnGrid();
			AllLights[_index].IsBeingRemoved = false;
		}
	}

	// private void ClearVColorsForLights(List<int> _lightIndices) {

	// 	// sort from bottom-left to top-right in grid
	// 	_lightIndices.Sort(
	// 		delegate (int val1, int val2) {
	// 			int val1X = AllLights[val1].MyGridCoord.x;
	// 			int val1Y = AllLights[val1].MyGridCoord.y;
	// 			int val2X = AllLights[val2].MyGridCoord.x;
	// 			int val2Y = AllLights[val2].MyGridCoord.y;
	// 			if (val1Y < val2Y)
	// 				return -1;
	// 			else if(val1Y > val2Y)
	// 				return 1;
	// 			else if(val1X < val2X)
	// 				return -1;
	// 			else if(val1X > val2X)
	// 				return 1;
	// 			else
	// 				return 0;
	// 		}
	// 	);

	// 	for (int i = 0; i < _lightIndices.Count; i++){
	// 		bool _skipToNextLight = false;

	// 		CustomLight _light = AllLights[_lightIndices[i]];
	// 		int _vDiameter = _light.Radius * 2 * UVControllerBasic.MESH_VERTICES_PER_EDGE;
	// 		int _gridEdgeX = Grid.GridSizeX * UVControllerBasic.MESH_VERTICES_PER_EDGE;
	// 		int _gridEdgeY = Grid.GridSizeY * UVControllerBasic.MESH_VERTICES_PER_EDGE;
	// 		int _vxGridStart = (_light.MyGridCoord.x - _light.Radius) * UVControllerBasic.MESH_VERTICES_PER_EDGE;
	// 		int _vyGridStart = (_light.MyGridCoord.y - _light.Radius) * UVControllerBasic.MESH_VERTICES_PER_EDGE;
	// 		int _vxGridEnd = _vxGridStart + _vDiameter;
	// 		int _vyGridEnd = _vyGridStart + _vDiameter;

	// 		int _vxGridNextStart = _vxGridEnd;
	// 		int _vyGridNextStart = _vyGridEnd;
	// 		if (i + 1 < _lightIndices.Count - 1){
	// 			CustomLight _nextLight = AllLights[_lightIndices[i + 1]];
	// 			_vxGridNextStart = (_nextLight.MyGridCoord.x - _nextLight.Radius) * UVControllerBasic.MESH_VERTICES_PER_EDGE;
	// 			_vyGridNextStart = (_nextLight.MyGridCoord.y - _nextLight.Radius) * UVControllerBasic.MESH_VERTICES_PER_EDGE;
	// 		}

	// 		int _vxGrid = _vxGridStart, _vyGrid = _vyGridStart;
	// 		mIterateVariables IterateExtraVariables = delegate (){
	// 			_vxGrid++;

	// 			if (_vxGrid >= _vxGridNextStart && _vyGrid >= _vyGridNextStart){
	// 				_skipToNextLight = true;
	// 			} 
	// 			else if (_vxGrid > _vxGridEnd || _vxGrid == _gridEdgeX){
	// 				_vxGrid = _vxGridStart;
	// 				_vyGrid++;
	// 				if (_vyGrid > _vyGridEnd ||_vyGrid == _gridEdgeY){
	// 					_skipToNextLight = true;
	// 				}
	// 			}
	// 		};
	// 		int _totalIterations = _vDiameter * _vDiameter;
	// 		for (int i2 = 0; i2 < _totalIterations; i2++, IterateExtraVariables()){
	// 			if(_skipToNextLight)
	// 				break;

	// 			VertMap_TotalColorNoBlur[_vxGrid, _vyGrid] = Color.clear;
	// 		}
	// 		if (_skipToNextLight)
	// 			continue;
	// 	}
	// }

	public static void AddToLightsInRangeMap(CustomLight _light, bool _b){
		Vector2i[,] _tiles = _light.GetTilesInRange(_onlyWithColliders: false);

		int _xMin = _light.MyGridCoord.x - _light.Radius;
		int _xMax = _xMin + _tiles.GetLength(0);
		int _xGrid = _xMin;
		int _yGrid = _light.MyGridCoord.y - _light.Radius;
		mIterateVariables IterateExtraVariables = delegate (){
			_xGrid++;
			if (_xGrid == _xMax){
				_xGrid = _xMin;
				_yGrid++;
			}
		};

		for (int i = 0; i < _tiles.Length; i++, IterateExtraVariables()){
			if(_yGrid >= Grid.GridSize.y)
				break;
			if(_yGrid < 0 || _xGrid < 0 || _xGrid >= Grid.GridSize.x)
				continue;

			for (int i2 = 0; i2 < GridMap_LightsInRange[_xGrid, _yGrid].Length; i2++){
				int _index = GridMap_LightsInRange[_xGrid, _yGrid][i2];
				if(_b && _index == -1){
					Vector2 _pos = Grid.Instance.GetWorldPointFromTileCoord(new Vector2i(_xGrid, _yGrid));
					GridMap_LightsInRange[_xGrid, _yGrid][i2] = _light.LightIndex;
					break;
				}
				else if(!_b && _index == _light.LightIndex){
					GridMap_LightsInRange[_xGrid, _yGrid][i2] = -1;
					//ClearCachedLightForTileVertices(_xGrid, _yGrid, i2);
					break;
				}
			}
		}
	}
	// static void ClearCachedLightForTileVertices(int _xGrid, int _yGrid, int _lightIndex) {
	// 	int _vxGrid = _xGrid * UVControllerBasic.MESH_VERTICES_PER_EDGE;
	// 	int _vyGrid = _yGrid * UVControllerBasic.MESH_VERTICES_PER_EDGE;
	// 	int[] _xGridNeighbours;
	// 	int[] _yGridNeighbours;
	// 	int[] _vIndexNeighbours;

	// 	int _vx = 0, _vy = 0;
	// 	mIterateVariables IterateExtraVariables = delegate (){
	// 		_vx++;
	// 		if (_vx == UVControllerBasic.MESH_VERTICES_PER_EDGE){
	// 			_vx = 0;
	// 			_vy++;
	// 		}
	// 	};
	// 	for (int i = 0; i < UVControllerBasic.MESH_VERTEXCOUNT; i++, IterateExtraVariables()){
	// 		CustomLight.GetGridVerticesAtSamePosition(_vx, _vy, _xGrid, _yGrid, _xGrid == 0, true, out _xGridNeighbours, out _yGridNeighbours, out _vIndexNeighbours);
	// 		for (int i2 = 0; i2 < _xGridNeighbours.Length; i2++){
	// 			int _vxGridNeighbour 	= _xGridNeighbours[i2] * UVControllerBasic.MESH_VERTICES_PER_EDGE;
	// 			int _vyGridNeighbour 	= _yGridNeighbours[i2] * UVControllerBasic.MESH_VERTICES_PER_EDGE;
	// 			int _vxNeighbour 		= _vIndexNeighbours[i2] % UVControllerBasic.MESH_VERTICES_PER_EDGE;
	// 			int _vyNeighbour 		= _vIndexNeighbours[i2] - _vxNeighbour;
	// 			VertMap_LightReceived[_vxGridNeighbour + _vxNeighbour, _vyGridNeighbour + _vyNeighbour][_lightIndex] = 0;
	// 		}
	// 	}
	// }

	public static int GetLightsInRangeIndex(int _vxGrid, int _vyGrid, int _lightIndex) {
		int _xGrid = _vxGrid / UVControllerBasic.MESH_VERTICES_PER_EDGE;
		int _yGrid = _vyGrid / UVControllerBasic.MESH_VERTICES_PER_EDGE;

		int[] _lightsHittingVertex = GridMap_LightsInRange[_xGrid, _yGrid];
		for (int i = 0; i < _lightsHittingVertex.Length; i++){
			if (_lightsHittingVertex[i] == _lightIndex){
				return i;
			}
		}
		return -1;
	}

	public static void ForceUpdateAllLights() {
		float _timeStarted = Time.realtimeSinceStartup;

        for (int i = 0; i < AllLights.Count; i++) {
            if (AllLights[i].MyMesh != null)
                AllLights[i].MyMesh.Clear();

			 if (!AllLights[i].isTurnedOn)
                continue;

            AllLights[i].UpdateLight();
        }

        Debug.Log("All Lights Updated: " + (Time.realtimeSinceStartup - _timeStarted) + "s");
    }
}
