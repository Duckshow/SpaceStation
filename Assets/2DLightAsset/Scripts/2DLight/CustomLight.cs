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

	[NonSerialized]
	public Vector2 MyWorldPos;

	private bool hasRunStart = false;

	// public LightManager.VertMap<bool> VLightMap_Hit;
	// public LightManager.VertMap<float> VLightMap_Intensity;

	private Vector2i vertexLightSize;

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

		// VLightMap_Hit 			= new bool	[Diameter * UVControllerBasic.MESH_VERTICES_PER_EDGE, Diameter * UVControllerBasic.MESH_VERTICES_PER_EDGE];
		// VLightMap_Intensity 	= new float	[Diameter * UVControllerBasic.MESH_VERTICES_PER_EDGE, Diameter * UVControllerBasic.MESH_VERTICES_PER_EDGE];
		// vertexLightSize = new Vector2i(Diameter * UVControllerBasic.MESH_VERTICES_PER_EDGE, Diameter * UVControllerBasic.MESH_VERTICES_PER_EDGE);
		// VLightMap_Hit = new LightManager.VertMap<bool>(vertexLightSize);
		// VLightMap_Intensity = new LightManager.VertMap<float>(vertexLightSize);

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
		if (isTurnedOn && (transform.position.x != MyWorldPos.x || transform.position.y != MyWorldPos.y)){
			Debug.LogWarning(transform.name + "'s position changed while turned on! Resetting!");
			transform.position = MyWorldPos;
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
		MyWorldPos = (Vector2)transform.position;
		GetAllTilesInRange();
		PreparePooledColliders();
		SetVertices();
		DiscardPooledColliders();
		RenderLightMesh();
		UpdatePointCollisionArray();
		ClearCachedVertexColors();
		SetBasicLightInfo();
	}
	public void RemoveLightsEffectOnGrid(){
		GetAllTilesInRange();
		ClearCachedVertexColors();
		SetBasicLightInfo();
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

		// for 	(int y = 0, _yGrid = MyGridCoord.y - Radius; y < Diameter; y++, _yGrid++){
		// 	for (int x = 0, _xGrid = MyGridCoord.x - Radius; x < Diameter; x++, _xGrid++){
		// 		bool _usable = true;
		// 		if (_xGrid < 0 || _xGrid >= Grid.GridSize.x || _yGrid < 0 || _yGrid >= Grid.GridSize.y){
		// 			_usable = false;
		// 		}
		// 		else if(_onlyWithColliders && !ObjectPooler.Instance.HasPoolForID(Grid.Instance.grid[_xGrid, _yGrid].ExactType)){
		// 			_usable = false;
		// 		}

		// 		_tiles[x, y] = new TileReference(_xGrid, _yGrid, _usable);
		// 	}
		// }

		SpaceNavigator.IterateOverLightsTilesOnGrid(this, (SpaceNavigator _spaces) => {
			bool _usable = true;
			Vector2i _gridAxisLengths = SpaceNavigator.GetGridSize();
			Vector2i _gridPos = _spaces.GetGridPos();
			if (_gridPos.x < 0 || _gridPos.x >= _gridAxisLengths.x || _gridPos.y < 0 || _gridPos.y >= _gridAxisLengths.y){
				_usable = false;
			}
			else if(_onlyWithColliders && !ObjectPooler.Instance.HasPoolForID(Grid.Instance.grid[_gridPos.x, _gridPos.y].ExactType)){
				_usable = false;
			}

			Vector2i _lightPos = _spaces.GetLightPos();
			_tiles[_lightPos.x, _lightPos.y] = new TileReference(_gridPos.x, _gridPos.y, _usable);
		});

		return _tiles;
	}

	private void ClearCachedVertexColors() {
		// Vector2i _vLightSize = new Vector2i(Diameter * UVControllerBasic.MESH_VERTICES_PER_EDGE - 1, Diameter * UVControllerBasic.MESH_VERTICES_PER_EDGE - 1);
		// Vector2i _vGridPos 		= LightManager.ConvertToVertexGridSpace(Vector2i.zero, this);
		// Vector2i _vGridPosFirst = _vGridPos;
		// Vector2i _vGridPosLast 	= LightManager.ConvertToVertexGridSpace(_vLightSize, this);

		// mIterateVariables IterateExtraVariables = delegate (){
		// 	_vGridPos.x++;
		// 	if (_vGridPos.x > _vGridPosLast.x){
		// 		_vGridPos.x = _vGridPosFirst.x;
		// 		_vGridPos.y++;
		// 	}
		// };

		// int _totalIterations = (int)(_vLightSize.x * _vLightSize.y);
		// for (int i = 0; i < _totalIterations; i++, IterateExtraVariables()){
		// 	Vector2i _vTilePos = LightManager.ConvertToVertexTileSpace(_vGridPos);
		// 	if (_vGridPos.x > 0 && _vTilePos.x == 0) continue;
		// 	if (_vGridPos.y > 0 && _vTilePos.y == 0) continue;
		// 	if (_vGridPos.x >= LightManager.VGridMap_TotalColorNoBlur.GetLength(0)) continue;
		// 	if (_vGridPos.y >= LightManager.VGridMap_TotalColorNoBlur.GetLength(1)) continue;

		// 	LightManager.VertexSiblings.Setup(_vGridPos);
		// 	LightManager.VertexSiblings.SetValueInVertexGridMap<Color>(LightManager.VGridMap_TotalColorNoBlur, Color.clear, _vGridPos);
		// }

		// SpaceNavigator.IterateOverLightsVerticesOnVGrid(this, (SpaceNavigator _spaces) =>{
		// 	Vector2i _vGridPos = _spaces.GetVertexGridPos();
		// 	LightManager.VertexSiblings.Setup(_vGridPos);
		// 	LightManager.VertexSiblings.SetValueInVertexGridMap<Color>(LightManager.VGridMap_TotalColorNoBlur, Color.clear, _vGridPos);
		// });
		// LightManager.VertMap<Color>.IterateOverVertMap(LightManager.VGridMap_TotalColorNoBlur, (Vector2i _mapPos) =>{
		// 	LightManager.VGridMap_TotalColorNoBlur.SetValue(_mapPos, Color.clear);
		// });
	}

	private void SetBasicLightInfo(){
		// Vector2i _vGridPos 		= LightManager.ConvertToVertexGridSpace(tilesInRange[0, 0].GridPos, Vector2i.zero);
		// Vector2i _vGridPosFirst = _vGridPos;
		// Vector2i _vGridPosLast 	= LightManager.ConvertToVertexGridSpace(tilesInRange.GetLast().GridPos, UVControllerBasic.MESH_VERTICES_PER_EDGE_AS_VECTOR);

		// mIterateVariables IterateExtraVariables = delegate (){
		// 	_vGridPos.x++;
		// 	if (_vGridPos.x >= _vGridPosLast.x){
		// 		_vGridPos.x = _vGridPosFirst.x;
		// 		_vGridPos.y++;
		// 	}
		// };

		// int _totalIterations = (int)(tilesInRange.Length * Mathf.Pow(UVControllerBasic.MESH_VERTICES_PER_EDGE, 2));
		// for (int i = 0; i < _totalIterations; i++, IterateExtraVariables()){
		// 	Vector2i _gLightPos = LightManager.ConvertToLightSpace(_vGridPos, this);
		// 	Vector2i _vTilePos = LightManager.ConvertToVertexTileSpace(_vGridPos);

		// 	if (_gLightPos.x > 0 && _vTilePos.x == 0) continue;
		// 	if (_gLightPos.y > 0 && _vTilePos.y == 0) continue;
		// 	if (!tilesInRange[_gLightPos.x, _gLightPos.y].Usable) continue;

		// 	LightManager.VertexSiblings.Setup(_vGridPos);

		// 	Vector2 _vWorldPos = LightManager.ConvertToWorldSpace(_vGridPos);
		// 	float _distance = (_vWorldPos - MyWorldPos).magnitude;
		// 	bool _illuminated = !IsBeingRemoved && IsInsideLightMesh(_vWorldPos);
		// 	float _lightFromThis = Intensity * Mathf.Pow(1 - (_distance / Radius), 2);

		// 	LightManager.VertexSiblings.SetValueInVertexLightMap<bool>(VLightMap_Hit, _illuminated, this, _vGridPos);
		// 	LightManager.VertexSiblings.SetValueInVertexLightMap<float>(VLightMap_Intensity, _lightFromThis, this, _vGridPos);
		// }

		// SpaceNavigator.IterateOverLightsVerticesOnVGridAndSkipOverlaps(this, (SpaceNavigator _spaces) => {
		// 	Vector2i _lightPos = _spaces.GetLightPos();
		// 	if (!tilesInRange[_lightPos.x, _lightPos.y].Usable) return;

		// 	Vector2i _vGridPos = _spaces.GetVertexGridPos();
		// 	LightManager.VertexSiblings.Setup(_vGridPos);

		// 	Vector2 _vWorldPos = _spaces.GetWorldPos();
		// 	float _distance = (_vWorldPos - MyWorldPos).magnitude;
		// 	bool _illuminated = !IsBeingRemoved && IsInsideLightMesh(_vWorldPos);
		// 	float _lightFromThis = Intensity * Mathf.Pow(1 - (_distance / Radius), 2);

		// 	LightManager.VertexSiblings.SetValueInVertexLightMap<bool>(VLightMap_Hit, _illuminated, this, _vGridPos);
		// 	LightManager.VertexSiblings.SetValueInVertexLightMap<float>(VLightMap_Intensity, _lightFromThis, this, _vGridPos);
		// });

		SpaceNavigator.IterateOverLightsVerticesOnVertexMap(this, (SpaceNavigator _spaces) => {
			Vector2i _lightPos = _spaces.GetLightPos();
			//_spaces.PrintDebugLog();
			if (!tilesInRange[_lightPos.x, _lightPos.y].Usable) return;

			Vector2i _vGridPos = _spaces.GetVertexGridPos();
			Vector2 _vWorldPos = _spaces.GetWorldPos();

			float _distance = (_vWorldPos - MyWorldPos).magnitude;
			bool _illuminated = !IsBeingRemoved && IsInsideLightMesh(_vWorldPos);
			float _lightFromThis = Intensity * Mathf.Pow(1 - (_distance / Radius), 2);

			LightManager.VertexMap.VertexInfo _vertex = LightManager.VertexMap.TryGetVertex(_vGridPos);
			for (int i = 0; i < _vertex.LightsInRange.Length; i++){
				LightManager.VertexMap.VertexInfo.LightInfo _lightInfo = _vertex.LightsInRange[i];
				if (_lightInfo.Index == -1) { 
					break;
				}
				if (_lightInfo.Index == LightIndex){
					_lightInfo.Hit = _illuminated;
					_lightInfo.Intensity = _lightFromThis;
					break;
				}
			}
		});
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
		// int x = 0, y = 0;
		// mIterateVariables IterateExtraVariables = delegate (){
		// 	x++;
		// 	if (x == tilesInRangeWithCollider.GetLength(0)){
		// 		x = 0;
		// 		y++;
		// 	}
		// };
		// int _totalIterations = tilesInRangeWithCollider.GetLength(0) * tilesInRangeWithCollider.GetLength(1);
		// for (int i = 0; i < _totalIterations; i++, IterateExtraVariables()){
		// 	TileReference _tile = tilesInRangeWithCollider[x, y];
		// 	if(!_tile.Usable) continue;

		// 	Tile _t = Grid.Instance.grid[_tile.GridPos.x, _tile.GridPos.y];
		// 	PolygonCollider2D _coll = ObjectPooler.Instance.GetPooledObject<PolygonCollider2D>(_t.ExactType);
        //     if (_coll == null)
        //         continue;

        //     _coll.transform.position = _t.WorldPosition;
        //     pooledColliders.Enqueue(_coll);
        // }
		SpaceNavigator.IterateOverLightsTilesOnGrid(this, (SpaceNavigator _spaces) => {
			Vector2i _lightPos = _spaces.GetLightPos();
			TileReference _tileRef = tilesInRangeWithCollider[_lightPos.x, _lightPos.y];
			if (!_tileRef.Usable) return;

			Tile _tile = Grid.Instance.grid[_tileRef.GridPos.x, _tileRef.GridPos.y];
			PolygonCollider2D _coll = ObjectPooler.Instance.GetPooledObject<PolygonCollider2D>(_tile.ExactType);
			if (_coll == null) return;

			_coll.transform.position = _tile.WorldPosition;
			pooledColliders.Enqueue(_coll);
		});
	}
	void DiscardPooledColliders(){
        int _count = pooledColliders.Count;
		for (int i = 0; i < _count; i++)
            pooledColliders.Dequeue().GetComponent<PoolerObject>().ReturnToPool();
	}
}

