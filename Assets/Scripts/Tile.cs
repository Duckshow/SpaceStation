using UnityEngine;
using System;

public class Tile : IHeapItem<Tile> {

    public const float RADIUS = 0.5f;
	public const float DIAMETER = 1;
	public const int RESOLUTION = 64;
    public const float PIXEL_RADIUS = 0.0078125f;

    public CachedAssets.WallSet.Purpose ExactType;
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
            case TileOrientation.None:
                return TileOrientation.None;
        }

        throw new System.Exception("Somehow couldn't find a reverse direction! Input was " + _direction.ToString() + "!");   
    }

    public Type TempType = Type.Empty;
    public TileOrientation TempOrientation = TileOrientation.None;
    //[NonSerialized] public bool HasBeenEvaluated = false;

    public bool Walkable { get; private set; }

    private bool buildingAllowed = true;
    public bool _BuildingAllowed_ { get { return buildingAllowed; } private set { buildingAllowed = value; } }
    // public int GridX { get; private set; }
    // public int GridY { get; private set; }
    public Vector2i GridCoord { get; private set; }

    public Vector2 WorldPosition { get; private set; }
    public Vector2 DefaultPositionWorld { get; private set; }
    public Vector2 CharacterPositionWorld { // the position a character should stand on (exists to better simulate zero-g)
        get {
			if (_WallType_ == Type.Empty) {
                Vector2 _offset = Vector2.zero;
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

    public bool CanConnectTemp_L { get; private set; }
    public bool CanConnectTemp_T { get; private set; }
    public bool CanConnectTemp_R { get; private set; }
    public bool CanConnectTemp_B { get; private set; }
    public bool CanConnectTempFloor_L { get; private set; }
    public bool CanConnectTempFloor_T { get; private set; }
    public bool CanConnectTempFloor_R { get; private set; }
    public bool CanConnectTempFloor_B { get; private set; }

    [NonSerialized] public bool HasConnectable_L = false; // TODO: could all these bools just be a few matrices??
    [NonSerialized] public bool HasConnectable_T = false;
    [NonSerialized] public bool HasConnectable_R = false;
    [NonSerialized] public bool HasConnectable_B = false;
	[NonSerialized] public bool HasConnectableFloor_L = false;
	[NonSerialized] public bool HasConnectableFloor_T = false;
	[NonSerialized] public bool HasConnectableFloor_R = false;
	[NonSerialized] public bool HasConnectableFloor_B = false;

    [NonSerialized] public bool HasConnectableTemp_L = false;
    [NonSerialized] public bool HasConnectableTemp_T = false;
    [NonSerialized] public bool HasConnectableTemp_R = false;
    [NonSerialized] public bool HasConnectableTemp_B = false;
	[NonSerialized] public bool HasConnectableTempFloor_L = false;
	[NonSerialized] public bool HasConnectableTempFloor_T = false;
	[NonSerialized] public bool HasConnectableTempFloor_R = false;
	[NonSerialized] public bool HasConnectableTempFloor_B = false;

    [NonSerialized] public bool HideFloorCorner_TL = false;
    [NonSerialized] public bool HideFloorCorner_TR = false;
    [NonSerialized] public bool HideFloorCorner_BR = false;
    [NonSerialized] public bool HideFloorCorner_BL = false;
    [NonSerialized] public bool HideWallCorner_Tl = false;
    [NonSerialized] public bool HideWallCorner_TR = false;
    [NonSerialized] public bool HideWallCorner_BR = false;
    [NonSerialized] public bool HideWallCorner_BL = false;

    [NonSerialized] public bool IsBlocked_L = false;
    [NonSerialized] public bool IsBlocked_T = false;
    [NonSerialized] public bool IsBlocked_R = false;
    [NonSerialized] public bool IsBlocked_B = false;

    [NonSerialized] public Tile ParentTile;
    [NonSerialized] public int GCost;
    [NonSerialized] public int HCost;
    public int _FCost_ { get { return GCost + HCost; } }

    private int heapIndex;
    public int HeapIndex {
        get { return heapIndex; }
        set { heapIndex = value; }
    }

	public UVController MyUVController;
    public TileAnimator Animator;

    public bool StopAheadAndBehindMeWhenCrossing { get; private set; }
    public int MovementPenalty { get; private set; }
    public bool ForceActorStopWhenPassingThis { get; private set; }

	public int ThingsUsingThis = 0;

    private TileObject occupyingTileObject = null;
    public TileObject GetOccupyingTileObject(){ 
        return occupyingTileObject; 
    }
    public void SetOccupyingTileObject(TileObject _newOccupant){
        if (occupyingTileObject != null && occupyingTileObject != _newOccupant)
            Debug.LogErrorFormat("{0}'s new tile ({1}) is occupied by {2}! This shouldn't happen!", _newOccupant.transform.name, GridCoord, occupyingTileObject.transform.name);
        occupyingTileObject = _newOccupant;
    }
    public void ClearOccupyingTileObject(TileObject _caller){
        if(occupyingTileObject != null && occupyingTileObject != _caller)
            Debug.LogErrorFormat("{0} tried to set Tile({1})'s occupyingTileObject to null, but isn't actually its current occupant!", _caller.transform.name, GridCoord);
        occupyingTileObject = null;
    }


    public Tile(Vector3 _worldPos, int _gridX, int _gridY) {
        WorldPosition = _worldPos;
        GridCoord = new Vector2i(_gridX, _gridY);

		MyUVController = ((GameObject)Grid.Instantiate(CachedAssets.Instance.TilePrefab, new Vector3(WorldPosition.x, WorldPosition.y + 0.5f, 0), Quaternion.identity)).GetComponent<UVController>();
		MyUVController.name = "TileQuad " + GridCoord.x + "x" + GridCoord.y + " (" + WorldPosition.x + ", " + WorldPosition.y + ")";
		MyUVController.Setup();
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
        sCachedNeighbour_T = _gridY < Grid.GridSizeY - 1 ? Grid.Instance.grid[_gridX, _gridY + 1] : null;
        return sCachedNeighbour_T != null;
    }
    public static Tile sCachedNeighbour_R;
    public static bool sTryTempCacheNeighbour_R(int _gridX, int _gridY) {
        sCachedNeighbour_R = _gridX < Grid.GridSizeX - 1 ? Grid.Instance.grid[_gridX + 1, _gridY] : null;
        return sCachedNeighbour_R != null;
    }
    public static Tile sCachedNeighbour_B;
    public static bool sTryTempCacheNeighbour_B(int _gridX, int _gridY) {
        sCachedNeighbour_B = _gridY > 0 ? Grid.Instance.grid[_gridX, _gridY - 1] : null;
        return sCachedNeighbour_B != null;
    }
    public static Tile sCachedNeighbour_TL;
    public static bool sTryTempCacheNeighbour_TL(int _gridX, int _gridY) {
        sCachedNeighbour_TL = _gridX > 0 && _gridY < Grid.GridSizeY - 1 ? Grid.Instance.grid[_gridX - 1, _gridY + 1] : null;
        return sCachedNeighbour_TL != null;
    }
    public static Tile sCachedNeighbour_TR;
    public static bool sTryTempCacheNeighbour_TR(int _gridX, int _gridY) {
        sCachedNeighbour_TR = _gridX < Grid.GridSizeX - 1 && _gridY < Grid.GridSizeY - 1 ? Grid.Instance.grid[_gridX + 1, _gridY + 1] : null;
        return sCachedNeighbour_TR != null;
    }
    public static Tile sCachedNeighbour_BR;
    public static bool sTryTempCacheNeighbour_BR(int _gridX, int _gridY) {
        sCachedNeighbour_BR = _gridX < Grid.GridSizeX - 1 && _gridY > 0 ? Grid.Instance.grid[_gridX + 1, _gridY - 1] : null;
        return sCachedNeighbour_BR != null;
    }
    public static Tile sCachedNeighbour_BL;
    public static bool sTryTempCacheNeighbour_BL(int _gridX, int _gridY) {
        sCachedNeighbour_BL = _gridX > 0 && _gridY > 0 ? Grid.Instance.grid[_gridX - 1, _gridY - 1] : null;
        return sCachedNeighbour_BL != null;
    }

    private static bool[] sConnectability = new bool[4];
    private static bool[] sGetConnectability(Type _type, TileOrientation _orientation = TileOrientation.None) {
        sConnectability[0] = false; //L
        sConnectability[1] = false; //T
        sConnectability[2] = false; //R
        sConnectability[3] = false; //B
        switch (_type) {
            case Type.Empty:
                break;
            case Type.Solid:
                sConnectability[0] = true;
                sConnectability[1] = true;
                sConnectability[2] = true;
                sConnectability[3] = true;
                break;
            case Type.Diagonal:
                switch (_orientation) {
                    case TileOrientation.BottomLeft:
                        sConnectability[0] = true;
                        sConnectability[3] = true;
                        break;
                    case TileOrientation.TopLeft:
                        sConnectability[0] = true;
                        sConnectability[1] = true;
                        break;
                    case TileOrientation.TopRight:
                        sConnectability[1] = true;
                        sConnectability[2] = true;
                        break;
                    case TileOrientation.BottomRight:
                        sConnectability[2] = true;
                        sConnectability[3] = true;
                        break;
                }
                break;
            case Type.Door:
                switch (_orientation) {
                    // vertical
                    case TileOrientation.Bottom:
                    case TileOrientation.Top:
                        sConnectability[1] = true;
                        sConnectability[3] = true;
                        break;
                    // horizontal
                    case TileOrientation.Left:
                    case TileOrientation.Right:
                        sConnectability[0] = true;
                        sConnectability[2] = true;
                        break;
                }
                break;
            case Type.Airlock:
                switch (_orientation) {
                    // vertical
                    case TileOrientation.Bottom:
                    case TileOrientation.Top:
                        sConnectability[1] = true;
                        sConnectability[3] = true;
                        break;
                    // horizontal
                    case TileOrientation.Left:
                    case TileOrientation.Right:
                        sConnectability[0] = true;
                        sConnectability[2] = true;
                        break;
                }
                break;
            default:
                throw new System.Exception(_type.ToString() + " has not been properly implemented yet!");
        }
        return sConnectability;
    }

    public void SetTileType(Type _newType, TileOrientation _newOrientation, bool _temporarily = false) {
		if (_temporarily) {
            TempType = _newType;
            TempOrientation = _newOrientation;

            if (TempType == _WallType_ && TempOrientation == _Orientation_) {
                CanConnectTemp_L = false;
                CanConnectTemp_T = false;
                CanConnectTemp_R = false;
                CanConnectTemp_B = false;
            }
            else {
                sConnectability = sGetConnectability(_newType, _newOrientation);
                CanConnectTemp_L = sConnectability[0];
                CanConnectTemp_T = sConnectability[1];
                CanConnectTemp_R = sConnectability[2];
                CanConnectTemp_B = sConnectability[3];
            }

            if (_newType == Type.Empty) {
                HasConnectableTemp_L = false;
                HasConnectableTemp_T = false;
                HasConnectableTemp_R = false;
                HasConnectableTemp_B = false;
            }

            if (sTryTempCacheNeighbour_L(GridCoord.x, GridCoord.y) && sCachedNeighbour_L.TempType != Type.Empty)
                UpdateNeighbourWall(sCachedNeighbour_L, TileOrientation.Left, true);
            if (sTryTempCacheNeighbour_T(GridCoord.x, GridCoord.y) && sCachedNeighbour_T.TempType != Type.Empty)
                UpdateNeighbourWall(sCachedNeighbour_T, TileOrientation.Top, true);
            if (sTryTempCacheNeighbour_R(GridCoord.x, GridCoord.y) && sCachedNeighbour_R.TempType != Type.Empty)
                UpdateNeighbourWall(sCachedNeighbour_R, TileOrientation.Right, true);
            if (sTryTempCacheNeighbour_B(GridCoord.x, GridCoord.y) && sCachedNeighbour_B.TempType != Type.Empty)
                UpdateNeighbourWall(sCachedNeighbour_B, TileOrientation.Bottom, true);

            if (sTryTempCacheNeighbour_TL(GridCoord.x, GridCoord.y) && sCachedNeighbour_TL.TempType != Type.Empty)
                UpdateNeighbourWall(sCachedNeighbour_TL, TileOrientation.TopLeft, true);
            if (sTryTempCacheNeighbour_TR(GridCoord.x, GridCoord.y) && sCachedNeighbour_TR.TempType != Type.Empty)
                UpdateNeighbourWall(sCachedNeighbour_TR, TileOrientation.TopRight, true);
            if (sTryTempCacheNeighbour_BR(GridCoord.x, GridCoord.y) && sCachedNeighbour_BR.TempType != Type.Empty)
                UpdateNeighbourWall(sCachedNeighbour_BR, TileOrientation.BottomRight, true);
            if (sTryTempCacheNeighbour_BL(GridCoord.x, GridCoord.y) && sCachedNeighbour_BL.TempType != Type.Empty)
                UpdateNeighbourWall(sCachedNeighbour_BL, TileOrientation.BottomLeft, true);
            return;
        }

        Animator.StopAnimating();

		prevType = _WallType_;
		_WallType_ = _newType;
		prevOrientation = _Orientation_;
		_Orientation_ = _newOrientation;

		MyUVController.GridLayers[(int)MeshSorter.GridLayerEnum.Bottom].Orientation = _newOrientation;
		MyUVController.GridLayers[(int)MeshSorter.GridLayerEnum.Top].Orientation = _newOrientation;
		MyUVController.SortingLayer = MeshSorter.SortingLayerEnum.Grid;
		MyUVController.Sort(GridCoord.y); // TODO: this probably only needs to happen once, or?

        ForceActorStopWhenPassingThis = false;
        MovementPenalty = 0; //TODO: use this for something!

        if (prevType == Type.Diagonal) {
			if ((prevOrientation == TileOrientation.BottomLeft && !CanConnectFloor_T && !CanConnectFloor_R) ||
			   (prevOrientation == TileOrientation.TopLeft && !CanConnectFloor_B && !CanConnectFloor_R) ||
			   (prevOrientation == TileOrientation.TopRight && !CanConnectFloor_B && !CanConnectFloor_L) ||
			   (prevOrientation == TileOrientation.BottomRight && !CanConnectFloor_T && !CanConnectFloor_L))
				SetFloorType (Type.Empty, _newOrientation);
        }

        sConnectability = sGetConnectability(_newType, _newOrientation);
        CanConnect_L = sConnectability[0];
        CanConnect_T = sConnectability[1];
        CanConnect_R = sConnectability[2];
        CanConnect_B = sConnectability[3];

        switch (_newType) {
            case Type.Empty:
                Walkable = true;
                DefaultPositionWorld = WorldPosition;
                break;
            case Type.Solid:
                Walkable = false;
				SetFloorType (Type.Empty, _newOrientation);
                break;
			case Type.Diagonal:
				Walkable = true;
                switch (_newOrientation) {
                    case TileOrientation.BottomLeft:
                        DefaultPositionWorld = WorldPosition + new Vector2(0.25f, 0.25f);
                        break;
                    case TileOrientation.TopLeft:
                        DefaultPositionWorld = WorldPosition + new Vector2(0.25f, -0.25f);
                        break;
                    case TileOrientation.TopRight:
                        DefaultPositionWorld = WorldPosition + new Vector2(-0.25f, -0.25f);
                        break;
                    case TileOrientation.BottomRight:
                        DefaultPositionWorld = WorldPosition + new Vector2(-0.25f, 0.25f);
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
                        DefaultPositionWorld = WorldPosition + new Vector2(0, -0.15f);
                        break;
                    // horizontal
                    case TileOrientation.Left:
                    case TileOrientation.Right:
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
                        DefaultPositionWorld = WorldPosition + new Vector2(0, -0.25f);
                        break;
                    // horizontal
                    case TileOrientation.Left:
                    case TileOrientation.Right:
                        DefaultPositionWorld = WorldPosition + new Vector2(0, -0.35f);
                        break;
                }

				SetFloorType (Type.Empty, _newOrientation);
                break;
            default:
                throw new System.Exception(_newType.ToString() + " has not been properly implemented yet!");
        }

		if (sTryTempCacheNeighbour_L(GridCoord.x, GridCoord.y))
			UpdateNeighbourWall(sCachedNeighbour_L, TileOrientation.Left, false);
		if (sTryTempCacheNeighbour_T(GridCoord.x, GridCoord.y))
			UpdateNeighbourWall(sCachedNeighbour_T, TileOrientation.Top, false);
		if (sTryTempCacheNeighbour_R(GridCoord.x, GridCoord.y))
			UpdateNeighbourWall(sCachedNeighbour_R, TileOrientation.Right, false);
		if (sTryTempCacheNeighbour_B(GridCoord.x, GridCoord.y))
			UpdateNeighbourWall(sCachedNeighbour_B, TileOrientation.Bottom, false);

        if (sTryTempCacheNeighbour_TL(GridCoord.x, GridCoord.y))
            UpdateNeighbourWall(sCachedNeighbour_TL, TileOrientation.TopLeft, false);
        if (sTryTempCacheNeighbour_TR(GridCoord.x, GridCoord.y))
            UpdateNeighbourWall(sCachedNeighbour_TR, TileOrientation.TopRight, false);
        if (sTryTempCacheNeighbour_BR(GridCoord.x, GridCoord.y))
            UpdateNeighbourWall(sCachedNeighbour_BR, TileOrientation.BottomRight, false);
        if (sTryTempCacheNeighbour_BL(GridCoord.x, GridCoord.y))
            UpdateNeighbourWall(sCachedNeighbour_BL, TileOrientation.BottomLeft, false);

        ExactType = CachedAssets.Instance.GetTileDefinition(this);
        ChangeWallGraphics (
			CachedAssets.Instance.GetWallAssetForTile (_WallType_, _Orientation_, 0, true, HasConnectable_L, HasConnectable_T, HasConnectable_R, HasConnectable_B),
			CachedAssets.Instance.GetWallAssetForTile (_WallType_, _Orientation_, 0, false, HasConnectable_L, HasConnectable_T, HasConnectable_R, HasConnectable_B),
            false
        );

		LightManager.ScheduleUpdateLights(GridCoord);
    }
    public void SetFloorType(Type _newType, TileOrientation _newOrientation, bool _temporarily = false){
        if (_temporarily) {
            TempType = _newType;
            TempOrientation = _newOrientation;

            if (TempType == _FloorType_ && TempOrientation == _FloorOrientation_) {
                CanConnectTempFloor_L = false;
                CanConnectTempFloor_T = false;
                CanConnectTempFloor_R = false;
                CanConnectTempFloor_B = false;
            }
            else {
                sConnectability = sGetConnectability(_newType, _newOrientation);
                CanConnectTempFloor_L = sConnectability[0];
                CanConnectTempFloor_T = sConnectability[1];
                CanConnectTempFloor_R = sConnectability[2];
                CanConnectTempFloor_B = sConnectability[3];
            }

            if (_newType == Type.Empty) {
                HasConnectableTempFloor_L = false;
                HasConnectableTempFloor_T = false;
                HasConnectableTempFloor_R = false;
                HasConnectableTempFloor_B = false;
            }

            if (sTryTempCacheNeighbour_L(GridCoord.x, GridCoord.y) && sCachedNeighbour_L.TempType != Type.Empty)
                UpdateNeighbourFloor(sCachedNeighbour_L, TileOrientation.Left, true);
            if (sTryTempCacheNeighbour_T(GridCoord.x, GridCoord.y) && sCachedNeighbour_T.TempType != Type.Empty)
                UpdateNeighbourFloor(sCachedNeighbour_T, TileOrientation.Top, true);
            if (sTryTempCacheNeighbour_R(GridCoord.x, GridCoord.y) && sCachedNeighbour_R.TempType != Type.Empty)
                UpdateNeighbourFloor(sCachedNeighbour_R, TileOrientation.Right, true);
            if (sTryTempCacheNeighbour_B(GridCoord.x, GridCoord.y) && sCachedNeighbour_B.TempType != Type.Empty)
                UpdateNeighbourFloor(sCachedNeighbour_B, TileOrientation.Bottom, true);

            if (sTryTempCacheNeighbour_TL(GridCoord.x, GridCoord.y) && sCachedNeighbour_TL.TempType != Type.Empty)
                UpdateNeighbourFloor(sCachedNeighbour_TL, TileOrientation.TopLeft, true);
            if (sTryTempCacheNeighbour_TR(GridCoord.x, GridCoord.y) && sCachedNeighbour_TR.TempType != Type.Empty)
                UpdateNeighbourFloor(sCachedNeighbour_TR, TileOrientation.TopRight, true);
            if (sTryTempCacheNeighbour_BR(GridCoord.x, GridCoord.y) && sCachedNeighbour_BR.TempType != Type.Empty)
                UpdateNeighbourFloor(sCachedNeighbour_BR, TileOrientation.BottomRight, true);
            if (sTryTempCacheNeighbour_BL(GridCoord.x, GridCoord.y) && sCachedNeighbour_BL.TempType != Type.Empty)
                UpdateNeighbourFloor(sCachedNeighbour_BL, TileOrientation.BottomLeft, true);
            return;
        }

		_FloorType_ = _newType;
		_FloorOrientation_ = _newOrientation;

		MyUVController.GridLayers[(int)MeshSorter.GridLayerEnum.Floor].Orientation = _newOrientation;
		MyUVController.SortingLayer = MeshSorter.SortingLayerEnum.Grid;
		MyUVController.Sort(GridCoord.y); // TODO: this probably only needs to happen once, or?

        //ForceActorStopWhenPassingThis = false; // if floor actually needs this, it has to be its own - otherwise breaks airlocks and such!
		MovementPenalty = 0; //TODO: use this for something!

        sConnectability = sGetConnectability(_newType, _newOrientation);
		CanConnectFloor_L = sConnectability[0];
		CanConnectFloor_T = sConnectability[1];
		CanConnectFloor_R = sConnectability[2];
		CanConnectFloor_B = sConnectability[3];

		if (sTryTempCacheNeighbour_L(GridCoord.x, GridCoord.y))
			UpdateNeighbourFloor(sCachedNeighbour_L, TileOrientation.Left, false);
		if (sTryTempCacheNeighbour_T(GridCoord.x, GridCoord.y))
			UpdateNeighbourFloor(sCachedNeighbour_T, TileOrientation.Top, false);
		if (sTryTempCacheNeighbour_R(GridCoord.x, GridCoord.y))
			UpdateNeighbourFloor(sCachedNeighbour_R, TileOrientation.Right, false);
		if (sTryTempCacheNeighbour_B(GridCoord.x, GridCoord.y))
			UpdateNeighbourFloor(sCachedNeighbour_B, TileOrientation.Bottom, false);

        if (sTryTempCacheNeighbour_TL(GridCoord.x, GridCoord.y))
            UpdateNeighbourFloor(sCachedNeighbour_TL, TileOrientation.TopLeft, false);
        if (sTryTempCacheNeighbour_TR(GridCoord.x, GridCoord.y))
            UpdateNeighbourFloor(sCachedNeighbour_TR, TileOrientation.TopRight, false);
        if (sTryTempCacheNeighbour_BR(GridCoord.x, GridCoord.y))
            UpdateNeighbourFloor(sCachedNeighbour_BR, TileOrientation.BottomRight, false);
        if (sTryTempCacheNeighbour_BL(GridCoord.x, GridCoord.y))
            UpdateNeighbourFloor(sCachedNeighbour_BL, TileOrientation.BottomLeft, false);

        ExactType = CachedAssets.Instance.GetTileDefinition(this);
        ChangeFloorGraphics(
            CachedAssets.Instance.GetFloorAssetForTile(_FloorType_, _FloorOrientation_, 0, HasConnectableFloor_L, HasConnectableFloor_T, HasConnectableFloor_R, HasConnectableFloor_B),
            false
        );

		LightManager.ScheduleUpdateLights(GridCoord);
	}

	// WARNING: this doesn't support changing the type and orientation of the tile, so if you're gonna change the type of a tile
	// you're gonna want to update its neighbours, but with something more fleshed out than this!
    void UpdateNeighbourWall(Tile _neighbour, TileOrientation _directionFromThisTile, bool _temporarily) {
        if (_temporarily) {
            switch (_directionFromThisTile) {
                case TileOrientation.Bottom:
                    _neighbour.HasConnectableTemp_T = CanConnectTemp_B;
                    break;
                case TileOrientation.Left:
                    _neighbour.HasConnectableTemp_R = CanConnectTemp_L;
                    break;
                case TileOrientation.Top:
                    _neighbour.HasConnectableTemp_B = CanConnectTemp_T;
                    break;
                case TileOrientation.Right:
                    _neighbour.HasConnectableTemp_L = CanConnectTemp_R;
                    break;
                case TileOrientation.TopLeft:
                case TileOrientation.TopRight:
                case TileOrientation.BottomRight:
                case TileOrientation.BottomLeft:
                    _neighbour.UpdateWallCornerHider(true);
                    return;
            }

            if (_neighbour.TempType == _neighbour.wallType)
                return;

            _neighbour.ChangeWallGraphics(
                CachedAssets.Instance.GetWallAssetForTile(_neighbour.TempType, _neighbour.TempOrientation, 0, true, _neighbour.HasConnectableTemp_L, _neighbour.HasConnectableTemp_T, _neighbour.HasConnectableTemp_R, _neighbour.HasConnectableTemp_B),
                CachedAssets.Instance.GetWallAssetForTile(_neighbour.TempType, _neighbour.TempOrientation, 0, false, _neighbour.HasConnectableTemp_L, _neighbour.HasConnectableTemp_T, _neighbour.HasConnectableTemp_R, _neighbour.HasConnectableTemp_B),
                _temporary: true
            );
            return;
        }
        switch (_directionFromThisTile) {
			case TileOrientation.Bottom:
				_neighbour.HasConnectable_T = CanConnect_B;
				_neighbour.IsBlocked_T = Grid.OtherTileIsBlockingPath(_WallType_, _Orientation_, TileOrientation.Top);
				//_neighbour.ConnectedDiagonal_T = (_WallType_ == Type.Diagonal && (_Orientation_ == TileOrientation.BottomLeft || _Orientation_ == TileOrientation.BottomRight)) ? this : null;
				//_neighbour.ConnectedDoorOrAirlock_T = (_WallType_ == Type.Door || _WallType_ == Type.Airlock) ? this : null;

                // prevent building in front of door
				//if ((_WallType_ == Type.Door || _WallType_ == Type.Airlock) && _IsHorizontal_)
    //                _neighbour.SetBuildingAllowed(false);
                break;
            case TileOrientation.Left:
                _neighbour.HasConnectable_R = CanConnect_L;
				_neighbour.IsBlocked_L = Grid.OtherTileIsBlockingPath(_WallType_, _Orientation_, TileOrientation.Left);
				//_neighbour.ConnectedDiagonal_R = (_WallType_ == Type.Diagonal && (_Orientation_ == TileOrientation.TopLeft || _Orientation_ == TileOrientation.BottomLeft)) ? this : null;
				//_neighbour.ConnectedDoorOrAirlock_R = (_WallType_ == Type.Door || _WallType_ == Type.Airlock) ? this : null;

                // prevent building in front of door
				//if ((_WallType_ == Type.Door || _WallType_ == Type.Airlock) && _IsVertical_)
    //                _neighbour.SetBuildingAllowed(false);
                break;
            case TileOrientation.Top:
                _neighbour.HasConnectable_B = CanConnect_T;
				_neighbour.IsBlocked_B = Grid.OtherTileIsBlockingPath(_WallType_, _Orientation_, TileOrientation.Bottom);
				//_neighbour.ConnectedDiagonal_B = (_WallType_ == Type.Diagonal && (_Orientation_ == TileOrientation.TopLeft || _Orientation_ == TileOrientation.TopRight)) ? this : null;
				//_neighbour.ConnectedDoorOrAirlock_B = (_WallType_ == Type.Door || _WallType_ == Type.Airlock) ? this : null;

                // prevent building in front of door
                //if ((_WallType_ == Type.Door || _WallType_ == Type.Airlock) && _IsHorizontal_)
                //    _neighbour.SetBuildingAllowed(false);

                //if (_WallType_ == Type.Door || _WallType_ == Type.Airlock) {
                //                // prevent building in front of door
                //	if (_IsHorizontal_)
                //                    _neighbour.SetBuildingAllowed(false);

                //                // sort connected neighbour of door on top, so as to hide actors moving through it
                //	else if (_IsVertical_) {
                //                    _neighbour.BottomQuad.SortCustom(TopQuad.GetSortOrder() - 2);
                //                    _neighbour.TopQuad.SortCustom(TopQuad.GetSortOrder() - 1);
                //                }
                //            }
                //else if (_PrevType_ == Type.Door || _PrevType_ == Type.Airlock) {
                //                // reset to ordinary sorting
                //	if (_IsVertical_) {
                //                    _neighbour.BottomQuad.RemoveCustomSort();
                //                    _neighbour.TopQuad.RemoveCustomSort();
                //                }
                //            }
                break;
            case TileOrientation.Right:
                _neighbour.HasConnectable_L = CanConnect_R;
				_neighbour.IsBlocked_L = Grid.OtherTileIsBlockingPath(_WallType_, _Orientation_, TileOrientation.Left);
				//_neighbour.ConnectedDiagonal_L = (_WallType_ == Type.Diagonal && (_Orientation_ == TileOrientation.BottomRight || _Orientation_ == TileOrientation.TopRight)) ? this : null;
				//_neighbour.ConnectedDoorOrAirlock_L = (_WallType_ == Type.Door || _WallType_ == Type.Airlock) ? this : null;

                // prevent building in front of door
				//if ((_WallType_ == Type.Door || _WallType_ == Type.Airlock) && _IsVertical_)
    //               _neighbour.SetBuildingAllowed(false);
                break;
			case TileOrientation.TopLeft:
			case TileOrientation.TopRight:
			case TileOrientation.BottomRight:
			case TileOrientation.BottomLeft:
                _neighbour.UpdateWallCornerHider(false);
                return;
        }

        _neighbour.ExactType = CachedAssets.Instance.GetTileDefinition(_neighbour);
		_neighbour.ChangeWallGraphics (
			CachedAssets.Instance.GetWallAssetForTile (_neighbour._WallType_, _neighbour._Orientation_, 0, true, _neighbour.HasConnectable_L, _neighbour.HasConnectable_T, _neighbour.HasConnectable_R, _neighbour.HasConnectable_B),
			CachedAssets.Instance.GetWallAssetForTile (_neighbour._WallType_, _neighbour._Orientation_, 0, false, _neighbour.HasConnectable_L, _neighbour.HasConnectable_T, _neighbour.HasConnectable_R, _neighbour.HasConnectable_B),
            false
        );
    }
	// WARNING: this doesn't support changing the type and orientation of the tile, so if you're gonna change the type of a tile
	// you're gonna want to update its neighbours, but with something more fleshed out than this!
	void UpdateNeighbourFloor(Tile _neighbour, TileOrientation _directionFromThisTile, bool _temporarily) {
        if (_temporarily) {
            if (_neighbour.TempType == Type.Empty)
                return;

            switch (_directionFromThisTile) {
                case TileOrientation.Bottom:
                    _neighbour.HasConnectableTempFloor_T = CanConnectTempFloor_B;
                    break;
                case TileOrientation.Left:
                    _neighbour.HasConnectableTempFloor_R = CanConnectTempFloor_L;
                    break;
                case TileOrientation.Top:
                    _neighbour.HasConnectableTempFloor_B = CanConnectTempFloor_T;
                    break;
                case TileOrientation.Right:
                    _neighbour.HasConnectableTempFloor_L = CanConnectTempFloor_R;
                    break;
                case TileOrientation.TopLeft:
                case TileOrientation.TopRight:
                case TileOrientation.BottomRight:
                case TileOrientation.BottomLeft:
                    _neighbour.UpdateFloorCornerHider(true);
                    return;
            }

            if (_neighbour.TempType == _neighbour._FloorType_)
                return;

            _neighbour.ChangeFloorGraphics(
                CachedAssets.Instance.GetFloorAssetForTile(_neighbour.TempType, _neighbour.TempOrientation, 0, _neighbour.HasConnectableTempFloor_L, _neighbour.HasConnectableTempFloor_T, _neighbour.HasConnectableTempFloor_R, _neighbour.HasConnectableTempFloor_B), 
                true
            );
            return;
        }
        switch (_directionFromThisTile) {
			case TileOrientation.Bottom:
                _neighbour.HasConnectableFloor_T = CanConnectFloor_B;
				//_neighbour.ConnectedDiagonalFloor_T = (_FloorType_ == Type.Diagonal && (_FloorOrientation_ == TileOrientation.BottomLeft || _FloorOrientation_ == TileOrientation.BottomRight)) ? this : null;
                break;
			case TileOrientation.Left:
                _neighbour.HasConnectableFloor_R = CanConnectFloor_L;
				//_neighbour.ConnectedDiagonalFloor_R = (_FloorType_ == Type.Diagonal && (_FloorOrientation_ == TileOrientation.TopLeft || _FloorOrientation_ == TileOrientation.BottomLeft)) ? this : null;
				break;
			case TileOrientation.Top:
                _neighbour.HasConnectableFloor_B = CanConnectFloor_T;
				//_neighbour.ConnectedDiagonalFloor_B = (_FloorType_ == Type.Diagonal && (_FloorOrientation_ == TileOrientation.TopLeft || _FloorOrientation_ == TileOrientation.TopRight)) ? this : null;
				break;
			case TileOrientation.Right:
                _neighbour.HasConnectableFloor_L = CanConnectFloor_R;
				//_neighbour.ConnectedDiagonalFloor_L = (_FloorType_ == Type.Diagonal && (_FloorOrientation_ == TileOrientation.BottomRight || _FloorOrientation_ == TileOrientation.TopRight)) ? this : null;
				break;
            case TileOrientation.TopLeft:
            case TileOrientation.TopRight:
            case TileOrientation.BottomRight:
            case TileOrientation.BottomLeft:
                _neighbour.UpdateFloorCornerHider(false);
                return;
		}

        _neighbour.ExactType = CachedAssets.Instance.GetTileDefinition(_neighbour);
		_neighbour.ChangeFloorGraphics (
            CachedAssets.Instance.GetFloorAssetForTile(_neighbour._FloorType_, _neighbour._FloorOrientation_, 0, _neighbour.HasConnectableFloor_L, _neighbour.HasConnectableFloor_T, _neighbour.HasConnectableFloor_R, _neighbour.HasConnectableFloor_B),
            false
        );
    }

    //private bool IsInAnyWayConnected(TileOrientation _direction) {
    //    //switch (_direction) {
    //    //    case TileOrientation.Bottom:
    //    //        return (CanConnect_B && HasConnectable_B) || (CanConnectTemp_B && HasConnectableTemp_B);
    //    //    case TileOrientation.Left:
    //    //        return (CanConnect_L && HasConnectable_L) || (CanConnectTemp_L && HasConnectableTemp_L);
    //    //    case TileOrientation.Top:
    //    //        return (CanConnect_T && HasConnectable_T) || (CanConnectTemp_T && HasConnectableTemp_T);
    //    //    case TileOrientation.Right:
    //    //        return (CanConnect_R && HasConnectable_R) || (CanConnectTemp_R && HasConnectableTemp_R);
    //    //}
    //    switch (_direction) {
    //        case TileOrientation.Bottom:
    //            return (CanConnect_B || CanConnectTemp_B) && (HasConnectable_B || HasConnectableTemp_B);
    //        case TileOrientation.Left:
    //            return (CanConnect_L || CanConnectTemp_L) && (HasConnectable_L || HasConnectableTemp_L);
    //        case TileOrientation.Top:
    //            return (CanConnect_T || CanConnectTemp_T) && (HasConnectable_T || HasConnectableTemp_T);
    //        case TileOrientation.Right:
    //            return (CanConnect_R || CanConnectTemp_R) && (HasConnectable_R || HasConnectableTemp_R);
    //    }
    //    return false;
    //}
    //private bool IsInAnyWayConnectedFloor(TileOrientation _direction) {
    //    //switch (_direction) {
    //    //    case TileOrientation.Bottom:
    //    //        return (CanConnectFloor_B && HasConnectableFloor_B) || (CanConnectTempFloor_B && HasConnectableTempFloor_B);
    //    //    case TileOrientation.Left:
    //    //        return (CanConnectFloor_L && HasConnectableFloor_L) || (CanConnectTempFloor_L && HasConnectableTempFloor_L);
    //    //    case TileOrientation.Top:
    //    //        return (CanConnectFloor_T && HasConnectableFloor_T) || (CanConnectTempFloor_T && HasConnectableTempFloor_T);
    //    //    case TileOrientation.Right:
    //    //        return (CanConnectFloor_R && HasConnectableFloor_R) || (CanConnectTempFloor_R && HasConnectableTempFloor_R);
    //    //}
    //    switch (_direction) {
    //        case TileOrientation.Bottom:
    //            return (CanConnectFloor_B || CanConnectTempFloor_B) && (HasConnectableFloor_B || HasConnectableTempFloor_B);
    //        case TileOrientation.Left:
    //            return (CanConnectFloor_L || CanConnectTempFloor_L) && (HasConnectableFloor_L || HasConnectableTempFloor_L);
    //        case TileOrientation.Top:
    //            return (CanConnectFloor_T || CanConnectTempFloor_T) && (HasConnectableFloor_T || HasConnectableTempFloor_T);
    //        case TileOrientation.Right:
    //            return (CanConnectFloor_R || CanConnectTempFloor_R) && (HasConnectableFloor_R || HasConnectableTempFloor_R);
    //    }
    //    return false;
    //}
    public void UpdateWallCornerHider(bool _temporarily) {
        if (_temporarily && _WallType_ == TempType && TempType != Type.Empty)
            return;

        sTryTempCacheNeighbour_L(GridCoord.x, GridCoord.y);
        sTryTempCacheNeighbour_T(GridCoord.x, GridCoord.y);
        sTryTempCacheNeighbour_R(GridCoord.x, GridCoord.y);
        sTryTempCacheNeighbour_B(GridCoord.x, GridCoord.y);

        if (_temporarily){
			MyUVController.ChangeAsset(MeshSorter.GridLayerEnum.TopCorners, CachedAssets.Instance.GetWallCornerAsset(
				CanConnectTemp_T && HasConnectableTemp_T && sCachedNeighbour_T != null && sCachedNeighbour_T.CanConnectTemp_L && sCachedNeighbour_T.HasConnectableTemp_L && CanConnectTemp_L && HasConnectableTemp_L && sCachedNeighbour_L != null && sCachedNeighbour_L.CanConnectTemp_T && sCachedNeighbour_L.HasConnectableTemp_T,
				CanConnectTemp_T && HasConnectableTemp_T && sCachedNeighbour_T != null && sCachedNeighbour_T.CanConnectTemp_R && sCachedNeighbour_T.HasConnectableTemp_R && CanConnectTemp_R && HasConnectableTemp_R && sCachedNeighbour_R != null && sCachedNeighbour_R.CanConnectTemp_T && sCachedNeighbour_R.HasConnectableTemp_T,
				CanConnectTemp_B && HasConnectableTemp_B && sCachedNeighbour_B != null && sCachedNeighbour_B.CanConnectTemp_R && sCachedNeighbour_B.HasConnectableTemp_R && CanConnectTemp_R && HasConnectableTemp_R && sCachedNeighbour_R != null && sCachedNeighbour_R.CanConnectTemp_B && sCachedNeighbour_R.HasConnectableTemp_B,
				CanConnectTemp_B && HasConnectableTemp_B && sCachedNeighbour_B != null && sCachedNeighbour_B.CanConnectTemp_L && sCachedNeighbour_B.HasConnectableTemp_L && CanConnectTemp_L && HasConnectableTemp_L && sCachedNeighbour_L != null && sCachedNeighbour_L.CanConnectTemp_B && sCachedNeighbour_L.HasConnectableTemp_B),
				true
			);
		}
		else{
			MyUVController.ChangeAsset(MeshSorter.GridLayerEnum.TopCorners, CachedAssets.Instance.GetWallCornerAsset(
				CanConnect_T && HasConnectable_T && sCachedNeighbour_T != null && sCachedNeighbour_T.CanConnect_L && sCachedNeighbour_T.HasConnectable_L && CanConnect_L && HasConnectable_L && sCachedNeighbour_L != null && sCachedNeighbour_L.CanConnect_T && sCachedNeighbour_L.HasConnectable_T,
				CanConnect_T && HasConnectable_T && sCachedNeighbour_T != null && sCachedNeighbour_T.CanConnect_R && sCachedNeighbour_T.HasConnectable_R && CanConnect_R && HasConnectable_R && sCachedNeighbour_R != null && sCachedNeighbour_R.CanConnect_T && sCachedNeighbour_R.HasConnectable_T,
				CanConnect_B && HasConnectable_B && sCachedNeighbour_B != null && sCachedNeighbour_B.CanConnect_R && sCachedNeighbour_B.HasConnectable_R && CanConnect_R && HasConnectable_R && sCachedNeighbour_R != null && sCachedNeighbour_R.CanConnect_B && sCachedNeighbour_R.HasConnectable_B,
				CanConnect_B && HasConnectable_B && sCachedNeighbour_B != null && sCachedNeighbour_B.CanConnect_L && sCachedNeighbour_B.HasConnectable_L && CanConnect_L && HasConnectable_L && sCachedNeighbour_L != null && sCachedNeighbour_L.CanConnect_B && sCachedNeighbour_L.HasConnectable_B),
				false
			);
		}
		// if (_temporarily) {
        //     //WallCornerHider.SetDebugBools(CanConnectTemp_L, HasConnectableTemp_L, CanConnectTemp_T, HasConnectableTemp_T, CanConnectTemp_R, HasConnectableTemp_R, CanConnectTemp_B, HasConnectableTemp_B);
        //     WallCornerHider.ChangeAsset(CachedAssets.Instance.GetWallCornerAsset(
        //         CanConnectTemp_T && HasConnectableTemp_T && sCachedNeighbour_T != null && sCachedNeighbour_T.CanConnectTemp_L && sCachedNeighbour_T.HasConnectableTemp_L && CanConnectTemp_L && HasConnectableTemp_L && sCachedNeighbour_L != null && sCachedNeighbour_L.CanConnectTemp_T && sCachedNeighbour_L.HasConnectableTemp_T,
        //         CanConnectTemp_T && HasConnectableTemp_T && sCachedNeighbour_T != null && sCachedNeighbour_T.CanConnectTemp_R && sCachedNeighbour_T.HasConnectableTemp_R && CanConnectTemp_R && HasConnectableTemp_R && sCachedNeighbour_R != null && sCachedNeighbour_R.CanConnectTemp_T && sCachedNeighbour_R.HasConnectableTemp_T,
        //         CanConnectTemp_B && HasConnectableTemp_B && sCachedNeighbour_B != null && sCachedNeighbour_B.CanConnectTemp_R && sCachedNeighbour_B.HasConnectableTemp_R && CanConnectTemp_R && HasConnectableTemp_R && sCachedNeighbour_R != null && sCachedNeighbour_R.CanConnectTemp_B && sCachedNeighbour_R.HasConnectableTemp_B,
        //         CanConnectTemp_B && HasConnectableTemp_B && sCachedNeighbour_B != null && sCachedNeighbour_B.CanConnectTemp_L && sCachedNeighbour_B.HasConnectableTemp_L && CanConnectTemp_L && HasConnectableTemp_L && sCachedNeighbour_L != null && sCachedNeighbour_L.CanConnectTemp_B && sCachedNeighbour_L.HasConnectableTemp_B),
        //         true
        //     );
        // }
        // else {
        //     WallCornerHider.ChangeAsset(CachedAssets.Instance.GetWallCornerAsset(
        //         CanConnect_T && HasConnectable_T && sCachedNeighbour_T != null && sCachedNeighbour_T.CanConnect_L && sCachedNeighbour_T.HasConnectable_L && CanConnect_L && HasConnectable_L && sCachedNeighbour_L != null && sCachedNeighbour_L.CanConnect_T && sCachedNeighbour_L.HasConnectable_T,
        //         CanConnect_T && HasConnectable_T && sCachedNeighbour_T != null && sCachedNeighbour_T.CanConnect_R && sCachedNeighbour_T.HasConnectable_R && CanConnect_R && HasConnectable_R && sCachedNeighbour_R != null && sCachedNeighbour_R.CanConnect_T && sCachedNeighbour_R.HasConnectable_T,
        //         CanConnect_B && HasConnectable_B && sCachedNeighbour_B != null && sCachedNeighbour_B.CanConnect_R && sCachedNeighbour_B.HasConnectable_R && CanConnect_R && HasConnectable_R && sCachedNeighbour_R != null && sCachedNeighbour_R.CanConnect_B && sCachedNeighbour_R.HasConnectable_B,
        //         CanConnect_B && HasConnectable_B && sCachedNeighbour_B != null && sCachedNeighbour_B.CanConnect_L && sCachedNeighbour_B.HasConnectable_L && CanConnect_L && HasConnectable_L && sCachedNeighbour_L != null && sCachedNeighbour_L.CanConnect_B && sCachedNeighbour_L.HasConnectable_B), 
        //         false
        //     );
        // }
    }
    public void UpdateFloorCornerHider(bool _temporarily) {
        if (_temporarily && _FloorType_ == TempType && TempType != Type.Empty)
            return;

        sTryTempCacheNeighbour_L(GridCoord.x, GridCoord.y);
        sTryTempCacheNeighbour_T(GridCoord.x, GridCoord.y);
        sTryTempCacheNeighbour_R(GridCoord.x, GridCoord.y);
        sTryTempCacheNeighbour_B(GridCoord.x, GridCoord.y);

        if (_temporarily){
			MyUVController.ChangeAsset(MeshSorter.GridLayerEnum.FloorCorners, CachedAssets.Instance.GetFloorCornerAsset(
				CanConnectTempFloor_T && HasConnectableTempFloor_T && sCachedNeighbour_T != null && sCachedNeighbour_T.CanConnectTempFloor_L && sCachedNeighbour_T.HasConnectableTempFloor_L && CanConnectTempFloor_L && HasConnectableTempFloor_L && sCachedNeighbour_L != null && sCachedNeighbour_L.CanConnectTempFloor_T && sCachedNeighbour_L.HasConnectableTempFloor_T,
				CanConnectTempFloor_T && HasConnectableTempFloor_T && sCachedNeighbour_T != null && sCachedNeighbour_T.CanConnectTempFloor_R && sCachedNeighbour_T.HasConnectableTempFloor_R && CanConnectTempFloor_R && HasConnectableTempFloor_R && sCachedNeighbour_R != null && sCachedNeighbour_R.CanConnectTempFloor_T && sCachedNeighbour_R.HasConnectableTempFloor_T,
				CanConnectTempFloor_B && HasConnectableTempFloor_B && sCachedNeighbour_B != null && sCachedNeighbour_B.CanConnectTempFloor_R && sCachedNeighbour_B.HasConnectableTempFloor_R && CanConnectTempFloor_R && HasConnectableTempFloor_R && sCachedNeighbour_R != null && sCachedNeighbour_R.CanConnectTempFloor_B && sCachedNeighbour_R.HasConnectableTempFloor_B,
				CanConnectTempFloor_B && HasConnectableTempFloor_B && sCachedNeighbour_B != null && sCachedNeighbour_B.CanConnectTempFloor_L && sCachedNeighbour_B.HasConnectableTempFloor_L && CanConnectTempFloor_L && HasConnectableTempFloor_L && sCachedNeighbour_L != null && sCachedNeighbour_L.CanConnectTempFloor_B && sCachedNeighbour_L.HasConnectableTempFloor_B),
				true
			);
		}
		else{
			MyUVController.ChangeAsset(MeshSorter.GridLayerEnum.FloorCorners, CachedAssets.Instance.GetFloorCornerAsset(
				CanConnectFloor_T && HasConnectableFloor_T && sCachedNeighbour_T != null && sCachedNeighbour_T.CanConnectFloor_L && sCachedNeighbour_T.HasConnectableFloor_L && CanConnectFloor_L && HasConnectableFloor_L && sCachedNeighbour_L != null && sCachedNeighbour_L.CanConnectFloor_T && sCachedNeighbour_L.HasConnectableFloor_T,
				CanConnectFloor_T && HasConnectableFloor_T && sCachedNeighbour_T != null && sCachedNeighbour_T.CanConnectFloor_R && sCachedNeighbour_T.HasConnectableFloor_R && CanConnectFloor_R && HasConnectableFloor_R && sCachedNeighbour_R != null && sCachedNeighbour_R.CanConnectFloor_T && sCachedNeighbour_R.HasConnectableFloor_T,
				CanConnectFloor_B && HasConnectableFloor_B && sCachedNeighbour_B != null && sCachedNeighbour_B.CanConnectFloor_R && sCachedNeighbour_B.HasConnectableFloor_R && CanConnectFloor_R && HasConnectableFloor_R && sCachedNeighbour_R != null && sCachedNeighbour_R.CanConnectFloor_B && sCachedNeighbour_R.HasConnectableFloor_B,
				CanConnectFloor_B && HasConnectableFloor_B && sCachedNeighbour_B != null && sCachedNeighbour_B.CanConnectFloor_L && sCachedNeighbour_B.HasConnectableFloor_L && CanConnectFloor_L && HasConnectableFloor_L && sCachedNeighbour_L != null && sCachedNeighbour_L.CanConnectFloor_B && sCachedNeighbour_L.HasConnectableFloor_B),
				false
			);
		}
		// if (_temporarily) {
        //     FloorCornerHider.ChangeAsset(CachedAssets.Instance.GetFloorCornerAsset(
        //         CanConnectTempFloor_T && HasConnectableTempFloor_T && sCachedNeighbour_T != null && sCachedNeighbour_T.CanConnectTempFloor_L && sCachedNeighbour_T.HasConnectableTempFloor_L && CanConnectTempFloor_L && HasConnectableTempFloor_L && sCachedNeighbour_L != null && sCachedNeighbour_L.CanConnectTempFloor_T && sCachedNeighbour_L.HasConnectableTempFloor_T,
        //         CanConnectTempFloor_T && HasConnectableTempFloor_T && sCachedNeighbour_T != null && sCachedNeighbour_T.CanConnectTempFloor_R && sCachedNeighbour_T.HasConnectableTempFloor_R && CanConnectTempFloor_R && HasConnectableTempFloor_R && sCachedNeighbour_R != null && sCachedNeighbour_R.CanConnectTempFloor_T && sCachedNeighbour_R.HasConnectableTempFloor_T,
        //         CanConnectTempFloor_B && HasConnectableTempFloor_B && sCachedNeighbour_B != null && sCachedNeighbour_B.CanConnectTempFloor_R && sCachedNeighbour_B.HasConnectableTempFloor_R && CanConnectTempFloor_R && HasConnectableTempFloor_R && sCachedNeighbour_R != null && sCachedNeighbour_R.CanConnectTempFloor_B && sCachedNeighbour_R.HasConnectableTempFloor_B,
        //         CanConnectTempFloor_B && HasConnectableTempFloor_B && sCachedNeighbour_B != null && sCachedNeighbour_B.CanConnectTempFloor_L && sCachedNeighbour_B.HasConnectableTempFloor_L && CanConnectTempFloor_L && HasConnectableTempFloor_L && sCachedNeighbour_L != null && sCachedNeighbour_L.CanConnectTempFloor_B && sCachedNeighbour_L.HasConnectableTempFloor_B),
        //         true
        //     );
        // }
        // else {
        //     FloorCornerHider.ChangeAsset(CachedAssets.Instance.GetFloorCornerAsset(
        //         CanConnectFloor_T && HasConnectableFloor_T && sCachedNeighbour_T != null && sCachedNeighbour_T.CanConnectFloor_L && sCachedNeighbour_T.HasConnectableFloor_L && CanConnectFloor_L && HasConnectableFloor_L && sCachedNeighbour_L != null && sCachedNeighbour_L.CanConnectFloor_T && sCachedNeighbour_L.HasConnectableFloor_T,
        //         CanConnectFloor_T && HasConnectableFloor_T && sCachedNeighbour_T != null && sCachedNeighbour_T.CanConnectFloor_R && sCachedNeighbour_T.HasConnectableFloor_R && CanConnectFloor_R && HasConnectableFloor_R && sCachedNeighbour_R != null && sCachedNeighbour_R.CanConnectFloor_T && sCachedNeighbour_R.HasConnectableFloor_T,
        //         CanConnectFloor_B && HasConnectableFloor_B && sCachedNeighbour_B != null && sCachedNeighbour_B.CanConnectFloor_R && sCachedNeighbour_B.HasConnectableFloor_R && CanConnectFloor_R && HasConnectableFloor_R && sCachedNeighbour_R != null && sCachedNeighbour_R.CanConnectFloor_B && sCachedNeighbour_R.HasConnectableFloor_B,
        //         CanConnectFloor_B && HasConnectableFloor_B && sCachedNeighbour_B != null && sCachedNeighbour_B.CanConnectFloor_L && sCachedNeighbour_B.HasConnectableFloor_L && CanConnectFloor_L && HasConnectableFloor_L && sCachedNeighbour_L != null && sCachedNeighbour_L.CanConnectFloor_B && sCachedNeighbour_L.HasConnectableFloor_B),
        //         false
        //     );
        // }
    }

	// TODO: could these wall/floor-methods be merged?
    public void ChangeWallGraphics(Vector2i _bottomAssetCoord, Vector2i _topAssetCoord, bool _temporary) {
		MyUVController.ChangeAsset(MeshSorter.GridLayerEnum.Bottom, _bottomAssetCoord, _temporary);
		MyUVController.ChangeAsset(MeshSorter.GridLayerEnum.Top, _topAssetCoord, _temporary);
		UpdateWallCornerHider (_temporary);
    }
    public void ResetTempSettingsWall() {
		MyUVController.StopTempMode();
        UpdateWallCornerHider(false);
    }
	public void ChangeFloorGraphics(Vector2i _assetCoord, bool _temporary) {
		MyUVController.ChangeAsset(MeshSorter.GridLayerEnum.Floor, _assetCoord, _temporary);
		UpdateFloorCornerHider (_temporary);
	}
    public void ResetTempSettingsFloor() {
		MyUVController.StopTempMode();
        UpdateFloorCornerHider(false);
    }
    public void SetFloorColor(byte _colorIndex, bool _temporary) {
		MyUVController.ChangeColor(_colorIndex, _temporary);
    }
    public void ResetFloorColor(){
		MyUVController.ResetColor();
    }
    public void SetWallColor(byte _colorIndex, bool _temporary) {
		MyUVController.ChangeColor(_colorIndex, _temporary);
    }
    public void ResetWallColor(){
		MyUVController.ResetColor();
    }

    public void OnActorApproachingTile(TileOrientation _direction) {
		switch (_WallType_) {
            case Type.Empty:
            case Type.Solid:
            case Type.Diagonal:
                break;
            case Type.Door:
                Animator.Animate(Animator.GetDoorAnimation(TileAnimator.AnimationContextEnum.Open).Forward(), null, _loop: false);
                break;
            case Type.Airlock:
                Animator.Animate(
                    Animator.GetAirlockAnimation(TileAnimator.AnimationPartEnum.Bottom, TileAnimator.AnimationContextEnum.Open, _direction).Forward(), 
                    Animator.GetAirlockAnimation(TileAnimator.AnimationPartEnum.Top, TileAnimator.AnimationContextEnum.Open, _direction).Forward(), 
                    _loop: false
                );
                break;
            default:
				throw new System.NotImplementedException(_WallType_ + " hasn't been properly implemented yet!");
        }
    }
    Vector2i[][] animationSequenceTop;
    Vector2i[][] animationSequenceBottom;
    public void OnActorEnterTile(TileOrientation _direction, out float _yieldTime) {
        _yieldTime = 0;
		switch (_WallType_) {
            case Type.Empty:
            case Type.Solid:
            case Type.Diagonal:
                break;
            case Type.Door:
                Animator.Animate(Animator.GetDoorAnimation(TileAnimator.AnimationContextEnum.Open).Reverse(), null, _loop: false);
                break;
            case Type.Airlock:
                animationSequenceTop = new Vector2i[][] {
                    Animator.GetAirlockAnimation(TileAnimator.AnimationPartEnum.Top, TileAnimator.AnimationContextEnum.Open, _direction).Reverse(),
                    Animator.GetAirlockAnimation(TileAnimator.AnimationPartEnum.Top, TileAnimator.AnimationContextEnum.Wait, TileOrientation.None).Forward(),
                    Animator.GetAirlockAnimation(TileAnimator.AnimationPartEnum.Top, TileAnimator.AnimationContextEnum.Open, GetReverseDirection(_direction)).Forward(),
                    Animator.GetAirlockAnimation(TileAnimator.AnimationPartEnum.Top, TileAnimator.AnimationContextEnum.Open, GetReverseDirection(_direction)).Reverse() };
                animationSequenceBottom = new Vector2i[][] {
                    Animator.GetAirlockAnimation(TileAnimator.AnimationPartEnum.Bottom, TileAnimator.AnimationContextEnum.Open, _direction).Reverse(),
                    Animator.GetAirlockAnimation(TileAnimator.AnimationPartEnum.Bottom, TileAnimator.AnimationContextEnum.Wait, TileOrientation.None).Forward(),
                    Animator.GetAirlockAnimation(TileAnimator.AnimationPartEnum.Bottom, TileAnimator.AnimationContextEnum.Open, GetReverseDirection(_direction)).Forward(),
                    Animator.GetAirlockAnimation(TileAnimator.AnimationPartEnum.Bottom, TileAnimator.AnimationContextEnum.Open, GetReverseDirection(_direction)).Reverse() };

                Animator.AnimateSequence(animationSequenceBottom, animationSequenceTop);

                // hardcoded because it probably hopefully shouldn't change much... set to wait until half of the open-animation
                _yieldTime = Animator.GetProperWaitTimeForTileAnim(animationSequenceBottom[0], animationSequenceTop[0]) + 
                             Animator.GetProperWaitTimeForTileAnim(animationSequenceBottom[1], animationSequenceTop[1]) + 
                             (Animator.GetProperWaitTimeForTileAnim(animationSequenceBottom[2], animationSequenceTop[2]) * 0.5f);
                break;
            default:
				throw new System.NotImplementedException(_WallType_ + " hasn't been properly implemented yet!");
        }
    }

    public void SetBuildingAllowed(bool _b) {
        //Tile _neighbour;
        //bool _isAdjacentHorizontally = false;
        //bool _isAdjacentVertically = false;
        _BuildingAllowed_ = false;
        if (_b) {
            // if you have new fail-conditions, insert them here.
            // everything's currently disabled, because now you can do stuff adjacent to doors and stuff
            // keeping it here because it's nifty code I guess

            //int _gridX, _gridY;
            //for (int y = -1; y <= 1; y++) {
            //    for (int x = -1; x <= 1; x++) {
            //        if (x == 0 && y == 0) // need to be able to remove the source of the non-allowance :/
            //            continue;

            //        _gridX = GridX + x;
            //        _gridY = GridY + y;

            //        if (_gridX >= 0 && _gridX < Grid.Instance.GridSizeX && _gridY >= 0 && _gridY < Grid.Instance.GridSizeY) {
            //            _neighbour = Grid.Instance.grid[_gridX, _gridY];

            //            _isAdjacentHorizontally = x != 0 && y == 0;
            //            _isAdjacentVertically = x == 0 && y != 0;

            //            // fail conditions
            //            if ((_isAdjacentHorizontally || _isAdjacentVertically) && _neighbour._WallType_ == Type.Door)
            //                return;
            //            if ((_isAdjacentHorizontally || _isAdjacentVertically) && _neighbour._WallType_ == Type.Airlock)
            //                return;
            //        }
            //    }
            //}
        }

        _BuildingAllowed_ = _b;
    }
}
