using UnityEngine;

public class TileObject : MonoBehaviour {

    Tile myTile;

	void Start () {
        // childed tileobjects should rely on their parent
        if (transform.parent != null && transform.parent.GetComponentInParent<TileObject>() != null)
            return;

        myTile = Grid.Instance.GetClosestFreeNode(transform.position);

        if(GetComponent<Actor>())
            transform.position = myTile.CharacterPositionWorld;
        else
            transform.position = myTile.DefaultPositionWorld;
        myTile.IsOccupied = true;
	}
}
