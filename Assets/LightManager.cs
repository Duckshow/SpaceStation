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
	public static Color[,] 		VertMap_TotalColorNoBlur; 		// the current color of each vertex (without blur!)
	public static Vector4[,]	VertMap_DomLightIndices;
	public static Vector4[,]	VertMap_DomLightIntensities;

	private delegate void mIterateVariables();

	private static Vector2i vertMapSize;


	void Awake(){
		SinCosTable.Init();
		Init();
	}
	private static void Init(){
		GridMaterial = CachedAssets.Instance.MaterialGrid;
		vertMapSize = Grid.GridSize * UVControllerBasic.MESH_VERTICES_PER_EDGE;

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

		VertMap_TotalColorNoBlur 	= new Color 	[vertMapSize.x, vertMapSize.y];
		VertMap_DomLightIndices 	= new Vector4	[vertMapSize.x, vertMapSize.y];
		VertMap_DomLightIntensities = new Vector4 	[vertMapSize.x, vertMapSize.y];

		Vector4 _minusOne = new Vector4(-1, -1, -1, -1);
		int _vxGrid = 0, _vyGrid = 0;
		IterateGridVariables = delegate (){
			_vxGrid++;
			if (_vxGrid == Grid.GridSize.x * UVControllerBasic.MESH_VERTICES_PER_EDGE){
				_vxGrid = 0;
				_vyGrid++;
			}
		};
		_totalIterations = Grid.GridSize.x * UVControllerBasic.MESH_VERTICES_PER_EDGE * Grid.GridSize.y * UVControllerBasic.MESH_VERTICES_PER_EDGE;
		for (int i = 0; i < _totalIterations; i++, IterateGridVariables()){
			VertMap_DomLightIndices		[_vxGrid, _vyGrid] = _minusOne;
			VertMap_DomLightIntensities	[_vxGrid, _vyGrid] = _minusOne;
		}
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
		if (GridMap_TilesUpdated[_gGridPos.x, _gGridPos.y]) 
			return;
		GridMap_TilesUpdated[_gGridPos.x, _gGridPos.y] = true;
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
			Vector2i _posGrid = tilesNeedingUpdate.Dequeue();
			int[] _lightsInRange = GridMap_LightsInRange[_posGrid.x, _posGrid.y];
			for (int i = 0; i < _lightsInRange.Length; i++){
				int _index = _lightsInRange[i];
				if (_index == -1) break;
				if (AllLights[_index].IsBeingRemoved) continue;

				if(!lightsToUpdate.Contains(_index))
					lightsToUpdate.Add(_index);
			}

			GridMap_TilesUpdated[_posGrid.x, _posGrid.y] = false;
		}

		if (lightsToUpdate.Count > 0){
			for (int i = 0; i < lightsToUpdate.Count; i++){
				CustomLight _light = AllLights[lightsToUpdate[i]];
				if(_light.IsBeingRemoved) continue;
				_light.UpdateLight();
			}
			for (int i = 0; i < lightsToUpdate.Count; i++){
				CustomLight _light = AllLights[lightsToUpdate[i]];
				if (_light.IsBeingRemoved) continue;
				_light.PostProcessLight();
			}
			lightsToUpdate.Clear();
		}

		if (lightsToRemove.Count > 0){
			for (int i = 0; i < lightsToRemove.Count; i++){
				CustomLight _light = AllLights[lightsToRemove[i]];
				_light.RemoveLightsEffectOnGrid();
			}
			for (int i = 0; i < lightsToRemove.Count; i++){
				CustomLight _light = AllLights[lightsToRemove[i]];
				_light.PostProcessLight();
				_light.IsBeingRemoved = false;
			}
			lightsToRemove.Clear();
		}
	}

	public static void AddToLightsInRangeMap(CustomLight _light, bool _b){
		CustomLight.TileReference[,] _tiles = _light.GetTilesInRange(_onlyWithColliders: false);

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
				int _lightIndex = GridMap_LightsInRange[_xGrid, _yGrid][i2];
				if (_b && _lightIndex == _light.LightIndex){
					break; // already exists for this tile
				}
				else if(_b && _lightIndex == -1){
					Vector2 _pos = Grid.Instance.GetWorldPointFromTileCoord(new Vector2i(_xGrid, _yGrid));
					GridMap_LightsInRange[_xGrid, _yGrid][i2] = _light.LightIndex;
					break;
				}
				else if(!_b && _lightIndex == _light.LightIndex){
					GridMap_LightsInRange[_xGrid, _yGrid].PseudoRemoveAt<int>(i2, _emptyValue: -1);
					break;
				}
			}
		}
	}
}
