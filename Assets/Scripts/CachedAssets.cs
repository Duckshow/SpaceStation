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
	public class DoubleInt {
        public int X;
        public int Y;
        public DoubleInt(int _x, int _y) {
            X = _x;
            Y = _y;
        }
    }
    [System.Serializable]
    public class WallSet {

        public const float TEXTURE_SIZE_X = 256;
        public const float TEXTURE_SIZE_Y = 2176;

        public static DoubleInt index_Single = new DoubleInt(0, 0);
        public static DoubleInt index_Vertical_T = new DoubleInt(1, 0);
        public static DoubleInt index_Vertical_M = new DoubleInt(2, 0);
        public static DoubleInt index_Vertical_B = new DoubleInt(3, 0);

        public static DoubleInt index_FourWay = new DoubleInt(0, 2);
        public static DoubleInt index_Horizontal_L = new DoubleInt(1, 2);
        public static DoubleInt index_Horizontal_M = new DoubleInt(2, 2);
        public static DoubleInt index_Horizontal_R = new DoubleInt(3, 2);

        public static DoubleInt index_Corner_TopRight = new DoubleInt(0, 4);
        public static DoubleInt index_Corner_TopLeft = new DoubleInt(1, 4);
        public static DoubleInt index_Corner_BottomRight = new DoubleInt(2, 4);
        public static DoubleInt index_Corner_BottomLeft = new DoubleInt(3, 4);

        public static DoubleInt index_Tee_Right = new DoubleInt(0, 6);
        public static DoubleInt index_Tee_Left = new DoubleInt(1, 6);
        public static DoubleInt index_Tee_Top = new DoubleInt(2, 6);
        public static DoubleInt index_Tee_Bottom = new DoubleInt(3, 6);

        public static DoubleInt index_Diagonal_TopRight = new DoubleInt(0, 8);
        public static DoubleInt index_Diagonal_TopLeft = new DoubleInt(1, 8);
        public static DoubleInt index_Diagonal_BottomRight = new DoubleInt(2, 8);
        public static DoubleInt index_Diagonal_BottomLeft = new DoubleInt(3, 8);

        public static TileAnimator.TileAnimation anim_DoorVertical_Open = new TileAnimator.TileAnimation(
            new DoubleInt[] { new DoubleInt(0, 10), new DoubleInt(1, 10), new DoubleInt(2, 10), new DoubleInt(3, 10)},
            new DoubleInt[] { new DoubleInt(0, 12), new DoubleInt(1, 12), new DoubleInt(2, 12), new DoubleInt(3, 12) });
        public static TileAnimator.TileAnimation anim_DoorVertical_Close = new TileAnimator.TileAnimation(
            new DoubleInt[] { new DoubleInt(3, 10), new DoubleInt(2, 10), new DoubleInt(1, 10), new DoubleInt(0, 10) },
            new DoubleInt[] { new DoubleInt(3, 12), new DoubleInt(2, 12), new DoubleInt(1, 12), new DoubleInt(0, 12) });

        public static TileAnimator.TileAnimation anim_DoorHorizontal_Open = new TileAnimator.TileAnimation(
            new DoubleInt[] { new DoubleInt(0, 14), new DoubleInt(1, 14), new DoubleInt(2, 14), new DoubleInt(3, 14)},
            new DoubleInt[] { new DoubleInt(0, 16), new DoubleInt(1, 16), new DoubleInt(2, 16), new DoubleInt(3, 16) });
        public static TileAnimator.TileAnimation anim_DoorHorizontal_Close = new TileAnimator.TileAnimation(
            new DoubleInt[] { new DoubleInt(3, 14), new DoubleInt(2, 14), new DoubleInt(1, 14), new DoubleInt(0, 14) },
            new DoubleInt[] { new DoubleInt(3, 16), new DoubleInt(2, 16), new DoubleInt(1, 16), new DoubleInt(0, 16) });

        public static TileAnimator.TileAnimation anim_AirlockHorizontal_OpenBottom = new TileAnimator.TileAnimation(
            new DoubleInt[] { new DoubleInt(0, 18), new DoubleInt(1, 18), new DoubleInt(2, 18), new DoubleInt(3, 18) }, 
            new DoubleInt[] { new DoubleInt(0, 20), new DoubleInt(1, 20), new DoubleInt(2, 20), new DoubleInt(3, 20) });
        public static TileAnimator.TileAnimation anim_AirlockHorizontal_CloseBottom = new TileAnimator.TileAnimation(
            new DoubleInt[] { new DoubleInt(3, 18), new DoubleInt(2, 18), new DoubleInt(1, 18), new DoubleInt(0, 18) },
            new DoubleInt[] { new DoubleInt(3, 20), new DoubleInt(2, 20), new DoubleInt(1, 20), new DoubleInt(0, 20) });

        public static TileAnimator.TileAnimation anim_AirlockHorizontal_OpenTop = new TileAnimator.TileAnimation(
            new DoubleInt[] { new DoubleInt(0, 18), new DoubleInt(0, 18), new DoubleInt(0, 18), new DoubleInt(0, 18) },
            new DoubleInt[] { new DoubleInt(0, 20), new DoubleInt(0, 20), new DoubleInt(0, 20), new DoubleInt(0, 20) });
        public static TileAnimator.TileAnimation anim_AirlockHorizontal_CloseTop = new TileAnimator.TileAnimation(
            new DoubleInt[] { new DoubleInt(0, 18), new DoubleInt(0, 18), new DoubleInt(0, 18), new DoubleInt(0, 18) },
            new DoubleInt[] { new DoubleInt(0, 20), new DoubleInt(0, 20), new DoubleInt(0, 20), new DoubleInt(0, 20) });

        public static TileAnimator.TileAnimation anim_AirlockHorizontal_Wait = new TileAnimator.TileAnimation(
            new DoubleInt[] { new DoubleInt(0, 18), new DoubleInt(0, 18), new DoubleInt(0, 18), new DoubleInt(0, 18) },
            new DoubleInt[] { new DoubleInt(0, 22), new DoubleInt(1, 22), new DoubleInt(2, 22), new DoubleInt(3, 22) });

        public static TileAnimator.TileAnimation anim_AirlockVertical_OpenLeft = new TileAnimator.TileAnimation(
            new DoubleInt[] { new DoubleInt(0, 24), new DoubleInt(1, 24), new DoubleInt(2, 24), new DoubleInt(3, 24) },
            new DoubleInt[] { new DoubleInt(0, 26), new DoubleInt(1, 26), new DoubleInt(2, 26), new DoubleInt(3, 26) });
        public static TileAnimator.TileAnimation anim_AirlockVertical_CloseLeft = new TileAnimator.TileAnimation(
            new DoubleInt[] { new DoubleInt(3, 24), new DoubleInt(2, 24), new DoubleInt(1, 24), new DoubleInt(0, 24) },
            new DoubleInt[] { new DoubleInt(3, 26), new DoubleInt(2, 26), new DoubleInt(1, 26), new DoubleInt(0, 26) });

        public static TileAnimator.TileAnimation anim_AirlockVertical_OpenRight = new TileAnimator.TileAnimation(
            new DoubleInt[] { new DoubleInt(0, 24), new DoubleInt(1, 28), new DoubleInt(2, 28), new DoubleInt(3, 28) },
            new DoubleInt[] { new DoubleInt(0, 26), new DoubleInt(1, 30), new DoubleInt(2, 30), new DoubleInt(3, 30) });
        public static TileAnimator.TileAnimation anim_AirlockVertical_CloseRight = new TileAnimator.TileAnimation(
            new DoubleInt[] { new DoubleInt(3, 28), new DoubleInt(2, 28), new DoubleInt(1, 28), new DoubleInt(3, 24) },
            new DoubleInt[] { new DoubleInt(3, 30), new DoubleInt(2, 30), new DoubleInt(1, 30), new DoubleInt(0, 26) });

        public static TileAnimator.TileAnimation anim_AirlockVertical_Wait = new TileAnimator.TileAnimation(
            new DoubleInt[] { new DoubleInt(0, 24), new DoubleInt(0, 24), new DoubleInt(0, 24), new DoubleInt(0, 24) },
            new DoubleInt[] { new DoubleInt(0, 32), new DoubleInt(1, 32), new DoubleInt(2, 32), new DoubleInt(3, 32) });
    }

    public GameObject TilePrefab;

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

    public DoubleInt GetAssetForTile(Tile.TileType _tileType, Tile.TileOrientation _tileOrientation, int _styleIndex, bool _isBottom, bool _hasConnection_Left, bool _hasConnection_Top, bool _hasConnection_Right, bool _hasConnection_Bottom) {
        switch (_tileType) {
            case Tile.TileType.Empty:
                return null;
            case Tile.TileType.Wall:
                if (!_isBottom) // for now at least
                    return null;

                if (_hasConnection_Left) {
                    if (_hasConnection_Top) {
                        if (_hasConnection_Right) {
                            if (_hasConnection_Bottom) return WallSet.index_FourWay;
                            else return WallSet.index_Tee_Top;
                        }
                        else if (_hasConnection_Bottom) return WallSet.index_Tee_Left;
                        else return WallSet.index_Corner_TopLeft;
                    }
                    else if (_hasConnection_Right) {
                        if (_hasConnection_Bottom) return WallSet.index_Tee_Bottom;
                        else return WallSet.index_Horizontal_M;
                    }
                    else if (_hasConnection_Bottom) return WallSet.index_Corner_BottomLeft;
                    else return WallSet.index_Horizontal_R;
                }
                else if (_hasConnection_Top) {
                    if (_hasConnection_Right) {
                        if (_hasConnection_Bottom) return WallSet.index_Tee_Right;
                        else return WallSet.index_Corner_TopRight;
                    }
                    else if (_hasConnection_Bottom) return WallSet.index_Vertical_M;
                    else return WallSet.index_Vertical_B;
                }
                else if (_hasConnection_Right) {
                    if (_hasConnection_Bottom) return WallSet.index_Corner_BottomRight;
                    else return WallSet.index_Horizontal_L;
                }
                else if (_hasConnection_Bottom) return WallSet.index_Vertical_T;
                else return WallSet.index_Single;

            case Tile.TileType.Diagonal:
                switch (_tileOrientation) {
                    case Tile.TileOrientation.BottomLeft:
                        return (_isBottom ? null : WallSet.index_Diagonal_BottomLeft);
                    case Tile.TileOrientation.BottomRight:
                        return (_isBottom ? null : WallSet.index_Diagonal_BottomRight);
                    case Tile.TileOrientation.TopLeft:
                        return (_isBottom ? WallSet.index_Diagonal_TopLeft : null);
                    case Tile.TileOrientation.TopRight:
                        return (_isBottom ? WallSet.index_Diagonal_TopRight : null);
                }
                break;
            case Tile.TileType.Door:
                switch (_tileOrientation) {
                    case Tile.TileOrientation.None:
                    case Tile.TileOrientation.Bottom:
                    case Tile.TileOrientation.Top:
                        return _isBottom ? WallSet.anim_DoorVertical_Open.GetBottomFirstFrame() : WallSet.anim_DoorVertical_Open.GetTopFirstFrame();
                    case Tile.TileOrientation.Left:
                    case Tile.TileOrientation.Right:
                        return _isBottom ? WallSet.anim_DoorHorizontal_Open.GetBottomFirstFrame() : WallSet.anim_DoorHorizontal_Open.GetTopFirstFrame();
                }
                break;
            default:
                throw new System.NotImplementedException(_tileType + " hasn't been properly implemented yet!");
        }

        return null;
    }

    //public TileAnimator.TileAnimation[] GetAnimationForTile(TileAnimator.AnimationContextEnum _context, Tile.TileType _tileType, Tile.TileOrientation _tileOrientation, int _styleIndex) {
    //    switch (_tileType) {
    //        case Tile.TileType.Door:
    //            switch (_tileOrientation) {
    //                case Tile.TileOrientation.None:
    //                case Tile.TileOrientation.Bottom:
    //                case Tile.TileOrientation.Top:
    //                    switch (_context) {
    //                        case TileAnimator.AnimationContextEnum.Entry:
    //                            break;
    //                        case TileAnimator.AnimationContextEnum.Exit:
    //                            break;
    //                        case TileAnimator.AnimationContextEnum.Wait:
    //                            break;
    //                        default:
    //                            throw new System.NotImplementedException(_context + " hasn't been properly implemented yet!");
    //                    }
    //                    return _getBottomLayer ? null : WallSet.index_DoorVertical_Animation;
    //                case Tile.TileOrientation.Left:
    //                case Tile.TileOrientation.Right:
    //                    return _getBottomLayer ? WallSet.index_DoorHorizontal_Bottom_Animation : WallSet.index_DoorHorizontal_Top_Animation;
    //            }
    //            break;
    //        default:
    //            throw new System.NotImplementedException(_tileType + " doesn't appear to have an animation!");
    //    }

    //    return null;
    //}
}
