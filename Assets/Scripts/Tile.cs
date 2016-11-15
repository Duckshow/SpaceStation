using UnityEngine;
using System.Collections.Generic;

public class Tile : IHeapItem<Tile> {

    public enum TileType { Empty, Wall, Diagonal, Door }
    private TileType type = TileType.Empty;
    public TileType _Type_ { get { return type; } }
    private TileType prevType = TileType.Empty;
    public TileType _PrevType_ { get { return prevType; } }
    public enum TileOrientation { None, Bottom, BottomLeft, Left, TopLeft, Top, TopRight, Right, BottomRight }
    private TileOrientation orientation = TileOrientation.None;
    public TileOrientation _Orientation_ { get { return orientation; } }
    public bool _IsHorizontal_ { get { return orientation == TileOrientation.Left || orientation == TileOrientation.Right; } }
    public bool _IsVertical_ { get { return orientation == TileOrientation.Bottom || orientation == TileOrientation.Top; } }

    public bool Walkable { get; private set; }
    public bool IsOccupied = false;
    private bool buildingAllowed = true;
    public bool _BuildingAllowed_ { get { return buildingAllowed; } private set { buildingAllowed = value; } }
    public int GridX { get; private set; }
    public int GridY { get; private set; }
    public int LocalGridX { get; private set; }
    public int LocalGridY { get; private set; }
    public int GridSliceIndex { get; private set; }

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

    public int MovementPenalty { get; private set; }

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

    [HideInInspector] public Tile ConnectedDiagonal_L;
	[HideInInspector] public Tile ConnectedDiagonal_T;
	[HideInInspector] public Tile ConnectedDiagonal_R;
	[HideInInspector] public Tile ConnectedDiagonal_B;
    [HideInInspector] public Tile ConnectedDoor_L;
	[HideInInspector] public Tile ConnectedDoor_T;
	[HideInInspector] public Tile ConnectedDoor_R;
	[HideInInspector] public Tile ConnectedDoor_B;

    [HideInInspector] public Tile ParentTile;
    [HideInInspector] public int GCost;
    [HideInInspector] public int HCost;
    public int _FCost_ { get { return GCost + HCost; } }

    private int heapIndex;
    public int HeapIndex {
        get { return heapIndex; }
        set { heapIndex = value; }
    }


    public Tile(TileType _type, TileOrientation _orientation, Vector3 _worldPos, int _gridX, int _gridY, int _localGridX, int _localGridY, int _gridSliceIndex, int _penalty) {
        WorldPosition = _worldPos;
        SetTileType(_type, _orientation);
        GridX = _gridX;
        GridY = _gridY;
        LocalGridX = _localGridX;
        LocalGridY = _localGridY;
        GridSliceIndex = _gridSliceIndex;
        MovementPenalty = _penalty;
    }

    public int CompareTo(Tile nodeToCompare) {
        int compare = _FCost_.CompareTo(nodeToCompare._FCost_);
        if (compare == 0)
            compare = HCost.CompareTo(nodeToCompare.HCost);

        return -compare;
    }

    public void SetTileType(TileType _newType, TileOrientation _newOrientation) {
        prevType = type;
        type = _newType;
        orientation = _newOrientation;

        if (prevType == TileType.Door) {
            Grid.Instance.grid[GridX + 1, GridY].SetBuildingAllowed(true);
            Grid.Instance.grid[GridX - 1, GridY].SetBuildingAllowed(true);
            Grid.Instance.grid[GridX, GridY + 1].SetBuildingAllowed(true);
            Grid.Instance.grid[GridX, GridY - 1].SetBuildingAllowed(true);

            Grid.Instance.grid[GridX - 1, GridY].ConnectedDoor_R = null;
            Grid.Instance.grid[GridX + 1, GridY].ConnectedDoor_L = null;
            Grid.Instance.grid[GridX, GridY - 1].ConnectedDoor_T = null;
            Grid.Instance.grid[GridX, GridY + 1].ConnectedDoor_B = null;
        }
        if (prevType == TileType.Diagonal) {
            Grid.Instance.grid[GridX - 1, GridY].ConnectedDiagonal_R = null;
            Grid.Instance.grid[GridX + 1, GridY].ConnectedDiagonal_L = null;
            Grid.Instance.grid[GridX, GridY - 1].ConnectedDiagonal_T = null;
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

                        Grid.Instance.grid[GridX - 1, GridY].ConnectedDiagonal_R = this;
                        Grid.Instance.grid[GridX, GridY - 1].ConnectedDiagonal_T = this;
                        break;
                    case TileOrientation.TopLeft:
                        Walkable = true;
                        DefaultPositionWorld = WorldPosition + new Vector3(0.25f, -0.25f, 0);
                        CanConnect_L = true;
                        CanConnect_T = true;
                        CanConnect_R = false;
                        CanConnect_B = false;

                        Grid.Instance.grid[GridX - 1, GridY].ConnectedDiagonal_R = this;
                        Grid.Instance.grid[GridX, GridY + 1].ConnectedDiagonal_B = this;
                        break;
                    case TileOrientation.TopRight:
                        Walkable = true;
                        DefaultPositionWorld = WorldPosition + new Vector3(-0.25f, -0.25f, 0);
                        CanConnect_L = false;
                        CanConnect_T = true;
                        CanConnect_R = true;
                        CanConnect_B = false;

                        Grid.Instance.grid[GridX + 1, GridY].ConnectedDiagonal_L = this;
                        Grid.Instance.grid[GridX, GridY + 1].ConnectedDiagonal_B = this;
                        break;
                    case TileOrientation.BottomRight:
                        Walkable = true;
                        DefaultPositionWorld = WorldPosition + new Vector3(-0.25f, 0.25f, 0);
                        CanConnect_L = false;
                        CanConnect_T = false;
                        CanConnect_R = true;
                        CanConnect_B = true;

                        Grid.Instance.grid[GridX + 1, GridY].ConnectedDiagonal_L = this;
                        Grid.Instance.grid[GridX, GridY - 1].ConnectedDiagonal_T = this;
                        break;
                }
                break;
            case TileType.Door:
                Walkable = true;
                DefaultPositionWorld = WorldPosition;
                switch (orientation) {
                    // vertical
                    case TileOrientation.Bottom:
                    case TileOrientation.Top:
                        CanConnect_L = false;
                        CanConnect_T = true;
                        CanConnect_R = false;
                        CanConnect_B = true;

                        Grid.Instance.grid[GridX + 1, GridY].SetBuildingAllowed(false);
                        Grid.Instance.grid[GridX - 1, GridY].SetBuildingAllowed(false);
                        Grid.Instance.grid[GridX, GridY + 1].ConnectedDoor_B = this;
                        Grid.Instance.grid[GridX, GridY - 1].ConnectedDoor_T = this;
                        break;
                    // horizontal
                    case TileOrientation.Left:
                    case TileOrientation.Right:
                        CanConnect_L = true;
                        CanConnect_T = false;
                        CanConnect_R = true;
                        CanConnect_B = false;

                        Grid.Instance.grid[GridX, GridY + 1].SetBuildingAllowed(false);
                        Grid.Instance.grid[GridX, GridY - 1].SetBuildingAllowed(false);
                        Grid.Instance.grid[GridX + 1, GridY].ConnectedDoor_L = this;
                        Grid.Instance.grid[GridX - 1, GridY].ConnectedDoor_R = this;
                        break;
                }
                break;
            default:
                throw new System.Exception(_newType.ToString() + " has not been implemented yet!");
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
