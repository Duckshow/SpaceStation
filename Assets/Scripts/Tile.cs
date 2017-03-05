using UnityEngine;

public class Tile : IHeapItem<Tile> {

    public enum Type { Empty, Solid, Diagonal, Door, Airlock }
	private Type wallType = Type.Empty;
	public Type _WallType_ { get { return wallType; } private set { wallType = value; }}
	private Type floorType = Type.Empty;
	public Type _FloorType_ { get { return floorType; } private set{ floorType = value; } }
    private Type prevType = Type.Empty;
    public Type _PrevType_ { get { return prevType; } }
    public enum TileOrientation { None, Bottom, BottomLeft, Left, TopLeft, Top, TopRight, Right, BottomRight }
    private TileOrientation orientation = TileOrientation.None;
	public TileOrientation _Orientation_ { get { return orientation; } private set{ orientation = value; } }
	private TileOrientation floorOrientation = TileOrientation.None;
	public TileOrientation _FloorOrientation_ { get { return floorOrientation; } private set{ floorOrientation = value; } }
    private TileOrientation prevOrientation = TileOrientation.None;
	public bool _IsHorizontal_ { get { return _Orientation_ == TileOrientation.Left || _Orientation_ == TileOrientation.Right; } }
	public bool _IsVertical_ { get { return _Orientation_ == TileOrientation.Bottom || _Orientation_ == TileOrientation.Top; } }
    public static TileOrientation GetReverseDirection(TileOrientation _direction) {
        switch (_direction) {
            case TileOrientation.Bottom:
                return TileOrientation.Top;
			case TileOrientation.BottomLeft:
				return TileOrientation.TopRight;
            case TileOrientation.Left:
                return TileOrientation.Right;
			case TileOrientation.TopLeft:
				return TileOrientation.BottomRight;
			case TileOrientation.Top:
                return TileOrientation.Bottom;
			case TileOrientation.TopRight:
				return TileOrientation.BottomLeft;
            case TileOrientation.Right:
                return TileOrientation.Left;
			case TileOrientation.BottomRight:
				return TileOrientation.TopLeft;
        }
        return Tile.TileOrientation.None;
    }

    public bool Walkable { get; private set; }
    public bool IsOccupied = false;
    private bool buildingAllowed = true;
    public bool _BuildingAllowed_ { get { return buildingAllowed; } private set { buildingAllowed = value; } }
    public int GridX { get; private set; }
    public int GridY { get; private set; }

    public Vector3 WorldPosition { get; private set; }
    public Vector3 DefaultPositionWorld { get; private set; }
    public Vector3 CharacterPositionWorld { // the position a character should stand on (exists to better simulate zero-g)
        get {
			if (_WallType_ == Type.Empty) {
                Vector3 _offset = Vector3.zero;
                _offset.x = IsBlocked_L ? -0.4f : IsBlocked_R ? 0.4f : 0;
                _offset.y = IsBlocked_B ? -0.4f : IsBlocked_T ? 0.4f : 0;
                return DefaultPositionWorld + _offset;
            }
            else
                return DefaultPositionWorld;
        }
    }

    public bool CanConnect_L { get; private set; }
    public bool CanConnect_T { get; private set; }
    public bool CanConnect_R { get; private set; }
    public bool CanConnect_B { get; private set; }
	public bool CanConnectFloor_L { get; private set; }
	public bool CanConnectFloor_T { get; private set; }
	public bool CanConnectFloor_R { get; private set; }
	public bool CanConnectFloor_B { get; private set; }

    [HideInInspector] public bool HasConnectable_L = false;
    [HideInInspector] public bool HasConnectable_T = false;
    [HideInInspector] public bool HasConnectable_R = false;
    [HideInInspector] public bool HasConnectable_B = false;
	[HideInInspector] public bool HasConnectableFloor_L = false;
	[HideInInspector] public bool HasConnectableFloor_T = false;
	[HideInInspector] public bool HasConnectableFloor_R = false;
	[HideInInspector] public bool HasConnectableFloor_B = false;

    [HideInInspector] public bool IsBlocked_L = false;
    [HideInInspector] public bool IsBlocked_T = false;
    [HideInInspector] public bool IsBlocked_R = false;
    [HideInInspector] public bool IsBlocked_B = false;

    // optimization: could probably cache these some smarter way
    [HideInInspector] public Tile ConnectedDiagonal_L;
	[HideInInspector] public Tile ConnectedDiagonal_T;
	[HideInInspector] public Tile ConnectedDiagonal_R;
	[HideInInspector] public Tile ConnectedDiagonal_B;
	[HideInInspector] public Tile ConnectedDiagonalFloor_L;
	[HideInInspector] public Tile ConnectedDiagonalFloor_T;
	[HideInInspector] public Tile ConnectedDiagonalFloor_R;
	[HideInInspector] public Tile ConnectedDiagonalFloor_B;
    [HideInInspector] public Tile ConnectedDoorOrAirlock_L;
	[HideInInspector] public Tile ConnectedDoorOrAirlock_T;
	[HideInInspector] public Tile ConnectedDoorOrAirlock_R;
	[HideInInspector] public Tile ConnectedDoorOrAirlock_B;

    [HideInInspector] public Tile ParentTile;
    [HideInInspector] public int GCost;
    [HideInInspector] public int HCost;
    public int _FCost_ { get { return GCost + HCost; } }

    private int heapIndex;
    public int HeapIndex {
        get { return heapIndex; }
        set { heapIndex = value; }
    }

	public UVController FloorQuad;
    public UVController BottomQuad;
    public UVController TopQuad;
    public TileAnimator Animator;

    public bool StopAheadAndBehindMeWhenCrossing { get; private set; }
    public int MovementPenalty { get; private set; }
    public bool ForceActorStopWhenPassingThis { get; private set; }


    public Tile(Vector3 _worldPos, int _gridX, int _gridY) {
        WorldPosition = _worldPos;
        GridX = _gridX;
        GridY = _gridY;
    }
		
	public void Init() {
		FloorQuad = ((GameObject)Grid.Instantiate(CachedAssets.Instance.TilePrefab, new Vector3(WorldPosition.x, WorldPosition.y + 0.5f, 0), Quaternion.identity)).GetComponent<UVController>();
		BottomQuad = ((GameObject)Grid.Instantiate(CachedAssets.Instance.TilePrefab, new Vector3(WorldPosition.x, WorldPosition.y + 0.5f, 0), Quaternion.identity)).GetComponent<UVController>();
        TopQuad = ((GameObject)Grid.Instantiate(CachedAssets.Instance.TilePrefab, new Vector3(WorldPosition.x, WorldPosition.y + 0.5f, 0), Quaternion.identity)).GetComponent<UVController>();
        
		FloorQuad.name = "TileQuad " + GridX + "x" + GridY + " (" + WorldPosition.x + ", " + WorldPosition.y + ")"; 
		BottomQuad.transform.parent = FloorQuad.transform;
		TopQuad.transform.parent = BottomQuad.transform;

		FloorQuad.Setup ();
		BottomQuad.Setup();
        TopQuad.Setup();
        Animator = new TileAnimator(this);
    }

    public int CompareTo(Tile nodeToCompare) {
        int compare = _FCost_.CompareTo(nodeToCompare._FCost_);
        if (compare == 0)
            compare = HCost.CompareTo(nodeToCompare.HCost);

        return -compare;
    }

    private static Tile cachedNeighbour_L;
    private static Tile cachedNeighbour_T;
    private static Tile cachedNeighbour_R;
    private static Tile cachedNeighbour_B;

    public void SetTileType(Type _newType, TileOrientation _newOrientation) {

        Animator.StopAnimating();

		prevType = _WallType_;
		_WallType_ = _newType;
		prevOrientation = _Orientation_;
		_Orientation_ = _newOrientation;

        BottomQuad.Type = _newType;
        BottomQuad.Orientation = _newOrientation;
		BottomQuad.SortingLayer = UVController.SortingLayerEnum.Bottom;
        BottomQuad.Sort(GridY);

        TopQuad.Type = _newType;
        TopQuad.Orientation = _newOrientation;
		TopQuad.SortingLayer = UVController.SortingLayerEnum.Top;
        TopQuad.Sort(GridY);

        ForceActorStopWhenPassingThis = false;
        MovementPenalty = 0; //TODO: use this for something!

		if (prevType == Type.Door || prevType == Type.Airlock) {
            Grid.Instance.grid[GridX + 1, GridY].SetBuildingAllowed(true);
            Grid.Instance.grid[GridX - 1, GridY].SetBuildingAllowed(true);
            Grid.Instance.grid[GridX, GridY + 1].SetBuildingAllowed(true);
            Grid.Instance.grid[GridX, GridY - 1].SetBuildingAllowed(true);

            Grid.Instance.grid[GridX - 1, GridY].ConnectedDoorOrAirlock_R = null;
            Grid.Instance.grid[GridX + 1, GridY].ConnectedDoorOrAirlock_L = null;
            Grid.Instance.grid[GridX, GridY - 1].ConnectedDoorOrAirlock_T = null;
            Grid.Instance.grid[GridX, GridY + 1].ConnectedDoorOrAirlock_B = null;
        }
        if (prevType == Type.Diagonal) {
            if(GridX > 0)
                Grid.Instance.grid[GridX - 1, GridY].ConnectedDiagonal_R = null;
            if(GridX < Grid.Instance.GridSizeX - 1)
                Grid.Instance.grid[GridX + 1, GridY].ConnectedDiagonal_L = null;
            if(GridY > 0)
                Grid.Instance.grid[GridX, GridY - 1].ConnectedDiagonal_T = null;
            if (GridY < Grid.Instance.GridSizeY - 1)
                Grid.Instance.grid[GridX, GridY + 1].ConnectedDiagonal_B = null;

			if ((prevOrientation == TileOrientation.BottomLeft && !CanConnectFloor_T && !CanConnectFloor_R) ||
			   (prevOrientation == TileOrientation.TopLeft && !CanConnectFloor_B && !CanConnectFloor_R) ||
			   (prevOrientation == TileOrientation.TopRight && !CanConnectFloor_B && !CanConnectFloor_L) ||
			   (prevOrientation == TileOrientation.BottomRight && !CanConnectFloor_T && !CanConnectFloor_L))
				SetFloorType (Type.Empty, _newOrientation);
        }

		CanConnect_L = false;
		CanConnect_T = false;
		CanConnect_R = false;
		CanConnect_B = false;

        switch (_newType) {
            case Type.Empty:
                Walkable = true;
                DefaultPositionWorld = WorldPosition;
                break;
            case Type.Solid:
                Walkable = false;
                CanConnect_L = true;
                CanConnect_T = true;
                CanConnect_R = true;
                CanConnect_B = true;

				SetFloorType (Type.Empty, _newOrientation);
                break;
			case Type.Diagonal:
				Walkable = true;
                switch (_newOrientation) {
                    case TileOrientation.BottomLeft:
                        DefaultPositionWorld = WorldPosition + new Vector3(0.25f, 0.25f, 0);
                        CanConnect_L = true;
                        CanConnect_B = true;
                        break;
                    case TileOrientation.TopLeft:
                        DefaultPositionWorld = WorldPosition + new Vector3(0.25f, -0.25f, 0);
                        CanConnect_L = true;
                        CanConnect_T = true;
                        break;
                    case TileOrientation.TopRight:
                        DefaultPositionWorld = WorldPosition + new Vector3(-0.25f, -0.25f, 0);
                        CanConnect_T = true;
                        CanConnect_R = true;
                        break;
                    case TileOrientation.BottomRight:
                        DefaultPositionWorld = WorldPosition + new Vector3(-0.25f, 0.25f, 0);
                        CanConnect_R = true;
                        CanConnect_B = true;
                        break;
                }
				if (_FloorType_ != Type.Empty)
					SetFloorType (Type.Diagonal, GetReverseDirection(_newOrientation));
				break;
            case Type.Door:
                Walkable = true;
                ForceActorStopWhenPassingThis = true;
				switch (_Orientation_) {
                    // vertical
                    case TileOrientation.Bottom:
                    case TileOrientation.Top:
                        DefaultPositionWorld = WorldPosition + new Vector3(0, -0.15f, 0);

                        CanConnect_T = true;
                        CanConnect_B = true;
                        break;
                    // horizontal
                    case TileOrientation.Left:
                    case TileOrientation.Right:
                        CanConnect_L = true;
                        CanConnect_R = true;
                        break;
                }

				SetFloorType (Type.Empty, _newOrientation);
                break;
            case Type.Airlock:
                Walkable = true;
                ForceActorStopWhenPassingThis = true;
				switch (_Orientation_) {
                    // vertical
                    case TileOrientation.Bottom:
                    case TileOrientation.Top:
                        DefaultPositionWorld = WorldPosition + new Vector3(0, -0.25f, 0);

                        CanConnect_T = true;
                        CanConnect_B = true;
                        break;
                    // horizontal
                    case TileOrientation.Left:
                    case TileOrientation.Right:
                        DefaultPositionWorld = WorldPosition + new Vector3(0, -0.35f, 0);

                        CanConnect_L = true;
                        CanConnect_R = true;
                        break;
                }

				SetFloorType (Type.Empty, _newOrientation);
                break;
            default:
                throw new System.Exception(_newType.ToString() + " has not been properly implemented yet!");
        }

		cachedNeighbour_L = GridX > 0 ? Grid.Instance.grid[GridX - 1, GridY] : null;
		cachedNeighbour_T = GridY < Grid.Instance.GridSizeY - 1 ? Grid.Instance.grid[GridX, GridY + 1] : null;
		cachedNeighbour_R = GridX < Grid.Instance.GridSizeX - 1 ? Grid.Instance.grid[GridX + 1, GridY] : null;
		cachedNeighbour_B = GridY > 0 ? Grid.Instance.grid[GridX, GridY - 1] : null;

		if (cachedNeighbour_L != null)
			UpdateNeighbourWall(cachedNeighbour_L, TileOrientation.Left);
		if (cachedNeighbour_T != null)
			UpdateNeighbourWall(cachedNeighbour_T, TileOrientation.Top);
		if (cachedNeighbour_R != null)
			UpdateNeighbourWall(cachedNeighbour_R, TileOrientation.Right);
		if (cachedNeighbour_B != null)
			UpdateNeighbourWall(cachedNeighbour_B, TileOrientation.Bottom);

		ChangeWallGraphics (
			CachedAssets.Instance.GetWallAssetForTile (_WallType_, _Orientation_, 0, true, HasConnectable_L, HasConnectable_T, HasConnectable_R, HasConnectable_B),
			CachedAssets.Instance.GetWallAssetForTile (_WallType_, _Orientation_, 0, false, HasConnectable_L, HasConnectable_T, HasConnectable_R, HasConnectable_B));
    }
	public void SetFloorType(Type _newType, TileOrientation _newOrientation){

		if (_FloorType_ == Type.Diagonal) {
			if(GridX > 0)
				Grid.Instance.grid[GridX - 1, GridY].ConnectedDiagonalFloor_R = null;
			if(GridX < Grid.Instance.GridSizeX - 1)
				Grid.Instance.grid[GridX + 1, GridY].ConnectedDiagonalFloor_L = null;
			if(GridY > 0)
				Grid.Instance.grid[GridX, GridY - 1].ConnectedDiagonalFloor_T = null;
			if (GridY < Grid.Instance.GridSizeY - 1)
				Grid.Instance.grid[GridX, GridY + 1].ConnectedDiagonalFloor_B = null;
		}

		_FloorType_ = _newType;
		_FloorOrientation_ = _newOrientation;

		FloorQuad.Type = _newType;
		FloorQuad.Orientation = _newOrientation;
		FloorQuad.SortingLayer = UVController.SortingLayerEnum.Floor;
		FloorQuad.Sort(GridY);

		ForceActorStopWhenPassingThis = false;
		MovementPenalty = 0; //TODO: use this for something!


		CanConnectFloor_L = false;
		CanConnectFloor_T = false;
		CanConnectFloor_R = false;
		CanConnectFloor_B = false;

		switch (_newType) {
			case Type.Empty:
				break;
			case Type.Solid:
				CanConnectFloor_L = true;
				CanConnectFloor_T = true;
				CanConnectFloor_R = true;
				CanConnectFloor_B = true;
				break;
			case Type.Diagonal:
				switch (_newOrientation) {
					case TileOrientation.BottomLeft:
						CanConnectFloor_L = true;
						CanConnectFloor_B = true;
						break;
					case TileOrientation.TopLeft:
						CanConnectFloor_L = true;
						CanConnectFloor_T = true;
						break;
					case TileOrientation.TopRight:
						CanConnectFloor_T = true;
						CanConnectFloor_R = true;
						break;
					case TileOrientation.BottomRight:
						CanConnectFloor_R = true;
						CanConnectFloor_B = true;
						break;
				}
				break;
			case Type.Door:
			case Type.Airlock:
				throw new System.Exception (_newType.ToString() + " isn't applicable to Floor!");
			default:
				throw new System.Exception(_newType.ToString() + " has not been properly implemented yet!");
		}

		cachedNeighbour_L = GridX > 0 ? Grid.Instance.grid[GridX - 1, GridY] : null;
		cachedNeighbour_T = GridY < Grid.Instance.GridSizeY - 1 ? Grid.Instance.grid[GridX, GridY + 1] : null;
		cachedNeighbour_R = GridX < Grid.Instance.GridSizeX - 1 ? Grid.Instance.grid[GridX + 1, GridY] : null;
		cachedNeighbour_B = GridY > 0 ? Grid.Instance.grid[GridX, GridY - 1] : null;
		if (cachedNeighbour_L != null)
			UpdateNeighbourFloor(cachedNeighbour_L, TileOrientation.Left);
		if (cachedNeighbour_T != null)
			UpdateNeighbourFloor(cachedNeighbour_T, TileOrientation.Top);
		if (cachedNeighbour_R != null)
			UpdateNeighbourFloor(cachedNeighbour_R, TileOrientation.Right);
		if (cachedNeighbour_B != null)
			UpdateNeighbourFloor(cachedNeighbour_B, TileOrientation.Bottom);

		ChangeFloorGraphics (CachedAssets.Instance.GetFloorAssetForTile(_FloorType_, _FloorOrientation_, 0, HasConnectableFloor_L, HasConnectableFloor_T, HasConnectableFloor_R, HasConnectableFloor_B));
	}

	// WARNING: this doesn't support changing the type and orientation of the tile, so if you're gonna change the type of a tile
	// you're gonna want to update its neighbours, but with something more fleshed out than this!
    void UpdateNeighbourWall(Tile _neighbour, TileOrientation _directionFromThisTile) {
        switch (_directionFromThisTile) {
			case TileOrientation.Bottom:
				_neighbour.HasConnectable_T = CanConnect_B;
				_neighbour.IsBlocked_T = Grid.OtherTileIsBlockingPath(_WallType_, _Orientation_, TileOrientation.Top);
				_neighbour.ConnectedDiagonal_T = (_WallType_ == Type.Diagonal && (_Orientation_ == TileOrientation.BottomLeft || _Orientation_ == TileOrientation.BottomRight)) ? this : null;
				_neighbour.ConnectedDoorOrAirlock_T = (_WallType_ == Type.Door || _WallType_ == Type.Airlock) ? this : null;

                // prevent building in front of door
				if ((_WallType_ == Type.Door || _WallType_ == Type.Airlock) && _IsHorizontal_)
                    _neighbour.SetBuildingAllowed(false);
                break;
            case TileOrientation.Left:
				_neighbour.HasConnectable_R = CanConnect_L;
				_neighbour.IsBlocked_L = Grid.OtherTileIsBlockingPath(_WallType_, _Orientation_, TileOrientation.Left);
				_neighbour.ConnectedDiagonal_R = (_WallType_ == Type.Diagonal && (_Orientation_ == TileOrientation.TopLeft || _Orientation_ == TileOrientation.BottomLeft)) ? this : null;
				_neighbour.ConnectedDoorOrAirlock_R = (_WallType_ == Type.Door || _WallType_ == Type.Airlock) ? this : null;

                // prevent building in front of door
				if ((_WallType_ == Type.Door || _WallType_ == Type.Airlock) && _IsVertical_)
                    _neighbour.SetBuildingAllowed(false);
                break;
            case TileOrientation.Top:
				_neighbour.HasConnectable_B = CanConnect_T;
				_neighbour.IsBlocked_B = Grid.OtherTileIsBlockingPath(_WallType_, _Orientation_, TileOrientation.Bottom);
				_neighbour.ConnectedDiagonal_B = (_WallType_ == Type.Diagonal && (_Orientation_ == TileOrientation.TopLeft || _Orientation_ == TileOrientation.TopRight)) ? this : null;
				_neighbour.ConnectedDoorOrAirlock_B = (_WallType_ == Type.Door || _WallType_ == Type.Airlock) ? this : null;

				if (_WallType_ == Type.Door || _WallType_ == Type.Airlock) {

                    // prevent building in front of door
					if (_IsHorizontal_)
                        _neighbour.SetBuildingAllowed(false);

                    // sort connected neighbour of door on top, so as to hide actors moving through it
					else if (_IsVertical_) {
                        _neighbour.BottomQuad.SortCustom(TopQuad.GetSortOrder() - 2);
                        _neighbour.TopQuad.SortCustom(TopQuad.GetSortOrder() - 1);
                    }
                }
				else if (_PrevType_ == Type.Door || _PrevType_ == Type.Airlock) {
                    // reset to ordinary sorting
					if (_IsVertical_) {
                        _neighbour.BottomQuad.RemoveCustomSort();
                        _neighbour.TopQuad.RemoveCustomSort();
                    }
                }
                break;
            case TileOrientation.Right:
				_neighbour.HasConnectable_L = CanConnect_R;
				_neighbour.IsBlocked_L = Grid.OtherTileIsBlockingPath(_WallType_, _Orientation_, TileOrientation.Left);
				_neighbour.ConnectedDiagonal_L = (_WallType_ == Type.Diagonal && (_Orientation_ == TileOrientation.BottomRight || _Orientation_ == TileOrientation.TopRight)) ? this : null;
				_neighbour.ConnectedDoorOrAirlock_L = (_WallType_ == Type.Door || _WallType_ == Type.Airlock) ? this : null;

                // prevent building in front of door
				if ((_WallType_ == Type.Door || _WallType_ == Type.Airlock) && _IsVertical_)
                   _neighbour.SetBuildingAllowed(false);
                break;
			case TileOrientation.TopLeft:
			case TileOrientation.TopRight:
			case TileOrientation.BottomRight:
			case TileOrientation.BottomLeft:
				break;

            default:
                throw new System.NotImplementedException("Ah! UpdateNeighbour() doesn't support " + _directionFromThisTile.ToString() + " as a direction yet!");
        }

		_neighbour.ChangeWallGraphics (
			CachedAssets.Instance.GetWallAssetForTile (_neighbour._WallType_, _neighbour._Orientation_, 0, true, _neighbour.HasConnectable_L, _neighbour.HasConnectable_T, _neighbour.HasConnectable_R, _neighbour.HasConnectable_B),
			CachedAssets.Instance.GetWallAssetForTile (_neighbour._WallType_, _neighbour._Orientation_, 0, false, _neighbour.HasConnectable_L, _neighbour.HasConnectable_T, _neighbour.HasConnectable_R, _neighbour.HasConnectable_B));
	}
	// WARNING: this doesn't support changing the type and orientation of the tile, so if you're gonna change the type of a tile
	// you're gonna want to update its neighbours, but with something more fleshed out than this!
	void UpdateNeighbourFloor(Tile _neighbour, TileOrientation _directionFromThisTile) {
		switch (_directionFromThisTile) {
			case TileOrientation.Bottom:
				_neighbour.HasConnectableFloor_T = CanConnectFloor_B;
				_neighbour.ConnectedDiagonalFloor_T = (_FloorType_ == Type.Diagonal && (_FloorOrientation_ == TileOrientation.BottomLeft || _FloorOrientation_ == TileOrientation.BottomRight)) ? this : null;
				break;
			case TileOrientation.Left:
				_neighbour.HasConnectableFloor_R = CanConnectFloor_L;
				_neighbour.ConnectedDiagonalFloor_R = (_FloorType_ == Type.Diagonal && (_FloorOrientation_ == TileOrientation.TopLeft || _FloorOrientation_ == TileOrientation.BottomLeft)) ? this : null;
				break;
			case TileOrientation.Top:
				_neighbour.HasConnectableFloor_B = CanConnectFloor_T;
				_neighbour.ConnectedDiagonalFloor_B = (_FloorType_ == Type.Diagonal && (_FloorOrientation_ == TileOrientation.TopLeft || _FloorOrientation_ == TileOrientation.TopRight)) ? this : null;
				break;
			case TileOrientation.Right:
				_neighbour.HasConnectableFloor_L = CanConnectFloor_R;
				_neighbour.ConnectedDiagonalFloor_L = (_FloorType_ == Type.Diagonal && (_FloorOrientation_ == TileOrientation.BottomRight || _FloorOrientation_ == TileOrientation.TopRight)) ? this : null;
				break;

			default:
				throw new System.NotImplementedException("Ah! UpdateNeighbour() doesn't support " + _directionFromThisTile.ToString() + " as a direction yet!");
		}

		_neighbour.ChangeFloorGraphics (CachedAssets.Instance.GetFloorAssetForTile(_neighbour._FloorType_, _neighbour._FloorOrientation_, 0, _neighbour.HasConnectableFloor_L, _neighbour.HasConnectableFloor_T, _neighbour.HasConnectableFloor_R, _neighbour.HasConnectableFloor_B));
	}
   	
	public void ChangeWallGraphics(CachedAssets.DoubleInt _bottomAssetIndices, CachedAssets.DoubleInt _topAssetIndices) {
		BottomQuad.ChangeAsset(_bottomAssetIndices);
        TopQuad.ChangeAsset(_topAssetIndices);
    }
	public void ChangeFloorGraphics(CachedAssets.DoubleInt _assetIndices) {
		FloorQuad.ChangeAsset(_assetIndices);
	}

    public void OnActorApproachingTile(TileOrientation _direction) {
		switch (_WallType_) {
            case Type.Empty:
            case Type.Solid:
            case Type.Diagonal:
                break;
            case Type.Door:
                Animator.Animate(Animator.GetDoorAnimation(TileAnimator.AnimationContextEnum.Open), _forward: true, _loop: false);
                break;
            case Type.Airlock:
                Animator.Animate(Animator.GetAirlockAnimation(TileAnimator.AnimationContextEnum.Open, _direction), _forward: true, _loop: false);
                break;
            default:
				throw new System.NotImplementedException(_WallType_ + " hasn't been properly implemented yet!");
        }
    }
    TileAnimator.TileAnimation[] animationSequence;
    public void OnActorEnterTile(TileOrientation _direction, out float _yieldTime) {
        _yieldTime = 0;
		switch (_WallType_) {
            case Type.Empty:
            case Type.Solid:
            case Type.Diagonal:
                break;
            case Type.Door:
                Animator.Animate(Animator.GetDoorAnimation(TileAnimator.AnimationContextEnum.Close), _forward: true, _loop: false);
                break;
            case Type.Airlock:
                animationSequence = new TileAnimator.TileAnimation[] {
                    Animator.GetAirlockAnimation(TileAnimator.AnimationContextEnum.Close, _direction),
                    Animator.GetAirlockAnimation(TileAnimator.AnimationContextEnum.Wait, TileOrientation.None),
                    Animator.GetAirlockAnimation(TileAnimator.AnimationContextEnum.Open, GetReverseDirection(_direction)),
                    Animator.GetAirlockAnimation(TileAnimator.AnimationContextEnum.Close, GetReverseDirection(_direction)) };

                Animator.AnimateSequence(animationSequence);
                _yieldTime = Animator.GetProperWaitTimeForAnim(animationSequence[0]) + Animator.GetProperWaitTimeForAnim(animationSequence[1]) + (Animator.GetProperWaitTimeForAnim(animationSequence[2]) * 0.5f);
                break;
            default:
				throw new System.NotImplementedException(_WallType_ + " hasn't been properly implemented yet!");
        }
    }

    public void SetBuildingAllowed(bool _b) {
        Tile _neighbour;
        bool _isAdjacentHorizontally = false;
        bool _isAdjacentVertically = false;
		_BuildingAllowed_ = false;
        if (_b) {
            int _gridX, _gridY;
            for (int y = -1; y <= 1; y++) {
                for (int x = -1; x <= 1; x++) {
					if (x == 0 && y == 0) // need to be able to remove the source of the non-allowance :/
						continue;

					_gridX = GridX + x;
                    _gridY = GridY + y;

                    if (_gridX >= 0 && _gridX < Grid.Instance.GridSizeX && _gridY >= 0 && _gridY < Grid.Instance.GridSizeY) {
						_neighbour = Grid.Instance.grid[_gridX, _gridY];

                        _isAdjacentHorizontally = x != 0 && y == 0;
                        _isAdjacentVertically = x == 0 && y != 0;

						// fail conditions
                        if ((_isAdjacentHorizontally || _isAdjacentVertically) &&  _neighbour._WallType_ == Type.Door)
                            return;
						if ((_isAdjacentHorizontally || _isAdjacentVertically) &&  _neighbour._WallType_ == Type.Airlock)
							return;
                    }
                }
            }
        }

        _BuildingAllowed_ = _b;
    }
}
