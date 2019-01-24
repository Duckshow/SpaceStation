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
		ScheduleUpdateGraphicsForSurroundingTiles();
	}

	public bool IsWall { get; private set; }
	public bool IsWallTemporarily { get; private set; }
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

    public void TrySetIsWall(bool _isWall) {
		if (IsWall == _isWall){
			return;
		}

		IsWall = _isWall;
		RoomManager.GetInstance().ScheduleUpdateForRoomOfNode(GridPos);
		ScheduleUpdateGraphicsForSurroundingTiles();
	}

	 public void TrySetIsWallTemporary(bool _isWallTemporary) {
		if (UseIsWallTemporary){
			return;
		}
		
		IsWallTemporarily = _isWallTemporary;
		UseIsWallTemporary = true;
		ScheduleUpdateGraphicsForSurroundingTiles();
	}

	 public void TryClearIsWallTemporary() {
		if (!UseIsWallTemporary){
			return;
		}

		IsWallTemporarily = false;
		UseIsWallTemporary = false;
		ScheduleUpdateGraphicsForSurroundingTiles();
	}

    public void ScheduleUpdateGraphicsForSurroundingTiles() {
		ColorManager.ColorUsage _context = ColorManager.ColorUsage.Default;
		if (UseIsWallTemporary && IsWallTemporarily) _context = ColorManager.ColorUsage.New;
		if (UseIsWallTemporary && !IsWallTemporarily) _context = ColorManager.ColorUsage.Delete;
		// if (!_isBuildingAllowed)	context = ColorManager.ColorUsage.Blocked;
		byte _colorIndex = ColorManager.GetColorIndex(_context);

		byte[] _colorChannelIndices = new byte[BuildTool.ToolSettingsColor.COLOR_CHANNEL_COUNT]{
			_colorIndex,
			_colorIndex,
			_colorIndex,
			_colorIndex,
			_colorIndex,
			_colorIndex,
			_colorIndex,
			_colorIndex,
			_colorIndex,
			_colorIndex
		};

		UVController _tileTL, _tileTR, _tileBR, _tileBL;
		GameGrid.NeighborFinder.GetSurroundingTiles(GridPos, out _tileTL, out _tileTR, out _tileBR, out _tileBL);
		if (_tileTL != null) { UpdateTileAssetsAndColor(_tileTL, _colorChannelIndices); }
		if (_tileTR != null) { UpdateTileAssetsAndColor(_tileTR, _colorChannelIndices); }
		if (_tileBR != null) { UpdateTileAssetsAndColor(_tileBR, _colorChannelIndices); }
		if (_tileBL != null) { UpdateTileAssetsAndColor(_tileBL, _colorChannelIndices); }
	}

	void UpdateTileAssetsAndColor(UVController _tile, byte[] _colorChannelIndices) { 
		if (UseIsWallTemporary){
			_tile.SetColor(_colorChannelIndices, _isPermanent: false);
		}
		else{
			_tile.ClearTemporaryColor();
		}

		_tile.ScheduleUpdate();
	}

	public void SetColor(byte[] _colorChannelIndices, bool _isPermanent) { 
		UVController _tileTL, _tileTR, _tileBR, _tileBL;
		GameGrid.NeighborFinder.GetSurroundingTiles(GridPos, out _tileTL, out _tileTR, out _tileBR, out _tileBL);

		if (_tileTR != null){
			_tileTR.SetColor(_colorChannelIndices, _isPermanent);
		}
	}
	
	public void ClearTemporaryColor() {
		UVController _tileTL, _tileTR, _tileBR, _tileBL;
		GameGrid.NeighborFinder.GetSurroundingTiles(GridPos, out _tileTL, out _tileTR, out _tileBR, out _tileBL);

		// if (_tileTL != null){
		// 	_tileTL.ClearTemporaryColor();
		// }
		if (_tileTR != null){
			_tileTR.ClearTemporaryColor();
		}
		// if (_tileBR != null){
		// 	_tileBR.ClearTemporaryColor();
		// }
		// if (_tileBL != null){
		// 	_tileBL.ClearTemporaryColor();
		// }
	}

	public void SetIsBuildingAllowed(bool _b) {
        IsBuildingAllowed = _b;
    }
}
