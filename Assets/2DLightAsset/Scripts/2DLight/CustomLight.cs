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

    void Start() {
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
    private static Texture2D TextureLightDotXs;
    private static Texture2D TextureLightDotYs;

    private static Color32[] ReportedDotXs;
    private static Color32[] ReportedDotYs;

    private static int shaderPropertyDotX;
    private static int shaderPropertyDotY;
    private static Material GridMaterial;

    private static bool[,] sGridColliderArray;

    [EasyButtons.Button]
    public static void UpdateAllLights() {

        if(sGridColliderArray == null)
            sGridColliderArray = new bool[Grid.Instance.GridSizeX, Grid.Instance.GridSizeY];
        for (int x = 0; x < Grid.Instance.GridSizeX; x++){
            for (int y = 0; y < Grid.Instance.GridSizeY; y++){
                sGridColliderArray[x, y] = CachedAssets.Instance.WallSets[0].GetShadowCollider(Grid.Instance.grid[x, y].ExactType, Grid.Instance.grid[x, y].Animator.CurrentFrame);
            }
        }

        // create textures and arrays
        if (TextureLightDotXs == null) {
            TextureLightDotXs = new Texture2D(Grid.Instance.GridSizeX, Grid.Instance.GridSizeY, TextureFormat.RGBA32, false);
            TextureLightDotXs.filterMode = FilterMode.Point;
        }
        if (TextureLightDotYs == null) {
            TextureLightDotYs = new Texture2D(Grid.Instance.GridSizeX, Grid.Instance.GridSizeY, TextureFormat.RGBA32, false);
            TextureLightDotYs.filterMode = FilterMode.Point;
        }

        // find material and propertyIDs
        if (GridMaterial == null)
            GridMaterial = Grid.Instance.grid[0, 0].BottomQuad.Renderer.sharedMaterial;
        if (shaderPropertyDotX == 0)
            shaderPropertyDotX = Shader.PropertyToID("_DotXs");
        if (shaderPropertyDotY == 0)
            shaderPropertyDotY = Shader.PropertyToID("_DotYs");

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
        TextureLightDotXs.SetPixels32(ReportedDotXs);
        TextureLightDotYs.SetPixels32(ReportedDotYs);
        TextureLightDotXs.Apply();
        TextureLightDotYs.Apply();


        // apply textures to shader
        GridMaterial.SetTexture(shaderPropertyDotX, TextureLightDotXs);
        GridMaterial.SetTexture(shaderPropertyDotY, TextureLightDotYs);
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
        for (int y = 0; y < Grid.Instance.GridSizeY; y++) {
            for (int x = 0; x < Grid.Instance.GridSizeX; x++) {
                t = Grid.Instance.grid[x, y];
                if (!CachedAssets.Instance.WallSets[0].GetShadowCollider(t.ExactType, t.Animator.CurrentFrame, t.WorldPosition, ref sShadowCollider)) { 
                    if((t.WorldPosition - (Vector2)transform.position).magnitude < lightRadius)
                        tilesInRange.Add(t);
                    continue;
                }

                breakLoops = false;
                for (int j = 0; j < sShadowCollider.Paths.Length; j++){
                    for (int k = 0; k < sShadowCollider.Paths[j].Vertices.Length; k++){
                        if (((t.WorldPosition + sShadowCollider.Paths[j].Vertices[k]) - (Vector2)transform.position).magnitude <= lightRadius) {
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
            }
        }
    }

    private bool sortAngles = false;
    private bool lows = false; // check si hay menores a -0.5
    private bool highs = false; // check si hay mayores a 2.0
    private float magRange = 0.15f;
    private List<Verts> tempVerts = new List<Verts>();
    private CachedAssets.MovableCollider polCollider;
    private Verts v;
    private Vector3 worldPoint;
    private Vector2 rayHit;
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
    //private bool gridcastHit = false;
    //private List<Tile> gridcastHits = new List<Tile>();
    void SetLight() {
        allVertices.Clear();// Since these lists are populated every frame, clear them first to prevent overpopulation

        //--Step 2: Obtain vertices for each mesh --//
        //---------------------------------------------------------------------//

        magRange = 0.15f;
        tempVerts.Clear();
        polCollider = new CachedAssets.MovableCollider();
        for (int i = 0; i < allTiles.Count; i++) {
            tempVerts.Clear();
            CachedAssets.Instance.WallSets[0].GetShadowCollider(allTiles[i].ExactType, allTiles[i].Animator.CurrentFrame, allTiles[i].WorldPosition, ref polCollider);
            
            // the following variables used to fix sorting bug
            // the calculated angles are in mixed quadrants (1 and 4)
            lows = false; // check for minors at -0.5
            highs = false; // check for majors at 2.0

            for (int pIndex = 0; pIndex < polCollider.Paths.Length; pIndex++) {
                for (int vIndex = 0; vIndex < polCollider.Paths[pIndex].Vertices.Length; vIndex++) {  // ...and for every vertex we have of each collider...
                    v = new Verts();

                    // Convert vertex to world space
                    worldPoint = polCollider.WorldPosition + polCollider.Paths[pIndex].Vertices[vIndex];
                    if (Gridcast(transform.position, worldPoint, true, out rayHit)) {
                        v.Pos = rayHit;
                        if (worldPoint.sqrMagnitude >= (rayHit.sqrMagnitude - magRange) && worldPoint.sqrMagnitude <= (rayHit.sqrMagnitude + magRange)) {
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

                        if (Gridcast(from, from + (dir.normalized * mag), true, out rayHit)) { 
                            hitPos = rayHit;
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

            if (Gridcast(transform.position, v.Pos, false, out rayHit))
                v.Pos = transform.InverseTransformPoint(rayHit);
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

    private static Vector2 CORNER_OFFSET = new Vector2(-0.5f, -0.5f);
    private static Vector2 CORNER_OFFSET_OUTSIDE_GRID = new Vector2(0.5f, -0.5f);
    static void CalculateLightingForGrid() {
        if (ReportedDotXs == null)
            ReportedDotXs = new Color32[Grid.Instance.GridSizeX * Grid.Instance.GridSizeY];
        if(ReportedDotYs == null)
            ReportedDotYs = new Color32[Grid.Instance.GridSizeX * Grid.Instance.GridSizeY];

        for (int x = 0; x < Grid.Instance.GridSizeX + 1; x++){ // +1 because we're getting each corner
            for (int y = 0; y < Grid.Instance.GridSizeY + 1; y++){


                // TODO: cast against light


                // get lighting from all lights for the bottom-left corner (if outside grid, use bottom-right corner of the neighbour inside the grid)
                int _safeX = Mathf.Min(x, Grid.Instance.GridSizeX - 1);
                int _safeY = Mathf.Min(y, Grid.Instance.GridSizeY - 1);
                Vector2 _offset = (x < Grid.Instance.GridSizeX && y < Grid.Instance.GridSizeY) ? CORNER_OFFSET : CORNER_OFFSET_OUTSIDE_GRID;

                Vector4 _lights; // the 4 most dominant lights
                Color32 _finalColor = GetTotalVertexLighting(Grid.Instance.grid[_safeX, _safeY].WorldPosition + _offset, out _lights);

                // apply vertex color to all vertices in this corner
                // THE INT IS COMPLETE GUESSWORK D:
                if (x < Grid.Instance.GridSizeX && y > 0) { 
                    Grid.Instance.grid[x, y - 1].FloorQuad.SetVertexColor(0, _finalColor);
                    Grid.Instance.grid[x, y - 1].FloorCornerHider.SetVertexColor(0, _finalColor);
                    Grid.Instance.grid[x, y - 1].BottomQuad.SetVertexColor(0, _finalColor);
                    Grid.Instance.grid[x, y - 1].TopQuad.SetVertexColor(0, _finalColor);
                    Grid.Instance.grid[x, y - 1].WallCornerHider.SetVertexColor(0, _finalColor);
                }
                if (x > 0 && y > 0) { 
                    Grid.Instance.grid[x - 1, y - 1].FloorQuad.SetVertexColor(1, _finalColor);
                    Grid.Instance.grid[x - 1, y - 1].FloorCornerHider.SetVertexColor(1, _finalColor);
                    Grid.Instance.grid[x - 1, y - 1].BottomQuad.SetVertexColor(1, _finalColor);
                    Grid.Instance.grid[x - 1, y - 1].TopQuad.SetVertexColor(1, _finalColor);
                    Grid.Instance.grid[x - 1, y - 1].WallCornerHider.SetVertexColor(1, _finalColor);
                }
                if (x > 0 && y < Grid.Instance.GridSizeY) { 
                    Grid.Instance.grid[x - 1, y].FloorQuad.SetVertexColor(2, _finalColor);
                    Grid.Instance.grid[x - 1, y].FloorCornerHider.SetVertexColor(2, _finalColor);
                    Grid.Instance.grid[x - 1, y].BottomQuad.SetVertexColor(2, _finalColor);
                    Grid.Instance.grid[x - 1, y].TopQuad.SetVertexColor(2, _finalColor);
                    Grid.Instance.grid[x - 1, y].WallCornerHider.SetVertexColor(2, _finalColor);
                }
                if (x < Grid.Instance.GridSizeX && y < Grid.Instance.GridSizeY) { 
                    Grid.Instance.grid[x, y].FloorQuad.SetVertexColor(3, _finalColor);
                    Grid.Instance.grid[x, y].FloorCornerHider.SetVertexColor(3, _finalColor);
                    Grid.Instance.grid[x, y].BottomQuad.SetVertexColor(3, _finalColor);
                    Grid.Instance.grid[x, y].TopQuad.SetVertexColor(3, _finalColor);
                    Grid.Instance.grid[x, y].WallCornerHider.SetVertexColor(3, _finalColor);
                }


                // TODO: try to make Dot part of vertex-stuff


                // use the total vertexlighting to calculate dot x and y (per tile, NOT per corner)
                if (x < Grid.Instance.GridSizeX && y < Grid.Instance.GridSizeY){
                    // dot X
                    ReportedDotXs[(y * Grid.Instance.GridSizeX) + x].r = (byte)GetAngle(Grid.Instance.grid[x, y].WorldPosition, (Vector2)AllLights[(int)_lights.x].transform.position, Vector2.left, 255);
                    ReportedDotXs[(y * Grid.Instance.GridSizeX) + x].g = (byte)GetAngle(Grid.Instance.grid[x, y].WorldPosition, (Vector2)AllLights[(int)_lights.y].transform.position, Vector2.left, 255);
                    ReportedDotXs[(y * Grid.Instance.GridSizeX) + x].b = (byte)GetAngle(Grid.Instance.grid[x, y].WorldPosition, (Vector2)AllLights[(int)_lights.z].transform.position, Vector2.left, 255);
                    ReportedDotXs[(y * Grid.Instance.GridSizeX) + x].a = (byte)GetAngle(Grid.Instance.grid[x, y].WorldPosition, (Vector2)AllLights[(int)_lights.w].transform.position, Vector2.left, 255);

                    // dot Y
                    ReportedDotYs[(y * Grid.Instance.GridSizeX) + x].r = (byte)GetAngle(Grid.Instance.grid[x, y].WorldPosition, (Vector2)AllLights[(int)_lights.x].transform.position, Vector2.down, 255);
                    ReportedDotYs[(y * Grid.Instance.GridSizeX) + x].g = (byte)GetAngle(Grid.Instance.grid[x, y].WorldPosition, (Vector2)AllLights[(int)_lights.y].transform.position, Vector2.down, 255);
                    ReportedDotYs[(y * Grid.Instance.GridSizeX) + x].b = (byte)GetAngle(Grid.Instance.grid[x, y].WorldPosition, (Vector2)AllLights[(int)_lights.z].transform.position, Vector2.down, 255);
                    ReportedDotYs[(y * Grid.Instance.GridSizeX) + x].a = (byte)GetAngle(Grid.Instance.grid[x, y].WorldPosition, (Vector2)AllLights[(int)_lights.w].transform.position, Vector2.down, 255);
                }
            }
        }
    }
    static float GetAngle(Vector2 _pos1, Vector2 _pos2, Vector2 _referenceAngle, int maxAngle) { // TODO: replace with GetAngleClockwise!
        return maxAngle * (0.5f * (1 + Vector2.Dot(
                                            _referenceAngle, 
                                            Vector3.Normalize(_pos1 - _pos2))));
    }
    static float GetAngleClockwise(Vector2 _pos1, Vector2 _pos2, int _maxAngle){
        // gets the angle between _pos1 and _pos2, ranging from 0 to _maxAngle.
        // the angle goes all-the-way-around, like a clock and unlike a dot-product.

        float _vertical     = (0.5f * (1 + Vector2.Dot(Vector2.down,   Vector3.Normalize(_pos1 - _pos2))));
        float _horizontal   =              Vector2.Dot(Vector2.right, Vector3.Normalize(_pos1 - _pos2));

        _vertical = (_vertical * (_maxAngle * 0.5f));
        if (_horizontal < 0)
            _vertical = Mathf.Abs(_vertical - _maxAngle);

        return _vertical;
    }

    static Color32 GetTotalVertexLighting(Vector2 _pos, out Vector4 _dominantLightIndices){

        // four because we can only store Dot-info for four lights (mostly bc performance) and then any higher number makes little sense
        _dominantLightIndices = new Vector4();
        Vector4 dominantLightLevels = new Vector4();
        Vector4 dominantLightColors = new Vector4(); // todo: make a QuadrupleInt
        for (int i = 0; i < AllLights.Count; i++){ // optimization: can I lower the amount of lights I'm iterating over? Maybe by using a tree?
            float newRange = AllLights[i].lightRadius;
            float newDistance = Mathf.Min(Mathf.RoundToInt(Vector2.Distance(_pos, AllLights[i].transform.position)), 255);
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

    private static CachedAssets.MovableCollider sShadowCollider = new CachedAssets.MovableCollider();
    private static CachedAssets.MovableCollider[] sExtraShadowColliders;
    bool Gridcast(Vector2 _start, Vector2 _end, bool debug, out Vector2 _rayhit) {

        // calculate indices to iterate over
        List<BresenhamsLine.OverlapWithTiles> _cast = BresenhamsLine.Gridcast(_start, _end);

        // get colliders at start position
        if (_cast.Count > 0) {
            CachedAssets.Instance.WallSets[0].GetShadowCollider(_cast[0].Tile.ExactType, _cast[0].Tile.Animator.CurrentFrame, _cast[0].Tile.WorldPosition, ref sShadowCollider);

            if (_cast[0].ExtraTiles != null){
                sExtraShadowColliders = new CachedAssets.MovableCollider[_cast[0].ExtraTiles.Length];
                for (int i = 0; i < _cast[0].ExtraTiles.Length; i++)
                    CachedAssets.Instance.WallSets[0].GetShadowCollider(_cast[0].ExtraTiles[i].ExactType, _cast[0].ExtraTiles[i].Animator.CurrentFrame, _cast[0].ExtraTiles[i].WorldPosition, ref sExtraShadowColliders[i]);
            }
        }

       // use approximate pixel positions to iterate through cast and test each pixel for shadowcollider
        int _currentIndex = 0;
        int _iterationsInCast = 0;
        int _goal = Mathf.RoundToInt(((_end - _start).magnitude + 1) * Tile.RESOLUTION); // the distance in amount of tiles, multiplied by the amount of pixels across a tile
        Vector2 _curPos = _start;
        while (_iterationsInCast <= _goal || _currentIndex < _cast.Count) {
            _curPos = Vector2.Lerp(_start, _end, (float)_iterationsInCast / (float)_goal);
            _iterationsInCast++;

            // if pixel is closer to next collider, set next collider as current
            if (_currentIndex < _cast.Count - 1 && (_cast[_currentIndex + 1].Tile.WorldPosition - _curPos).magnitude <= (_cast[_currentIndex].Tile.WorldPosition - _curPos).magnitude) {
                _currentIndex++;

                CachedAssets.Instance.WallSets[0].GetShadowCollider(_cast[_currentIndex].Tile.ExactType, _cast[_currentIndex].Tile.Animator.CurrentFrame, _cast[_currentIndex].Tile.WorldPosition, ref sShadowCollider);

                // get extra colliders (happens if pixel is (more-or-less) in a corner)
                if (_cast[_currentIndex].ExtraTiles != null){
                    sExtraShadowColliders = new CachedAssets.MovableCollider[_cast[_currentIndex].ExtraTiles.Length];
                    for (int i = 0; i < _cast[_currentIndex].ExtraTiles.Length; i++)
                        CachedAssets.Instance.WallSets[0].GetShadowCollider(_cast[_currentIndex].ExtraTiles[i].ExactType, _cast[_currentIndex].ExtraTiles[i].Animator.CurrentFrame, _cast[_currentIndex].ExtraTiles[i].WorldPosition, ref sExtraShadowColliders[i]);
                }
            }

            // check for hit
            float closest = 1000;
            if (sShadowCollider != null && sShadowCollider.OverlapPointOrAlmost(_curPos, out closest)) {
                _rayhit = _curPos;
                hits.Add(_curPos);
                hitsDistance.Add(closest);
                return true;
            }
            // check for hit if in corner
            if (sExtraShadowColliders != null){
                for (int i = 0; i < sExtraShadowColliders.Length; i++){
                    if (sExtraShadowColliders[i] != null && sExtraShadowColliders[i].OverlapPointOrAlmost(_curPos, out closest)){
                        _rayhit = _curPos;
                        hits.Add(_curPos);
                        hitsDistance.Add(closest);
                        return true;
                    }
                }
            }

            // done, so no hit
            if ((float)_iterationsInCast / (float)_goal > 1) {
                _rayhit = Vector2.zero;
                nonHits.Add(_curPos);
                nonHitsDistance.Add(closest);
                nonHitsHadCollider.Add(sShadowCollider != null && sExtraShadowColliders != null && sExtraShadowColliders.Length > 0);
                return false;
            }
        }

        throw new System.Exception("Gridcast exceeded 100.000 iterations to reach target! Something is totally wrong or your cast is way too big! D:");
    }
    bool GridcastSimple(Vector2 _start, CachedAssets.DoubleInt _startTileCoords, Vector2 _end, CachedAssets.DoubleInt _endTileCoords){
        // _start and _end local to grid (zero is in bottom left)
        Vector2 _startLocal = new Vector2(_startTileCoords.X + (_start.x - Mathf.Round(_start.x)), _startTileCoords.Y + (_start.y - Mathf.Round(_start.y)));
        Vector2 _endLocal = new Vector2(_endTileCoords.X + (_end.x - Mathf.Round(_end.x)), _endTileCoords.Y + (_end.y - Mathf.Round(_end.y)));

        // find tiles along cast with a shadowcollider
        List<BresenhamsLine.OverlapSimple> _cast = BresenhamsLine.GridcastSimple(_startLocal, _endLocal);

        // determine if the cast should be able to hit its own collider or not (assuming we're shooting from a corner because that's what I'm using it for currently >.>)
        bool _canHitStartTile = false;
        Vector2 _tilePos = Grid.Instance.grid[_startTileCoords.X, _startTileCoords.Y].WorldPosition;
        float _diffX = _start.x - _tilePos.x;
        float _diffY = _start.y - _tilePos.y;
        int _euler = (int)GetAngleClockwise(_start, _tilePos, 360);
        if (_diffX > 0 && _diffY < 0) // TL corner of tile
            _canHitStartTile = _euler > 90 && _euler < 180;
        else if (_diffX < 0 && _diffY < 0) // TR corner of tile
            _canHitStartTile = _euler > 180 && _euler < 270;
        else if (_diffX < 0 && _diffY > 0) // BR corner of tile
            _canHitStartTile = _euler > 270;
        else if (_diffX > 0 && _diffY > 0) // BL corner of tile
            _canHitStartTile = _euler > 0 && _euler < 90;

        // stop hitting yourself
        if(_canHitStartTile && sGridColliderArray[(int)_cast[0].Pos.x, (int)_cast[0].Pos.y])
            return true;
        for (int i = 1; i < _cast.Count; i++){
            // check if hit tile
            if(sGridColliderArray[(int)_cast[i].Pos.x, (int)_cast[i].Pos.y])
                return true;

            // else check if hit any equally close tiles
            else if (_cast[i].ExtraPositions != null){
                for (int j = 0; j < _cast[i].ExtraPositions.Length; j++){
                    if(sGridColliderArray[(int)_cast[i].ExtraPositions[j].x, (int)_cast[i].ExtraPositions[j].y])
                        return true;
                }
            }
        }

        return false;
    }

    Vector2 testPos;
    bool IsPossibleToGridcastFurther(Vector2 _start, Vector2 _dir) {
        testPos = _start + (_dir * 0.01f);

        t = Grid.Instance.GetTileFromWorldPoint(testPos);
        CachedAssets.Instance.WallSets[0].GetShadowCollider(t.ExactType, t.Animator.CurrentFrame, t.WorldPosition, ref sShadowCollider);

        float closest = 0;
        return sShadowCollider != null && sShadowCollider.OverlapPointOrAlmost(testPos, out closest);
    }

    List<Vector2> hits = new List<Vector2>();
    List<float> hitsDistance = new List<float>();
    List<Vector2> nonHits = new List<Vector2>();
    List<bool> nonHitsHadCollider = new List<bool>();
    List<float> nonHitsDistance = new List<float>();
}

