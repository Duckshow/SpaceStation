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
    public float lightRadius = 20f;
    [Range(0, 1)] public float Intensity = 1;
    public byte LightColor = 40; // bright yellow
    public LayerMask layer;
    [Range(4, 40)] public int lightSegments = 8;
    public Transform MeshTransform;

    //[HideInInspector] public PolygonCollider2D[] allMeshes; // Array for all of the meshes in our scene
    [HideInInspector]
    public List<Tile> allTiles = new List<Tile>(); // Array for all of the meshes in our scene
    [HideInInspector]
    public List<Verts> allVertices = new List<Verts>(); // Array for all of the vertices in our meshes

    private Mesh lightMesh; // Mesh for our light mesh
    private new MeshRenderer renderer;
    private CanInspect myInspector;


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

    static bool hasRunStartUpdate = false;
    void Start() {
        if(hasRunStartUpdate)
            return;

        hasRunStartUpdate = true;
        Invoke("CallTheFuckingMethod", 0.1f);
    }
    void CallTheFuckingMethod() {
        UpdateAllLights();
    }

    void PostPickUp(){ // TODO: would be good if picked-up objects were visible and jumped between tiles when moving. that way the light can update as it's moved as well.
        UpdateAllLights();
    }
    void PostPutDown() {
        UpdateAllLights();
    }

    private static Material GridMaterial;

    [EasyButtons.Button]
    public void UpdateAllLights() {
        float _timeStarted = Time.realtimeSinceStartup;

        // find material and propertyIDs
        if (GridMaterial == null)
			GridMaterial = Grid.Instance.grid[0, 0].MyUVController.Renderer.sharedMaterial;
		// if (GridMaterial == null)
        //     GridMaterial = Grid.Instance.grid[0, 0].BottomQuad.Renderer.sharedMaterial;

        // get to business
        for (int i = 0; i < AllLights.Count; i++) {
            if (AllLights[i].lightMesh != null)
                AllLights[i].lightMesh.Clear();

            if (AllLights[i].myInspector.CurrentState != CanInspect.State.Default)
                continue;

            AllLights[i].UpdateLight();
        }
        CalculateLightingForGrid();
        Debug.Log("All Lights Updated: " + (Time.realtimeSinceStartup - _timeStarted) + "s");
    }

    void UpdateLight() {
        GetAllMeshes();
        PrepareGridcastColliders();
        SetLight();
        DiscardGridcastColliders();
        RenderLightMesh();
        ResetBounds();
		UpdatePointCollisionArray();
	}

    private Tile t;
    private bool breakLoops = false;
    private List<Tile> tilesInRange = new List<Tile>();
    void GetAllMeshes() {
        tilesInRange.Clear();

        allTiles.Clear();
        for (int y = 0; y < Grid.GridSizeY; y++) {
            for (int x = 0; x < Grid.GridSizeX; x++) {
                t = Grid.Instance.grid[x, y];
                PolygonCollider2D _coll = ObjectPooler.Instance.GetPooledObject<PolygonCollider2D>(t.ExactType);
                if (_coll == null) { 
                    if((t.WorldPosition - (Vector2)transform.position).magnitude < lightRadius)
                        tilesInRange.Add(t);
                    continue;
                }

                _coll.transform.position = t.WorldPosition;

                breakLoops = false;
                for (int pIndex = 0; pIndex < _coll.pathCount; pIndex++){
                    for (int vIndex = 0; vIndex < _coll.GetPath(pIndex).Length; vIndex++){
                        if (((t.WorldPosition + _coll.GetPath(pIndex)[vIndex]) - (Vector2)transform.position).magnitude <= lightRadius) {
                            allTiles.Add(t);
                            tilesInRange.Add(t);
                            breakLoops = true;
                        }

                        if (breakLoops)
                            break;
                    }

                    if(breakLoops) 
                        break;
                }

                _coll.GetComponent<PoolerObject>().ReturnToPool();
            }
        }
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
        for (int i = 0; i < allTiles.Count; i++) {
            tempVerts.Clear();
            
            // the following variables used to fix sorting bug
            // the calculated angles are in mixed quadrants (1 and 4)
            lows = false; // check for minors at -0.5
            highs = false; // check for majors at 2.0

            PolygonCollider2D _coll = ObjectPooler.Instance.GetPooledObject<PolygonCollider2D>(allTiles[i].ExactType);
            _coll.transform.position = allTiles[i].WorldPosition;
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
	private delegate Color mTryGetNeighbourColor(int _x, int _y);
	void CalculateLightingForGrid() {
		Color[,] _cachedColors = new Color[Grid.GridSizeX * 3, Grid.GridSizeY * 3];
		Vector2 _offset = new Vector2();
        for (int x = 0; x < Grid.GridSizeX; x++){
            for (int y = 0; y < Grid.GridSizeY; y++){
                for (int vy = 0; vy < 3; vy++){
                    if(y > 0 && vy == 0)
                        continue;

                    for (int vx = 0; vx < 3; vx++){
                        if(x > 0 && vx == 0)
                            continue;

                        int _vIndex = vy * 3 + vx;
                        int _totalX = x * 3 + vx;
                        int _totalY = y * 3 + vy;

                        // get colors from lights
                        _offset.x = (vx - 1) * VERTEX_DISTANCE;
                        _offset.y = (vy - 1) * VERTEX_DISTANCE;
                        Vector4 _lights; // the 4 most dominant lights
                        Vector2 _tilePos = Grid.Instance.grid[x, y].WorldPosition;
                        Color _finalColor = GetTotalVertexLighting(_tilePos + _offset, out _lights);
                        _finalColor.a = 1;

						// get two dots per light describing angle to respective lights, concatted to one float each
						float _doubleDot0 = _lights.x < 0 ? 0 : GetDoubleDotAngle(_tilePos + _offset, AllLights[(int)_lights.x].transform.position);
                        float _doubleDot1 = _lights.y < 0 ? 0 : GetDoubleDotAngle(_tilePos + _offset, AllLights[(int)_lights.y].transform.position);
                        float _doubleDot2 = _lights.z < 0 ? 0 : GetDoubleDotAngle(_tilePos + _offset, AllLights[(int)_lights.z].transform.position);
                        float _doubleDot3 = _lights.w < 0 ? 0 : GetDoubleDotAngle(_tilePos + _offset, AllLights[(int)_lights.w].transform.position);

						SetDoubleDotForTile(x, y, _vIndex, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
                        _cachedColors[_totalX, _totalY] = _finalColor;

						bool _affectsTopHalf = vy == 2;
						if (_affectsTopHalf){ // top half of *this* tile
							SetDoubleDotForTile(x, y, (vy + 1) * 3 + vx, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
							if (vx == 2)
								SetDoubleDotForTile(x, y, (vy + 2) * 3 + 1, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
						}
						bool _affectsR = x < Grid.GridSizeX - 1 && vx == 2;
                        bool _affectsT = y < Grid.GridSizeY - 1 && _affectsTopHalf;
                        if (_affectsR){
                            SetDoubleDotForTile(x + 1, y, _vIndex - vx, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
                            _cachedColors[_totalX + 1, _totalY] = _finalColor;
							if (_affectsTopHalf) { // top half of right neighbour tile
								SetDoubleDotForTile(x + 1, y, (vy + 1) * 3, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
								SetDoubleDotForTile(x + 1, y, (vy + 2) * 3, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
							}
						}
                        if (_affectsT){
                            SetDoubleDotForTile(x, y + 1, vx, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
                            _cachedColors[_totalX, _totalY + 1] = _finalColor;
						}
                        if (_affectsR && _affectsT){
                            SetDoubleDotForTile(x + 1, y + 1, 0, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
                            _cachedColors[_totalX + 1, _totalY + 1] = _finalColor;
						}
                    }
                }
            }
        }
        Color[] _neighbourColors = new Color[8];
        Color _clear = new Color(0, 0, 0, 0);
        Color _myColor;
        Color _myColorMod = new Color();
        int _maxX = _cachedColors.GetLength(0) - 1;
        int _maxY = _cachedColors.GetLength(1) - 1;
        for (int x = 0; x < Grid.GridSizeX; x++){
            for (int y = 0; y < Grid.GridSizeY; y++){
                for (int vy = 0; vy < 3; vy++){
                    if(y > 0 && vy == 0)
                        continue;

                    for (int vx = 0; vx < 3; vx++){
                        if(x > 0 && vx == 0)
                            continue;

						// try get colors from neighbouring vertices (for smoothing)
                        int _totalX = x * 3 + vx;
                        int _totalY = y * 3 + vy;
						int _diffToRightX = vx == 0 ? 1 : 2;
						int _diffToAboveY = vy == 0 ? 1 : 2;
						int _failAmount = 0;
						mTryGetNeighbourColor TryGetNeighbourColor = delegate (int _x, int _y){
							if (_x < 0 || _x > _maxX || _y < 0 || _y > _maxY){
								_failAmount++;
								return _clear;
							}
							return _cachedColors[_x, _y];
						};
						_neighbourColors[0] = TryGetNeighbourColor(_totalX - 1, _totalY - 1);
						_neighbourColors[1] = TryGetNeighbourColor(_totalX, 	_totalY - 1);
						_neighbourColors[2] = TryGetNeighbourColor(_totalX + _diffToRightX, _totalY - 1);
						_neighbourColors[3] = TryGetNeighbourColor(_totalX - 1, _totalY);
						_neighbourColors[4] = TryGetNeighbourColor(_totalX + _diffToRightX, _totalY);
						_neighbourColors[5] = TryGetNeighbourColor(_totalX - 1, _totalY + _diffToAboveY);
						_neighbourColors[6] = TryGetNeighbourColor(_totalX, 	_totalY + _diffToAboveY);
						_neighbourColors[7] = TryGetNeighbourColor(_totalX + _diffToRightX, _totalY + _diffToAboveY);

						// apply found colors
                        for (int i = 0; i < _neighbourColors.Length; i++)
                            _myColorMod += _neighbourColors[i];
                        _myColorMod /= Mathf.Max(_neighbourColors.Length - _failAmount, 1);
						_myColor = (_cachedColors[_totalX, _totalY] + _myColorMod) * 0.5f;
                        _myColor.a = 1;

						// set color on all vertices at this position
                        int _vIndex = vy * 3 + vx;
                        SetVertexColorForTile(x, y, _vIndex, _myColor);

						bool _affectsTopHalf = vy == 2;
						if (_affectsTopHalf){ // top half of *this* tile
							SetVertexColorForTile(x, y, (vy + 1) * 3 + vx, _myColor);
							if(vx == 2)
								SetVertexColorForTile(x, y, (vy + 2) * 3 + 1, _myColor);
						}

						bool _affectsR = x < Grid.GridSizeX - 1 && vx == 2;
                        bool _affectsT = y < Grid.GridSizeY - 1 && vy == 2;
						if (_affectsR) { 
							SetVertexColorForTile(x + 1, y, _vIndex - vx, _myColor);
							if (_affectsTopHalf) { // top half of right neighbour tile
								SetVertexColorForTile(x + 1, y, (vy + 1) * 3, _myColor);
								SetVertexColorForTile(x + 1, y, (vy + 2) * 3, _myColor);
							}
						}
                        if (_affectsT)
                            SetVertexColorForTile(x, y + 1, vx, _myColor);
                        if (_affectsR && _affectsT)
                            SetVertexColorForTile(x + 1, y + 1, 0, _myColor);
                    }
                }
            }
        }
    }
	void SetDoubleDotForTile(int _gridX, int _gridY, int _vertex, float _doubleDot0, float _doubleDot1, float _doubleDot2, float _doubleDot3){
		Grid.Instance.grid[_gridX, _gridY].MyUVController.SetUVDoubleDot(_vertex, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
		// Grid.Instance.grid[_gridX, _gridY].FloorQuad.       SetUVDoubleDot(_vertex, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
        // Grid.Instance.grid[_gridX, _gridY].FloorCornerHider.SetUVDoubleDot(_vertex, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
        // Grid.Instance.grid[_gridX, _gridY].BottomQuad.      SetUVDoubleDot(_vertex, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
        // Grid.Instance.grid[_gridX, _gridY].TopQuad.         SetUVDoubleDot(_vertex, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
        // Grid.Instance.grid[_gridX, _gridY].WallCornerHider. SetUVDoubleDot(_vertex, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
    }
    void SetVertexColorForTile(int _gridX, int _gridY, int _vertex, Color32 _color){
		Grid.Instance.grid[_gridX, _gridY].MyUVController.SetVertexColor(_vertex, _color);
		// Grid.Instance.grid[_gridX, _gridY].FloorQuad.       SetVertexColor(_vertex, _color);
        // Grid.Instance.grid[_gridX, _gridY].FloorCornerHider.SetVertexColor(_vertex, _color);
        // Grid.Instance.grid[_gridX, _gridY].BottomQuad.      SetVertexColor(_vertex, _color);
        // Grid.Instance.grid[_gridX, _gridY].TopQuad.         SetVertexColor(_vertex, _color);
        // Grid.Instance.grid[_gridX, _gridY].WallCornerHider. SetVertexColor(_vertex, _color);
    }

    static float GetAngle01(Vector2 _pos1, Vector2 _pos2, Vector2 _referenceAngle, int maxAngle) { // TODO: replace with GetAngleClockwise!
        return maxAngle * (0.5f * (1 + Vector2.Dot(
                                            _referenceAngle, 
                                            Vector3.Normalize(_pos1 - _pos2))));
    }
	private const float DOUBLE_DOT_MAX_ANGLE = 1;
	static int GetDoubleDotAngle(Vector2 _pos1, Vector2 _pos2){ // Find an angle (0->1) and convert to two dot-products concatted into one number (eg 0.25 + 0.5 = 255) 
		// get an angle between 0->1. The angle goes all the way around, but counter-clockwise, so sorta like a clock and unlike a dot
		float _vertical = (0.5f * (DOUBLE_DOT_MAX_ANGLE + Vector2.Dot(Vector2.down, Vector3.Normalize(_pos1 - _pos2))));
		float _horizontal = Vector2.Dot(Vector2.left, Vector3.Normalize(_pos1 - _pos2));

		_vertical = (_vertical * (DOUBLE_DOT_MAX_ANGLE * 0.5f));
		if (_horizontal < 0)
			_vertical = Mathf.Abs(_vertical - 1);

		// convert angle to two dot products
        _horizontal = _vertical + 0.25f;
        _horizontal -= Mathf.Floor(_horizontal);

         // store dots (0->1) as ints (0->1000)
        int _dotX = Mathf.RoundToInt(Mathf.Max(0.001f, GetDotifiedAngle(_horizontal)) * 1000);
        int _dotY = Mathf.RoundToInt(Mathf.Max(0.001f, GetDotifiedAngle(_vertical)) * 1000); 

        // merge ints into one
		return _dotX | (_dotY << 16);
    }
    static float GetDotifiedAngle(float _angle){ // Take an angle (between 0, 1) and convert to something like 0->1->0
        _angle *= 2;
        float _floored = Mathf.Floor(_angle);
        return Mathf.Abs(_floored - (_angle - _floored));
    }

	Color GetTotalVertexLighting(Vector2 _worldPos, out Vector4 _highestLightLevelIndices){
		Color _totalColor = new Color();
		Vector4 _highestLightLevels = new Vector4();
		_highestLightLevelIndices = new Vector4(-1, -1, -1, -1);

		float _newDistance;
		float _newLightLevel;
		bool _illuminated;
		
        for (int i = 0; i < AllLights.Count; i++) {
            _newDistance = Vector2.Distance(_worldPos, AllLights[i].transform.position);
            if (_newDistance > AllLights[i].lightRadius)
                continue;

			_illuminated = IsInsideLightMesh(_worldPos, AllLights[i]);
            _newLightLevel = AllLights[i].Intensity * Mathf.Pow(1 - (_newDistance / AllLights[i].lightRadius), 2);

			// the four strongest lights will be cached and used for rendering shadows
			TryAddToLightsCastingShadows(i, _newLightLevel, _illuminated, ref _highestLightLevels, ref _highestLightLevelIndices);

			// but all lights will affect the luminosity and color of the vertex (if hit, that is)
			if(_illuminated)
				_totalColor += Mouse.Instance.Coloring.AllColors[(int)AllLights[i].LightColor] * _newLightLevel;
		}

        return _totalColor;
    }
	
	List<LightIndexLevelPairClass> lightLevelList = new List<LightIndexLevelPairClass>(4);
	class LightIndexLevelPairClass{
		public float Index;
		public float Level;
		public LightIndexLevelPairClass(float i, float l) { Index = i; Level = l; }
		public void Set(float i, float l) { Index = i; Level = l; }
	}
	void TryAddToLightsCastingShadows(int _lightIndex, float _newLightLevel, bool _illuminated, ref Vector4 _highestLightLevels, ref Vector4 _highestLightLevelIndices) { 
		if (lightLevelList.Count != 4){
			lightLevelList.Clear();
			lightLevelList.Add(new LightIndexLevelPairClass(-1, 0));
			lightLevelList.Add(new LightIndexLevelPairClass(-1, 0));
			lightLevelList.Add(new LightIndexLevelPairClass(-1, 0));
			lightLevelList.Add(new LightIndexLevelPairClass(-1, 0));
		}

		// cache and reverse-sort previously cached lights
		lightLevelList[0].Set(_highestLightLevelIndices.x, _highestLightLevels.x);
		lightLevelList[1].Set(_highestLightLevelIndices.y, _highestLightLevels.y);
		lightLevelList[2].Set(_highestLightLevelIndices.z, _highestLightLevels.z);
		lightLevelList[3].Set(_highestLightLevelIndices.w, _highestLightLevels.w);
		lightLevelList.OrderBy(x => -x.Level); // reverse sort
		for (int i = 0; i < lightLevelList.Count; i++){
			if (_illuminated && _newLightLevel >= lightLevelList[i].Level){
				lightLevelList.Insert(i, new LightIndexLevelPairClass(_lightIndex, _newLightLevel));
				break;
			}
			else if (!_illuminated && lightLevelList[i].Level == 0){ // we still wanna save the index to prevent some shadow-issues around corners
				lightLevelList.Insert(i, new LightIndexLevelPairClass(_lightIndex, 0));
				break;
			}
		}

		// re-add lights to proper variable
		_highestLightLevelIndices.x = lightLevelList[0].Index;
		_highestLightLevelIndices.y = lightLevelList[1].Index;
		_highestLightLevelIndices.z = lightLevelList[2].Index;
		_highestLightLevelIndices.w = lightLevelList[3].Index;
		_highestLightLevels.x = lightLevelList[0].Level;
		_highestLightLevels.y = lightLevelList[1].Level;
		_highestLightLevels.z = lightLevelList[2].Level;
		_highestLightLevels.w = lightLevelList[3].Level;
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

	private const float VERTEX_ON_EDGE_TOLERANCE = 0.05f;
	public Vector2[] PointCollisionArray;
	void UpdatePointCollisionArray(){
		// cache vertices relative to world - but skip zero as it messes with the IsInsideLightMesh-algorithm
		PointCollisionArray = new Vector2[lightMesh.vertexCount - 1];
		for (int i = 0; i < PointCollisionArray.Length; i++){
			Vector3 _dir = (lightMesh.vertices[i + 1] - transform.position).normalized;
			PointCollisionArray[i] = transform.position + lightMesh.vertices[i + 1] + _dir * VERTEX_ON_EDGE_TOLERANCE;
		}
	}
	bool IsInsideLightMesh(Vector2 _pos, CustomLight _light){
		bool _inside = false;
		for (int i = 0, i2 = _light.PointCollisionArray.Length - 1; i < _light.PointCollisionArray.Length; i2 = i, i++){
			Vector3 _vx1 = _light.PointCollisionArray[i];
			Vector3 _vx2 = _light.PointCollisionArray[i2];

			float _minY = Mathf.Min(_vx1.y, _vx2.y);
			float _maxY = Mathf.Max(_vx1.y, _vx2.y);
			bool _isBetweenVertices = _minY <= _pos.y && _pos.y < _maxY;

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

    Queue<PolygonCollider2D> collidersForGridcast = new Queue<PolygonCollider2D>();
    void PrepareGridcastColliders(){
        for (int y = 0; y < Grid.GridSizeY; y++){
            for (int x = 0; x < Grid.GridSizeX; x++){
                if((myInspector.MyTileObject.MyTile.GridCoord - Grid.Instance.grid[x, y].WorldPosition).magnitude > lightRadius)
                    continue;
                PolygonCollider2D _coll = ObjectPooler.Instance.GetPooledObject<PolygonCollider2D>(Grid.Instance.grid[x, y].ExactType);
                if(_coll == null)
                    continue;

                _coll.transform.position = Grid.Instance.grid[x, y].WorldPosition;                
                collidersForGridcast.Enqueue(_coll);
            }
        }
    }
    private const float GRIDCAST_TOLERANCE = 0.01f;
    bool Gridcast(Vector2 _start, Vector2 _end, out RaycastHit2D _rayhit){
        Debug.Log("pew");
        _rayhit = Physics2D.Linecast(_start, _end);
        return _rayhit.collider != null && (_end - _rayhit.point).magnitude > 0.01f;
    }
    void DiscardGridcastColliders(){
		for (int i = 0; i < collidersForGridcast.Count; i++)
            collidersForGridcast.Dequeue().GetComponent<PoolerObject>().ReturnToPool();
    }
}

