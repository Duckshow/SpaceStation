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

	public Int2 GetWallAsset(Int2 _tileGridPos, out bool _isAnyWallTemporary, out bool _isAnyUsingIsWallTemporary) {
		int assetSetIndex = 0;

		Node _nodeTL, _nodeTR, _nodeBR, _nodeBL;
		GameGrid.NeighborFinder.GetSurroundingNodes(_tileGridPos, out _nodeTL, out _nodeTR, out _nodeBR, out _nodeBL);

		bool _hasNodeTL = _nodeTL != null;
		bool _hasNodeTR = _nodeTR != null;
		bool _hasNodeBR = _nodeBR != null;
		bool _hasNodeBL = _nodeBL != null;

		bool _isWallTL = _hasNodeTL && _nodeTL.IsWall;
		bool _isWallTR = _hasNodeTR && _nodeTR.IsWall;
		bool _isWallBR = _hasNodeBR && _nodeBR.IsWall;
		bool _isWallBL = _hasNodeBL && _nodeBL.IsWall;

		bool _isWallTemporaryTL = _hasNodeTL && _nodeTL.IsWallTemporarily;
		bool _isWallTemporaryTR = _hasNodeTR && _nodeTR.IsWallTemporarily;
		bool _isWallTemporaryBR = _hasNodeBR && _nodeBR.IsWallTemporarily;
		bool _isWallTemporaryBL = _hasNodeBL && _nodeBL.IsWallTemporarily;
		_isAnyWallTemporary = _isWallTemporaryTL || _isWallTemporaryTR || _isWallTemporaryBR || _isWallTemporaryBL;

		bool _useIsWallTemporaryTL = _hasNodeTL && _nodeTL.UseIsWallTemporary;
		bool _useIsWallTemporaryTR = _hasNodeTR && _nodeTR.UseIsWallTemporary;
		bool _useIsWallTemporaryBR = _hasNodeBR && _nodeBR.UseIsWallTemporary;
		bool _useIsWallTemporaryBL = _hasNodeBL && _nodeBL.UseIsWallTemporary;
		_isAnyUsingIsWallTemporary = _useIsWallTemporaryTL || _useIsWallTemporaryTR || _useIsWallTemporaryBR || _useIsWallTemporaryBL;

		bool _isEitherKindOfWallTL = _isWallTL || (_useIsWallTemporaryTL && _isWallTemporaryTL);
		bool _isEitherKindOfWallTR = _isWallTR || (_useIsWallTemporaryTR && _isWallTemporaryTR);
		bool _isEitherKindOfWallBR = _isWallBR || (_useIsWallTemporaryBR && _isWallTemporaryBR);
		bool _isEitherKindOfWallBL = _isWallBL || (_useIsWallTemporaryBL && _isWallTemporaryBL);

		bool _isInsideRoom = false;
		_isInsideRoom = _isInsideRoom || _hasNodeTL && RoomManager.GetInstance().IsInsideShip(_nodeTL.RoomIndex);
		_isInsideRoom = _isInsideRoom || _hasNodeTR && RoomManager.GetInstance().IsInsideShip(_nodeTR.RoomIndex);
		_isInsideRoom = _isInsideRoom || _hasNodeBR && RoomManager.GetInstance().IsInsideShip(_nodeBR.RoomIndex);
		_isInsideRoom = _isInsideRoom || _hasNodeBL && RoomManager.GetInstance().IsInsideShip(_nodeBL.RoomIndex);

		// bool _isBuildingAllowedTL = _hasNodeTL && _nodeTL.IsBuildingAllowed;
		// bool _isBuildingAllowedTR = _hasNodeTR && _nodeTR.IsBuildingAllowed;
		// bool _isBuildingAllowedBR = _hasNodeBR && _nodeBR.IsBuildingAllowed;
		// bool _isBuildingAllowedBL = _hasNodeBL && _nodeBL.IsBuildingAllowed;
		// bool _isBuildingAllowed = _isBuildingAllowedTL && _isBuildingAllowedTR && _isBuildingAllowedBR && _isBuildingAllowedBL;

		if (!_isInsideRoom){
			return AssetSets[assetSetIndex].Empty;
		}
		else if (_isEitherKindOfWallTL && _isEitherKindOfWallTR && _isEitherKindOfWallBR && _isEitherKindOfWallBL){
			return AssetSets[assetSetIndex].Wall_TL_TR_BR_BL;
		}
		else if (_isEitherKindOfWallTL && _isEitherKindOfWallTR && _isEitherKindOfWallBR){
			return AssetSets[assetSetIndex].Wall_TL_TR_BR;
		}
		else if (_isEitherKindOfWallTR && _isEitherKindOfWallBR && _isEitherKindOfWallBL){
			return AssetSets[assetSetIndex].Wall_TR_BR_BL;
		}
		else if (_isEitherKindOfWallBR && _isEitherKindOfWallBL && _isEitherKindOfWallTL){
			return AssetSets[assetSetIndex].Wall_BR_BL_TL;
		}
		else if (_isEitherKindOfWallBL && _isEitherKindOfWallTL && _isEitherKindOfWallTR){
			return AssetSets[assetSetIndex].Wall_BL_TL_TR;
		}
		else if (_isEitherKindOfWallTL && _isEitherKindOfWallTR){
			return AssetSets[assetSetIndex].Wall_TL_TR;
		}
		else if (_isEitherKindOfWallTR && _isEitherKindOfWallBR){
			return AssetSets[assetSetIndex].Wall_TR_BR;
		}
		else if (_isEitherKindOfWallBR && _isEitherKindOfWallBL){
			return AssetSets[assetSetIndex].Wall_BR_BL;
		}
		else if (_isEitherKindOfWallBL && _isEitherKindOfWallTL){
			return AssetSets[assetSetIndex].Wall_BL_TL;
		}
		else if (_isEitherKindOfWallTL && _isEitherKindOfWallBR){
			return AssetSets[assetSetIndex].Wall_TL_BR;
		}
		else if (_isEitherKindOfWallTR && _isEitherKindOfWallBL){
			return AssetSets[assetSetIndex].Wall_TR_BL;
		}
		else if (_isEitherKindOfWallTL){
			return AssetSets[assetSetIndex].Wall_TL;
		}
		else if (_isEitherKindOfWallTR){
			return AssetSets[assetSetIndex].Wall_TR;
		}
		else if (_isEitherKindOfWallBR){
			return AssetSets[assetSetIndex].Wall_BR;
		}
		else if (_isEitherKindOfWallBL){
			return AssetSets[assetSetIndex].Wall_BL;
		}
		else {
			return AssetSets[assetSetIndex].Wall_None;
		}
	}
}
