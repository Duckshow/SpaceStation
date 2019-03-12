using UnityEngine;

public class NodeObject : EventOwner {

	//public UVController[] MyUVControllers;
	public UVController MyUVController;
	public NodeObject Parent { get; private set; }
	private CanInspect myInspector;
	private Int2 nodeGridPosition = new Int2(-1, -1);


	public override bool IsUsingAwakeDefault() { return true; }
	public override void AwakeDefault() { 
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

	public Node GetNode() {
		return GameGrid.GetInstance().TryGetNode(nodeGridPosition);
	}

	public void SetGridPosition(Node _newNode, bool _setPosition = true) {
        if (Parent != null)
            return;

		Node _oldNode = GetNode();

		NodeObject _occupant = _newNode.GetOccupyingNodeObject();
		if (_occupant != null && _occupant != this) { 
			Debug.LogError(transform.name + "'s new tile is occupied by someone! This shouldn't happen!");
		}

		if (_oldNode != null) {
			if (_oldNode.GetOccupyingNodeObject() != this) { 
				Debug.LogErrorFormat("Node occupied by {0} was claimed by {1}!", _oldNode.GetOccupyingNodeObject(), name);
			}

			_oldNode.ClearOccupyingNodeObject(this);
        }

		nodeGridPosition = _newNode.GridPos;

		if (_setPosition) { 
			transform.position =_newNode.WorldPos;
		}

        if (isActive) {
            _newNode.SetOccupyingNodeObject(this);
        }

        Sort();
	}

    public void SetParent(NodeObject _parent) {
        Parent = _parent;
        transform.parent = _parent != null ? _parent.transform : null;
    }

    private bool isActive = true;
    public void Activate() {
		if (isActive) { 
			return;
		}

		Node _node = GetNode();
		if (_node == null) { 
			return;
		}

        isActive = true;
        _node = GameGrid.GetInstance().GetClosestFreeNode(_node);
		if (_node == null) { 
			throw new System.Exception(name + " couldn't find a free node!");
		}

        _node.SetOccupyingNodeObject(this);
        transform.position = _node.WorldPos;

        Sort();
    }
    public void DeActivate() {
		if (!isActive) { 
			return;
		}

		Node _node = GetNode();
		if (_node == null) { 
			return;
		}

        isActive = false;
        _node.ClearOccupyingNodeObject(this);
    }

    public void Sort() {
		if (MyUVController == null) { 
			transform.position = new Vector3(transform.position.x, transform.position.y, -(GameGrid.SIZE.y - nodeGridPosition.y) * 0.5f);
		}
		else { 
			MyUVController.Sort(nodeGridPosition.y);
		}
    }

	public int GetRoomIndex() {
		return GetNode().RoomIndex;
	}
}
