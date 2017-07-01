using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BresenhamTester : MonoBehaviour {

	public Vector2 StartPos;
	public Vector2 Angle;
	public float Length;
	public int Resolution;
	public int Resolution2;

	private float resolution;
	private Vector2 resPos;


	[EasyButtons.Button]
	public void DrawLine(){
		

		resolution = 1 / (float)Resolution;
        //BresenhamsLine bres = new BresenhamsLine(StartPos, (StartPos + (Angle * Length)), Resolution);
        //foreach(Vector2 _p in bres){
        //	resPos = new Vector2(_p.x, _p.y);
        //          Debug.Log(resPos);
        //	Debug.DrawLine(new Vector2(resPos.x - (resolution * 0.5f), resPos.y - (resolution * 0.5f)), new Vector2(resPos.x + (resolution * 0.5f), resPos.y - (resolution * 0.5f)), Color.cyan);
        //	Debug.DrawLine(new Vector2(resPos.x + (resolution * 0.5f), resPos.y - (resolution * 0.5f)), new Vector2(resPos.x + (resolution * 0.5f), resPos.y + (resolution * 0.5f)), Color.cyan);
        //	Debug.DrawLine(new Vector2(resPos.x + (resolution * 0.5f), resPos.y + (resolution * 0.5f)), new Vector2(resPos.x - (resolution * 0.5f), resPos.y + (resolution * 0.5f)), Color.cyan);
        //	Debug.DrawLine(new Vector2(resPos.x - (resolution * 0.5f), resPos.y + (resolution * 0.5f)), new Vector2(resPos.x - (resolution * 0.5f), resPos.y - (resolution * 0.5f)), Color.cyan);
        //}

        //resolution = 1 / (float)Resolution2;
        //bres = new BresenhamsLine(StartPos, (StartPos + (Angle * Length)), Resolution2);
        //foreach(Vector2 _p in bres){
        //	Debug.Log(resPos);
        //	resPos = new Vector2(_p.x, _p.y);
        //	Debug.DrawLine(new Vector2(resPos.x - (resolution * 0.5f), resPos.y - (resolution * 0.5f)), new Vector2(resPos.x + (resolution * 0.5f), resPos.y - (resolution * 0.5f)), Color.green);
        //	Debug.DrawLine(new Vector2(resPos.x + (resolution * 0.5f), resPos.y - (resolution * 0.5f)), new Vector2(resPos.x + (resolution * 0.5f), resPos.y + (resolution * 0.5f)), Color.green);
        //	Debug.DrawLine(new Vector2(resPos.x + (resolution * 0.5f), resPos.y + (resolution * 0.5f)), new Vector2(resPos.x - (resolution * 0.5f), resPos.y + (resolution * 0.5f)), Color.green);
        //	Debug.DrawLine(new Vector2(resPos.x - (resolution * 0.5f), resPos.y + (resolution * 0.5f)), new Vector2(resPos.x - (resolution * 0.5f), resPos.y - (resolution * 0.5f)), Color.green);
        //}

        Vector2 start = StartPos + new Vector2(-1, 1);
        //Debug.DrawLine(start, (start + (Angle * Length)), Color.red);
        List<Vector2> newBres;// = BresenhamsLine.DDASuperCover(start, (start + (Angle * Length)));
        //for (int i = 0; i < newBres.Count; i++) {
        //    Debug.DrawLine(new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), Color.cyan);
        //    Debug.DrawLine(new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), Color.cyan);
        //    Debug.DrawLine(new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), Color.cyan);
        //    Debug.DrawLine(new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), Color.cyan);
        //}

        start = StartPos + new Vector2(1, 1);
        Debug.DrawLine(start, (start + (new Vector2(Angle.x * 1, Angle.y * 1) * Length)), Color.red);
        newBres = BresenhamsLine.DDASuperCover(start, (start + ((new Vector2(Angle.x * 1, Angle.y * 1)) * Length)));
        for (int i = 0; i < newBres.Count; i++) {
            Debug.DrawLine(new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), Color.green);
            Debug.DrawLine(new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), Color.green);
            Debug.DrawLine(new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), Color.green);
            Debug.DrawLine(new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), Color.green);
        }

        //start = StartPos + new Vector2(-1, -1);
        //Debug.DrawLine(start, (start + (new Vector2(Angle.x * 1, Angle.y * -1) * Length)), Color.red);
        //newBres = BresenhamsLine.DDASuperCover(start, (start + ((new Vector2(Angle.x * 1, Angle.y * -1)) * Length)));
        //for (int i = 0; i < newBres.Count; i++) {
        //    Debug.DrawLine(new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), Color.red);
        //    Debug.DrawLine(new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), Color.red);
        //    Debug.DrawLine(new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), Color.red);
        //    Debug.DrawLine(new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), Color.red);
        //}

        //start = StartPos + new Vector2(1, -1);
        //Debug.DrawLine(start, (start + (new Vector2(Angle.x * -1, Angle.y * -1) * Length)), Color.red);
        //newBres = BresenhamsLine.DDASuperCover(start, (start + ((new Vector2(Angle.x * -1, Angle.y * -1)) * Length)));
        //for (int i = 0; i < newBres.Count; i++) {
        //    Debug.DrawLine(new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), Color.magenta);
        //    Debug.DrawLine(new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), Color.magenta);
        //    Debug.DrawLine(new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), Color.magenta);
        //    Debug.DrawLine(new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), Color.magenta);
        //}
    }

    // List<Collider2D> results = new List<Collider2D>();
    // public LayerMask mask;
    // [EasyButtons.Button]
    // public void ColliderTest(){
    // 	results.Clear();

    // 	for(int i = 0; i < Colliders.transform.childCount; i++){
    // 		if((Colliders.transform.GetChild(i).position - transform.position).magnitude < Length)
    // 			results.Add(Colliders.transform.GetChild(i).GetComponent<Collider2D>());
    // 	}


    // 	for(int i = 0; i < results.Count; i++){
    // 		results[i].gameObject.SetActive(true);
    // 	}
    // }
}