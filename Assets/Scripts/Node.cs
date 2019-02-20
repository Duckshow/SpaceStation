using UnityEngine;
using System;
using System.Collections.Generic;

public class Node : IHeapItem<Node> {

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

	public GameGridInteractiveObject InteractiveObject { get; private set; }
	public GameGridInteractiveObject InteractiveObjectTemporary { get; private set; }
	public bool UseInteractiveObjectTemporary { get; private set; }
	public Rotation InteractiveObjectRotation { get; private set; }
	public Rotation InteractiveObjectRotationTemporary { get; private set; }

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
		NeighborFinder.GetSurroundingNodes(GridPos, out _nodeTL, out _nodeT, out _nodeTR, out _nodeR, out _nodeBR, out _nodeB, out _nodeBL, out _nodeL);

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

		NeighborFinder.TryCacheNeighbor(GridPos, _direction);
		NeighborFinder.TryCacheNeighbor(GridPos, _directionX);
		NeighborFinder.TryCacheNeighbor(GridPos, _directionY);

		Node _neighborXY = NeighborFinder.CachedNeighbors[_direction];
		Node _neighborY = NeighborFinder.CachedNeighbors[_directionX];
		Node _neighborX = NeighborFinder.CachedNeighbors[_directionY];

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

	public Int2 GridPos { get; private set; }
    public Vector2 WorldPos { get; private set; }

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
			NeighborFinder.TryCacheNeighbor(GridPos, NeighborEnum.All);
			LampManager.GetInstance().TryAddNodeToUpdate(NeighborFinder.CachedNeighbors[NeighborEnum.TL]);
			LampManager.GetInstance().TryAddNodeToUpdate(NeighborFinder.CachedNeighbors[NeighborEnum.T]);
			LampManager.GetInstance().TryAddNodeToUpdate(NeighborFinder.CachedNeighbors[NeighborEnum.TR]);
			LampManager.GetInstance().TryAddNodeToUpdate(NeighborFinder.CachedNeighbors[NeighborEnum.R]);
			LampManager.GetInstance().TryAddNodeToUpdate(NeighborFinder.CachedNeighbors[NeighborEnum.BR]);
			LampManager.GetInstance().TryAddNodeToUpdate(NeighborFinder.CachedNeighbors[NeighborEnum.B]);
			LampManager.GetInstance().TryAddNodeToUpdate(NeighborFinder.CachedNeighbors[NeighborEnum.BL]);
			LampManager.GetInstance().TryAddNodeToUpdate(NeighborFinder.CachedNeighbors[NeighborEnum.L]);
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

	public void TrySetInteractiveObject(GameGridInteractiveObject _interactive, Rotation _rotation) {
		if (InteractiveObject == _interactive){
			return;
		}

		InteractiveObject = _interactive;
		InteractiveObjectRotation = _rotation;
		ScheduleUpdateGraphicsForSurroundingTiles();
	}

	public void TrySetInteractiveObjectTemporary(GameGridInteractiveObject _interactive, Rotation _rotation) {
		if (UseInteractiveObjectTemporary){
			return;
		}

		InteractiveObjectTemporary = _interactive;
		InteractiveObjectRotationTemporary = _rotation;
		UseInteractiveObjectTemporary = true;
		ScheduleUpdateGraphicsForSurroundingTiles();
	}

	public void TryClearInteractiveObjectTemporary() {
		if (!UseInteractiveObjectTemporary){
			return;
		}

		InteractiveObjectTemporary = null;
		UseInteractiveObjectTemporary = false;
		ScheduleUpdateGraphicsForSurroundingTiles();
	}

    public void ScheduleUpdateGraphicsForSurroundingTiles() {
		ColorManager.ColorUsage _context = ColorManager.ColorUsage.Default;

		if (UseIsWallTemporary && IsWallTemporarily){
			_context = ColorManager.ColorUsage.New;
		} 
		if (UseIsWallTemporary && !IsWallTemporarily){
			_context = ColorManager.ColorUsage.Delete;
		} 
	
		if (UseInteractiveObjectTemporary && InteractiveObjectTemporary != null){
			_context = ColorManager.ColorUsage.New;

			if (InteractiveObject != null){
				_context = ColorManager.ColorUsage.Blocked;
			}
		}
		if (UseInteractiveObjectTemporary && InteractiveObjectTemporary == null){
			_context = ColorManager.ColorUsage.Delete;
		}

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

		Int2 _tileGridPosTL, _tileGridPosTR, _tileGridPosBR, _tileGridPosBL;
		NeighborFinder.GetSurroundingTiles(GridPos, out _tileGridPosTL, out _tileGridPosTR, out _tileGridPosBR, out _tileGridPosBL);

		if (_tileGridPosTR != Int2.MinusOne) {
			GameGrid.GetInstance().ScheduleUpdateForTile(_tileGridPosTR);
			UpdateTileColor(_tileGridPosTR, _colorChannelIndices);
			SetTileVertexLighting(_tileGridPosTR, lightingTR, _vertexIndex: GameGridMesh.VERTEX_INDEX_BOTTOM_LEFT);
		}
		if (_tileGridPosBL != Int2.MinusOne) {
			GameGrid.GetInstance().ScheduleUpdateForTile(_tileGridPosBL);
			UpdateTileColor(_tileGridPosBL, _colorChannelIndices);
			SetTileVertexLighting(_tileGridPosBL, lightingBL, _vertexIndex: GameGridMesh.VERTEX_INDEX_TOP_RIGHT);
		}
		if (_tileGridPosTL != Int2.MinusOne) {
			GameGrid.GetInstance().ScheduleUpdateForTile(_tileGridPosTL);
			UpdateTileColor(_tileGridPosTL, _colorChannelIndices);
			SetTileVertexLighting(_tileGridPosTL, lightingTL, _vertexIndex: GameGridMesh.VERTEX_INDEX_BOTTOM_RIGHT);
		}
		if (_tileGridPosBR != Int2.MinusOne) {
			GameGrid.GetInstance().ScheduleUpdateForTile(_tileGridPosBR);
			UpdateTileColor(_tileGridPosBR, _colorChannelIndices);
			SetTileVertexLighting(_tileGridPosBR, lightingBR, _vertexIndex: GameGridMesh.VERTEX_INDEX_TOP_LEFT);
		}
	}

	void UpdateTileColor(Int2 _tileGridPos, byte[] _colorChannelIndices) { 
		if (UseIsWallTemporary){
			GameGrid.GetInstance().SetColor(_tileGridPos, _colorChannelIndices, _isPermanent: false);
		}
		else{
			GameGrid.GetInstance().ClearTemporaryColor(_tileGridPos);
		}
	}

	void SetTileVertexLighting(Int2 _tileGridPos, Color32 _lighting, int _vertexIndex) { 
		GameGrid.GetInstance().SetLighting(_tileGridPos, _vertexIndex, _lighting);
	}

	public void SetColor(byte[] _colorChannelIndices, bool _isPermanent) { 
		Int2 _tileGridPosTL, _tileGridPosTR, _tileGridPosBR, _tileGridPosBL;
		NeighborFinder.GetSurroundingTiles(GridPos, out _tileGridPosTL, out _tileGridPosTR, out _tileGridPosBR, out _tileGridPosBL);

		if (_tileGridPosTR != Int2.MinusOne){
			GameGrid.GetInstance().SetColor(_tileGridPosTR, _colorChannelIndices, _isPermanent);
		}
	}
	
	public void ClearTemporaryColor() {
		Int2 _tileGridPosTL, _tileGridPosTR, _tileGridPosBR, _tileGridPosBL;
		NeighborFinder.GetSurroundingTiles(GridPos, out _tileGridPosTL, out _tileGridPosTR, out _tileGridPosBR, out _tileGridPosBL);

		// if (_tileTL != null){
		// 	_tileTL.ClearTemporaryColor();
		// }
		if (_tileGridPosTR != Int2.MinusOne){
			GameGrid.GetInstance().ClearTemporaryColor(_tileGridPosTR);
		}
		// if (_tileBR != null){
		// 	_tileBR.ClearTemporaryColor();
		// }
		// if (_tileBL != null){
		// 	_tileBL.ClearTemporaryColor();
		// }
	}

	public bool IsWalkable() { return !IsWall; }
	public float WaitTime = 0.0f;
}
