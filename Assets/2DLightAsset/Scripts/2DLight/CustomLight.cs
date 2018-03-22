using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor; // for debug-gizmos only
using System;
using Utilities;

public partial class CustomLight : MonoBehaviour {

    public int Radius = 20;
	public Vector2i radiusVector;
	public Vector2i GetRadiusAsVector() {
		if(radiusVector.magnitude == 0) radiusVector = new Vector2i(Radius, Radius);
		return radiusVector;
	}
	public int Diameter { get { return Radius * 2 + 1; } }
    [Range(0, 1)] public float Intensity = 1;
    
	public byte LightColor = 40; // bright yellow
	public Color GetLightColor() {
		return Mouse.Instance.Coloring.AllColors[LightColor];
	}

    private CanInspect myInspector;

	public int LightIndex = -1;

	public bool isTurnedOn { get; private set; }
	[NonSerialized] public bool IsBeingRemoved;
	[NonSerialized] public Vector2i MyGridCoord;
	void UpdateGridCoord(){
		MyGridCoord = myInspector.MyTileObject.MyTile.GridCoord;
	}

	private delegate void mIterateVariables();

	private Vector2 myWorldPos;

	private bool hasRunStart = false;

	public bool[,] VXLightMap_Hit; // TODO: a huge chunk of all my maps represent vertices on the same worldpos, so... superfluous info. Maybe make a map-class?
	public float[,] VXLightMap_Intensity;

	T TryGetAdjacentValueInVertexGridMap<T>(T[,] _vertexGridMap, int _x, int _y, ref int _failIncrement){
		if (_x < 0 || _y < 0 || _x >= _vertexGridMap.GetLength(0) || _y >= _vertexGridMap.GetLength(1)){
			_failIncrement++;
			return default(T);
		}
		return _vertexGridMap[_x, _y]; ;
	}

	[EasyButtons.Button]
	public void TestGridCoord(){
		Debug.Log(Grid.Instance.GetTileCoordFromWorldPoint(transform.position));
	}

	public void SetLightIndex(int _index) {
		LightIndex = _index;
		transform.name = "Light #" + _index;
	}

	void Awake(){
		LightManager.OnLightInit(this);

		isTurnedOn = true;
		IsBeingRemoved = false;

		myInspector = GetComponent<CanInspect>();

		VXLightMap_Hit 			= new bool	[Diameter * UVControllerBasic.MESH_VERTICES_PER_EDGE, Diameter * UVControllerBasic.MESH_VERTICES_PER_EDGE];
		VXLightMap_Intensity 	= new float	[Diameter * UVControllerBasic.MESH_VERTICES_PER_EDGE, Diameter * UVControllerBasic.MESH_VERTICES_PER_EDGE];

		MeshFilter meshFilter = MeshTransform.GetComponent<MeshFilter>();
        renderer = MeshTransform.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = lightMaterial;
        
		LightMesh = new Mesh();
        meshFilter.mesh = LightMesh;
        LightMesh.name = MESH_NAME;
        LightMesh.MarkDynamic();
	}
	void OnDestroy(){
		LightManager.OnLightDestroyed(this);
	}
	void OnEnable() {
        myInspector.PostPickUp += PostPickUp;
        myInspector.PostPutDown += PostPutDown;
		myInspector.OnHide += OnHide;
		if (hasRunStart){
			TurnOn(_b: true);
		}
	}
    void OnDisable() {
        myInspector.PostPickUp -= PostPickUp;
        myInspector.PostPutDown -= PostPutDown;
		myInspector.OnHide -= OnHide;
		TurnOn(_b: false);
	}
	void Start() {
		hasRunStart = true;
		TurnOn(_b: true);
	}

	void LateUpdate(){
		if (isTurnedOn && (transform.position.x != myWorldPos.x || transform.position.y != myWorldPos.y)){
			Debug.LogWarning(transform.name + "'s position changed while turned on! Resetting!");
			transform.position = myWorldPos;
		}
	}

	void PostPickUp(){ // TODO: would be good if picked-up objects were visible and jumped between tiles when moving. that way the light can update as it's moved as well.
		TurnOn(_b: false);
    }
    void PostPutDown() {
		UpdateGridCoord();
		TurnOn(_b: true);
    }

	void OnHide(bool _b) {
		TurnOn(!_b);
	}
	void TurnOn(bool _b) {
		isTurnedOn = _b;
		OnTurnedOn(_b);
	}
	void OnTurnedOn(bool _b){
		UpdateGridCoord(); // TODO: caching gridcoord is probably unnecessary

		isTurnedOn = _b;
		if (_b) { 
			LightManager.AddToLightsInRangeMap(this, true);
			LightManager.ScheduleUpdateLights(MyGridCoord);
		}
		else { 
			LightManager.ScheduleRemoveLight(LightManager.AllLights.FindIndex(x => x == this));
		}
	}

	public void UpdateLight() {
		myWorldPos = (Vector2)transform.position;
		GetAllTilesInRange();
		PreparePooledColliders();
		SetVertices();
		DiscardPooledColliders();
		RenderLightMesh();
		UpdatePointCollisionArray();
		IterateOverTilesInRange(SetBasicLightInfo);
	}
	public void RemoveLightsEffectOnGrid(){
		GetAllTilesInRange();
		IterateOverTilesInRange(SetBasicLightInfo);
	}
	public void PostProcessLight() {
		GetAllTilesInRange();
		IterateOverTilesInRange(SetPostProcessEffects);
		if(IsBeingRemoved)
			LightManager.AddToLightsInRangeMap(this, false);
	}

    private Tile t;
    private bool breakLoops = false;
	public class TileReference {
		public Vector2i GridPos;
		public bool Usable = true;
		public TileReference(int x, int y, bool usable) { 
			GridPos = new Vector2i(x, y); 
			Usable = usable; 
		}
	}
	private static TileReference[,] tilesInRange;
    private static TileReference[,] tilesInRangeWithCollider;
	void GetAllTilesInRange() {
		tilesInRange 				= GetTilesInRange(_onlyWithColliders: false);
		tilesInRangeWithCollider 	= GetTilesInRange(_onlyWithColliders: true);
	}
	public TileReference[,] GetTilesInRange(bool _onlyWithColliders){
		TileReference[,] _tiles = new TileReference[Diameter, Diameter];

		for 	(int y = 0, _yGrid = MyGridCoord.y - Radius; y < Diameter; y++, _yGrid++){
			for (int x = 0, _xGrid = MyGridCoord.x - Radius; x < Diameter; x++, _xGrid++){
				bool _usable = true;
				if (_xGrid < 0 || _xGrid >= Grid.GridSize.x || _yGrid < 0 || _yGrid >= Grid.GridSize.y){
					_usable = false;
				}
				else if(_onlyWithColliders && !ObjectPooler.Instance.HasPoolForID(Grid.Instance.grid[_xGrid, _yGrid].ExactType)){
					_usable = false;
				}

				_tiles[x, y] = new TileReference(_xGrid, _yGrid, _usable);
			}
		}

		return _tiles;
	}

	static class VertexSiblings { // vertices sharing the same world pos
		public class Sibling {
			public bool Affected = false;
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
				vGridPos = ConvertToVertexGridSpace(gGridPos, vTilePos);
			}

			public void SetUVDots(Vector2 _doubleDot_0, Vector2 _doubleDot_1, Vector2 _doubleDot_2, Vector2 _doubleDot_3) {
				Grid.Instance.grid[gGridPos.x, gGridPos.y].MyUVController.SetUVDots(vTilePos, _doubleDot_0, _doubleDot_1, _doubleDot_2, _doubleDot_3);
			}
			public void SetVertexColor(Color _color){
				Grid.Instance.grid[gGridPos.x, gGridPos.y].MyUVController.SetVertexColor(vTilePos, _color);
			}
			public void SetValueInVertexLightMap<T>(T[,] _vertexLightMap, T _value, CustomLight _light){
				Vector2i _vLightPos = ConvertToVertexLightSpace(vGridPos, _light);
				if (_vLightPos.x < _vertexLightMap.GetLength(0) && _vLightPos.y < _vertexLightMap.GetLength(1)) { 
					_vertexLightMap[_vLightPos.x, _vLightPos.y] = _value;
				}
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
			Vector2i _vTilePos = ConvertToVertexTileSpace(_vGridPos);
			Vector2i _gGridPos = ConvertToGridSpace(_vGridPos);
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
		public static void SetUVDots(Vector2 _doubleDot_0, Vector2 _doubleDot_1, Vector2 _doubleDot_2, Vector2 _doubleDot_3) {
			if (Current.Affected) 			Current.			SetUVDots(_doubleDot_0, _doubleDot_1, _doubleDot_2, _doubleDot_3);
			if (OwnTopHalf.Affected) 		OwnTopHalf.			SetUVDots(_doubleDot_0, _doubleDot_1, _doubleDot_2, _doubleDot_3);
			if (OwnTopHalfLeft.Affected) 	OwnTopHalfLeft.		SetUVDots(_doubleDot_0, _doubleDot_1, _doubleDot_2, _doubleDot_3);
			if (OwnTopHalfRight.Affected) 	OwnTopHalfRight.	SetUVDots(_doubleDot_0, _doubleDot_1, _doubleDot_2, _doubleDot_3);
			if (Top.Affected) 				Top.				SetUVDots(_doubleDot_0, _doubleDot_1, _doubleDot_2, _doubleDot_3);
			if (Right.Affected) 			Right.				SetUVDots(_doubleDot_0, _doubleDot_1, _doubleDot_2, _doubleDot_3);
			if (RightTopHalf.Affected) 		RightTopHalf.		SetUVDots(_doubleDot_0, _doubleDot_1, _doubleDot_2, _doubleDot_3);
			if (RightTopHalfLeft.Affected) 	RightTopHalfLeft.	SetUVDots(_doubleDot_0, _doubleDot_1, _doubleDot_2, _doubleDot_3);
			if (TopRight.Affected) 			TopRight.			SetUVDots(_doubleDot_0, _doubleDot_1, _doubleDot_2, _doubleDot_3);
		}
		public static void SetVertexColor(Color _color, CustomLight _light){
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

		public static void SetValueInVertexGridMap<T>(T[,] _vertexGridMap, T _value) {
			if (Current.Affected) 	_vertexGridMap[Current.vGridPos.x, 	Current.vGridPos.y] 	= _value;
			if (Top.Affected) 		_vertexGridMap[Top.vGridPos.x, 		Top.vGridPos.y] 		= _value;
			if (Right.Affected)		_vertexGridMap[Right.vGridPos.x, 	Right.vGridPos.y] 		= _value;
			if (TopRight.Affected)	_vertexGridMap[TopRight.vGridPos.x, TopRight.vGridPos.y] 	= _value;
		}
		public static void SetValueInVertexLightMap<T>(T[,] _vertexLightMap, T _value, CustomLight _light){
			if (Current.Affected) 	Current.	SetValueInVertexLightMap<T>(_vertexLightMap, _value, _light);
			if (Top.Affected)		Top.		SetValueInVertexLightMap<T>(_vertexLightMap, _value, _light);
			if (Right.Affected)		Right.		SetValueInVertexLightMap<T>(_vertexLightMap, _value, _light);
			if (TopRight.Affected)	TopRight.	SetValueInVertexLightMap<T>(_vertexLightMap, _value, _light);
		}
	}

	void IterateOverTilesInRange(Action<Vector2i> _tileMethod){
		//Vector2i _vGridPosStart = ConvertToVertexGridSpace(new Vector2i(0, 0), this);
		//Vector2i _vGridPosEnd = ConvertToVertexGridSpace(new Vector2i(Diameter * UVControllerBasic.MESH_VERTICES_PER_EDGE, Diameter * UVControllerBasic.MESH_VERTICES_PER_EDGE), this);
		// _vGridPosStart.x = Mathf.Clamp(_vGridPosStart.x, 0, Grid.GridSize.x * UVControllerBasic.MESH_VERTICES_PER_EDGE);
		// _vGridPosStart.y = Mathf.Clamp(_vGridPosStart.y, 0, Grid.GridSize.y * UVControllerBasic.MESH_VERTICES_PER_EDGE);
		// _vGridPosEnd.x = Mathf.Clamp(_vGridPosEnd.x, 0, Grid.GridSize.x * UVControllerBasic.MESH_VERTICES_PER_EDGE);
		// _vGridPosEnd.y = Mathf.Clamp(_vGridPosEnd.y, 0, Grid.GridSize.y * UVControllerBasic.MESH_VERTICES_PER_EDGE);
		//Vector2i _vGridPos = _vGridPosStart;

		Vector2i _vGridPos 		= ConvertToVertexGridSpace(tilesInRange[0, 0].GridPos, Vector2i.zero);
		Vector2i _vGridPosFirst = _vGridPos;
		Vector2i _vGridPosLast 	= ConvertToVertexGridSpace(tilesInRange.GetLast().GridPos, UVControllerBasic.MESH_VERTICES_PER_EDGE_AS_VECTOR);
		// Vector2 _vWorldStart = ConvertToWorldSpace(_vGridPosStart);
		// Vector2 _vWorldEnd = ConvertToWorldSpace(_vGridPosEnd);
		// Debug.Log(_vWorldStart + ", " + _vWorldEnd);
		// SuperDebug.MarkPoint(_vWorldStart, Color.red);
		// SuperDebug.MarkPoint(_vWorldEnd, Color.red);

		mIterateVariables IterateExtraVariables = delegate (){
			_vGridPos.x++;
			if (_vGridPos.x >= _vGridPosLast.x){
				_vGridPos.x = _vGridPosFirst.x;
				_vGridPos.y++;
			}
		};
		// int _totalIterations = (_vGridPosEnd.x - _vGridPosStart.x) * (_vGridPosEnd.y - _vGridPosStart.y);
		// for (int i = 0; i < _totalIterations; i++, IterateExtraVariables()){
		// 	Vector2i _gLightPos = ConvertToLightSpace(_vGridPos, this);
		// 	Vector2i _vTilePos = ConvertToVertexTileSpace(_vGridPos);
		// 	if (_gLightPos.x > 0 && _vTilePos.x == 0) continue;
		// 	if (_gLightPos.y > 0 && _vTilePos.y == 0) continue;
		// 	if (tilesInRange[_gLightPos.x, _gLightPos.y].x == -1) continue;

		// 	_tileMethod(_vGridPos);
		// }

		int _totalIterations = (int)(tilesInRange.Length * Mathf.Pow(UVControllerBasic.MESH_VERTICES_PER_EDGE, 2));
		for (int i = 0; i < _totalIterations; i++, IterateExtraVariables()){
			Vector2i _gLightPos = ConvertToLightSpace(_vGridPos, this);
			Vector2i _vTilePos = ConvertToVertexTileSpace(_vGridPos);

			// if(LightIndex == 0)
			// 	SuperDebug.MarkPoint(ConvertToWorldSpace(_vGridPos), Color.magenta);
			// if (LightIndex == 1)
			// 	SuperDebug.MarkPoint(ConvertToWorldSpace(_vGridPos), Color.cyan);

			if (_gLightPos.x > 0 && _vTilePos.x == 0) continue;
			if (_gLightPos.y > 0 && _vTilePos.y == 0) continue;
			if (!tilesInRange[_gLightPos.x, _gLightPos.y].Usable) continue;

			_tileMethod(_vGridPos);
		}
	}
	private void SetBasicLightInfo(Vector2i _vGridPos){
		VertexSiblings.Setup(_vGridPos);

		Vector2i _vTilePos = ConvertToVertexTileSpace(_vGridPos);
		Vector2i _gLightPos = ConvertToLightSpace(_vGridPos, this);
		Vector2i _gGridPos = ConvertToGridSpace(_vGridPos);
		Vector2 _vWorldPos = ConvertToWorldSpace(_vGridPos);
		int _vTileIndex = _vTilePos.y * UVControllerBasic.MESH_VERTICES_PER_EDGE + _vTilePos.x;
		bool _illuminated;
		float _lightFromThis;
		Color _vertexColor = GetColorForVertex(
			_vTilePos: 		_vTilePos, 
			_vLightPos: 	ConvertToVertexLightSpace(_vGridPos, this), 
			_gLightPos: 	_gLightPos, 
			_vGridPos: 		_vGridPos, 
			_gGridPos:		_gGridPos, 
			_vWorldPos: 	_vWorldPos, 
			_vIndex: 		_vTileIndex, 
			_illuminated: 	out _illuminated, 
			_lightFromThis: out _lightFromThis
		);
		//if (LightIndex == 1){
			// if(IsBeingRemoved)
			// 	SuperDebug.MarkPoint(_vWorldPos, Color.red);
			// else
			// 	SuperDebug.MarkPoint(_vWorldPos, Color.cyan);
		//}

		VertexSiblings.SetValueInVertexLightMap<bool>(VXLightMap_Hit, _illuminated, this);
		VertexSiblings.SetValueInVertexLightMap<float>(VXLightMap_Intensity, _lightFromThis, this);
		VertexSiblings.SetValueInVertexGridMap<Color>(LightManager.VertMap_TotalColorNoBlur, _vertexColor);
	}
	
	private void SetPostProcessEffects(Vector2i _vGridPos) {
		VertexSiblings.Setup(_vGridPos);
		
		Vector2i _vTilePos = ConvertToVertexTileSpace(_vGridPos);
		Vector2i _gGridPos = ConvertToGridSpace(_vGridPos);
		Vector2 _vWorldPos = ConvertToWorldSpace(_vGridPos);

		Vector4 _dominantLightIndices = GetShadowCastingLightsIndices(_vGridPos, _gGridPos);
		Vector2 _doubleDot_0 = _dominantLightIndices.x >= 0 ? GetDotXY(_vWorldPos, LightManager.AllLights[(int)_dominantLightIndices.x]) : Vector2.zero;
		Vector2 _doubleDot_1 = _dominantLightIndices.y >= 0 ? GetDotXY(_vWorldPos, LightManager.AllLights[(int)_dominantLightIndices.y]) : Vector2.zero;
		Vector2 _doubleDot_2 = _dominantLightIndices.z >= 0 ? GetDotXY(_vWorldPos, LightManager.AllLights[(int)_dominantLightIndices.z]) : Vector2.zero;
		Vector2 _doubleDot_3 = _dominantLightIndices.w >= 0 ? GetDotXY(_vWorldPos, LightManager.AllLights[(int)_dominantLightIndices.w]) : Vector2.zero;

		Grid.Instance.grid[_gGridPos.x, _gGridPos.y].MyUVController.DominantLights = _dominantLightIndices;
		VertexSiblings.SetUVDots(_doubleDot_0, _doubleDot_1, _doubleDot_2, _doubleDot_3);

		int _failAmount = 0;
		int _diffToRightX = _vTilePos.x == 0 ? 1 : 2; // TODO: this can't be right. It should be == 1, right?
		int _diffToAboveY = _vTilePos.y == 0 ? 1 : 2;

		Color _vertexColor = Color.clear;
		_vertexColor += TryGetAdjacentValueInVertexGridMap(LightManager.VertMap_TotalColorNoBlur, _vGridPos.x - 1, _vGridPos.y - 1, ref _failAmount);
		_vertexColor += TryGetAdjacentValueInVertexGridMap(LightManager.VertMap_TotalColorNoBlur, _vGridPos.x, _vGridPos.y - 1, ref _failAmount);
		_vertexColor += TryGetAdjacentValueInVertexGridMap(LightManager.VertMap_TotalColorNoBlur, _vGridPos.x + _diffToRightX, _vGridPos.y - 1, ref _failAmount);
		_vertexColor += TryGetAdjacentValueInVertexGridMap(LightManager.VertMap_TotalColorNoBlur, _vGridPos.x - 1, _vGridPos.y, ref _failAmount);
		_vertexColor += TryGetAdjacentValueInVertexGridMap(LightManager.VertMap_TotalColorNoBlur, _vGridPos.x, _vGridPos.y, ref _failAmount); // this vertex
		_vertexColor += TryGetAdjacentValueInVertexGridMap(LightManager.VertMap_TotalColorNoBlur, _vGridPos.x + _diffToRightX, _vGridPos.y, ref _failAmount);
		_vertexColor += TryGetAdjacentValueInVertexGridMap(LightManager.VertMap_TotalColorNoBlur, _vGridPos.x - 1, _vGridPos.y + _diffToAboveY, ref _failAmount);
		_vertexColor += TryGetAdjacentValueInVertexGridMap(LightManager.VertMap_TotalColorNoBlur, _vGridPos.x, _vGridPos.y + _diffToAboveY, ref _failAmount);
		_vertexColor += TryGetAdjacentValueInVertexGridMap(LightManager.VertMap_TotalColorNoBlur, _vGridPos.x + _diffToRightX, _vGridPos.y + _diffToAboveY, ref _failAmount);
		_vertexColor /= Mathf.Max(Mathf.Pow(UVControllerBasic.MESH_VERTICES_PER_EDGE, 2) - _failAmount, 1);
		_vertexColor.a = 1;

		VertexSiblings.SetVertexColor(_vertexColor, this);
	}

	static Vector2i ConvertToVertexGridSpace(Vector2i _vLightPos, CustomLight _light){
		return _vLightPos + Vector2i.Scale(_light.MyGridCoord - _light.GetRadiusAsVector(), UVControllerBasic.MESH_VERTICES_PER_EDGE_AS_VECTOR);
	}
	static Vector2i ConvertToVertexGridSpace(Vector2i _gGridPos, Vector2i _vTilePos){
		return _gGridPos * UVControllerBasic.MESH_VERTICES_PER_EDGE + _vTilePos;
	}
	static Vector2i ConvertToGridSpace(Vector2i _vGridPos){
		return _vGridPos / UVControllerBasic.MESH_VERTICES_PER_EDGE;
	}
	static Vector2i ConvertToLightSpace(Vector2i _vGridPos, CustomLight _light){
		return ConvertToGridSpace(_vGridPos) - (_light.MyGridCoord - _light.GetRadiusAsVector());
	}
	static Vector2i ConvertToVertexLightSpace(Vector2i _vGridPos, CustomLight _light){
		return _vGridPos - Vector2i.Scale(_light.MyGridCoord - _light.GetRadiusAsVector(), UVControllerBasic.MESH_VERTICES_PER_EDGE_AS_VECTOR);
	}
	static Vector2i ConvertToVertexTileSpace(Vector2i _vGridPos){ // WARNING: does not support top-half vertices! Confusing? Yes!
		return _vGridPos - ConvertToGridSpace(_vGridPos) * UVControllerBasic.MESH_VERTICES_PER_EDGE;
	}
    Vector2 ConvertToWorldSpace(Vector2i _vGridPos){
		Vector2i _gGridPos 	= ConvertToGridSpace(_vGridPos);
		Vector2 _localPos 	= new Vector2(_vGridPos.x * UVControllerBasic.MESH_VERTEX_SEPARATION, _vGridPos.y * UVControllerBasic.MESH_VERTEX_SEPARATION);
		Vector2 _correction = new Vector2(_gGridPos.x * UVControllerBasic.MESH_VERTEX_SEPARATION, _gGridPos.y * UVControllerBasic.MESH_VERTEX_SEPARATION); // discount every third vertex (except first) since they overlap
		Vector2 _gridWorldPos = Grid.Instance.transform.position;
		return _gridWorldPos - Grid.GridSizeHalf + _localPos - _correction;
    }

	static Vector2 GetDotXY(Vector2 _vWorldPos, CustomLight _light){
        // get an angle between 0->1. The angle goes all the way around, but counter-clockwise, so sorta like a clock and unlike a dot
		float _vertical 	= (Vector2.Dot(Vector2.down, (_vWorldPos - _light.myWorldPos).normalized) + 1) * 0.5f;
		float _horizontal 	=  (Vector2.Dot(Vector2.left, (_vWorldPos - _light.myWorldPos).normalized) + 1) * 0.5f;

		return new Vector2(
			Mathf.Max(0.001f, _horizontal), 
			Mathf.Max(0.001f, _vertical)
		);
	}

	private Color GetColorForVertex(Vector2i _vTilePos, Vector2i _vLightPos, Vector2i _gLightPos, Vector2i _vGridPos, Vector2i _gGridPos, Vector2 _vWorldPos,  int _vIndex, out bool _illuminated, out float _lightFromThis){
		// take all previously hit light and recompile into one color
		Color _totalColor = Color.clear;
		int[] _lightsInRange = LightManager.GridMap_LightsInRange[_gGridPos.x, _gGridPos.y];
		int actualCount = 0;
		for (int i = 0; i < _lightsInRange.Length; i++){ // TODO: it's a bit weird that every light will get light from every other light, when we could just have the vertices do this after all lights have sent their individual stuff
			if (_lightsInRange[i] == -1) break;

			CustomLight _otherLight = LightManager.AllLights[_lightsInRange[i]];
			if (_otherLight == this) continue;
			if (_otherLight.IsBeingRemoved) continue;

			Vector2i _vLightPosOtherLight = ConvertToVertexLightSpace(_vGridPos, _otherLight);
			bool _hit = _otherLight.VXLightMap_Hit[_vLightPosOtherLight.x, _vLightPosOtherLight.y];
			if (!_hit) continue;

			actualCount++;
			CombineLightWithOthersLight(_otherLight.VXLightMap_Intensity[_vLightPosOtherLight.x, _vLightPosOtherLight.y], _otherLight.GetLightColor(), ref _totalColor);
		}

		float _distance = (_vWorldPos - myInspector.MyTileObject.MyTile.WorldPosition).magnitude;
		_lightFromThis = Intensity * Mathf.Pow(1 - (_distance / Radius), 2);
		_illuminated = !IsBeingRemoved && IsInsideLightMesh(_vWorldPos);

		if(_illuminated){
			CombineLightWithOthersLight(_lightFromThis, GetLightColor(), ref _totalColor);
		}

		_totalColor.a = 1;
		return _totalColor;
    }
	private void CombineLightWithOthersLight(float _newIntensity, Color _newColor, ref Color _total) {
		_newColor *= _newIntensity;
		if (_total.r < _newColor.r)
			_total.r += _newColor.r;
		if (_total.g < _newColor.g)
			_total.g += _newColor.g;
		if (_total.b < _newColor.b)
			_total.b += _newColor.b;
	}

	private Vector4 GetShadowCastingLightsIndices(Vector2i _vGridPos, Vector2i _gGridPos){
		Vector4 _dominantIndices 	= LightManager.VertMap_DomLightIndices		[_vGridPos.x, _vGridPos.y];
		Vector4 _dominantIntensities = LightManager.VertMap_DomLightIntensities	[_vGridPos.x, _vGridPos.y];

		for (int i = 0; i < LightManager.GridMap_LightsInRange[_gGridPos.x, _gGridPos.y].Length; i++){
			int _lightIndex = LightManager.GridMap_LightsInRange[_gGridPos.x, _gGridPos.y][i];
			if (_lightIndex == -1) break;

			CustomLight _otherLight = LightManager.AllLights[_lightIndex];
			if(IsLightAlreadyApplied(_otherLight, ref _dominantIndices.x, ref _dominantIntensities.x)) continue;
			if(IsLightAlreadyApplied(_otherLight, ref _dominantIndices.y, ref _dominantIntensities.y)) continue;
			if(IsLightAlreadyApplied(_otherLight, ref _dominantIndices.z, ref _dominantIntensities.z)) continue;
			if(IsLightAlreadyApplied(_otherLight, ref _dominantIndices.w, ref _dominantIntensities.w)) continue;


			Vector2i _vLightPos = ConvertToVertexLightSpace(_vGridPos, _otherLight);
			bool _hit = _otherLight.VXLightMap_Hit[_vLightPos.x, _vLightPos.y];
			float _intensity = _hit ? _otherLight.VXLightMap_Intensity[_vLightPos.x, _vLightPos.y] : 0;

			float _lowestDominance = 10000;
			int _lowestDominanceIndex = -1;
			IsChannelValueLowerThanPreviousLow(0, _dominantIntensities.x, ref _lowestDominanceIndex, ref _lowestDominance);
			IsChannelValueLowerThanPreviousLow(1, _dominantIntensities.y, ref _lowestDominanceIndex, ref _lowestDominance);
			IsChannelValueLowerThanPreviousLow(2, _dominantIntensities.z, ref _lowestDominanceIndex, ref _lowestDominance);
			IsChannelValueLowerThanPreviousLow(3, _dominantIntensities.w, ref _lowestDominanceIndex, ref _lowestDominance);

			if (_lowestDominanceIndex == 0){
				_dominantIndices.x = _lightIndex;
				_dominantIntensities.x = _intensity;
			}
			if (_lowestDominanceIndex == 1){
				_dominantIndices.y = _lightIndex;
				_dominantIntensities.y = _intensity;
			}
			if (_lowestDominanceIndex == 2){
				_dominantIndices.z = _lightIndex;
				_dominantIntensities.z = _intensity;
			}
			if (_lowestDominanceIndex == 3){
				_dominantIndices.w = _lightIndex;
				_dominantIntensities.w = _intensity;
			}
		}

		LightManager.VertMap_DomLightIndices[_vGridPos.x, _vGridPos.y] = _dominantIndices;
		LightManager.VertMap_DomLightIntensities[_vGridPos.x, _vGridPos.y] = _dominantIntensities;
		return _dominantIndices;
	}
	bool IsLightAlreadyApplied(CustomLight _light, ref float _index, ref float _intensity){
		if(_index == _light.LightIndex){
			if (_light.IsBeingRemoved){
				_index = -1;
				_intensity = -1;
			}
			return true;
		}
		return false;
	}
	void IsChannelValueLowerThanPreviousLow(int _index, float _value, ref int _lowestIndex, ref float _lowest){
		if (_value < _lowest){
			_lowest = _value;
			_lowestIndex = _index;
		}
	}

	private const float VERTEX_ON_EDGE_TOLERANCE = 0.01f;
	public Vector2[] PointCollisionArray;
	void UpdatePointCollisionArray(){
		// cache vertices relative to world - but skip zero as it messes with the IsInsideLightMesh-algorithm
		PointCollisionArray = new Vector2[LightMesh.vertexCount - 1];
		Vector3[] _vertices = LightMesh.vertices;
		for (int i = 0; i < PointCollisionArray.Length; i++){
			Vector3 _vertex = _vertices[i + 1]; 
			Vector3 _dir = (_vertex - transform.position).normalized;
			PointCollisionArray[i] = transform.position + _vertex + _dir * VERTEX_ON_EDGE_TOLERANCE;
		}
	}

	Queue<PolygonCollider2D> pooledColliders = new Queue<PolygonCollider2D>();
	void PreparePooledColliders() {
		int x = 0, y = 0;
		mIterateVariables IterateExtraVariables = delegate (){
			x++;
			if (x == tilesInRangeWithCollider.GetLength(0)){
				x = 0;
				y++;
			}
		};
		int _totalIterations = tilesInRangeWithCollider.GetLength(0) * tilesInRangeWithCollider.GetLength(1);
		for (int i = 0; i < _totalIterations; i++, IterateExtraVariables()){
			TileReference _tile = tilesInRangeWithCollider[x, y];
			if(!_tile.Usable) continue;

			Tile _t = Grid.Instance.grid[_tile.GridPos.x, _tile.GridPos.y];
			PolygonCollider2D _coll = ObjectPooler.Instance.GetPooledObject<PolygonCollider2D>(_t.ExactType);
            if (_coll == null)
                continue;

            _coll.transform.position = _t.WorldPosition;
            pooledColliders.Enqueue(_coll);
        }
	}
	void DiscardPooledColliders(){
        int _count = pooledColliders.Count;
		for (int i = 0; i < _count; i++)
            pooledColliders.Dequeue().GetComponent<PoolerObject>().ReturnToPool();
	}
}

