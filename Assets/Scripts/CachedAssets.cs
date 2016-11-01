using UnityEngine;
using System.Collections;

public class CachedAssets : MonoBehaviour {

    public static CachedAssets Instance;

    [System.Serializable]
    public struct OrientedAsset {
        [SerializeField] private Sprite Back;
        [SerializeField] private Sprite Front;
        [SerializeField] private Sprite Left;
        [SerializeField] private Sprite Right;

        public Sprite GetOrientedAsset(ActorOrientation.OrientationEnum _orientation) {
            switch (_orientation) {
                case ActorOrientation.OrientationEnum.Down:
                    return Front;
                case ActorOrientation.OrientationEnum.Up:
                    return Back;
                case ActorOrientation.OrientationEnum.Left:
                    return Left;
                case ActorOrientation.OrientationEnum.Right:
                    return Right;
            }

            return null;
        }
    }

    [System.Serializable]
    public struct WallSet {

        [Header("Single Walls")]
        public Sprite Single;

        [Header("Vertical Walls")]
        public Sprite Vertical_T;
        public Sprite Vertical_M;
        public Sprite Vertical_B;

        [Header("Horizontal Walls")]
        public Sprite Horizontal_L;
        public Sprite Horizontal_M;
        public Sprite Horizontal_R;

        [Header("Corner Walls")]
        public Sprite Corner_TopLeft;
        public Sprite Corner_TopRight;
        public Sprite Corner_BottomRight;
        public Sprite Corner_BottomLeft;

        [Header("T-Walls")]
        public Sprite Tee_L;
        public Sprite Tee_T;
        public Sprite Tee_R;
        public Sprite Tee_B;

        [Header("4-Way Walls")]
        public Sprite FourWay;

        [Header("Diagonal Walls")]
        public Sprite Diagonal_TopLeft;
        public Sprite Diagonal_TopRight;
        public Sprite Diagonal_BottomRight;
        public Sprite Diagonal_BottomLeft;

    }

    [Header("Character Assets")]
    public OrientedAsset[] HairStyles;
    public OrientedAsset[] Heads;
    public OrientedAsset[] Eyes;
    public OrientedAsset[] Beards;

    [Header("Grid Assets")]
    public WallSet[] WallSets;


    void Awake() {
        Instance = this;
    }

    public Color[] GetCachedAssetPixels(Sprite _asset) {
        return _asset.texture.GetPixels(Mathf.RoundToInt(_asset.rect.xMin), Mathf.RoundToInt(_asset.rect.yMin), Mathf.RoundToInt(_asset.rect.width), Mathf.RoundToInt(_asset.rect.height));
    }

    public Sprite GetAssetForTile(Tile _tile, int _styleIndex, bool _isOnGroundLevel) {
        switch (_tile._Type_) {
            case Tile.TileType.Wall:
                bool _left = _tile.HasConnectable_L;
                bool _top = _tile.HasConnectable_T;
                bool _right = _tile.HasConnectable_R;
                bool _bottom = _tile.HasConnectable_B;
                if (_left) {
                    if (_top) {
                        if (_right) {
                            if (_bottom) return WallSets[_styleIndex].FourWay;
                            else return WallSets[_styleIndex].Tee_B;
                        }
                        else if (_bottom) return WallSets[_styleIndex].Tee_R;
                        else return WallSets[_styleIndex].Corner_TopLeft;
                    }
                    else if (_right) {
                        if (_bottom) return WallSets[_styleIndex].Tee_T;
                        else return WallSets[_styleIndex].Horizontal_M;
                    }
                    else if (_bottom) return WallSets[_styleIndex].Corner_BottomLeft;
                    else return WallSets[_styleIndex].Horizontal_R;
                }
                else if (_top) {
                    if (_right) {
                        if (_bottom) return WallSets[_styleIndex].Tee_L;
                        else return WallSets[_styleIndex].Corner_TopRight;
                    }
                    else if (_bottom) return WallSets[_styleIndex].Vertical_M;
                    else return WallSets[_styleIndex].Vertical_B;
                }
                else if (_right) {
                    if (_bottom) return WallSets[_styleIndex].Corner_BottomRight;
                    else return WallSets[_styleIndex].Horizontal_L;
                }
                else if (_bottom) return WallSets[_styleIndex].Vertical_T;
                else return WallSets[_styleIndex].Single;

            case Tile.TileType.Diagonal:
                switch (_tile._Orientation_) {
                    case Tile.TileOrientation.BottomLeft:
                        return WallSets[_styleIndex].Diagonal_BottomLeft;
                    case Tile.TileOrientation.TopLeft:
                        return WallSets[_styleIndex].Diagonal_TopLeft;
                    case Tile.TileOrientation.TopRight:
                        return WallSets[_styleIndex].Diagonal_TopRight;
                    case Tile.TileOrientation.BottomRight:
                        return WallSets[_styleIndex].Diagonal_BottomRight;
                }
                break;
            case Tile.TileType.Door:
                break;
            case Tile.TileType.DoorEntrance:
                break;
        }

        return null;
    }
}
