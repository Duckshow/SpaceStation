using UnityEngine;

public class ColliderTester : MonoBehaviour {

    public Vector2[] Points1;
    public Vector2[] Points2;


    [EasyButtons.Button]
    public void SetPath() {
        GetComponent<PolygonCollider2D>().pathCount = 2;
        GetComponent<PolygonCollider2D>().SetPath(0, Points1);
        GetComponent<PolygonCollider2D>().SetPath(1, Points2);
    }
}
