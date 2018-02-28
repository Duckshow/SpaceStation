using UnityEngine;

public class TileObject : MonoBehaviour {

    public Tile MyTile;
    //public UVController[] MyUVControllers;
	public UVController MyUVController;
	public TileObject Parent { get; private set; }
	private CanInspect myInspector;


	void Awake() {
		myInspector = GetComponent<CanInspect>();
	}
	void OnEnable() { 
		if(myInspector != null)
			myInspector.OnHide += OnHide;
	}
	void OnDisable() {
		if (myInspector != null)
			myInspector.OnHide -= OnHide;
	}
	void Start () {
        if (Grid.Instance == null)
            return;

        SetGridPosition(Grid.Instance.GetClosestFreeNode(transform.position));
	}

	void OnHide(bool _b) { 
		if(_b)
			DeActivate();
		else
			Activate();
	}

	public void SetGridPosition(Tile _tile, bool _setPosition = true) {
        if (Parent != null)
            return;

        TileObject _occupant = _tile.GetOccupyingTileObject();
        if (_occupant != null && _occupant != this)
            Debug.LogError(transform.name + "'s new tile is occupied by someone! This shouldn't happen!");
        if (MyTile != null) {
            if(MyTile.GetOccupyingTileObject() != this)
                Debug.LogError("MyTile is occupied by someone other than me! This shouldn't happen!");
                
            MyTile.ClearOccupyingTileObject(this);
        }

        MyTile = _tile;
        if(_setPosition)
            transform.position = GetComponent<Actor>() ? MyTile.CharacterPositionWorld : MyTile.DefaultPositionWorld;

        if (isActive) {
            MyTile.SetOccupyingTileObject(this);
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

        MyTile.SetOccupyingTileObject(this);
        transform.position = GetComponent<Actor>() ? MyTile.CharacterPositionWorld : MyTile.DefaultPositionWorld;

        Sort();
    }
    public void DeActivate() {
        if (!isActive)
            return;
        if (MyTile == null)
            return;

        isActive = false;
        MyTile.ClearOccupyingTileObject(this);
    }

    public void Sort() {
		if (MyUVController == null)
			transform.position = new Vector3(transform.position.x, transform.position.y, -(Grid.GridSize.y - MyTile.GridCoord.y) * 0.5f);
		else
			MyUVController.Sort(MyTile.GridCoord.y);
    }
}
