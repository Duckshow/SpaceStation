using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor; // for debug-gizmos only
using System;
using Utilities;

public class Verts {
    public float Angle;
    public int Location; // 1 = left end point | 0 = middle | -1 = right endpoint
    public Vector2 Pos;
    public bool Endpoint;
}

public class CustomLight : MonoBehaviour {

    public bool DebugMode = false;

    [Space]

    public string version = "1.0.5"; //release date 09/01/2017
    public Material lightMaterial;
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

	public LayerMask layer;
    [Range(4, 40)] public int lightSegments = 8;
    public Transform MeshTransform;

    //[HideInInspector] public PolygonCollider2D[] allMeshes; // Array for all of the meshes in our scene
    [HideInInspector]
    public List<Verts> allVertices = new List<Verts>(); // Array for all of the vertices in our meshes

	[NonSerialized] public Mesh MyMesh; // Mesh for our light mesh
    private new MeshRenderer renderer;
    private CanInspect myInspector;

	[NonSerialized] public int MyIndex = -1;

	public bool isTurnedOn { get; private set; }
	[NonSerialized] public bool IsBeingRemoved;
	[NonSerialized] public Vector2i MyGridCoord;
	void UpdateGridCoord(){
		MyGridCoord = Grid.Instance.GetTileCoordFromWorldPoint(transform.position);
	}

	private delegate void mIterateVariables();

	private const string MESH_NAME = "Light Mesh";

	public bool[,] VXLightMap_Hit;
	public float[,] VXLightMap_Intensity;

	void AssignValueToVertMap<T>(T[,] _vertexLightMap, Vector2i _vGridPos, T _value) {
		Vector2i _vLightPos = ConvertToVertexLightSpace(_vGridPos, this);
		_vertexLightMap[_vLightPos.x, _vLightPos.y] = _value;

		Vector2i[] _gGridPosNeighbours;
		Vector2i[] _vTilePosNeighbours;
		GetGridVerticesAtSamePosition(
			_gGridPos: 				ConvertToGridSpace(_vGridPos), 
			_vTilePos: 				ConvertToVertexTileSpace(_vGridPos), 
			_isOnLeftEdge: 			_vLightPos.x == 0, 
			_includeTopHalfStuff: 	false, 
			_gGridPosNeighbours: 	out _gGridPosNeighbours, 
			_vTilePosNeighbours: 	out _vTilePosNeighbours
		);
		for (int i = 0; i < _gGridPosNeighbours.Length; i++){
			Vector2i _vGridPosNeighbour = ConvertToVertexGridSpace(_gGridPosNeighbours[i], _vTilePosNeighbours[i]);
			Vector2i _vLightPosNeighbour = ConvertToVertexLightSpace(_vGridPosNeighbour, this);
			// if(_vLightPosNeighbour.x < 0 ||_vLightPosNeighbour.x >= _vertexLightMap.GetLength(0)) 
			// 	continue;
			// if (_vLightPosNeighbour.y < 0 || _vLightPosNeighbour.y >= _vertexLightMap.GetLength(1))
			// 	continue;

			_vertexLightMap[_vLightPosNeighbour.x, _vLightPosNeighbour.y] = _value;
		}
	}


	[EasyButtons.Button]
	public void TestGridCoord(){
		Debug.Log(Grid.Instance.GetTileCoordFromWorldPoint(transform.position));
	}


	void Awake(){
		isTurnedOn = true;
		IsBeingRemoved = false;

		MyIndex = LightManager.AllLights.Count;
		LightManager.AllLights.Add(this);
        
		myInspector = GetComponent<CanInspect>();

        MeshFilter meshFilter = MeshTransform.GetComponent<MeshFilter>();
        renderer = MeshTransform.GetComponent<MeshRenderer>();
        renderer.sharedMaterial = lightMaterial;
        
		MyMesh = new Mesh();
        meshFilter.mesh = MyMesh;
        MyMesh.name = MESH_NAME;
        MyMesh.MarkDynamic();

		VXLightMap_Hit 			= new bool	[Diameter * UVControllerBasic.MESH_VERTICES_PER_EDGE, Diameter * UVControllerBasic.MESH_VERTICES_PER_EDGE];
		VXLightMap_Intensity 	= new float	[Diameter * UVControllerBasic.MESH_VERTICES_PER_EDGE, Diameter * UVControllerBasic.MESH_VERTICES_PER_EDGE];
	}
	void OnDestroy(){
       LightManager.AllLights.Remove(this);		
	}
	void OnEnable() {
        myInspector.PostPickUp += PostPickUp;
        myInspector.PostPutDown += PostPutDown;
		myInspector.OnHide += OnHide;
	}
    void OnDisable() {
        myInspector.PostPickUp -= PostPickUp;
        myInspector.PostPutDown -= PostPutDown;
		myInspector.OnHide -= OnHide;
    }

	void Start(){
		UpdateGridCoord(); // TODO: caching gridcoord is probably unnecessary
		
		if (isTurnedOn){
			OnTurnedOn(_b: true);
		}
	}

    void PostPickUp(){ // TODO: would be good if picked-up objects were visible and jumped between tiles when moving. that way the light can update as it's moved as well.
    }
    void PostPutDown() {
		UpdateGridCoord();
    }

	void OnHide(bool _b) {
		TurnOn(!_b);
	}
	void TurnOn(bool _b) {
		isTurnedOn = _b;
		OnTurnedOn(_b);
	}
	void OnTurnedOn(bool _b){
		isTurnedOn = _b;
		LightManager.AddToLightsInRangeMap(this, _b);
		if(_b)
			LightManager.ScheduleUpdateLights(MyGridCoord);
		else
			LightManager.ScheduleRemoveLight(LightManager.AllLights.FindIndex(x => x == this));
	}

	public void UpdateLight() {
		GetAllTilesInRange();
		PreparePooledColliders();
		SetVertices();
		DiscardPooledColliders();
		RenderLightMesh();
        ResetBounds();
		UpdatePointCollisionArray();
		CalculateLightingForTilesInRange(_excludeMe: false);
	}

	public void RemoveLightsEffectOnGrid(){
		GetAllTilesInRange();
		CalculateLightingForTilesInRange(_excludeMe: true);
	}

    private Tile t;
    private bool breakLoops = false;
    private Vector2i[,] tilesInRange;
    private Vector2i[,] tilesInRangeWithCollider;
	void GetAllTilesInRange() {
		tilesInRange 				= GetTilesInRange(_onlyWithColliders: false);
		tilesInRangeWithCollider 	= GetTilesInRange(_onlyWithColliders: true);
	}
	public Vector2i[,] GetTilesInRange(bool _onlyWithColliders){
		Vector2i[,] _tiles = new Vector2i[Diameter, Diameter];

		for 	(int y = 0, _yGrid = MyGridCoord.y - Radius; y < Diameter; y++, _yGrid++){
			for (int x = 0, _xGrid = MyGridCoord.x - Radius; x < Diameter; x++, _xGrid++){
				_tiles[x, y] = new Vector2i(_xGrid, _yGrid);
				
				if (_xGrid < 0 || _xGrid >= Grid.GridSize.x || _yGrid < 0 || _yGrid >= Grid.GridSize.y){
					_tiles[x, y].x = -1;
					_tiles[x, y].y = -1;
				}
				else if(_onlyWithColliders && !ObjectPooler.Instance.HasPoolForID(Grid.Instance.grid[_xGrid, _yGrid].ExactType)){
					_tiles[x, y].x = -1;
					_tiles[x, y].y = -1;
				}
			}
		}

		return _tiles;
	}

    private bool sortAngles = false;
    private bool lows = false; // check si hay menores a -0.5
    private bool highs = false; // check si hay mayores a 2.0
    private float magRange = 0.15f;
    private List<Verts> tempVerts = new List<Verts>();
    private Verts v;
    private Vector3 worldPoint;
    private RaycastHit2D rayHit;
    private int posLowAngle;
    private int posHighAngle;
    private float lowestAngle;
    private float highestAngle;
    private Vector3 fromCast;
    private bool isEndpoint;
    private Vector2 from;
    private Vector2 dir;
    private float mag;
    private const float CHECK_POINT_LAST_RAY_OFFSET = 0.005f;
    private Vector2 rayCont;
    private Vector3 hitPos;
    private Vector2 newDir;
    private Verts vL;
    private int theta;
    private int amount;
    private float rangeAngleComparision;
    private Verts vertex1;
    private Verts vertex2;
	private delegate void mSortList(List<Verts> _list);
	void SetVertices() {
		mSortList SortList = delegate(List<Verts> _list) {
			_list.Sort((item1, item2) => (item2.Angle.CompareTo(item1.Angle)));
		};

		allVertices.Clear();// Since these lists are populated every frame, clear them first to prevent overpopulation

        magRange = 0.15f;
        tempVerts.Clear();
		int x = 0, y = 0;
		mIterateVariables IterateExtraVariables = delegate (){
			x++;
			if (x == tilesInRangeWithCollider.GetLength(0)){
				x = 0;
				y++;
			}
		};
		int _totalIterations = tilesInRangeWithCollider.GetLength(0) * tilesInRangeWithCollider.GetLength(1);
		for (int i = 0; i < _totalIterations; i++, IterateExtraVariables()) {
			Vector2i _tileCoord = tilesInRangeWithCollider[x, y];
			if(_tileCoord.x < 0 || _tileCoord.y < 0)
				continue;

			Tile _t = Grid.Instance.grid[_tileCoord.x, _tileCoord.y];
            PolygonCollider2D _coll = ObjectPooler.Instance.GetPooledObject<PolygonCollider2D>(_t.ExactType);
            _coll.transform.position = _t.WorldPosition;

			tempVerts.Clear();
            
            // the following variables used to fix sorting bug
            // the calculated angles are in mixed quadrants (1 and 4)
            lows = false; // check for minors at -0.5
            highs = false; // check for majors at 2.0


            for (int pIndex = 0; pIndex < _coll.pathCount; pIndex++){
                for (int vIndex = 0; vIndex < _coll.GetPath(pIndex).Length; vIndex++){ // ...and for every vertex we have of each collider...
                    v = new Verts();

                    // Convert vertex to world space
                    worldPoint = (Vector2)_coll.transform.position + _coll.GetPath(pIndex)[vIndex];
                    if (Gridcast(transform.position, worldPoint, out rayHit)) {
                        v.Pos = rayHit.point;
                        if (worldPoint.sqrMagnitude >= (rayHit.point.sqrMagnitude - magRange) && worldPoint.sqrMagnitude <= (rayHit.point.sqrMagnitude + magRange)) {
                            v.Endpoint = true;
                            if (DebugMode)
                                Debug.DrawLine(transform.position, v.Pos, Color.red);
                        }
                    }
                    else {
                        v.Pos = worldPoint;
                        v.Endpoint = true;
                        if (DebugMode)
                            Debug.DrawLine(transform.position, v.Pos, Color.yellow);
                    }

                    //--Convert To local space for build mesh (mesh craft only in local vertex)
                    v.Pos = transform.InverseTransformPoint(v.Pos);
                    v.Angle = GetVectorAngle(v.Pos.x, v.Pos.y); //--Calculate angle

                    // -- bookmark if an angle is lower than 0 or higher than 2f --//
                    //-- helper method for fix bug on shape located in 2 or more quadrants
                    if (v.Angle < 0f)
                        lows = true;

                    if (v.Angle > 2f)
                        highs = true;

                    //--Add verts to the main list
                    if ((v.Pos).magnitude <= Radius)
                        tempVerts.Add(v);

                    if (sortAngles == false)
                        sortAngles = true;
                }
            }
            _coll.GetComponent<PoolerObject>().ReturnToPool();

            // Identify the endpoints (left and right)
            if (tempVerts.Count > 0) {
                SortList(tempVerts); // sort first

                posLowAngle = 0; // save the indice of left ray
                posHighAngle = 0; // same last in right side

                if (highs == true && lows == true) {  //-- FIX BUG OF SORTING CUANDRANT 1-4 --//
                    lowestAngle = -1f; //tempVerts[0].angle; // init with first data
                    highestAngle = tempVerts[0].Angle;

                    for (int j = 0; j < tempVerts.Count; j++) {
                        if (tempVerts[j].Angle < 1f && tempVerts[j].Angle > lowestAngle) {
                            lowestAngle = tempVerts[j].Angle;
                            posLowAngle = j;
                        }
                        if (tempVerts[j].Angle > 2f && tempVerts[j].Angle < highestAngle) {
                            highestAngle = tempVerts[j].Angle;
                            posHighAngle = j;
                        }
                    }
                }
                else {
                    //-- conventional position of ray points
                    // save the indice of left ray
                    posLowAngle = 0;
                    posHighAngle = tempVerts.Count - 1;
                }

                tempVerts[posLowAngle].Location = 1; // right
                tempVerts[posHighAngle].Location = -1; // left

                //--Add vertices to the main mesh's vertices--//
                allVertices.AddRange(tempVerts);

                // -- r == 0 --> right ray
                // -- r == 1 --> left ray
                for (int r = 0; r < 2; r++) {
                    //-- Cast a ray in same direction continuos mode, start a last point of last ray --//
                    fromCast = new Vector3();
                    isEndpoint = false;

                    if (r == 0) {
                        fromCast = transform.TransformPoint(tempVerts[posLowAngle].Pos);
                        isEndpoint = tempVerts[posLowAngle].Endpoint;
                    }
                    else if (r == 1) {
                        fromCast = transform.TransformPoint(tempVerts[posHighAngle].Pos);
                        isEndpoint = tempVerts[posHighAngle].Endpoint;
                    }

                    if (isEndpoint == true) {
                        dir = fromCast - transform.position;
                        mag = Radius;
                        from = (Vector2)fromCast + (dir * CHECK_POINT_LAST_RAY_OFFSET);

                        int debugger = -1;

                        if(Gridcast(from, (from + dir.normalized * Radius), out rayHit)){
                            hitPos = rayHit.point;
                            debugger = 1;

                            if (Vector2.Distance(hitPos, transform.position) > Radius){
                                dir = transform.InverseTransformDirection(dir); //local p
                                hitPos = transform.TransformPoint(dir.normalized * mag); // world p
                            }
                        }
                        else {
                            newDir = transform.InverseTransformDirection(dir);  //local p
                            hitPos = (Vector2)transform.TransformPoint(newDir.normalized * mag); //world p
                            debugger = 2;
                        }

                        if (DebugMode) { 
                            if(debugger == 0)
                                Debug.DrawLine(fromCast, hitPos, Color.green);
                            if (debugger == 1)
                                Debug.DrawLine(fromCast, hitPos, Color.cyan);
                            if (debugger == 2)
                                Debug.DrawLine(fromCast, hitPos, Color.red);
                            if (debugger == 3)
                                Debug.DrawLine(fromCast, hitPos, Color.magenta);
                        }

                        vL = new Verts();
                        vL.Pos = transform.InverseTransformPoint(hitPos);
                        vL.Angle = GetVectorAngle(vL.Pos.x, vL.Pos.y);
                        allVertices.Add(vL);
                    }
                }
            }
        }
        //--Step 3: Generate vectors for light cast--//
        //---------------------------------------------------------------------//

        theta = 0;
        amount = 360 / lightSegments;
        for (int i = 0; i < lightSegments; i++) {
            theta = amount * i;
            if (theta == 360)
                theta = 0;

            v = new Verts();
            v.Pos = new Vector3((LightManager.SinCosTable.sSinArray[theta]), (LightManager.SinCosTable.sCosArray[theta]), 0); // in degrees (previous calculate)
            v.Angle = GetVectorAngle(v.Pos.x, v.Pos.y);
            v.Pos *= Radius;
            v.Pos += (Vector2)transform.position;

            if(Gridcast(transform.position, v.Pos, out rayHit))
                v.Pos = transform.InverseTransformPoint(rayHit.point);
            else
                v.Pos = transform.InverseTransformPoint(v.Pos);
            allVertices.Add(v);
        }

        //-- Step 4: Sort each vertex by angle (along sweep ray 0 - 2PI)--//
        //---------------------------------------------------------------------//
        if (sortAngles == true) {
            SortList(allVertices);
        }
        //-----------------------------------------------------------------------------


        //--auxiliary step (change order vertices close to light first in position when has same direction) --//
        rangeAngleComparision = 0.00001f;
        for (int i = 0; i < allVertices.Count - 1; i++) {

            vertex1 = allVertices[i];
            vertex2 = allVertices[i + 1];

            // -- Compare the local angle of each vertex and decide if we have to make an exchange-- //
            if (vertex1.Angle >= vertex2.Angle - rangeAngleComparision && vertex1.Angle <= vertex2.Angle + rangeAngleComparision) {
                if (vertex2.Location == -1) { // Right Ray
                    if (vertex1.Pos.sqrMagnitude > vertex2.Pos.sqrMagnitude) {
                        allVertices[i] = vertex2;
                        allVertices[i + 1] = vertex1;
                    }
                }

                // ALREADY DONE!!
                if (vertex1.Location == 1) { // Left Ray
                    if (vertex1.Pos.sqrMagnitude < vertex2.Pos.sqrMagnitude) {
                        allVertices[i] = vertex2;
                        allVertices[i + 1] = vertex1;
                    }
                }
            }
        }
	}

	// private void ClearCachedVColorsForTilesInRange() {
	// 	int vxGrid = 0, vyGrid = 0;
	// 	mIterateVariables IterateExtraVariables = delegate (){
	// 		vxGrid++;
	// 		if (vxGrid == Grid.GridSizeX * UVControllerBasic.MESH_VERTICES_PER_EDGE){
	// 			vxGrid = 0;
	// 			vyGrid++;
	// 		}
	// 	};
	// 	for (int i = 0; i < LightManager.VertMap_TotalColorNoBlur.Length; i++, IterateExtraVariables()){
	// 		LightManager.VertMap_TotalColorNoBlur[vxGrid, vyGrid] = Color.black;
	// 	}
	// }

	private delegate Color mTryGetNeighbourColor(int _x, int _y);
	private void CalculateLightingForTilesInRange(bool _excludeMe){
		// find colors and dots for all lights and apply
		Vector2[] _dots = new Vector2[4];

		Vector2i _vGridPosStart = ConvertToVertexGridSpace(new Vector2i(0, 0), this);
		Vector2i _vGridPosEnd 	= ConvertToVertexGridSpace(new Vector2i((Diameter - 1) * UVControllerBasic.MESH_VERTICES_PER_EDGE, (Diameter - 1) * UVControllerBasic.MESH_VERTICES_PER_EDGE), this);
		_vGridPosStart.x 	= Mathf.Clamp(_vGridPosStart.x, 0, Grid.GridSize.x * UVControllerBasic.MESH_VERTICES_PER_EDGE);
		_vGridPosStart.y 	= Mathf.Clamp(_vGridPosStart.y, 0, Grid.GridSize.y * UVControllerBasic.MESH_VERTICES_PER_EDGE);
		_vGridPosEnd.x 		= Mathf.Clamp(_vGridPosEnd.x,	0, Grid.GridSize.x * UVControllerBasic.MESH_VERTICES_PER_EDGE);
		_vGridPosEnd.y 		= Mathf.Clamp(_vGridPosEnd.y, 	0, Grid.GridSize.y * UVControllerBasic.MESH_VERTICES_PER_EDGE);
		Vector2i _vGridPos = _vGridPosStart;

		mIterateVariables IterateExtraVariables = delegate (){
			_vGridPos.x++;
			if (_vGridPos.x >= _vGridPosEnd.x){
				_vGridPos.x = _vGridPosStart.x;
				_vGridPos.y++;
			}
		};
		int _totalIterations = (_vGridPosEnd.x - _vGridPosStart.x) * (_vGridPosEnd.y - _vGridPosStart.y);
		for (int i = 0; i < _totalIterations; i++, IterateExtraVariables()){
			Vector2i _gLightPos = ConvertToLightSpace(_vGridPos, this);
			Vector2i _vTilePos = ConvertToVertexTileSpace(_vGridPos);

			if (_gLightPos.x > 0 && _vTilePos.x == 0) continue;
			if (_gLightPos.y > 0 && _vTilePos.y == 0) continue;
			if(tilesInRange[_gLightPos.x, _gLightPos.y].x < 0) 	continue;

			Vector2i _gGridPos = ConvertToGridSpace(_vGridPos);
			Vector2i _vLightPos = ConvertToVertexLightSpace(_vGridPos, this);
			Vector2 _vWorldPos = ConvertToWorldSpace(_vGridPos);
			int _vTileIndex = _vTilePos.y * UVControllerBasic.MESH_VERTICES_PER_EDGE + _vTilePos.x;

			// get colors from lights
			bool _illuminated;
			float _lightFromThis;
			Color _newVertexColor = GetColorForVertex(_vTilePos, _vLightPos, _gLightPos, _vGridPos, _gGridPos, _vWorldPos, _vTileIndex, _excludeMe, out _illuminated, out _lightFromThis);
			
			// get two dots per light describing angle to four strongest lights
			Vector4 _dominantLightIndices = GetShadowCastingLightsIndices(_vGridPos, _excludeMe, _illuminated, _lightFromThis);
			_dots[0] = _dominantLightIndices.x >= 0 ? GetDotXY(_vWorldPos, LightManager.AllLights[(int)_dominantLightIndices.x]) : Vector2.zero;
			_dots[1] = _dominantLightIndices.y >= 0 ? GetDotXY(_vWorldPos, LightManager.AllLights[(int)_dominantLightIndices.y]) : Vector2.zero;
			_dots[2] = _dominantLightIndices.z >= 0 ? GetDotXY(_vWorldPos, LightManager.AllLights[(int)_dominantLightIndices.z]) : Vector2.zero;
			_dots[3] = _dominantLightIndices.w >= 0 ? GetDotXY(_vWorldPos, LightManager.AllLights[(int)_dominantLightIndices.w]) : Vector2.zero;

			Vector2i[] _gGridPosNeighbours;
			Vector2i[] _vTilePosNeighbours;
			GetGridVerticesAtSamePosition(
				_gGridPos:	 			_gGridPos, 
				_vTilePos:				_vTilePos,
				_isOnLeftEdge: 			_gLightPos.x == 0, 
				_includeTopHalfStuff: 	true, 
				_gGridPosNeighbours: 	out _gGridPosNeighbours, 
				_vTilePosNeighbours: 	out _vTilePosNeighbours
			);

			for (int i2 = 0; i2 < _gGridPosNeighbours.Length; i2++){
				Grid.Instance.grid[_gGridPosNeighbours[i2].x, _gGridPosNeighbours[i2].y].MyUVController.SetUVDots(_vTilePosNeighbours[i2], _dots[0], _dots[1], _dots[2], _dots[3]);
			}

			GetGridVerticesAtSamePosition(
				_gGridPos:  			_gGridPos, 
				_vTilePos:				_vTilePos,
				_isOnLeftEdge: 			_gLightPos.x == 0, 
				_includeTopHalfStuff: 	false, 
				_gGridPosNeighbours: 	out _gGridPosNeighbours, 
				_vTilePosNeighbours: 	out _vTilePosNeighbours
			);
			for (int i2 = 0; i2 < _gGridPosNeighbours.Length; i2++){
				Vector2i _vGridSpace = ConvertToVertexGridSpace(_gGridPosNeighbours[i2], _vTilePosNeighbours[i2]);
				LightManager.VertMap_TotalColorNoBlur[_vGridSpace.x, _vGridSpace.y] = _newVertexColor;
			}


			// // DEBUG 
			// for (int i2 = 0; i2 < _gGridPosNeighbours.Length; i2++){
			// 	// if(_xLight == 0 && vx == 0 || _yLight == 0 && vy == 0)
			// 	// 	Grid.Instance.grid[_xGridNeighbours[i2], _yGridNeighbours[i2]].MyUVController.SetVertexColor(_vIndexNeighbours[i2], Color.red);
			// 	// else
			// 		Grid.Instance.grid[_gGridPosNeighbours[i2].x, _gGridPosNeighbours[i2].y].MyUVController.SetVertexColor(_vTilePosNeighbours[i2], _newVertexColor);
			// }
		}

	//DEBUG
	//return;

		// blur and apply colors
		Color[] _adjacentColors = new Color[9];
		
		_vGridPos = _vGridPosStart;
		for (int i = 0; i < _totalIterations; i++, IterateExtraVariables()){
			Vector2i _gLightPos = ConvertToLightSpace(_vGridPos, this);
			Vector2i _vTilePos = ConvertToVertexTileSpace(_vGridPos);

			if (_gLightPos.x > 0 && _vTilePos.x == 0) continue;
			if (_gLightPos.y > 0 && _vTilePos.y == 0) continue;
			if(tilesInRange[_gLightPos.x, _gLightPos.y].x < 0) 	continue;

			Vector2i _gGridPos = ConvertToGridSpace(_vGridPos);
			Vector2i _vLightPos = ConvertToVertexLightSpace(_vGridPos, this);

			// try get colors from neighbouring vertices (for smoothing)
			int _diffToRightX = _vTilePos.x == 0 ? 1 : 2; // TODO: this can't be right. It should be == 1, right?
			int _diffToAboveY = _vTilePos.y == 0 ? 1 : 2;
			int _failAmount = 0;
			mTryGetNeighbourColor TryGetAdjacentColor = delegate (int _x, int _y){
				if (_x < 0 || _y < 0 || _x >= LightManager.VertMap_TotalColorNoBlur.GetLength(0) || _y >= LightManager.VertMap_TotalColorNoBlur.GetLength(1)){
					_failAmount++;
					return Color.clear;
				}
				return LightManager.VertMap_TotalColorNoBlur[_x, _y];
			};

			_adjacentColors[0] = TryGetAdjacentColor(_vGridPos.x - 1, 				_vGridPos.y - 1);
			_adjacentColors[1] = TryGetAdjacentColor(_vGridPos.x, 					_vGridPos.y - 1);
			_adjacentColors[2] = TryGetAdjacentColor(_vGridPos.x + _diffToRightX, 	_vGridPos.y - 1);
			_adjacentColors[3] = TryGetAdjacentColor(_vGridPos.x - 1, 				_vGridPos.y);
			_adjacentColors[4] = TryGetAdjacentColor(_vGridPos.x, _vGridPos.y); // this tile
			_adjacentColors[5] = TryGetAdjacentColor(_vGridPos.x + _diffToRightX, 	_vGridPos.y);
			_adjacentColors[6] = TryGetAdjacentColor(_vGridPos.x - 1, 				_vGridPos.y + _diffToAboveY);
			_adjacentColors[7] = TryGetAdjacentColor(_vGridPos.x, 					_vGridPos.y + _diffToAboveY);
			_adjacentColors[8] = TryGetAdjacentColor(_vGridPos.x + _diffToRightX, 	_vGridPos.y + _diffToAboveY);

			Color _myColor = new Color();
			for (int i2 = 0; i2 < _adjacentColors.Length; i2++)
				_myColor += _adjacentColors[i2];

			_myColor /= Mathf.Max(_adjacentColors.Length - _failAmount, 1);
			_myColor.a = 1;

			Vector2i[] _gGridPosNeighbours;
			Vector2i[] _vTilePosNeighbours;
			GetGridVerticesAtSamePosition(
				_gGridPos:  			_gGridPos, 
				_vTilePos:  			_vTilePos, 
				_isOnLeftEdge: 			_gLightPos.x == 0, 
				_includeTopHalfStuff: 	false, 
				_gGridPosNeighbours: 	out _gGridPosNeighbours, 
				_vTilePosNeighbours: 	out _vTilePosNeighbours
			);
			for (int i2 = 0; i2 < _gGridPosNeighbours.Length; i2++){
				Grid.Instance.grid[_gGridPosNeighbours[i2].x, _gGridPosNeighbours[i2].y].MyUVController.SetVertexColor(_vTilePosNeighbours[i2], _myColor);
			}
		}
	}

	// static int GetXGrid(int _xLight, CustomLight _light){
	// 	return _light.MyGridCoord.x - (_light.Radius - _xLight);
	// }
	// static int GetYGrid(int _yLight, CustomLight _light){
	// 	return _light.MyGridCoord.y - (_light.Radius - _yLight);
	// }
	// static int GetVXGrid(int _xGrid, int _vxTile){
	// 	return _xGrid * UVControllerBasic.MESH_VERTICES_PER_EDGE + _vxTile;
	// }
	// static int GetVYGrid(int _yGrid, int _vyTile){
	// 	return _yGrid * UVControllerBasic.MESH_VERTICES_PER_EDGE + _vyTile;
	// }
	// static int GetXLight(int _xGrid, CustomLight _light){
	// 	return _xGrid - (_light.MyGridCoord.x - _light.Radius);
	// }
	// static int GetYLight(int _yGrid, CustomLight _light){
	// 	return _yGrid - (_light.MyGridCoord.y - _light.Radius);
	// }
	// static int GetVXLight(int _xLight, int _vxTile){
	// 	return _xLight * UVControllerBasic.MESH_VERTICES_PER_EDGE + _vxTile;
	// }
	// static int GetVYLight(int _yLight, int _vyTile){
	// 	return _yLight * UVControllerBasic.MESH_VERTICES_PER_EDGE + _vyTile;
	// }
	// static int GetVXTile(int _vIndex, int _vyTile){
	// 	return _vIndex - _vyTile * UVControllerBasic.MESH_VERTICES_PER_EDGE;
	// }
	// static int GetVYTile(int _vIndex){
	// 	return Mathf.FloorToInt(_vIndex / UVControllerBasic.MESH_VERTICES_PER_EDGE);
	// }
	private const float VERTEX_DISTANCE = 0.5f;
    // float GetVXWorld(int _xGrid, int _vx){
    //     return Grid.Instance.grid[_xGrid, 0].WorldPosition.x + (_vx - 1) * VERTEX_DISTANCE;
    // }
	// float GetVYWorld(int _yGrid, int _vy){
    //     return Grid.Instance.grid[0, _yGrid].WorldPosition.y + (_vy - 1) * VERTEX_DISTANCE;
    // }

	static Vector2i edgeVertexCount = UVControllerBasic.MESH_VERTICES_PER_EDGE_AS_VECTOR;
	static Vector2i ConvertToVertexGridSpace(Vector2i _vLightPos, CustomLight _light){
		return _vLightPos + Vector2i.Scale(edgeVertexCount, _light.MyGridCoord - _light.GetRadiusAsVector());
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
		return _vGridPos - Vector2i.Scale(edgeVertexCount, _light.MyGridCoord - _light.GetRadiusAsVector());
	}
	static Vector2i ConvertToVertexTileSpace(Vector2i _vGridPos){
		return _vGridPos - ConvertToGridSpace(_vGridPos) * UVControllerBasic.MESH_VERTICES_PER_EDGE;
	}
    Vector2 ConvertToWorldSpace(Vector2i _vGridPos){
		Vector2i _gGridPos 	= ConvertToGridSpace(_vGridPos);
		Vector2 _localPos 	= new Vector2(_vGridPos.x * VERTEX_DISTANCE, _vGridPos.y * VERTEX_DISTANCE);
		Vector2 _correction = new Vector2(_gGridPos.x * VERTEX_DISTANCE, _gGridPos.y * VERTEX_DISTANCE); // discount every third vertex (except first) since they overlap
		Vector2 _gridWorldPos = Grid.Instance.transform.position;
		return _gridWorldPos - Grid.GridSizeHalf + _localPos - _correction;
    }

	private enum VertexNeighbourEnum { TileAbove, TopHalf, TopHalfLeft, TopHalfRight, TileRight, TileRightTopHalf, TileRightTopHalfLeft, TileTopRight };
	private static Array vertexNeighbourArray = System.Enum.GetValues(typeof(VertexNeighbourEnum));
	public static void GetGridVerticesAtSamePosition(Vector2i _gGridPos, Vector2i _vTilePos, bool _isOnLeftEdge, bool _includeTopHalfStuff, out Vector2i[] _gGridPosNeighbours, out Vector2i[] _vTilePosNeighbours) {

		int _verticesAffectedCount = 1;
		bool[] _cachedAffectsVertexResults = new bool[vertexNeighbourArray.Length];
		for (int i = 0; i < vertexNeighbourArray.Length; i++){
			bool _affects = AffectsVertex((VertexNeighbourEnum)i, _vTilePos, _gGridPos, _isOnLeftEdge, _includeTopHalfStuff);
			_cachedAffectsVertexResults[i] = _affects;
			if(_affects) _verticesAffectedCount++;
		}
		
		_gGridPosNeighbours = new Vector2i[_verticesAffectedCount];
		_vTilePosNeighbours = new Vector2i[_verticesAffectedCount];
		_gGridPosNeighbours[0] = _gGridPos;
		_vTilePosNeighbours[0] = _vTilePos;

		for (int i = 0, _affectIndex = 1; i < vertexNeighbourArray.Length; i++){
			if (!_cachedAffectsVertexResults[i]) continue;

			VertexNeighbourEnum _neighbourDirection = (VertexNeighbourEnum)i;
			_gGridPosNeighbours[_affectIndex] = GetGridSpacePosForNeighbour(_neighbourDirection, _gGridPos);
			_vTilePosNeighbours[_affectIndex] = GetVertexTilePosForNeighbour(_neighbourDirection, _vTilePos);
			_affectIndex++;
		}
	}

	static bool AffectsVertex(VertexNeighbourEnum _neighbour, Vector2i _vTileSpace, Vector2i _gGridSpace, bool _isOnLeftEdge, bool _includeTopHalf){
		int _vertEdgeMaxIndex = UVControllerBasic.MESH_VERTICES_PER_EDGE - 1;
		switch (_neighbour){
			case VertexNeighbourEnum.TileAbove:
				return _gGridSpace.y + 1 < Grid.GridSize.y && _vTileSpace.y == _vertEdgeMaxIndex;
			case VertexNeighbourEnum.TopHalf:
				return _includeTopHalf && _vTileSpace.y == _vertEdgeMaxIndex;
			case VertexNeighbourEnum.TopHalfLeft:
				return AffectsVertex(VertexNeighbourEnum.TopHalf, _vTileSpace, _gGridSpace, _isOnLeftEdge, _includeTopHalf) && _isOnLeftEdge;
			case VertexNeighbourEnum.TopHalfRight:
				return AffectsVertex(VertexNeighbourEnum.TopHalf, _vTileSpace, _gGridSpace, _isOnLeftEdge, _includeTopHalf) && _vTileSpace.x == _vertEdgeMaxIndex;
			case VertexNeighbourEnum.TileRight:
				return _gGridSpace.x + 1 < Grid.GridSize.x && _vTileSpace.x == _vertEdgeMaxIndex;
			case VertexNeighbourEnum.TileRightTopHalf:
			case VertexNeighbourEnum.TileRightTopHalfLeft:
				return AffectsVertex(VertexNeighbourEnum.TopHalf, _vTileSpace, _gGridSpace, _isOnLeftEdge, _includeTopHalf) && AffectsVertex(VertexNeighbourEnum.TileRight, _vTileSpace, _gGridSpace, _isOnLeftEdge, _includeTopHalf);
			case VertexNeighbourEnum.TileTopRight:
				return AffectsVertex(VertexNeighbourEnum.TileRight, _vTileSpace, _gGridSpace, _isOnLeftEdge, _includeTopHalf) && AffectsVertex(VertexNeighbourEnum.TileAbove, _vTileSpace, _gGridSpace, _isOnLeftEdge, _includeTopHalf);
			default:
				Debug.LogError(_neighbour.ToString() + " hasn't been properly implemented yet!");
				return false;
		}
	}
	static Vector2i GetGridSpacePosForNeighbour(VertexNeighbourEnum _neighbour, Vector2i _gGridPos){
		switch (_neighbour){
			case VertexNeighbourEnum.TopHalf:
			case VertexNeighbourEnum.TopHalfLeft:
			case VertexNeighbourEnum.TopHalfRight:{
				break;
			}
			case VertexNeighbourEnum.TileRight:
			case VertexNeighbourEnum.TileRightTopHalf:
			case VertexNeighbourEnum.TileRightTopHalfLeft:{
				_gGridPos.x += 1;
				break;
			}
			case VertexNeighbourEnum.TileAbove:{
				_gGridPos.y += 1;
				break;
			}
			case VertexNeighbourEnum.TileTopRight:{
				_gGridPos.x += 1;
				_gGridPos.y += 1;
				break;
			}
			default:{
				Debug.LogError(_neighbour.ToString() + " hans't been properly implemented yet!");
				break;
			}
		}
		return _gGridPos;
	}
	static Vector2i GetVertexTilePosForNeighbour(VertexNeighbourEnum _neighbour, Vector2i _vTilePos){
		switch (_neighbour){
			case VertexNeighbourEnum.TileAbove: {
				_vTilePos.y = 0;
				break;
			}			
			case VertexNeighbourEnum.TopHalf:{
				_vTilePos.y += 1;
				break;
			}
			case VertexNeighbourEnum.TopHalfLeft:{
				_vTilePos.x = 0;
				_vTilePos.y += 2;
				break;
			}
			case VertexNeighbourEnum.TopHalfRight:{
				_vTilePos.x = 1;  // 1 because top-half has two, not three, vertices ^^'
				_vTilePos.y += 2;
				break;
			}
			case VertexNeighbourEnum.TileRight:{
				_vTilePos.x = 0;
				break;
			}
			case VertexNeighbourEnum.TileRightTopHalf:{
				_vTilePos.x = 0;
				_vTilePos.y += 1;
				break;
			}
			case VertexNeighbourEnum.TileRightTopHalfLeft:{
				_vTilePos.x = 0;
				_vTilePos.y += 2;
				break;
			}
			case VertexNeighbourEnum.TileTopRight:{
				_vTilePos.x = 0;
				_vTilePos.y = 0;
				break;
			}	
			default:{
				Debug.LogError(_neighbour.ToString() + " hans't been properly implemented yet!");
				break;
			}
		}
		return _vTilePos;
	}

	static float GetAngle01(Vector2 _pos1, Vector2 _pos2, Vector2 _referenceAngle, int maxAngle) { // TODO: replace with GetAngleClockwise!
        return maxAngle * (0.5f * (1 + Vector2.Dot(
                                            _referenceAngle, 
                                            Vector3.Normalize(_pos1 - _pos2))));
    }
	// find the horizontal and vertical dot-products between two vectors
	static Vector2 GetDotXY(Vector2 _vWorldPos, CustomLight _light){
        Vector2 _lightPos = _light.transform.position;

        // get an angle between 0->1. The angle goes all the way around, but counter-clockwise, so sorta like a clock and unlike a dot
		float _vertical 	= (Vector2.Dot(Vector2.down, (_vWorldPos - _lightPos).normalized) + 1) * 0.5f;
		float _horizontal 	=  Vector2.Dot(Vector2.left, (_vWorldPos - _lightPos).normalized);

		_vertical *= 0.5f;
		if (_horizontal < 0)
			_vertical = Mathf.Abs(_vertical - 1);

		_horizontal = _vertical + 0.25f;
		_horizontal -= Mathf.Floor(_horizontal);

		return new Vector2(
			Mathf.Max(0.001f, GetDotifiedAngle(_horizontal)), 
			Mathf.Max(0.001f, GetDotifiedAngle(_vertical))
		);
	}
    static float GetDotifiedAngle(float _angle){ // Take an angle (between 0, 1) and convert to something like 0->1->0
        _angle *= 2;
        float _floored = Mathf.Floor(_angle);
        return Mathf.Abs(_floored - (_angle - _floored));
    }

	struct LightIndexLevelPairClass{
		public float Index;
		public float Level;
		public LightIndexLevelPairClass(float i, float l) { Index = i; Level = l; }
		public void Set(float i, float l) { Index = i; Level = l; }
	}
	private static LightIndexLevelPairClass[] lightLevelList = new LightIndexLevelPairClass[] {
		new LightIndexLevelPairClass(-1, 0),
		new LightIndexLevelPairClass(-1, 0),
		new LightIndexLevelPairClass(-1, 0),
		new LightIndexLevelPairClass(-1, 0)
	};
	private Color GetColorForVertex(Vector2i _vTilePos, Vector2i _vLightPos, Vector2i _gLightPos, Vector2i _vGridPos, Vector2i _gGridPos, Vector2 _vWorldPos,  int _vIndex, bool _excludeMe, out bool _illuminated, out float _lightFromThis){
		Color _cachedColor = Color.clear;

		// take all previously hit light and recompile into one color
		int[] _lightsInRange = LightManager.GridMap_LightsInRange[_gGridPos.x, _gGridPos.y];
		int actualCount = 0;
		for (int i = 0; i < _lightsInRange.Length; i++){
			if(_lightsInRange[i] == -1)
				break;

			CustomLight _otherLight = LightManager.AllLights[_lightsInRange[i]];
			if(_otherLight == this)
				continue;
		
			Vector2i _vLightPosOtherLight = ConvertToVertexLightSpace(_vGridPos, _otherLight);
			if(!_otherLight.VXLightMap_Hit[_vLightPosOtherLight.x, _vLightPosOtherLight.y])
				continue;

			actualCount++;
			CombineLightWithOthersLight(_otherLight.VXLightMap_Intensity[_vLightPosOtherLight.x, _vLightPosOtherLight.y], _otherLight.GetLightColor(), ref _cachedColor);
		}

		float _distance = (_vWorldPos - myInspector.MyTileObject.MyTile.WorldPosition).magnitude;
		_lightFromThis = Intensity * Mathf.Pow(1 - (_distance / Radius), 2);
		_illuminated = !_excludeMe && IsInsideLightMesh(_vWorldPos);

		// apply new light to total color
		AssignValueToVertMap<bool>(VXLightMap_Hit, _vGridPos, _illuminated);
		AssignValueToVertMap<float>(VXLightMap_Intensity, _vGridPos, _lightFromThis);
		if(_illuminated){
			CombineLightWithOthersLight(_lightFromThis, GetLightColor(), ref _cachedColor);
		}

		_cachedColor.a = 1;
		LightManager.VertMap_TotalColorNoBlur[_vGridPos.x, _vGridPos.y] = _cachedColor;
		return _cachedColor;
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
	private Vector4 GetShadowCastingLightsIndices(Vector2i _vGridPos, bool _excludeMe, bool _illuminated, float _lightFromThis){
		lightLevelList[0].Index = LightManager.VertMap_DomLightIndices[_vGridPos.x, _vGridPos.y].x;
		lightLevelList[1].Index = LightManager.VertMap_DomLightIndices[_vGridPos.x, _vGridPos.y].y;
		lightLevelList[2].Index = LightManager.VertMap_DomLightIndices[_vGridPos.x, _vGridPos.y].z;
		lightLevelList[3].Index = LightManager.VertMap_DomLightIndices[_vGridPos.x, _vGridPos.y].w;

		lightLevelList[0].Level = LightManager.VertMap_DomLightIntensities[_vGridPos.x, _vGridPos.y].x;
		lightLevelList[1].Level = LightManager.VertMap_DomLightIntensities[_vGridPos.x, _vGridPos.y].y;
		lightLevelList[2].Level = LightManager.VertMap_DomLightIntensities[_vGridPos.x, _vGridPos.y].z;
		lightLevelList[3].Level = LightManager.VertMap_DomLightIntensities[_vGridPos.x, _vGridPos.y].w;

		lightLevelList.OrderBy(x => -x.Level); // reverse sort
		if (!_excludeMe){
			for (int i = 0; i < 4; i++){
				float _lightFromOther = lightLevelList[i].Level;
				if (_illuminated && _lightFromThis >= _lightFromOther){
					lightLevelList.Insert(i, new LightIndexLevelPairClass(LightManager.AllLights.FindIndex(x => x == this), _lightFromThis));
					break;
				}
				else if (!_illuminated && _lightFromOther == 0){ // we still wanna save the index to prevent some shadow-issues around corners
					lightLevelList.Insert(i, new LightIndexLevelPairClass(LightManager.AllLights.FindIndex(x => x == this), 0));
					break;
				}
			}
		}

		LightManager.VertMap_DomLightIndices[_vGridPos.x, _vGridPos.y].x = lightLevelList[0].Index;
		LightManager.VertMap_DomLightIndices[_vGridPos.x, _vGridPos.y].y = lightLevelList[1].Index;
		LightManager.VertMap_DomLightIndices[_vGridPos.x, _vGridPos.y].z = lightLevelList[2].Index;
		LightManager.VertMap_DomLightIndices[_vGridPos.x, _vGridPos.y].w = lightLevelList[3].Index;

		LightManager.VertMap_DomLightIntensities[_vGridPos.x, _vGridPos.y].x = lightLevelList[0].Level;
		LightManager.VertMap_DomLightIntensities[_vGridPos.x, _vGridPos.y].y = lightLevelList[1].Level;
		LightManager.VertMap_DomLightIntensities[_vGridPos.x, _vGridPos.y].z = lightLevelList[2].Level;
		LightManager.VertMap_DomLightIntensities[_vGridPos.x, _vGridPos.y].w = lightLevelList[3].Level;

		return LightManager.VertMap_DomLightIndices[_vGridPos.x, _vGridPos.y];
	}

    void RenderLightMesh() {
        //-- Step 5: fill the mesh with vertices--//

        Vector3[] initVerticesMeshLight = new Vector3[allVertices.Count + 1];
        initVerticesMeshLight[0] = Vector3.zero;

        for (int i = 0; i < allVertices.Count; i++)
            initVerticesMeshLight[i + 1] = allVertices[i].Pos;

        MyMesh.Clear();
        MyMesh.vertices = initVerticesMeshLight;

        Vector2[] uvs = new Vector2[initVerticesMeshLight.Length];
        for (int i = 0; i < initVerticesMeshLight.Length; i++)
            uvs[i] = new Vector2(initVerticesMeshLight[i].x, initVerticesMeshLight[i].y);

        MyMesh.uv = uvs;

        // triangles
        int index = 0;
        int[] triangles = new int[(allVertices.Count * 3)];
        for (int i = 0; i < (allVertices.Count * 3); i += 3) {
            triangles[i] = 0;
            triangles[i + 1] = index + 1;

            if (i == (allVertices.Count * 3) - 3) //-- if is the last vertex (one loop)
                triangles[i + 2] = 1;
            else // next next vertex	
                triangles[i + 2] = index + 2;

            index++;
        }

        MyMesh.triangles = triangles;
        renderer.sharedMaterial = lightMaterial;
        renderer.material.SetFloat("_UVScale", 1 / Radius);
	}

    void ResetBounds() {
        Bounds _bounds = MyMesh.bounds;
        _bounds.center = Vector3.zero;
        MyMesh.bounds = _bounds;
    }

	private const float VERTEX_ON_EDGE_TOLERANCE = 0.01f;
	public Vector2[] PointCollisionArray;
	void UpdatePointCollisionArray(){
		// cache vertices relative to world - but skip zero as it messes with the IsInsideLightMesh-algorithm
		PointCollisionArray = new Vector2[MyMesh.vertexCount - 1];
		Vector3[] _vertices = MyMesh.vertices;
		for (int i = 0; i < PointCollisionArray.Length; i++){
			Vector3 _vertex = _vertices[i + 1]; 
			Vector3 _dir = (_vertex - transform.position).normalized;
			PointCollisionArray[i] = transform.position + _vertex + _dir * VERTEX_ON_EDGE_TOLERANCE;
		}
	}
	private bool IsInsideLightMesh(Vector2 _worldPos){
		bool _inside = false;
		for (int i = 0, i2 = PointCollisionArray.Length - 1; i < PointCollisionArray.Length; i2 = i, i++){
			Vector2 _vert1 = PointCollisionArray[i];
			Vector2 _vert2 = PointCollisionArray[i2];

			bool _isBetweenVertices = _vert1.y <= _worldPos.y && _worldPos.y < _vert2.y || _vert2.y <= _worldPos.y && _worldPos.y < _vert1.y;
			float _progressY = (_worldPos.y - _vert1.y) / (_vert2.y - _vert1.y);
			float _progressX = (_vert2.x - _vert1.x) * _progressY;
			bool _isLeftOfEdge = _worldPos.x < _vert1.x + _progressX;

			if (_isBetweenVertices && _isLeftOfEdge)
				_inside = !_inside;
		}

		return _inside;
	}

    float GetVectorAngle(float x, float y) {
		// Height performance for calculate angle on a vector (only for sort)
        // APROXIMATE VALUES -- NOT EXACT!! //
        float ax = Mathf.Abs(x);
        float ay = Mathf.Abs(y);
        float angle = y / (ax + ay);
        if (x < 0)
            angle = 2 - angle;

        return angle;
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
			Vector2i _tileCoord = tilesInRangeWithCollider[x, y];
			if(_tileCoord.x < 0 || _tileCoord.y < 0)
				continue;

			Tile _t = Grid.Instance.grid[_tileCoord.x, _tileCoord.y];
			PolygonCollider2D _coll = ObjectPooler.Instance.GetPooledObject<PolygonCollider2D>(_t.ExactType);
            if (_coll == null)
                continue;

            _coll.transform.position = _t.WorldPosition;
            pooledColliders.Enqueue(_coll);
        }
	}
	private const float GRIDCAST_TOLERANCE = 0.05f;
    bool Gridcast(Vector2 _start, Vector2 _end, out RaycastHit2D _rayhit){
		_rayhit = Physics2D.Linecast(_start, _end);
        return _rayhit.collider != null && (_end - _rayhit.point).magnitude > GRIDCAST_TOLERANCE;
    }
	void DiscardPooledColliders(){
        int _count = pooledColliders.Count;
		for (int i = 0; i < _count; i++)
            pooledColliders.Dequeue().GetComponent<PoolerObject>().ReturnToPool();
	}
}

