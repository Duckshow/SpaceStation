using UnityEngine;

public class NodeObject : EventOwner {

    public Node MyTile;
	//public UVController[] MyUVControllers;
	public UVController MyUVController;
	public NodeObject Parent { get; private set; }
	private CanInspect myInspector;


	void Awake() {
		myInspector = GetComponent<CanInspect>();
	}

	protected override void OnEnable() {
		base.OnEnable();
		if(myInspector != null)
			myInspector.OnHide += OnHide;
	}
	
	protected override void OnDisable() {
		base.OnDisable();
		if (myInspector != null)
			myInspector.OnHide -= OnHide;
	}

	public override bool IsUsingStartDefault() { return true; }
	public override void StartDefault () {
		base.StartDefault();
		
		if (GameGrid.GetInstance() == null)
            return;

        SetGridPosition(GameGrid.GetInstance().GetClosestFreeNode(transform.position));
	}

	void OnHide(bool _b) { 
		if(_b)
			DeActivate();
		else
			Activate();
	}

	public void SetGridPosition(Node _tile, bool _setPosition = true) {
        if (Parent != null)
            return;

        NodeObject _occupant = _tile.GetOccupyingTileObject();
        if (_occupant != null && _occupant != this)
            Debug.LogError(transform.name + "'s new tile is occupied by someone! This shouldn't happen!");
        if (MyTile != null) {
            if(MyTile.GetOccupyingTileObject() != this)
                Debug.LogError("MyTile is occupied by someone other than me! This shouldn't happen!");
                
            MyTile.ClearOccupyingTileObject(this);
        }

        MyTile = _tile;
        if(_setPosition)
            transform.position = GetComponent<Actor>() ? MyTile.GetWorldPosCharacter() : MyTile.WorldPosDefault;

        if (isActive) {
            MyTile.SetOccupyingTileObject(this);
        }

        Sort();
	}

    public void SetParent(NodeObject _parent) {
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
        MyTile = GameGrid.GetInstance().GetClosestFreeNode(MyTile);
        if (MyTile == null)
            throw new System.Exception(name + " couldn't find a free tile!");

        MyTile.SetOccupyingTileObject(this);
        transform.position = GetComponent<Actor>() ? MyTile.GetWorldPosCharacter() : MyTile.WorldPosDefault;

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
			transform.position = new Vector3(transform.position.x, transform.position.y, -(GameGrid.SIZE.y - MyTile.GridPos.y) * 0.5f);
		else
			MyUVController.Sort(MyTile.GridPos.y);
    }
}
