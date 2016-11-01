using UnityEngine;
using System.Collections.Generic;

public class Tile : IHeapItem<Tile> {

    public enum TileType { Empty, Wall, Diagonal, Door, DoorEntrance }
    private TileType type = TileType.Empty;
    public TileType _Type_ { get { return type; } }
    public enum TileOrientation { None, Bottom, BottomLeft, Left, TopLeft, Top, TopRight, Right, BottomRight }
    private TileOrientation orientation = TileOrientation.None;
    public TileOrientation _Orientation_ { get { return orientation; } }

    public bool Walkable { get; private set; }
    public bool IsOccupied = false;
    public int GridX { get; private set; }
    public int GridY { get; private set; }
    public int LocalGridX { get; private set; }
    public int LocalGridY { get; private set; }
    public int GridSliceIndex { get; private set; }

    public Vector3 WorldPosition { get; private set; }
    public Vector3 DefaultPositionWorld { get; private set; }
    public Vector3 CharacterPositionWorld { // the position a character should stand on (exists to better simulate zero-g)
        get {
            if (type == TileType.Empty || type == TileType.DoorEntrance) {
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


    public void SetTileType(TileType _type, TileOrientation _orientation) {
        type = _type;
        orientation = _orientation;

        switch (_type) {
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
                switch (_orientation) {
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
            default:
                throw new System.Exception(_type.ToString() + " has not been implemented yet!");
        }
    }
}
