using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor; // for debug-gizmos only

public class Verts {
    public float Angle;
    public int Location; // 1 = left end point | 0 = middle | -1 = right endpoint
    public Vector2 Pos;
    public bool Endpoint;
}

public class CustomLight : MonoBehaviour {

    private static List<CustomLight> AllLights = new List<CustomLight>();

    public bool DebugMode = false;

    [Space]

    public string version = "1.0.5"; //release date 09/01/2017
    public Material lightMaterial;
    public int lightRadius = 20;
    [Range(0, 1)] public float Intensity = 1;
    public byte LightColor = 40; // bright yellow
    public LayerMask layer;
    [Range(4, 40)] public int lightSegments = 8;
    public Transform MeshTransform;

    //[HideInInspector] public PolygonCollider2D[] allMeshes; // Array for all of the meshes in our scene
    [HideInInspector]
    public List<Verts> allVertices = new List<Verts>(); // Array for all of the vertices in our meshes

    private Mesh lightMesh; // Mesh for our light mesh
    private new MeshRenderer renderer;
    private CanInspect myInspector;

	private delegate void mIterateVariables();


	void OnEnable() {
        AllLights.Add(this);

        if(myInspector == null)
            myInspector = GetComponent<CanInspect>();

        myInspector.PostPickUp += PostPickUp;
        myInspector.PostPutDown += PostPutDown;

        SinCosTable.InitSinCos();

        MeshFilter meshFilter = MeshTransform.GetComponent<MeshFilter>();        // Add a Mesh Filter component to the light game object so it can take on a form
        renderer = MeshTransform.GetComponent<MeshRenderer>();               // Add a Mesh Renderer component to the light game object so the form can become visible
        renderer.sharedMaterial = lightMaterial;                                                // Add the specified material
        lightMesh = new Mesh();                                                                 // create a new mesh for our light mesh
        meshFilter.mesh = lightMesh;                                                            // Set this newly created mesh to the mesh filter
        lightMesh.name = "Light Mesh";                                                          // Give it a name
        lightMesh.MarkDynamic();      
    }
    void OnDisable() {
        myInspector.PostPickUp -= PostPickUp;
        myInspector.PostPutDown -= PostPutDown;

        AllLights.Remove(this);
    }

    void PostPickUp(){ // TODO: would be good if picked-up objects were visible and jumped between tiles when moving. that way the light can update as it's moved as well.
        UpdateAllLights();
    }
    void PostPutDown() {
        UpdateAllLights();
    }

	private static Vector4[,] lightIndexMap; 		// the four most dominant lights' indices for each tile
	private static Vector4[,] lightIntensityMap; 	// how strongly the dominant lights hit each tile
	private static Color[,] lightColorMap; 			// the current color of each tile
	private static Material GridMaterial;
    //[EasyButtons.Button]
	public static void UpdateAllLights() {
        float _timeStarted = Time.realtimeSinceStartup;

		int _mapSizeX = Grid.GridSizeX * UVControllerBasic.MESH_VERTICES_PER_EDGE;
		int _mapSizeY = Grid.GridSizeY * UVControllerBasic.MESH_VERTICES_PER_EDGE;
		lightIndexMap 		= new Vector4	[_mapSizeX, _mapSizeY];
		lightIntensityMap 	= new Vector4	[_mapSizeX, _mapSizeY];
		lightColorMap 		= new Color		[_mapSizeX, _mapSizeY];

		if (GridMaterial == null)
			GridMaterial = Grid.Instance.grid[0, 0].MyUVController.Renderer.sharedMaterial;

        for (int i = 0; i < AllLights.Count; i++) {
            if (AllLights[i].lightMesh != null)
                AllLights[i].lightMesh.Clear();

            if (AllLights[i].myInspector.CurrentState != CanInspect.State.Default)
                continue;

            AllLights[i].UpdateLight();
        }
        //CalculateLightingForGrid();
        Debug.Log("All Lights Updated: " + (Time.realtimeSinceStartup - _timeStarted) + "s");
    }

    void UpdateLight() {
        GetAllMeshes();
		PreparePooledColliders();
		SetLight();
		DiscardPooledColliders();
		RenderLightMesh();
        ResetBounds();
		UpdatePointCollisionArray();
		CalculateLightingForTilesInRange();
	}

    private Tile t;
    private bool breakLoops = false;
    private Tile[,] tilesInRange;
    private Tile[,] tilesInRangeWithCollider;
	private Vector2i[,] verticesInRange; // only vertices in the bottom-half, to be precise
	void GetAllMeshes() {
		int _radius = lightRadius + 1; // +1 because we use neighbours for smoothing and optimization
		int _diameter = _radius * 2;
		int _leftMostX = Mathf.Max(myInspector.MyTileObject.MyTile.GridCoord.x - _radius, 0);
		int _bottomMostY = Mathf.Max(myInspector.MyTileObject.MyTile.GridCoord.y - _radius, 0);

		int _d = _diameter + 1; // +1 to account for center tile
		tilesInRange 				= new Tile[_d, _d];
		tilesInRangeWithCollider 	= new Tile[_d, _d];
		verticesInRange 			= new Vector2i[_d * UVControllerBasic.MESH_VERTICES_PER_EDGE, _d * UVControllerBasic.MESH_VERTICES_PER_EDGE];

		Vector2 _lightPos = myInspector.MyTileObject.MyTile.WorldPosition;

        int _skipRestOfThisX = -1;
		int vx = 0, vy = 0, x = 0, y = 0;
		mIterateVariables IterateExtraVariables = delegate (){
			vx++;
			if (vx == UVControllerBasic.MESH_VERTICES_PER_EDGE){
				vx = 0;
				vy++;
				if (vy == UVControllerBasic.MESH_VERTICES_PER_EDGE){
					vy = 0;
					x++;
					if (x == Grid.GridSizeX){
						x = 0;
						y++;
					}
				}
			}
		};
		int _totalIterations = Grid.GridSizeX * UVControllerBasic.MESH_VERTICES_PER_EDGE * Grid.GridSizeY * UVControllerBasic.MESH_VERTICES_PER_EDGE;
        for (int i = 0; i < _totalIterations; i++, IterateExtraVariables()){
			if(x == _skipRestOfThisX)
				continue;
			_skipRestOfThisX = -1;

			if (Mathf.RoundToInt((GetVertexWorldPos(x, y, vx, vy) - _lightPos).magnitude) <= _radius) { 
				_skipRestOfThisX = x;

				int _xLocal = x - _leftMostX;
				int _yLocal = y - _bottomMostY;
				tilesInRange[_xLocal, _yLocal] = Grid.Instance.grid[x, y];

				int _vertsPerEdge = UVControllerBasic.MESH_VERTICES_PER_EDGE;
				for (int iy = 0; iy < _vertsPerEdge; iy++){
					for (int ix = 0; ix < _vertsPerEdge; ix++){
						verticesInRange[_xLocal * _vertsPerEdge, _yLocal * _vertsPerEdge] = new Vector2i(_xLocal + ix, _yLocal + iy);
					}
				}
				if(ObjectPooler.Instance.HasPoolForID(Grid.Instance.grid[x, y].ExactType))
					tilesInRangeWithCollider[_xLocal, _yLocal] = tilesInRange[_xLocal, _yLocal];
			}
        }

		// for (int y = 0; y < Grid.GridSizeY; y++) {
		//     for (int x = 0; x < Grid.GridSizeX; x++) {
		//         t = Grid.Instance.grid[x, y];
		//         PolygonCollider2D _coll = ObjectPooler.Instance.GetPooledObject<PolygonCollider2D>(t.ExactType);
		//         if (_coll == null) { 
		//             continue;
		//         }

		//         _coll.transform.position = t.WorldPosition;

		//         breakLoops = false;
		//         for (int pIndex = 0; pIndex < _coll.pathCount; pIndex++){
		//             for (int vIndex = 0; vIndex < _coll.GetPath(pIndex).Length; vIndex++){
		//                 if (((t.WorldPosition + _coll.GetPath(pIndex)[vIndex]) - _lightPos).magnitude <= lightRadius) {
		//                     tilesInRange.Add(t);
		//                     breakLoops = true;
		//                 }

		//                 if (breakLoops)
		//                     break;
		//             }

		//             if(breakLoops) 
		//                 break;
		//         }

		//         _coll.GetComponent<PoolerObject>().ReturnToPool();
		//     }
		// }
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
	void SetLight() {
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
			Tile _t = tilesInRangeWithCollider[x, y];
			if(_t == null)
				continue;
            PolygonCollider2D _coll = ObjectPooler.Instance.GetPooledObject<PolygonCollider2D>(tilesInRangeWithCollider[x, y].ExactType);
            _coll.transform.position = tilesInRangeWithCollider[x, y].WorldPosition;

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
                    if ((v.Pos).magnitude <= lightRadius)
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
                        mag = lightRadius;
                        from = (Vector2)fromCast + (dir * CHECK_POINT_LAST_RAY_OFFSET);

                        int debugger = -1;

                        if(Gridcast(from, (from + dir.normalized * lightRadius), out rayHit)){
                            hitPos = rayHit.point;
                            debugger = 1;

                            if (Vector2.Distance(hitPos, transform.position) > lightRadius){
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
            v.Pos = new Vector3((SinCosTable.sSinArray[theta]), (SinCosTable.sCosArray[theta]), 0); // in degrees (previous calculate)
            v.Angle = GetVectorAngle(v.Pos.x, v.Pos.y);
            v.Pos *= lightRadius;
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

	private const float VERTEX_DISTANCE = 0.5f;
    static Vector2 GetVertexWorldPos(int _x, int _y, int _vx, int _vy){
        Vector2 _offset = new Vector2(
            (_vx - 1) * VERTEX_DISTANCE,
            (_vy - 1) * VERTEX_DISTANCE
        );
        return Grid.Instance.grid[_x, _y].WorldPosition + _offset;
    }
	private delegate Color mTryGetNeighbourColor(int _x, int _y);
    private delegate void mApplyToVertex(int _x, int _y, int _vertex);
	private void CalculateLightingForTilesInRange(){

		// find colors and dots for all lights and apply
		Color[,] _cachedColors = new Color[verticesInRange.GetLength(0), verticesInRange.GetLength(1)];
		Vector2 _offset = new Vector2();
		Vector2 _vertexWorldPos;
		Vector2[] _dots = new Vector2[4];

		int vx = 0, vy = 0, _vIndex = 0, _xLocal = 0, _yLocal = 0;
		mIterateVariables IterateExtraVariables = delegate (){
			vx++;
			_vIndex++;
			if (vx == UVControllerBasic.MESH_VERTICES_PER_EDGE){
				vx = 0;
				vy++;
				if (vy == UVControllerBasic.MESH_VERTICES_PER_EDGE){
					vy = 0;
					_vIndex = 0;
					_xLocal++;
					if (_xLocal == tilesInRange.GetLength(0)){
						_xLocal = 0;
						_yLocal++;
					}
				}
			}
		};
		int _totalIterations = (int)(verticesInRange.GetLength(0) * verticesInRange.GetLength(1));
		for (int i = 0; i < _totalIterations; i++, IterateExtraVariables()){
			if (_xLocal > 0 && vx == 0)
				continue;
			if (_yLocal > 0 && vy == 0)
				continue;
			if(tilesInRange[_xLocal, _yLocal] == null)
				continue;

			int _xWorld = tilesInRange[_xLocal, _yLocal].GridCoord.x;
			int _yWorld = tilesInRange[_xLocal, _yLocal].GridCoord.y;

			//int _vIndex = vy * 3 + vx;
			// int _totalX = x * 3 + vx;
			// int _totalY = y * 3 + vy;

			// get colors from lights
			_vertexWorldPos = GetVertexWorldPos(_xWorld, _yWorld, vx, vy);
			float _dist = (_vertexWorldPos - myInspector.MyTileObject.MyTile.WorldPosition).magnitude;
			bool _illuminated;
			float _lightFromThis;
			Color _finalColor = GetColorForVertex(_xWorld, _yWorld, _vertexWorldPos, _dist, out _illuminated, out _lightFromThis);
			_finalColor.a = 1;
			
			// get two dots per light describing angle to four strongest lights
			Vector4 _dominantLightIndices = GetShadowCastingLightsIndices(_xWorld, _yWorld, _vertexWorldPos, _illuminated, _lightFromThis);
			_dots[0] = GetDotXY(_vertexWorldPos, (int)_dominantLightIndices.x);
			_dots[1] = GetDotXY(_vertexWorldPos, (int)_dominantLightIndices.y);
			_dots[2] = GetDotXY(_vertexWorldPos, (int)_dominantLightIndices.z);
			_dots[3] = GetDotXY(_vertexWorldPos, (int)_dominantLightIndices.w);

			int _vxLocal = _xLocal * UVControllerBasic.MESH_VERTICES_PER_EDGE + vx;
			int _vyLocal = _yLocal * UVControllerBasic.MESH_VERTICES_PER_EDGE + vy;

			mApplyToVertex ApplyUVDots = delegate (int _x, int _y, int _vertex){
				if(tilesInRange[_x, _y] == null)
					return;
				tilesInRange[_x, _y].MyUVController.SetUVDots(_vertex, _dots[0], _dots[1], _dots[2], _dots[3]);
			};
			Vector3i[] _neighbourIndices;
			_neighbourIndices = GetNeighboursToApplyStuffTo(_xLocal, _yLocal, _xWorld, _yWorld, vx, vy, _includeTopHalfStuff: true, _xLocalMax: tilesInRange.GetLength(0), _yLocalMax: tilesInRange.GetLength(1));
			for (int i2 = 0; i2 < _neighbourIndices.Length; i2++){
				ApplyUVDots(_neighbourIndices[i2].x, _neighbourIndices[i2].y, _neighbourIndices[i2].z);
			}
			_neighbourIndices = GetNeighboursToApplyStuffTo(_vxLocal, _vyLocal, _xWorld, _yWorld, vx, vy, _includeTopHalfStuff: false, _xLocalMax: _cachedColors.GetLength(0), _yLocalMax: _cachedColors.GetLength(1));
			for (int i2 = 0; i2 < _neighbourIndices.Length; i2++){
				_cachedColors[_neighbourIndices[i2].x, _neighbourIndices[i2].y] = _finalColor;
			}





			mApplyToVertex ApplyVertexColor = delegate (int _x, int _y, int _vertex){
				if (tilesInRange[_x, _y] == null)
					return;
				tilesInRange[_x, _y].MyUVController.SetVertexColor(_vertex, _finalColor);
			};
			_neighbourIndices = GetNeighboursToApplyStuffTo(_xLocal, _yLocal, _xWorld, _yWorld, vx, vy, _includeTopHalfStuff: false, _xLocalMax: tilesInRange.GetLength(0), _yLocalMax: tilesInRange.GetLength(1));
			for (int i2 = 0; i2 < _neighbourIndices.Length; i2++){
				ApplyVertexColor(_neighbourIndices[i2].x, _neighbourIndices[i2].y, _neighbourIndices[i2].z);
			}
		}
		return;

		// blur and apply colors
		Color[] _neighbourColors = new Color[9];
		int _maxX = _cachedColors.GetLength(0) - 1;
		int _maxY = _cachedColors.GetLength(1) - 1;
		
		vx = 0; 
		vy = 0; 
		_vIndex = 0; 
		_xLocal = 0; 
		_yLocal = 0;
		for (int i = 0; i < _totalIterations; i++, IterateExtraVariables()){
			if (_xLocal > 0 && vx == 0)
				continue;
			if (_yLocal > 0 && vy == 0)
				continue;
			if(tilesInRange[_xLocal, _yLocal] == null)
				continue;

			int _xWorld = tilesInRange[_xLocal, _yLocal].GridCoord.x;
			int _yWorld = tilesInRange[_xLocal, _yLocal].GridCoord.y;
			int _vxLocal = _xLocal * UVControllerBasic.MESH_VERTICES_PER_EDGE + vx;
			int _vyLocal = _yLocal * UVControllerBasic.MESH_VERTICES_PER_EDGE + vy;

			// try get colors from neighbouring vertices (for smoothing)
			int _diffToRightX = vx == 0 ? 1 : 2;
			int _diffToAboveY = vy == 0 ? 1 : 2;
			int _failAmount = 0;
			mTryGetNeighbourColor TryGetNeighbourColor = delegate (int _x, int _y){
				if (_x < 0 || _x > _maxX || _y < 0 || _y > _maxY){
					_failAmount++;
					return Color.clear;
				}
				return _cachedColors[_x, _y];
			};

			_neighbourColors[0] = TryGetNeighbourColor(_vxLocal - 1, 				_vyLocal - 1);
			_neighbourColors[1] = TryGetNeighbourColor(_vxLocal, 					_vyLocal - 1);
			_neighbourColors[2] = TryGetNeighbourColor(_vxLocal + _diffToRightX, 	_vyLocal - 1);
			_neighbourColors[3] = TryGetNeighbourColor(_vxLocal - 1, 				_vyLocal);
			_neighbourColors[4] = TryGetNeighbourColor(_vxLocal, _vyLocal); // this tile
			_neighbourColors[5] = TryGetNeighbourColor(_vxLocal + _diffToRightX, 	_vyLocal);
			_neighbourColors[6] = TryGetNeighbourColor(_vxLocal - 1, 				_vyLocal + _diffToAboveY);
			_neighbourColors[7] = TryGetNeighbourColor(_vxLocal, 					_vyLocal + _diffToAboveY);
			_neighbourColors[8] = TryGetNeighbourColor(_vxLocal + _diffToRightX, 	_vyLocal + _diffToAboveY);

			Color _myColor = new Color();
			for (int i2 = 0; i2 < _neighbourColors.Length; i2++)
				_myColor += _neighbourColors[i2];

			_myColor /= Mathf.Max(_neighbourColors.Length - _failAmount, 1);



			_myColor = _cachedColors[_xLocal, _yLocal];




			_myColor.a = 1;

			mApplyToVertex ApplyVertexColor = delegate (int _x, int _y, int _vertex){
				if(tilesInRange[_x, _y] == null)
					return;
				tilesInRange[_x, _y].MyUVController.SetVertexColor(_vertex, _myColor);
			};
			Vector3i[] _neighbourIndices = GetNeighboursToApplyStuffTo(_xLocal, _yLocal, _xWorld, _yWorld, vx, vy, _includeTopHalfStuff: false, _xLocalMax: tilesInRange.GetLength(0), _yLocalMax: tilesInRange.GetLength(1));
			for (int i2 = 0; i2 < _neighbourIndices.Length; i2++){
				ApplyVertexColor(_neighbourIndices[i2].x, _neighbourIndices[i2].y, _neighbourIndices[i2].z);
			}
		}
	}
	// private static void CalculateLightingForGrid() {

	//     // find colors and dots for all lights and apply
	//     Color[,] _cachedColors = new Color[Grid.GridSizeX * 3, Grid.GridSizeY * 3];
	// 	Vector2 _offset = new Vector2();
	//     Vector4 _dominantLights;
	//     Vector2 _vertexWorldPos;
	//     Color _finalColor;
	//     Vector2[] _dots = new Vector2[4];
	//     for (int y = 0; y < Grid.GridSizeY; y++){ // note: merging these loops into one appears to do nothing for performance, so don't
	//         for (int x = 0; x < Grid.GridSizeX; x++){
	//             for (int vy = 0; vy < 3; vy++){
	//                 if(y > 0 && vy == 0)
	//                     continue;

	//                 for (int vx = 0; vx < 3; vx++){
	//                     if(x > 0 && vx == 0)
	//                         continue;

	//                     int _vIndex = vy * 3 + vx;
	//                     int _totalX = x * 3 + vx;
	//                     int _totalY = y * 3 + vy;

	//                     // get colors from lights
	//                     _vertexWorldPos = GetVertexWorldPos(x, y, vx, vy);
	//                     _finalColor = GetTotalVertexLighting(_vertexWorldPos, out _dominantLights);
	//                     _finalColor.a = 1;

	// 					// get two dots per light describing angle to respective lights
	//                     _dots[0] = GetDotXY(_vertexWorldPos, (int)_dominantLights.x);
	//                     _dots[1] = GetDotXY(_vertexWorldPos, (int)_dominantLights.y);
	//                     _dots[2] = GetDotXY(_vertexWorldPos, (int)_dominantLights.z);
	//                     _dots[3] = GetDotXY(_vertexWorldPos, (int)_dominantLights.w);

	//                     mApplyToVertex ApplyUVDots = delegate (int _x, int _y, int _vertex){
	//                         Grid.Instance.grid[_x, _y].MyUVController.SetUVDots(_vertex, _dots[0], _dots[1], _dots[2], _dots[3]);
	// 					};

	//                     ApplyUVDots(x, y, _vIndex);
	//                     _cachedColors[_totalX, _totalY] = _finalColor;

	// 					bool _affectsTopHalf = vy == 2;
	// 					if (_affectsTopHalf){ // top half of *this* tile
	//                          ApplyUVDots(x, y, (vy + 1) * 3 + vx);
	//                         if (vx == 2)
	//                             ApplyUVDots(x, y, (vy + 2) * 3 + 1);
	// 					}

	// 					bool _affectsR = x < Grid.GridSizeX - 1 && vx == 2;
	//                     bool _affectsT = y < Grid.GridSizeY - 1 && _affectsTopHalf;
	//                     if (_affectsR){
	//                         ApplyUVDots(x + 1, y, _vIndex - vx);
	//                         _cachedColors[_totalX + 1, _totalY] = _finalColor;

	// 						if (_affectsTopHalf) { // top half of right neighbour tile
	//                             ApplyUVDots(x + 1, y, (vy + 1) * 3);
	//                             ApplyUVDots(x + 1, y, (vy + 2) * 3);
	// 						}
	// 					}
	//                     if (_affectsT){
	//                         ApplyUVDots(x, y + 1, vx);
	//                         _cachedColors[_totalX, _totalY + 1] = _finalColor;
	// 					}
	//                     if (_affectsR && _affectsT){
	//                         ApplyUVDots(x + 1, y + 1, 0);
	//                         _cachedColors[_totalX + 1, _totalY + 1] = _finalColor;
	// 					}
	//                 }
	//             }
	//         }
	//     }

	//     // blur and apply colors
	//     Color[] _neighbourColors = new Color[8];
	//     Color _myColor;
	//     Color _myColorMod = new Color();
	//     int _maxX = _cachedColors.GetLength(0) - 1;
	//     int _maxY = _cachedColors.GetLength(1) - 1;
	//     for (int y = 0; y < Grid.GridSizeY; y++){
	//         for (int x = 0; x < Grid.GridSizeX; x++){
	//             for (int vy = 0; vy < 3; vy++){
	//                 if(y > 0 && vy == 0)
	//                     continue;

	//                 for (int vx = 0; vx < 3; vx++){
	//                     if(x > 0 && vx == 0)
	//                         continue;

	// 					// try get colors from neighbouring vertices (for smoothing)
	//                     int _vIndex = vy * 3 + vx;
	//                     int _totalX = x * 3 + vx;
	//                     int _totalY = y * 3 + vy;
	// 					int _diffToRightX = vx == 0 ? 1 : 2;
	// 					int _diffToAboveY = vy == 0 ? 1 : 2;
	// 					int _failAmount = 0;
	// 					mTryGetNeighbourColor TryGetNeighbourColor = delegate (int _x, int _y){
	// 						if (_x < 0 || _x > _maxX || _y < 0 || _y > _maxY){
	// 							_failAmount++;
	// 							return Color.clear;
	// 						}
	// 						return _cachedColors[_x, _y];
	// 					};
	// 					_neighbourColors[0] = TryGetNeighbourColor(_totalX - 1, _totalY - 1);
	// 					_neighbourColors[1] = TryGetNeighbourColor(_totalX, 	_totalY - 1);
	// 					_neighbourColors[2] = TryGetNeighbourColor(_totalX + _diffToRightX, _totalY - 1);
	// 					_neighbourColors[3] = TryGetNeighbourColor(_totalX - 1, _totalY);
	// 					_neighbourColors[4] = TryGetNeighbourColor(_totalX + _diffToRightX, _totalY);
	// 					_neighbourColors[5] = TryGetNeighbourColor(_totalX - 1, _totalY + _diffToAboveY);
	// 					_neighbourColors[6] = TryGetNeighbourColor(_totalX, 	_totalY + _diffToAboveY);
	// 					_neighbourColors[7] = TryGetNeighbourColor(_totalX + _diffToRightX, _totalY + _diffToAboveY);

	// 					// apply found colors
	//                     for (int i = 0; i < _neighbourColors.Length; i++)
	//                         _myColorMod += _neighbourColors[i];
	//                     _myColorMod /= Mathf.Max(_neighbourColors.Length - _failAmount, 1);
	// 					_myColor = (_cachedColors[_totalX, _totalY] + _myColorMod) * 0.5f;
	//                     _myColor.a = 1;

	//                     mApplyToVertex ApplyVertexColor = delegate (int _x, int _y, int _vertex){
	//                         Grid.Instance.grid[_x, _y].MyUVController.SetVertexColor(_vertex, _myColor);
	// 					};
	//                     ApplyVertexColor(x, y, _vIndex);

	// 					bool _affectsTopHalf = vy == 2;
	// 					if (_affectsTopHalf){ // top half of *this* tile
	// 						ApplyVertexColor(x, y, (vy + 1) * 3 + vx);
	// 						if(vx == 2)
	// 							ApplyVertexColor(x, y, (vy + 2) * 3 + 1);
	// 					}

	// 					bool _affectsR = x < Grid.GridSizeX - 1 && vx == 2;
	//                     bool _affectsT = y < Grid.GridSizeY - 1 && vy == 2;
	// 					if (_affectsR) { 
	// 						ApplyVertexColor(x + 1, y, _vIndex - vx);
	// 						if (_affectsTopHalf) { // top half of right neighbour tile
	// 							ApplyVertexColor(x + 1, y, (vy + 1) * 3);
	// 							ApplyVertexColor(x + 1, y, (vy + 2) * 3);
	// 						}
	// 					}
	//                     if (_affectsT)
	//                         ApplyVertexColor(x, y + 1, vx);
	//                     if (_affectsR && _affectsT)
	//                         ApplyVertexColor(x + 1, y + 1, 0);
	//                 }
	//             }
	//         }
	//     }
	// }

	Vector3i[] GetNeighboursToApplyStuffTo(int _xLocal, int _yLocal, int _xWorld, int _yWorld, int _vx, int _vy, bool _includeTopHalfStuff, int _xLocalMax = -1, int _yLocalMax = -1) {
		int _vertsPerEdge = UVControllerBasic.MESH_VERTICES_PER_EDGE;
		int _vIndex = _vy * _vertsPerEdge + _vx;

		List<Vector3i> _indexList = new List<Vector3i>() { 
			new Vector3i(_xLocal, _yLocal, _vIndex)
		};

		bool _affectsTopHalf = _includeTopHalfStuff && _vy == _vertsPerEdge - 1;
		bool _affectsR = (_xLocalMax < 0 || _xLocal + 1 < _xLocalMax) && _xWorld + 1 < Grid.GridSizeX && _vx == 2;
		bool _affectsT = (_yLocalMax < 0 || _yLocal + 1 < _yLocalMax) && _yWorld + 1 < Grid.GridSizeY && _vy == _vertsPerEdge - 1;

		if (_affectsTopHalf){ // top half of *this* tile
			_indexList.Add(new Vector3i(_xLocal, _yLocal, (_vy + 1) * _vertsPerEdge + _vx));

			if(_xWorld == 0 && _vx == 0)
				_indexList.Add(new Vector3i(_xLocal, _yLocal, (_vy + 2) * _vertsPerEdge));
			if (_vx == 2)
				_indexList.Add(new Vector3i(_xLocal, _yLocal, ((_vy + 2) * _vertsPerEdge) + 1)); // +1 instead of _vx because top-half has fewer vertices ^^'
		}
		if (_affectsR){
			_indexList.Add(new Vector3i(_xLocal + 1, _yLocal, _vIndex - _vx));

			if (_affectsTopHalf){ // top half of right neighbour tile
				_indexList.Add(new Vector3i(_xLocal + 1, _yLocal, (_vy + 1) * _vertsPerEdge));
				_indexList.Add(new Vector3i(_xLocal + 1, _yLocal, (_vy + 2) * _vertsPerEdge));
			}
		}
		if (_affectsT){
			_indexList.Add(new Vector3i(_xLocal, _yLocal + 1, _vx));
		}
		if (_affectsR && _affectsT){
			_indexList.Add(new Vector3i(_xLocal + 1, _yLocal + 1, 0));
		}

		return _indexList.ToArray();
	}

	static float GetAngle01(Vector2 _pos1, Vector2 _pos2, Vector2 _referenceAngle, int maxAngle) { // TODO: replace with GetAngleClockwise!
        return maxAngle * (0.5f * (1 + Vector2.Dot(
                                            _referenceAngle, 
                                            Vector3.Normalize(_pos1 - _pos2))));
    }
	// find the horizontal and vertical dot-products between two vectors
	static Vector2 GetDotXY(Vector2 _pos1, int _lightIndex){
        if(_lightIndex < 0)
            return Vector2.zero; 

        Vector2 _lightPos = AllLights[_lightIndex].transform.position;

        // get an angle between 0->1. The angle goes all the way around, but counter-clockwise, so sorta like a clock and unlike a dot
		float _vertical 	= (Vector2.Dot(Vector2.down, (_pos1 - _lightPos).normalized) + 1) * 0.5f;
		float _horizontal 	=  Vector2.Dot(Vector2.left, (_pos1 - _lightPos).normalized);

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
	private Color GetColorForVertex(int _xWorld, int _yWorld, Vector2 _worldPos, float _distance, out bool _illuminated, out float _lightFromThis){
		_lightFromThis = Intensity * Mathf.Pow(1 - (_distance / lightRadius), 2); dwaodnwa // this is the issue right here! _distance can't be zero!

		Color _cachedColor = lightColorMap[_xWorld, _yWorld];
		Color lightColor = Mouse.Instance.Coloring.AllColors[(int)LightColor];
		Color newColor = Mouse.Instance.Coloring.AllColors[(int)LightColor] * _lightFromThis;

		_illuminated = IsInsideLightMesh(_worldPos);
		if(_illuminated){
			if(_cachedColor.r < lightColor.r)
				_cachedColor.r = Mathf.Min(_cachedColor.r + newColor.r, lightColor.r);
			if(_cachedColor.g < lightColor.g)
				_cachedColor.g = Mathf.Min(_cachedColor.g + newColor.g, lightColor.g);
			if(_cachedColor.b < lightColor.b)
				_cachedColor.b = Mathf.Min(_cachedColor.b + newColor.b, lightColor.b);
		}

		lightColorMap[_xWorld, _yWorld] = _cachedColor;
		return _cachedColor;
    }
	private Vector4 GetShadowCastingLightsIndices(int _xWorld, int _yWorld, Vector2 _worldPos, bool _illuminated, float _lightFromThis){

		lightLevelList[0].Index = lightIndexMap[_xWorld, _yWorld].x;
		lightLevelList[1].Index = lightIndexMap[_xWorld, _yWorld].y;
		lightLevelList[2].Index = lightIndexMap[_xWorld, _yWorld].z;
		lightLevelList[3].Index = lightIndexMap[_xWorld, _yWorld].w;

		lightLevelList[0].Level = lightIntensityMap[_xWorld, _yWorld].x;
		lightLevelList[1].Level = lightIntensityMap[_xWorld, _yWorld].y;
		lightLevelList[2].Level = lightIntensityMap[_xWorld, _yWorld].z;
		lightLevelList[3].Level = lightIntensityMap[_xWorld, _yWorld].w;

		lightLevelList.OrderBy(x => -x.Level); // reverse sort
		for (int i = 0; i < 4; i++){
			float _level = lightLevelList[i].Level;
			if (_illuminated && _lightFromThis >= _level){
				lightLevelList.Insert(i, new LightIndexLevelPairClass(AllLights.FindIndex(x => x == this), _lightFromThis));
				break;
			}
			else if (!_illuminated && _level == 0){ // we still wanna save the index to prevent some shadow-issues around corners
				lightLevelList.Insert(i, new LightIndexLevelPairClass(AllLights.FindIndex(x => x == this), 0));
				break;
			}
		}

		lightIndexMap[_xWorld, _yWorld].x = lightLevelList[0].Index;
		lightIndexMap[_xWorld, _yWorld].y = lightLevelList[1].Index;
		lightIndexMap[_xWorld, _yWorld].z = lightLevelList[2].Index;
		lightIndexMap[_xWorld, _yWorld].w = lightLevelList[3].Index;

		lightIntensityMap[_xWorld, _yWorld].x = lightLevelList[0].Level;
		lightIntensityMap[_xWorld, _yWorld].y = lightLevelList[1].Level;
		lightIntensityMap[_xWorld, _yWorld].z = lightLevelList[2].Level;
		lightIntensityMap[_xWorld, _yWorld].w = lightLevelList[3].Level;

		return lightIndexMap[_xWorld, _yWorld];
	}

    void RenderLightMesh() {
        //-- Step 5: fill the mesh with vertices--//

        Vector3[] initVerticesMeshLight = new Vector3[allVertices.Count + 1];
        initVerticesMeshLight[0] = Vector3.zero;

        for (int i = 0; i < allVertices.Count; i++)
            initVerticesMeshLight[i + 1] = allVertices[i].Pos;

        lightMesh.Clear();
        lightMesh.vertices = initVerticesMeshLight;

        Vector2[] uvs = new Vector2[initVerticesMeshLight.Length];
        for (int i = 0; i < initVerticesMeshLight.Length; i++)
            uvs[i] = new Vector2(initVerticesMeshLight[i].x, initVerticesMeshLight[i].y);

        lightMesh.uv = uvs;

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

        lightMesh.triangles = triangles;
        renderer.sharedMaterial = lightMaterial;
        renderer.material.SetFloat("_UVScale", 1 / lightRadius);
	}

    void ResetBounds() {
        Bounds _bounds = lightMesh.bounds;
        _bounds.center = Vector3.zero;
        lightMesh.bounds = _bounds;
    }

	private const float VERTEX_ON_EDGE_TOLERANCE = 0.01f;
	public Vector2[] PointCollisionArray;
	void UpdatePointCollisionArray(){
		// cache vertices relative to world - but skip zero as it messes with the IsInsideLightMesh-algorithm
		PointCollisionArray = new Vector2[lightMesh.vertexCount - 1];
		for (int i = 0; i < PointCollisionArray.Length; i++){
			Vector3 _dir = (lightMesh.vertices[i + 1] - transform.position).normalized;
			PointCollisionArray[i] = transform.position + lightMesh.vertices[i + 1] + _dir * VERTEX_ON_EDGE_TOLERANCE;
		}
	}
	private bool IsInsideLightMesh(Vector2 _pos){
		bool _inside = false;
		for (int i = 0, i2 = PointCollisionArray.Length - 1; i < PointCollisionArray.Length; i2 = i, i++){
			Vector2 _vx1 = PointCollisionArray[i];
			Vector2 _vx2 = PointCollisionArray[i2];

			bool _isBetweenVertices = _vx1.y <= _pos.y && _pos.y < _vx2.y || _vx2.y <= _pos.y && _pos.y < _vx1.y;
			float _progressY = (_pos.y - _vx1.y) / (_vx2.y - _vx1.y);
			float _progressX = (_vx2.x - _vx1.x) * _progressY;
			bool _isLeftOfEdge = _pos.x < _vx1.x + _progressX;

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
			Tile _t = tilesInRangeWithCollider[x, y];
			if(_t == null)
				continue;
			PolygonCollider2D _coll = ObjectPooler.Instance.GetPooledObject<PolygonCollider2D>(_t.ExactType);
            if (_coll == null)
                continue;

            _coll.transform.position = tilesInRangeWithCollider[x, y].WorldPosition;
            pooledColliders.Enqueue(_coll);
        }
		// for (int y = 0; y < Grid.GridSizeY; y++){
		// 	for (int x = 0; x < Grid.GridSizeX; x++){
		// 		if ((myInspector.MyTileObject.MyTile.WorldPosition - Grid.Instance.grid[x, y].WorldPosition).magnitude > lightRadius)
		// 			continue;
		// 		PolygonCollider2D _coll = ObjectPooler.Instance.GetPooledObject<PolygonCollider2D>(Grid.Instance.grid[x, y].ExactType);
		// 		if (_coll == null)
		// 			continue;

		// 		_coll.transform.position = Grid.Instance.grid[x, y].WorldPosition;
		// 		collidersInRange.Enqueue(_coll);
		// 	}
		// }
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

