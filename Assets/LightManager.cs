using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LightManager : MonoBehaviour {

	public class SinCosTable{
		public static float[] sSinArray;
		public static float[] sCosArray;

		public static void Init(){
			sSinArray = new float[360];
			sCosArray = new float[360];
			
			for(int i = 0; i < 360; i++){
				sSinArray[i] = Mathf.Sin(i * Mathf.Deg2Rad);
				sCosArray[i] = Mathf.Cos(i * Mathf.Deg2Rad);
			}
		}
	}

    public static List<CustomLight> AllLights = new List<CustomLight>();
	public const int MAX_LIGHTS_AFFECTING_VERTEX = 32;
	public static Material 		GridMaterial;
	public static int[,][] 		GridMap_LightsInRangeMap;		// indices for each light that can reach each vertex
	public static Vector4[,] 	VertMap_DomLightIndices; 		// the four most dominant lights' indices for each vertex
	public static Vector4[,] 	VertMap_DomLightIntensities; 	// how strongly the dominant lights hit each vertex
	public static Color[,] 		VertMap_ColorNoBlur; 			// the current color of each vertex (without blur!)

	private delegate void mIterateVariables();


	void Awake(){
		SinCosTable.Init();
		Init();
	}
	private static void Init(){
		GridMaterial = CachedAssets.Instance.MaterialGrid;

		// Grid stuff
		GridMap_LightsInRangeMap = new int[Grid.GridSizeX, Grid.GridSizeY][];
		int _xGrid = 0, _yGrid = 0;
		mIterateVariables IterateGridVariables = delegate (){
			_xGrid++;
			if (_xGrid == Grid.GridSizeX){
				_xGrid = 0;
				_yGrid++;
			}
		};
		int _totalIterations = Grid.GridSizeX * Grid.GridSizeY;
		for (int i = 0; i < _totalIterations; i++, IterateGridVariables()){
			int[] _lightIndices = new int[MAX_LIGHTS_AFFECTING_VERTEX];
			for (int i2 = 0; i2 < _lightIndices.Length; i2++){
				_lightIndices[i2] = -1;
			}

			GridMap_LightsInRangeMap[_xGrid, _yGrid] = _lightIndices;
		}

		// Vertex stuff
		int _mapSizeX = Grid.GridSizeX * UVControllerBasic.MESH_VERTICES_PER_EDGE;
		int _mapSizeY = Grid.GridSizeY * UVControllerBasic.MESH_VERTICES_PER_EDGE;
		VertMap_DomLightIndices 		= new Vector4	[_mapSizeX, _mapSizeY];
		VertMap_DomLightIntensities 	= new Vector4	[_mapSizeX, _mapSizeY];
		VertMap_ColorNoBlur 				= new Color		[_mapSizeX, _mapSizeY];
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
			VertMap_DomLightIndices[_vxGrid, _vyGrid].x = -1;
			VertMap_DomLightIndices[_vxGrid, _vyGrid].y = -1;
			VertMap_DomLightIndices[_vxGrid, _vyGrid].z = -1;
			VertMap_DomLightIndices[_vxGrid, _vyGrid].w = -1;
		}
	}

	void LateUpdate(){
		while (lightsNeedingUpdate.Count > 0){
			CustomLight _light = AllLights[lightsNeedingUpdate.Dequeue()];
			if(_light.isBeingRemoved)
				continue;

			_light.UpdateLight();
		}
		while (lightsNeedingRemoval.Count > 0){
			AllLights[lightsNeedingRemoval.Dequeue()].RemoveLightsEffectOnGrid();
		}
	}


	public static void AddThisToLightsInRangeMap(bool _b){
		Vector2i[,] _tiles = GetTilesInRange(_onlyWithColliders: false);

		int _myLightIndex = AllLights.FindIndex(x => x == this);
		int _xMin = MyGridCoord.x - lightRadius;
		int _xMax = _xMin + _tiles.GetLength(0);
		int _x = _xMin;
		int _y = MyGridCoord.y - lightRadius;
		mIterateVariables IterateExtraVariables = delegate (){
			_x++;
			if (_x == _xMax){
				_x = _xMin;
				_y++;
			}
		};
		for (int i = 0; i < _tiles.Length; i++, IterateExtraVariables()){
			if(_y < 0 || _y >= Grid.GridSizeY)
				break;
			if(_x < 0 || _x >= Grid.GridSizeX)
				continue;

			for (int i2 = 0; i2 < GridMap_LightsInRangeMap[_x, _y].Length; i2++){
				int _index = GridMap_LightsInRangeMap[_x, _y][i2];
				if(_b && _index == -1){
					Vector2 _pos = Grid.Instance.GetWorldPointFromTileCoord(new Vector2i(_x, _y));
					Debug.DrawLine(_pos + new Vector2(0, 0.1f), _pos + new Vector2(0.1f, 0), Color.red, Mathf.Infinity);
					Debug.DrawLine(_pos + new Vector2(0.1f, 0), _pos + new Vector2(0, -0.1f), Color.red, Mathf.Infinity);
					Debug.DrawLine(_pos + new Vector2(0, -0.1f), _pos + new Vector2(-0.1f, 0), Color.red, Mathf.Infinity);
					Debug.DrawLine(_pos + new Vector2(-0.1f, 0), _pos + new Vector2(0, 0.1f), Color.red, Mathf.Infinity);

					GridMap_LightsInRangeMap[_x, _y][i2] = _myLightIndex;
					break;
				}
				else if(!_b && _index == _myLightIndex){
					GridMap_LightsInRangeMap[_x, _y][i2] = -1;
					break;
				}
			}
		}
	}

	public static void ForceUpdateAllLights() {
		float _timeStarted = Time.realtimeSinceStartup;

        for (int i = 0; i < AllLights.Count; i++) {
            if (AllLights[i].lightMesh != null)
                AllLights[i].lightMesh.Clear();

			 if (!AllLights[i].isTurnedOn)
                continue;

            AllLights[i].UpdateLight();
        }
        //CalculateLightingForGrid();
        Debug.Log("All Lights Updated: " + (Time.realtimeSinceStartup - _timeStarted) + "s");
    }

	private static Queue<int> lightsNeedingUpdate = new Queue<int>();
	private static Queue<int> lightsNeedingRemoval = new Queue<int>();
	public static void ScheduleUpdateLights(Vector2i _tilePos){
		int[] _indices = gridMap_LightsInRangeMap[_tilePos.x, _tilePos.y];
		int _index = -1;
		for (int i = 0; i < _indices.Length; i++){
			_index = _indices[i];
			if(_index == -1)
				break;
			if(lightsNeedingUpdate.Contains(_index))
				continue;

			lightsNeedingUpdate.Enqueue(_index);
		}
	}
	public static void ScheduleRemoveLight(int _lightIndex){
		if(lightsNeedingRemoval.Contains(_lightIndex))
			return;

		lightsNeedingRemoval.Enqueue(_lightIndex);
	}
}
