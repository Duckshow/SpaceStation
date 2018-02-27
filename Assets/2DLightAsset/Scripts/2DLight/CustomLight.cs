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

	void AssignValueToVertMap<T>(T[,] _vertexLightMap, int _vxGrid, int _vyGrid, T _value){
		int _vxLight = ConvertToVertexLightSpace(XYEnum.X, _vxGrid, this);
		int _vyLight = ConvertToVertexLightSpace(XYEnum.Y, _vyGrid, this);
		_vertexLightMap[_vxLight, _vyLight] = _value;

		int[] _xGridNeighbours;
		int[] _yGridNeighbours;
		int[] _vIndexNeighbours;
		GetGridVerticesAtSamePosition(_vxGrid, _vyGrid, _vxLight == 0, false, out _xGridNeighbours, out _yGridNeighbours, out _vIndexNeighbours);
		for (int i = 0; i < _xGridNeighbours.Length; i++){
			int _xLightNeighbour = GetXLight(_xGridNeighbours[i], this);
			int _yLightNeighbour = GetYLight(_yGridNeighbours[i], this);
			int _vyTileNeighbour = GetVYTile(_vIndexNeighbours[i]);
			int _vxTileNeighbour = GetVXTile(_vIndexNeighbours[i], _vyTileNeighbour);
			int _vxLightNeighbour = GetVXLight(_xLightNeighbour, _vxTileNeighbour);
			int _vyLightNeighbour = GetVYLight(_yLightNeighbour, _vyTileNeighbour);

			if(_vxLightNeighbour < 0 || _vxLightNeighbour >= _vertexLightMap.GetLength(0)) 
				continue;
			if (_vyLightNeighbour < 0 || _vyLightNeighbour >= _vertexLightMap.GetLength(1))
				continue;

			_vertexLightMap[_vxLightNeighbour, _vyLightNeighbour] = _value;
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
				
				if (_xGrid < 0 || _xGrid >= Grid.GridSizeX || _yGrid < 0 || _yGrid >= Grid.GridSizeY){
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
    private delegate void mApplyToVertex(int _x, int _y, int _vertex);
	private void CalculateLightingForTilesInRange(bool _excludeMe){
		// find colors and dots for all lights and apply
		Vector2[] _dots = new Vector2[4];

		int _vxTile = 0, _vyTile = 0, _vIndex = 0, _xLight = 0, _yLight = 0;
		mIterateVariables IterateExtraVariables = delegate (){
			_vxTile++;
			_vIndex++;
			if (_vxTile == UVControllerBasic.MESH_VERTICES_PER_EDGE){
				_vxTile = 0;
				_vyTile++;
				if (_vyTile == UVControllerBasic.MESH_VERTICES_PER_EDGE){
					_vyTile = 0;
					_vIndex = 0;
					_xLight++;
					if (_xLight == tilesInRange.GetLength(0)){
						_xLight = 0;
						_yLight++;
					}
				}
			}
		};
		int _totalIterations = (int)(tilesInRange.GetLength(0) * UVControllerBasic.MESH_VERTICES_PER_EDGE * tilesInRange.GetLength(1) * UVControllerBasic.MESH_VERTICES_PER_EDGE);
		for (int i = 0; i < _totalIterations; i++, IterateExtraVariables()){
			if (_xLight > 0 && _vxTile == 0)
				continue;
			if (_yLight > 0 && _vyTile == 0)
				continue;
			if(tilesInRange[_xLight, _yLight].x < 0 || tilesInRange[_xLight, _yLight].y < 0)
				continue;

			int _xGrid 		= GetXGrid	(_xLight, this);
			int _yGrid 		= GetYGrid	(_yLight, this);
			int _vxLight 	= GetVXLight(_xLight, _vxTile);
			int _vyLight 	= GetVYLight(_yLight, _vyTile);
			int _vxGrid 	= GetVXGrid	(_xGrid, _vxTile);
			int _vyGrid 	= GetVYGrid	(_yGrid, _vyTile);
			float _vxWorld 	= GetVXWorld(_xGrid, _vxTile);
			float _vyWorld 	= GetVYWorld(_yGrid, _vyTile);

			// get colors from lights
			bool _illuminated;
			float _lightFromThis;
			Color _newVertexColor = GetColorForVertex(_vxGrid, _vyGrid, _xGrid, _yGrid, _xLight, _yLight, _vxLight, _vyLight, _vIndex, _vxWorld, _vyWorld, _excludeMe, out _illuminated, out _lightFromThis);
			
			// get two dots per light describing angle to four strongest lights
			Vector4 _dominantLightIndices = GetShadowCastingLightsIndices(_vxGrid, _vyGrid, _excludeMe, _illuminated, _lightFromThis);
			_dots[0] = GetDotXY(_vxWorld, _vyWorld, (int)_dominantLightIndices.x);
			_dots[1] = GetDotXY(_vxWorld, _vyWorld, (int)_dominantLightIndices.y);
			_dots[2] = GetDotXY(_vxWorld, _vyWorld, (int)_dominantLightIndices.z);
			_dots[3] = GetDotXY(_vxWorld, _vyWorld, (int)_dominantLightIndices.w);

			int[] _xGridNeighbours;
			int[] _yGridNeighbours;
			int[] _vIndexNeighbours;
			GetGridVerticesAtSamePosition(_vxGrid, _vyGrid, _xLight == 0, true, out _xGridNeighbours, out _yGridNeighbours, out _vIndexNeighbours);
			for (int i2 = 0; i2 < _xGridNeighbours.Length; i2++){
				Grid.Instance.grid[_xGridNeighbours[i2], _yGridNeighbours[i2]].MyUVController.SetUVDots(_vIndexNeighbours[i2], _dots[0], _dots[1], _dots[2], _dots[3]);
			}

			GetGridVerticesAtSamePosition(_vxGrid, _vyGrid, _xLight == 0, false, out _xGridNeighbours, out _yGridNeighbours, out _vIndexNeighbours);
			for (int i2 = 0; i2 < _xGridNeighbours.Length; i2++){
				int _vy = GetVYTile(_vIndexNeighbours[i2]);
				int _vx = GetVXTile(_vIndexNeighbours[i2], _vy);
				int _vyG = GetVYGrid(_yGridNeighbours[i2], _vy);
				int _vxG = GetVXGrid(_xGridNeighbours[i2], _vx);
				LightManager.VertMap_TotalColorNoBlur[_vxG, _vyG] = _newVertexColor;
			}


			// // DEBUG 
			// for (int i2 = 0; i2 < _xGridNeighbours.Length; i2++){
			// 	// if(_xLight == 0 && vx == 0 || _yLight == 0 && vy == 0)
			// 	// 	Grid.Instance.grid[_xGridNeighbours[i2], _yGridNeighbours[i2]].MyUVController.SetVertexColor(_vIndexNeighbours[i2], Color.red);
			// 	// else
			// 		Grid.Instance.grid[_xGridNeighbours[i2], _yGridNeighbours[i2]].MyUVController.SetVertexColor(_vIndexNeighbours[i2], _newVertexColor);
			// }
		}

	// DEBUG
	//return;

		// blur and apply colors
		Color[] _adjacentColors = new Color[9];
		
		_vxTile = 0; 
		_vyTile = 0; 
		_vIndex = 0; 
		_xLight = 0; 
		_yLight = 0;
		for (int i = 0; i < _totalIterations; i++, IterateExtraVariables()){
			if (_xLight > 0 && _vxTile == 0)
				continue;
			if (_yLight > 0 && _vyTile == 0)
				continue;
			if(tilesInRange[_xLight, _yLight].x < 0 || tilesInRange[_xLight, _yLight].y < 0)
				continue;

			int _xGrid 		= GetXGrid(_xLight, this);
			int _yGrid 		= GetYGrid(_yLight, this);
			int _vxLight 	= GetVXLight(_xLight, _vxTile);
			int _vyLight 	= GetVYLight(_yLight, _vyTile);

			// try get colors from neighbouring vertices (for smoothing)
			int _diffToRightX = _vxTile == 0 ? 1 : 2;
			int _diffToAboveY = _vyTile == 0 ? 1 : 2;
			int _failAmount = 0;
			mTryGetNeighbourColor TryGetAdjacentColor = delegate (int _x, int _y){
				if (_x < 0 || _y < 0 || _x >= LightManager.VertMap_TotalColorNoBlur.GetLength(0) || _y >= LightManager.VertMap_TotalColorNoBlur.GetLength(1)){
					_failAmount++;
					return Color.clear;
				}
				return LightManager.VertMap_TotalColorNoBlur[_x, _y];
			};

			int _vxGrid = GetVXGrid(_xGrid, _vxTile);
			int _vyGrid = GetVYGrid(_yGrid, _vyTile);
			_adjacentColors[0] = TryGetAdjacentColor(_vxGrid - 1, 				_vyGrid - 1);
			_adjacentColors[1] = TryGetAdjacentColor(_vxGrid, 					_vyGrid - 1);
			_adjacentColors[2] = TryGetAdjacentColor(_vxGrid + _diffToRightX, 	_vyGrid - 1);
			_adjacentColors[3] = TryGetAdjacentColor(_vxGrid - 1, 				_vyGrid);
			_adjacentColors[4] = TryGetAdjacentColor(_vxGrid, _vyGrid); // this tile
			_adjacentColors[5] = TryGetAdjacentColor(_vxGrid + _diffToRightX, 	_vyGrid);
			_adjacentColors[6] = TryGetAdjacentColor(_vxGrid - 1, 				_vyGrid + _diffToAboveY);
			_adjacentColors[7] = TryGetAdjacentColor(_vxGrid, 					_vyGrid + _diffToAboveY);
			_adjacentColors[8] = TryGetAdjacentColor(_vxGrid + _diffToRightX, 	_vyGrid + _diffToAboveY);

			Color _myColor = new Color();
			for (int i2 = 0; i2 < _adjacentColors.Length; i2++)
				_myColor += _adjacentColors[i2];

			_myColor /= Mathf.Max(_adjacentColors.Length - _failAmount, 1);
			_myColor.a = 1;

			int[] _xGridForNeighbours;
			int[] _yGridForNeighbours;
			int[] _vIndexForNeighbours;
			mApplyToVertex ApplyVertexColor = delegate (int _x, int _y, int _vertex){
				Grid.Instance.grid[_x, _y].MyUVController.SetVertexColor(_vertex, _myColor);
			};
			GetGridVerticesAtSamePosition(_vxGrid, _vyGrid, _xLight == 0, false, out _xGridForNeighbours, out _yGridForNeighbours, out _vIndexForNeighbours);
			for (int i2 = 0; i2 < _xGridForNeighbours.Length; i2++){
				ApplyVertexColor(_xGridForNeighbours[i2], _yGridForNeighbours[i2], _vIndexForNeighbours[i2]);
			}
		}
	}

	static int GetXGrid(int _xLight, CustomLight _light){
		return _light.MyGridCoord.x - (_light.Radius - _xLight);
	}
	static int GetYGrid(int _yLight, CustomLight _light){
		return _light.MyGridCoord.y - (_light.Radius - _yLight);
	}
	static int GetVXGrid(int _xGrid, int _vxTile){
		return _xGrid * UVControllerBasic.MESH_VERTICES_PER_EDGE + _vxTile;
	}
	static int GetVYGrid(int _yGrid, int _vyTile){
		return _yGrid * UVControllerBasic.MESH_VERTICES_PER_EDGE + _vyTile;
	}
	static int GetXLight(int _xGrid, CustomLight _light){
		return _xGrid - (_light.MyGridCoord.x - _light.Radius);
	}
	static int GetYLight(int _yGrid, CustomLight _light){
		return _yGrid - (_light.MyGridCoord.y - _light.Radius);
	}
	static int GetVXLight(int _xLight, int _vxTile){
		return _xLight * UVControllerBasic.MESH_VERTICES_PER_EDGE + _vxTile;
	}
	static int GetVYLight(int _yLight, int _vyTile){
		return _yLight * UVControllerBasic.MESH_VERTICES_PER_EDGE + _vyTile;
	}
	static int GetVXTile(int _vIndex, int _vyTile){
		return _vIndex - _vyTile * UVControllerBasic.MESH_VERTICES_PER_EDGE;
	}
	static int GetVYTile(int _vIndex){
		return Mathf.FloorToInt(_vIndex / UVControllerBasic.MESH_VERTICES_PER_EDGE);
	}
	private const float VERTEX_DISTANCE = 0.5f;
    float GetVXWorld(int _xGrid, int _vx){
        return Grid.Instance.grid[_xGrid, 0].WorldPosition.x + (_vx - 1) * VERTEX_DISTANCE;
    }
	float GetVYWorld(int _yGrid, int _vy){
        return Grid.Instance.grid[0, _yGrid].WorldPosition.y + (_vy - 1) * VERTEX_DISTANCE;
    }

	// static int GetVXGrid(int _vxLight, CustomLight _light){
	// 	return _vxLight + UVControllerBasic.MESH_VERTICES_PER_EDGE * (_light.MyGridCoord.x - _light.Radius);
	// }
	// static int GetVYGrid(int _vyLight, CustomLight _light){
	// 	return _vyLight + UVControllerBasic.MESH_VERTICES_PER_EDGE * (_light.MyGridCoord.y - _light.Radius);
	// }
	// static int GetXGrid(int _vxGrid){
	// 	return _vxGrid / UVController.MESH_VERTICES_PER_EDGE;
	// }
	// static int GetYGrid(int _vyGrid){
	// 	return _vyGrid / UVController.MESH_VERTICES_PER_EDGE;
	// }
	// static int GetXLight(int _vxGrid, CustomLight _light){
	// 	return GetXGrid(_vxGrid) - (_light.MyGridCoord.x - _light.Radius);
	// }
	// static int GetYLight(int _vyGrid, CustomLight _light){
	// 	return GetYGrid(_vyGrid) - (_light.MyGridCoord.y - _light.Radius);
	// }
	// static int GetVXLight(int _vxGrid, CustomLight _light){
	// 	return _vxGrid - UVControllerBasic.MESH_VERTICES_PER_EDGE * (_light.MyGridCoord.x - _light.Radius);
	// }
	// static int GetVYLight(int _vyGrid, CustomLight _light){
	// 	return _vyGrid - UVControllerBasic.MESH_VERTICES_PER_EDGE * (_light.MyGridCoord.y - _light.Radius);
	// }
	// static int GetVXTile(int _vxGrid){
	// 	return _vxGrid - GetXGrid(_vxGrid);
	// }
	// static int GetVYTile(int _vyGrid){
	// 	return _vyGrid - GetYGrid(_vyGrid);
	// }
	// private const float VERTEX_DISTANCE = 0.5f;
    // float GetVXWorld(int _vxGrid){
	// 	float _distance = _vxGrid * VERTEX_DISTANCE;
	// 	float _correction = GetXGrid(_vxGrid) * VERTEX_DISTANCE; // discount every third vertex (except first) since they overlap
	// 	return (Grid.Instance.transform.position.x - Grid.GridSizeXHalf) + (_distance - _correction);
    // }
	// float GetVYWorld(int _vyGrid){
	// 	float _distance = _vyGrid * VERTEX_DISTANCE;
	// 	float _correction = GetYGrid(_vyGrid) * VERTEX_DISTANCE; // discount every third vertex (except first) since they overlap
	// 	return (Grid.Instance.transform.position.y - Grid.GridSizeYHalf) + (_distance - _correction);    
	// }



	static int ConvertToVertexGridSpace(XYEnum _axis, int _vLight, CustomLight _light){
		return _vLight + UVControllerBasic.MESH_VERTICES_PER_EDGE * (_light.MyGridCoord.GetXOrY(_axis) - _light.Radius);
	}
	static int ConvertToGridSpace(int _vGrid){
		return _vGrid / UVController.MESH_VERTICES_PER_EDGE;
	}
	static int ConvertToLightSpace(XYEnum _axis, int _vGrid, CustomLight _light){
		return ConvertToGridSpace(_vGrid) - (_light.MyGridCoord.GetXOrY(_axis) - _light.Radius);
	}
	static int ConvertToVertexLightSpace(XYEnum _axis, int _vGrid, CustomLight _light){
		return _vGrid - UVControllerBasic.MESH_VERTICES_PER_EDGE * (_light.MyGridCoord.GetXOrY(_axis) - _light.Radius);
	}
	static int ConvertToVertexTileSpace(XYEnum _axis, int _vGrid){
		return _vGrid - ConvertToGridSpace(_vGrid);
	}
    static float ConvertToVertexWorldSpace(XYEnum _axis, int _vGrid){
		int _gridSizeHalf = _axis == XYEnum.X ? Grid.GridSizeXHalf : Grid.GridSizeYHalf;
		float _distance = _vGrid * VERTEX_DISTANCE;
		float _correction = ConvertToGridSpace(_vGrid) * VERTEX_DISTANCE; // discount every third vertex (except first) since they overlap
		return (Grid.Instance.transform.position.GetXOrY(_axis) - _gridSizeHalf) + (_distance - _correction);
    }



	enum VertexNeighbourEnum { TileAbove, TopHalf, TopHalfLeft, TopHalfRight, TileRight, TileRightTopHalf, TileRightTopHalfLeft, TileTopRight };
	struct VertexNeighbourStruct {
		public bool Affected;
		public VertexNeighbourEnum Neighbour;
		public VertexNeighbourStruct(bool _affected, VertexNeighbourEnum _neighbour) { Affected = _affected; Neighbour = _neighbour; }
	}
	public static void GetGridVerticesAtSamePosition(int _vxGrid, int _vyGrid, bool _isOnLeftEdge, bool _includeTopHalfStuff, out int[] _xGridForNeighbours, out int[] _yGridForNeighbours, out int[] _vIndexForNeighbours) {

		Vector2i _gGridSpace = new Vector2i();
		_gGridSpace.x= Mathf.FloorToInt(_vxGrid / UVControllerBasic.MESH_VERTICES_PER_EDGE);
		_gGridSpace.y = Mathf.FloorToInt(_vyGrid / UVControllerBasic.MESH_VERTICES_PER_EDGE);
		Vector2i _vTileSpace = new Vector2i();
		_vTileSpace.x = _vxGrid - _gGridSpace.x * UVControllerBasic.MESH_VERTICES_PER_EDGE;
		_vTileSpace.y = _vyGrid - _gGridSpace.y * UVControllerBasic.MESH_VERTICES_PER_EDGE;
		
		int _vIndex = _vTileSpace.y * UVControllerBasic.MESH_VERTICES_PER_EDGE + _vTileSpace.x;

		bool _tileAbove 			= AffectsVertex(VertexNeighbourEnum.TileAbove, 			_vTileSpace, _gGridSpace, _isOnLeftEdge, _includeTopHalfStuff);
		bool _topHalf 				= AffectsVertex(VertexNeighbourEnum.TopHalf, 			_vTileSpace, _gGridSpace, _isOnLeftEdge, _includeTopHalfStuff);
		bool _topHalf_L 			= AffectsVertex(VertexNeighbourEnum.TopHalfLeft, 		_vTileSpace, _gGridSpace, _isOnLeftEdge, _includeTopHalfStuff);
		bool _topHalf_R 			= AffectsVertex(VertexNeighbourEnum.TopHalfRight, 		_vTileSpace, _gGridSpace, _isOnLeftEdge, _includeTopHalfStuff);
		bool _tileRight 			= AffectsVertex(VertexNeighbourEnum.TileRight, 			_vTileSpace, _gGridSpace, _isOnLeftEdge, _includeTopHalfStuff);
		bool _tileRightTopHalf 		= AffectsVertex(VertexNeighbourEnum.TileRightTopHalf, 	_vTileSpace, _gGridSpace, _isOnLeftEdge, _includeTopHalfStuff);
		bool _tileRightTopHalf_L 	= AffectsVertex(VertexNeighbourEnum.TileRightTopHalf, 	_vTileSpace, _gGridSpace, _isOnLeftEdge, _includeTopHalfStuff);
		bool _tileTopRight 			= AffectsVertex(VertexNeighbourEnum.TileTopRight, 		_vTileSpace, _gGridSpace, _isOnLeftEdge, _includeTopHalfStuff);

		VertexNeighbourStruct[] _theNeighbours = new VertexNeighbourStruct[] { 
			new VertexNeighbourStruct(_tileAbove, 			VertexNeighbourEnum.TileAbove),
			new VertexNeighbourStruct(_topHalf, 			VertexNeighbourEnum.TopHalf),
			new VertexNeighbourStruct(_topHalf_L, 			VertexNeighbourEnum.TopHalfLeft),
			new VertexNeighbourStruct(_topHalf_R, 			VertexNeighbourEnum.TopHalfRight),
			new VertexNeighbourStruct(_tileRight, 			VertexNeighbourEnum.TileRight),
			new VertexNeighbourStruct(_tileRightTopHalf, 	VertexNeighbourEnum.TileRightTopHalf),
			new VertexNeighbourStruct(_tileRightTopHalf_L, 	VertexNeighbourEnum.TileRightTopHalfLeft),
			new VertexNeighbourStruct(_tileTopRight, 		VertexNeighbourEnum.TileTopRight)
		};

		int _amountTrue = 1; // 1 because *this* vertex is always affected
		for (int i = 0; i < _theNeighbours.Length; i++){
			if(_theNeighbours[i].Affected)
				_amountTrue++;
		}
		_xGridForNeighbours 	= new int[_amountTrue];
		_yGridForNeighbours 	= new int[_amountTrue];
		_vIndexForNeighbours 	= new int[_amountTrue];
		_xGridForNeighbours[0] 	= _gGridSpace.x;
		_yGridForNeighbours[0] 	= _gGridSpace.y;
		_vIndexForNeighbours[0] = _vIndex;

		for (int _index = 0, _affectIndex = 1; _index < _theNeighbours.Length; _index++){
			if(!_theNeighbours[_index].Affected)
				continue;

			_xGridForNeighbours[_affectIndex] 	= GetGridCoordForNeighbour(XYEnum.X, _theNeighbours[_index].Neighbour, _gGridSpace.x);
			_yGridForNeighbours[_affectIndex] 	= GetGridCoordForNeighbour(XYEnum.Y, _theNeighbours[_index].Neighbour, _gGridSpace.y);
			_vIndexForNeighbours[_affectIndex] 	= GetVIndexForNeighbour(_theNeighbours[_index].Neighbour, _vTileSpace, _vIndex);
			_affectIndex++;
		}
	}

	static bool AffectsVertex(VertexNeighbourEnum _neighbour, Vector2i _vTileSpace, Vector2i _gGridSpace, bool _isOnLeftEdge, bool _includeTopHalf){
		int _vertEdgeMaxIndex = UVControllerBasic.MESH_VERTICES_PER_EDGE - 1;
		switch (_neighbour){
			case VertexNeighbourEnum.TileAbove:
				return _gGridSpace.y + 1 < Grid.GridSizeY && _vTileSpace.y == _vertEdgeMaxIndex;
			case VertexNeighbourEnum.TopHalf:
				return _includeTopHalf && _vTileSpace.y == _vertEdgeMaxIndex;
			case VertexNeighbourEnum.TopHalfLeft:
				return AffectsVertex(VertexNeighbourEnum.TopHalf, _vTileSpace, _gGridSpace, _isOnLeftEdge, _includeTopHalf) && _isOnLeftEdge;
			case VertexNeighbourEnum.TopHalfRight:
				return AffectsVertex(VertexNeighbourEnum.TopHalf, _vTileSpace, _gGridSpace, _isOnLeftEdge, _includeTopHalf) && _vTileSpace.x == _vertEdgeMaxIndex;
			case VertexNeighbourEnum.TileRight:
				return _gGridSpace.x + 1 < Grid.GridSizeX && _vTileSpace.x == _vertEdgeMaxIndex;
			case VertexNeighbourEnum.TileRightTopHalf:
			case VertexNeighbourEnum.TileRightTopHalfLeft:
				return AffectsVertex(VertexNeighbourEnum.TopHalf, _vTileSpace, _gGridSpace, _isOnLeftEdge, _includeTopHalf) && AffectsVertex(VertexNeighbourEnum.TileRight, _vTileSpace, _gGridSpace, _isOnLeftEdge, _includeTopHalf);
			case VertexNeighbourEnum.TileTopRight:
				return AffectsVertex(VertexNeighbourEnum.TileRight, _vTileSpace, _gGridSpace, _isOnLeftEdge, _includeTopHalf) && AffectsVertex(VertexNeighbourEnum.TileAbove, _vTileSpace, _gGridSpace, _isOnLeftEdge, _includeTopHalf);
			default:
				Debug.LogError(_neighbour.ToString() + " hans't been properly implemented yet!");
				return false;
		}
	}
	static int GetGridCoordForNeighbour(XYEnum _axis, VertexNeighbourEnum _neighbour, int _gGrid){
		if (_axis == XYEnum.X){
			switch (_neighbour){
				case VertexNeighbourEnum.TileAbove:
				case VertexNeighbourEnum.TopHalf:
				case VertexNeighbourEnum.TopHalfLeft:
				case VertexNeighbourEnum.TopHalfRight:
					return _gGrid;
				case VertexNeighbourEnum.TileRight:
				case VertexNeighbourEnum.TileRightTopHalf:
				case VertexNeighbourEnum.TileRightTopHalfLeft:
				case VertexNeighbourEnum.TileTopRight:
					return _gGrid + 1;
				default:
					Debug.LogError(_neighbour.ToString() + " hans't been properly implemented yet!");
					return 0;
			}
		}
		else{
			switch (_neighbour){
				case VertexNeighbourEnum.TopHalf:
				case VertexNeighbourEnum.TopHalfLeft:
				case VertexNeighbourEnum.TopHalfRight:
				case VertexNeighbourEnum.TileRight:
				case VertexNeighbourEnum.TileRightTopHalf:
				case VertexNeighbourEnum.TileRightTopHalfLeft:
					return _gGrid;
				case VertexNeighbourEnum.TileAbove:
				case VertexNeighbourEnum.TileTopRight:
					return _gGrid + 1;
				default:
					Debug.LogError(_neighbour.ToString() + " hans't been properly implemented yet!");
					return 0;
			}
		}
	}
	static int GetVIndexForNeighbour(VertexNeighbourEnum _neighbour, Vector2i _vTileSpace, int _vIndex){
		int _vertsPerEdge = UVControllerBasic.MESH_VERTICES_PER_EDGE;
		switch (_neighbour){
			case VertexNeighbourEnum.TileAbove: 			return _vTileSpace.x;
			case VertexNeighbourEnum.TopHalf: 				return (_vTileSpace.y + 1) * _vertsPerEdge + _vTileSpace.x;
			case VertexNeighbourEnum.TopHalfLeft:			return (_vTileSpace.y + 2) * _vertsPerEdge;
			case VertexNeighbourEnum.TopHalfRight:			return (_vTileSpace.y + 2) * _vertsPerEdge + 1; // +1 instead of _vx because top-half has fewer vertices ^^'
			case VertexNeighbourEnum.TileRight:				return _vIndex - _vTileSpace.x;
			case VertexNeighbourEnum.TileRightTopHalf:		return (_vTileSpace.y + 1) * _vertsPerEdge;
			case VertexNeighbourEnum.TileRightTopHalfLeft:	return (_vTileSpace.y + 2) * _vertsPerEdge;
			case VertexNeighbourEnum.TileTopRight:			return 0;
			default:
				Debug.LogError(_neighbour.ToString() + " hans't been properly implemented yet!");
				return 0;
		}
	}

	static float GetAngle01(Vector2 _pos1, Vector2 _pos2, Vector2 _referenceAngle, int maxAngle) { // TODO: replace with GetAngleClockwise!
        return maxAngle * (0.5f * (1 + Vector2.Dot(
                                            _referenceAngle, 
                                            Vector3.Normalize(_pos1 - _pos2))));
    }
	// find the horizontal and vertical dot-products between two vectors
	static Vector2 GetDotXY(float _vxWorld, float _vyWorld, int _lightIndex){
        if(_lightIndex < 0)
            return Vector2.zero; 

		Vector2 _worldPos = new Vector2(_vxWorld, _vyWorld);
        Vector2 _lightPos = LightManager.AllLights[_lightIndex].transform.position;

        // get an angle between 0->1. The angle goes all the way around, but counter-clockwise, so sorta like a clock and unlike a dot
		float _vertical 	= (Vector2.Dot(Vector2.down, (_worldPos - _lightPos).normalized) + 1) * 0.5f;
		float _horizontal 	=  Vector2.Dot(Vector2.left, (_worldPos - _lightPos).normalized);

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
	private Color GetColorForVertex(int _vxGrid, int _vyGrid, int _xGrid, int _yGrid, int _xLight, int _yLight, int _vxLight, int _vyLight, int _vIndex, float _vxWorld, float _vyWorld, bool _excludeMe, out bool _illuminated, out float _lightFromThis){
		Color _cachedColor = Color.clear;

		int _vyTile = GetVYTile(_vIndex);
		int _vxTile = GetVXTile(_vIndex, _vyTile);
		// take all previously hit light and recompile into one color
		int[] _lightsInRange = LightManager.GridMap_LightsInRange[_xGrid, _yGrid];
		int actualCount = 0;
		for (int i = 0; i < _lightsInRange.Length; i++){
			if(_lightsInRange[i] == -1)
				break;

			CustomLight _otherLight = LightManager.AllLights[_lightsInRange[i]];
			int _xLightOther = GetXLight(_xGrid, _otherLight); 
			int _yLightOther = GetYLight(_yGrid, _otherLight);
			int _vxLightOther = GetVXLight(_xLightOther, _vxTile);
			int _vyLightOther = GetVYLight(_yLightOther, _vyTile);

			if(_otherLight == this)
				continue;
		
			if(!_otherLight.VXLightMap_Hit[_vxLightOther, _vyLightOther])
				continue;

			actualCount++;
			CombineLightWithOthersLight(_otherLight.VXLightMap_Intensity[_vxLightOther, _vyLightOther], _otherLight.GetLightColor(), ref _cachedColor);
		}

		float _distance = (new Vector2(_vxWorld, _vyWorld) - myInspector.MyTileObject.MyTile.WorldPosition).magnitude;
		_lightFromThis = Intensity * Mathf.Pow(1 - (_distance / Radius), 2);
		_illuminated = !_excludeMe && IsInsideLightMesh(_vxWorld, _vyWorld);

		// apply new light to total color
		AssignValueToVertMap<bool>(VXLightMap_Hit, _vxGrid, _vyGrid, _illuminated);
		AssignValueToVertMap<float>(VXLightMap_Intensity, _vxGrid, _vyGrid, _lightFromThis);
		if(_illuminated){
			CombineLightWithOthersLight(_lightFromThis, GetLightColor(), ref _cachedColor);
		}

		_cachedColor.a = 1;
		LightManager.VertMap_TotalColorNoBlur[_vxGrid, _vyGrid] = _cachedColor;
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
	private Vector4 GetShadowCastingLightsIndices(int _vxGrid, int _vyGrid, bool _excludeMe, bool _illuminated, float _lightFromThis){
		lightLevelList[0].Index = LightManager.VertMap_DomLightIndices[_vxGrid, _vyGrid].x;
		lightLevelList[1].Index = LightManager.VertMap_DomLightIndices[_vxGrid, _vyGrid].y;
		lightLevelList[2].Index = LightManager.VertMap_DomLightIndices[_vxGrid, _vyGrid].z;
		lightLevelList[3].Index = LightManager.VertMap_DomLightIndices[_vxGrid, _vyGrid].w;

		lightLevelList[0].Level = LightManager.VertMap_DomLightIntensities[_vxGrid, _vyGrid].x;
		lightLevelList[1].Level = LightManager.VertMap_DomLightIntensities[_vxGrid, _vyGrid].y;
		lightLevelList[2].Level = LightManager.VertMap_DomLightIntensities[_vxGrid, _vyGrid].z;
		lightLevelList[3].Level = LightManager.VertMap_DomLightIntensities[_vxGrid, _vyGrid].w;

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

		LightManager.VertMap_DomLightIndices[_vxGrid, _vyGrid].x = lightLevelList[0].Index;
		LightManager.VertMap_DomLightIndices[_vxGrid, _vyGrid].y = lightLevelList[1].Index;
		LightManager.VertMap_DomLightIndices[_vxGrid, _vyGrid].z = lightLevelList[2].Index;
		LightManager.VertMap_DomLightIndices[_vxGrid, _vyGrid].w = lightLevelList[3].Index;

		LightManager.VertMap_DomLightIntensities[_vxGrid, _vyGrid].x = lightLevelList[0].Level;
		LightManager.VertMap_DomLightIntensities[_vxGrid, _vyGrid].y = lightLevelList[1].Level;
		LightManager.VertMap_DomLightIntensities[_vxGrid, _vyGrid].z = lightLevelList[2].Level;
		LightManager.VertMap_DomLightIntensities[_vxGrid, _vyGrid].w = lightLevelList[3].Level;

		return LightManager.VertMap_DomLightIndices[_vxGrid, _vyGrid];
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
		for (int i = 0; i < PointCollisionArray.Length; i++){
			Vector3 _dir = (MyMesh.vertices[i + 1] - transform.position).normalized;
			PointCollisionArray[i] = transform.position + MyMesh.vertices[i + 1] + _dir * VERTEX_ON_EDGE_TOLERANCE;
		}
	}
	private bool IsInsideLightMesh(float _xWorld, float _yWorld){
		bool _inside = false;
		for (int i = 0, i2 = PointCollisionArray.Length - 1; i < PointCollisionArray.Length; i2 = i, i++){
			Vector2 _vert1 = PointCollisionArray[i];
			Vector2 _vert2 = PointCollisionArray[i2];

			bool _isBetweenVertices = _vert1.y <= _yWorld && _yWorld < _vert2.y || _vert2.y <= _yWorld && _yWorld < _vert1.y;
			float _progressY = (_yWorld - _vert1.y) / (_vert2.y - _vert1.y);
			float _progressX = (_vert2.x - _vert1.x) * _progressY;
			bool _isLeftOfEdge = _xWorld < _vert1.x + _progressX;

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

