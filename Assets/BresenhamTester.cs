using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BresenhamTester : MonoBehaviour {

	public Vector2 StartPos;
	public Vector2 Angle;
	public float Length;
	public int Resolution;
	public int Resolution2;

	public GameObject Colliders;

	private float resolution;
	private Vector2 resPos;


	void Start(){
		int y = 0;
		for(int i = 0; i < Colliders.transform.childCount; i++){
			Colliders.transform.GetChild(i).transform.localPosition = new Vector3(i % 30, y, 0);
			if(i % 30 == 0)
				y++;
		}
	}

	[EasyButtons.Button]
	public void DrawLine(){
		
		Debug.DrawLine(StartPos, (StartPos + (Angle * Length)), Color.red);

		resolution = 1 / (float)Resolution;
		BresenhamsLine bres = new BresenhamsLine(StartPos, (StartPos + (Angle * Length)), Resolution);
		foreach(Vector2 _p in bres){
			resPos = new Vector2(_p.x * resolution, _p.y * resolution);
			Debug.DrawLine(new Vector2(resPos.x - (resolution * 0.5f), resPos.y - (resolution * 0.5f)), new Vector2(resPos.x + (resolution * 0.5f), resPos.y - (resolution * 0.5f)), Color.cyan);
			Debug.DrawLine(new Vector2(resPos.x + (resolution * 0.5f), resPos.y - (resolution * 0.5f)), new Vector2(resPos.x + (resolution * 0.5f), resPos.y + (resolution * 0.5f)), Color.cyan);
			Debug.DrawLine(new Vector2(resPos.x + (resolution * 0.5f), resPos.y + (resolution * 0.5f)), new Vector2(resPos.x - (resolution * 0.5f), resPos.y + (resolution * 0.5f)), Color.cyan);
			Debug.DrawLine(new Vector2(resPos.x - (resolution * 0.5f), resPos.y + (resolution * 0.5f)), new Vector2(resPos.x - (resolution * 0.5f), resPos.y - (resolution * 0.5f)), Color.cyan);
		}

		resolution = 1 / (float)Resolution2;
		bres = new BresenhamsLine(StartPos, (StartPos + (Angle * Length)), Resolution2);
		foreach(Vector2 _p in bres){
			Debug.Log(resPos);
			resPos = new Vector2(_p.x * resolution, _p.y * resolution);
			Debug.DrawLine(new Vector2(resPos.x - (resolution * 0.5f), resPos.y - (resolution * 0.5f)), new Vector2(resPos.x + (resolution * 0.5f), resPos.y - (resolution * 0.5f)), Color.green);
			Debug.DrawLine(new Vector2(resPos.x + (resolution * 0.5f), resPos.y - (resolution * 0.5f)), new Vector2(resPos.x + (resolution * 0.5f), resPos.y + (resolution * 0.5f)), Color.green);
			Debug.DrawLine(new Vector2(resPos.x + (resolution * 0.5f), resPos.y + (resolution * 0.5f)), new Vector2(resPos.x - (resolution * 0.5f), resPos.y + (resolution * 0.5f)), Color.green);
			Debug.DrawLine(new Vector2(resPos.x - (resolution * 0.5f), resPos.y + (resolution * 0.5f)), new Vector2(resPos.x - (resolution * 0.5f), resPos.y - (resolution * 0.5f)), Color.green);
		}
	}

	List<Collider2D> results = new List<Collider2D>();
	public LayerMask mask;
	[EasyButtons.Button]
	public void ColliderTest(){
		results.Clear();

		for(int i = 0; i < Colliders.transform.childCount; i++){
			if((Colliders.transform.GetChild(i).position - transform.position).magnitude < Length)
				results.Add(Colliders.transform.GetChild(i).GetComponent<Collider2D>());
		}

		
		for(int i = 0; i < results.Count; i++){
			results[i].gameObject.SetActive(true);
		}
	}
}