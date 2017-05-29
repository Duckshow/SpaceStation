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

        public Texture2D ShadowMap;
        [System.Serializable]
        public class PathVertices{ // workaround for serializing jagged array
            public Vector2[] Vertices;
            public PathVertices(){ }
            public PathVertices(int _length){ Vertices = new Vector2[_length]; }
        }
        // note: code for generating these can be found in CachedAssetsEditor.cs
        public PathVertices[] wall_Single_shadow;
        public PathVertices[] wall_FourWay_shadow;
        public PathVertices[] wall_Vertical_T_shadow;
        public PathVertices[] wall_Vertical_M_shadow;
        public PathVertices[] wall_Vertical_B_shadow;
        public PathVertices[] wall_Horizontal_L_shadow;
        public PathVertices[] wall_Horizontal_M_shadow;
        public PathVertices[] wall_Horizontal_R_shadow;

        public PathVertices[] wall_Corner_TopRight_shadow;
        public PathVertices[] wall_Corner_TopLeft_shadow;
        public PathVertices[] wall_Corner_BottomRight_shadow;
        public PathVertices[] wall_Corner_BottomLeft_shadow;
        public PathVertices[] wall_Tee_Right_shadow;
        public PathVertices[] wall_Tee_Left_shadow;
        public PathVertices[] wall_Tee_Top_shadow;
        public PathVertices[] wall_Tee_Bottom_shadow;

        public PathVertices[] wall_Diagonal_TopRight_shadow;
        public PathVertices[] wall_Diagonal_TopRight_T_shadow;
        public PathVertices[] wall_Diagonal_TopRight_R_shadow;
        public PathVertices[] wall_Diagonal_TopRight_TR_shadow;
        public PathVertices[] wall_Diagonal_TopLeft_shadow;
        public PathVertices[] wall_Diagonal_TopLeft_T_shadow;
        public PathVertices[] wall_Diagonal_TopLeft_L_shadow;
        public PathVertices[] wall_Diagonal_TopLeft_TL_shadow;
        public PathVertices[] wall_Diagonal_BottomRight_shadow;
        public PathVertices[] wall_Diagonal_BottomRight_B_shadow;
        public PathVertices[] wall_Diagonal_BottomRight_R_shadow;
        public PathVertices[] wall_Diagonal_BottomRight_BR_shadow;
        public PathVertices[] wall_Diagonal_BottomLeft_shadow;
        public PathVertices[] wall_Diagonal_BottomLeft_B_shadow;
        public PathVertices[] wall_Diagonal_BottomLeft_L_shadow;
        public PathVertices[] wall_Diagonal_BottomLeft_BL_shadow;

        // TODO: animations don't work at all currently - change that!
        // public PathVertices[][] anim_DoorVertical_Open_shadow;
        // public PathVertices[][] anim_DoorVertical_Close_shadow;
        // public PathVertices[][] anim_DoorHorizontal_Open_shadow;
        // public PathVertices[][] anim_DoorHorizontal_Close_shadow;
        // public PathVertices[][] anim_AirlockHorizontal_OpenBottom_shadow;
        // public PathVertices[][] anim_AirlockHorizontal_CloseBottom_shadow;
        // public PathVertices[][] anim_AirlockHorizontal_OpenTop_shadow;
        // public PathVertices[][] anim_AirlockHorizontal_CloseTop_shadow;
        // public PathVertices[][] anim_AirlockHorizontal_Wait_shadow;
        // public PathVertices[][] anim_AirlockVertical_OpenLeft_shadow;
        // public PathVertices[][] anim_AirlockVertical_CloseLeft_shadow;
        // public PathVertices[][] anim_AirlockVertical_OpenRight_shadow;
        // public PathVertices[][] anim_AirlockVertical_CloseRight_shadow;
        // public PathVertices[][] anim_AirlockVertical_Wait_shadow;

        private PathVertices[] pv;
        public PolygonCollider2D GetShadowCollider(P _type){
            switch(_type){
                case P.Wall_Single:
                    pv = wall_Single_shadow;
                    break;
                case P.Wall_Fourway:
                    pv = wall_FourWay_shadow;
                    break;
                case P.Wall_Vertical_T:
                    pv = wall_Vertical_T_shadow;
                    break;
                case P.Wall_Vertical_M:
                    pv = wall_Vertical_M_shadow;
                    break;
                case P.Wall_Vertical_B:
                    pv = wall_Vertical_B_shadow;
                    break;
                case P.Wall_Horizontal_L:
                    pv = wall_Horizontal_L_shadow;
                    break;
                case P.Wall_Horizontal_M:
                    pv = wall_Horizontal_M_shadow;
                    break;
                case P.Wall_Horizontal_R:
                    pv = wall_Horizontal_R_shadow;
                    break;
                case P.Wall_Corner_TR:
                    pv = wall_Corner_TopRight_shadow;
                    break;
                case P.Wall_Corner_TL:
                    pv = wall_Corner_TopLeft_shadow;
                    break;
                case P.Wall_Corner_BR:
                    pv = wall_Corner_BottomRight_shadow;
                    break;
                case P.Wall_Corner_BL:
                    pv = wall_Corner_BottomLeft_shadow;
                    break;
                case P.Wall_Tee_R:
                    pv = wall_Tee_Right_shadow;
                    break;
                case P.Wall_Tee_L:
                    pv = wall_Tee_Left_shadow;
                    break;
                case P.Wall_Tee_T:
                    pv = wall_Tee_Top_shadow;
                    break;
                case P.Wall_Tee_B:
                    pv = wall_Tee_Bottom_shadow;
                    break;
                case P.Wall_Diagonal_TR:
                    pv = wall_Diagonal_TopRight_shadow;
                    break;
                case P.Wall_Diagonal_TR_R:
                    pv = wall_Diagonal_TopRight_R_shadow;
                    break;
                case P.Wall_Diagonal_TR_T:
                    pv = wall_Diagonal_TopRight_T_shadow;
                    break;
                case P.Wall_Diagonal_TR_TR:
                    pv = wall_Diagonal_TopRight_TR_shadow;
                    break;
                case P.Wall_Diagonal_TL:
                    pv = wall_Diagonal_TopLeft_shadow;
                    break;
                case P.Wall_Diagonal_TL_L:
                    pv = wall_Diagonal_TopLeft_L_shadow;
                    break;
                case P.Wall_Diagonal_TL_T:
                    pv = wall_Diagonal_TopLeft_T_shadow;
                    break;
                case P.Wall_Diagonal_TL_TL:
                    pv = wall_Diagonal_TopLeft_TL_shadow;
                    break;
                case P.Wall_Diagonal_BR:
                    pv = wall_Diagonal_BottomRight_shadow;
                    break;
                case P.Wall_Diagonal_BR_R:
                    pv = wall_Diagonal_BottomRight_R_shadow;
                    break;
                case P.Wall_Diagonal_BR_B:
                    pv = wall_Diagonal_BottomRight_B_shadow;
                    break;
                case P.Wall_Diagonal_BR_BR:
                    pv = wall_Diagonal_BottomRight_BR_shadow;
                    break;
                case P.Wall_Diagonal_BL:
                    pv = wall_Diagonal_BottomLeft_shadow;
                    break;
                case P.Wall_Diagonal_BL_L:
                    pv = wall_Diagonal_BottomLeft_L_shadow;
                    break;
                case P.Wall_Diagonal_BL_B:
                    pv = wall_Diagonal_BottomLeft_B_shadow;
                    break;
                case P.Wall_Diagonal_BL_BL:
                    pv = wall_Diagonal_BottomLeft_BL_shadow;
                    break;
                default:
                    pv = null;
                    break;
                // case P.DoorVertical:
                //     pv = floor_Single_shadow;
                //     break;
                // case P.DoorHorizontal:
                //     pv = floor_Single_shadow;
                //     break;
                // case P.AirlockHorizontal_OpenBottom_BOTTOM:
                //     pv = floor_Single_shadow;
                //     break;
                // case P.AirlockHorizontal_OpenBottom_TOP:
                //     pv = floor_Single_shadow;
                //     break;
                // case P.AirlockHorizontal_OpenTop:
                //     pv = floor_Single_shadow;
                //     break;
                // case P.AirlockHorizontal_Wait:
                //     pv = floor_Single_shadow;
                //     break;
                // case P.AirlockVertical_OpenLeft_BOTTOM:
                //     pv = floor_Single_shadow;
                //     break;
                // case P.AirlockVertical_OpenLeft_TOP:
                //     pv = floor_Single_shadow;
                //     break;
                // case P. AirlockVertical_OpenRight_BOTTOM:
                //     pv = floor_Single_shadow;
                //     break;
                // case P.AirlockVertical_OpenRight_TOP:
                //     pv = floor_Single_shadow;
                //     break;
                // case P.AirlockVertical_Wait:
                //     pv = anim_AirlockVertical_Wait;
                //     break;
            }
            if (pv != null){
                for(int i = 0; i < pv.Length; i++)
                    ShadowCollider.SetPath(i, pv[i].Vertices);
            }
            return ShadowCollider;
        }
    }
    public WallSet[] WallSets;
    public static PolygonCollider2D ShadowCollider;
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
