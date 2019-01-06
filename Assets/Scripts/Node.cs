using UnityEngine;
using System;
using System.Collections.Generic;

public class Node : IHeapItem<Node> {

	public const float RADIUS = 0.5f;
	public const float DIAMETER = 1;
	public const int RESOLUTION = 16;

	public bool HasWallT;
	public bool HasWallR;
	public bool HasWallB;
	public bool HasWallL;

	public int RoomIndex { get; private set; }
	public void SetRoomIndex(int _index) {
		// Debug.Log("Set " + _index);
		RoomIndex = _index;
		UpdateGraphicsForSurroundingTiles(_isTemporary: false);
	}

	public bool IsWall { get; private set; }
	public bool IsWallTemporary { get; private set; }
	public bool UseIsWallTemporary { get; private set; }
	public bool IsBuildingAllowed { get; private set; }
	public bool HasDoorOrAirlock { get; private set; }
	public float WaitTime = 0.0f;

	public bool IsWalkable() {
		return (!IsWall || HasDoorOrAirlock) && GetOccupyingTileObject() == null;
	}

	public Int2 GridPos { get; private set; }
    public Vector2 WorldPos { get; private set; }
    public Vector2 WorldPosDefault { get; private set; }
    public Vector2 GetWorldPosCharacter() { // the position a character should stand on (exists to better emulate zero-g)
		return WorldPosDefault;
    }

    [NonSerialized] public Node ParentNode;
    [NonSerialized] public int GCost;
    [NonSerialized] public int HCost;
    public int _FCost_ { get { return GCost + HCost; } }

    private int heapIndex;
    public int HeapIndex {
        get { return heapIndex; }
        set { heapIndex = value; }
    }

    public int MovementPenalty { get; private set; }

    private NodeObject occupyingTileObject = null;
    public NodeObject GetOccupyingTileObject(){ 
        return occupyingTileObject; 
    }
    public void SetOccupyingTileObject(NodeObject _newOccupant){
        if (occupyingTileObject != null && occupyingTileObject != _newOccupant)
            Debug.LogErrorFormat("{0}'s new tile ({1}) is occupied by {2}! This shouldn't happen!", _newOccupant.transform.name, GridPos, occupyingTileObject.transform.name);
        occupyingTileObject = _newOccupant;
    }
    public void ClearOccupyingTileObject(NodeObject _caller){
        if(occupyingTileObject != null && occupyingTileObject != _caller)
            Debug.LogErrorFormat("{0} tried to set Tile({1})'s occupyingTileObject to null, but isn't actually its current occupant!", _caller.transform.name, GridPos);
        occupyingTileObject = null;
    }


    public Node(Vector3 _worldPos, int _gridX, int _gridY) {
		WorldPos = _worldPos;
        GridPos = new Int2(_gridX, _gridY);

		// MyUVController = ((GameObject)GameGrid.Instantiate(CachedAssets.Instance.TilePrefab, new Vector3(PosWorld.x, PosWorld.y + 0.5f, 0), Quaternion.identity)).GetComponent<UVController>();
		// MyUVController.name = "TileQuad " + PosGrid.x + "x" + PosGrid.y + " (" + PosWorld.x + ", " + PosWorld.y + ")";
		// MyUVController.Setup();
        // Animator = new TileAnimator(this);
    }

	public int CompareTo(Node nodeToCompare) {
        int compare = _FCost_.CompareTo(nodeToCompare._FCost_);
        if (compare == 0)
            compare = HCost.CompareTo(nodeToCompare.HCost);

        return -compare;
    }

    public void SetIsWall(bool _isWall) {
		IsWall = _isWall;
		RoomManager.GetInstance().ScheduleUpdateForRoomOfNode(GridPos);
		UpdateGraphicsForSurroundingTiles(_isTemporary: false);
	}

	 public void SetIsWallTemporary(bool _isWallTemporary) {
		IsWallTemporary = _isWallTemporary;
		UseIsWallTemporary = true;
		UpdateGraphicsForSurroundingTiles(_isTemporary: true);
	}

	 public void ClearIsWallTemporary() {
		IsWallTemporary = false;
		UseIsWallTemporary = false;
		UpdateGraphicsForSurroundingTiles(_isTemporary: false);
	}

    public void UpdateGraphicsForSurroundingTiles(bool _isTemporary) {
		UVController _tileTL, _tileTR, _tileBR, _tileBL;
		GameGrid.NeighborFinder.GetSurroundingTiles(GridPos, out _tileTL, out _tileTR, out _tileBR, out _tileBL);
		if(_tileTL != null) _tileTL.ScheduleUpdate();
		if(_tileTR != null) _tileTR.ScheduleUpdate();
		if(_tileBR != null) _tileBR.ScheduleUpdate();
		if(_tileBL != null) _tileBL.ScheduleUpdate();
	}

    public void SetColor(byte _colorIndex, bool _isTemporary) {
		// MyUVController.ChangeColor(_colorIndex, _temporary);
    }
    public void RemoveTemporaryColor(){
		// MyUVController.ResetColor();
    }

    public void SetIsBuildingAllowed(bool _b) {
        IsBuildingAllowed = _b;
    }
}
