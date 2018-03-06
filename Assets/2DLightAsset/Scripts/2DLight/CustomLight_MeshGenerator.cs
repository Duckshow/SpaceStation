using UnityEngine;
using System.Collections.Generic;
using System.Linq;
using UnityEditor; // for debug-gizmos only
using System;
using Utilities;

public class Verts {
    public float QuadrantAngle;
    public int Location; // 1 = left end point | 0 = middle | -1 = right endpoint
    public Vector2 LocalPos;
    public bool HasHitTarget;
}

public partial class CustomLight : MonoBehaviour {

    public Material lightMaterial;
    
	public LayerMask layer;
    [Range(4, 40)] public int lightSegments = 8;
    public Transform MeshTransform;

    //[HideInInspector] public PolygonCollider2D[] allMeshes; // Array for all of the meshes in our scene
    [HideInInspector]
    public List<Verts> allVertices = new List<Verts>(); // Array for all of the vertices in our meshes

	[NonSerialized] public Mesh MyMesh; // Mesh for our light mesh
    private new MeshRenderer renderer;



	private const string MESH_NAME = "Light Mesh";


    private const float MAGNITUDE_RANGE = 0.15f;
    private List<Verts> tempVerts = new List<Verts>();
    private RaycastHit2D rayHit;
    private Vector3 fromCast;
    private bool isEndpoint;
    private Vector2 rayOrigin;
    private float mag;
    private const float CHECK_POINT_LAST_RAY_OFFSET = 0.005f;
    private Vector2 rayCont;
    private int currentAngle;
    private int degreesPerSegment;
    private Verts vertex1;
    private Verts vertex2;
	private delegate void mSortList(List<Verts> _list);
	void SetVertices() {
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
			tempVerts.Clear();

			// get collider
			Tile _colliderTile = Grid.Instance.grid[_gGridPos.x, _gGridPos.y];
            PolygonCollider2D _collider = ObjectPooler.Instance.GetPooledObject<PolygonCollider2D>(_colliderTile.ExactType);
            _collider.transform.position = _colliderTile.WorldPosition;

            
            for (int pIndex = 0; pIndex < _collider.pathCount; pIndex++){ 					// iterate over collider-paths
                for (int vIndex = 0; vIndex < _collider.GetPath(pIndex).Length; vIndex++){ 	// iterate over path-vertices
                    Vector2 _targetPosWorld = (Vector2)_collider.transform.position + _collider.GetPath(pIndex)[vIndex];
                    Verts _newVertex = new Verts();

                    if (Gridcast(myWorldPos, _targetPosWorld, out rayHit)) {
                        _newVertex.LocalPos = rayHit.point;
						_newVertex.HasHitTarget = false;
                    }
                    else {
                        _newVertex.LocalPos = _targetPosWorld;
                        _newVertex.HasHitTarget = true;
                    }

                    _newVertex.LocalPos = transform.InverseTransformPoint(_newVertex.LocalPos);	// to local
                    _newVertex.QuadrantAngle = GetQuadrantAngle(_newVertex.LocalPos.x, _newVertex.LocalPos.y);

                    //--Add verts to the main list
                    if (_newVertex.LocalPos.magnitude <= Radius)
                        tempVerts.Add(_newVertex);
                }
            }

			// return collider
            _collider.GetComponent<PoolerObject>().ReturnToPool();

            // get the two edge-most vertices and continue gridcasting (bc they're corners and don't stop light)
            if (tempVerts.Count > 0) {
				SortVerticesByAngle(tempVerts);
				
				int _minIndex = 0;
				int _maxIndex = tempVerts.Count - 1;
                Verts _firstVertex = tempVerts[_minIndex];
				Verts _lastVertex = tempVerts[_maxIndex];
                
				_firstVertex.Location = 1;
                _lastVertex.Location = -1;
                allVertices.AddRange(tempVerts);

				if(_firstVertex.HasHitTarget)	ContinueGridcast(transform.TransformPoint(_firstVertex.LocalPos));
				if(_lastVertex.HasHitTarget) 	ContinueGridcast(transform.TransformPoint(_lastVertex.LocalPos));
				
				tempVerts[_minIndex] = _firstVertex;
				tempVerts[_maxIndex] = _lastVertex;
			}
        }

        currentAngle = 0;
        degreesPerSegment = 360 / lightSegments;
        for (int i = 0; i < lightSegments; i++) {
            currentAngle = degreesPerSegment * i;
            if (currentAngle == 360)
                currentAngle = 0;

            Verts _newVertex = new Verts();
            _newVertex.LocalPos = Radius * new Vector3((LightManager.SinCosTable.SinArray[currentAngle]), (LightManager.SinCosTable.CosArray[currentAngle]), 0); // in degrees (previous calculate)
            _newVertex.QuadrantAngle = GetQuadrantAngle(_newVertex.LocalPos.x, _newVertex.LocalPos.y);

            if(Gridcast(myWorldPos, myWorldPos + _newVertex.LocalPos, out rayHit)){
                _newVertex.LocalPos = transform.InverseTransformPoint(rayHit.point);
            }

            allVertices.Add(_newVertex);
        }

	    SortVerticesByAngle(allVertices);
        CorrectSortingErrors();
	}
	void SortVerticesByAngle (List<Verts> _list) {
		_list.Sort((item1, item2) => (item2.QuadrantAngle.CompareTo(item1.QuadrantAngle)));
	}
	void ContinueGridcast(Vector2 vertexWorldPos) {
		Vector2 diff = vertexWorldPos - myWorldPos;
		Vector2 rayOrigin = vertexWorldPos + (diff * CHECK_POINT_LAST_RAY_OFFSET);
		Vector2 rayEnd = rayOrigin + diff.normalized * (Radius - diff.magnitude);
		Vector2 hitPos; 

        bool _hit = Gridcast(rayOrigin, rayEnd, out rayHit);
		if (_hit)   hitPos = rayHit.point;
		else        hitPos = myWorldPos + (diff.normalized * Radius);

		Verts _newVertex = new Verts();
		_newVertex.LocalPos = transform.InverseTransformPoint(hitPos);	// to local
		_newVertex.QuadrantAngle = GetQuadrantAngle(_newVertex.LocalPos.x, _newVertex.LocalPos.y);
		allVertices.Add(_newVertex);
	}
    float GetQuadrantAngle(float x, float y) { 
        // approximate but high performance way of calculating angle. Range: -1 to 2.99
        float angle = y / (Mathf.Abs(x) + Mathf.Abs(y));
        if (x < 0) angle = 2 - angle;
        return angle;
    }

    private const float SORTING_ERROR_CORRECTION_TOLERANCE = 0.00001f;
    void CorrectSortingErrors(){
        /* Since ContinueGridcast ends up putting two vertices on the same angle, 
        when the vertices are sorted these can get mixed up. So let's fix that.*/

        for (int i = 0; i < allVertices.Count - 1; i++) {
            vertex1 = allVertices[i];
            vertex2 = allVertices[i + 1];

            if (vertex1.QuadrantAngle >= vertex2.QuadrantAngle - SORTING_ERROR_CORRECTION_TOLERANCE && vertex1.QuadrantAngle <= vertex2.QuadrantAngle + SORTING_ERROR_CORRECTION_TOLERANCE) {
                if (vertex2.Location == -1) { // Right Ray
                    if (vertex1.LocalPos.sqrMagnitude > vertex2.LocalPos.sqrMagnitude) {
                        allVertices[i] = vertex2;
                        allVertices[i + 1] = vertex1;
                    }
                }

                if (vertex1.Location == 1) { // Left Ray
                    if (vertex1.LocalPos.sqrMagnitude < vertex2.LocalPos.sqrMagnitude) {
                        allVertices[i] = vertex2;
                        allVertices[i + 1] = vertex1;
                    }
                }
            }
        }
    }

    void RenderLightMesh() { adpmw // continue here. Let's finish this.
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

	private const float GRIDCAST_TOLERANCE = 0.05f;
    bool Gridcast(Vector2 _start, Vector2 _end, out RaycastHit2D _rayhit){
		_rayhit = Physics2D.Linecast(_start, _end);
        return _rayhit.collider != null && (_end - _rayhit.point).magnitude > GRIDCAST_TOLERANCE;
    }
}

