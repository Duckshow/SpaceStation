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

	private Color32 lightingTL = new Color32(0, 0, 0, 0);
	private Color32 lightingTR = new Color32(0, 0, 0, 0);
	private Color32 lightingBR = new Color32(0, 0, 0, 0);
	private Color32 lightingBL = new Color32(0, 0, 0, 0);

	public Color32 GetLighting() { 
		if (!lightingTL.Equals(lightingTR) || !lightingTL.Equals(lightingBR) || !lightingTL.Equals(lightingBL)){
			Debug.LogErrorFormat("Node({0})'s lighting varies across vertices, but is still expected to return a uniform lighting variable!");
		}

		return lightingTL;
	}

	public void SetLighting(Color32 _light) {
		lightingTL = _light;
		lightingTR = _light;
		lightingBR = _light;
		lightingBL = _light;
		ScheduleUpdateGraphicsForSurroundingTiles();
	}

	public void SetLightingBasedOnNeighbors() {
		Node _nodeTL, _nodeT, _nodeTR, _nodeR, _nodeBR, _nodeB, _nodeBL, _nodeL;
		GameGrid.NeighborFinder.GetSurroundingNodes(GridPos, out _nodeTL, out _nodeT, out _nodeTR, out _nodeR, out _nodeBR, out _nodeB, out _nodeBL, out _nodeL);

		lightingTL = GetLightingFromDirection(NeighborEnum.TL);
		lightingTR = GetLightingFromDirection(NeighborEnum.TR);
		lightingBR = GetLightingFromDirection(NeighborEnum.BR);
		lightingBL = GetLightingFromDirection(NeighborEnum.BL);

		ScheduleUpdateGraphicsForSurroundingTiles();
	}

	Color32 GetLightingFromDirection(NeighborEnum _direction) {
		NeighborEnum _directionY = NeighborEnum.None;
		NeighborEnum _directionX = NeighborEnum.None;
		switch (_direction){
			case NeighborEnum.None:
			case NeighborEnum.All:
			case NeighborEnum.T:
			case NeighborEnum.R:
			case NeighborEnum.B:
			case NeighborEnum.L:
				Debug.LogError(_direction + " isn't supported by GetLightingFromDirection()!");
				break;
			case NeighborEnum.TL:
				_directionY = NeighborEnum.T;
				_directionX = NeighborEnum.L;
				break;
			case NeighborEnum.TR:
				_directionY = NeighborEnum.T;
				_directionX = NeighborEnum.R;
				break;
			case NeighborEnum.BR:
				_directionY = NeighborEnum.B;
				_directionX = NeighborEnum.R;
				break;
			case NeighborEnum.BL:
				_directionY = NeighborEnum.B;
				_directionX = NeighborEnum.L;
				break;
			default:
				Debug.LogError(_direction + " hasn't been properly implemented yet!");
				break;
		}

		GameGrid.NeighborFinder.TryCacheNeighbor(GridPos, _direction);
		GameGrid.NeighborFinder.TryCacheNeighbor(GridPos, _directionX);
		GameGrid.NeighborFinder.TryCacheNeighbor(GridPos, _directionY);

		Node _neighborXY = GameGrid.NeighborFinder.CachedNeighbors[_direction];
		Node _neighborY = GameGrid.NeighborFinder.CachedNeighbors[_directionX];
		Node _neighborX = GameGrid.NeighborFinder.CachedNeighbors[_directionY];

		int _neighborsGivingLight = 0;

		Color32 _lightingFromNeighborXY = new Color32();
		if (_neighborXY != null && !_neighborXY.IsWall){
			_lightingFromNeighborXY = _neighborXY.GetLighting();
			_neighborsGivingLight++;
		}

		Color32 _lightingFromNeighborY = new Color32();
		if (_neighborY != null && !_neighborY.IsWall){
			_lightingFromNeighborY = _neighborY.GetLighting();
			_neighborsGivingLight++;
		}

		Color32 _lightingFromNeighborX = new Color32();
		if (_neighborX != null && !_neighborX.IsWall){
			_lightingFromNeighborX = _neighborX.GetLighting();
			_neighborsGivingLight++;
		}

		if (_neighborsGivingLight == 0){
			return new Color32();
		}

		Color32 _newLighting = new Color32(
			(byte)((_lightingFromNeighborXY.r + _lightingFromNeighborY.r + _lightingFromNeighborX.r) / _neighborsGivingLight),
			(byte)((_lightingFromNeighborXY.g + _lightingFromNeighborY.g + _lightingFromNeighborX.g) / _neighborsGivingLight),
			(byte)((_lightingFromNeighborXY.b + _lightingFromNeighborY.b + _lightingFromNeighborX.b) / _neighborsGivingLight),
			(byte)((_lightingFromNeighborXY.a + _lightingFromNeighborY.a + _lightingFromNeighborX.a) / _neighborsGivingLight)
		);

		// Color32 _newLighting = new Color32(
		// 	255, 255, 255, 255
		// );

		return _newLighting;
	}

	public bool IsWalkable() {
		return !IsWall || HasDoorOrAirlock;
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

    private NodeObject occupyingNodeObject = null;
    public NodeObject GetOccupyingNodeObject(){ 
        return occupyingNodeObject; 
    }
    public void SetOccupyingNodeObject(NodeObject _newOccupant){
        if (occupyingNodeObject != null && occupyingNodeObject != _newOccupant)
            Debug.LogErrorFormat("{0}'s new node ({1}) is occupied by {2}! This shouldn't happen!", _newOccupant.transform.name, GridPos, occupyingNodeObject.transform.name);
        occupyingNodeObject = _newOccupant;
    }
    public void ClearOccupyingNodeObject(NodeObject _caller){
        if(occupyingNodeObject != null && occupyingNodeObject != _caller)
            Debug.LogErrorFormat("{0} tried to set Node({1})'s occupyingNodeObject to null, but isn't actually its current occupant!", _caller.transform.name, GridPos);
        occupyingNodeObject = null;
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

		if (_isWall){
			GameGrid.NeighborFinder.TryCacheNeighbor(GridPos, NeighborEnum.All);
			LampManager.GetInstance().TryAddNodeToUpdate(GameGrid.NeighborFinder.CachedNeighbors[NeighborEnum.TL]);
			LampManager.GetInstance().TryAddNodeToUpdate(GameGrid.NeighborFinder.CachedNeighbors[NeighborEnum.T]);
			LampManager.GetInstance().TryAddNodeToUpdate(GameGrid.NeighborFinder.CachedNeighbors[NeighborEnum.TR]);
			LampManager.GetInstance().TryAddNodeToUpdate(GameGrid.NeighborFinder.CachedNeighbors[NeighborEnum.R]);
			LampManager.GetInstance().TryAddNodeToUpdate(GameGrid.NeighborFinder.CachedNeighbors[NeighborEnum.BR]);
			LampManager.GetInstance().TryAddNodeToUpdate(GameGrid.NeighborFinder.CachedNeighbors[NeighborEnum.B]);
			LampManager.GetInstance().TryAddNodeToUpdate(GameGrid.NeighborFinder.CachedNeighbors[NeighborEnum.BL]);
			LampManager.GetInstance().TryAddNodeToUpdate(GameGrid.NeighborFinder.CachedNeighbors[NeighborEnum.L]);
		}
		else{
			LampManager.GetInstance().TryAddNodeToUpdate(this);
		}

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
		if (_tileTR != null) { UpdateTileVisuals(_tileTR, _colorChannelIndices, _vertexIndex: 0, _lighting: lightingTR); }
		if (_tileBL != null) { UpdateTileVisuals(_tileBL, _colorChannelIndices, _vertexIndex: 1, _lighting: lightingBL); }
		if (_tileTL != null) { UpdateTileVisuals(_tileTL, _colorChannelIndices, _vertexIndex: 2, _lighting: lightingTL); }
		if (_tileBR != null) { UpdateTileVisuals(_tileBR, _colorChannelIndices, _vertexIndex: 3, _lighting: lightingBR); }
	}

	void UpdateTileVisuals(UVController _tile, byte[] _colorChannelIndices, int _vertexIndex, Color32 _lighting) { 
		if (UseIsWallTemporary){
			_tile.SetColor(_colorChannelIndices, _isPermanent: false);
		}
		else{
			_tile.ClearTemporaryColor();
		}

		_tile.SetLighting(_vertexIndex, _lighting);
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
