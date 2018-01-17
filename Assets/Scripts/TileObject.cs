using UnityEngine;

public class TileObject : MonoBehaviour {

    public Tile MyTile;
    //public UVController[] MyUVControllers;
	public UVController MyUVController;
	public TileObject Parent { get; private set; }

	void Start () {
        if (Grid.Instance == null)
            return;

        //myUVControllers = GetComponentsInChildren<UVController>();
        SetGridPosition(Grid.Instance.GetClosestFreeNode(transform.position));
	}

    public void SetGridPosition(Tile _tile, bool _setPosition = true) {
        if (Parent != null)
            return;

        if (MyTile != null) {
            MyTile.IsOccupiedByObject = false; // TODO: not really futureproof, is it?
            MyTile.OccupyingInspectable = null;
        }

        MyTile = _tile;
        if(_setPosition)
            transform.position = GetComponent<Actor>() ? MyTile.CharacterPositionWorld : MyTile.DefaultPositionWorld;

        if (isActive) {
            MyTile.IsOccupiedByObject = true;
            MyTile.OccupyingInspectable = GetComponent<CanInspect>();
        }

        Sort();
    }

    public void SetParent(TileObject _parent) {
        Parent = _parent;
        transform.parent = _parent != null ? _parent.transform : null;
    }

    private bool isActive = true;
    public void Activate() {
        if (isActive)
            return;
        if (MyTile == null)
            return;

        isActive = true;
        MyTile = Grid.Instance.GetClosestFreeNode(MyTile);
        if (MyTile == null)
            throw new System.Exception(name + " couldn't find a free tile!");

        MyTile.IsOccupiedByObject = true;
        MyTile.OccupyingInspectable = GetComponent<CanInspect>();
        transform.position = GetComponent<Actor>() ? MyTile.CharacterPositionWorld : MyTile.DefaultPositionWorld;

        Sort();
    }
    public void DeActivate() {
        if (!isActive)
            return;
        if (MyTile == null)
            return;

        isActive = false;
        MyTile.IsOccupiedByObject = false;
        MyTile.OccupyingInspectable = null;
    }

    public void Sort() {
		if (MyUVController == null)
			transform.position = new Vector3(transform.position.x, transform.position.y, -(Grid.GridSizeY - MyTile.GridCoord.y) * 0.5f);
		else
			MyUVController.Sort(MyTile.GridCoord.y);
    }
}
