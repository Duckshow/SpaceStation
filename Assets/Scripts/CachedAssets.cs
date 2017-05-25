using UnityEngine;
using System;
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
        public DoubleInt(int _x = 0, int _y = 0) {
            X = _x;
            Y = _y;
        }
    }
    [System.Serializable]
    public class WallSet {

        public const float TEXTURE_SIZE_X = 1024;
        public const float TEXTURE_SIZE_Y = 2176;

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
            Floor_Diagonal_TR_R,
            Floor_Diagonal_TR_T,
            Floor_Diagonal_TR_TR,
            Floor_Diagonal_TL,
            Floor_Diagonal_TL_L,
            Floor_Diagonal_TL_T,
            Floor_Diagonal_TL_TL,
            Floor_Diagonal_BR,
            Floor_Diagonal_BR_R,
            Floor_Diagonal_BR_B,
            Floor_Diagonal_BR_BR,
            Floor_Diagonal_BL,
            Floor_Diagonal_BL_L,
            Floor_Diagonal_BL_B,
            Floor_Diagonal_BL_BL,
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
            Wall_Diagonal_TR_R,
            Wall_Diagonal_TR_T,
            Wall_Diagonal_TR_TR,
            Wall_Diagonal_TL,
            Wall_Diagonal_TL_L,
            Wall_Diagonal_TL_T,
            Wall_Diagonal_TL_TL,
            Wall_Diagonal_BR,
            Wall_Diagonal_BR_R,
            Wall_Diagonal_BR_B,
            Wall_Diagonal_BR_BR,
            Wall_Diagonal_BL,
            Wall_Diagonal_BL_L,
            Wall_Diagonal_BL_B,
            Wall_Diagonal_BL_BL,
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
            DoorVertical,
            DoorHorizontal,
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
            new List<P>() {
                P.Floor_Single,
                P.Floor_Fourway,
                P.Floor_Vertical_T,
                P.Floor_Vertical_M,
                P.Floor_Vertical_B,
                P.Floor_Horizontal_L,
                P.Floor_Horizontal_M,
                P.Floor_Horizontal_R,
                P.Floor_Corner_TR,
                P.Floor_Corner_TL,
                P.Floor_Corner_BR,
                P.Floor_Corner_BL,
                P.Floor_Tee_R,
                P.Floor_Tee_L,
                P.Floor_Tee_T,
                P.Floor_Tee_B
            },
            new List<P>() {
                P.Floor_Diagonal_TR,
                P.Floor_Diagonal_TR_R,
                P.Floor_Diagonal_TR_T,
                P.Floor_Diagonal_TR_TR,
                P.Floor_Diagonal_TL,
                P.Floor_Diagonal_TL_L,
                P.Floor_Diagonal_TL_T,
                P.Floor_Diagonal_TL_TL,
                P.Floor_Diagonal_BR,
                P.Floor_Diagonal_BR_R,
                P.Floor_Diagonal_BR_B,
                P.Floor_Diagonal_BR_BR,
                P.Floor_Diagonal_BL,
                P.Floor_Diagonal_BL_L,
                P.Floor_Diagonal_BL_B,
                P.Floor_Diagonal_BL_BL
            },
            new List<P>() {
                P.FloorCornerHider_All,
                P.FloorCornerHider_TL_BR,
                P.FloorCornerHider_TR_BL,
                P.FloorCornerHider_TL,
                P.FloorCornerHider_TL_TR,
                P.FloorCornerHider_TL_TR_BR,
                P.FloorCornerHider_TR,
                P.FloorCornerHider_TR_BR,
                P.FloorCornerHider_TR_BR_BL,
                P.FloorCornerHider_BR,
                P.FloorCornerHider_BR_BL,
                P.FloorCornerHider_BR_BL_TL,
                P.FloorCornerHider_BL,
                P.FloorCornerHider_BL_TL,
                P.FloorCornerHider_BL_TL_TR
            },
            new List<P>() {
                P.Wall_Single,
                P.Wall_Fourway,
                P.Wall_Vertical_T,
                P.Wall_Vertical_M,
                P.Wall_Vertical_B,
                P.Wall_Horizontal_L,
                P.Wall_Horizontal_M,
                P.Wall_Horizontal_R,
                P.Wall_Corner_TR,
                P.Wall_Corner_TL,
                P.Wall_Corner_BR,
                P.Wall_Corner_BL,
                P.Wall_Tee_R,
                P.Wall_Tee_L,
                P.Wall_Tee_T,
                P.Wall_Tee_B
            },
            new List<P>() {
                P.Wall_Diagonal_TR,
                P.Wall_Diagonal_TR_R,
                P.Wall_Diagonal_TR_T,
                P.Wall_Diagonal_TR_TR,
                P.Wall_Diagonal_TL,
                P.Wall_Diagonal_TL_L,
                P.Wall_Diagonal_TL_T,
                P.Wall_Diagonal_TL_TL,
                P.Wall_Diagonal_BR,
                P.Wall_Diagonal_BR_R,
                P.Wall_Diagonal_BR_B,
                P.Wall_Diagonal_BR_BR,
                P.Wall_Diagonal_BL,
                P.Wall_Diagonal_BL_L,
                P.Wall_Diagonal_BL_B,
                P.Wall_Diagonal_BL_BL
            },
            new List<P>() {
                P.WallCornerHider_All,
                P.WallCornerHider_TL_BR,
                P.WallCornerHider_TR_BL,
                P.WallCornerHider_TL,
                P.WallCornerHider_TL_TR,
                P.WallCornerHider_TL_TR_BR,
                P.WallCornerHider_TR,
                P.WallCornerHider_TR_BR,
                P.WallCornerHider_TR_BR_BL,
                P.WallCornerHider_BR,
                P.WallCornerHider_BR_BL,
                P.WallCornerHider_BR_BL_TL,
                P.WallCornerHider_BL,
                P.WallCornerHider_BL_TL,
                P.WallCornerHider_BL_TL_TR
            },
            new List<P>() {
                P.DoorVertical
            },
            new List<P>() {
                P.DoorHorizontal
            },
            new List<P>() {
                P.AirlockHorizontal_OpenBottom_BOTTOM
            },
            new List<P>() {
                P.AirlockHorizontal_OpenBottom_TOP
            },
            new List<P>() {
                P.AirlockHorizontal_OpenTop
            },
            new List<P>() {
                P.AirlockHorizontal_Wait
            },
            new List<P>() {
                P.AirlockVertical_OpenLeft_BOTTOM
            },
            new List<P>() {
                P.AirlockVertical_OpenLeft_TOP
            },
            new List<P>() {
                P.AirlockVertical_OpenRight_BOTTOM
            },
            new List<P>() {
                P.AirlockVertical_OpenRight_TOP
            },
            new List<P>() {
                P.AirlockVertical_Wait
            }
        };
        private static DoubleInt GetTextureCoord(P id) {
            DoubleInt _di = new DoubleInt();
            int _index = AllAssetPurposes.FindIndex(x => x.Contains(id));
            _di.X = AllAssetPurposes[_index].FindIndex(x => x == id);
            _di.Y = _index * 2; // 2 because of the height of individual tile-assets
            return _di;
        }

        public static DoubleInt floor_Single = GetTextureCoord(P.Floor_Single);
        public static DoubleInt floor_FourWay = GetTextureCoord(P.Floor_Fourway);
        public static DoubleInt floor_Vertical_T = GetTextureCoord(P.Floor_Vertical_T);
        public static DoubleInt floor_Vertical_M = GetTextureCoord(P.Floor_Vertical_M);
        public static DoubleInt floor_Vertical_B = GetTextureCoord(P.Floor_Vertical_B);
        public static DoubleInt floor_Horizontal_L = GetTextureCoord(P.Floor_Horizontal_L);
        public static DoubleInt floor_Horizontal_M = GetTextureCoord(P.Floor_Horizontal_M);
        public static DoubleInt floor_Horizontal_R = GetTextureCoord(P.Floor_Horizontal_R);
        public static DoubleInt floor_Corner_TopRight = GetTextureCoord(P.Floor_Corner_TR);
        public static DoubleInt floor_Corner_TopLeft = GetTextureCoord(P.Floor_Corner_TL);
        public static DoubleInt floor_Corner_BottomRight = GetTextureCoord(P.Floor_Corner_BR);
        public static DoubleInt floor_Corner_BottomLeft = GetTextureCoord(P.Floor_Corner_BL);
        public static DoubleInt floor_Tee_Right = GetTextureCoord(P.Floor_Tee_R);
        public static DoubleInt floor_Tee_Left = GetTextureCoord(P.Floor_Tee_L);
        public static DoubleInt floor_Tee_Top = GetTextureCoord(P.Floor_Tee_T);
        public static DoubleInt floor_Tee_Bottom = GetTextureCoord(P.Floor_Tee_B);

        public static DoubleInt floor_Diagonal_TopRight = GetTextureCoord(P.Floor_Diagonal_TR);
        public static DoubleInt floor_Diagonal_TopRight_T = GetTextureCoord(P.Floor_Diagonal_TR_T);
        public static DoubleInt floor_Diagonal_TopRight_R = GetTextureCoord(P.Floor_Diagonal_TR_R);
        public static DoubleInt floor_Diagonal_TopRight_TR = GetTextureCoord(P.Floor_Diagonal_TR_TR);
        public static DoubleInt floor_Diagonal_TopLeft = GetTextureCoord(P.Floor_Diagonal_TL);
        public static DoubleInt floor_Diagonal_TopLeft_T = GetTextureCoord(P.Floor_Diagonal_TL_T);
        public static DoubleInt floor_Diagonal_TopLeft_L = GetTextureCoord(P.Floor_Diagonal_TL_L);
        public static DoubleInt floor_Diagonal_TopLeft_TL = GetTextureCoord(P.Floor_Diagonal_TL_TL);
        public static DoubleInt floor_Diagonal_BottomRight = GetTextureCoord(P.Floor_Diagonal_BR);
        public static DoubleInt floor_Diagonal_BottomRight_B = GetTextureCoord(P.Floor_Diagonal_BR_B);
        public static DoubleInt floor_Diagonal_BottomRight_R = GetTextureCoord(P.Floor_Diagonal_BR_R);
        public static DoubleInt floor_Diagonal_BottomRight_BR = GetTextureCoord(P.Floor_Diagonal_BR_BR);
        public static DoubleInt floor_Diagonal_BottomLeft = GetTextureCoord(P.Floor_Diagonal_BL);
        public static DoubleInt floor_Diagonal_BottomLeft_B = GetTextureCoord(P.Floor_Diagonal_BL_B);
        public static DoubleInt floor_Diagonal_BottomLeft_L = GetTextureCoord(P.Floor_Diagonal_BL_L);
        public static DoubleInt floor_Diagonal_BottomLeft_BL = GetTextureCoord(P.Floor_Diagonal_BL_BL);

        public static DoubleInt floorCornerHider_All = GetTextureCoord(P.FloorCornerHider_All);
        public static DoubleInt floorCornerHider_TL_BR = GetTextureCoord(P.FloorCornerHider_TL_BR);
        public static DoubleInt floorCornerHider_TR_BL = GetTextureCoord(P.FloorCornerHider_TR_BL);
        public static DoubleInt floorCornerHider_TL = GetTextureCoord(P.FloorCornerHider_TL);
        public static DoubleInt floorCornerHider_TL_TR = GetTextureCoord(P.FloorCornerHider_TL_TR);
        public static DoubleInt floorCornerHider_TL_TR_BR = GetTextureCoord(P.FloorCornerHider_TL_TR_BR);
        public static DoubleInt floorCornerHider_TR = GetTextureCoord(P.FloorCornerHider_TR);
        public static DoubleInt floorCornerHider_TR_BR = GetTextureCoord(P.FloorCornerHider_TR_BR);
        public static DoubleInt floorCornerHider_TR_BR_BL = GetTextureCoord(P.FloorCornerHider_TR_BR_BL);
        public static DoubleInt floorCornerHider_BR = GetTextureCoord(P.FloorCornerHider_BR);
        public static DoubleInt floorCornerHider_BR_BL = GetTextureCoord(P.FloorCornerHider_BR_BL);
        public static DoubleInt floorCornerHider_BR_BL_TL = GetTextureCoord(P.FloorCornerHider_BR_BL_TL);
        public static DoubleInt floorCornerHider_BL = GetTextureCoord(P.FloorCornerHider_BL);
        public static DoubleInt floorCornerHider_BL_TL = GetTextureCoord(P.FloorCornerHider_BL_TL);
        public static DoubleInt floorCornerHider_BL_TL_TR = GetTextureCoord(P.FloorCornerHider_BL_TL_TR);

        public static DoubleInt wall_Single = GetTextureCoord(P.Wall_Single);
        public static DoubleInt wall_FourWay = GetTextureCoord(P.Wall_Fourway);
        public static DoubleInt wall_Vertical_T = GetTextureCoord(P.Wall_Vertical_T);
        public static DoubleInt wall_Vertical_M = GetTextureCoord(P.Wall_Vertical_M);
        public static DoubleInt wall_Vertical_B = GetTextureCoord(P.Wall_Vertical_B);
        public static DoubleInt wall_Horizontal_L = GetTextureCoord(P.Wall_Horizontal_L);
        public static DoubleInt wall_Horizontal_M = GetTextureCoord(P.Wall_Horizontal_M);
        public static DoubleInt wall_Horizontal_R = GetTextureCoord(P.Wall_Horizontal_R);

        public static DoubleInt wall_Corner_TopRight = GetTextureCoord(P.Wall_Corner_TR);
        public static DoubleInt wall_Corner_TopLeft = GetTextureCoord(P.Wall_Corner_TL);
        public static DoubleInt wall_Corner_BottomRight = GetTextureCoord(P.Wall_Corner_BR);
        public static DoubleInt wall_Corner_BottomLeft = GetTextureCoord(P.Wall_Corner_BL);
        public static DoubleInt wall_Tee_Right = GetTextureCoord(P.Wall_Tee_R);
        public static DoubleInt wall_Tee_Left = GetTextureCoord(P.Wall_Tee_L);
        public static DoubleInt wall_Tee_Top = GetTextureCoord(P.Wall_Tee_T);
        public static DoubleInt wall_Tee_Bottom = GetTextureCoord(P.Wall_Tee_B);

        public static DoubleInt wall_Diagonal_TopRight = GetTextureCoord(P.Wall_Diagonal_TR);
        public static DoubleInt wall_Diagonal_TopRight_T = GetTextureCoord(P.Wall_Diagonal_TR_T);
        public static DoubleInt wall_Diagonal_TopRight_R = GetTextureCoord(P.Wall_Diagonal_TR_R);
        public static DoubleInt wall_Diagonal_TopRight_TR = GetTextureCoord(P.Wall_Diagonal_TR_TR);
        public static DoubleInt wall_Diagonal_TopLeft = GetTextureCoord(P.Wall_Diagonal_TL);
        public static DoubleInt wall_Diagonal_TopLeft_T = GetTextureCoord(P.Wall_Diagonal_TL_T);
        public static DoubleInt wall_Diagonal_TopLeft_L = GetTextureCoord(P.Wall_Diagonal_TL_L);
        public static DoubleInt wall_Diagonal_TopLeft_TL = GetTextureCoord(P.Wall_Diagonal_TL_TL);
        public static DoubleInt wall_Diagonal_BottomRight = GetTextureCoord(P.Wall_Diagonal_BR);
        public static DoubleInt wall_Diagonal_BottomRight_B = GetTextureCoord(P.Wall_Diagonal_BR_B);
        public static DoubleInt wall_Diagonal_BottomRight_R = GetTextureCoord(P.Wall_Diagonal_BR_R);
        public static DoubleInt wall_Diagonal_BottomRight_BR = GetTextureCoord(P.Wall_Diagonal_BR_BR);
        public static DoubleInt wall_Diagonal_BottomLeft = GetTextureCoord(P.Wall_Diagonal_BL);
        public static DoubleInt wall_Diagonal_BottomLeft_B = GetTextureCoord(P.Wall_Diagonal_BL_B);
        public static DoubleInt wall_Diagonal_BottomLeft_L = GetTextureCoord(P.Wall_Diagonal_BL_L);
        public static DoubleInt wall_Diagonal_BottomLeft_BL = GetTextureCoord(P.Wall_Diagonal_BL_BL);

        public static DoubleInt wallCornerHider_All = GetTextureCoord(P.WallCornerHider_All);
        public static DoubleInt wallCornerHider_TL_BR = GetTextureCoord(P.WallCornerHider_TL_BR);
        public static DoubleInt wallCornerHider_TR_BL = GetTextureCoord(P.WallCornerHider_TR_BL);
        public static DoubleInt wallCornerHider_TL = GetTextureCoord(P.WallCornerHider_TL);
        public static DoubleInt wallCornerHider_TL_TR = GetTextureCoord(P.WallCornerHider_TL_TR);
        public static DoubleInt wallCornerHider_TL_TR_BR = GetTextureCoord(P.WallCornerHider_TL_TR_BR);
        public static DoubleInt wallCornerHider_TR = GetTextureCoord(P.WallCornerHider_TR);
        public static DoubleInt wallCornerHider_TR_BR = GetTextureCoord(P.WallCornerHider_TR_BR);
        public static DoubleInt wallCornerHider_TR_BR_BL = GetTextureCoord(P.WallCornerHider_TR_BR_BL);
        public static DoubleInt wallCornerHider_BR = GetTextureCoord(P.WallCornerHider_BR);
        public static DoubleInt wallCornerHider_BR_BL = GetTextureCoord(P.WallCornerHider_BR_BL);
        public static DoubleInt wallCornerHider_BR_BL_TL = GetTextureCoord(P.WallCornerHider_BR_BL_TL);
        public static DoubleInt wallCornerHider_BL = GetTextureCoord(P.WallCornerHider_BL);
        public static DoubleInt wallCornerHider_BL_TL = GetTextureCoord(P.WallCornerHider_BL_TL);
        public static DoubleInt wallCornerHider_BL_TL_TR = GetTextureCoord(P.WallCornerHider_BL_TL_TR);

        public static TileAnimator.TileAnimation anim_DoorVertical_Open = new TileAnimator.TileAnimation(GetTextureCoord(P.DoorVertical).Y, -1, 4);
        public static TileAnimator.TileAnimation anim_DoorVertical_Close = new TileAnimator.TileAnimation(GetTextureCoord(P.DoorVertical).Y, -1, 4).Reverse();
        public static TileAnimator.TileAnimation anim_DoorHorizontal_Open = new TileAnimator.TileAnimation(GetTextureCoord(P.DoorHorizontal).Y, -1, 4);
        public static TileAnimator.TileAnimation anim_DoorHorizontal_Close = new TileAnimator.TileAnimation(GetTextureCoord(P.DoorHorizontal).Y, -1, 4).Reverse();
        public static TileAnimator.TileAnimation anim_AirlockHorizontal_OpenBottom = new TileAnimator.TileAnimation(GetTextureCoord(P.AirlockHorizontal_OpenBottom_BOTTOM).Y, GetTextureCoord(P.AirlockHorizontal_OpenBottom_TOP).Y, 4);
        public static TileAnimator.TileAnimation anim_AirlockHorizontal_CloseBottom = new TileAnimator.TileAnimation(GetTextureCoord(P.AirlockHorizontal_OpenBottom_BOTTOM).Y, GetTextureCoord(P.AirlockHorizontal_OpenBottom_TOP).Y, 4).Reverse();
        public static TileAnimator.TileAnimation anim_AirlockHorizontal_OpenTop = new TileAnimator.TileAnimation(GetTextureCoord(P.AirlockHorizontal_OpenBottom_BOTTOM).Y, GetTextureCoord(P.AirlockHorizontal_OpenTop).Y, 4, bottomForceFrameX: 0);
        public static TileAnimator.TileAnimation anim_AirlockHorizontal_CloseTop = new TileAnimator.TileAnimation(GetTextureCoord(P.AirlockHorizontal_OpenBottom_BOTTOM).Y, GetTextureCoord(P.AirlockHorizontal_OpenTop).Y, 4, bottomForceFrameX: 0).Reverse();
        public static TileAnimator.TileAnimation anim_AirlockHorizontal_Wait = new TileAnimator.TileAnimation(GetTextureCoord(P.AirlockHorizontal_OpenBottom_BOTTOM).Y, GetTextureCoord(P.AirlockHorizontal_Wait).Y, 8, bottomForceFrameX: 0);
        public static TileAnimator.TileAnimation anim_AirlockVertical_OpenLeft = new TileAnimator.TileAnimation(GetTextureCoord(P.AirlockVertical_OpenLeft_BOTTOM).Y, GetTextureCoord(P.AirlockVertical_OpenLeft_TOP).Y, 4);
        public static TileAnimator.TileAnimation anim_AirlockVertical_CloseLeft = new TileAnimator.TileAnimation(GetTextureCoord(P.AirlockVertical_OpenLeft_BOTTOM).Y, GetTextureCoord(P.AirlockVertical_OpenLeft_TOP).Y, 4).Reverse();
        public static TileAnimator.TileAnimation anim_AirlockVertical_OpenRight = new TileAnimator.TileAnimation(GetTextureCoord(P.AirlockVertical_OpenRight_BOTTOM).Y, GetTextureCoord(P.AirlockVertical_OpenRight_TOP).Y, 4, bottomForceFirstFrame: GetTextureCoord(P.AirlockVertical_OpenLeft_BOTTOM), topForceFirstFrame: GetTextureCoord(P.AirlockVertical_OpenLeft_TOP));
        public static TileAnimator.TileAnimation anim_AirlockVertical_CloseRight = new TileAnimator.TileAnimation(GetTextureCoord(P.AirlockVertical_OpenRight_BOTTOM).Y, GetTextureCoord(P.AirlockVertical_OpenRight_TOP).Y, 4, bottomForceFirstFrame: GetTextureCoord(P.AirlockVertical_OpenLeft_BOTTOM), topForceFirstFrame: GetTextureCoord(P.AirlockVertical_OpenLeft_TOP)).Reverse();
        public static TileAnimator.TileAnimation anim_AirlockVertical_Wait = new TileAnimator.TileAnimation(GetTextureCoord(P.AirlockVertical_OpenLeft_BOTTOM).Y, GetTextureCoord(P.AirlockVertical_Wait).Y, 8, bottomForceFrameX: 0);

        public Vector2[][] floor_Single_shadow;
        public Vector2[][] floor_FourWay_shadow;
        public Vector2[][] floor_Vertical_T_shadow;
        public Vector2[][] floor_Vertical_M_shadow;
        public Vector2[][] floor_Vertical_B_shadow;
        public Vector2[][] floor_Horizontal_L_shadow;
        public Vector2[][] floor_Horizontal_M_shadow;
        public Vector2[][] floor_Horizontal_R_shadow;
        public Vector2[][] floor_Corner_TopRight_shadow;
        public Vector2[][] floor_Corner_TopLeft_shadow;
        public Vector2[][] floor_Corner_BottomRight_shadow;
        public Vector2[][] floor_Corner_BottomLeft_shadow;
        public Vector2[][] floor_Tee_Right_shadow;
        public Vector2[][] floor_Tee_Left_shadow;
        public Vector2[][] floor_Tee_Top_shadow;
        public Vector2[][] floor_Tee_Bottom_shadow;

        public Vector2[][] floor_Diagonal_TopRight_shadow;
        public Vector2[][] floor_Diagonal_TopRight_T_shadow;
        public Vector2[][] floor_Diagonal_TopRight_R_shadow;
        public Vector2[][] floor_Diagonal_TopRight_TR_shadow;
        public Vector2[][] floor_Diagonal_TopLeft_shadow;
        public Vector2[][] floor_Diagonal_TopLeft_T_shadow;
        public Vector2[][] floor_Diagonal_TopLeft_L_shadow;
        public Vector2[][] floor_Diagonal_TopLeft_TL_shadow;
        public Vector2[][] floor_Diagonal_BottomRight_shadow;
        public Vector2[][] floor_Diagonal_BottomRight_B_shadow;
        public Vector2[][] floor_Diagonal_BottomRight_R_shadow;
        public Vector2[][] floor_Diagonal_BottomRight_BR_shadow;
        public Vector2[][] floor_Diagonal_BottomLeft_shadow;
        public Vector2[][] floor_Diagonal_BottomLeft_B_shadow;
        public Vector2[][] floor_Diagonal_BottomLeft_L_shadow;
        public Vector2[][] floor_Diagonal_BottomLeft_BL_shadow;

        public Vector2[][] floorCornerHider_All_shadow;
        public Vector2[][] floorCornerHider_TL_BR_shadow;
        public Vector2[][] floorCornerHider_TR_BL_shadow;
        public Vector2[][] floorCornerHider_TL_shadow;
        public Vector2[][] floorCornerHider_TL_TR_shadow;
        public Vector2[][] floorCornerHider_TL_TR_BR_shadow;
        public Vector2[][] floorCornerHider_TR_shadow;
        public Vector2[][] floorCornerHider_TR_BR_shadow;
        public Vector2[][] floorCornerHider_TR_BR_BL_shadow;
        public Vector2[][] floorCornerHider_BR_shadow;
        public Vector2[][] floorCornerHider_BR_BL_shadow;
        public Vector2[][] floorCornerHider_BR_BL_TL_shadow;
        public Vector2[][] floorCornerHider_BL_shadow;
        public Vector2[][] floorCornerHider_BL_TL_shadow;
        public Vector2[][] floorCornerHider_BL_TL_TR_shadow;

        public Vector2[][] wall_Single_shadow;
        public Vector2[][] wall_FourWay_shadow;
        public Vector2[][] wall_Vertical_T_shadow;
        public Vector2[][] wall_Vertical_M_shadow;
        public Vector2[][] wall_Vertical_B_shadow;
        public Vector2[][] wall_Horizontal_L_shadow;
        public Vector2[][] wall_Horizontal_M_shadow;
        public Vector2[][] wall_Horizontal_R_shadow;

        public Vector2[][] wall_Corner_TopRight_shadow;
        public Vector2[][] wall_Corner_TopLeft_shadow;
        public Vector2[][] wall_Corner_BottomRight_shadow;
        public Vector2[][] wall_Corner_BottomLeft_shadow;
        public Vector2[][] wall_Tee_Right_shadow;
        public Vector2[][] wall_Tee_Left_shadow;
        public Vector2[][] wall_Tee_Top_shadow;
        public Vector2[][] wall_Tee_Bottom_shadow;

        public Vector2[][] wall_Diagonal_TopRight_shadow;
        public Vector2[][] wall_Diagonal_TopRight_T_shadow;
        public Vector2[][] wall_Diagonal_TopRight_R_shadow;
        public Vector2[][] wall_Diagonal_TopRight_TR_shadow;
        public Vector2[][] wall_Diagonal_TopLeft_shadow;
        public Vector2[][] wall_Diagonal_TopLeft_T_shadow;
        public Vector2[][] wall_Diagonal_TopLeft_L_shadow;
        public Vector2[][] wall_Diagonal_TopLeft_TL_shadow;
        public Vector2[][] wall_Diagonal_BottomRight_shadow;
        public Vector2[][] wall_Diagonal_BottomRight_B_shadow;
        public Vector2[][] wall_Diagonal_BottomRight_R_shadow;
        public Vector2[][] wall_Diagonal_BottomRight_BR_shadow;
        public Vector2[][] wall_Diagonal_BottomLeft_shadow;
        public Vector2[][] wall_Diagonal_BottomLeft_B_shadow;
        public Vector2[][] wall_Diagonal_BottomLeft_L_shadow;
        public Vector2[][] wall_Diagonal_BottomLeft_BL_shadow;

        public Vector2[][] wallCornerHider_All_shadow;
        public Vector2[][] wallCornerHider_TL_BR_shadow;
        public Vector2[][] wallCornerHider_TR_BL_shadow;
        public Vector2[][] wallCornerHider_TL_shadow;
        public Vector2[][] wallCornerHider_TL_TR_shadow;
        public Vector2[][] wallCornerHider_TL_TR_BR_shadow;
        public Vector2[][] wallCornerHider_TR_shadow;
        public Vector2[][] wallCornerHider_TR_BR_shadow;
        public Vector2[][] wallCornerHider_TR_BR_BL_shadow;
        public Vector2[][] wallCornerHider_BR_shadow;
        public Vector2[][] wallCornerHider_BR_BL_shadow;
        public Vector2[][] wallCornerHider_BR_BL_TL_shadow;
        public Vector2[][] wallCornerHider_BL_shadow;
        public Vector2[][] wallCornerHider_BL_TL_shadow;
        public Vector2[][] wallCornerHider_BL_TL_TR_shadow;

        public Vector2[][][] anim_DoorVertical_Open_shadow;
        public Vector2[][][] anim_DoorVertical_Close_shadow;
        public Vector2[][][] anim_DoorHorizontal_Open_shadow;
        public Vector2[][][] anim_DoorHorizontal_Close_shadow;
        public Vector2[][][] anim_AirlockHorizontal_OpenBottom_shadow;
        public Vector2[][][] anim_AirlockHorizontal_CloseBottom_shadow;
        public Vector2[][][] anim_AirlockHorizontal_OpenTop_shadow;
        public Vector2[][][] anim_AirlockHorizontal_CloseTop_shadow;
        public Vector2[][][] anim_AirlockHorizontal_Wait_shadow;
        public Vector2[][][] anim_AirlockVertical_OpenLeft_shadow;
        public Vector2[][][] anim_AirlockVertical_CloseLeft_shadow;
        public Vector2[][][] anim_AirlockVertical_OpenRight_shadow;
        public Vector2[][][] anim_AirlockVertical_CloseRight_shadow;
        public Vector2[][][] anim_AirlockVertical_Wait_shadow;


        public Texture2D ShadowMap;
    //    public void GenerateShadowColliders() {
    //        floor_Single_shadow = GenerateShadowCollider(floor_Single);
    //        floor_FourWay_shadow = GenerateShadowCollider(floor_FourWay);
    //        floor_Vertical_T_shadow = GenerateShadowCollider(floor_Vertical_T);
    //        floor_Vertical_M_shadow = GenerateShadowCollider(floor_Vertical_M);
    //        floor_Vertical_B_shadow = GenerateShadowCollider(floor_Vertical_B);
    //        floor_Horizontal_L_shadow = GenerateShadowCollider(floor_Horizontal_L);
    //        floor_Horizontal_M_shadow = GenerateShadowCollider(floor_Horizontal_M);
    //        floor_Horizontal_R_shadow = GenerateShadowCollider(floor_Horizontal_R);
    //        floor_Corner_TopRight_shadow = GenerateShadowCollider(floor_Corner_TopRight);
    //        floor_Corner_TopLeft_shadow = GenerateShadowCollider(floor_Corner_TopLeft);
    //        floor_Corner_BottomRight_shadow = GenerateShadowCollider(floor_Corner_BottomRight);
    //        floor_Corner_BottomLeft_shadow = GenerateShadowCollider(floor_Corner_BottomLeft);
    //        floor_Tee_Right_shadow = GenerateShadowCollider(floor_Tee_Right);
    //        floor_Tee_Left_shadow = GenerateShadowCollider(floor_Tee_Left);
    //        floor_Tee_Top_shadow = GenerateShadowCollider(floor_Tee_Top);
    //        floor_Tee_Bottom_shadow = GenerateShadowCollider(floor_Tee_Bottom);

    //        floor_Diagonal_TopRight_shadow = GenerateShadowCollider(floor_Diagonal_TopRight);
    //        floor_Diagonal_TopRight_T_shadow = GenerateShadowCollider(floor_Diagonal_TopRight_T);
    //        floor_Diagonal_TopRight_R_shadow = GenerateShadowCollider(floor_Diagonal_TopRight_R);
    //        floor_Diagonal_TopRight_TR_shadow = GenerateShadowCollider(floor_Diagonal_TopRight_TR);
    //        floor_Diagonal_TopLeft_shadow = GenerateShadowCollider(floor_Diagonal_TopLeft);
    //        floor_Diagonal_TopLeft_T_shadow = GenerateShadowCollider(floor_Diagonal_TopLeft_T);
    //        floor_Diagonal_TopLeft_L_shadow = GenerateShadowCollider(floor_Diagonal_TopLeft_L);
    //        floor_Diagonal_TopLeft_TL_shadow = GenerateShadowCollider(floor_Diagonal_TopLeft_TL);
    //        floor_Diagonal_BottomRight_shadow = GenerateShadowCollider(floor_Diagonal_BottomRight);
    //        floor_Diagonal_BottomRight_B_shadow = GenerateShadowCollider(floor_Diagonal_BottomRight_B);
    //        floor_Diagonal_BottomRight_R_shadow = GenerateShadowCollider(floor_Diagonal_BottomRight_R);
    //        floor_Diagonal_BottomRight_BR_shadow = GenerateShadowCollider(floor_Diagonal_BottomRight_BR);
    //        floor_Diagonal_BottomLeft_shadow = GenerateShadowCollider(floor_Diagonal_BottomLeft);
    //        floor_Diagonal_BottomLeft_B_shadow = GenerateShadowCollider(floor_Diagonal_BottomLeft_B);
    //        floor_Diagonal_BottomLeft_L_shadow = GenerateShadowCollider(floor_Diagonal_BottomLeft_L);
    //        floor_Diagonal_BottomLeft_BL_shadow = GenerateShadowCollider(floor_Diagonal_BottomLeft_BL);

    //        floorCornerHider_All_shadow = GenerateShadowCollider(floorCornerHider_All);
    //        floorCornerHider_TL_BR_shadow = GenerateShadowCollider(floorCornerHider_TL_BR);
    //        floorCornerHider_TR_BL_shadow = GenerateShadowCollider(floorCornerHider_TR_BL);
    //        floorCornerHider_TL_shadow = GenerateShadowCollider(floorCornerHider_TL);
    //        floorCornerHider_TL_TR_shadow = GenerateShadowCollider(floorCornerHider_TL_TR);
    //        floorCornerHider_TL_TR_BR_shadow = GenerateShadowCollider(floorCornerHider_TL_TR_BR);
    //        floorCornerHider_TR_shadow = GenerateShadowCollider(floorCornerHider_TR);
    //        floorCornerHider_TR_BR_shadow = GenerateShadowCollider(floorCornerHider_TR_BR);
    //        floorCornerHider_TR_BR_BL_shadow = GenerateShadowCollider(floorCornerHider_TR_BR_BL);
    //        floorCornerHider_BR_shadow = GenerateShadowCollider(floorCornerHider_BR);
    //        floorCornerHider_BR_BL_shadow = GenerateShadowCollider(floorCornerHider_BR_BL);
    //        floorCornerHider_BR_BL_TL_shadow = GenerateShadowCollider(floorCornerHider_BR_BL_TL);
    //        floorCornerHider_BL_shadow = GenerateShadowCollider(floorCornerHider_BL);
    //        floorCornerHider_BL_TL_shadow = GenerateShadowCollider(floorCornerHider_BL_TL);
    //        floorCornerHider_BL_TL_TR_shadow = GenerateShadowCollider(floorCornerHider_BL_TL_TR);

    //        wall_Single_shadow = GenerateShadowCollider(wall_Single);
    //        wall_FourWay_shadow = GenerateShadowCollider(wall_FourWay);
    //        wall_Vertical_T_shadow = GenerateShadowCollider(wall_Vertical_T);
    //        wall_Vertical_M_shadow = GenerateShadowCollider(wall_Vertical_M);
    //        wall_Vertical_B_shadow = GenerateShadowCollider(wall_Vertical_B);
    //        wall_Horizontal_L_shadow = GenerateShadowCollider(wall_Horizontal_L);
    //        wall_Horizontal_M_shadow = GenerateShadowCollider(wall_Horizontal_M);
    //        wall_Horizontal_R_shadow = GenerateShadowCollider(wall_Horizontal_R);

    //        wall_Corner_TopRight_shadow = GenerateShadowCollider(wall_Corner_TopRight);
    //        wall_Corner_TopLeft_shadow = GenerateShadowCollider(wall_Corner_TopLeft);
    //        wall_Corner_BottomRight_shadow = GenerateShadowCollider(wall_Corner_BottomRight);
    //        wall_Corner_BottomLeft_shadow = GenerateShadowCollider(wall_Corner_BottomLeft);
    //        wall_Tee_Right_shadow = GenerateShadowCollider(wall_Tee_Right);
    //        wall_Tee_Left_shadow = GenerateShadowCollider(wall_Tee_Left);
    //        wall_Tee_Top_shadow = GenerateShadowCollider(wall_Tee_Top);
    //        wall_Tee_Bottom_shadow = GenerateShadowCollider(wall_Tee_Bottom);

    //        wall_Diagonal_TopRight_shadow = GenerateShadowCollider(wall_Diagonal_TopRight);
    //        wall_Diagonal_TopRight_T_shadow = GenerateShadowCollider(wall_Diagonal_TopRight_T);
    //        wall_Diagonal_TopRight_R_shadow = GenerateShadowCollider(wall_Diagonal_TopRight_R);
    //        wall_Diagonal_TopRight_TR_shadow = GenerateShadowCollider(wall_Diagonal_TopRight_TR);
    //        wall_Diagonal_TopLeft_shadow = GenerateShadowCollider(wall_Diagonal_TopLeft);
    //        wall_Diagonal_TopLeft_T_shadow = GenerateShadowCollider(wall_Diagonal_TopLeft_T);
    //        wall_Diagonal_TopLeft_L_shadow = GenerateShadowCollider(wall_Diagonal_TopLeft_L);
    //        wall_Diagonal_TopLeft_TL_shadow = GenerateShadowCollider(wall_Diagonal_TopLeft_TL);
    //        wall_Diagonal_BottomRight_shadow = GenerateShadowCollider(wall_Diagonal_BottomRight);
    //        wall_Diagonal_BottomRight_B_shadow = GenerateShadowCollider(wall_Diagonal_BottomRight_B);
    //        wall_Diagonal_BottomRight_R_shadow = GenerateShadowCollider(wall_Diagonal_BottomRight_R);
    //        wall_Diagonal_BottomRight_BR_shadow = GenerateShadowCollider(wall_Diagonal_BottomRight_BR);
    //        wall_Diagonal_BottomLeft_shadow = GenerateShadowCollider(wall_Diagonal_BottomLeft);
    //        wall_Diagonal_BottomLeft_B_shadow = GenerateShadowCollider(wall_Diagonal_BottomLeft_B);
    //        wall_Diagonal_BottomLeft_L_shadow = GenerateShadowCollider(wall_Diagonal_BottomLeft_L);
    //        wall_Diagonal_BottomLeft_BL_shadow = GenerateShadowCollider(wall_Diagonal_BottomLeft_BL);

    //        wallCornerHider_All_shadow = GenerateShadowCollider(wallCornerHider_All);
    //        wallCornerHider_TL_BR_shadow = GenerateShadowCollider(wallCornerHider_TL_BR);
    //        wallCornerHider_TR_BL_shadow = GenerateShadowCollider(wallCornerHider_TR_BL);
    //        wallCornerHider_TL_shadow = GenerateShadowCollider(wallCornerHider_TL);
    //        wallCornerHider_TL_TR_shadow = GenerateShadowCollider(wallCornerHider_TL_TR);
    //        wallCornerHider_TL_TR_BR_shadow = GenerateShadowCollider(wallCornerHider_TL_TR_BR);
    //        wallCornerHider_TR_shadow = GenerateShadowCollider(wallCornerHider_TR);
    //        wallCornerHider_TR_BR_shadow = GenerateShadowCollider(wallCornerHider_TR_BR);
    //        wallCornerHider_TR_BR_BL_shadow = GenerateShadowCollider(wallCornerHider_TR_BR_BL);
    //        wallCornerHider_BR_shadow = GenerateShadowCollider(wallCornerHider_BR);
    //        wallCornerHider_BR_BL_shadow = GenerateShadowCollider(wallCornerHider_BR_BL);
    //        wallCornerHider_BR_BL_TL_shadow = GenerateShadowCollider(wallCornerHider_BR_BL_TL);
    //        wallCornerHider_BL_shadow = GenerateShadowCollider(wallCornerHider_BL);
    //        wallCornerHider_BL_TL_shadow = GenerateShadowCollider(wallCornerHider_BL_TL);
    //        wallCornerHider_BL_TL_TR_shadow = GenerateShadowCollider(wallCornerHider_BL_TL_TR);
    //    } 
    //    private List<Color32> shadowPixels;
    //    private Color32[][] assetPixels = new Color32[Grid.TILE_RESOLUTION * 2][];
    //    private Dictionary<int, Vector2> startingPixels = new Dictionary<int, Vector2>();
    //    private int currentX = -1;
    //    private int currentY = -1;
    //    private int prevX = -1;
    //    private int prevY = -1;
    //    private List<List<Vector2>> vertices;
    //    private int startingPixelindex;
    //    Vector2[][] GenerateShadowCollider(DoubleInt _texturePos) {
    //        if (shadowPixels == null)
    //            shadowPixels = new List<Color32>(ShadowMap.GetPixels32());
    //        for (int y = 0; y < assetPixels.Length; y++)
    //            assetPixels[y] = new Color32[Grid.TILE_RESOLUTION];

    //        for (int y = 0; y < assetPixels.Length; y++) {
    //            for (int x = 0; x < assetPixels[y].Length; x++) {
    //                if(y < assetPixels.Length * 0.5f)
    //                    assetPixels[y][x] = shadowPixels[Mathf.RoundToInt((Grid.TILE_RESOLUTION * (_texturePos.Y + y)) + (_texturePos.X + x))];
    //                else
    //                    assetPixels[y][x] = shadowPixels[Mathf.RoundToInt((Grid.TILE_RESOLUTION * (_texturePos.Y + y + 1)) + (_texturePos.X + x))];
    //            }
    //        }

    //        startingPixels.Clear();
    //        for (int y = 0; y < assetPixels.Length; y++) {
    //            for (int x = 0; x < assetPixels[y].Length; x++) {
                    
    //                // find a pixel for each path and cache as starting points
    //                if (assetPixels[y][x].a > 0 && !startingPixels.ContainsKey(assetPixels[y][x].a))
    //                    startingPixels.Add(assetPixels[y][x].a, new Vector2(x, y));
    //            }
    //        }

    //        startingPixelindex = 0;
    //        vertices = new List<List<Vector2>>(startingPixels.Count);
    //        foreach (KeyValuePair<int, Vector2> pixel in startingPixels) {
    //            vertices[startingPixelindex] = new List<Vector2>();
    //            currentX = (int)pixel.Value.x;
    //            currentY = (int)pixel.Value.y;
    //            prevX = currentX;
    //            prevY = currentY;

    //            while (currentX != pixel.Value.x && currentY != pixel.Value.y) {
    //                // if corner, add as vertex
    //                if (assetPixels[currentY][currentX].r == 255)
    //                    vertices[startingPixelindex].Add(new Vector2(currentX, currentY));

    //                if (currentX - prevX != -1 || currentY - prevY != 0) { // if not from L, try L
    //                    if (assetPixels[currentY][currentX - 1].a == pixel.Key) {
    //                        prevX = currentX;
    //                        prevY = currentY;
    //                        currentX -= 1;
    //                        continue;
    //                    }
    //                }
    //                if (currentX - prevX != -1 || currentY - prevY != 1) { // if not from TL, try TL
    //                    if (assetPixels[currentY + 1][currentX - 1].a == pixel.Key) {
    //                        prevX = currentX;
    //                        prevY = currentY;
    //                        currentX -= 1;
    //                        currentY += 1;
    //                        continue;
    //                    }
    //                }
    //                if (currentX - prevX != 0 || currentY - prevY != 1) { // if not from T, try T
    //                    if (assetPixels[currentY + 1][currentX].a == pixel.Key) {
    //                        prevX = currentX;
    //                        prevY = currentY;
    //                        currentY += 1;
    //                        continue;
    //                    }
    //                }
    //                if (currentX - prevX != 1 || currentY - prevY != 1) { // if not from TR, try TR
    //                    if (assetPixels[currentY + 1][currentX + 1].a == pixel.Key) {
    //                        prevX = currentX;
    //                        prevY = currentY;
    //                        currentX += 1;
    //                        currentY += 1;
    //                        continue;
    //                    }
    //                }
    //                if (currentX - prevX != 1 || currentY - prevY != 0) { // if not from R, try R
    //                    if (assetPixels[currentY][currentX + 1].a == pixel.Key) {
    //                        prevX = currentX;
    //                        prevY = currentY;
    //                        currentX += 1;
    //                        continue;
    //                    }
    //                }
    //                if (currentX - prevX != 1 || currentY - prevY != -1) { // if not from BR, try BR
    //                    if (assetPixels[currentY - 1][currentX + 1].a == pixel.Key) {
    //                        prevX = currentX;
    //                        prevY = currentY;
    //                        currentX += 1;
    //                        currentY -= 1;
    //                        continue;
    //                    }
    //                }
    //                if (currentX - prevX != 0 || currentY - prevY != -1) { // if not from B, try B
    //                    if (assetPixels[currentY - 1][currentX].a == pixel.Key) {
    //                        prevX = currentX;
    //                        prevY = currentY;
    //                        currentY -= 1;
    //                        continue;
    //                    }
    //                }
    //                if (currentX - prevX != -1 || currentY - prevY != -1) { // // if not from BL, try BL
    //                    if (assetPixels[currentY - 1][currentX - 1].a == pixel.Key) {
    //                        prevX = currentX;
    //                        prevY = currentY;
    //                        currentX -= 1;
    //                        currentY -= 1;
    //                        continue;
    //                    }
    //                }
    //            }

    //            startingPixelindex++;
    //        }

    //        Vector2[][] array = new Vector2[vertices.Count][];
    //        for (int i = 0; i < array.Length; i++)
    //            array[i] = vertices[i].ToArray();
    //        return array;
    //    }
    }
    public WallSet[] WallSets;

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
                    case Tile.TileOrientation.TopRight: {
                            if (_hasConnection_Top) {
                                if (_hasConnection_Right) return (_isBottom ? WallSet.wall_Diagonal_TopRight_TR : null);
                                else return (_isBottom ? WallSet.wall_Diagonal_TopRight_T : null);
                            }
                            else if(_hasConnection_Right) return (_isBottom ? WallSet.wall_Diagonal_TopRight_R : null);
                            else return (_isBottom ? WallSet.wall_Diagonal_TopRight : null);
                        }

                    case Tile.TileOrientation.TopLeft: {
                            if (_hasConnection_Top) {
                                if (_hasConnection_Left) return (_isBottom ? WallSet.wall_Diagonal_TopLeft_TL : null);
                                else return (_isBottom ? WallSet.wall_Diagonal_TopLeft_T : null);
                            }
                            else if(_hasConnection_Left) return (_isBottom ? WallSet.wall_Diagonal_TopLeft_L : null);
                            else return (_isBottom ? WallSet.wall_Diagonal_TopLeft : null);
                        }
                    case Tile.TileOrientation.BottomRight: {
                            if (_hasConnection_Bottom) {
                                if (_hasConnection_Right) return (_isBottom ? null : WallSet.wall_Diagonal_BottomRight_BR);
                                else return(_isBottom ? null : WallSet.wall_Diagonal_BottomRight_B);
                            }
                            else if (_hasConnection_Right) return (_isBottom ? null : WallSet.wall_Diagonal_BottomRight_R);
                            else return(_isBottom ? null : WallSet.wall_Diagonal_BottomRight);
                        }
                    case Tile.TileOrientation.BottomLeft: {
                            if (_hasConnection_Bottom) {
                                if (_hasConnection_Left) return (_isBottom ? null : WallSet.wall_Diagonal_BottomLeft_BL);
                                else return(_isBottom ? null : WallSet.wall_Diagonal_BottomLeft_B);
                            }
                            else if (_hasConnection_Left) return (_isBottom ? null : WallSet.wall_Diagonal_BottomLeft_L);
                            else return(_isBottom ? null : WallSet.wall_Diagonal_BottomLeft);
                        }
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
                    case Tile.TileOrientation.TopRight: {
                            if (_hasConnection_Top) {
                                if (_hasConnection_Right) return WallSet.floor_Diagonal_TopRight_TR;
                                else return WallSet.floor_Diagonal_TopRight_T;
                            }
                            else if (_hasConnection_Right) return WallSet.floor_Diagonal_TopRight_R;
                            else return WallSet.floor_Diagonal_TopRight;
                        }

                    case Tile.TileOrientation.TopLeft: {
                            if (_hasConnection_Top) {
                                if (_hasConnection_Left) return WallSet.floor_Diagonal_TopLeft_TL;
                                else return WallSet.floor_Diagonal_TopLeft_T;
                            }
                            else if (_hasConnection_Left) return WallSet.floor_Diagonal_TopLeft_L;
                            else return WallSet.floor_Diagonal_TopLeft;
                        }
                    case Tile.TileOrientation.BottomRight: {
                            if (_hasConnection_Bottom) {
                                if (_hasConnection_Right) return  WallSet.floor_Diagonal_BottomRight_BR;
                                else return  WallSet.floor_Diagonal_BottomRight_B;
                            }
                            else if (_hasConnection_Right) return  WallSet.floor_Diagonal_BottomRight_R;
                            else return  WallSet.floor_Diagonal_BottomRight;
                        }
                    case Tile.TileOrientation.BottomLeft: {
                            if (_hasConnection_Bottom) {
                                if (_hasConnection_Left) return  WallSet.floor_Diagonal_BottomLeft_BL;
                                else return  WallSet.floor_Diagonal_BottomLeft_B;
                            }
                            else if (_hasConnection_Left) return  WallSet.floor_Diagonal_BottomLeft_L;
                            else return  WallSet.floor_Diagonal_BottomLeft;
                        }
                    default:
                        throw new System.Exception(_tileOrientation + " is not supported by diagonals!");
                }
            case Tile.Type.Door:
			case Tile.Type.Airlock:
				throw new System.Exception (_tileType.ToString() + " does not apply to Floor!");
			default:
				throw new System.NotImplementedException(_tileType + " hasn't been properly implemented yet!");
		}
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
}
