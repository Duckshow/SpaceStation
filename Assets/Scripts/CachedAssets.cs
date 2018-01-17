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
    public class WallSet {

        public const int TEXTURE_SIZE_X = 1024; // TODO: this should be automated, seriously
        public const int TEXTURE_SIZE_Y = 2688;

        public enum Purpose {
			Null,
			UVControllerBasic, // used to pool UVControllerBasics. Confusing, yes.

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
            AirlockHorizontal,
            AirlockHorizontal_Open_B,
            AirlockHorizontal_Open_B_TOP,
            AirlockHorizontal_Open_T,
            AirlockHorizontal_Open_T_TOP,
            AirlockHorizontal_Wait,
            AirlockHorizontal_Wait_TOP,
            AirlockVertical,
            AirlockVertical_Open_L,
            AirlockVertical_Open_L_TOP,
            AirlockVertical_Open_R,
            AirlockVertical_Open_R_TOP,
            AirlockVertical_Wait,
            AirlockVertical_Wait_TOP
        }
        private static List<List<Purpose>> AllAssetPurposes = new List<List<Purpose>>() {
            new List<Purpose>() {
				Purpose.Null
			},
			new List<Purpose>() {
                Purpose.Floor_Single,
                Purpose.Floor_Fourway,
                Purpose.Floor_Vertical_T,
                Purpose.Floor_Vertical_M,
                Purpose.Floor_Vertical_B,
                Purpose.Floor_Horizontal_L,
                Purpose.Floor_Horizontal_M,
                Purpose.Floor_Horizontal_R,
                Purpose.Floor_Corner_TR,
                Purpose.Floor_Corner_TL,
                Purpose.Floor_Corner_BR,
                Purpose.Floor_Corner_BL,
                Purpose.Floor_Tee_R,
                Purpose.Floor_Tee_L,
                Purpose.Floor_Tee_T,
                Purpose.Floor_Tee_B
            },
            new List<Purpose>() {
                Purpose.Floor_Diagonal_TR,
                Purpose.Floor_Diagonal_TR_R,
                Purpose.Floor_Diagonal_TR_T,
                Purpose.Floor_Diagonal_TR_TR,
                Purpose.Floor_Diagonal_TL,
                Purpose.Floor_Diagonal_TL_L,
                Purpose.Floor_Diagonal_TL_T,
                Purpose.Floor_Diagonal_TL_TL,
                Purpose.Floor_Diagonal_BR,
                Purpose.Floor_Diagonal_BR_R,
                Purpose.Floor_Diagonal_BR_B,
                Purpose.Floor_Diagonal_BR_BR,
                Purpose.Floor_Diagonal_BL,
                Purpose.Floor_Diagonal_BL_L,
                Purpose.Floor_Diagonal_BL_B,
                Purpose.Floor_Diagonal_BL_BL
            },
            new List<Purpose>() {
                Purpose.FloorCornerHider_All,
                Purpose.FloorCornerHider_TL_BR,
                Purpose.FloorCornerHider_TR_BL,
                Purpose.FloorCornerHider_TL,
                Purpose.FloorCornerHider_TL_TR,
                Purpose.FloorCornerHider_TL_TR_BR,
                Purpose.FloorCornerHider_TR,
                Purpose.FloorCornerHider_TR_BR,
                Purpose.FloorCornerHider_TR_BR_BL,
                Purpose.FloorCornerHider_BR,
                Purpose.FloorCornerHider_BR_BL,
                Purpose.FloorCornerHider_BR_BL_TL,
                Purpose.FloorCornerHider_BL,
                Purpose.FloorCornerHider_BL_TL,
                Purpose.FloorCornerHider_BL_TL_TR
            },
            new List<Purpose>() {
                Purpose.Wall_Single,
                Purpose.Wall_Fourway,
                Purpose.Wall_Vertical_T,
                Purpose.Wall_Vertical_M,
                Purpose.Wall_Vertical_B,
                Purpose.Wall_Horizontal_L,
                Purpose.Wall_Horizontal_M,
                Purpose.Wall_Horizontal_R,
                Purpose.Wall_Corner_TR,
                Purpose.Wall_Corner_TL,
                Purpose.Wall_Corner_BR,
                Purpose.Wall_Corner_BL,
                Purpose.Wall_Tee_R,
                Purpose.Wall_Tee_L,
                Purpose.Wall_Tee_T,
                Purpose.Wall_Tee_B
            },
            new List<Purpose>() {
                Purpose.Wall_Diagonal_TR,
                Purpose.Wall_Diagonal_TR_R,
                Purpose.Wall_Diagonal_TR_T,
                Purpose.Wall_Diagonal_TR_TR,
                Purpose.Wall_Diagonal_TL,
                Purpose.Wall_Diagonal_TL_L,
                Purpose.Wall_Diagonal_TL_T,
                Purpose.Wall_Diagonal_TL_TL,
                Purpose.Wall_Diagonal_BR,
                Purpose.Wall_Diagonal_BR_R,
                Purpose.Wall_Diagonal_BR_B,
                Purpose.Wall_Diagonal_BR_BR,
                Purpose.Wall_Diagonal_BL,
                Purpose.Wall_Diagonal_BL_L,
                Purpose.Wall_Diagonal_BL_B,
                Purpose.Wall_Diagonal_BL_BL
            },
            new List<Purpose>() {
                Purpose.WallCornerHider_All,
                Purpose.WallCornerHider_TL_BR,
                Purpose.WallCornerHider_TR_BL,
                Purpose.WallCornerHider_TL,
                Purpose.WallCornerHider_TL_TR,
                Purpose.WallCornerHider_TL_TR_BR,
                Purpose.WallCornerHider_TR,
                Purpose.WallCornerHider_TR_BR,
                Purpose.WallCornerHider_TR_BR_BL,
                Purpose.WallCornerHider_BR,
                Purpose.WallCornerHider_BR_BL,
                Purpose.WallCornerHider_BR_BL_TL,
                Purpose.WallCornerHider_BL,
                Purpose.WallCornerHider_BL_TL,
                Purpose.WallCornerHider_BL_TL_TR
            },
            new List<Purpose>() {
                Purpose.DoorVertical
            },
            new List<Purpose>() {
                Purpose.DoorHorizontal
            },
            new List<Purpose>() {
                Purpose.AirlockHorizontal_Open_B
            },
            new List<Purpose>() {
                Purpose.AirlockHorizontal_Open_B_TOP
            },
            new List<Purpose>() {
                Purpose.AirlockHorizontal_Open_T
            },
            new List<Purpose>() {
                Purpose.AirlockHorizontal_Open_T_TOP
            },
            new List<Purpose>() {
                Purpose.AirlockHorizontal_Wait
            },
            new List<Purpose>() {
                Purpose.AirlockHorizontal_Wait_TOP
            },
            new List<Purpose>() {
                Purpose.AirlockVertical_Open_L
            },
            new List<Purpose>() {
                Purpose.AirlockVertical_Open_L_TOP
            },
            new List<Purpose>() {
                Purpose.AirlockVertical_Open_R
            },
            new List<Purpose>() {
                Purpose.AirlockVertical_Open_R_TOP
            },
            new List<Purpose>() {
                Purpose.AirlockVertical_Wait
            },
            new List<Purpose>() {
                Purpose.AirlockVertical_Wait_TOP
            }
        };
        private static Vector2i GetTextureCoord(Purpose id) {
            Vector2i _di = new Vector2i();
            int _index = AllAssetPurposes.FindIndex(x => x.Contains(id));
            _di.x = AllAssetPurposes[_index].FindIndex(x => x == id);
            _di.y = _index * 2; // 2 because of the height of individual tile-assets
            return _di;
        }

		public static Vector2i Null 							= GetTextureCoord(Purpose.Null);

		public static Vector2i floor_Single 					= GetTextureCoord(Purpose.Floor_Single);
        public static Vector2i floor_FourWay 					= GetTextureCoord(Purpose.Floor_Fourway);
        public static Vector2i floor_Vertical_T 				= GetTextureCoord(Purpose.Floor_Vertical_T);
        public static Vector2i floor_Vertical_M 				= GetTextureCoord(Purpose.Floor_Vertical_M);
        public static Vector2i floor_Vertical_B 				= GetTextureCoord(Purpose.Floor_Vertical_B);
        public static Vector2i floor_Horizontal_L 				= GetTextureCoord(Purpose.Floor_Horizontal_L);
        public static Vector2i floor_Horizontal_M 				= GetTextureCoord(Purpose.Floor_Horizontal_M);
        public static Vector2i floor_Horizontal_R 				= GetTextureCoord(Purpose.Floor_Horizontal_R);
        public static Vector2i floor_Corner_TopRight 			= GetTextureCoord(Purpose.Floor_Corner_TR);
        public static Vector2i floor_Corner_TopLeft 			= GetTextureCoord(Purpose.Floor_Corner_TL);
        public static Vector2i floor_Corner_BottomRight 		= GetTextureCoord(Purpose.Floor_Corner_BR);
        public static Vector2i floor_Corner_BottomLeft 		    = GetTextureCoord(Purpose.Floor_Corner_BL);
        public static Vector2i floor_Tee_Right 				    = GetTextureCoord(Purpose.Floor_Tee_R);
        public static Vector2i floor_Tee_Left 					= GetTextureCoord(Purpose.Floor_Tee_L);
        public static Vector2i floor_Tee_Top 					= GetTextureCoord(Purpose.Floor_Tee_T);
        public static Vector2i floor_Tee_Bottom 				= GetTextureCoord(Purpose.Floor_Tee_B);

        public static Vector2i floor_Diagonal_TopRight 		    = GetTextureCoord(Purpose.Floor_Diagonal_TR);
        public static Vector2i floor_Diagonal_TopRight_T 		= GetTextureCoord(Purpose.Floor_Diagonal_TR_T);
        public static Vector2i floor_Diagonal_TopRight_R 		= GetTextureCoord(Purpose.Floor_Diagonal_TR_R);
        public static Vector2i floor_Diagonal_TopRight_TR 		= GetTextureCoord(Purpose.Floor_Diagonal_TR_TR);
        public static Vector2i floor_Diagonal_TopLeft 			= GetTextureCoord(Purpose.Floor_Diagonal_TL);
        public static Vector2i floor_Diagonal_TopLeft_T 		= GetTextureCoord(Purpose.Floor_Diagonal_TL_T);
        public static Vector2i floor_Diagonal_TopLeft_L 		= GetTextureCoord(Purpose.Floor_Diagonal_TL_L);
        public static Vector2i floor_Diagonal_TopLeft_TL 		= GetTextureCoord(Purpose.Floor_Diagonal_TL_TL);
        public static Vector2i floor_Diagonal_BottomRight 		= GetTextureCoord(Purpose.Floor_Diagonal_BR);
        public static Vector2i floor_Diagonal_BottomRight_B 	= GetTextureCoord(Purpose.Floor_Diagonal_BR_B);
        public static Vector2i floor_Diagonal_BottomRight_R 	= GetTextureCoord(Purpose.Floor_Diagonal_BR_R);
        public static Vector2i floor_Diagonal_BottomRight_BR 	= GetTextureCoord(Purpose.Floor_Diagonal_BR_BR);
        public static Vector2i floor_Diagonal_BottomLeft 		= GetTextureCoord(Purpose.Floor_Diagonal_BL);
        public static Vector2i floor_Diagonal_BottomLeft_B 	    = GetTextureCoord(Purpose.Floor_Diagonal_BL_B);
        public static Vector2i floor_Diagonal_BottomLeft_L 	    = GetTextureCoord(Purpose.Floor_Diagonal_BL_L);
        public static Vector2i floor_Diagonal_BottomLeft_BL 	= GetTextureCoord(Purpose.Floor_Diagonal_BL_BL);

        public static Vector2i floorCornerHider_All 			= GetTextureCoord(Purpose.FloorCornerHider_All);
        public static Vector2i floorCornerHider_TL_BR 			= GetTextureCoord(Purpose.FloorCornerHider_TL_BR);
        public static Vector2i floorCornerHider_TR_BL 			= GetTextureCoord(Purpose.FloorCornerHider_TR_BL);
        public static Vector2i floorCornerHider_TL 			    = GetTextureCoord(Purpose.FloorCornerHider_TL);
        public static Vector2i floorCornerHider_TL_TR 			= GetTextureCoord(Purpose.FloorCornerHider_TL_TR);
        public static Vector2i floorCornerHider_TL_TR_BR 		= GetTextureCoord(Purpose.FloorCornerHider_TL_TR_BR);
        public static Vector2i floorCornerHider_TR 			    = GetTextureCoord(Purpose.FloorCornerHider_TR);
        public static Vector2i floorCornerHider_TR_BR 			= GetTextureCoord(Purpose.FloorCornerHider_TR_BR);
        public static Vector2i floorCornerHider_TR_BR_BL 		= GetTextureCoord(Purpose.FloorCornerHider_TR_BR_BL);
        public static Vector2i floorCornerHider_BR 			    = GetTextureCoord(Purpose.FloorCornerHider_BR);
        public static Vector2i floorCornerHider_BR_BL 			= GetTextureCoord(Purpose.FloorCornerHider_BR_BL);
        public static Vector2i floorCornerHider_BR_BL_TL 		= GetTextureCoord(Purpose.FloorCornerHider_BR_BL_TL);
        public static Vector2i floorCornerHider_BL 			    = GetTextureCoord(Purpose.FloorCornerHider_BL);
        public static Vector2i floorCornerHider_BL_TL 			= GetTextureCoord(Purpose.FloorCornerHider_BL_TL);
        public static Vector2i floorCornerHider_BL_TL_TR 		= GetTextureCoord(Purpose.FloorCornerHider_BL_TL_TR);

        public static Vector2i wall_Single 					    = GetTextureCoord(Purpose.Wall_Single);
        public static Vector2i wall_FourWay 					= GetTextureCoord(Purpose.Wall_Fourway);
        public static Vector2i wall_Vertical_T 				    = GetTextureCoord(Purpose.Wall_Vertical_T);
        public static Vector2i wall_Vertical_M 				    = GetTextureCoord(Purpose.Wall_Vertical_M);
        public static Vector2i wall_Vertical_B 				    = GetTextureCoord(Purpose.Wall_Vertical_B);
        public static Vector2i wall_Horizontal_L 				= GetTextureCoord(Purpose.Wall_Horizontal_L);
        public static Vector2i wall_Horizontal_M 				= GetTextureCoord(Purpose.Wall_Horizontal_M);
        public static Vector2i wall_Horizontal_R 				= GetTextureCoord(Purpose.Wall_Horizontal_R);

        public static Vector2i wall_Corner_TopRight 			= GetTextureCoord(Purpose.Wall_Corner_TR);
        public static Vector2i wall_Corner_TopLeft 			    = GetTextureCoord(Purpose.Wall_Corner_TL);
        public static Vector2i wall_Corner_BottomRight 		    = GetTextureCoord(Purpose.Wall_Corner_BR);
        public static Vector2i wall_Corner_BottomLeft 			= GetTextureCoord(Purpose.Wall_Corner_BL);
        public static Vector2i wall_Tee_Right 					= GetTextureCoord(Purpose.Wall_Tee_R);
        public static Vector2i wall_Tee_Left 					= GetTextureCoord(Purpose.Wall_Tee_L);
        public static Vector2i wall_Tee_Top 					= GetTextureCoord(Purpose.Wall_Tee_T);
        public static Vector2i wall_Tee_Bottom 				    = GetTextureCoord(Purpose.Wall_Tee_B);

        public static Vector2i wall_Diagonal_TopRight 			= GetTextureCoord(Purpose.Wall_Diagonal_TR);
        public static Vector2i wall_Diagonal_TopRight_T 		= GetTextureCoord(Purpose.Wall_Diagonal_TR_T);
        public static Vector2i wall_Diagonal_TopRight_R 		= GetTextureCoord(Purpose.Wall_Diagonal_TR_R);
        public static Vector2i wall_Diagonal_TopRight_TR 		= GetTextureCoord(Purpose.Wall_Diagonal_TR_TR);
        public static Vector2i wall_Diagonal_TopLeft 			= GetTextureCoord(Purpose.Wall_Diagonal_TL);
        public static Vector2i wall_Diagonal_TopLeft_T 		    = GetTextureCoord(Purpose.Wall_Diagonal_TL_T);
        public static Vector2i wall_Diagonal_TopLeft_L 		    = GetTextureCoord(Purpose.Wall_Diagonal_TL_L);
        public static Vector2i wall_Diagonal_TopLeft_TL 		= GetTextureCoord(Purpose.Wall_Diagonal_TL_TL);
        public static Vector2i wall_Diagonal_BottomRight 		= GetTextureCoord(Purpose.Wall_Diagonal_BR);
        public static Vector2i wall_Diagonal_BottomRight_B 	    = GetTextureCoord(Purpose.Wall_Diagonal_BR_B);
        public static Vector2i wall_Diagonal_BottomRight_R 	    = GetTextureCoord(Purpose.Wall_Diagonal_BR_R);
        public static Vector2i wall_Diagonal_BottomRight_BR 	= GetTextureCoord(Purpose.Wall_Diagonal_BR_BR);
        public static Vector2i wall_Diagonal_BottomLeft 		= GetTextureCoord(Purpose.Wall_Diagonal_BL);
        public static Vector2i wall_Diagonal_BottomLeft_B 		= GetTextureCoord(Purpose.Wall_Diagonal_BL_B);
        public static Vector2i wall_Diagonal_BottomLeft_L 		= GetTextureCoord(Purpose.Wall_Diagonal_BL_L);
        public static Vector2i wall_Diagonal_BottomLeft_BL 	    = GetTextureCoord(Purpose.Wall_Diagonal_BL_BL);

        public static Vector2i wallCornerHider_All 			    = GetTextureCoord(Purpose.WallCornerHider_All);
        public static Vector2i wallCornerHider_TL_BR 			= GetTextureCoord(Purpose.WallCornerHider_TL_BR);
        public static Vector2i wallCornerHider_TR_BL 			= GetTextureCoord(Purpose.WallCornerHider_TR_BL);
        public static Vector2i wallCornerHider_TL 				= GetTextureCoord(Purpose.WallCornerHider_TL);
        public static Vector2i wallCornerHider_TL_TR 			= GetTextureCoord(Purpose.WallCornerHider_TL_TR);
        public static Vector2i wallCornerHider_TL_TR_BR 		= GetTextureCoord(Purpose.WallCornerHider_TL_TR_BR);
        public static Vector2i wallCornerHider_TR 				= GetTextureCoord(Purpose.WallCornerHider_TR);
        public static Vector2i wallCornerHider_TR_BR 			= GetTextureCoord(Purpose.WallCornerHider_TR_BR);
        public static Vector2i wallCornerHider_TR_BR_BL 		= GetTextureCoord(Purpose.WallCornerHider_TR_BR_BL);
        public static Vector2i wallCornerHider_BR 				= GetTextureCoord(Purpose.WallCornerHider_BR);
        public static Vector2i wallCornerHider_BR_BL 			= GetTextureCoord(Purpose.WallCornerHider_BR_BL);
        public static Vector2i wallCornerHider_BR_BL_TL 		= GetTextureCoord(Purpose.WallCornerHider_BR_BL_TL);
        public static Vector2i wallCornerHider_BL 				= GetTextureCoord(Purpose.WallCornerHider_BL);
        public static Vector2i wallCornerHider_BL_TL 			= GetTextureCoord(Purpose.WallCornerHider_BL_TL);
        public static Vector2i wallCornerHider_BL_TL_TR 		= GetTextureCoord(Purpose.WallCornerHider_BL_TL_TR);

        public static TileAnimator.TileAnimation anim_DoorVertical_Open 				= new TileAnimator.TileAnimation(GetTextureCoord(Purpose.DoorVertical).y, 4);
        public static TileAnimator.TileAnimation anim_DoorHorizontal_Open 				= new TileAnimator.TileAnimation(GetTextureCoord(Purpose.DoorHorizontal).y, 4);

        public static TileAnimator.TileAnimation anim_AirlockHorizontal_Open_B_Top 		= new TileAnimator.TileAnimation(GetTextureCoord(Purpose.AirlockHorizontal_Open_B_TOP).y, 4);
        public static TileAnimator.TileAnimation anim_AirlockHorizontal_Open_B_Bottom 	= new TileAnimator.TileAnimation(GetTextureCoord(Purpose.AirlockHorizontal_Open_B).y, 4);
        public static TileAnimator.TileAnimation anim_AirlockHorizontal_Open_T_Top 		= new TileAnimator.TileAnimation(GetTextureCoord(Purpose.AirlockHorizontal_Open_T_TOP).y, 4);
        public static TileAnimator.TileAnimation anim_AirlockHorizontal_Open_T_Bottom 	= new TileAnimator.TileAnimation(GetTextureCoord(Purpose.AirlockHorizontal_Open_T).y, 4);
        public static TileAnimator.TileAnimation anim_AirlockHorizontal_Wait_Top 		= new TileAnimator.TileAnimation(GetTextureCoord(Purpose.AirlockHorizontal_Wait_TOP).y, 8);
        public static TileAnimator.TileAnimation anim_AirlockHorizontal_Wait_Bottom 	= new TileAnimator.TileAnimation(GetTextureCoord(Purpose.AirlockHorizontal_Wait).y, 8);

        public static TileAnimator.TileAnimation anim_AirlockVertical_Open_L_Top 		= new TileAnimator.TileAnimation(GetTextureCoord(Purpose.AirlockVertical_Open_L_TOP).y, 4);
        public static TileAnimator.TileAnimation anim_AirlockVertical_Open_L_Bottom 	= new TileAnimator.TileAnimation(GetTextureCoord(Purpose.AirlockVertical_Open_L).y, 4);
        public static TileAnimator.TileAnimation anim_AirlockVertical_Open_R_Top 		= new TileAnimator.TileAnimation(GetTextureCoord(Purpose.AirlockVertical_Open_R_TOP).y, 4);
        public static TileAnimator.TileAnimation anim_AirlockVertical_Open_R_Bottom 	= new TileAnimator.TileAnimation(GetTextureCoord(Purpose.AirlockVertical_Open_R).y, 4);
        public static TileAnimator.TileAnimation anim_AirlockVertical_Wait_Top 			= new TileAnimator.TileAnimation(GetTextureCoord(Purpose.AirlockVertical_Wait_TOP).y, 8);
        public static TileAnimator.TileAnimation anim_AirlockVertical_Wait_Bottom 		= new TileAnimator.TileAnimation(GetTextureCoord(Purpose.AirlockVertical_Wait).y, 8);

        public Texture2D ShadowMap;

        // note: code for generating these can be found in CachedAssetsEditor.cs
        public PolygonCollider2D wall_Single_shadow;
        public PolygonCollider2D wall_FourWay_shadow;
        public PolygonCollider2D wall_Vertical_T_shadow;
        public PolygonCollider2D wall_Vertical_M_shadow;
        public PolygonCollider2D wall_Vertical_B_shadow;
        public PolygonCollider2D wall_Horizontal_L_shadow;
        public PolygonCollider2D wall_Horizontal_M_shadow;
        public PolygonCollider2D wall_Horizontal_R_shadow;

        public PolygonCollider2D wall_Corner_TopRight_shadow;
        public PolygonCollider2D wall_Corner_TopLeft_shadow;
        public PolygonCollider2D wall_Corner_BottomRight_shadow;
        public PolygonCollider2D wall_Corner_BottomLeft_shadow;
        public PolygonCollider2D wall_Tee_Right_shadow;
        public PolygonCollider2D wall_Tee_Left_shadow;
        public PolygonCollider2D wall_Tee_Top_shadow;
        public PolygonCollider2D wall_Tee_Bottom_shadow;

        public PolygonCollider2D wall_Diagonal_TopRight_shadow;
        public PolygonCollider2D wall_Diagonal_TopRight_T_shadow;
        public PolygonCollider2D wall_Diagonal_TopRight_R_shadow;
        public PolygonCollider2D wall_Diagonal_TopRight_TR_shadow;
        public PolygonCollider2D wall_Diagonal_TopLeft_shadow;
        public PolygonCollider2D wall_Diagonal_TopLeft_T_shadow;
        public PolygonCollider2D wall_Diagonal_TopLeft_L_shadow;
        public PolygonCollider2D wall_Diagonal_TopLeft_TL_shadow;
        public PolygonCollider2D wall_Diagonal_BottomRight_shadow;
        public PolygonCollider2D wall_Diagonal_BottomRight_B_shadow;
        public PolygonCollider2D wall_Diagonal_BottomRight_R_shadow;
        public PolygonCollider2D wall_Diagonal_BottomRight_BR_shadow;
        public PolygonCollider2D wall_Diagonal_BottomLeft_shadow;
        public PolygonCollider2D wall_Diagonal_BottomLeft_B_shadow;
        public PolygonCollider2D wall_Diagonal_BottomLeft_L_shadow;
        public PolygonCollider2D wall_Diagonal_BottomLeft_BL_shadow;

        public PolygonCollider2D anim_DoorVertical_shadow;
        public PolygonCollider2D anim_DoorHorizontal_shadow;
        public PolygonCollider2D anim_AirlockHorizontal_shadow;
        public PolygonCollider2D anim_AirlockVertical_shadow;
    }

    // [System.Serializable]
    // public class MovableCollider { // workaround for moving a collider and raycasting against it on the same frame
    //     public Vector2 WorldPosition;
    //     public ColliderVertices[] Paths;
    //     public MovableCollider() { }
    //     public MovableCollider(int _length) { Paths = new ColliderVertices[_length]; }

    //     public void SetPaths(ColliderVertices[] _p) {
    //         if (_p == null) {
    //             Paths = new ColliderVertices[0];
    //             return;
    //         }

    //         Paths = _p;
    //     }
    //     public void SetPath(int _path, Vector2[] _vertices) {
    //         Paths[_path].Vertices = _vertices;
    //     }
    //     //private int totalCount;
    //     //public int GetTotalPointCount() {
    //     //    totalCount = 0;
    //     //    for (int i = 0; i < Paths.Length; i++)
    //     //        totalCount += Paths[i].Vertices.Length;
    //     //    return totalCount;
    //     //}

    //     private bool intersect = false;
    //     private Vector2 point;
    //     private Vector2[] vertices;
    //     private int j;
    //     private const float VERTEX_HIT_DISTANCE = 0.01f;
    //     public bool OverlapPointOrAlmost(Vector2 _pos, out float closest) {
    //         point = _pos - WorldPosition;
    //         intersect = false;
    //         closest = 10000;
    //         for (int p = 0; p < Paths.Length; p++) {
    //             j = Paths[p].Vertices.Length - 1;
    //             vertices = Paths[p].Vertices;

    //             // is point inside collider?
    //             for (int v = 0; v < vertices.Length; j = v++) {
    //                 // stolen from the internets D:
    //                 if (((vertices[v].y <= point.y && point.y < vertices[j].y) || (vertices[j].y <= point.y && point.y < vertices[v].y)) &&
    //                     (point.x < (vertices[j].x - vertices[v].x) * (point.y - vertices[v].y) / (vertices[j].y - vertices[v].y) + vertices[v].x))
    //                     intersect = !intersect;
    //             }
    //             if (intersect)
    //                 return true;

    //             // is point super-close to any of the vertices?
    //             for (int v = 0; v < vertices.Length; v++) { 
    //                 if((point - vertices[v]).magnitude < closest)
    //                     closest = (point - vertices[v]).magnitude;

    //                 if ((point - vertices[v]).magnitude < VERTEX_HIT_DISTANCE)
    //                     return true;
    //             }
    //         }

    //         return false;
    //     }
    // }
    // [System.Serializable]
    // public class ColliderVertices { // workaround for serializing jagged array
    //     public Vector2[] Vertices;
    //     public ColliderVertices() { }
    //     public ColliderVertices(int _length) { Vertices = new Vector2[_length]; }
    // }

    public WallSet[] WallSets;
    //public PolygonCollider2D ShadowCollider;
    public GameObject TilePrefab;

    [Header("Character Assets")]
    public OrientedAsset[] HairStyles;
    public OrientedAsset[] Heads;
    public OrientedAsset[] Eyes;
    public OrientedAsset[] Beards;


    void Awake() {
        Instance = this;
    }


    private bool isConnected_L;
    private bool isConnected_T;
    private bool isConnected_R;
    private bool isConnected_B;
    public WallSet.Purpose GetTileDefinition(Tile _tile) {
        isConnected_L = false;
        isConnected_T = false;
        isConnected_R = false;
        isConnected_B = false;

        if (_tile._WallType_ != Tile.Type.Empty) {
            isConnected_L = _tile.CanConnect_L && _tile.HasConnectable_L;
            isConnected_T = _tile.CanConnect_T && _tile.HasConnectable_T;
            isConnected_R = _tile.CanConnect_R && _tile.HasConnectable_R;
            isConnected_B = _tile.CanConnect_B && _tile.HasConnectable_B;

            switch (_tile._WallType_) {
                case Tile.Type.Solid:
                    if (isConnected_L) {
                        if (isConnected_T) {
                            if (isConnected_R) {
                                if (isConnected_B) return WallSet.Purpose.Wall_Fourway;
                                else return WallSet.Purpose.Wall_Tee_T;
                            }
                            else if (isConnected_B) return WallSet.Purpose.Wall_Tee_L;
                            else return WallSet.Purpose.Wall_Corner_TL;
                        }
                        else if (isConnected_R) {
                            if (isConnected_B) return WallSet.Purpose.Wall_Tee_B;
                            else return WallSet.Purpose.Wall_Horizontal_M;
                        }
                        else if (isConnected_B) return WallSet.Purpose.Wall_Corner_BL;
                        else return WallSet.Purpose.Wall_Horizontal_R;
                    }
                    else if (isConnected_T) {
                        if (isConnected_R) {
                            if (isConnected_B) return WallSet.Purpose.Wall_Tee_R;
                            else return WallSet.Purpose.Wall_Corner_TR;
                        }
                        else if (isConnected_B) return WallSet.Purpose.Wall_Vertical_M;
                        else return WallSet.Purpose.Wall_Vertical_B;
                    }
                    else if (isConnected_R) {
                        if (isConnected_B) return WallSet.Purpose.Wall_Corner_BR;
                        else return WallSet.Purpose.Wall_Horizontal_L;
                    }
                    else if (isConnected_B) return WallSet.Purpose.Wall_Vertical_T;
                    else return WallSet.Purpose.Wall_Single;

                case Tile.Type.Diagonal:
                    switch (_tile._Orientation_) {
                        case Tile.TileOrientation.TopRight: {
                                if (isConnected_T) {
                                    if (isConnected_R) return WallSet.Purpose.Wall_Diagonal_TR_TR;
                                    else return WallSet.Purpose.Wall_Diagonal_TR_T;
                                }
                                else if (isConnected_R) return WallSet.Purpose.Wall_Diagonal_TR_R;
                                else return WallSet.Purpose.Wall_Diagonal_TR;
                            }

                        case Tile.TileOrientation.TopLeft: {
                                if (isConnected_T) {
                                    if (isConnected_L) return WallSet.Purpose.Wall_Diagonal_TL_TL;
                                    else return WallSet.Purpose.Wall_Diagonal_TL_T;
                                }
                                else if (isConnected_L) return WallSet.Purpose.Wall_Diagonal_TL_L;
                                else return WallSet.Purpose.Wall_Diagonal_TL;
                            }
                        case Tile.TileOrientation.BottomRight: {
                                if (isConnected_B) {
                                    if (isConnected_R) return WallSet.Purpose.Wall_Diagonal_BR_BR;
                                    else return WallSet.Purpose.Wall_Diagonal_BR_B;
                                }
                                else if (isConnected_R) return WallSet.Purpose.Wall_Diagonal_BR_R;
                                else return WallSet.Purpose.Wall_Diagonal_BR;
                            }
                        case Tile.TileOrientation.BottomLeft: {
                                if (isConnected_B) {
                                    if (isConnected_L) return WallSet.Purpose.Wall_Diagonal_BL_BL;
                                    else return WallSet.Purpose.Wall_Diagonal_BL_B;
                                }
                                else if (isConnected_L) return WallSet.Purpose.Wall_Diagonal_BL_L;
                                else return WallSet.Purpose.Wall_Diagonal_BL;
                            }
                        default:
                            throw new System.Exception(_tile._Orientation_ + " is not supported by diagonals!");
                    }
                case Tile.Type.Door:
                    switch (_tile._Orientation_) {
                        case Tile.TileOrientation.None:
                        case Tile.TileOrientation.Bottom:
                        case Tile.TileOrientation.Top:
                            return WallSet.Purpose.DoorVertical;
                        case Tile.TileOrientation.Left:
                        case Tile.TileOrientation.Right:
                            return WallSet.Purpose.DoorHorizontal;
                    }
                    break;
                case Tile.Type.Airlock:
                    switch (_tile._Orientation_) {
                        case Tile.TileOrientation.None:
                        case Tile.TileOrientation.Bottom:
                        case Tile.TileOrientation.Top:
                            return WallSet.Purpose.AirlockVertical_Open_L;
                        case Tile.TileOrientation.Left:
                        case Tile.TileOrientation.Right:
                            return WallSet.Purpose.AirlockHorizontal_Open_T_TOP;
                    }
                    break;
                default:
                    throw new System.NotImplementedException(_tile._Orientation_ + " hasn't been properly implemented yet!");
            }
        }
        else if (_tile._FloorType_ != Tile.Type.Empty) {
            isConnected_L = _tile.CanConnectFloor_L && _tile.HasConnectableFloor_L;
            isConnected_T = _tile.CanConnectFloor_T && _tile.HasConnectableFloor_T;
            isConnected_R = _tile.CanConnectFloor_R && _tile.HasConnectableFloor_R;
            isConnected_B = _tile.CanConnectFloor_B && _tile.HasConnectableFloor_B;

            switch (_tile._FloorType_) {
                case Tile.Type.Solid:
                    if (isConnected_L) {
                        if (isConnected_T) {
                            if (isConnected_R) {
                                if (isConnected_B) return WallSet.Purpose.Floor_Fourway;
                                else return WallSet.Purpose.Floor_Tee_T;
                            }
                            else if (isConnected_B) return WallSet.Purpose.Floor_Tee_L;
                            else return WallSet.Purpose.Floor_Corner_TL;
                        }
                        else if (isConnected_R) {
                            if (isConnected_B) return WallSet.Purpose.Floor_Tee_B;
                            else return WallSet.Purpose.Floor_Horizontal_M;
                        }
                        else if (isConnected_B) return WallSet.Purpose.Floor_Corner_BL;
                        else return WallSet.Purpose.Floor_Horizontal_R;
                    }
                    else if (isConnected_T) {
                        if (isConnected_R) {
                            if (isConnected_B) return WallSet.Purpose.Floor_Tee_R;
                            else return WallSet.Purpose.Floor_Corner_TR;
                        }
                        else if (isConnected_B) return WallSet.Purpose.Floor_Vertical_M;
                        else return WallSet.Purpose.Floor_Vertical_B;
                    }
                    else if (isConnected_R) {
                        if (isConnected_B) return WallSet.Purpose.Floor_Corner_BR;
                        else return WallSet.Purpose.Floor_Horizontal_L;
                    }
                    else if (isConnected_B) return WallSet.Purpose.Floor_Vertical_T;
                    else return WallSet.Purpose.Floor_Single;

                case Tile.Type.Diagonal:
                    switch (_tile._FloorOrientation_) {
                        case Tile.TileOrientation.TopRight: {
                                if (isConnected_T) {
                                    if (isConnected_R) return WallSet.Purpose.Floor_Diagonal_TR_TR;
                                    else return WallSet.Purpose.Floor_Diagonal_TR_T;
                                }
                                else if (isConnected_R) return WallSet.Purpose.Floor_Diagonal_TR_R;
                                else return WallSet.Purpose.Floor_Diagonal_TR;
                            }
                        case Tile.TileOrientation.TopLeft: {
                                if (isConnected_T) {
                                    if (isConnected_L) return WallSet.Purpose.Floor_Diagonal_TL_TL;
                                    else return WallSet.Purpose.Floor_Diagonal_TL_T;
                                }
                                else if (isConnected_L) return WallSet.Purpose.Floor_Diagonal_TL_L;
                                else return WallSet.Purpose.Floor_Diagonal_TL;
                            }
                        case Tile.TileOrientation.BottomRight: {
                                if (isConnected_B) {
                                    if (isConnected_R) return WallSet.Purpose.Floor_Diagonal_BR_BR;
                                    else return WallSet.Purpose.Floor_Diagonal_BR_B;
                                }
                                else if (isConnected_R) return WallSet.Purpose.Floor_Diagonal_BR_R;
                                else return WallSet.Purpose.Floor_Diagonal_BR;
                            }
                        case Tile.TileOrientation.BottomLeft: {
                                if (isConnected_B) {
                                    if (isConnected_L) return WallSet.Purpose.Floor_Diagonal_BL_BL;
                                    else return WallSet.Purpose.Floor_Diagonal_BL_B;
                                }
                                else if (isConnected_L) return WallSet.Purpose.Floor_Diagonal_BL_L;
                                else return WallSet.Purpose.Floor_Diagonal_BL;
                            }
                        default:
                            throw new System.Exception(_tile._FloorOrientation_ + " is not supported by diagonals!");
                    }
                case Tile.Type.Door:
                case Tile.Type.Airlock:
                    throw new System.Exception(_tile._FloorType_.ToString() + " does not apply to Floor!");
                default:
                    throw new System.NotImplementedException(_tile._FloorOrientation_ + " hasn't been properly implemented yet!");
            }
        }

        return WallSet.Purpose.Null;
    }

    // TODO: this should be able to just use the ExactType, saving some processing
    public Vector2i GetWallAssetForTile(Tile.Type _tileType, Tile.TileOrientation _tileOrientation, int _styleIndex, bool _isBottom, bool _hasConnection_Left, bool _hasConnection_Top, bool _hasConnection_Right, bool _hasConnection_Bottom) {
		switch (_tileType) {
            case Tile.Type.Empty:
                return WallSet.Null;
			case Tile.Type.Solid:
                if (!_isBottom) // for now at least
                    return WallSet.Null;

                if (_hasConnection_Left) {
                    if (_hasConnection_Top) {
                        if (_hasConnection_Right) {
                            if (_hasConnection_Bottom) 		return WallSet.wall_FourWay;
                            else 							return WallSet.wall_Tee_Top;
                        }
                        else if (_hasConnection_Bottom) 	return WallSet.wall_Tee_Left;
                        else 								return WallSet.wall_Corner_TopLeft;
                    }
                    else if (_hasConnection_Right) {
                        if (_hasConnection_Bottom) 			return WallSet.wall_Tee_Bottom;
                        else 								return WallSet.wall_Horizontal_M;
                    }
                    else if (_hasConnection_Bottom) 		return WallSet.wall_Corner_BottomLeft;
                    else 									return WallSet.wall_Horizontal_R;
                }
                else if (_hasConnection_Top) {
                    if (_hasConnection_Right) {
                        if (_hasConnection_Bottom) 			return WallSet.wall_Tee_Right;
                        else 								return WallSet.wall_Corner_TopRight;
                    }
                    else if (_hasConnection_Bottom) 		return WallSet.wall_Vertical_M;
                    else 									return WallSet.wall_Vertical_B;
                }
                else if (_hasConnection_Right) {
                    if (_hasConnection_Bottom) 				return WallSet.wall_Corner_BottomRight;
                    else 									return WallSet.wall_Horizontal_L;
                }
                else if (_hasConnection_Bottom) 			return WallSet.wall_Vertical_T;
                else 										return WallSet.wall_Single;

            case Tile.Type.Diagonal:
                switch (_tileOrientation) {
                    case Tile.TileOrientation.TopRight: {
							if (!_isBottom) 						return WallSet.Null;
							else if (_hasConnection_Top) {
								if (_hasConnection_Right) 			return WallSet.wall_Diagonal_TopRight_TR;
                                else 								return WallSet.wall_Diagonal_TopRight_T;
                            }
							else if (_hasConnection_Right) 			return WallSet.wall_Diagonal_TopRight_R;
                            else 									return WallSet.wall_Diagonal_TopRight;
                        }

                    case Tile.TileOrientation.TopLeft: {
							if (!_isBottom)					return WallSet.Null;
							else if (_hasConnection_Top) {
								if (_hasConnection_Left) 	return WallSet.wall_Diagonal_TopLeft_TL;
                                else 						return WallSet.wall_Diagonal_TopLeft_T;
                            }
							else if(_hasConnection_Left) 	return WallSet.wall_Diagonal_TopLeft_L;
                            else 							return WallSet.wall_Diagonal_TopLeft;
                        }
                    case Tile.TileOrientation.BottomRight: {
							if (_isBottom) 					return WallSet.Null;
                            else if (_hasConnection_Bottom) {
                                if (_hasConnection_Right) 	return WallSet.wall_Diagonal_BottomRight_BR;
                                else 						return WallSet.wall_Diagonal_BottomRight_B;
                            }
                            else if (_hasConnection_Right) 	return WallSet.wall_Diagonal_BottomRight_R;
                            else 							return WallSet.wall_Diagonal_BottomRight;
                        }
                    case Tile.TileOrientation.BottomLeft: {
							if (_isBottom) 					return WallSet.Null;
							else if (_hasConnection_Bottom) {
                                if (_hasConnection_Left) 	return WallSet.wall_Diagonal_BottomLeft_BL;
                                else 						return WallSet.wall_Diagonal_BottomLeft_B;
                            }
                            else if (_hasConnection_Left) 	return WallSet.wall_Diagonal_BottomLeft_L;
                            else 							return WallSet.wall_Diagonal_BottomLeft;
                        }
					default:
						throw new System.Exception (_tileOrientation + " is not supported by diagonals!");
                }
            case Tile.Type.Door:
                switch (_tileOrientation) {
                    case Tile.TileOrientation.None:
                    case Tile.TileOrientation.Bottom:
                    case Tile.TileOrientation.Top:
                        return _isBottom ? WallSet.anim_DoorVertical_Open.First : WallSet.Null;
                    case Tile.TileOrientation.Left:
                    case Tile.TileOrientation.Right:
                        return _isBottom ? WallSet.anim_DoorHorizontal_Open.First : WallSet.Null;
                }
                break;
            case Tile.Type.Airlock:
                switch (_tileOrientation) {
                    case Tile.TileOrientation.None:
                    case Tile.TileOrientation.Bottom:
                    case Tile.TileOrientation.Top:
                        return _isBottom ? WallSet.anim_AirlockVertical_Open_L_Bottom.First : WallSet.anim_AirlockVertical_Open_L_Top.First;
                    case Tile.TileOrientation.Left:
                    case Tile.TileOrientation.Right:
                        return _isBottom ? WallSet.anim_AirlockHorizontal_Open_T_Bottom.First : WallSet.anim_AirlockHorizontal_Open_T_Top.First;
                }
                break;
            default:
                throw new System.NotImplementedException(_tileType + " hasn't been properly implemented yet!");
        }

        return CachedAssets.WallSet.Null;
    }
	public Vector2i GetFloorAssetForTile(Tile.Type _tileType, Tile.TileOrientation _tileOrientation, int _styleIndex, bool _hasConnection_Left, bool _hasConnection_Top, bool _hasConnection_Right, bool _hasConnection_Bottom) {
		switch (_tileType) {
			case Tile.Type.Empty:
				return WallSet.Null;
			case Tile.Type.Solid:
				if (_hasConnection_Left) {
					if (_hasConnection_Top) {
						if (_hasConnection_Right) {
							if (_hasConnection_Bottom) 		return WallSet.floor_FourWay;
							else 							return WallSet.floor_Tee_Top;
						}
						else if (_hasConnection_Bottom) 	return WallSet.floor_Tee_Left;
						else 								return WallSet.floor_Corner_TopLeft;
					}
					else if (_hasConnection_Right) {
						if (_hasConnection_Bottom) 			return WallSet.floor_Tee_Bottom;
						else 								return WallSet.floor_Horizontal_M;
					}
					else if (_hasConnection_Bottom) 		return WallSet.floor_Corner_BottomLeft;
					else 									return WallSet.floor_Horizontal_R;
				}
				else if (_hasConnection_Top) {
					if (_hasConnection_Right) {
						if (_hasConnection_Bottom) 			return WallSet.floor_Tee_Right;
						else 								return WallSet.floor_Corner_TopRight;
					}
					else if (_hasConnection_Bottom) 		return WallSet.floor_Vertical_M;
					else 									return WallSet.floor_Vertical_B;
				}
				else if (_hasConnection_Right) {
					if (_hasConnection_Bottom) 				return WallSet.floor_Corner_BottomRight;
					else 									return WallSet.floor_Horizontal_L;
				}
				else if (_hasConnection_Bottom) 			return WallSet.floor_Vertical_T;
				else 										return WallSet.floor_Single;

            case Tile.Type.Diagonal:
                switch (_tileOrientation) {
                    case Tile.TileOrientation.TopRight: {
                            if (_hasConnection_Top) {
                                if (_hasConnection_Right) 	return WallSet.floor_Diagonal_TopRight_TR;
                                else 						return WallSet.floor_Diagonal_TopRight_T;
                            }
                            else if (_hasConnection_Right) 	return WallSet.floor_Diagonal_TopRight_R;
                            else 							return WallSet.floor_Diagonal_TopRight;
                        }

                    case Tile.TileOrientation.TopLeft: {
                            if (_hasConnection_Top) {
                                if (_hasConnection_Left) 	return WallSet.floor_Diagonal_TopLeft_TL;
                                else 						return WallSet.floor_Diagonal_TopLeft_T;
                            }
                            else if (_hasConnection_Left) 	return WallSet.floor_Diagonal_TopLeft_L;
                            else 							return WallSet.floor_Diagonal_TopLeft;
                        }
                    case Tile.TileOrientation.BottomRight: {
                            if (_hasConnection_Bottom) {
                                if (_hasConnection_Right) 	return  WallSet.floor_Diagonal_BottomRight_BR;
                                else 						return  WallSet.floor_Diagonal_BottomRight_B;
                            }
                            else if (_hasConnection_Right) 	return  WallSet.floor_Diagonal_BottomRight_R;
                            else 							return  WallSet.floor_Diagonal_BottomRight;
                        }
                    case Tile.TileOrientation.BottomLeft: {
                            if (_hasConnection_Bottom) {
                                if (_hasConnection_Left) 	return  WallSet.floor_Diagonal_BottomLeft_BL;
                                else 						return  WallSet.floor_Diagonal_BottomLeft_B;
                            }
                            else if (_hasConnection_Left) 	return  WallSet.floor_Diagonal_BottomLeft_L;
                            else 							return  WallSet.floor_Diagonal_BottomLeft;
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
    public Vector2i GetWallCornerAsset(bool _TL, bool _TR, bool _BR, bool _BL) {
        if (_TL) {
            if (_TR) {
                if (_BR) {
                    if (_BL) 	return WallSet.wallCornerHider_All;
					else 		return WallSet.wallCornerHider_TL_TR_BR;
                }
                else if (_BL)	return WallSet.wallCornerHider_BL_TL_TR;
                else 			return WallSet.wallCornerHider_TL_TR;
            }
            else if (_BR) {
                if (_BL)		return WallSet.wallCornerHider_BR_BL_TL;
				else 			return WallSet.wallCornerHider_TL_BR;
            }
            else if (_BL)		return WallSet.wallCornerHider_BL_TL;
			else 				return WallSet.wallCornerHider_TL;
        }
        else if (_TR) {
            if (_BR) {
                if (_BL) 		return WallSet.wallCornerHider_TR_BR_BL;
				else 			return WallSet.wallCornerHider_TR_BR;
            }
            else if (_BL)		return WallSet.wallCornerHider_TR_BL;
			else 				return WallSet.wallCornerHider_TR;
        }
        else if (_BR) {
            if (_BL)			return WallSet.wallCornerHider_BR_BL;
			else 				return WallSet.wallCornerHider_BR;
        }
        else if (_BL)			return WallSet.wallCornerHider_BL;
		else 					return WallSet.Null;
    }
    public Vector2i GetFloorCornerAsset(bool _TL, bool _TR, bool _BR, bool _BL) {
        if (_TL) {
            if (_TR) {
                if (_BR) {
                    if (_BL)	return WallSet.floorCornerHider_All;
					else 		return WallSet.floorCornerHider_TL_TR_BR;
                }
                else if (_BL)	return WallSet.floorCornerHider_BL_TL_TR;
				else 			return WallSet.floorCornerHider_TL_TR;
            }
            else if (_BR) {
                if (_BL) 		return WallSet.floorCornerHider_BR_BL_TL;
				else 			return WallSet.floorCornerHider_TL_BR;
            }
            else if (_BL)		return WallSet.floorCornerHider_BL_TL;
			else 				return WallSet.floorCornerHider_TL;
        }
        else if (_TR) {
            if (_BR) {
                if (_BL) 		return WallSet.floorCornerHider_TR_BR_BL;
				else 			return WallSet.floorCornerHider_TR_BR;
            }
            else if (_BL) 		return WallSet.floorCornerHider_TR_BL;
			else 				return WallSet.floorCornerHider_TR;
        }
        else if (_BR) {
            if (_BL) 			return WallSet.floorCornerHider_BR_BL;
			else 				return WallSet.floorCornerHider_BR;
        }
        else if(_BL) 			return WallSet.floorCornerHider_BL;
		else 					return WallSet.Null;
    }
}
