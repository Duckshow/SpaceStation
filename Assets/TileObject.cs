using UnityEngine;

public class TileObject : MonoBehaviour {

    Tile myTile;

	void Start () {
        // childed tileobjects should rely on their parent
        if (transform.parent != null && transform.parent.GetComponentInParent<TileObject>() != null)
            return;

        myTile = Grid.Instance.GetClosestFreeNode(transform.position);
        transform.position = myTile.WorldPosition + myTile.CenterPosition;
        myTile.IsOccupied = true;
	}
}
