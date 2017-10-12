using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BresenhamTester : MonoBehaviour {

	public Vector2 StartPos;
    public Vector2 EndPos;

    [Space]
    public Vector2 Angle;
    public bool UseAngle = false;
    
    [Space]
    public float Length;
	public int Resolution;
	public int Resolution2;

    private Vector2 end;
    private float resolution;
	private Vector2 resPos;


	[EasyButtons.Button]
	public void DrawLine(){
        Vector2 start = StartPos;
        end = UseAngle ? (start + (Angle * Length)) : EndPos;
        resolution = 1 / (float)Resolution;

        Debug.DrawLine(start, end, Color.red);
        List<BresenhamsLine.OverlapWithTiles> newBres = BresenhamsLine.Gridcast(start, end);
        for (int i = 0; i < newBres.Count; i++) {
            Debug.DrawLine(new Vector2(newBres[i].TilePos.x - (resolution * 0.5f), newBres[i].TilePos.y - (resolution * 0.5f)), new Vector2(newBres[i].TilePos.x + (resolution * 0.5f), newBres[i].TilePos.y - (resolution * 0.5f)), Color.cyan);
            Debug.DrawLine(new Vector2(newBres[i].TilePos.x + (resolution * 0.5f), newBres[i].TilePos.y - (resolution * 0.5f)), new Vector2(newBres[i].TilePos.x + (resolution * 0.5f), newBres[i].TilePos.y + (resolution * 0.5f)), Color.cyan);
            Debug.DrawLine(new Vector2(newBres[i].TilePos.x + (resolution * 0.5f), newBres[i].TilePos.y + (resolution * 0.5f)), new Vector2(newBres[i].TilePos.x - (resolution * 0.5f), newBres[i].TilePos.y + (resolution * 0.5f)), Color.cyan);
            Debug.DrawLine(new Vector2(newBres[i].TilePos.x - (resolution * 0.5f), newBres[i].TilePos.y + (resolution * 0.5f)), new Vector2(newBres[i].TilePos.x - (resolution * 0.5f), newBres[i].TilePos.y - (resolution * 0.5f)), Color.cyan);
            
            if (newBres[i].ExtraTilePositions != null){
                for (int j = 0; j < newBres[i].ExtraTilePositions.Length; j++){
                    Debug.DrawLine(new Vector2(newBres[i].ExtraTilePositions[j].x - (resolution * 0.5f), newBres[i].ExtraTilePositions[j].y - (resolution * 0.5f)), new Vector2(newBres[i].ExtraTilePositions[j].x + (resolution * 0.5f), newBres[i].ExtraTilePositions[j].y - (resolution * 0.5f)), Color.cyan);
                    Debug.DrawLine(new Vector2(newBres[i].ExtraTilePositions[j].x + (resolution * 0.5f), newBres[i].ExtraTilePositions[j].y - (resolution * 0.5f)), new Vector2(newBres[i].ExtraTilePositions[j].x + (resolution * 0.5f), newBres[i].ExtraTilePositions[j].y + (resolution * 0.5f)), Color.cyan);
                    Debug.DrawLine(new Vector2(newBres[i].ExtraTilePositions[j].x + (resolution * 0.5f), newBres[i].ExtraTilePositions[j].y + (resolution * 0.5f)), new Vector2(newBres[i].ExtraTilePositions[j].x - (resolution * 0.5f), newBres[i].ExtraTilePositions[j].y + (resolution * 0.5f)), Color.cyan);
                    Debug.DrawLine(new Vector2(newBres[i].ExtraTilePositions[j].x - (resolution * 0.5f), newBres[i].ExtraTilePositions[j].y + (resolution * 0.5f)), new Vector2(newBres[i].ExtraTilePositions[j].x - (resolution * 0.5f), newBres[i].ExtraTilePositions[j].y - (resolution * 0.5f)), Color.cyan);
                }
            }
        }

        // start = StartPos + new Vector2(1, 1);
        // Debug.DrawLine(start, (start + (new Vector2(Angle.x * -1, Angle.y * 1) * Length)), Color.red);
        // newBres = BresenhamsLine.Gridcast(start, (start + ((new Vector2(Angle.x * -1, Angle.y * 1)) * Length)));
        // for (int i = 0; i < newBres.Count; i++) {
        //     Debug.DrawLine(new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), Color.green);
        //     Debug.DrawLine(new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), Color.green);
        //     Debug.DrawLine(new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), Color.green);
        //     Debug.DrawLine(new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), Color.green);
        // }

        // start = StartPos + new Vector2(-1, -1);
        // Debug.DrawLine(start, (start + (new Vector2(Angle.x * 1, Angle.y * -1) * Length)), Color.red);
        // newBres = BresenhamsLine.Gridcast(start, (start + ((new Vector2(Angle.x * 1, Angle.y * -1)) * Length)));
        // for (int i = 0; i < newBres.Count; i++) {
        //    Debug.DrawLine(new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), Color.red);
        //    Debug.DrawLine(new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), Color.red);
        //    Debug.DrawLine(new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), Color.red);
        //    Debug.DrawLine(new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), Color.red);
        // }

        // start = StartPos + new Vector2(1, -1);
        // Debug.DrawLine(start, (start + (new Vector2(Angle.x * -1, Angle.y * -1) * Length)), Color.red);
        // newBres = BresenhamsLine.Gridcast(start, (start + ((new Vector2(Angle.x * -1, Angle.y * -1)) * Length)));
        // for (int i = 0; i < newBres.Count; i++) {
        //    Debug.DrawLine(new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), Color.magenta);
        //    Debug.DrawLine(new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), Color.magenta);
        //    Debug.DrawLine(new Vector2(newBres[i].x + (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), Color.magenta);
        //    Debug.DrawLine(new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y + (resolution * 0.5f)), new Vector2(newBres[i].x - (resolution * 0.5f), newBres[i].y - (resolution * 0.5f)), Color.magenta);
        // }
    }
    [EasyButtons.Button]
    public void DrawRandomLine()
    {
        resolution = 1 / (float)Resolution;

        Vector2 start = StartPos + new Vector2(Random.value, Random.value);
        Vector2 angle = new Vector2(Random.value, Random.value);
        if(Random.value > 0.5f)
            angle.x *= -1;
        if (Random.value > 0.5f)
            angle.y *= -1;

        Debug.Log("Line: " + start + ", with angle: " + angle);
        Debug.DrawLine(start, (start + (angle * Length)), Color.red);
        List<BresenhamsLine.OverlapWithTiles> newBres = BresenhamsLine.Gridcast(start, (start + (angle * Length)));
       for (int i = 0; i < newBres.Count; i++){
            Debug.DrawLine(new Vector2(newBres[i].TilePos.x - (resolution * 0.5f), newBres[i].TilePos.y - (resolution * 0.5f)), new Vector2(newBres[i].TilePos.x + (resolution * 0.5f), newBres[i].TilePos.y - (resolution * 0.5f)), Color.cyan);
            Debug.DrawLine(new Vector2(newBres[i].TilePos.x + (resolution * 0.5f), newBres[i].TilePos.y - (resolution * 0.5f)), new Vector2(newBres[i].TilePos.x + (resolution * 0.5f), newBres[i].TilePos.y + (resolution * 0.5f)), Color.cyan);
            Debug.DrawLine(new Vector2(newBres[i].TilePos.x + (resolution * 0.5f), newBres[i].TilePos.y + (resolution * 0.5f)), new Vector2(newBres[i].TilePos.x - (resolution * 0.5f), newBres[i].TilePos.y + (resolution * 0.5f)), Color.cyan);
            Debug.DrawLine(new Vector2(newBres[i].TilePos.x - (resolution * 0.5f), newBres[i].TilePos.y + (resolution * 0.5f)), new Vector2(newBres[i].TilePos.x - (resolution * 0.5f), newBres[i].TilePos.y - (resolution * 0.5f)), Color.cyan);

            if (newBres[i].ExtraTilePositions != null){
                for (int j = 0; j < newBres[i].ExtraTilePositions.Length; j++){
                    Debug.DrawLine(new Vector2(newBres[i].ExtraTilePositions[j].x - (resolution * 0.5f), newBres[i].ExtraTilePositions[j].y - (resolution * 0.5f)), new Vector2(newBres[i].ExtraTilePositions[j].x + (resolution * 0.5f), newBres[i].ExtraTilePositions[j].y - (resolution * 0.5f)), Color.cyan);
                    Debug.DrawLine(new Vector2(newBres[i].ExtraTilePositions[j].x + (resolution * 0.5f), newBres[i].ExtraTilePositions[j].y - (resolution * 0.5f)), new Vector2(newBres[i].ExtraTilePositions[j].x + (resolution * 0.5f), newBres[i].ExtraTilePositions[j].y + (resolution * 0.5f)), Color.cyan);
                    Debug.DrawLine(new Vector2(newBres[i].ExtraTilePositions[j].x + (resolution * 0.5f), newBres[i].ExtraTilePositions[j].y + (resolution * 0.5f)), new Vector2(newBres[i].ExtraTilePositions[j].x - (resolution * 0.5f), newBres[i].ExtraTilePositions[j].y + (resolution * 0.5f)), Color.cyan);
                    Debug.DrawLine(new Vector2(newBres[i].ExtraTilePositions[j].x - (resolution * 0.5f), newBres[i].ExtraTilePositions[j].y + (resolution * 0.5f)), new Vector2(newBres[i].ExtraTilePositions[j].x - (resolution * 0.5f), newBres[i].ExtraTilePositions[j].y - (resolution * 0.5f)), Color.cyan);
                }    
            }
        }
    }
    [EasyButtons.Button]
    public void ReplayLine(){
        resolution = 1 / (float)Resolution;
        
        Debug.DrawLine(BresenhamsLine.lastCastStart, BresenhamsLine.lastCastEnd, Color.red);
        List<BresenhamsLine.OverlapWithTiles> newBres = BresenhamsLine.ReplayGridcast();
        for (int i = 0; i < newBres.Count; i++){
            Debug.DrawLine(new Vector2(newBres[i].TilePos.x - (resolution * 0.5f), newBres[i].TilePos.y - (resolution * 0.5f)), new Vector2(newBres[i].TilePos.x + (resolution * 0.5f), newBres[i].TilePos.y - (resolution * 0.5f)), Color.cyan);
            Debug.DrawLine(new Vector2(newBres[i].TilePos.x + (resolution * 0.5f), newBres[i].TilePos.y - (resolution * 0.5f)), new Vector2(newBres[i].TilePos.x + (resolution * 0.5f), newBres[i].TilePos.y + (resolution * 0.5f)), Color.cyan);
            Debug.DrawLine(new Vector2(newBres[i].TilePos.x + (resolution * 0.5f), newBres[i].TilePos.y + (resolution * 0.5f)), new Vector2(newBres[i].TilePos.x - (resolution * 0.5f), newBres[i].TilePos.y + (resolution * 0.5f)), Color.cyan);
            Debug.DrawLine(new Vector2(newBres[i].TilePos.x - (resolution * 0.5f), newBres[i].TilePos.y + (resolution * 0.5f)), new Vector2(newBres[i].TilePos.x - (resolution * 0.5f), newBres[i].TilePos.y - (resolution * 0.5f)), Color.cyan);

            if (newBres[i].ExtraTilePositions != null){
                for (int j = 0; j < newBres[i].ExtraTilePositions.Length; j++){
                    Debug.DrawLine(new Vector2(newBres[i].ExtraTilePositions[j].x - (resolution * 0.5f), newBres[i].ExtraTilePositions[j].y - (resolution * 0.5f)), new Vector2(newBres[i].ExtraTilePositions[j].x + (resolution * 0.5f), newBres[i].ExtraTilePositions[j].y - (resolution * 0.5f)), Color.cyan);
                    Debug.DrawLine(new Vector2(newBres[i].ExtraTilePositions[j].x + (resolution * 0.5f), newBres[i].ExtraTilePositions[j].y - (resolution * 0.5f)), new Vector2(newBres[i].ExtraTilePositions[j].x + (resolution * 0.5f), newBres[i].ExtraTilePositions[j].y + (resolution * 0.5f)), Color.cyan);
                    Debug.DrawLine(new Vector2(newBres[i].ExtraTilePositions[j].x + (resolution * 0.5f), newBres[i].ExtraTilePositions[j].y + (resolution * 0.5f)), new Vector2(newBres[i].ExtraTilePositions[j].x - (resolution * 0.5f), newBres[i].ExtraTilePositions[j].y + (resolution * 0.5f)), Color.cyan);
                    Debug.DrawLine(new Vector2(newBres[i].ExtraTilePositions[j].x - (resolution * 0.5f), newBres[i].ExtraTilePositions[j].y + (resolution * 0.5f)), new Vector2(newBres[i].ExtraTilePositions[j].x - (resolution * 0.5f), newBres[i].ExtraTilePositions[j].y - (resolution * 0.5f)), Color.cyan);
                }
                
            }
        }
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