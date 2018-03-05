using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor; // for debug-gizmos only
using System;
using Utilities;

public class Verts {
    public float Angle;
    public int Location; // 1 = left end point | 0 = middle | -1 = right endpoint
    public Vector2 LocalPos;
    public bool Endpoint;
}

public class CustomLight : MonoBehaviour {

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

	[NonSerialized] public int LightIndex = -1;

	public bool isTurnedOn { get; private set; }
	[NonSerialized] public bool IsBeingRemoved;
	[NonSerialized] public Vector2i MyGridCoord;
	void UpdateGridCoord(){
		MyGridCoord = Grid.Instance.GetTileCoordFromWorldPoint(transform.position);
	}

	private delegate void mIterateVariables();

	private const string MESH_NAME = "Light Mesh";

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


	void Awake(){
		isTurnedOn = true;
		IsBeingRemoved = false;

		LightIndex = LightManager.AllLights.Count;
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
		LightIndex = -1;
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
    private bool _bottomLeftQuadrant = false; // check si hay menores a -0.5
    private bool _bottomRightQuadrant = false; // check si hay mayores a 2.0
    private const float MAGNITUDE_RANGE = 0.15f;
    private List<Verts> tempVerts = new List<Verts>();
    private RaycastHit2D rayHit;
    private int posLowAngle;
    private int posHighAngle;
    private float lowestAngle;
    private float highestAngle;
    private Vector3 fromCast;
    private bool isEndpoint;
    private Vector2 rayOrigin;
    private float mag;
    private const float CHECK_POINT_LAST_RAY_OFFSET = 0.005f;
    private Vector2 rayCont;
    private Verts vL;
    private int theta;
    private int amount;
    private float rangeAngleComparision;
    private Verts vertex1;
    private Verts vertex2;
	private delegate void mSortList(List<Verts> _list);
	void SetVertices() {
		Vector2 _lightPosWorld = transform.position;

		mSortList SortList = delegate(List<Verts> _list) {
			_list.Sort((item1, item2) => (item2.Angle.CompareTo(item1.Angle)));
		};

		allVertices.Clear();

		int x = 0, y = 0;
		Vector2i _gGridPos = tilesInRangeWithCollider[0, 0];
		mIterateVariables IterateExtraVariables = delegate (){
			x++;
			if (x >= tilesInRangeWithCollider.GetLength(0)){
				x = 0;
				y++;
				if (y >= tilesInRangeWithCollider.GetLength(1)){
					return;
				}
			}

			_gGridPos = tilesInRangeWithCollider[x, y];
		};
		int _totalIterations = tilesInRangeWithCollider.GetLength(0) * tilesInRangeWithCollider.GetLength(1);
		for (int i = 0; i < _totalIterations; i++, IterateExtraVariables()) {
			if(_gGridPos.x < 0 || _gGridPos.y < 0) continue;

			// get collider
			Tile _colliderTile = Grid.Instance.grid[_gGridPos.x, _gGridPos.y];
            PolygonCollider2D _collider = ObjectPooler.Instance.GetPooledObject<PolygonCollider2D>(_colliderTile.ExactType);
            _collider.transform.position = _colliderTile.WorldPosition;

			tempVerts.Clear();
            
            _bottomLeftQuadrant = false;
            _bottomRightQuadrant = false;

            for (int pIndex = 0; pIndex < _collider.pathCount; pIndex++){ 					// iterate over collider-paths
                for (int vIndex = 0; vIndex < _collider.GetPath(pIndex).Length; vIndex++){ 	// iterate over path-vertices
                    Vector2 _targetPosWorld = (Vector2)_collider.transform.position + _collider.GetPath(pIndex)[vIndex];
                    Verts _newVertex = new Verts();

                    if (Gridcast(_lightPosWorld, _targetPosWorld, out rayHit)) {
                        _newVertex.LocalPos = rayHit.point;
						_newVertex.Endpoint = false;
                    }
                    else {
                        _newVertex.LocalPos = _targetPosWorld;
                        _newVertex.Endpoint = true;
                    }

                    _newVertex.LocalPos = transform.InverseTransformPoint(_newVertex.LocalPos);	// to local
                    _newVertex.Angle = GetQuadrantSortingAngle(_newVertex.LocalPos.x, _newVertex.LocalPos.y);

					// -- bookmark if an angle is lower than 0 or higher than 2f --//
					//-- helper method for fix bug on shape located in 2 or more quadrants
					if (_newVertex.Angle < 0f)
                        _bottomLeftQuadrant = true;

                    if (_newVertex.Angle > 2f)
                        _bottomRightQuadrant = true;

                    //--Add verts to the main list
                    if (_newVertex.LocalPos.magnitude <= Radius) adpmawp // continue here
                        tempVerts.Add(_newVertex);

                    if (sortAngles == false)
                        sortAngles = true;
                }
            }
            _collider.GetComponent<PoolerObject>().ReturnToPool();

            // Identify the endpoints (left and right)
            if (tempVerts.Count > 0) {
                SortList(tempVerts); // sort first

                posLowAngle = 0; // save the indice of left ray
                posHighAngle = 0; // same last in right side

                if (_bottomRightQuadrant == true && _bottomLeftQuadrant == true) {  //-- FIX BUG OF SORTING CUANDRANT 1-4 --//
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
                    // save the index of left ray
                    posLowAngle = 0;
                    posHighAngle = tempVerts.Count - 1;
                }

                tempVerts[posLowAngle].Location = 1; // right
                tempVerts[posHighAngle].Location = -1; // left

                allVertices.AddRange(tempVerts);

				// right ray, then left ray
				ShootRay(transform.TransformPoint(tempVerts[posLowAngle].LocalPos), 	isEndpoint: tempVerts[posLowAngle].Endpoint);
				ShootRay(transform.TransformPoint(tempVerts[posHighAngle].LocalPos), isEndpoint: tempVerts[posHighAngle].Endpoint);
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

            Verts v = new Verts();
            v.LocalPos = new Vector3((LightManager.SinCosTable.sSinArray[theta]), (LightManager.SinCosTable.sCosArray[theta]), 0); // in degrees (previous calculate)
            v.Angle = GetQuadrantSortingAngle(v.LocalPos.x, v.LocalPos.y);
            v.LocalPos *= Radius;
            v.LocalPos += (Vector2)transform.position;

            if(Gridcast(transform.position, v.LocalPos, out rayHit))
                v.LocalPos = transform.InverseTransformPoint(rayHit.point);
            else
                v.LocalPos = transform.InverseTransformPoint(v.LocalPos);
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
                    if (vertex1.LocalPos.sqrMagnitude > vertex2.LocalPos.sqrMagnitude) {
                        allVertices[i] = vertex2;
                        allVertices[i + 1] = vertex1;
                    }
                }

                // ALREADY DONE!!
                if (vertex1.Location == 1) { // Left Ray
                    if (vertex1.LocalPos.sqrMagnitude < vertex2.LocalPos.sqrMagnitude) {
                        allVertices[i] = vertex2;
                        allVertices[i + 1] = vertex1;
                    }
                }
            }
        }
	}
	void ShootRay(Vector2 fromCast, bool isEndpoint) {
		if(!isEndpoint) return;

		Vector2 dirLocal;
		Vector2 dirWorld = fromCast - (Vector2)transform.position;
		Vector2 rayOrigin = fromCast + (dirWorld * CHECK_POINT_LAST_RAY_OFFSET);
		Vector2 rayEnd = rayOrigin + dirWorld.normalized * Radius;
		Vector2 hitPos;

		if (Gridcast(rayOrigin, rayEnd, out rayHit)){
			hitPos = rayHit.point;

			if (Vector2.Distance(hitPos, transform.position) > Radius){
				dirLocal = transform.InverseTransformDirection(dirWorld); 			//local p
				hitPos = transform.TransformPoint(dirLocal.normalized * Radius); 	// world p
			}
		}
		else{
			// TODO: does this need to convert to local?
			dirLocal = transform.InverseTransformDirection(dirWorld);  			//to local
			hitPos = transform.TransformPoint(dirLocal.normalized * Radius); 	//to world
		}

		vL = new Verts();
		vL.LocalPos = transform.InverseTransformPoint(hitPos);	// to local
		vL.Angle = GetQuadrantSortingAngle(vL.LocalPos.x, vL.LocalPos.y);
		allVertices.Add(vL);
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
				_vertexLightMap[_vLightPos.x, _vLightPos.y] = _value;
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
		public static void SetVertexColor(Color _color){
			if (Current.Affected) 	Current.	SetVertexColor(_color);
			if (Top.Affected) 		Top.		SetVertexColor(_color);
			if (Right.Affected) 	Right.		SetVertexColor(_color);
			if (TopRight.Affected) 	TopRight.	SetVertexColor(_color);
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


	private void CalculateLightingForTilesInRange(bool _excludeMe){
		Color _vertexColor;

		Vector2i _vGridPosStart = ConvertToVertexGridSpace(new Vector2i(0, 0), this);
		Vector2i _vGridPosEnd 	= ConvertToVertexGridSpace(new Vector2i((Diameter - 1) * UVControllerBasic.MESH_VERTICES_PER_EDGE, (Diameter - 1) * UVControllerBasic.MESH_VERTICES_PER_EDGE), this);
		_vGridPosStart.x 		= Mathf.Clamp(_vGridPosStart.x, 0, Grid.GridSize.x * UVControllerBasic.MESH_VERTICES_PER_EDGE);
		_vGridPosStart.y 		= Mathf.Clamp(_vGridPosStart.y, 0, Grid.GridSize.y * UVControllerBasic.MESH_VERTICES_PER_EDGE);
		_vGridPosEnd.x 			= Mathf.Clamp(_vGridPosEnd.x,	0, Grid.GridSize.x * UVControllerBasic.MESH_VERTICES_PER_EDGE);
		_vGridPosEnd.y 			= Mathf.Clamp(_vGridPosEnd.y, 	0, Grid.GridSize.y * UVControllerBasic.MESH_VERTICES_PER_EDGE);
		Vector2i _vGridPos 		= _vGridPosStart;

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

			SetVertexSiblings(_vGridPos);

			Vector2 _vWorldPos = ConvertToWorldSpace(_vGridPos);
			int _vTileIndex = _vTilePos.y * UVControllerBasic.MESH_VERTICES_PER_EDGE + _vTilePos.x;
			bool _illuminated;
			float _lightFromThis;
			_vertexColor = GetColorForVertex(
				_vTilePos: 		_vTilePos, 
				_vLightPos: 	ConvertToVertexLightSpace(_vGridPos, this), 
				_gLightPos: 	_gLightPos, 
				_vGridPos: 		_vGridPos, 
				_gGridPos:		ConvertToGridSpace(_vGridPos), 
				_vWorldPos: 	_vWorldPos, 
				_vIndex: 		_vTileIndex, 
				_excludeMe: 	_excludeMe, 
				_illuminated: 	out _illuminated, 
				_lightFromThis: out _lightFromThis
			);

			VertexSiblings.SetValueInVertexLightMap<bool>(VXLightMap_Hit, _illuminated, this);
			VertexSiblings.SetValueInVertexLightMap<float>(VXLightMap_Intensity, _lightFromThis, this);
			
			// get two dots per light describing angle to four strongest lights
			Vector4 _dominantLightIndices = GetShadowCastingLightsIndices(_vGridPos, _excludeMe, _illuminated, _lightFromThis);
			Vector2 _doubleDot_0 = _dominantLightIndices.x >= 0 ? GetDotXY(_vWorldPos, LightManager.AllLights[(int)_dominantLightIndices.x]) : Vector2.zero;
			Vector2 _doubleDot_1 = _dominantLightIndices.y >= 0 ? GetDotXY(_vWorldPos, LightManager.AllLights[(int)_dominantLightIndices.y]) : Vector2.zero;
			Vector2 _doubleDot_2 = _dominantLightIndices.z >= 0 ? GetDotXY(_vWorldPos, LightManager.AllLights[(int)_dominantLightIndices.z]) : Vector2.zero;
			Vector2 _doubleDot_3 = _dominantLightIndices.w >= 0 ? GetDotXY(_vWorldPos, LightManager.AllLights[(int)_dominantLightIndices.w]) : Vector2.zero;

			VertexSiblings.SetUVDots(_doubleDot_0, _doubleDot_1, _doubleDot_2, _doubleDot_3);
			VertexSiblings.SetValueInVertexGridMap<Color>(LightManager.VertMap_TotalColorNoBlur, _vertexColor);
		}

		_vGridPos = _vGridPosStart;
		for (int i = 0; i < _totalIterations; i++, IterateExtraVariables()){
			Vector2i _gLightPos = ConvertToLightSpace(_vGridPos, this);
			Vector2i _vTilePos = ConvertToVertexTileSpace(_vGridPos);
			if (_gLightPos.x > 0 && _vTilePos.x == 0) continue;
			if (_gLightPos.y > 0 && _vTilePos.y == 0) continue;
			if(tilesInRange[_gLightPos.x, _gLightPos.y].x < 0) 	continue;

			int _failAmount = 0;
			int _diffToRightX = _vTilePos.x == 0 ? 1 : 2; // TODO: this can't be right. It should be == 1, right?
			int _diffToAboveY = _vTilePos.y == 0 ? 1 : 2;

			_vertexColor = Color.clear;
			_vertexColor += TryGetAdjacentValueInVertexGridMap(LightManager.VertMap_TotalColorNoBlur, _vGridPos.x - 1, 				_vGridPos.y - 1, 				ref _failAmount);
			_vertexColor += TryGetAdjacentValueInVertexGridMap(LightManager.VertMap_TotalColorNoBlur, _vGridPos.x, 					_vGridPos.y - 1, 				ref _failAmount);
			_vertexColor += TryGetAdjacentValueInVertexGridMap(LightManager.VertMap_TotalColorNoBlur, _vGridPos.x + _diffToRightX, 	_vGridPos.y - 1, 				ref _failAmount);
			_vertexColor += TryGetAdjacentValueInVertexGridMap(LightManager.VertMap_TotalColorNoBlur, _vGridPos.x - 1, 				_vGridPos.y, 					ref _failAmount);
			_vertexColor += TryGetAdjacentValueInVertexGridMap(LightManager.VertMap_TotalColorNoBlur, _vGridPos.x, 					_vGridPos.y, 					ref _failAmount); // this vertex
			_vertexColor += TryGetAdjacentValueInVertexGridMap(LightManager.VertMap_TotalColorNoBlur, _vGridPos.x + _diffToRightX, _vGridPos.y, 					ref _failAmount);
			_vertexColor += TryGetAdjacentValueInVertexGridMap(LightManager.VertMap_TotalColorNoBlur, _vGridPos.x - 1, 				_vGridPos.y + _diffToAboveY, 	ref _failAmount);
			_vertexColor += TryGetAdjacentValueInVertexGridMap(LightManager.VertMap_TotalColorNoBlur, _vGridPos.x, 					_vGridPos.y + _diffToAboveY, 	ref _failAmount);
			_vertexColor += TryGetAdjacentValueInVertexGridMap(LightManager.VertMap_TotalColorNoBlur, _vGridPos.x + _diffToRightX, 	_vGridPos.y + _diffToAboveY, 	ref _failAmount);
			_vertexColor /= Mathf.Max(Mathf.Pow(UVControllerBasic.MESH_VERTICES_PER_EDGE, 2) - _failAmount, 1);
			_vertexColor.a = 1;

			SetVertexSiblings(_vGridPos);
			VertexSiblings.SetVertexColor(_vertexColor);
		}
	}

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

	void SetVertexSiblings(Vector2i _vGridPos) {
		Vector2i _vTilePos = ConvertToVertexTileSpace(_vGridPos);
		Vector2i _gGridPos = ConvertToGridSpace(_vGridPos);
		bool _isOnLeftEdge = _vTilePos.x == 0;
		bool _isOnRightEdge = _vTilePos.x == UVControllerBasic.MESH_VERTICES_PER_EDGE - 1;
		bool _affectsTopHalf = _vTilePos.y == UVControllerBasic.MESH_VERTICES_PER_EDGE - 1;

		VertexSiblings.Current.SetNewValues(
			true,
			_gGridPos.x, _gGridPos.y,
			_vTilePos.x, _vTilePos.y
		);
		VertexSiblings.OwnTopHalf.SetNewValues(
			_affectsTopHalf,
			_gGridPos.x, _gGridPos.y,
			_vTilePos.x, _vTilePos.y + 1
		);
		VertexSiblings.OwnTopHalfLeft.SetNewValues(
			_affectsTopHalf && _isOnLeftEdge,
			_gGridPos.x, _gGridPos.y,
			0, _vTilePos.y + 2
		);
		VertexSiblings.OwnTopHalfRight.SetNewValues(
			_affectsTopHalf && _isOnRightEdge,
			_gGridPos.x, _gGridPos.y,
			1, _vTilePos.y + 2
		);
		VertexSiblings.Right.SetNewValues(
			_isOnRightEdge && _gGridPos.x + 1 < Grid.GridSize.x,
			_gGridPos.x + 1, _gGridPos.y,
			0, _vTilePos.y
		);
		VertexSiblings.RightTopHalf.SetNewValues(
			_isOnRightEdge && _affectsTopHalf,
			_gGridPos.x + 1, _gGridPos.y,
			0, _vTilePos.y + 1
		);
		VertexSiblings.RightTopHalfLeft.SetNewValues(
			_isOnRightEdge && _affectsTopHalf,
			_gGridPos.x + 1, _gGridPos.y,
			0, _vTilePos.y + 2
		);
		VertexSiblings.Top.SetNewValues(
			_affectsTopHalf && _gGridPos.y + 1 < Grid.GridSize.y,
			_gGridPos.x, _gGridPos.y + 1,
			_vTilePos.x, 0
		);
		VertexSiblings.TopRight.SetNewValues(
			VertexSiblings.Top.Affected && VertexSiblings.Right.Affected,
			_gGridPos.x + 1, _gGridPos.y + 1,
			0, 0
		);
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

	private Color GetColorForVertex(Vector2i _vTilePos, Vector2i _vLightPos, Vector2i _gLightPos, Vector2i _vGridPos, Vector2i _gGridPos, Vector2 _vWorldPos,  int _vIndex, bool _excludeMe, out bool _illuminated, out float _lightFromThis){
		// take all previously hit light and recompile into one color
		Color _totalColor = Color.clear;
		int[] _lightsInRange = LightManager.GridMap_LightsInRange[_gGridPos.x, _gGridPos.y];
		int actualCount = 0;
		for (int i = 0; i < _lightsInRange.Length; i++){
			if (_lightsInRange[i] == -1) break;

			CustomLight _otherLight = LightManager.AllLights[_lightsInRange[i]];
			if (_otherLight == this) continue;

			Vector2i _vLightPosOtherLight = ConvertToVertexLightSpace(_vGridPos, _otherLight);
			bool _hit = _otherLight.VXLightMap_Hit[_vLightPosOtherLight.x, _vLightPosOtherLight.y];
			if (!_hit) continue;

			actualCount++;
			CombineLightWithOthersLight(_otherLight.VXLightMap_Intensity[_vLightPosOtherLight.x, _vLightPosOtherLight.y], _otherLight.GetLightColor(), ref _totalColor);
		}

		float _distance = (_vWorldPos - myInspector.MyTileObject.MyTile.WorldPosition).magnitude;
		_lightFromThis = Intensity * Mathf.Pow(1 - (_distance / Radius), 2);
		_illuminated = !_excludeMe && IsInsideLightMesh(_vWorldPos);

		if(_illuminated){
			CombineLightWithOthersLight(_lightFromThis, GetLightColor(), ref _totalColor);
		}

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

	private int[] _indices 			= new int[LightManager.MAX_LIGHTS_CASTING_SHADOWS];
	private float[] _intensities 	= new float[LightManager.MAX_LIGHTS_CASTING_SHADOWS];
	private Vector4 GetShadowCastingLightsIndices(Vector2i _vGridPos, bool _excludeMe, bool _illuminated, float _lightFromThis){
		Vector4 _currentIndices = LightManager.VertMap_DomLightIndices[_vGridPos.x, _vGridPos.y];

		if (!_excludeMe){
			_indices[0] = (int)_currentIndices.x;
			_indices[1] = (int)_currentIndices.y;
			_indices[2] = (int)_currentIndices.z;
			_indices[3] = (int)_currentIndices.w;

			Vector4 _currentIntensities = LightManager.VertMap_DomLightIntensities[_vGridPos.x, _vGridPos.y];
			_intensities[0] = _currentIntensities.x;
			_intensities[1] = _currentIntensities.y;
			_intensities[2] = _currentIntensities.z;
			_intensities[3] = _currentIntensities.w;

			if(!_illuminated) _lightFromThis = 0;
			for (int i = LightManager.MAX_LIGHTS_CASTING_SHADOWS - 1; i >= 0; i--){
				if (_lightFromThis >= _intensities[i] && (i == 0 || _lightFromThis <= _intensities[i - 1])) {
					_indices.Insert(i, LightIndex);
					_intensities.Insert(i, _lightFromThis);
					break;
				}
			}

			_currentIndices.x = _indices[0];
			_currentIndices.y = _indices[1];
			_currentIndices.z = _indices[2];
			_currentIndices.w = _indices[3];
			LightManager.VertMap_DomLightIndices[_vGridPos.x, _vGridPos.y] = _currentIndices;

			_currentIntensities.x = _intensities[0];
			_currentIntensities.y = _intensities[1];
			_currentIntensities.z = _intensities[2];
			_currentIntensities.w = _intensities[3];
			LightManager.VertMap_DomLightIntensities[_vGridPos.x, _vGridPos.y] = _currentIntensities;
		}

		return _currentIndices;
	}

    void RenderLightMesh() {
        //-- Step 5: fill the mesh with vertices--//

        Vector3[] initVerticesMeshLight = new Vector3[allVertices.Count + 1];
        initVerticesMeshLight[0] = Vector3.zero;

        for (int i = 0; i < allVertices.Count; i++)
            initVerticesMeshLight[i + 1] = allVertices[i].LocalPos;

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

	// approximate but high performance way of calculating angle. Range: -1 to 2.99
    private float GetQuadrantSortingAngle(float x, float y) { 
        float angle = y / (Mathf.Abs(x) + Mathf.Abs(y));
        if (x < 0) angle = 2 - angle;
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

