/****************************************************************************
Copyright (c) 2014 Martin Ysa

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:
 
The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.
 
THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
****************************************************************************/

using UnityEngine;
using System.Collections.Generic;

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
    public int Intensity = 1;
    public byte LightColor = 40; // bright yellow
    public LayerMask layer;
    [Range(4, 40)]
    public int lightSegments = 8;
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

        //if(myInspector.CurrentState == CanInspect.State.Default)
        //    UpdateLight();
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
        //if(lightMesh != null)
        //    lightMesh.Clear();

        //isUpToDate = true;
        UpdateAllLights();
    }
    void PostPutDown() {
        //isUpToDate = false;
        UpdateAllLights();
    }

    //private static Texture2D TextureLightAngles;
    // private static Texture2D TextureLightDotXs;
    // private static Texture2D TextureLightDotYs;

    // private static Color32[] ReportedDotXs;
    // private static Color32[] ReportedDotYs;

    // private static int shaderPropertyDotX;
    // private static int shaderPropertyDotY;
    private static Material GridMaterial;

    //private static bool[,] sGridColliderArray;

    [EasyButtons.Button]
    public void UpdateAllLights() {
        float _timeStarted = Time.realtimeSinceStartup;

        // if(sGridColliderArray == null)
        //     sGridColliderArray = new bool[Grid.GridSizeX, Grid.GridSizeY];
        // for (int x = 0; x < Grid.GridSizeX; x++){
        //     for (int y = 0; y < Grid.GridSizeY; y++){
        //         sGridColliderArray[x, y] = CachedAssets.Instance.WallSets[0].HasShadowCollider(Grid.Instance.grid[x, y].ExactType);
        //     }
        // }

        // create textures and arrays
        // if (TextureLightDotXs == null) {
        //     TextureLightDotXs = new Texture2D(Grid.GridSizeX, Grid.GridSizeY, TextureFormat.RGBA32, false);
        //     TextureLightDotXs.filterMode = FilterMode.Point;
        // }
        // if (TextureLightDotYs == null) {
        //     TextureLightDotYs = new Texture2D(Grid.GridSizeX, Grid.GridSizeY, TextureFormat.RGBA32, false);
        //     TextureLightDotYs.filterMode = FilterMode.Point;
        // }

        // find material and propertyIDs
        if (GridMaterial == null)
            GridMaterial = Grid.Instance.grid[0, 0].BottomQuad.Renderer.sharedMaterial;
        // if (shaderPropertyDotX == 0)
        //     shaderPropertyDotX = Shader.PropertyToID("_DotXs");
        // if (shaderPropertyDotY == 0)
        //     shaderPropertyDotY = Shader.PropertyToID("_DotYs");

        // get to business
        for (int i = 0; i < AllLights.Count; i++) {
            if (AllLights[i].lightMesh != null)
                AllLights[i].lightMesh.Clear();

            if (AllLights[i].myInspector.CurrentState != CanInspect.State.Default)
                continue;

            AllLights[i].UpdateLight();
        }
        CalculateLightingForGrid();

        // apply arrays to textures
        // TextureLightDotXs.SetPixels32(ReportedDotXs);
        // TextureLightDotYs.SetPixels32(ReportedDotYs);
        // TextureLightDotXs.Apply();
        // TextureLightDotYs.Apply();


        // apply textures to shader
        // GridMaterial.SetTexture(shaderPropertyDotX, TextureLightDotXs);
        // GridMaterial.SetTexture(shaderPropertyDotY, TextureLightDotYs);

        Debug.Log("All Lights Updated: " + (Time.realtimeSinceStartup - _timeStarted) + "s");
    }

    void UpdateLight() { // shouldn't call this by itself. must update ALL lights.
        GetAllMeshes();
        SetLight();
        RenderLightMesh();
        ResetBounds();
        //myDefaultLight.range = lightRadius;// * 4;
        //isUpToDate = true;
    }

    private Tile t;
    private bool breakLoops = false;
    private List<Tile> tilesInRange = new List<Tile>();
    void GetAllMeshes() {
        tilesInRange.Clear();
        //-- Step 1: obtain all active meshes in the scene --//
        //---------------------------------------------------------------------//

        //Collider2D [] allColl2D = Physics2D.OverlapCircleAll(transform.position, lightRadius, layer);
        //allMeshes = new PolygonCollider2D[allColl2D.Length];
        //for (int i=0; i<allColl2D.Length; i++)
        //	allMeshes[i] = (PolygonCollider2D)allColl2D[i];

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
    void SetLight() {
        allVertices.Clear();// Since these lists are populated every frame, clear them first to prevent overpopulation

        //--Step 2: Obtain vertices for each mesh --//
        //---------------------------------------------------------------------//

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
                    v.Angle = GetVectorAngle(true, v.Pos.x, v.Pos.y); //--Calculate angle

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
                        vL.Angle = GetVectorAngle(true, vL.Pos.x, vL.Pos.y);
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
            v.Angle = GetVectorAngle(true, v.Pos.x, v.Pos.y);
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

    //private static Vector2 CORNER_OFFSET = new Vector2(-0.5f, -0.5f);
    //private static Vector2 CORNER_OFFSET_OUTSIDE_GRID = new Vector2(0.5f, -0.5f);
    private const float VERTEX_DISTANCE = 0.5f;
    void CalculateLightingForGrid() {
        // if (ReportedDotXs == null)
        //     ReportedDotXs = new Color32[Grid.GridSizeX * Grid.GridSizeY];
        // if(ReportedDotYs == null)
        //     ReportedDotYs = new Color32[Grid.GridSizeX * Grid.GridSizeY];

        Vector2 _offset = new Vector2();
        for (int x = 0; x < Grid.GridSizeX; x++){
            for (int y = 0; y < Grid.GridSizeY; y++){
                
                for (int vx = 0; vx < 3; vx++){
                    if(x > 0 && vx == 0)
                        continue;

                    for (int vy = 0; vy < 3; vy++){
                        if(y > 0 && vy == 0)
                            continue;

                        _offset.x = (vx - 1) * VERTEX_DISTANCE;
                        _offset.y = (vy - 1) * VERTEX_DISTANCE;

                        // get colors from lights
                        Vector4 _lights; // the 4 most dominant lights
                        Vector2 _tilePos = Grid.Instance.grid[x, y].WorldPosition;
                        Color32 _finalColor = GetTotalVertexLighting(_tilePos + _offset, out _lights);
                        _finalColor.a = 255;

                        // get two dots per light describing angle to respective lights, concatted to one float each
                        float _doubleDot0 = ConvertAngle01ToConcatDot(GetAngleCCW(_tilePos + _offset, AllLights[(int)_lights.x].transform.position, 1));
                        float _doubleDot1 = ConvertAngle01ToConcatDot(GetAngleCCW(_tilePos + _offset, AllLights[(int)_lights.y].transform.position, 1));
                        float _doubleDot2 = ConvertAngle01ToConcatDot(GetAngleCCW(_tilePos + _offset, AllLights[(int)_lights.z].transform.position, 1));
                        float _doubleDot3 = ConvertAngle01ToConcatDot(GetAngleCCW(_tilePos + _offset, AllLights[(int)_lights.w].transform.position, 1));

                        // set colors
                        int _vIndex = vy * 3 + vx;
                        Grid.Instance.grid[x, y].FloorQuad.                         SetVertexColor(_vIndex, _finalColor);
                        Grid.Instance.grid[x, y].FloorCornerHider.                  SetVertexColor(_vIndex, _finalColor);
                        Grid.Instance.grid[x, y].BottomQuad.                        SetVertexColor(_vIndex, _finalColor);
                        Grid.Instance.grid[x, y].TopQuad.                           SetVertexColor(_vIndex, _finalColor);
                        Grid.Instance.grid[x, y].WallCornerHider.                   SetVertexColor(_vIndex, _finalColor);

                        // set angles
                        Grid.Instance.grid[x, y].FloorQuad.                         SetUVDoubleDot(_vIndex, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
                        Grid.Instance.grid[x, y].FloorCornerHider.                  SetUVDoubleDot(_vIndex, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
                        Grid.Instance.grid[x, y].BottomQuad.                        SetUVDoubleDot(_vIndex, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
                        Grid.Instance.grid[x, y].TopQuad.                           SetUVDoubleDot(_vIndex, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
                        Grid.Instance.grid[x, y].WallCornerHider.                   SetUVDoubleDot(_vIndex, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);

                        //SuperDebug.Log(_tilePos + _offset, Color.red, _angle0.ToString());

                        bool _affectsR = x < Grid.GridSizeX - 1 && vx == 2;
                        bool _affectsT = y < Grid.GridSizeY - 1 && vy == 2;
                        if (_affectsR){
                            int xR = x + 1;
                            int _vIndexR = vy * 3;
                            Grid.Instance.grid[xR, y].FloorQuad.                    SetVertexColor(_vIndexR, _finalColor);
                            Grid.Instance.grid[xR, y].FloorCornerHider.             SetVertexColor(_vIndexR, _finalColor);
                            Grid.Instance.grid[xR, y].BottomQuad.                   SetVertexColor(_vIndexR, _finalColor);
                            Grid.Instance.grid[xR, y].TopQuad.                      SetVertexColor(_vIndexR, _finalColor);
                            Grid.Instance.grid[xR, y].WallCornerHider.              SetVertexColor(_vIndexR, _finalColor);

                            Grid.Instance.grid[xR, y].FloorQuad.                    SetUVDoubleDot(_vIndexR, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
                            Grid.Instance.grid[xR, y].FloorCornerHider.             SetUVDoubleDot(_vIndexR, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
                            Grid.Instance.grid[xR, y].BottomQuad.                   SetUVDoubleDot(_vIndexR, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
                            Grid.Instance.grid[xR, y].TopQuad.                      SetUVDoubleDot(_vIndexR, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
                            Grid.Instance.grid[xR, y].WallCornerHider.              SetUVDoubleDot(_vIndexR, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
                        }
                        if (_affectsT){
                            int yT = y + 1;
                            Grid.Instance.grid[x, yT].FloorQuad.                    SetVertexColor(vx, _finalColor);
                            Grid.Instance.grid[x, yT].FloorCornerHider.             SetVertexColor(vx, _finalColor);
                            Grid.Instance.grid[x, yT].BottomQuad.                   SetVertexColor(vx, _finalColor);
                            Grid.Instance.grid[x, yT].TopQuad.                      SetVertexColor(vx, _finalColor);
                            Grid.Instance.grid[x, yT].WallCornerHider.              SetVertexColor(vx, _finalColor);

                            Grid.Instance.grid[x, yT].FloorQuad.                    SetUVDoubleDot(vx, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
                            Grid.Instance.grid[x, yT].FloorCornerHider.             SetUVDoubleDot(vx, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
                            Grid.Instance.grid[x, yT].BottomQuad.                   SetUVDoubleDot(vx, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
                            Grid.Instance.grid[x, yT].TopQuad.                      SetUVDoubleDot(vx, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
                            Grid.Instance.grid[x, yT].WallCornerHider.              SetUVDoubleDot(vx, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
                        }
                        if (_affectsR && _affectsT){
                            int xTR = x + 1;
                            int yTR = y + 1;
                            Grid.Instance.grid[xTR, yTR].FloorQuad.                 SetVertexColor(0, _finalColor);
                            Grid.Instance.grid[xTR, yTR].FloorCornerHider.          SetVertexColor(0, _finalColor);
                            Grid.Instance.grid[xTR, yTR].BottomQuad.                SetVertexColor(0, _finalColor);
                            Grid.Instance.grid[xTR, yTR].TopQuad.                   SetVertexColor(0, _finalColor);
                            Grid.Instance.grid[xTR, yTR].WallCornerHider.           SetVertexColor(0, _finalColor);

                            Grid.Instance.grid[xTR, yTR].FloorQuad.                 SetUVDoubleDot(0, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
                            Grid.Instance.grid[xTR, yTR].FloorCornerHider.          SetUVDoubleDot(0, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
                            Grid.Instance.grid[xTR, yTR].BottomQuad.                SetUVDoubleDot(0, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
                            Grid.Instance.grid[xTR, yTR].TopQuad.                   SetUVDoubleDot(0, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
                            Grid.Instance.grid[xTR, yTR].WallCornerHider.           SetUVDoubleDot(0, _doubleDot0, _doubleDot1, _doubleDot2, _doubleDot3);
                        }
                    }
                }

                

                // TODO: try to make Dot part of vertex-stuff
                // // use the total vertexlighting to calculate dot x and y (per tile, NOT per corner)
                // if (x < Grid.GridSizeX && y < Grid.GridSizeY){
                //     // dot X
                //     ReportedDotXs[(y * Grid.GridSizeX) + x].r = (byte)GetAngle01(Grid.Instance.grid[x, y].WorldPosition, (Vector2)AllLights[(int)_lights.x].transform.position, Vector2.left, 255);
                //     ReportedDotXs[(y * Grid.GridSizeX) + x].g = (byte)GetAngle01(Grid.Instance.grid[x, y].WorldPosition, (Vector2)AllLights[(int)_lights.y].transform.position, Vector2.left, 255);
                //     ReportedDotXs[(y * Grid.GridSizeX) + x].b = (byte)GetAngle01(Grid.Instance.grid[x, y].WorldPosition, (Vector2)AllLights[(int)_lights.z].transform.position, Vector2.left, 255);
                //     ReportedDotXs[(y * Grid.GridSizeX) + x].a = (byte)GetAngle01(Grid.Instance.grid[x, y].WorldPosition, (Vector2)AllLights[(int)_lights.w].transform.position, Vector2.left, 255);

                //     // dot Y
                //     ReportedDotYs[(y * Grid.GridSizeX) + x].r = (byte)GetAngle01(Grid.Instance.grid[x, y].WorldPosition, (Vector2)AllLights[(int)_lights.x].transform.position, Vector2.down, 255);
                //     ReportedDotYs[(y * Grid.GridSizeX) + x].g = (byte)GetAngle01(Grid.Instance.grid[x, y].WorldPosition, (Vector2)AllLights[(int)_lights.y].transform.position, Vector2.down, 255);
                //     ReportedDotYs[(y * Grid.GridSizeX) + x].b = (byte)GetAngle01(Grid.Instance.grid[x, y].WorldPosition, (Vector2)AllLights[(int)_lights.z].transform.position, Vector2.down, 255);
                //     ReportedDotYs[(y * Grid.GridSizeX) + x].a = (byte)GetAngle01(Grid.Instance.grid[x, y].WorldPosition, (Vector2)AllLights[(int)_lights.w].transform.position, Vector2.down, 255);
                // }
            }
        }
    }
    static float GetAngle01(Vector2 _pos1, Vector2 _pos2, Vector2 _referenceAngle, int maxAngle) { // TODO: replace with GetAngleClockwise!
        return maxAngle * (0.5f * (1 + Vector2.Dot(
                                            _referenceAngle, 
                                            Vector3.Normalize(_pos1 - _pos2))));
    }
    static float GetAngleCCW(Vector2 _pos1, Vector2 _pos2, int _maxAngle){ // CCW as in counter-clockwise. currently starts pointing down, like an upside-down clock
        // gets the angle between _pos1 and _pos2, ranging from 0 to _maxAngle.
        // the angle goes all-the-way-around, like a clock and unlike a dot-product.

        float _vertical     = (0.5f * (1 + Vector2.Dot(Vector2.down,   Vector3.Normalize(_pos1 - _pos2))));
        float _horizontal   =              Vector2.Dot(Vector2.left, Vector3.Normalize(_pos1 - _pos2));

        _vertical = (_vertical * (_maxAngle * 0.5f));
        if (_horizontal < 0)
            _vertical = Mathf.Abs(_vertical - _maxAngle);

        return _vertical;
    }
    static float ConvertAngle01ToConcatDot(float _angle){ // Take an angle (between 0, 1) and convert to two dot-products concatted into one number (0.25 + 0.5 = 25.5) 
        float _relX = _angle + 0.25f;
        _relX -= Mathf.Floor(_relX);

        float _val = (GetDotifiedAngle(_relX) * 1000) + GetDotifiedAngle(_angle);

        // float _x = Mathf.Floor(_val) * 0.001f;
        // float _y = _val - Mathf.Floor(_val);
        // Debug.Log(_x + ", " + _y);
        return _val;
    }
    static float GetDotifiedAngle(float _angle){ // Take an angle (between 0, 1) and convert to something like 0->1->0

    // float = 13.37f;
    // dotX = floor( 13.37f ) * 0.01f;
    // dotY = ( 13.37f - floor( 13.37f ));
    // dotX = 0.13f, dotY = 0.37f! 

        _angle *= 2;
        float _floored = Mathf.Floor(_angle);
        return Mathf.Abs(_floored - (_angle - _floored));
    }

    Color32 GetTotalVertexLighting(Vector2 _pos, out Vector4 _dominantLightIndices){

        // four because we can only store Dot-info for four lights (mostly bc performance) and then any higher number makes little sense
        _dominantLightIndices = new Vector4();
        Vector4 dominantLightLevels = new Vector4();
        Vector4 dominantLightColors = new Vector4(); // todo: make a QuadrupleInt
        for (int i = 0; i < AllLights.Count; i++) { // optimization: can I lower the amount of lights I'm iterating over? Maybe by using a tree?
            float newRange = AllLights[i].lightRadius;
            float newDistance = Mathf.Min(Mathf.RoundToInt(Vector2.Distance(_pos, AllLights[i].transform.position)), 255);

            if (newDistance > newRange)
                continue;

            //DoubleInt _tilePos = new DoubleInt(Mathf.RoundToInt(_pos.x + Grid.GridSizeXHalf), Mathf.RoundToInt(_pos.y + Grid.GridSizeYHalf));
            // if(_tilePos.X - AllLights[i].myInspector.MyTileObject.MyTile.GridCoord.X <= 0)
            //     _tilePos.X -= 1;
            // if (_tilePos.Y - AllLights[i].myInspector.MyTileObject.MyTile.GridCoord.Y <= 0)
            //     _tilePos.Y -= 1;
            RaycastHit2D _rayHit;
            if(Gridcast(AllLights[i].transform.position, _pos, out _rayHit))
                continue;

            float newIntensity = AllLights[i].Intensity * 255;
            float newLightLevel = newIntensity * (1 - (newDistance / newRange));

            if(newLightLevel > dominantLightLevels.x){
                _dominantLightIndices.x = i;
                dominantLightLevels.x = newLightLevel;
                dominantLightColors.x = AllLights[i].LightColor;
            }
            else if (newLightLevel > dominantLightLevels.y){
                _dominantLightIndices.y = i;
                dominantLightLevels.y = newLightLevel;
                dominantLightColors.y = AllLights[i].LightColor;
            }
            else if (newLightLevel > dominantLightLevels.z){
                _dominantLightIndices.z = i;
                dominantLightLevels.z = newLightLevel;
                dominantLightColors.z = AllLights[i].LightColor;
            }
            else if (newLightLevel > dominantLightLevels.w){
                _dominantLightIndices.w = i;
                dominantLightLevels.w = newLightLevel;
                dominantLightColors.w = AllLights[i].LightColor;
            }
        }

        Color color1 = Mouse.Instance.Coloring.AllColors[(int)dominantLightColors.x] * dominantLightLevels.x;
        Color color2 = Mouse.Instance.Coloring.AllColors[(int)dominantLightColors.y] * dominantLightLevels.y;
        Color color3 = Mouse.Instance.Coloring.AllColors[(int)dominantLightColors.z] * dominantLightLevels.z;
        Color color4 = Mouse.Instance.Coloring.AllColors[(int)dominantLightColors.w] * dominantLightLevels.w;

        Color32 finalColor = new Color32();
        finalColor += color1 * 255;
        finalColor += color2 * 255;
        finalColor += color3 * 255;
        finalColor += color4 * 255;

        return finalColor;
    }

    private Vector3[] initVerticesMeshLight;
    private Vector2[] uvs;
    private int index;
    private int[] triangles;
    void RenderLightMesh() {
        //-- Step 5: fill the mesh with vertices--//
        //---------------------------------------------------------------------//

        initVerticesMeshLight = new Vector3[allVertices.Count + 1];
        initVerticesMeshLight[0] = Vector3.zero;

        for (int i = 0; i < allVertices.Count; i++)
            initVerticesMeshLight[i + 1] = allVertices[i].Pos;

        lightMesh.Clear();
        lightMesh.vertices = initVerticesMeshLight;

        uvs = new Vector2[initVerticesMeshLight.Length];
        for (int i = 0; i < initVerticesMeshLight.Length; i++)
            uvs[i] = new Vector2(initVerticesMeshLight[i].x, initVerticesMeshLight[i].y);

        lightMesh.uv = uvs;

        // triangles
        index = 0;
        triangles = new int[(allVertices.Count * 3)];
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
        //lightMesh.RecalculateNormals();
        renderer.sharedMaterial = lightMaterial;
        renderer.material.SetFloat("_UVScale", 1 / lightRadius);
    }
    private Bounds bounds;
    void ResetBounds() {
        bounds = lightMesh.bounds;
        bounds.center = Vector3.zero;
        lightMesh.bounds = bounds;
    }
    void SortList(List<Verts> list) {
        list.Sort((item1, item2) => (item2.Angle.CompareTo(item1.Angle)));
        //list.Sort(new VertCompare());
    }
    public class VertCompare : IComparer<Verts>{
        private float previousMagnitude;
        private float prevMag;

        public int Compare(Verts v1, Verts v2){
            prevMag = previousMagnitude;
            previousMagnitude = v1.Pos.magnitude;

            if (v1.Angle == v2.Angle)
                return 1;

            return (int)Mathf.Sign(v1.Angle - v2.Angle);
        }
    }

    void DrawLinePerVertex() {
        for (int i = 0; i < allVertices.Count; i++) {
            if (i < (allVertices.Count - 1))
                Debug.DrawLine(allVertices[i].Pos, allVertices[i + 1].Pos, new Color(i * 0.02f, i * 0.02f, i * 0.02f));
            else
                Debug.DrawLine(allVertices[i].Pos, allVertices[0].Pos, new Color(i * 0.02f, i * 0.02f, i * 0.02f));
        }
    }

    private float angle;
    float GetVectorAngle(bool pseudo, float x, float y) {
        angle = 0;
        if (pseudo == true)
            angle = PseudoAngle(x, y);
        else
            angle = Mathf.Atan2(y, x);

        return angle;
    }
    float PseudoAngle(float dx, float dy) {
        // Height performance for calculate angle on a vector (only for sort)
        // APROXIMATE VALUES -- NOT EXACT!! //
        float ax = Mathf.Abs(dx);
        float ay = Mathf.Abs(dy);
        float p = dy / (ax + ay);
        if (dx < 0)
            p = 2 - p;

        return p;
    }

    // private PolygonCollider2D sShadowCollider;
    // private PolygonCollider2D[] sExtraShadowColliders;
    private const float GRIDCAST_TOLERANCE = 0.01f;
    bool Gridcast(Vector2 _start, Vector2 _end, out RaycastHit2D _rayhit){
        float _length = (_start - _end).magnitude;

        // find relevant colliders
        List<PolygonCollider2D> _collidersUsed = new List<PolygonCollider2D>();
        for (int y = 0; y < Grid.GridSizeY; y++){
            for (int x = 0; x < Grid.GridSizeX; x++){
                if((_start - Grid.Instance.grid[x, y].WorldPosition).magnitude > _length)
                    continue;
                PolygonCollider2D _coll = ObjectPooler.Instance.GetPooledObject<PolygonCollider2D>(Grid.Instance.grid[x, y].ExactType);
                if(_coll == null)
                    continue;

                _coll.transform.position = Grid.Instance.grid[x, y].WorldPosition;                
                _collidersUsed.Add(_coll);
            }
        }

        // pew
        _rayhit = Physics2D.Linecast(_start, _end);

        // clean-up
        for (int i = 0; i < _collidersUsed.Count; i++)
            _collidersUsed[i].GetComponent<PoolerObject>().ReturnToPool();

        return _rayhit.collider != null && (_end - _rayhit.point).magnitude > 0.01f;
    }

    // bool Gridcast(Vector2 _start, Vector2 _end, bool debug, out Vector2 _rayhit) { // TODO: replace with raycast

    //     // calculate indices to iterate over
    //     List<BresenhamsLine.OverlapWithTiles> _cast = BresenhamsLine.Gridcast(_start, _end);

    //     // get colliders at start position
    //     if (_cast.Count > 0) {
            
    //         CachedAssets.Instance.WallSets[0].GetShadowCollider(_cast[0].Tile.ExactType, out sShadowCollider);
    //         sShadowCollider.transform.position = _cast[0].Tile.WorldPosition;

    //         if (_cast[0].ExtraTiles != null){
    //             sExtraShadowColliders = new PolygonCollider2D[_cast[0].ExtraTiles.Length];
    //             for (int i = 0; i < _cast[0].ExtraTiles.Length; i++){
    //                 CachedAssets.Instance.WallSets[0].GetShadowCollider(_cast[0].ExtraTiles[i].ExactType, out sExtraShadowColliders[i]);
    //                 sExtraShadowColliders[i].transform.position = _cast[0].Tile.WorldPosition;
    //             }
    //         }
    //     }

    //    // use approximate pixel positions to iterate through cast and test each pixel for shadowcollider
    //     int _currentIndex = 0;
    //     int _iterationsInCast = 0;
    //     int _goal = Mathf.RoundToInt(((_end - _start).magnitude + 1) * Tile.RESOLUTION); // the distance in amount of tiles, multiplied by the amount of pixels across a tile
    //     Vector2 _curPos = _start;
    //     while (_iterationsInCast <= _goal || _currentIndex < _cast.Count) {
    //         _curPos = Vector2.Lerp(_start, _end, (float)_iterationsInCast / (float)_goal);
    //         _iterationsInCast++;

    //         // if pixel is closer to next collider, set next collider as current
    //         if (_currentIndex < _cast.Count - 1 && (_cast[_currentIndex + 1].Tile.WorldPosition - _curPos).magnitude <= (_cast[_currentIndex].Tile.WorldPosition - _curPos).magnitude) {
    //             _currentIndex++;

    //             CachedAssets.Instance.WallSets[0].GetShadowCollider(_cast[_currentIndex].Tile.ExactType, out sShadowCollider);
    //             sShadowCollider.transform.position = _cast[_currentIndex].Tile.WorldPosition;

    //             // get extra colliders (happens if pixel is (more-or-less) in a corner)
    //             if (_cast[_currentIndex].ExtraTiles != null){
    //                 sExtraShadowColliders = new PolygonCollider2D[_cast[_currentIndex].ExtraTiles.Length];
    //                 for (int i = 0; i < _cast[_currentIndex].ExtraTiles.Length; i++){
    //                     CachedAssets.Instance.WallSets[0].GetShadowCollider(_cast[_currentIndex].ExtraTiles[i].ExactType, out sExtraShadowColliders[i]);
    //                     sExtraShadowColliders[i].transform.position = _cast[_currentIndex].ExtraTiles[i].WorldPosition;
    //                 }
    //             }
    //         }

    //         // check for hit
    //         float closest = 1000;
    //         if (sShadowCollider != null && sShadowCollider.OverlapPoint(_curPos)) {
    //             _rayhit = _curPos;
    //             hits.Add(_curPos);
    //             return true;
    //         }
    //         // check for hit if in corner
    //         if (sExtraShadowColliders != null){
    //             for (int i = 0; i < sExtraShadowColliders.Length; i++){
    //                 if (sExtraShadowColliders[i] != null && sExtraShadowColliders[i].OverlapPoint(_curPos)){
    //                     _rayhit = _curPos;
    //                     hits.Add(_curPos);
    //                     return true;
    //                 }
    //             }
    //         }

    //         // done, so no hit
    //         if ((float)_iterationsInCast / (float)_goal > 1) {
    //             _rayhit = Vector2.zero;
    //             nonHits.Add(_curPos);
    //             nonHitsDistance.Add(closest);
    //             nonHitsHadCollider.Add(sShadowCollider != null && sExtraShadowColliders != null && sExtraShadowColliders.Length > 0);
    //             return false;
    //         }
    //     }

    //     throw new System.Exception("Gridcast exceeded 100.000 iterations to reach target! Something is totally wrong or your cast is way too big! D:");
    // }
    // static bool GridcastSimple(Vector2 _start, CachedAssets.DoubleInt _startTileCoords, Vector2 _end, CachedAssets.DoubleInt _endTileCoords){
    //     // _start and _end local to grid (zero is in bottom left)
    //     Vector2 _startLocal = new Vector2(_startTileCoords.X + (_start.x - Mathf.Round(_start.x)), _startTileCoords.Y + (_start.y - Mathf.Round(_start.y)));
    //     Vector2 _endLocal = new Vector2(_endTileCoords.X + (_end.x - Mathf.Round(_end.x)), _endTileCoords.Y + (_end.y - Mathf.Round(_end.y)));

    //     // find tiles along cast with a shadowcollider
    //     List<BresenhamsLine.OverlapSimple> _cast = BresenhamsLine.GridcastSimple(_startLocal, _endLocal);

    //     // determine if the cast should be able to hit its own collider or not (assuming we're shooting from a corner because that's what I'm using it for currently >.>)
    //     bool _canHitStartTile = CanHitOwnTile(_start, _end, Grid.Instance.grid[_startTileCoords.X, _startTileCoords.Y].WorldPosition);
    //     //Debug.Log(_diffX + ", " + _diffY + ": " + _euler);

    //     Color _col = new Color(Random.value, Random.value, Random.value, 1);
    //     Vector2 _tilePos = Grid.Instance.grid[_startTileCoords.X, _startTileCoords.Y].WorldPosition;
    //     float _scale = 0.1f;
    //     // Debug.DrawLine(_start, _end, col, Mathf.Infinity);
    //     // Debug.DrawLine(_tilePos + new Vector2(-_scale, _scale), _tilePos + new Vector2(_scale, _scale), col, Mathf.Infinity);
    //     // Debug.DrawLine(_tilePos + new Vector2(_scale, _scale), _tilePos + new Vector2(_scale, -_scale), col, Mathf.Infinity);
    //     // Debug.DrawLine(_tilePos + new Vector2(_scale, -_scale), _tilePos + new Vector2(-_scale, -_scale), col, Mathf.Infinity);
    //     // Debug.DrawLine(_tilePos + new Vector2(-_scale, -_scale), _tilePos + new Vector2(-_scale, _scale), col, Mathf.Infinity);

        

    //     // stop hitting yourself
    //     if (CanHitOwnTile(_start, _end, Grid.Instance.grid[_startTileCoords.X, _startTileCoords.Y].WorldPosition) && 
    //         sGridColliderArray[(int)_cast[0].Pos.x, (int)_cast[0].Pos.y]){

    //         SuperDebug.Log(
    //             _start, _col,
    //             "I hit myself!",
    //             (_cast[0].ExtraPositions != null && _cast[0].ExtraPositions.Length > 0).ToString(),
    //             (_cast[1].ExtraPositions != null && _cast[1].ExtraPositions.Length > 0).ToString(),
    //             _start + " -> " + _end + ":",
    //             (_cast[0].Pos - new Vector2(Grid.GridSizeXHalf, Grid.GridSizeYHalf)).ToString(),
    //             (_cast[1].Pos - new Vector2(Grid.GridSizeXHalf, Grid.GridSizeYHalf)).ToString()
    //         );
    //         return true;
    //     }
    //     else if (_cast[0].ExtraPositions != null){
    //         for (int i = 0; i < _cast[0].ExtraPositions.Length; i++){
    //             if (CanHitOwnTile(_start, _end, Grid.Instance.grid[(int)_cast[0].ExtraPositions[i].x, (int)_cast[0].ExtraPositions[i].y].WorldPosition) &&
    //                 sGridColliderArray[(int)_cast[0].ExtraPositions[i].x, (int)_cast[0].ExtraPositions[i].y]){

    //                 SuperDebug.Log(
    //                     _start, _col,
    //                     "I hit myself! Extra, Extra!",
    //                     (_cast[0].ExtraPositions != null && _cast[0].ExtraPositions.Length > 0).ToString(),
    //                     (_cast[1].ExtraPositions != null && _cast[1].ExtraPositions.Length > 0).ToString(),
    //                     _start + " -> " + _end + ":",
    //                     (_cast[0].Pos - new Vector2(Grid.GridSizeXHalf, Grid.GridSizeYHalf)).ToString(),
    //                     (_cast[1].Pos - new Vector2(Grid.GridSizeXHalf, Grid.GridSizeYHalf)).ToString()
    //                 );
    //                 return true;
    //             }
    //         }
    //     }
    //     for (int i = 1; i < _cast.Count; i++){
    //         // check if hit tile
    //         if (sGridColliderArray[(int)_cast[i].Pos.x, (int)_cast[i].Pos.y]) {
    //             Vector2 pos = _cast[i].Pos - new Vector2(Grid.GridSizeXHalf, Grid.GridSizeXHalf);
    //             Debug.DrawLine(_start, _end, _col, Mathf.Infinity);
    //             Debug.DrawLine(pos + new Vector2(-_scale, _scale), pos + new Vector2(_scale, _scale), _col, Mathf.Infinity);
    //             Debug.DrawLine(pos + new Vector2(_scale, _scale), pos + new Vector2(_scale, -_scale), _col, Mathf.Infinity);
    //             Debug.DrawLine(pos + new Vector2(_scale, -_scale), pos + new Vector2(-_scale, -_scale), _col, Mathf.Infinity);
    //             Debug.DrawLine(pos + new Vector2(-_scale, -_scale), pos + new Vector2(-_scale, _scale), _col, Mathf.Infinity);

    //             SuperDebug.Log(
    //                 _start, _col,
    //                 (_cast[0].ExtraPositions != null && _cast[0].ExtraPositions.Length > 0).ToString(),
    //                 (_cast[1].ExtraPositions != null && _cast[1].ExtraPositions.Length > 0).ToString(),
    //                 _start + " -> " + _end + " (" + pos + "):",
    //                 (_cast[0].Pos - new Vector2(Grid.GridSizeXHalf, Grid.GridSizeYHalf)).ToString(),
    //                 (_cast[1].Pos - new Vector2(Grid.GridSizeXHalf, Grid.GridSizeYHalf)).ToString()
    //             );

    //             return true;
    //         }

    //         // else check if hit any equally close tiles
    //         else if (_cast[i].ExtraPositions != null){
    //             for (int j = 0; j < _cast[i].ExtraPositions.Length; j++){

    //                 if (sGridColliderArray[(int)_cast[i].ExtraPositions[j].x, (int)_cast[i].ExtraPositions[j].y]) {
    //                     Vector2 pos = _cast[i].ExtraPositions[j] - new Vector2(Grid.GridSizeXHalf, Grid.GridSizeYHalf);
    //                     Debug.DrawLine(_start, _end, _col, Mathf.Infinity);
    //                     Debug.DrawLine(pos + new Vector2(-_scale, _scale), pos + new Vector2(_scale, _scale), _col, Mathf.Infinity);
    //                     Debug.DrawLine(pos + new Vector2(_scale, _scale), pos + new Vector2(_scale, -_scale), _col, Mathf.Infinity);
    //                     Debug.DrawLine(pos + new Vector2(_scale, -_scale), pos + new Vector2(-_scale, -_scale), _col, Mathf.Infinity);
    //                     Debug.DrawLine(pos + new Vector2(-_scale, -_scale), pos + new Vector2(-_scale, _scale), _col, Mathf.Infinity);

    //                     SuperDebug.Log(
    //                         _start, _col, 
    //                         "Extra, Extra!",
    //                         (_cast[0].ExtraPositions != null && _cast[0].ExtraPositions.Length > 0).ToString(),
    //                         (_cast[1].ExtraPositions != null && _cast[1].ExtraPositions.Length > 0).ToString(),
    //                         _start + " -> " + _end + " (" + pos + "):",
    //                         (_cast[0].Pos - new Vector2(Grid.GridSizeXHalf, Grid.GridSizeYHalf)).ToString(), 
    //                         (_cast[1].Pos - new Vector2(Grid.GridSizeXHalf, Grid.GridSizeYHalf)).ToString()
    //                     );

    //                     return true;
    //                 }
    //             }
    //         }
    //     }

    //     return false;
    // }

    // static bool CanHitOwnTile(Vector2 _start, Vector2 _end, Vector2 _startTileWorldPos) {
    //     Vector2 _diff = _start - _startTileWorldPos;
    //     int _euler = (int)GetAngleClockwise(_end, _start, 360);
    //     if (_diff.x < 0 && _diff.y > 0) // TL corner of tile
    //         return _euler >= 90 && _euler <= 180;
    //     else if (_diff.x > 0 && _diff.y > 0) // TR corner of tile
    //         return _euler >= 180 && _euler <= 270;
    //     else if (_diff.x > 0 && _diff.y < 0) // BR corner of tile
    //         return _euler >= 270;
    //     else if (_diff.x < 0 && _diff.y < 0) // BL corner of tile
    //         return _euler >= 0 && _euler <= 90;

    //     return false;
    // }

    // Vector2 testPos;
    // bool IsPossibleToGridcastFurther(Vector2 _start, Vector2 _dir) {
    //     testPos = _start + (_dir * 0.01f);

    //     t = Grid.Instance.GetTileFromWorldPoint(testPos);
    //     CachedAssets.Instance.WallSets[0].GetShadowCollider(t.ExactType, t.Animator.CurrentFrame, t.WorldPosition, ref sShadowCollider);

    //     float closest = 0;
    //     return sShadowCollider != null && sShadowCollider.OverlapPointOrAlmost(testPos, out closest);
    // }

    // List<Vector2> hits = new List<Vector2>();
    // List<Vector2> nonHits = new List<Vector2>();
    // List<bool> nonHitsHadCollider = new List<bool>();
    // List<float> nonHitsDistance = new List<float>();
}

