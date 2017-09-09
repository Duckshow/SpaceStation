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
    private Light myDefaultLight;
    private CanInspect myInspector;


    void OnEnable() {
        AllLights.Add(this);

        if(myInspector == null)
            myInspector = GetComponent<CanInspect>();

        myInspector.PostPickUp += PostPickUp;
        myInspector.PostPutDown += PostPutDown;

        if(myDefaultLight == null)
            myDefaultLight = GetComponentInChildren<Light>();
    }
    void OnDisable() {
        myInspector.PostPickUp -= PostPickUp;
        myInspector.PostPutDown -= PostPutDown;

        AllLights.Remove(this);
    }
    void Start() {
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

    private static Texture2D TextureLightAngles;
    private static Texture2D TextureLightColors;
    private static Texture2D TextureLightRanges;
    private static Texture2D TextureLightDistances;
    private static Texture2D TextureLightIntensities;
    private static Color32[] ClearTextureArray;
    private static Color32[] ReportedAngles;
    private static Color32[] ReportedColors;
    private static Color32[] ReportedRanges;
    private static Color32[] ReportedDistances;
    private static Color32[] ReportedIntensities;
    private static int shaderPropertyAngles;
    private static int shaderPropertyColors;
    private static int shaderPropertyRanges;
    private static int shaderPropertyDistances;
    private static int shaderPropertyIntensities;
    private static Material GridMaterial;
    [EasyButtons.Button]
    public void UpdateAllLights() {

        // create textures and arrays
        if (ClearTextureArray == null)
            ClearTextureArray = new Color32[Grid.Instance.GridSizeX * Grid.Instance.GridSizeY];
        if (ReportedAngles == null)
            ReportedAngles = new Color32[Grid.Instance.GridSizeX * Grid.Instance.GridSizeY];
        if (ReportedColors == null)
            ReportedColors = new Color32[Grid.Instance.GridSizeX * Grid.Instance.GridSizeY];
        if (ReportedRanges == null)
            ReportedRanges = new Color32[Grid.Instance.GridSizeX * Grid.Instance.GridSizeY];
        if (ReportedDistances == null)
            ReportedDistances = new Color32[Grid.Instance.GridSizeX * Grid.Instance.GridSizeY];
        if (ReportedIntensities == null)
            ReportedIntensities = new Color32[Grid.Instance.GridSizeX * Grid.Instance.GridSizeY];
        if (TextureLightAngles == null)
            TextureLightAngles = new Texture2D(Grid.Instance.GridSizeX, Grid.Instance.GridSizeY, TextureFormat.RGBA32, false);
        if (TextureLightColors == null)
            TextureLightColors = new Texture2D(Grid.Instance.GridSizeX, Grid.Instance.GridSizeY, TextureFormat.RGBA32, false);
        if (TextureLightRanges == null)
            TextureLightRanges = new Texture2D(Grid.Instance.GridSizeX, Grid.Instance.GridSizeY, TextureFormat.RGBA32, false);
        if (TextureLightDistances == null)
            TextureLightDistances = new Texture2D(Grid.Instance.GridSizeX, Grid.Instance.GridSizeY, TextureFormat.RGBA32, false);
        if (TextureLightIntensities == null)
            TextureLightIntensities = new Texture2D(Grid.Instance.GridSizeX, Grid.Instance.GridSizeY, TextureFormat.RGBA32, false);

        // find material and propertyIDs
        if (GridMaterial == null)
            GridMaterial = Grid.Instance.grid[0, 0].BottomQuad.Renderer.sharedMaterial;
        if (shaderPropertyAngles == 0)
            shaderPropertyAngles = Shader.PropertyToID("_Angles");
        if (shaderPropertyColors == 0)
            shaderPropertyColors = Shader.PropertyToID("_Colors");
        if (shaderPropertyRanges == 0)
            shaderPropertyRanges = Shader.PropertyToID("_Ranges");
        if (shaderPropertyDistances == 0)
            shaderPropertyDistances = Shader.PropertyToID("_Distances");
        if (shaderPropertyIntensities == 0)
            shaderPropertyIntensities = Shader.PropertyToID("_Intensities");

        // clear everything old
        ReportedAngles = ClearTextureArray;
        ReportedColors = ClearTextureArray;
        ReportedRanges = ClearTextureArray;
        ReportedDistances = ClearTextureArray;
        ReportedIntensities = ClearTextureArray;
        //ClearAllTileLightInfo();

        // get to business
        for (int i = 0; i < AllLights.Count; i++) {
            if (AllLights[i].lightMesh != null)
                AllLights[i].lightMesh.Clear();

            if (AllLights[i].myInspector.CurrentState != CanInspect.State.Default)
                continue;

            AllLights[i].UpdateLight();
        }

        // apply arrays to textures
        TextureLightAngles.SetPixels32(ReportedAngles);
        TextureLightColors.SetPixels32(ReportedColors);
        TextureLightRanges.SetPixels32(ReportedRanges);
        TextureLightDistances.SetPixels32(ReportedDistances);
        TextureLightIntensities.SetPixels32(ReportedIntensities);

        // apply textures to shader
        GridMaterial.SetTexture(shaderPropertyAngles, TextureLightAngles);
        GridMaterial.SetTexture(shaderPropertyColors, TextureLightColors);
        GridMaterial.SetTexture(shaderPropertyRanges, TextureLightRanges);
        GridMaterial.SetTexture(shaderPropertyDistances, TextureLightDistances);
        GridMaterial.SetTexture(shaderPropertyIntensities, TextureLightIntensities);

        dawd // I think I'm finished with the CPU side of things. Start looking at the shader-stuff. (oh, apparently the grid-shader is a surface shader, so I have to rewrite the whole thing.

        //if (Tile.GRID_LIGHTS_ANGLES == null)
        //    Tile.GRID_LIGHTS_ANGLES = new ulong[Grid.Instance.GridSizeX * Grid.Instance.GridSizeY];
        //if (Tile.GRID_LIGHTS_COLORS == null)
        //    Tile.GRID_LIGHTS_COLORS = new ulong[Grid.Instance.GridSizeX * Grid.Instance.GridSizeY];
        //if (Tile.GRID_LIGHTS_RANGES == null)
        //    Tile.GRID_LIGHTS_RANGES = new ulong[Grid.Instance.GridSizeX * Grid.Instance.GridSizeY];
        //if (Tile.GRID_LIGHTS_DISTANCES == null)
        //    Tile.GRID_LIGHTS_DISTANCES = new ulong[Grid.Instance.GridSizeX * Grid.Instance.GridSizeY];
        //if (Tile.GRID_LIGHTS_INTENSITIES == null)
        //    Tile.GRID_LIGHTS_INTENSITIES = new ulong[Grid.Instance.GridSizeX * Grid.Instance.GridSizeY];

        //int xy = 0;
        //for (int y = 0; y < Grid.Instance.GridSizeY; y++) {
        //    for (int x = 0; x < Grid.Instance.GridSizeX; x++) {
        //        xy = (Grid.Instance.GridSizeX * y) + 1;
        //        Tile.GRID_LIGHTS_ANGLES[xy] = Grid.Instance.grid[x, y].Lights_Angle;
        //        Tile.GRID_LIGHTS_COLORS[xy] = Grid.Instance.grid[x, y].Lights_Color;
        //        Tile.GRID_LIGHTS_RANGES[xy] = Grid.Instance.grid[x, y].Lights_Range;
        //        Tile.GRID_LIGHTS_DISTANCES[xy] = Grid.Instance.grid[x, y].Lights_Distance;
        //        Tile.GRID_LIGHTS_INTENSITIES[xy] = Grid.Instance.grid[x, y].Lights_Intensity;
        //    }
        //}



        //int xy = 0;
        //for (int y = 0; y < Grid.Instance.GridSizeY; y++) {
        //    for (int x = 0; x < Grid.Instance.GridSizeX; x++) {
        //        xy = (Grid.Instance.GridSizeX * y) + 1;
        //        TextureLightAngles.GetPixel(x, y).r =;

        //        Tile.GRID_LIGHTS_ANGLES[xy] = Grid.Instance.grid[x, y].Lights_Angle;
        //        Tile.GRID_LIGHTS_COLORS[xy] = Grid.Instance.grid[x, y].Lights_Color;
        //        Tile.GRID_LIGHTS_RANGES[xy] = Grid.Instance.grid[x, y].Lights_Range;
        //        Tile.GRID_LIGHTS_DISTANCES[xy] = Grid.Instance.grid[x, y].Lights_Distance;
        //        Tile.GRID_LIGHTS_INTENSITIES[xy] = Grid.Instance.grid[x, y].Lights_Intensity;
        //    }
        //}
    }

    void UpdateLight() { // shouldn't call this by itself. must update ALL lights.
        GetAllMeshes();
        ReportLightInfoToTilesHit();
        SetLight();
        RenderLightMesh();
        ResetBounds();
        myDefaultLight.range = lightRadius;// * 4;
        //isUpToDate = true;
    }

    //private bool isUpToDate = false;
    //void Update() {
    //    if(myInspector.CurrentState == CanInspect.State.Contained)
    //        return;
    //    if(isUpToDate)
    //        return;

    //    UpdateLight();
    //}

    private Tile t;
    private bool breakLoops = false;
    void GetAllMeshes() {
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
                if(!CachedAssets.Instance.WallSets[0].GetShadowCollider(t.ExactType, t.Animator.CurrentFrame, t.WorldPosition, ref sShadowCollider))
                    continue;

                breakLoops = false;
                for (int j = 0; j < sShadowCollider.Paths.Length; j++){
                    for (int k = 0; k < sShadowCollider.Paths[j].Vertices.Length; k++){
                        if (((t.WorldPosition + sShadowCollider.Paths[j].Vertices[k]) - (Vector2)transform.position).magnitude <= lightRadius) {
                            allTiles.Add(t);
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
    private bool gridcastHit = false;
    private List<Tile> gridcastHits = new List<Tile>();
    void SetLight() {
        gridcastHits.Clear();
        allVertices.Clear();// Since these lists are populated every frame, clear them first to prevent overpopulation

        //--Step 2: Obtain vertices for each mesh --//
        //---------------------------------------------------------------------//

        magRange = 0.15f;
        tempVerts.Clear();
        polCollider = new CachedAssets.MovableCollider();
        for (int i = 0; i < allTiles.Count; i++) {
            tempVerts.Clear();
            //polCollider = allMeshes[i];
            CachedAssets.Instance.WallSets[0].GetShadowCollider(allTiles[i].ExactType, allTiles[i].Animator.CurrentFrame, allTiles[i].WorldPosition, ref polCollider);
            
            // the following variables used to fix sorting bug
            // the calculated angles are in mixed quadrants (1 and 4)
            lows = false; // check for minors at -0.5
            highs = false; // check for majors at 2.0

            //if (((1 << polCollider.transform.gameObject.layer) & layer) != 0) { // check if collider's layer is in the current layermask (I think? :c)
            //for (int j = 0; j < polCollider.GetTotalPointCount(); j++) {    // ...and for every vertex we have of each collider...

            //}
            // if (true) {
            //    Gridcast(transform.position, (Vector2)transform.position + new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)), true, out rayHit);
            //    Gridcast(transform.position, (Vector2)transform.position + new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)), true, out rayHit);
            //    Gridcast(transform.position, (Vector2)transform.position + new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)), true, out rayHit);
            //    Gridcast(transform.position, (Vector2)transform.position + new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)), true, out rayHit);
            //    Gridcast(transform.position, (Vector2)transform.position + new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)), true, out rayHit);
            //    Gridcast(transform.position, (Vector2)transform.position + new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)), true, out rayHit);
            //    Gridcast(transform.position, (Vector2)transform.position + new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)), true, out rayHit);
            //    Gridcast(transform.position, (Vector2)transform.position + new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)), true, out rayHit);
            //    Gridcast(transform.position, (Vector2)transform.position + new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)), true, out rayHit);
            //    Gridcast(transform.position, (Vector2)transform.position + new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)), true, out rayHit);
            //    Gridcast(transform.position, (Vector2)transform.position + new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)), true, out rayHit);
            //    Gridcast(transform.position, (Vector2)transform.position + new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)), true, out rayHit);
            //    Gridcast(transform.position, (Vector2)transform.position + new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)), true, out rayHit);
            //    Gridcast(transform.position, (Vector2)transform.position + new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)), true, out rayHit);
            //    Gridcast(transform.position, (Vector2)transform.position + new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)), true, out rayHit);
            //    Gridcast(transform.position, (Vector2)transform.position + new Vector2(Random.Range(-10, 10), Random.Range(-10, 10)), true, out rayHit);
            // }
            // else {
            gridcastHit = false;
                for (int pIndex = 0; pIndex < polCollider.Paths.Length; pIndex++) {
                    for (int vIndex = 0; vIndex < polCollider.Paths[pIndex].Vertices.Length; vIndex++) {  // ...and for every vertex we have of each collider...
                        v = new Verts();

                    // Convert vertex to world space
                        worldPoint = polCollider.WorldPosition + polCollider.Paths[pIndex].Vertices[vIndex];
                        //for (int x = 0; x < polCollider.Paths[pIndex].Vertices.Length; x++) {
                        //    Debug.Log((polCollider.WorldPosition + polCollider.Paths[pIndex].Vertices[x]).ToString().Color(Color.green));
                        //}
                        //rayHit = Physics2D.Raycast(transform.position, worldPoint - transform.position, (worldPoint - transform.position).magnitude, layer);
                        //rayHit = Physics2D.Linecast(transform.position, worldPoint, layer);

                        if (Gridcast(transform.position, worldPoint, true, out rayHit)) {
                        gridcastHit = true;    
                        //v.Pos = rayHit.point;
                            //if(worldPoint.sqrMagnitude >= (rayHit.point.sqrMagnitude - magRange) && worldPoint.sqrMagnitude <= (rayHit.point.sqrMagnitude + magRange))
                            //	v.Endpoint = true;
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

                        //if(DebugMode)
                        //    Debug.DrawLine(transform.position, v.Pos, Color.white);

                        //--Convert To local space for build mesh (mesh craft only in local vertex)
                        v.Pos = transform.InverseTransformPoint(v.Pos); // optimization: could we do the Linecast in local space instead?
                                                                        //--Calculate angle
                        v.Angle = GetVectorAngle(true, v.Pos.x, v.Pos.y);

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
                    //break;
                }
            }

            if (gridcastHit)
                gridcastHits.Add(allTiles[i]);
            //}

            // }

            //for (int pIndex = 0; pIndex < polCollider.Paths.Length; pIndex++) {
            //    for (int vIndex = 0; vIndex < polCollider.Paths[pIndex].Vertices.Length; vIndex++) {
            //        Debug.Log((polCollider.WorldPosition + polCollider.Paths[pIndex].Vertices[vIndex]).ToString().Color(Color.magenta));
            //    }
            //}
           // Debug.Break();
            //return;

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
                        mag = lightRadius;// - fromCast.magnitude;
                        from = (Vector2)fromCast + (dir * CHECK_POINT_LAST_RAY_OFFSET);

                        //rayCont = Physics2D.Raycast(from, dir, mag, layer);
                        //if (rayCont)
                        //    hitPos = rayCont.point;

                        int debugger = -1;
                        // if (!IsPossibleToGridcastFurther(from, dir)) { 
                        //     hitPos = from;
                        //     debugger = 0;
                        // }

                        if (Gridcast(from/* + new Vector2(0.5f, 0.5f)*/, from + (dir.normalized * mag)/* + new Vector2(0.5f, 0.5f)*/, true, out rayHit)) { 
                            hitPos = rayHit;
                            debugger = 1;

                            if (Vector2.Distance(hitPos, transform.position) > lightRadius){
                                dir = transform.InverseTransformDirection(dir); //local p
                                hitPos = transform.TransformPoint(dir.normalized * mag); // world p
                                //debugger = 3;
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
            //v.pos = new Vector3((Mathf.Sin(theta)), (Mathf.Cos(theta)), 0); // in radians low performance
            v.Pos = new Vector3((SinCosTable.sSinArray[theta]), (SinCosTable.sCosArray[theta]), 0); // in degrees (previous calculate)
            v.Angle = GetVectorAngle(true, v.Pos.x, v.Pos.y);
            v.Pos *= lightRadius;
            v.Pos += (Vector2)transform.position;

            //rayHit = Physics2D.Raycast(transform.position, v.Pos - transform.position, lightRadius, layer);
            //if (!rayHit) {
            //	v.Pos = transform.InverseTransformPoint(v.Pos);
            //	allVertices.Add(v);
            //}
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

    //void ClearAllTileLightInfo() {
    //    for (int y = 0; y < Grid.Instance.GridSizeY; y++) {
    //        for (int x = 0; x < Grid.Instance.GridSizeX; x++) {
    //            Grid.Instance.grid[x, y].Lights_Angle = 0;
    //            Grid.Instance.grid[x, y].Lights_Color = 0;
    //            Grid.Instance.grid[x, y].Lights_Range = 0;
    //            Grid.Instance.grid[x, y].Lights_Distance = 0;
    //            Grid.Instance.grid[x, y].Lights_Intensity = 0;
    //        }
    //    }
    //}

    void ReportLightInfoToTilesHit() {
        for (int i = 0; i < gridcastHits.Count; i++)
            ReportLightInfoToTile(gridcastHits[i]);
    }

    //bool stopthetrain = false;

    byte reportAngle;
    byte reportColor;
    byte reportRange;
    byte reportDistance;
    byte reportIntensity;
    float lightLevel;
    Color32 cachedAngles;
    Color32 cachedColors;
    Color32 cachedRanges;
    Color32 cachedIntensities;
    Color32 cachedDistances;
    byte overwriteChannel;
    Vector2 C;
    float tan;
    int deg;
    void ReportLightInfoToTile(Tile _t) {
        reportAngle = 0;
        reportColor = 0;
        reportRange = 0;
        reportDistance = 0;
        reportIntensity = 0;

        //if (stopthetrain)
        //    return;

        // tile is affected by six other lights (max), so this one will be ignored
        //if (MathfExtensions.Digits(_t.Lights_Angle) >= 18)
        //    return;

        // range
        reportRange = (byte)lightRadius;
        //ConcatLightUlong(Mathf.RoundToInt(lightRadius), ref _t.Lights_Range);

        // distance
        reportDistance = (byte)Mathf.Min(Mathf.RoundToInt(Vector2.Distance(_t.WorldPosition, transform.position)), 255);
        //int _dist = Mathf.Min(Mathf.RoundToInt(Vector2.Distance(_t.WorldPosition, transform.position)), 999);
        //ConcatLightUlong(_dist, ref _t.Lights_Distance);

        // intensity
        reportIntensity = (byte)Intensity;
        //ConcatLightUlong(Intensity, ref _t.Lights_Intensity);

        // how strongly this tile is hit by the light
        lightLevel = reportIntensity * (1 - (reportDistance / reportRange));
        cachedIntensities = ReportedIntensities[(_t.GridY * Grid.Instance.GridSizeX) + _t.GridX];
        cachedDistances = ReportedDistances[(_t.GridY * Grid.Instance.GridSizeX) + _t.GridX];
        cachedRanges = ReportedRanges[(_t.GridY * Grid.Instance.GridSizeX) + _t.GridX];
        overwriteChannel = 0;
        if (lightLevel > (cachedIntensities.r * (1 - (cachedDistances.r / cachedRanges.r))))
            overwriteChannel = 1;
        else if (lightLevel > (cachedIntensities.g * (1 - (cachedDistances.g / cachedRanges.g))))
            overwriteChannel = 2;
        else if (lightLevel > (cachedIntensities.b * (1 - (cachedDistances.b / cachedRanges.b))))
            overwriteChannel = 3;
        else if (lightLevel > (cachedIntensities.a * (1 - (cachedDistances.a / cachedRanges.a))))
            overwriteChannel = 4;

        if (overwriteChannel == 0)
            return;

        // angle (0-360)
        C = (Vector2)transform.position - _t.WorldPosition;
        tan = Mathf.Atan2(C.y, C.x);
        deg = Mathf.RoundToInt(90 + (tan * Mathf.Rad2Deg));
        if (deg < 0)
            deg += 360;
        reportAngle = (byte)(((float)deg / (float)360) * 255); // convert degrees to byte
        //ConcatLightUlong(_deg, ref _t.Lights_Angle);

        // color
        reportColor = LightColor;
        //ConcatLightUlong(LightColor, ref _t.Lights_Color);

        cachedAngles = ReportedAngles[(_t.GridY * Grid.Instance.GridSizeX) + _t.GridX];
        cachedColors = ReportedColors[(_t.GridY * Grid.Instance.GridSizeX) + _t.GridX];
        switch (overwriteChannel) {
            case 1:
                cachedAngles.r = reportAngle;
                cachedColors.r = reportColor;
                cachedRanges.r = reportRange;
                cachedDistances.r = reportDistance;
                cachedIntensities.r = reportIntensity;
                break;
            case 2:
                cachedAngles.g = reportAngle;
                cachedColors.g = reportColor;
                cachedRanges.g = reportRange;
                cachedDistances.g = reportDistance;
                cachedIntensities.g = reportIntensity;
                break;
            case 3:
                cachedAngles.b = reportAngle;
                cachedColors.b = reportColor;
                cachedRanges.b = reportRange;
                cachedDistances.b = reportDistance;
                cachedIntensities.b = reportIntensity;
                break;
            case 4:
                cachedAngles.a = reportAngle;
                cachedColors.a = reportColor;
                cachedRanges.a = reportRange;
                cachedDistances.a = reportDistance;
                cachedIntensities.a = reportIntensity;
                break;
        }
        ReportedAngles[(_t.GridY * Grid.Instance.GridSizeX) + _t.GridX] = cachedAngles;
        ReportedColors[(_t.GridY * Grid.Instance.GridSizeX) + _t.GridX] = cachedColors;
        ReportedRanges[(_t.GridY * Grid.Instance.GridSizeX) + _t.GridX] = cachedRanges;
        ReportedDistances[(_t.GridY * Grid.Instance.GridSizeX) + _t.GridX] = cachedDistances;
        ReportedIntensities[(_t.GridY * Grid.Instance.GridSizeX) + _t.GridX] = cachedIntensities;

        //_t.SetFloorColor(Color.magenta);
        //_t.SetWallColor(Color.magenta);
        ////stopthetrain = true;
        //Debug.Break();
    }

    //string concatThis;
    //int digits1;
    //int digits2;
    //void ConcatLightUlong(int val, ref ulong lightUlong) {
    //    concatThis = "";
    //    digits1 = MathfExtensions.Digits(lightUlong);
    //    if (digits1 == 1) {
    //        digits2 = MathfExtensions.Digits(val);
    //        if (digits2 == 1)
    //            concatThis = "100" + val.ToString();
    //        else if (digits2 == 2)
    //            concatThis = "10" + val.ToString();
    //        else if (digits2 == 3)
    //            concatThis = "1" + val.ToString();

    //        lightUlong = ulong.Parse(concatThis);
    //    }
    //    else if (digits1 >= 4) {
    //        digits2 = MathfExtensions.Digits(val);
    //        if (digits2 == 1)
    //            concatThis = "00" + val.ToString();
    //        else if (digits2 == 2)
    //            concatThis = "0" + val.ToString();
    //        else
    //            concatThis = val.ToString();

    //        lightUlong = ulong.Parse(lightUlong.ToString() + concatThis);
    //    }
    //    else
    //        throw new System.Exception("lightUlong somehow has 2-3 digits! D:");
    //}

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

            if (v1.Angle == v2.Angle) {
                return 1;
                // if(prevMag - v1.Pos.magnitude < prevMag - v2.Pos.magnitude)
                //     return -1;
                // else
                //     return 1;
            }
            //return (int)Mathf.Sign(v1.Pos.magnitude - v2.Pos.magnitude);

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
        // Hight performance for calculate angle on a vector (only for sort)
        // APROXIMATE VALUES -- NOT EXACT!! //
        float ax = Mathf.Abs(dx);
        float ay = Mathf.Abs(dy);
        float p = dy / (ax + ay);
        if (dx < 0)
            p = 2 - p;

        return p;
    }

    private List<BresenhamsLine.Overlap> cast;
    private Vector2 tilePos;
    private static CachedAssets.MovableCollider sShadowCollider = new CachedAssets.MovableCollider();
    private static CachedAssets.MovableCollider[] sExtraShadowColliders;
    private int currentIndex;
    Color col;
    private int iterationsInCast = 0;
    private int goal = 0;
    bool Gridcast(Vector2 _start, Vector2 _end, bool debug, out Vector2 _rayhit) {
        col = new Color(Random.value, Random.value, Random.value, 1);
        //Debug.Log(castcount);
        // find tiles along cast with a shadowcollider
        cast = BresenhamsLine.Gridcast(_start, _end);

        //for (int i = 0; i < cast.Count; i++) {
            //Color32 _col = Color.Lerp(Color.red, Color.blue, ((float)i / (float)cast.Count));
            // if (debug) {
            //     if (i == 0)
            //         _col = Color.green;
            //     else if (i == cast.Count - 1)
            //         _col = Color.yellow;

            //     cast[i].Tile.SetWallColor(_col);
            //     cast[i].Tile.SetFloorColor(_col);
            // }

            // if (cast[i].ExtraTiles != null){
            //     for (int j = 0; j < cast[i].ExtraTiles.Length; j++){
            //         if (debug){
            //             cast[i].ExtraTiles[j].SetWallColor(_col);
            //             cast[i].ExtraTiles[j].SetFloorColor(_col);
            //         }
            //     }    
            // }
            

            // Debug.DrawLine(
            //    new Vector2(t.WorldPosition.x - Tile.RADIUS, t.WorldPosition.y - Tile.RADIUS), 
            //    new Vector2(t.WorldPosition.x + Tile.RADIUS, t.WorldPosition.y - Tile.RADIUS),
            //    col, Time.deltaTime);

            // Debug.DrawLine(
            //   new Vector2(t.WorldPosition.x + Tile.RADIUS, t.WorldPosition.y - Tile.RADIUS),
            //   new Vector2(t.WorldPosition.x + Tile.RADIUS, t.WorldPosition.y + Tile.RADIUS),
            //   col, Time.deltaTime);

            // Debug.DrawLine(
            //   new Vector2(t.WorldPosition.x + Tile.RADIUS, t.WorldPosition.y + Tile.RADIUS),
            //   new Vector2(t.WorldPosition.x - Tile.RADIUS, t.WorldPosition.y + Tile.RADIUS),
            //   col, Time.deltaTime);

            // Debug.DrawLine(
            //   new Vector2(t.WorldPosition.x - Tile.RADIUS, t.WorldPosition.y + Tile.RADIUS),
            //   new Vector2(t.WorldPosition.x - Tile.RADIUS, t.WorldPosition.y - Tile.RADIUS),
            //   col, Time.deltaTime);
        //}
        col = new Color(Random.value, Random.value, Random.value, 1);


        if (cast.Count > 0) {
            CachedAssets.Instance.WallSets[0].GetShadowCollider(cast[0].Tile.ExactType, cast[0].Tile.Animator.CurrentFrame, cast[0].Tile.WorldPosition, ref sShadowCollider);
            // if (sShadowCollider != null){
            //     for (int i = 0; i < sShadowCollider.Paths.Length; i++){
            //         for (int j = 1; j < sShadowCollider.Paths[i].Vertices.Length; j++){
            //             Debug.DrawLine(sShadowCollider.WorldPosition + sShadowCollider.Paths[i].Vertices[j - 1], sShadowCollider.WorldPosition + sShadowCollider.Paths[i].Vertices[j], col, Time.deltaTime);
            //         }
            //     }
            // }

            if (cast[0].ExtraTiles != null){
                sExtraShadowColliders = new CachedAssets.MovableCollider[cast[0].ExtraTiles.Length];
                for (int i = 0; i < cast[0].ExtraTiles.Length; i++){
                    CachedAssets.Instance.WallSets[0].GetShadowCollider(cast[0].ExtraTiles[i].ExactType, cast[0].ExtraTiles[i].Animator.CurrentFrame, cast[0].ExtraTiles[i].WorldPosition, ref sExtraShadowColliders[i]);
                    // if (sExtraShadowColliders[i] == null)
                    //     continue;

                    // for (int j = 0; j < sExtraShadowColliders[i].Paths.Length; j++){
                    //     for (int k = 1; k < sExtraShadowColliders[i].Paths[j].Vertices.Length; k++){
                    //         Debug.DrawLine(sExtraShadowColliders[i].WorldPosition + sExtraShadowColliders[i].Paths[j].Vertices[k - 1], sExtraShadowColliders[i].WorldPosition + sExtraShadowColliders[i].Paths[j].Vertices[k], col, Time.deltaTime);
                    //     }
                    // }
                }
            }
        }

        Vector2 _curPos = _start;

        currentIndex = 0;
        iterationsInCast = 0;
        goal = Mathf.RoundToInt(((_end - _start).magnitude + 1) * Tile.RESOLUTION); // the distance in amount of tiles, multiplied by the amount of pixels across a tile
        while (iterationsInCast <= goal || currentIndex < cast.Count) { // don't actually need the second Bresenhams, so replaced with While (clean this up a bit later, okay? :P)
            _curPos = Vector2.Lerp(_start, _end, (float)iterationsInCast / (float)goal);
            iterationsInCast++;

            // float val = 0.0078125f; // (1 / 64) / 2 == radius of pixel
            // Debug.DrawLine(
            //    new Vector2(_curPos.x - val, _curPos.y - val),
            //    new Vector2(_curPos.x + val, _curPos.y - val),
            //    col, Time.deltaTime);

            // Debug.DrawLine(
            //    new Vector2(_curPos.x + val, _curPos.y - val),
            //    new Vector2(_curPos.x + val, _curPos.y + val),
            //    col, Time.deltaTime);

            // Debug.DrawLine(
            //    new Vector2(_curPos.x + val, _curPos.y + val),
            //    new Vector2(_curPos.x - val, _curPos.y + val),
            //    col, Time.deltaTime);

            // Debug.DrawLine(
            //    new Vector2(_curPos.x - val, _curPos.y + val),
            //    new Vector2(_curPos.x - val, _curPos.y - val),
            //    col, Time.deltaTime);

            // if pixel is closer to next collider, set next collider as current
            if (currentIndex < cast.Count - 1 && (cast[currentIndex + 1].Tile.WorldPosition - _curPos).magnitude <= (cast[currentIndex].Tile.WorldPosition - _curPos).magnitude) {
                currentIndex++;

                col = new Color(Random.value, Random.value, Random.value, 1);

                //Debug.Log(tiles[currentIndex].GridX + ", " + tiles[currentIndex].GridY + " (" + iterationsInCast + ")");
                // val *= 2; // (1 / 64) / 2 == radius of pixel
                // Debug.DrawLine(
                //   new Vector2(_curPos.x - val, _curPos.y - val),
                //   new Vector2(_curPos.x + val, _curPos.y - val),
                //   col, Time.deltaTime);

                // Debug.DrawLine(
                //   new Vector2(_curPos.x + val, _curPos.y - val),
                //   new Vector2(_curPos.x + val, _curPos.y + val),
                //   col, Time.deltaTime);

                // Debug.DrawLine(
                //   new Vector2(_curPos.x + val, _curPos.y + val),
                //   new Vector2(_curPos.x - val, _curPos.y + val),
                //   col, Time.deltaTime);

                // Debug.DrawLine(
                //   new Vector2(_curPos.x - val, _curPos.y + val),
                //   new Vector2(_curPos.x - val, _curPos.y - val),
                //   col, Time.deltaTime);

                CachedAssets.Instance.WallSets[0].GetShadowCollider(cast[currentIndex].Tile.ExactType, cast[currentIndex].Tile.Animator.CurrentFrame, cast[currentIndex].Tile.WorldPosition, ref sShadowCollider);
                // if (sShadowCollider != null) {
                //     for (int i = 0; i < sShadowCollider.Paths.Length; i++) {
                //         for (int j = 1; j < sShadowCollider.Paths[i].Vertices.Length; j++) {
                //             Debug.DrawLine(sShadowCollider.WorldPosition + sShadowCollider.Paths[i].Vertices[j - 1], sShadowCollider.WorldPosition + sShadowCollider.Paths[i].Vertices[j], col, Time.deltaTime);
                //         }
                //     }
                // }

                if (cast[currentIndex].ExtraTiles != null){
                    sExtraShadowColliders = new CachedAssets.MovableCollider[cast[currentIndex].ExtraTiles.Length];
                    for (int i = 0; i < cast[currentIndex].ExtraTiles.Length; i++){
                        CachedAssets.Instance.WallSets[0].GetShadowCollider(cast[currentIndex].ExtraTiles[i].ExactType, cast[currentIndex].ExtraTiles[i].Animator.CurrentFrame, cast[currentIndex].ExtraTiles[i].WorldPosition, ref sExtraShadowColliders[i]);
                        // if (sExtraShadowColliders[i] == null)
                        //     continue;

                        // for (int j = 0; j < sExtraShadowColliders[i].Paths.Length; j++){
                        //     for (int k = 1; k < sExtraShadowColliders[i].Paths[j].Vertices.Length; k++){
                        //         Debug.DrawLine(sExtraShadowColliders[i].WorldPosition + sExtraShadowColliders[i].Paths[j].Vertices[k - 1], sExtraShadowColliders[i].WorldPosition + sExtraShadowColliders[i].Paths[j].Vertices[k], col, Time.deltaTime);
                        //     }
                        // }
                    }
                }

                col = new Color(Random.value, Random.value, Random.value, 1);
            }
            // else if (currentIndex == cast.Count - 1/* && iterationsInCast / _goal >= 1*/)
            //     currentIndex++;

            float closest = 1000;
            if (sShadowCollider != null && sShadowCollider.OverlapPointOrAlmost(_curPos, out closest)) {
                //Debug.Log("Hit!".Color(Color.green));
                //Debug.Log("Bing!".Color(Color.green));
                //Debug.Log(p);
                _rayhit = _curPos;
                hits.Add(_curPos);
                hitsDistance.Add(closest);
                //Debug.DrawLine(_start, rayHit, Color.green, 1);

                //float val = 0.015625f; // (1 / 64) / 2 == radius of pixel
                // val *= 2;
                // Debug.DrawLine(
                //   new Vector2(_curPos.x - val, _curPos.y - val),
                //   new Vector2(_curPos.x + val, _curPos.y - val),
                //   Color.magenta, Time.deltaTime);

                // Debug.DrawLine(
                //   new Vector2(_curPos.x + val, _curPos.y - val),
                //   new Vector2(_curPos.x + val, _curPos.y + val),
                //   Color.magenta, Time.deltaTime);

                // Debug.DrawLine(
                //   new Vector2(_curPos.x + val, _curPos.y + val),
                //   new Vector2(_curPos.x - val, _curPos.y + val),
                //   Color.magenta, Time.deltaTime);

                // Debug.DrawLine(
                //   new Vector2(_curPos.x - val, _curPos.y + val),
                //   new Vector2(_curPos.x - val, _curPos.y - val),
                //   Color.magenta, Time.deltaTime);
                //Debug.Log("hit!".Color(Color.green));
                return true;
            }
            if (sExtraShadowColliders != null){
                for (int i = 0; i < sExtraShadowColliders.Length; i++){
                    if (sExtraShadowColliders[i] != null && sExtraShadowColliders[i].OverlapPointOrAlmost(_curPos, out closest)){
                        _rayhit = _curPos;
                        hits.Add(_curPos);
                        hitsDistance.Add(closest);

                        // Debug.DrawLine(
                        // new Vector2(_curPos.x - val, _curPos.y - val),
                        // new Vector2(_curPos.x + val, _curPos.y - val),
                        // Color.magenta, Time.deltaTime);

                        // Debug.DrawLine(
                        // new Vector2(_curPos.x + val, _curPos.y - val),
                        // new Vector2(_curPos.x + val, _curPos.y + val),
                        // Color.magenta, Time.deltaTime);

                        // Debug.DrawLine(
                        // new Vector2(_curPos.x + val, _curPos.y + val),
                        // new Vector2(_curPos.x - val, _curPos.y + val),
                        // Color.magenta, Time.deltaTime);

                        // Debug.DrawLine(
                        // new Vector2(_curPos.x - val, _curPos.y + val),
                        // new Vector2(_curPos.x - val, _curPos.y - val),
                        // Color.magenta, Time.deltaTime);
                        //Debug.Log("hit!".Color(Color.green));
                        return true;
                    }
                }
            }

            if ((float)iterationsInCast / (float)goal > 1/* && currentIndex >= cast.Count - 1*/) {
                //Debug.Log("Miss!".Color(Color.red));

                //Debug.Log("Bing!".Color(Color.red));
                //Debug.DrawLine(_start, _end, Color.red);
                //Debug.Break();
                _rayhit = Vector2.zero;
                nonHits.Add(_curPos);
                nonHitsDistance.Add(closest);
                nonHitsHadCollider.Add(sShadowCollider != null && sExtraShadowColliders != null && sExtraShadowColliders.Length > 0);
                return false;
            }
        }

        throw new System.Exception("Gridcast exceeded 100.000 iterations to reach target! Something is totally wrong or your cast is way too big! D:");
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

    // void OnDrawGizmos(){
    //     for (int i = 0; i < hits.Count; i++) { 
    //         Handles.Label(hits[i], "x");
    //         Handles.Label(hits[i] + new Vector2(0.1f, 0), "Hit: " + hitsDistance[i]);
    //     }
    //     for (int i = 0; i < nonHits.Count; i++){
    //         Handles.Label(nonHits[i], "x");
    //         Handles.Label(nonHits[i] + new Vector2(0.1f, 0), "No hit: " + nonHitsDistance[i] + ", collider: " + nonHitsHadCollider[i]);
    //     }
    // }
}

