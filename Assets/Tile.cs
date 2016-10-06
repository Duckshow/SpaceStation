using UnityEngine;
using System.Collections.Generic;

public class Tile : IHeapItem<Tile> {

    public enum TileType { Default, Wall, Diagonal_LT, Diagonal_TR, Diagonal_RB, Diagonal_BL }
    private TileType type = TileType.Default;
    public TileType _Type_ { get { return type; } }

    public bool Walkable { get; private set; }
    public bool IsOccupied = false;
    public int GridX { get; private set; }
    public int GridY { get; private set; }
    public int LocalGridX { get; private set; }
    public int LocalGridY { get; private set; }
    public int GridSliceIndex { get; private set; }
    public Vector3 WorldPosition { get; private set; }
    public Vector3 CenterPositionWorld { get; private set; }
    public int MovementPenalty { get; private set; }

    public bool CanConnect_L { get; private set; }
    public bool CanConnect_T { get; private set; }
    public bool CanConnect_R { get; private set; }
    public bool CanConnect_B { get; private set; }

    [HideInInspector] public bool HasConnectable_L = false;
    [HideInInspector] public bool HasConnectable_T = false;
    [HideInInspector] public bool HasConnectable_R = false;
    [HideInInspector] public bool HasConnectable_B = false;
    //[HideInInspector] public bool HasConnectable_LT = false;
    //[HideInInspector] public bool HasConnectable_TR = false;
    //[HideInInspector] public bool HasConnectable_RB = false;
    //[HideInInspector] public bool HasConnectable_BL = false;

    [HideInInspector] public Tile ParentTile;
    [HideInInspector] public int GCost;
    [HideInInspector] public int HCost;
    public int _FCost_ { get { return GCost + HCost; } }

    private int heapIndex;
    public int HeapIndex {
        get { return heapIndex; }
        set { heapIndex = value; }
    }


    public Tile(TileType _type, Vector3 _worldPos, int _gridX, int _gridY, int _localGridX, int _localGridY, int _gridSliceIndex, int _penalty) {
        WorldPosition = _worldPos;
        SetTileType(_type);
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

    public void SetTileType(TileType _type) {
        type = _type;

        switch (_type) {
            case TileType.Default:
                Walkable = true;
                CenterPositionWorld = WorldPosition;
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
            case TileType.Diagonal_LT:
                Walkable = true;
                CenterPositionWorld = WorldPosition + new Vector3(0.25f, 0, -0.25f);
                CanConnect_L = true;
                CanConnect_T = true;
                CanConnect_R = false;
                CanConnect_B = false;
                break;
            case TileType.Diagonal_TR:
                Walkable = true;
                CenterPositionWorld = WorldPosition + new Vector3(-0.25f, 0, -0.25f);
                CanConnect_L = false;
                CanConnect_T = true;
                CanConnect_R = true;
                CanConnect_B = false;
                break;
            case TileType.Diagonal_RB:
                Walkable = true;
                CenterPositionWorld = WorldPosition + new Vector3(-0.25f, 0, 0.25f);
                CanConnect_L = false;
                CanConnect_T = false;
                CanConnect_R = true;
                CanConnect_B = true;
                break;
            case TileType.Diagonal_BL:
                Walkable = true;
                CenterPositionWorld = WorldPosition + new Vector3(0.25f, 0, 0.25f);
                CanConnect_L = true;
                CanConnect_T = false;
                CanConnect_R = false;
                CanConnect_B = true;
                break;
            default:
                throw new System.Exception(_type.ToString() + " has not been implemented yet!");
        }
    }
}
