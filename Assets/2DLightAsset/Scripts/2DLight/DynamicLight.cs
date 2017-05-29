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
	
public class Verts{
    public float Angle;
    public int Location; // 1 = left end point | 0 = middle | -1 = right endpoint
    public Vector3 Pos;
    public bool Endpoint;
}
	
public class DynamicLight : MonoBehaviour {
	
	public string version = "1.0.5"; //release date 09/01/2017
	public Material lightMaterial;
	public float lightRadius = 20f;
	public LayerMask layer;
	[Range(4,20)]
	public int lightSegments = 8;

	[HideInInspector] public PolygonCollider2D[] allMeshes; // Array for all of the meshes in our scene
	[HideInInspector] public List<Verts> allVertices = new List<Verts>(); // Array for all of the vertices in our meshes

	private Mesh lightMesh; // Mesh for our light mesh
    private new MeshRenderer renderer;


	void Start () {
		SinCosTable.InitSinCos();

        MeshFilter meshFilter = (MeshFilter)gameObject.AddComponent(typeof(MeshFilter));		// Add a Mesh Filter component to the light game object so it can take on a form
		renderer = gameObject.AddComponent(typeof(MeshRenderer)) as MeshRenderer;	            // Add a Mesh Renderer component to the light game object so the form can become visible
		renderer.sharedMaterial = lightMaterial;												// Add the specified material
		lightMesh = new Mesh();																	// create a new mesh for our light mesh
		meshFilter.mesh = lightMesh;															// Set this newly created mesh to the mesh filter
		lightMesh.name = "Light Mesh";                                  						// Give it a name
		lightMesh.MarkDynamic ();                                                               // Set mesh as dynamic
    }

    [EasyButtons.Button]
    public void UpdateLight(){
		GetAllMeshes();
		SetLight ();
		RenderLightMesh ();
		ResetBounds ();
    }

	void Update(){
		GetAllMeshes();
		SetLight ();
		RenderLightMesh ();
		ResetBounds ();
	}
		
	void GetAllMeshes(){
		//-- Step 1: obtain all active meshes in the scene --//
		//---------------------------------------------------------------------//

		Collider2D [] allColl2D = Physics2D.OverlapCircleAll(transform.position, lightRadius, layer);
		allMeshes = new PolygonCollider2D[allColl2D.Length];
		for (int i=0; i<allColl2D.Length; i++)
			allMeshes[i] = (PolygonCollider2D)allColl2D[i];
	}

    private bool sortAngles = false;
    private bool lows = false; // check si hay menores a -0.5
    private bool highs = false; // check si hay mayores a 2.0
    private float magRange = 0.15f;
    private List<Verts> tempVerts = new List<Verts>();
    private PolygonCollider2D polCollider;
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
    private RaycastHit2D rayCont;
    private Vector3 hitPos;
    private Vector2 newDir;
    private Verts vL;
    private int theta;
    private int amount;
    private float rangeAngleComparision;
    private Verts vertex1;
    private Verts vertex2;
    void SetLight () {
		allVertices.Clear();// Since these lists are populated every frame, clear them first to prevent overpopulation
			
		//--Step 2: Obtain vertices for each mesh --//
		//---------------------------------------------------------------------//
			
		magRange = 0.15f;
		tempVerts.Clear();
			
		for (int i = 0; i < allMeshes.Length; i++) {
			tempVerts.Clear();
			polCollider = allMeshes[i];
			// the following variables used to fix sorting bug
			// the calculated angles are in mixed quadrants (1 and 4)
			lows = false; // check for minors at -0.5
			highs = false; // check for majors at 2.0
				
			if(((1 << polCollider.transform.gameObject.layer) & layer) != 0){ // check if collider's layer is in the current layermask (I think? :c)
				for (int j = 0; j < polCollider.GetTotalPointCount(); j++) {	// ...and for every vertex we have of each collider...
					v = new Verts();
						
                    // Convert vertex to world space
					worldPoint = polCollider.transform.TransformPoint(polCollider.points[j]);

                    //rayHit = Physics2D.Raycast(transform.position, worldPoint - transform.position, (worldPoint - transform.position).magnitude, layer);
                    rayHit = Physics2D.Linecast(transform.position, worldPoint, layer);
                    if (rayHit){
						v.Pos = rayHit.point;
						if(worldPoint.sqrMagnitude >= (rayHit.point.sqrMagnitude - magRange) && worldPoint.sqrMagnitude <= (rayHit.point.sqrMagnitude + magRange))
							v.Endpoint = true;
					}
                    else {
						v.Pos =  worldPoint;
						v.Endpoint = true;
					}
						
					Debug.DrawLine(transform.position, v.Pos, Color.white);	
						
					//--Convert To local space for build mesh (mesh craft only in local vertex)
					v.Pos = transform.InverseTransformPoint(v.Pos); // optimization: could we do the Linecast in local space instead?
					//--Calculate angle
					v.Angle = GetVectorAngle(true,v.Pos.x, v.Pos.y);
						
					// -- bookmark if an angle is lower than 0 or higher than 2f --//
					//-- helper method for fix bug on shape located in 2 or more quadrants
					if(v.Angle < 0f)
						lows = true;
						
					if(v.Angle > 2f)
						highs = true;
						
					//--Add verts to the main list
					if((v.Pos).sqrMagnitude <= lightRadius*lightRadius)
						tempVerts.Add(v);
						
					if(sortAngles == false)
						sortAngles = true;
				}
			}
				
			// Identify the endpoints (left and right)
			if(tempVerts.Count > 0){
				SortList(tempVerts); // sort first
					
				posLowAngle = 0; // save the indice of left ray
				posHighAngle = 0; // same last in right side
					
				if(highs == true && lows == true){  //-- FIX BUG OF SORTING CUANDRANT 1-4 --//
					lowestAngle = -1f; //tempVerts[0].angle; // init with first data
					highestAngle = tempVerts[0].Angle;
						
					for(int j = 0; j < tempVerts.Count; j++){
						if(tempVerts[j].Angle < 1f && tempVerts[j].Angle > lowestAngle){
							lowestAngle = tempVerts[j].Angle;
							posLowAngle = j;
						}
						if(tempVerts[j].Angle > 2f && tempVerts[j].Angle < highestAngle){
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
				for (int r = 0; r < 2; r++){
					//-- Cast a ray in same direction continuos mode, start a last point of last ray --//
					fromCast = new Vector3();
					isEndpoint = false;
						
					if(r == 0){
						fromCast = transform.TransformPoint(tempVerts[posLowAngle].Pos);
						isEndpoint = tempVerts[posLowAngle].Endpoint;
					}
                    else if(r == 1){
						fromCast = transform.TransformPoint(tempVerts[posHighAngle].Pos);
						isEndpoint = tempVerts[posHighAngle].Endpoint;
					}
						
					if(isEndpoint == true){
						dir = fromCast - transform.position;
						mag = lightRadius;// - fromCast.magnitude;
						from = (Vector2)fromCast + (dir * CHECK_POINT_LAST_RAY_OFFSET);
							
						rayCont = Physics2D.Raycast(from, dir, mag, layer);
						if(rayCont)
							hitPos = rayCont.point;
						else{
							newDir = transform.InverseTransformDirection(dir);	//local p
							hitPos = (Vector2)transform.TransformPoint( newDir.normalized * mag); //world p
						}
							
						if((hitPos - transform.position).sqrMagnitude > (lightRadius * lightRadius)){
							dir = transform.InverseTransformDirection(dir);	//local p
							hitPos = transform.TransformPoint(dir.normalized * mag);
						}
							
						Debug.DrawLine(fromCast, hitPos, Color.green);	
							
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
		for (int i = 0; i < lightSegments; i++)  {
			theta = amount * i;
			if(theta == 360)
                theta = 0;
				
			v = new Verts();
			//v.pos = new Vector3((Mathf.Sin(theta)), (Mathf.Cos(theta)), 0); // in radians low performance
			v.Pos = new Vector3((SinCosTable.sSinArray[theta]), (SinCosTable.sCosArray[theta]), 0); // in degrees (previous calculate)
			v.Angle = GetVectorAngle(true,v.Pos.x, v.Pos.y);
			v.Pos *= lightRadius;
			v.Pos += transform.position;
				
			rayHit = Physics2D.Raycast(transform.position, v.Pos - transform.position, lightRadius, layer);
			if (!rayHit) {
				v.Pos = transform.InverseTransformPoint(v.Pos);
				allVertices.Add(v);
			}
		}
			
		//-- Step 4: Sort each vertex by angle (along sweep ray 0 - 2PI)--//
		//---------------------------------------------------------------------//
		if (sortAngles == true) {
			SortList(allVertices);
		}
		//-----------------------------------------------------------------------------
			
			
		//--auxiliary step (change order vertices close to light first in position when has same direction) --//
		rangeAngleComparision = 0.00001f;
		for(int i = 0; i < allVertices.Count - 1; i++){
				
			vertex1 = allVertices[i];
			vertex2 = allVertices[i + 1];
				
			// -- Compare the local angle of each vertex and decide if we have to make an exchange-- //
			if(vertex1.Angle >= vertex2.Angle - rangeAngleComparision && vertex1.Angle <= vertex2.Angle + rangeAngleComparision){
				if(vertex2.Location == -1){ // Right Ray
					if(vertex1.Pos.sqrMagnitude > vertex2.Pos.sqrMagnitude){
						allVertices[i] = vertex2;
						allVertices[i+1] = vertex1;
					}
				}
					
				// ALREADY DONE!!
				if(vertex1.Location == 1){ // Left Ray
					if(vertex1.Pos.sqrMagnitude < vertex2.Pos.sqrMagnitude){
						allVertices[i] = vertex2;
						allVertices[i+1] = vertex1;
					}
				}
			}
		}
			
	}

    private Vector3[] initVerticesMeshLight;
    private Vector2[] uvs;
    private int index;
    private int[] triangles;
    void RenderLightMesh(){
		//-- Step 5: fill the mesh with vertices--//
		//---------------------------------------------------------------------//
			
		initVerticesMeshLight = new Vector3[allVertices.Count + 1];
		initVerticesMeshLight[0] = Vector3.zero;
			
		for (int i = 0; i < allVertices.Count; i++)
			initVerticesMeshLight [i + 1] = allVertices[i].Pos;
			
		lightMesh.Clear ();
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
				
			if(i == (allVertices.Count*3)-3) //-- if is the last vertex (one loop)
                triangles[i + 2] = 1;
            else // next next vertex	
                triangles[i + 2] = index+2; 
				
			index++;
		}
			
		lightMesh.triangles = triangles;												
		//lightMesh.RecalculateNormals();
		renderer.sharedMaterial = lightMaterial;
	}
    private Bounds bounds;
    void ResetBounds() {
        bounds = lightMesh.bounds;
        bounds.center = Vector3.zero;
        lightMesh.bounds = bounds;
    }
    void SortList(List<Verts> list){
		list.Sort((item1, item2) => (item2.Angle.CompareTo(item1.Angle)));
	}
		
	void DrawLinePerVertex(){
		for (int i = 0; i < allVertices.Count; i++)
		{
			if (i < (allVertices.Count -1))
				Debug.DrawLine(allVertices [i].Pos , allVertices [i+1].Pos, new Color(i*0.02f, i*0.02f, i*0.02f));
			else
				Debug.DrawLine(allVertices [i].Pos , allVertices [0].Pos, new Color(i*0.02f, i*0.02f, i*0.02f));
		}
	}

    private float angle;
	float GetVectorAngle(bool pseudo, float x, float y){
        angle = 0;
		if(pseudo == true)
            angle = PseudoAngle(x, y);
		else
            angle = Mathf.Atan2(y, x);

        return angle;
	}
		
	float PseudoAngle(float dx, float dy){
		// Hight performance for calculate angle on a vector (only for sort)
		// APROXIMATE VALUES -- NOT EXACT!! //
		float ax = Mathf.Abs (dx);
		float ay = Mathf.Abs (dy);
		float p = dy / (ax + ay);
		if (dx < 0)
			p = 2 - p;

		return p;
	}

	private List<Tile> tilesInCast = new List<Tile>();
	private BresenhamsLine cast;
	void Gridcast(Vector2 _start, Vector2 _end){
		cast = new BresenhamsLine(_start, _end, 1);
		foreach(Vector2 _tilePos in cast)
			tilesInCast.Add(Grid.Instance.GetTileFromWorldPoint(_tilePos));
	}
}

