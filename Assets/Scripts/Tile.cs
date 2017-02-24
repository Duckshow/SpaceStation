using UnityEngine;

public class Tile : IHeapItem<Tile> {

    public enum TileType { Empty, Wall, Diagonal, Door, Airlock }
    private TileType type = TileType.Empty;
    public TileType _Type_ { get { return type; } }
    private TileType prevType = TileType.Empty;
    public TileType _PrevType_ { get { return prevType; } }
    public enum TileOrientation { None, Bottom, BottomLeft, Left, TopLeft, Top, TopRight, Right, BottomRight }
    private TileOrientation orientation = TileOrientation.None;
    public TileOrientation _Orientation_ { get { return orientation; } }
    private TileOrientation prevOrientation = TileOrientation.None;
    public bool _IsHorizontal_ { get { return orientation == TileOrientation.Left || orientation == TileOrientation.Right; } }
    public bool _IsVertical_ { get { return orientation == TileOrientation.Bottom || orientation == TileOrientation.Top; } }
    Tile.TileOrientation GetReverseDirection(Tile.TileOrientation _direction) {
        switch (_direction) {
            case Tile.TileOrientation.Bottom:
                return Tile.TileOrientation.Top;
            case Tile.TileOrientation.Left:
                return Tile.TileOrientation.Right;
            case Tile.TileOrientation.Top:
                return Tile.TileOrientation.Bottom;
            case Tile.TileOrientation.Right:
                return Tile.TileOrientation.Left;
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
            if (type == TileType.Empty) {
                Vector3 _offset = Vector3.zero;
                _offset.x = IsBlocked_L ? -0.25f : IsBlocked_R ? 0.25f : 0;
                _offset.y = IsBlocked_B ? -0.25f : IsBlocked_T ? 0.25f : 0;
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

    [HideInInspector] public bool HasConnectable_L = false;
    [HideInInspector] public bool HasConnectable_T = false;
    [HideInInspector] public bool HasConnectable_R = false;
    [HideInInspector] public bool HasConnectable_B = false;

    [HideInInspector] public bool IsBlocked_L = false;
    [HideInInspector] public bool IsBlocked_T = false;
    [HideInInspector] public bool IsBlocked_R = false;
    [HideInInspector] public bool IsBlocked_B = false;

    // optimization: could probably cache these some smarter way
    [HideInInspector] public Tile ConnectedDiagonal_L;
	[HideInInspector] public Tile ConnectedDiagonal_T;
	[HideInInspector] public Tile ConnectedDiagonal_R;
	[HideInInspector] public Tile ConnectedDiagonal_B;
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
        BottomQuad = ((GameObject)Grid.Instantiate(CachedAssets.Instance.TilePrefab, new Vector3(WorldPosition.x, WorldPosition.y + 0.5f, Grid.WORLD_BOTTOM_HEIGHT), Quaternion.identity)).GetComponent<UVController>();
        TopQuad = ((GameObject)Grid.Instantiate(CachedAssets.Instance.TilePrefab, new Vector3(WorldPosition.x, WorldPosition.y + 0.5f, Grid.WORLD_TOP_HEIGHT), Quaternion.identity)).GetComponent<UVController>();
        
        BottomQuad.name = "TileQuad " + GridX + "x" + GridY + " (" + WorldPosition.x + ", " + WorldPosition.y + ")"; 
		TopQuad.transform.parent = BottomQuad.transform;

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
    public void SetTileType(TileType _newType, TileOrientation _newOrientation) {

        Animator.StopAnimating();

        BottomQuad.HaveChangedGraphics = true;
        prevType = type;
        type = _newType;
        prevOrientation = orientation;
        orientation = _newOrientation;

        BottomQuad.Type = _newType;
        BottomQuad.Orientation = _newOrientation;
        BottomQuad.IsBottom = true;
        BottomQuad.Sort(GridY);
        //bottomQuad.SortAsBottom(GridY);

        TopQuad.Type = _newType;
        TopQuad.Orientation = _newOrientation;
        TopQuad.IsBottom = false;
        TopQuad.Sort(GridY);
        //topQuad.SortAsTop(GridY);

        ForceActorStopWhenPassingThis = false;
        MovementPenalty = 0; //TODO: use this for something!

        if (prevType == TileType.Door) {
            Grid.Instance.grid[GridX + 1, GridY].SetBuildingAllowed(true);
            Grid.Instance.grid[GridX - 1, GridY].SetBuildingAllowed(true);
            Grid.Instance.grid[GridX, GridY + 1].SetBuildingAllowed(true);
            Grid.Instance.grid[GridX, GridY - 1].SetBuildingAllowed(true);

            Grid.Instance.grid[GridX - 1, GridY].ConnectedDoorOrAirlock_R = null;
            Grid.Instance.grid[GridX + 1, GridY].ConnectedDoorOrAirlock_L = null;
            Grid.Instance.grid[GridX, GridY - 1].ConnectedDoorOrAirlock_T = null;
            Grid.Instance.grid[GridX, GridY + 1].ConnectedDoorOrAirlock_B = null;
        }
        if (prevType == TileType.Diagonal) {
            if(GridX > 0)
                Grid.Instance.grid[GridX - 1, GridY].ConnectedDiagonal_R = null;
            if(GridX < Grid.Instance.GridSizeX - 1)
                Grid.Instance.grid[GridX + 1, GridY].ConnectedDiagonal_L = null;
            if(GridY > 0)
                Grid.Instance.grid[GridX, GridY - 1].ConnectedDiagonal_T = null;
            if (GridY < Grid.Instance.GridSizeY - 1)
                Grid.Instance.grid[GridX, GridY + 1].ConnectedDiagonal_B = null;
        }

        switch (_newType) {
            case TileType.Empty:
                Walkable = true;
                DefaultPositionWorld = WorldPosition;

                CanConnect_L = false;
                CanConnect_T = false;
                CanConnect_R = false;
                CanConnect_B = false;
                break;
            case TileType.Wall:
                Walkable = false;
                CanConnect_L = true;
                CanConnect_T = true;
                CanConnect_R = true;
                CanConnect_B = true;
                break;
            case TileType.Diagonal:
                switch (_newOrientation) {
                    case TileOrientation.BottomLeft:
                        Walkable = true;
                        DefaultPositionWorld = WorldPosition + new Vector3(0.25f, 0.25f, 0);
                        CanConnect_L = true;
                        CanConnect_T = false;
                        CanConnect_R = false;
                        CanConnect_B = true;
                        break;
                    case TileOrientation.TopLeft:
                        Walkable = true;
                        DefaultPositionWorld = WorldPosition + new Vector3(0.25f, -0.25f, 0);
                        CanConnect_L = true;
                        CanConnect_T = true;
                        CanConnect_R = false;
                        CanConnect_B = false;
                        break;
                    case TileOrientation.TopRight:
                        Walkable = true;
                        DefaultPositionWorld = WorldPosition + new Vector3(-0.25f, -0.25f, 0);
                        CanConnect_L = false;
                        CanConnect_T = true;
                        CanConnect_R = true;
                        CanConnect_B = false;
                        break;
                    case TileOrientation.BottomRight:
                        Walkable = true;
                        DefaultPositionWorld = WorldPosition + new Vector3(-0.25f, 0.25f, 0);
                        CanConnect_L = false;
                        CanConnect_T = false;
                        CanConnect_R = true;
                        CanConnect_B = true;
                        break;
                }
                break;
            case TileType.Door:
                Walkable = true;
                ForceActorStopWhenPassingThis = true;
                switch (orientation) {
                    // vertical
                    case TileOrientation.Bottom:
                    case TileOrientation.Top:
                        DefaultPositionWorld = WorldPosition + new Vector3(0, -0.15f, 0);

                        CanConnect_L = false;
                        CanConnect_T = true;
                        CanConnect_R = false;
                        CanConnect_B = true;
                        break;
                    // horizontal
                    case TileOrientation.Left:
                    case TileOrientation.Right:
                        CanConnect_L = true;
                        CanConnect_T = false;
                        CanConnect_R = true;
                        CanConnect_B = false;
                        break;
                }
                break;
            case TileType.Airlock:
                Walkable = true;
                ForceActorStopWhenPassingThis = true;
                switch (orientation) {
                    // vertical
                    case TileOrientation.Bottom:
                    case TileOrientation.Top:
                        DefaultPositionWorld = WorldPosition + new Vector3(0, -0.25f, 0);

                        CanConnect_L = false;
                        CanConnect_T = true;
                        CanConnect_R = false;
                        CanConnect_B = true;
                        break;
                    // horizontal
                    case TileOrientation.Left:
                    case TileOrientation.Right:
                        DefaultPositionWorld = WorldPosition + new Vector3(0, -0.35f, 0);

                        CanConnect_L = true;
                        CanConnect_T = false;
                        CanConnect_R = true;
                        CanConnect_B = false;
                        break;
                }
                break;
            default:
                throw new System.Exception(_newType.ToString() + " has not been properly implemented yet!");
        }

        cachedNeighbour_L = GridX > 0 ? Grid.Instance.grid[GridX - 1, GridY] : null;
        if (cachedNeighbour_L != null)
            UpdateNeighbour(cachedNeighbour_L, TileOrientation.Left);

        cachedNeighbour_T = GridY < Grid.Instance.GridSizeY - 1 ? Grid.Instance.grid[GridX, GridY + 1] : null;
        if (cachedNeighbour_T != null)
            UpdateNeighbour(cachedNeighbour_T, TileOrientation.Top);

        cachedNeighbour_R = GridX < Grid.Instance.GridSizeX - 1 ? Grid.Instance.grid[GridX + 1, GridY] : null;
        if (cachedNeighbour_R != null)
            UpdateNeighbour(cachedNeighbour_R, TileOrientation.Right);

        cachedNeighbour_B = GridY > 0 ? Grid.Instance.grid[GridX, GridY - 1] : null;
        if (cachedNeighbour_B != null)
            UpdateNeighbour(cachedNeighbour_B, TileOrientation.Bottom);

        ChangeGraphics(
            CachedAssets.Instance.GetAssetForTile(type, orientation, 0, true, HasConnectable_L, HasConnectable_T, HasConnectable_R, HasConnectable_B),
			CachedAssets.Instance.GetAssetForTile(type, orientation, 0, false, HasConnectable_L, HasConnectable_T, HasConnectable_R, HasConnectable_B));
    }

    void UpdateNeighbour(Tile _neighbour, TileOrientation _directionFromThisTile) {
        switch (_directionFromThisTile) {
            case TileOrientation.Bottom:
                _neighbour.HasConnectable_T = CanConnect_B;
                _neighbour.IsBlocked_T = Grid.OtherTileIsBlockingPath(type, orientation, TileOrientation.Top);
                _neighbour.ConnectedDiagonal_T = (type == TileType.Diagonal && (orientation == TileOrientation.BottomLeft || orientation == TileOrientation.BottomRight)) ? this : null;
                _neighbour.ConnectedDoorOrAirlock_T = (type == TileType.Door || type == TileType.Airlock) ? this : null;

                // prevent building in front of door
                if (type == TileType.Door && (orientation == TileOrientation.Left || orientation == TileOrientation.Right))
                    _neighbour.SetBuildingAllowed(false);
                break;
            case TileOrientation.Left:
                _neighbour.HasConnectable_R = CanConnect_L;
                _neighbour.IsBlocked_L = Grid.OtherTileIsBlockingPath(type, orientation, TileOrientation.Left);
                _neighbour.ConnectedDiagonal_R = (type == TileType.Diagonal && (orientation == TileOrientation.TopLeft || orientation == TileOrientation.BottomLeft)) ? this : null;
                _neighbour.ConnectedDoorOrAirlock_R = (type == TileType.Door || type == TileType.Airlock) ? this : null;

                // prevent building in front of door
                if (type == TileType.Door && (orientation == TileOrientation.Top || orientation == TileOrientation.Bottom))
                    _neighbour.SetBuildingAllowed(false);
                break;
            case TileOrientation.Top:
                _neighbour.HasConnectable_B = CanConnect_T;
                _neighbour.IsBlocked_B = Grid.OtherTileIsBlockingPath(type, orientation, TileOrientation.Bottom);
                _neighbour.ConnectedDiagonal_B = (type == TileType.Diagonal && (orientation == TileOrientation.TopLeft || orientation == TileOrientation.TopRight)) ? this : null;
                _neighbour.ConnectedDoorOrAirlock_B = (type == TileType.Door || type == TileType.Airlock) ? this : null;

                if (type == TileType.Door) {

                    // prevent building in front of door
                    if (orientation == TileOrientation.Left || orientation == TileOrientation.Right)
                        _neighbour.SetBuildingAllowed(false);

                    // sort connected neighbour of door on top, so as to hide actors moving through it
                    else if (orientation == TileOrientation.Top || orientation == TileOrientation.Bottom) {
                        _neighbour.BottomQuad.SortCustom(TopQuad.GetSortOrder() - 2);
                        _neighbour.TopQuad.SortCustom(TopQuad.GetSortOrder() - 1);
                    }
                }
                else if (prevType == TileType.Door) {
                    // reset to ordinary sorting
                    if (prevOrientation == TileOrientation.Top || prevOrientation == TileOrientation.Bottom) {
                        _neighbour.BottomQuad.RemoveCustomSort();
                        _neighbour.TopQuad.RemoveCustomSort();
                    }
                }
                break;
            case TileOrientation.Right:
                _neighbour.HasConnectable_L = CanConnect_R;
                _neighbour.IsBlocked_L = Grid.OtherTileIsBlockingPath(type, orientation, TileOrientation.Left);
                _neighbour.ConnectedDiagonal_L = (type == TileType.Diagonal && (orientation == TileOrientation.BottomRight || orientation == TileOrientation.TopRight)) ? this : null;
                _neighbour.ConnectedDoorOrAirlock_L = (type == TileType.Door || type == TileType.Airlock) ? this : null;

                // prevent building in front of door
                if (type == TileType.Door && (orientation == TileOrientation.Top || orientation == TileOrientation.Bottom))
                   _neighbour.SetBuildingAllowed(false);
                break;

            default:
                throw new System.NotImplementedException("Ah! UpdateNeighbour() doesn't support " + _directionFromThisTile.ToString() + " as a direction yet!");
        }

		_neighbour.ChangeGraphics (
			CachedAssets.Instance.GetAssetForTile (_neighbour.type, _neighbour.orientation, 0, true, _neighbour.HasConnectable_L, _neighbour.HasConnectable_T, _neighbour.HasConnectable_R, _neighbour.HasConnectable_B),
			CachedAssets.Instance.GetAssetForTile (_neighbour.type, _neighbour.orientation, 0, false, _neighbour.HasConnectable_L, _neighbour.HasConnectable_T, _neighbour.HasConnectable_R, _neighbour.HasConnectable_B));
	}
   	
	public void ChangeGraphics(CachedAssets.DoubleInt _bottomAssetIndices, CachedAssets.DoubleInt _topAssetIndices) {
        BottomQuad.ChangeAsset(_bottomAssetIndices);
        TopQuad.ChangeAsset(_topAssetIndices);
    }

    public void OnActorApproachingTile(TileOrientation _direction) {
        switch (type) {
            case TileType.Empty:
            case TileType.Wall:
            case TileType.Diagonal:
                break;
            case TileType.Door:
                Animator.Animate(Animator.GetDoorAnimation(TileAnimator.AnimationContextEnum.Open), _forward: true, _loop: false);
                break;
            case TileType.Airlock:
                Animator.Animate(Animator.GetAirlockAnimation(TileAnimator.AnimationContextEnum.Open, _direction), _forward: true, _loop: false);
                break;
            default:
                throw new System.NotImplementedException(type + " hasn't been properly implemented yet!");
        }
    }
    TileAnimator.TileAnimation[] animationSequence;
    public void OnActorEnterTile(TileOrientation _direction, out float _yieldTime) {
        _yieldTime = 0;
        switch (type) {
            case TileType.Empty:
            case TileType.Wall:
            case TileType.Diagonal:
                break;
            case TileType.Door:
                Animator.Animate(Animator.GetDoorAnimation(TileAnimator.AnimationContextEnum.Close), _forward: true, _loop: false);
                break;
            case TileType.Airlock:
                animationSequence = new TileAnimator.TileAnimation[] {
                    Animator.GetAirlockAnimation(TileAnimator.AnimationContextEnum.Close, _direction),
                    Animator.GetAirlockAnimation(TileAnimator.AnimationContextEnum.Wait, TileOrientation.None),
                    Animator.GetAirlockAnimation(TileAnimator.AnimationContextEnum.Open, GetReverseDirection(_direction)),
                    Animator.GetAirlockAnimation(TileAnimator.AnimationContextEnum.Close, GetReverseDirection(_direction)) };

                Animator.AnimateSequence(animationSequence);
                _yieldTime = Animator.GetProperWaitTimeForAnim(animationSequence[0]) + Animator.GetProperWaitTimeForAnim(animationSequence[1]) + (Animator.GetProperWaitTimeForAnim(animationSequence[2]) * 0.5f);
                break;
            default:
                throw new System.NotImplementedException(type + " hasn't been properly implemented yet!");
        }
    }

    public void SetBuildingAllowed(bool _b) {
        Tile _neighbour;
        bool _isAdjacentHorizontally = false;
        bool _isAdjacentVertically = false;
        if (_b) {
            int _gridX, _gridY;
            for (int y = -1; y <= 1; y++) {
                for (int x = -1; x <= 1; x++) {
                    if (x == 0 && y == 0)
                        continue;

                    _gridX = GridX + x;
                    _gridY = GridY + y;

                    if (_gridX >= 0 && _gridX < Grid.Instance.GridSizeX && _gridY >= 0 && _gridY < Grid.Instance.GridSizeY) {
                        _neighbour = Grid.Instance.grid[_gridX, _gridY];

                        _isAdjacentHorizontally = x != 0 && y == 0;
                        _isAdjacentVertically = x == 0 && y != 0;

                        // is there an adjacent door? (non-diagonally)
                        if ((_isAdjacentHorizontally || _isAdjacentVertically) &&  _neighbour._Type_ == TileType.Door) {
                            // fail
                            return;
                        }
                    }
                }
            }
        }

        _BuildingAllowed_ = _b;
    }
}
