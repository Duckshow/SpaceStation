using UnityEngine;
using System.Collections.Generic;

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

        public const float TEXTURE_SIZE_X = 960;
        public const float TEXTURE_SIZE_Y = 2688;

        private enum P { // P for Purpose
            Floor_Single,
            Floor_Fourway,
            Floor_Vertical_T,
            Floor_Vertical_M,
            Floor_Vertical_B,
            Floor_Horizontal_L,
            Floor_Horizontal_M,
            Floor_Horizontal_R,
            Floor_Corner_TR,
            Floor_Corner_TL,
            Floor_Corner_BR,
            Floor_Corner_BL,
            Floor_Tee_R,
            Floor_Tee_L,
            Floor_Tee_T,
            Floor_Tee_B,
            Floor_Diagonal_TR,
            Floor_Diagonal_TL,
            Floor_Diagonal_BR,
            Floor_Diagonal_BL,
            FloorCornerHider_All,
            FloorCornerHider_TL_BR,
            FloorCornerHider_TR_BL,
            FloorCornerHider_TL,
            FloorCornerHider_TL_TR,
            FloorCornerHider_TL_TR_BR,
            FloorCornerHider_TR,
            FloorCornerHider_TR_BR,
            FloorCornerHider_TR_BR_BL,
            FloorCornerHider_BR,
            FloorCornerHider_BR_BL,
            FloorCornerHider_BR_BL_TL,
            FloorCornerHider_BL,
            FloorCornerHider_BL_TL,
            FloorCornerHider_BL_TL_TR,
            Wall_Single,
            Wall_Fourway,
            Wall_Vertical_T,
            Wall_Vertical_M,
            Wall_Vertical_B,
            Wall_Horizontal_L,
            Wall_Horizontal_M,
            Wall_Horizontal_R,
            Wall_Corner_TR,
            Wall_Corner_TL,
            Wall_Corner_BR,
            Wall_Corner_BL,
            Wall_Tee_R,
            Wall_Tee_L,
            Wall_Tee_T,
            Wall_Tee_B,
            Wall_Diagonal_TR,
            Wall_Diagonal_TL,
            Wall_Diagonal_BR,
            Wall_Diagonal_BL,
            WallCornerHider_All,
            WallCornerHider_TL_BR,
            WallCornerHider_TR_BL,
            WallCornerHider_TL,
            WallCornerHider_TL_TR,
            WallCornerHider_TL_TR_BR,
            WallCornerHider_TR,
            WallCornerHider_TR_BR,
            WallCornerHider_TR_BR_BL,
            WallCornerHider_BR,
            WallCornerHider_BR_BL,
            WallCornerHider_BR_BL_TL,
            WallCornerHider_BL,
            WallCornerHider_BL_TL,
            WallCornerHider_BL_TL_TR,
            DoorVertical_BOTTOM,
            DoorVertical_TOP,
            DoorHorizontal_BOTTOM,
            DoorHorizontal_TOP,
            AirlockHorizontal_OpenBottom_BOTTOM,
            AirlockHorizontal_OpenBottom_TOP,
            AirlockHorizontal_OpenTop,
            AirlockHorizontal_Wait,
            AirlockVertical_OpenLeft_BOTTOM,
            AirlockVertical_OpenLeft_TOP,
            AirlockVertical_OpenRight_BOTTOM,
            AirlockVertical_OpenRight_TOP,
            AirlockVertical_Wait
        }
        private static List<List<P>> AllAssetPurposes = new List<List<P>>() {
            new List<P>() { P.Floor_Single, P.Floor_Fourway, P.Floor_Vertical_T, P.Floor_Vertical_M, P.Floor_Vertical_B, P.Floor_Horizontal_L, P.Floor_Horizontal_M, P.Floor_Horizontal_R },
            new List<P>() { P.Floor_Corner_TR, P.Floor_Corner_TL, P.Floor_Corner_BR, P.Floor_Corner_BL, P.Floor_Tee_R, P.Floor_Tee_L, P.Floor_Tee_T, P.Floor_Tee_B },
            new List<P>() { P.Floor_Diagonal_TR, P.Floor_Diagonal_TL, P.Floor_Diagonal_BR, P.Floor_Diagonal_BL},
            new List<P>() { P.FloorCornerHider_All, P.FloorCornerHider_TL_BR, P.FloorCornerHider_TR_BL, P.FloorCornerHider_TL, P.FloorCornerHider_TL_TR, P.FloorCornerHider_TL_TR_BR, P.FloorCornerHider_TR, P.FloorCornerHider_TR_BR, P.FloorCornerHider_TR_BR_BL, P.FloorCornerHider_BR, P.FloorCornerHider_BR_BL, P.FloorCornerHider_BR_BL_TL, P.FloorCornerHider_BL, P.FloorCornerHider_BL_TL, P.FloorCornerHider_BL_TL_TR,},
            new List<P>() { P.Wall_Single, P.Wall_Fourway, P.Wall_Vertical_T, P.Wall_Vertical_M, P.Wall_Vertical_B, P.Wall_Horizontal_L, P.Wall_Horizontal_M, P.Wall_Horizontal_R },
            new List<P>() { P.Wall_Corner_TR, P.Wall_Corner_TL, P.Wall_Corner_BR, P.Wall_Corner_BL, P.Wall_Tee_R, P.Wall_Tee_L, P.Wall_Tee_T, P.Wall_Tee_B },
            new List<P>() { P.Wall_Diagonal_TR, P.Wall_Diagonal_TL, P.Wall_Diagonal_BR, P.Wall_Diagonal_BL},
            new List<P>() { P.WallCornerHider_All, P.WallCornerHider_TL_BR, P.WallCornerHider_TR_BL, P.WallCornerHider_TL, P.WallCornerHider_TL_TR, P.WallCornerHider_TL_TR_BR, P.WallCornerHider_TR, P.WallCornerHider_TR_BR, P.WallCornerHider_TR_BR_BL, P.WallCornerHider_BR, P.WallCornerHider_BR_BL, P.WallCornerHider_BR_BL_TL, P.WallCornerHider_BL, P.WallCornerHider_BL_TL, P.WallCornerHider_BL_TL_TR,},
            new List<P>() { P.DoorVertical_BOTTOM },
            new List<P>() { P.DoorVertical_TOP },
            new List<P>() { P.DoorHorizontal_BOTTOM },
            new List<P>() { P.DoorHorizontal_TOP },
            new List<P>() { P.AirlockHorizontal_OpenBottom_BOTTOM },
            new List<P>() { P.AirlockHorizontal_OpenBottom_TOP },
            new List<P>() { P.AirlockHorizontal_OpenTop },
            new List<P>() { P.AirlockHorizontal_Wait },
            new List<P>() { P.AirlockVertical_OpenLeft_BOTTOM },
            new List<P>() { P.AirlockVertical_OpenLeft_TOP },
            new List<P>() { P.AirlockVertical_OpenRight_BOTTOM },
            new List<P>() { P.AirlockVertical_OpenRight_TOP },
            new List<P>() { P.AirlockVertical_Wait }
        };
        private static int GetCoordY(P id) {
            return AllAssetPurposes.FindIndex(x => x.Contains(id)) * 2;
        }

        public static DoubleInt floor_Single = new DoubleInt(0, GetCoordY(P.Floor_Single));
        public static DoubleInt floor_FourWay = new DoubleInt(1, GetCoordY(P.Floor_Fourway));
        public static DoubleInt floor_Vertical_T = new DoubleInt(2, GetCoordY(P.Floor_Vertical_T));
        public static DoubleInt floor_Vertical_M = new DoubleInt(3, GetCoordY(P.Floor_Vertical_M));
        public static DoubleInt floor_Vertical_B = new DoubleInt(4, GetCoordY(P.Floor_Vertical_B));
        public static DoubleInt floor_Horizontal_L = new DoubleInt(5, GetCoordY(P.Floor_Horizontal_L));
        public static DoubleInt floor_Horizontal_M = new DoubleInt(6, GetCoordY(P.Floor_Horizontal_M));
        public static DoubleInt floor_Horizontal_R = new DoubleInt(7, GetCoordY(P.Floor_Horizontal_R));

        public static DoubleInt floor_Corner_TopRight = new DoubleInt(0, GetCoordY(P.Floor_Corner_TR));
        public static DoubleInt floor_Corner_TopLeft = new DoubleInt(1, GetCoordY(P.Floor_Corner_TL));
        public static DoubleInt floor_Corner_BottomRight = new DoubleInt(2, GetCoordY(P.Floor_Corner_BR));
        public static DoubleInt floor_Corner_BottomLeft = new DoubleInt(3, GetCoordY(P.Floor_Corner_BL));
        public static DoubleInt floor_Tee_Right = new DoubleInt(4, GetCoordY(P.Floor_Tee_R));
        public static DoubleInt floor_Tee_Left = new DoubleInt(5, GetCoordY(P.Floor_Tee_L));
        public static DoubleInt floor_Tee_Top = new DoubleInt(6, GetCoordY(P.Floor_Tee_T));
        public static DoubleInt floor_Tee_Bottom = new DoubleInt(7, GetCoordY(P.Floor_Tee_B));

        public static DoubleInt floor_Diagonal_TopRight = new DoubleInt(0, GetCoordY(P.Floor_Diagonal_TR));
        public static DoubleInt floor_Diagonal_TopLeft = new DoubleInt(1, GetCoordY(P.Floor_Diagonal_TL));
        public static DoubleInt floor_Diagonal_BottomRight = new DoubleInt(2, GetCoordY(P.Floor_Diagonal_BR));
        public static DoubleInt floor_Diagonal_BottomLeft = new DoubleInt(3, GetCoordY(P.Floor_Diagonal_BL));

        public static DoubleInt floorCornerHider_All = new DoubleInt(0, GetCoordY(P.FloorCornerHider_All));
        public static DoubleInt floorCornerHider_TL_BR = new DoubleInt(1, GetCoordY(P.FloorCornerHider_TL_BR));
        public static DoubleInt floorCornerHider_TR_BL = new DoubleInt(2, GetCoordY(P.FloorCornerHider_TR_BL));
        public static DoubleInt floorCornerHider_TL = new DoubleInt(3, GetCoordY(P.FloorCornerHider_TL));
        public static DoubleInt floorCornerHider_TL_TR = new DoubleInt(4, GetCoordY(P.FloorCornerHider_TL_TR));
        public static DoubleInt floorCornerHider_TL_TR_BR = new DoubleInt(5, GetCoordY(P.FloorCornerHider_TL_TR_BR));
        public static DoubleInt floorCornerHider_TR = new DoubleInt(6, GetCoordY(P.FloorCornerHider_TR));
        public static DoubleInt floorCornerHider_TR_BR = new DoubleInt(7, GetCoordY(P.FloorCornerHider_TR_BR));
        public static DoubleInt floorCornerHider_TR_BR_BL = new DoubleInt(8, GetCoordY(P.FloorCornerHider_TR_BR_BL));
        public static DoubleInt floorCornerHider_BR = new DoubleInt(9, GetCoordY(P.FloorCornerHider_BR));
        public static DoubleInt floorCornerHider_BR_BL = new DoubleInt(10, GetCoordY(P.FloorCornerHider_BR_BL));
        public static DoubleInt floorCornerHider_BR_BL_TL = new DoubleInt(11, GetCoordY(P.FloorCornerHider_BR_BL_TL));
        public static DoubleInt floorCornerHider_BL = new DoubleInt(12, GetCoordY(P.FloorCornerHider_BL));
        public static DoubleInt floorCornerHider_BL_TL = new DoubleInt(13, GetCoordY(P.FloorCornerHider_BL_TL));
        public static DoubleInt floorCornerHider_BL_TL_TR = new DoubleInt(14, GetCoordY(P.FloorCornerHider_BL_TL_TR));

        public static DoubleInt wall_Single = new DoubleInt(0, GetCoordY(P.Wall_Single));
        public static DoubleInt wall_FourWay = new DoubleInt(1, GetCoordY(P.Wall_Fourway));
        public static DoubleInt wall_Vertical_T = new DoubleInt(2, GetCoordY(P.Wall_Vertical_T));
        public static DoubleInt wall_Vertical_M = new DoubleInt(3, GetCoordY(P.Wall_Vertical_M));
        public static DoubleInt wall_Vertical_B = new DoubleInt(4, GetCoordY(P.Wall_Vertical_B));
        public static DoubleInt wall_Horizontal_L = new DoubleInt(5, GetCoordY(P.Wall_Horizontal_L));
        public static DoubleInt wall_Horizontal_M = new DoubleInt(6, GetCoordY(P.Wall_Horizontal_M));
        public static DoubleInt wall_Horizontal_R = new DoubleInt(7, GetCoordY(P.Wall_Horizontal_R));

        public static DoubleInt wall_Corner_TopRight = new DoubleInt(0, GetCoordY(P.Wall_Corner_TR));
        public static DoubleInt wall_Corner_TopLeft = new DoubleInt(1, GetCoordY(P.Wall_Corner_TL));
        public static DoubleInt wall_Corner_BottomRight = new DoubleInt(2, GetCoordY(P.Wall_Corner_BR));
        public static DoubleInt wall_Corner_BottomLeft = new DoubleInt(3, GetCoordY(P.Wall_Corner_BL));
        public static DoubleInt wall_Tee_Right = new DoubleInt(4, GetCoordY(P.Wall_Tee_R));
        public static DoubleInt wall_Tee_Left = new DoubleInt(5, GetCoordY(P.Wall_Tee_L));
        public static DoubleInt wall_Tee_Top = new DoubleInt(6, GetCoordY(P.Wall_Tee_T));
        public static DoubleInt wall_Tee_Bottom = new DoubleInt(7, GetCoordY(P.Wall_Tee_B));

        public static DoubleInt wall_Diagonal_TopRight = new DoubleInt(0, GetCoordY(P.Wall_Diagonal_TR));
        public static DoubleInt wall_Diagonal_TopLeft = new DoubleInt(1, GetCoordY(P.Wall_Diagonal_TL));
        public static DoubleInt wall_Diagonal_BottomRight = new DoubleInt(2, GetCoordY(P.Wall_Diagonal_BR));
        public static DoubleInt wall_Diagonal_BottomLeft = new DoubleInt(3, GetCoordY(P.Wall_Diagonal_BL));

        public static DoubleInt wallCornerHider_All = new DoubleInt(0, GetCoordY(P.WallCornerHider_All));
        public static DoubleInt wallCornerHider_TL_BR = new DoubleInt(1, GetCoordY(P.WallCornerHider_TL_BR));
        public static DoubleInt wallCornerHider_TR_BL = new DoubleInt(2, GetCoordY(P.WallCornerHider_TR_BL));
        public static DoubleInt wallCornerHider_TL = new DoubleInt(3, GetCoordY(P.WallCornerHider_TL));
        public static DoubleInt wallCornerHider_TL_TR = new DoubleInt(4, GetCoordY(P.WallCornerHider_TL_TR));
        public static DoubleInt wallCornerHider_TL_TR_BR = new DoubleInt(5, GetCoordY(P.WallCornerHider_TL_TR_BR));
        public static DoubleInt wallCornerHider_TR = new DoubleInt(6, GetCoordY(P.WallCornerHider_TR));
        public static DoubleInt wallCornerHider_TR_BR = new DoubleInt(7, GetCoordY(P.WallCornerHider_TR_BR));
        public static DoubleInt wallCornerHider_TR_BR_BL = new DoubleInt(8, GetCoordY(P.WallCornerHider_TR_BR_BL));
        public static DoubleInt wallCornerHider_BR = new DoubleInt(9, GetCoordY(P.WallCornerHider_BR));
        public static DoubleInt wallCornerHider_BR_BL = new DoubleInt(10, GetCoordY(P.WallCornerHider_BR_BL));
        public static DoubleInt wallCornerHider_BR_BL_TL = new DoubleInt(11, GetCoordY(P.WallCornerHider_BR_BL_TL));
        public static DoubleInt wallCornerHider_BL = new DoubleInt(12, GetCoordY(P.WallCornerHider_BL));
        public static DoubleInt wallCornerHider_BL_TL = new DoubleInt(13, GetCoordY(P.WallCornerHider_BL_TL));
        public static DoubleInt wallCornerHider_BL_TL_TR = new DoubleInt(14, GetCoordY(P.WallCornerHider_BL_TL_TR));

        public static TileAnimator.TileAnimation anim_DoorVertical_Open = new TileAnimator.TileAnimation(GetCoordY(P.DoorVertical_BOTTOM), GetCoordY(P.DoorVertical_BOTTOM), 4);
        public static TileAnimator.TileAnimation anim_DoorVertical_Close = new TileAnimator.TileAnimation(GetCoordY(P.DoorVertical_BOTTOM), GetCoordY(P.DoorVertical_BOTTOM), 4).Reverse();

        public static TileAnimator.TileAnimation anim_DoorHorizontal_Open = new TileAnimator.TileAnimation(GetCoordY(P.DoorHorizontal_BOTTOM), GetCoordY(P.DoorHorizontal_TOP), 4);
        public static TileAnimator.TileAnimation anim_DoorHorizontal_Close = new TileAnimator.TileAnimation(GetCoordY(P.DoorHorizontal_BOTTOM), GetCoordY(P.DoorHorizontal_TOP), 4).Reverse();

        public static TileAnimator.TileAnimation anim_AirlockHorizontal_OpenBottom = new TileAnimator.TileAnimation(GetCoordY(P.AirlockHorizontal_OpenBottom_BOTTOM), GetCoordY(P.AirlockHorizontal_OpenBottom_TOP), 4);
        public static TileAnimator.TileAnimation anim_AirlockHorizontal_CloseBottom = new TileAnimator.TileAnimation(GetCoordY(P.AirlockHorizontal_OpenBottom_BOTTOM), GetCoordY(P.AirlockHorizontal_OpenBottom_TOP), 4).Reverse();

        public static TileAnimator.TileAnimation anim_AirlockHorizontal_OpenTop = new TileAnimator.TileAnimation(GetCoordY(P.AirlockHorizontal_OpenBottom_BOTTOM), GetCoordY(P.AirlockHorizontal_OpenTop), 4, bottomForceFrameX: 0);
        public static TileAnimator.TileAnimation anim_AirlockHorizontal_CloseTop = new TileAnimator.TileAnimation(GetCoordY(P.AirlockHorizontal_OpenBottom_BOTTOM), GetCoordY(P.AirlockHorizontal_OpenTop), 4, bottomForceFrameX: 0).Reverse();

        public static TileAnimator.TileAnimation anim_AirlockHorizontal_Wait = new TileAnimator.TileAnimation(GetCoordY(P.AirlockHorizontal_OpenBottom_BOTTOM), GetCoordY(P.AirlockHorizontal_Wait), 8, bottomForceFrameX: 0);

        public static TileAnimator.TileAnimation anim_AirlockVertical_OpenLeft = new TileAnimator.TileAnimation(GetCoordY(P.AirlockVertical_OpenLeft_BOTTOM), GetCoordY(P.AirlockVertical_OpenLeft_TOP), 4);
        public static TileAnimator.TileAnimation anim_AirlockVertical_CloseLeft = new TileAnimator.TileAnimation(GetCoordY(P.AirlockVertical_OpenLeft_BOTTOM), GetCoordY(P.AirlockVertical_OpenLeft_TOP), 4).Reverse();

        public static TileAnimator.TileAnimation anim_AirlockVertical_OpenRight = new TileAnimator.TileAnimation(GetCoordY(P.AirlockVertical_OpenRight_BOTTOM), GetCoordY(P.AirlockVertical_OpenRight_TOP), 4);
        public static TileAnimator.TileAnimation anim_AirlockVertical_CloseRight = new TileAnimator.TileAnimation(GetCoordY(P.AirlockVertical_OpenRight_BOTTOM), GetCoordY(P.AirlockVertical_OpenRight_TOP), 4).Reverse();

        public static TileAnimator.TileAnimation anim_AirlockVertical_Wait = new TileAnimator.TileAnimation(GetCoordY(P.AirlockVertical_OpenLeft_BOTTOM), GetCoordY(P.AirlockVertical_Wait), 8, bottomForceFrameX: 0);
    }

    public GameObject TilePrefab;

    [Header("Character Assets")]
    public OrientedAsset[] HairStyles;
    public OrientedAsset[] Heads;
    public OrientedAsset[] Eyes;
    public OrientedAsset[] Beards;


    void Awake() {
        Instance = this;
    }

    public DoubleInt GetWallAssetForTile(Tile.Type _tileType, Tile.TileOrientation _tileOrientation, int _styleIndex, bool _isBottom, bool _hasConnection_Left, bool _hasConnection_Top, bool _hasConnection_Right, bool _hasConnection_Bottom) {
        switch (_tileType) {
            case Tile.Type.Empty:
                return null;
            case Tile.Type.Solid:
                if (!_isBottom) // for now at least
                    return null;

                if (_hasConnection_Left) {
                    if (_hasConnection_Top) {
                        if (_hasConnection_Right) {
                            if (_hasConnection_Bottom) return WallSet.wall_FourWay;
                            else return WallSet.wall_Tee_Top;
                        }
                        else if (_hasConnection_Bottom) return WallSet.wall_Tee_Left;
                        else return WallSet.wall_Corner_TopLeft;
                    }
                    else if (_hasConnection_Right) {
                        if (_hasConnection_Bottom) return WallSet.wall_Tee_Bottom;
                        else return WallSet.wall_Horizontal_M;
                    }
                    else if (_hasConnection_Bottom) return WallSet.wall_Corner_BottomLeft;
                    else return WallSet.wall_Horizontal_R;
                }
                else if (_hasConnection_Top) {
                    if (_hasConnection_Right) {
                        if (_hasConnection_Bottom) return WallSet.wall_Tee_Right;
                        else return WallSet.wall_Corner_TopRight;
                    }
                    else if (_hasConnection_Bottom) return WallSet.wall_Vertical_M;
                    else return WallSet.wall_Vertical_B;
                }
                else if (_hasConnection_Right) {
                    if (_hasConnection_Bottom) return WallSet.wall_Corner_BottomRight;
                    else return WallSet.wall_Horizontal_L;
                }
                else if (_hasConnection_Bottom) return WallSet.wall_Vertical_T;
                else return WallSet.wall_Single;

            case Tile.Type.Diagonal:
                switch (_tileOrientation) {
                    case Tile.TileOrientation.BottomLeft:
                        return (_isBottom ? null : WallSet.wall_Diagonal_BottomLeft);
                    case Tile.TileOrientation.BottomRight:
                        return (_isBottom ? null : WallSet.wall_Diagonal_BottomRight);
                    case Tile.TileOrientation.TopLeft:
                        return (_isBottom ? WallSet.wall_Diagonal_TopLeft : null);
                    case Tile.TileOrientation.TopRight:
                        return (_isBottom ? WallSet.wall_Diagonal_TopRight : null);
					default:
						throw new System.Exception (_tileOrientation + " is not supported by diagonals!");
                }
            case Tile.Type.Door:
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
            case Tile.Type.Airlock:
                switch (_tileOrientation) {
                    case Tile.TileOrientation.None:
                    case Tile.TileOrientation.Bottom:
                    case Tile.TileOrientation.Top:
                        return _isBottom ? WallSet.anim_AirlockVertical_OpenLeft.GetBottomFirstFrame() : WallSet.anim_AirlockVertical_OpenLeft.GetTopFirstFrame();
                    case Tile.TileOrientation.Left:
                    case Tile.TileOrientation.Right:
                        return _isBottom ? WallSet.anim_AirlockHorizontal_OpenTop.GetBottomFirstFrame() : WallSet.anim_AirlockHorizontal_OpenTop.GetTopFirstFrame();
                }
                break;
            default:
                throw new System.NotImplementedException(_tileType + " hasn't been properly implemented yet!");
        }

        return null;
    }

	public DoubleInt GetFloorAssetForTile(Tile.Type _tileType, Tile.TileOrientation _tileOrientation, int _styleIndex, bool _hasConnection_Left, bool _hasConnection_Top, bool _hasConnection_Right, bool _hasConnection_Bottom) {
		switch (_tileType) {
			case Tile.Type.Empty:
				return null;
			case Tile.Type.Solid:
				if (_hasConnection_Left) {
					if (_hasConnection_Top) {
						if (_hasConnection_Right) {
							if (_hasConnection_Bottom) return WallSet.floor_FourWay;
							else return WallSet.floor_Tee_Top;
						}
						else if (_hasConnection_Bottom) return WallSet.floor_Tee_Left;
						else return WallSet.floor_Corner_TopLeft;
					}
					else if (_hasConnection_Right) {
						if (_hasConnection_Bottom) return WallSet.floor_Tee_Bottom;
						else return WallSet.floor_Horizontal_M;
					}
					else if (_hasConnection_Bottom) return WallSet.floor_Corner_BottomLeft;
					else return WallSet.floor_Horizontal_R;
				}
				else if (_hasConnection_Top) {
					if (_hasConnection_Right) {
						if (_hasConnection_Bottom) return WallSet.floor_Tee_Right;
						else return WallSet.floor_Corner_TopRight;
					}
					else if (_hasConnection_Bottom) return WallSet.floor_Vertical_M;
					else return WallSet.floor_Vertical_B;
				}
				else if (_hasConnection_Right) {
					if (_hasConnection_Bottom) return WallSet.floor_Corner_BottomRight;
					else return WallSet.floor_Horizontal_L;
				}
				else if (_hasConnection_Bottom) return WallSet.floor_Vertical_T;
				else return WallSet.floor_Single;

			case Tile.Type.Diagonal:
				switch (_tileOrientation) {
					case Tile.TileOrientation.BottomLeft:
						return (WallSet.floor_Diagonal_BottomLeft);
					case Tile.TileOrientation.BottomRight:
						return (WallSet.floor_Diagonal_BottomRight);
					case Tile.TileOrientation.TopLeft:
						return (WallSet.floor_Diagonal_TopLeft);
					case Tile.TileOrientation.TopRight:
						return (WallSet.floor_Diagonal_TopRight);
				}
				break;
			case Tile.Type.Door:
			case Tile.Type.Airlock:
				throw new System.Exception (_tileType.ToString() + " does not apply to Floor!");
			default:
				throw new System.NotImplementedException(_tileType + " hasn't been properly implemented yet!");
		}

		return null;
	}

    public DoubleInt GetWallCornerAsset(bool _TL, bool _TR, bool _BR, bool _BL) {
        if (_TL) {
            if (_TR) {
                if (_BR) {
                    if (_BL)
                        return WallSet.wallCornerHider_All;

                    return WallSet.wallCornerHider_TL_TR_BR;
                }
                else if (_BL)
                    return WallSet.wallCornerHider_BL_TL_TR;

                return WallSet.wallCornerHider_TL_TR;
            }
            else if (_BR) {
                if (_BL)
                    return WallSet.wallCornerHider_BR_BL_TL;

                return WallSet.wallCornerHider_TL_BR;
            }
            else if (_BL)
                return WallSet.wallCornerHider_BL_TL;

            return WallSet.wallCornerHider_TL;
        }
        else if (_TR) {
            if (_BR) {
                if (_BL)
                    return WallSet.wallCornerHider_TR_BR_BL;

                return WallSet.wallCornerHider_TR_BR;
            }
            else if (_BL)
                return WallSet.wallCornerHider_TR_BL;

            return WallSet.wallCornerHider_TR;
        }
        else if (_BR) {
            if (_BL)
                return WallSet.wallCornerHider_BR_BL;

            return WallSet.wallCornerHider_BR;
        }
        else if (_BL)
            return WallSet.wallCornerHider_BL;

        return null;
    }
    public DoubleInt GetFloorCornerAsset(bool _TL, bool _TR, bool _BR, bool _BL) {
        if (_TL) {
            if (_TR) {
                if (_BR) {
                    if (_BL)
                        return WallSet.floorCornerHider_All;

                    return WallSet.floorCornerHider_TL_TR_BR;
                }
                else if (_BL)
                    return WallSet.floorCornerHider_BL_TL_TR;

                return WallSet.floorCornerHider_TL_TR;
            }
            else if (_BR) {
                if (_BL)
                    return WallSet.floorCornerHider_BR_BL_TL;

                return WallSet.floorCornerHider_TL_BR;
            }
            else if (_BL)
                return WallSet.floorCornerHider_BL_TL;

            return WallSet.floorCornerHider_TL;
        }
        else if (_TR) {
            if (_BR) {
                if (_BL)
                    return WallSet.floorCornerHider_TR_BR_BL;

                return WallSet.floorCornerHider_TR_BR;
            }
            else if (_BL)
                return WallSet.floorCornerHider_TR_BL;

            return WallSet.floorCornerHider_TR;
        }
        else if (_BR) {
            if (_BL)
                return WallSet.floorCornerHider_BR_BL;

            return WallSet.floorCornerHider_BR;
        }
        else if(_BL)
            return WallSet.floorCornerHider_BL;

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
