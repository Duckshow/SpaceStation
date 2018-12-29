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

	public Material MaterialGrid;
	public GameObject TilePrefab;

	[CreateAssetMenu(fileName = "New AssetSet.asset", menuName = "New AssetSet")]
	public class AssetSet : ScriptableObject {

		[Serializable]
		public class SortingSet {
			public Int2 Bottom;
			public Int2 Top;
		}

		[Serializable]
		public class DoorSet {
			public SortingSet Closed;
			public SortingSet OpenBelow;
			public SortingSet OpenAbove;
		}

		public Texture2D SpriteSheet;

		public Int2 Empty;
		public Int2 Wall_None;

		public Int2 Wall_TL;
		public Int2 Wall_TL_TR;
		public Int2 Wall_TL_TR_BR;
		[Space]
		public Int2 Wall_TR;
		public Int2 Wall_TR_BR;
		public Int2 Wall_TR_BR_BL;
		[Space]
		public Int2 Wall_BR;
		public Int2 Wall_BR_BL;
		public Int2 Wall_BR_BL_TL;
		[Space]
		public Int2 Wall_BL;
		public Int2 Wall_BL_TL;
		public Int2 Wall_BL_TL_TR;
		[Space]
		public Int2 Wall_TL_TR_BR_BL;
		public Int2 Wall_TR_BL;
		public Int2 Wall_TL_BR;
		[Space]
		public DoorSet DoorHorizontal;
		public DoorSet DoorVertical;
		public DoorSet AirlockHorizontal;
		public DoorSet AirlockVertical;
	}

	public AssetSet[] AssetSets;

	[Header("Character Assets")]
    public OrientedAsset[] HairStyles;
    public OrientedAsset[] Heads;
    public OrientedAsset[] Eyes;
    public OrientedAsset[] Beards;
	

    void Awake() {
        Instance = this;
    }

	public Int2 GetWallAsset(bool _isWallTL, bool _isWallTR, bool _isWallBR, bool _isWallBL) {
		int assetSetIndex = 0;

		if (_isWallTL && _isWallTR && _isWallBR && _isWallBL){
			return AssetSets[assetSetIndex].Wall_TL_TR_BR_BL;
		}
		else if (_isWallTL && _isWallTR && _isWallBR){
			return AssetSets[assetSetIndex].Wall_TL_TR_BR;
		}
		else if (_isWallTR && _isWallBR && _isWallBL){
			return AssetSets[assetSetIndex].Wall_TR_BR_BL;
		}
		else if (_isWallBR && _isWallBL && _isWallTL){
			return AssetSets[assetSetIndex].Wall_BR_BL_TL;
		}
		else if (_isWallBL && _isWallTL && _isWallTR){
			return AssetSets[assetSetIndex].Wall_BL_TL_TR;
		}
		else if (_isWallTL && _isWallTR){
			return AssetSets[assetSetIndex].Wall_TL_TR;
		}
		else if (_isWallTR && _isWallBR){
			return AssetSets[assetSetIndex].Wall_TR_BR;
		}
		else if (_isWallBR && _isWallBL){
			return AssetSets[assetSetIndex].Wall_BR_BL;
		}
		else if (_isWallBL && _isWallTL){
			return AssetSets[assetSetIndex].Wall_BL_TL;
		}
		else if (_isWallTL && _isWallBR){
			return AssetSets[assetSetIndex].Wall_TL_BR;
		}
		else if (_isWallTR && _isWallBL){
			return AssetSets[assetSetIndex].Wall_TR_BL;
		}
		else if (_isWallTL){
			return AssetSets[assetSetIndex].Wall_TL;
		}
		else if (_isWallTR){
			return AssetSets[assetSetIndex].Wall_TR;
		}
		else if (_isWallBR){
			return AssetSets[assetSetIndex].Wall_BR;
		}
		else if (_isWallBL){
			return AssetSets[assetSetIndex].Wall_BL;
		}
		else{
			return AssetSets[assetSetIndex].Wall_None;
		}
	}
}
