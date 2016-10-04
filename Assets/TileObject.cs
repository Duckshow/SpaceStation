using UnityEngine;

public class TileObject : MonoBehaviour {

    Tile myTile;

	void Start () {
        myTile = Grid.Instance.GetClosestFreeNode(transform.position);
        transform.position = myTile.WorldPosition + myTile.CenterPosition;
        myTile.IsOccupied = true;
	}
}
