using UnityEngine;
using System;
using System.Collections.Generic;

public class CachedAssets : MonoBehaviour {

	[Serializable]
	public class RotatableAssetPos {
		public Int2 Up;
		public Int2 Down;
		public Int2 Left;
		public Int2 Right;

		public Int2 GetAssetPos(Rotation _rotation) {
			switch (_rotation){
				case Rotation.Up: return Up;
				case Rotation.Down: return Down;
				case Rotation.Left: return Left;
				case Rotation.Right: return Right;
				default:
					Debug.LogError(_rotation + " hasn't been properly implemented yet!");
					return Int2.MinusOne;
			}
		}
	}

	[CreateAssetMenu(fileName = "New AssetCollection.asset", menuName = "New AssetCollection")]
	public class AssetCollection : ScriptableObject {
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
	}

    [System.Serializable]
    public struct OrientedAsset {
        [SerializeField] private Sprite Back;
        [SerializeField] private Sprite Front;
        [SerializeField] private Sprite Left;
        [SerializeField] private Sprite Right;

        public Sprite GetOrientedAsset(CharacterOrienter.OrientationEnum _orientation) {
            switch (_orientation) {
                case CharacterOrienter.OrientationEnum.Down:
                    return Front;
                case CharacterOrienter.OrientationEnum.Up:
                    return Back;
                case CharacterOrienter.OrientationEnum.Left:
                    return Left;
                case CharacterOrienter.OrientationEnum.Right:
                    return Right;
            }

            return null;
        }
    }

	public static CachedAssets Instance;

	public Material MaterialGrid;
	public GameObject TilePrefab;

	public AssetCollection DefaultAssets;

	[Header("Character Assets")]
    public OrientedAsset[] HairStyles;
    public OrientedAsset[] Heads;
    public OrientedAsset[] Eyes;
    public OrientedAsset[] Beards;

	private const string PROPERTY_TEXTURESIZEX = "TextureSizeX";
	private const string PROPERTY_TEXTURESIZEY = "TextureSizeY";


	void Awake() {
        Instance = this;
		MaterialGrid.SetInt(PROPERTY_TEXTURESIZEX, DefaultAssets.SpriteSheet.width);
		MaterialGrid.SetInt(PROPERTY_TEXTURESIZEY, DefaultAssets.SpriteSheet.height);
    }

	public Int2 GetWallAsset(Int2 _tileGridPos, Sorting _sorting) {
		Node _nodeTL, _nodeTR, _nodeBR, _nodeBL;
		NeighborFinder.GetSurroundingNodes(_tileGridPos, out _nodeTL, out _nodeTR, out _nodeBR, out _nodeBL);

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

		bool _useIsWallTemporaryTL = _hasNodeTL && _nodeTL.UseIsWallTemporary;
		bool _useIsWallTemporaryTR = _hasNodeTR && _nodeTR.UseIsWallTemporary;
		bool _useIsWallTemporaryBR = _hasNodeBR && _nodeBR.UseIsWallTemporary;
		bool _useIsWallTemporaryBL = _hasNodeBL && _nodeBL.UseIsWallTemporary;

		bool _isEitherKindOfWallTL = _isWallTL || (_useIsWallTemporaryTL && _isWallTemporaryTL);
		bool _isEitherKindOfWallTR = _isWallTR || (_useIsWallTemporaryTR && _isWallTemporaryTR);
		bool _isEitherKindOfWallBR = _isWallBR || (_useIsWallTemporaryBR && _isWallTemporaryBR);
		bool _isEitherKindOfWallBL = _isWallBL || (_useIsWallTemporaryBL && _isWallTemporaryBL);

		bool _isInsideRoom = false;
		_isInsideRoom = _isInsideRoom || _hasNodeTL && RoomManager.GetInstance().IsInsideShip(_nodeTL.RoomIndex);
		_isInsideRoom = _isInsideRoom || _hasNodeTR && RoomManager.GetInstance().IsInsideShip(_nodeTR.RoomIndex);
		_isInsideRoom = _isInsideRoom || _hasNodeBR && RoomManager.GetInstance().IsInsideShip(_nodeBR.RoomIndex);
		_isInsideRoom = _isInsideRoom || _hasNodeBL && RoomManager.GetInstance().IsInsideShip(_nodeBL.RoomIndex);

		if (!_isInsideRoom){
			return DefaultAssets.Empty;
		}
		else if (_isEitherKindOfWallTL && _isEitherKindOfWallTR && _isEitherKindOfWallBR && _isEitherKindOfWallBL){
			return DefaultAssets.Wall_TL_TR_BR_BL;
		}
		else if (_isEitherKindOfWallTL && _isEitherKindOfWallTR && _isEitherKindOfWallBR){
			return DefaultAssets.Wall_TL_TR_BR;
		}
		else if (_isEitherKindOfWallTR && _isEitherKindOfWallBR && _isEitherKindOfWallBL){
			return DefaultAssets.Wall_TR_BR_BL;
		}
		else if (_isEitherKindOfWallBR && _isEitherKindOfWallBL && _isEitherKindOfWallTL){
			return DefaultAssets.Wall_BR_BL_TL;
		}
		else if (_isEitherKindOfWallBL && _isEitherKindOfWallTL && _isEitherKindOfWallTR){
			return DefaultAssets.Wall_BL_TL_TR;
		}
		else if (_isEitherKindOfWallTL && _isEitherKindOfWallTR){
			return DefaultAssets.Wall_TL_TR;
		}
		else if (_isEitherKindOfWallTR && _isEitherKindOfWallBR){
			return DefaultAssets.Wall_TR_BR;
		}
		else if (_isEitherKindOfWallBR && _isEitherKindOfWallBL){
			return DefaultAssets.Wall_BR_BL;
		}
		else if (_isEitherKindOfWallBL && _isEitherKindOfWallTL){
			return DefaultAssets.Wall_BL_TL;
		}
		else if (_isEitherKindOfWallTL && _isEitherKindOfWallBR){
			return DefaultAssets.Wall_TL_BR;
		}
		else if (_isEitherKindOfWallTR && _isEitherKindOfWallBL){
			return DefaultAssets.Wall_TR_BL;
		}
		else if (_isEitherKindOfWallTL){
			return DefaultAssets.Wall_TL;
		}
		else if (_isEitherKindOfWallTR){
			return DefaultAssets.Wall_TR;
		}
		else if (_isEitherKindOfWallBR){
			return DefaultAssets.Wall_BR;
		}
		else if (_isEitherKindOfWallBL){
			return DefaultAssets.Wall_BL;
		}
		else {
			return DefaultAssets.Wall_None;
		}
	}

	public Int2 GetInteractiveAsset(Int2 _tileGridPos, Sorting _sorting) {
		Node _nodeTL, _nodeTR, _nodeBR, _nodeBL;
		NeighborFinder.GetSurroundingNodes(_tileGridPos, out _nodeTL, out _nodeTR, out _nodeBR, out _nodeBL);

		bool _hasNodeTL = _nodeTL != null;
		bool _hasNodeTR = _nodeTR != null;
		bool _hasNodeBR = _nodeBR != null;
		bool _hasNodeBL = _nodeBL != null;

		Node.InteractiveObject _interactiveObjectTL = _hasNodeTL ? _nodeTL.AttachedInteractiveObject : null;
		Node.InteractiveObject _interactiveObjectTR = _hasNodeTR ? _nodeTR.AttachedInteractiveObject : null;
		Node.InteractiveObject _interactiveObjectBR = _hasNodeBR ? _nodeBR.AttachedInteractiveObject : null;
		Node.InteractiveObject _interactiveObjectBL = _hasNodeBL ? _nodeBL.AttachedInteractiveObject : null;

		Node.InteractiveObject _interactiveObjectTemporaryTL = _hasNodeTL ? _nodeTL.AttachedInteractiveObjectTemporary : null;
		Node.InteractiveObject _interactiveObjectTemporaryTR = _hasNodeTR ? _nodeTR.AttachedInteractiveObjectTemporary : null;
		Node.InteractiveObject _interactiveObjectTemporaryBR = _hasNodeBR ? _nodeBR.AttachedInteractiveObjectTemporary : null;
		Node.InteractiveObject _interactiveObjectTemporaryBL = _hasNodeBL ? _nodeBL.AttachedInteractiveObjectTemporary : null;

		bool _useInteractiveObjectTemporaryTL = _hasNodeTL && _nodeTL.UseAttachedInteractiveObjectTemporary;
		bool _useInteractiveObjectTemporaryTR = _hasNodeTR && _nodeTR.UseAttachedInteractiveObjectTemporary;
		bool _useInteractiveObjectTemporaryBR = _hasNodeBR && _nodeBR.UseAttachedInteractiveObjectTemporary;
		bool _useInteractiveObjectTemporaryBL = _hasNodeBL && _nodeBL.UseAttachedInteractiveObjectTemporary;

		Node.InteractiveObject _foundInteractive = null;
		Direction _foundInteractiveDirection = Direction.None;
		Node _foundInteractivesNode = null;

		if (TryGetInteractiveObject(_nodeTL, out _foundInteractive)){
			_foundInteractiveDirection = Direction.TL;
			_foundInteractivesNode = _nodeTL;
		}
		else if (TryGetInteractiveObject(_nodeTR, out _foundInteractive)){
			_foundInteractiveDirection = Direction.TR;
			_foundInteractivesNode = _nodeTR;
		}
		else if (TryGetInteractiveObject(_nodeBR, out _foundInteractive)){
			_foundInteractiveDirection = Direction.BR;
			_foundInteractivesNode = _nodeBR;
		}
		else if (TryGetInteractiveObject(_nodeBL, out _foundInteractive)){
			_foundInteractiveDirection = Direction.BL;
			_foundInteractivesNode = _nodeBL;
		}

		if (_foundInteractive != null){
			return _foundInteractive.GetTileAssetPos(_sorting, _foundInteractiveDirection);
		}

		return DefaultAssets.Empty;
	}

	bool TryGetInteractiveObject(Node _node, out Node.InteractiveObject _foundInteractive) {
		if (_node == null){
			_foundInteractive = null;
			return false;
		}

		_foundInteractive = _node.UseAttachedInteractiveObjectTemporary ? _node.AttachedInteractiveObjectTemporary : _node.AttachedInteractiveObject;
		return _foundInteractive != null;
	}
}
