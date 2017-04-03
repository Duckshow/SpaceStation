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
        return TileOrientation.None;
    }

    public Type TempType = Type.Empty;
	public TileOrientation TempOrientation = TileOrientation.None;

    public bool Walkable { get; private set; }
    public bool IsOccupiedByObject = false;
    public CanInspect OccupyingInspectable;
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

    [HideInInspector] public bool HideFloorCorner_TL = false;
    [HideInInspector] public bool HideFloorCorner_TR = false;
    [HideInInspector] public bool HideFloorCorner_BR = false;
    [HideInInspector] public bool HideFloorCorner_BL = false;
    [HideInInspector] public bool HideWallCorner_Tl = false;
    [HideInInspector] public bool HideWallCorner_TR = false;
    [HideInInspector] public bool HideWallCorner_BR = false;
    [HideInInspector] public bool HideWallCorner_BL = false;

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
    public UVController FloorCornerHider;
    public UVController BottomQuad;
    public UVController TopQuad;
    public UVController WallCornerHider;
    public TileAnimator Animator;

    public bool StopAheadAndBehindMeWhenCrossing { get; private set; }
    public int MovementPenalty { get; private set; }
    public bool ForceActorStopWhenPassingThis { get; private set; }

	public int ThingsUsingThis = 0;


    public Tile(Vector3 _worldPos, int _gridX, int _gridY) {
        WorldPosition = _worldPos;
        GridX = _gridX;
        GridY = _gridY;

        FloorQuad = ((GameObject)Grid.Instantiate(CachedAssets.Instance.TilePrefab, new Vector3(WorldPosition.x, WorldPosition.y + 0.5f, 0), Quaternion.identity)).GetComponent<UVController>();
        FloorCornerHider = ((GameObject)Grid.Instantiate(CachedAssets.Instance.TilePrefab, new Vector3(WorldPosition.x, WorldPosition.y + 0.5f, 0), Quaternion.identity)).GetComponent<UVController>();
        BottomQuad = ((GameObject)Grid.Instantiate(CachedAssets.Instance.TilePrefab, new Vector3(WorldPosition.x, WorldPosition.y + 0.5f, 0), Quaternion.identity)).GetComponent<UVController>();
        TopQuad = ((GameObject)Grid.Instantiate(CachedAssets.Instance.TilePrefab, new Vector3(WorldPosition.x, WorldPosition.y + 0.5f, 0), Quaternion.identity)).GetComponent<UVController>();
        WallCornerHider = ((GameObject)Grid.Instantiate(CachedAssets.Instance.TilePrefab, new Vector3(WorldPosition.x, WorldPosition.y + 0.5f, 0), Quaternion.identity)).GetComponent<UVController>();

        FloorQuad.name = "TileQuad " + GridX + "x" + GridY + " (" + WorldPosition.x + ", " + WorldPosition.y + ")";
        FloorCornerHider.transform.parent = FloorQuad.transform;
        BottomQuad.transform.parent = FloorQuad.transform;
		TopQuad.transform.parent = BottomQuad.transform;
        WallCornerHider.transform.parent = FloorQuad.transform;

        FloorCornerHider.transform.localPosition = new Vector3(0, 0, -0.01f);
        WallCornerHider.transform.localPosition = new Vector3(0, 0, -0.01f);

        FloorQuad.Setup ();
        FloorCornerHider.Setup();
		BottomQuad.Setup();
        TopQuad.Setup();
        WallCornerHider.Setup();
        Animator = new TileAnimator(this);
    }
		
    public int CompareTo(Tile nodeToCompare) {
        int compare = _FCost_.CompareTo(nodeToCompare._FCost_);
        if (compare == 0)
            compare = HCost.CompareTo(nodeToCompare.HCost);

        return -compare;
    }

    public static Tile sCachedNeighbour_L;
    public static bool sTryTempCacheNeighbour_L(int _gridX, int _gridY) {
        sCachedNeighbour_L = _gridX > 0 ? Grid.Instance.grid[_gridX - 1, _gridY] : null;
        return sCachedNeighbour_L != null;
    }
    public static Tile sCachedNeighbour_T;
    public static bool sTryTempCacheNeighbour_T(int _gridX, int _gridY) {
        sCachedNeighbour_T = _gridY < Grid.Instance.GridSizeY - 1 ? Grid.Instance.grid[_gridX, _gridY + 1] : null;
        return sCachedNeighbour_T != null;
    }
    public static Tile sCachedNeighbour_R;
    public static bool sTryTempCacheNeighbour_R(int _gridX, int _gridY) {
        sCachedNeighbour_R = _gridX < Grid.Instance.GridSizeX - 1 ? Grid.Instance.grid[_gridX + 1, _gridY] : null;
        return sCachedNeighbour_R != null;
    }
    public static Tile sCachedNeighbour_B;
    public static bool sTryTempCacheNeighbour_B(int _gridX, int _gridY) {
        sCachedNeighbour_B = _gridY > 0 ? Grid.Instance.grid[_gridX, _gridY - 1] : null;
        return sCachedNeighbour_B != null;
    }
    public static Tile sCachedNeighbour_TL;
    public static bool sTryTempCacheNeighbour_TL(int _gridX, int _gridY) {
        sCachedNeighbour_TL = _gridX > 0 && _gridY < Grid.Instance.GridSizeY - 1 ? Grid.Instance.grid[_gridX - 1, _gridY + 1] : null;
        return sCachedNeighbour_TL != null;
    }
    public static Tile sCachedNeighbour_TR;
    public static bool sTryTempCacheNeighbour_TR(int _gridX, int _gridY) {
        sCachedNeighbour_TR = _gridX < Grid.Instance.GridSizeX - 1 && _gridY < Grid.Instance.GridSizeY - 1 ? Grid.Instance.grid[_gridX + 1, _gridY + 1] : null;
        return sCachedNeighbour_TR != null;
    }
    public static Tile sCachedNeighbour_BR;
    public static bool sTryTempCacheNeighbour_BR(int _gridX, int _gridY) {
        sCachedNeighbour_BR = _gridX < Grid.Instance.GridSizeX - 1 && _gridY > 0 ? Grid.Instance.grid[_gridX + 1, _gridY - 1] : null;
        return sCachedNeighbour_BR != null;
    }
    public static Tile sCachedNeighbour_BL;
    public static bool sTryTempCacheNeighbour_BL(int _gridX, int _gridY) {
        sCachedNeighbour_BL = _gridX > 0 && _gridY > 0 ? Grid.Instance.grid[_gridX - 1, _gridY - 1] : null;
        return sCachedNeighbour_BL != null;
    }

    public void SetTileType(Type _newType, TileOrientation _newOrientation) {
        Animator.StopAnimating();

		prevType = _WallType_;
		_WallType_ = _newType;
		prevOrientation = _Orientation_;
		_Orientation_ = _newOrientation;

        BottomQuad.Orientation = _newOrientation;
		BottomQuad.SortingLayer = UVController.SortingLayerEnum.Bottom;
        BottomQuad.Sort(GridY);

        TopQuad.Orientation = _newOrientation;
		TopQuad.SortingLayer = UVController.SortingLayerEnum.Top;
        TopQuad.Sort(GridY);

        WallCornerHider.SortingLayer = UVController.SortingLayerEnum.Top;
        WallCornerHider.Sort(GridY);

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

		if (sTryTempCacheNeighbour_L(GridX, GridY))
			UpdateNeighbourWall(sCachedNeighbour_L, TileOrientation.Left);
		if (sTryTempCacheNeighbour_T(GridX, GridY))
			UpdateNeighbourWall(sCachedNeighbour_T, TileOrientation.Top);
		if (sTryTempCacheNeighbour_R(GridX, GridY))
			UpdateNeighbourWall(sCachedNeighbour_R, TileOrientation.Right);
		if (sTryTempCacheNeighbour_B(GridX, GridY))
			UpdateNeighbourWall(sCachedNeighbour_B, TileOrientation.Bottom);

        if (sTryTempCacheNeighbour_TL(GridX, GridY))
            UpdateNeighbourWall(sCachedNeighbour_TL, TileOrientation.TopLeft);
        if (sTryTempCacheNeighbour_TR(GridX, GridY))
            UpdateNeighbourWall(sCachedNeighbour_TR, TileOrientation.TopRight);
        if (sTryTempCacheNeighbour_BR(GridX, GridY))
            UpdateNeighbourWall(sCachedNeighbour_BR, TileOrientation.BottomRight);
        if (sTryTempCacheNeighbour_BL(GridX, GridY))
            UpdateNeighbourWall(sCachedNeighbour_BL, TileOrientation.BottomLeft);

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

		FloorQuad.Orientation = _newOrientation;
		FloorQuad.SortingLayer = UVController.SortingLayerEnum.Floor;
		FloorQuad.Sort(GridY);

        FloorCornerHider.SortingLayer = UVController.SortingLayerEnum.Floor;
        FloorCornerHider.Sort(GridY);

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

		if (sTryTempCacheNeighbour_L(GridX, GridY))
			UpdateNeighbourFloor(sCachedNeighbour_L, TileOrientation.Left);
		if (sTryTempCacheNeighbour_T(GridX, GridY))
			UpdateNeighbourFloor(sCachedNeighbour_T, TileOrientation.Top);
		if (sTryTempCacheNeighbour_R(GridX, GridY))
			UpdateNeighbourFloor(sCachedNeighbour_R, TileOrientation.Right);
		if (sTryTempCacheNeighbour_B(GridX, GridY))
			UpdateNeighbourFloor(sCachedNeighbour_B, TileOrientation.Bottom);

        if (sTryTempCacheNeighbour_TL(GridX, GridY))
            UpdateNeighbourFloor(sCachedNeighbour_TL, TileOrientation.TopLeft);
        if (sTryTempCacheNeighbour_TR(GridX, GridY))
            UpdateNeighbourFloor(sCachedNeighbour_TR, TileOrientation.TopRight);
        if (sTryTempCacheNeighbour_BR(GridX, GridY))
            UpdateNeighbourFloor(sCachedNeighbour_BR, TileOrientation.BottomRight);
        if (sTryTempCacheNeighbour_BL(GridX, GridY))
            UpdateNeighbourFloor(sCachedNeighbour_BL, TileOrientation.BottomLeft);

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
                _neighbour.UpdateWallCornerHider();
                return;
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
            case TileOrientation.TopLeft:
            case TileOrientation.TopRight:
            case TileOrientation.BottomRight:
            case TileOrientation.BottomLeft:
                _neighbour.UpdateFloorCornerHider();
                return;
		}

		_neighbour.ChangeFloorGraphics (CachedAssets.Instance.GetFloorAssetForTile(_neighbour._FloorType_, _neighbour._FloorOrientation_, 0, _neighbour.HasConnectableFloor_L, _neighbour.HasConnectableFloor_T, _neighbour.HasConnectableFloor_R, _neighbour.HasConnectableFloor_B));
    }

    public void UpdateWallCornerHider() {
        sTryTempCacheNeighbour_L(GridX, GridY);
        sTryTempCacheNeighbour_T(GridX, GridY);
        sTryTempCacheNeighbour_R(GridX, GridY);
        sTryTempCacheNeighbour_B(GridX, GridY);

        WallCornerHider.ChangeAsset(CachedAssets.Instance.GetWallCornerAsset(
            CanConnect_T && HasConnectable_T && sCachedNeighbour_T != null && sCachedNeighbour_T.CanConnect_L && sCachedNeighbour_T.HasConnectable_L && CanConnect_L && HasConnectable_L && sCachedNeighbour_L != null && sCachedNeighbour_L.CanConnect_T && sCachedNeighbour_L.HasConnectable_T,
            CanConnect_T && HasConnectable_T && sCachedNeighbour_T != null && sCachedNeighbour_T.CanConnect_R && sCachedNeighbour_T.HasConnectable_R && CanConnect_R && HasConnectable_R && sCachedNeighbour_R != null && sCachedNeighbour_R.CanConnect_T && sCachedNeighbour_R.HasConnectable_T,
            CanConnect_B && HasConnectable_B && sCachedNeighbour_B != null && sCachedNeighbour_B.CanConnect_R && sCachedNeighbour_B.HasConnectable_R && CanConnect_R && HasConnectable_R && sCachedNeighbour_R != null && sCachedNeighbour_R.CanConnect_B && sCachedNeighbour_R.HasConnectable_B,
            CanConnect_B && HasConnectable_B && sCachedNeighbour_B != null && sCachedNeighbour_B.CanConnect_L && sCachedNeighbour_B.HasConnectable_L && CanConnect_L && HasConnectable_L && sCachedNeighbour_L != null && sCachedNeighbour_L.CanConnect_B && sCachedNeighbour_L.HasConnectable_B
         ));
    }
    public void UpdateFloorCornerHider() {
        sTryTempCacheNeighbour_L(GridX, GridY);
        sTryTempCacheNeighbour_T(GridX, GridY);
        sTryTempCacheNeighbour_R(GridX, GridY);
        sTryTempCacheNeighbour_B(GridX, GridY);

        FloorCornerHider.ChangeAsset(CachedAssets.Instance.GetFloorCornerAsset(
            CanConnectFloor_T && HasConnectableFloor_T && sCachedNeighbour_T != null && sCachedNeighbour_T.CanConnectFloor_L && sCachedNeighbour_T.HasConnectableFloor_L && CanConnectFloor_L && HasConnectableFloor_L && sCachedNeighbour_L != null && sCachedNeighbour_L.CanConnectFloor_T && sCachedNeighbour_L.HasConnectableFloor_T,
            CanConnectFloor_T && HasConnectableFloor_T && sCachedNeighbour_T != null && sCachedNeighbour_T.CanConnectFloor_R && sCachedNeighbour_T.HasConnectableFloor_R && CanConnectFloor_R && HasConnectableFloor_R && sCachedNeighbour_R != null && sCachedNeighbour_R.CanConnectFloor_T && sCachedNeighbour_R.HasConnectableFloor_T,
            CanConnectFloor_B && HasConnectableFloor_B && sCachedNeighbour_B != null && sCachedNeighbour_B.CanConnectFloor_R && sCachedNeighbour_B.HasConnectableFloor_R && CanConnectFloor_R && HasConnectableFloor_R && sCachedNeighbour_R != null && sCachedNeighbour_R.CanConnectFloor_B && sCachedNeighbour_R.HasConnectableFloor_B,
            CanConnectFloor_B && HasConnectableFloor_B && sCachedNeighbour_B != null && sCachedNeighbour_B.CanConnectFloor_L && sCachedNeighbour_B.HasConnectableFloor_L && CanConnectFloor_L && HasConnectableFloor_L && sCachedNeighbour_L != null && sCachedNeighbour_L.CanConnectFloor_B && sCachedNeighbour_L.HasConnectableFloor_B
        ));
    }

    public void ChangeWallGraphics(CachedAssets.DoubleInt _bottomAssetIndices, CachedAssets.DoubleInt _topAssetIndices, bool _temporary = false) {
		BottomQuad.ChangeAsset(_bottomAssetIndices, _temporary);
        TopQuad.ChangeAsset(_topAssetIndices, _temporary);
		UpdateWallCornerHider ();
		UpdateFloorCornerHider ();
    }
	public void ChangeFloorGraphics(CachedAssets.DoubleInt _assetIndices, bool _temporary = false) {
		FloorQuad.ChangeAsset(_assetIndices, _temporary);
		UpdateWallCornerHider ();
		UpdateFloorCornerHider ();
	}
    public void SetColor(Color32 _color32) {
        FloorQuad.ChangeColor(_color32);
        BottomQuad.ChangeColor(_color32);
        TopQuad.ChangeColor(_color32);
		WallCornerHider.ChangeColor (_color32);
		FloorCornerHider.ChangeColor (_color32);
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
