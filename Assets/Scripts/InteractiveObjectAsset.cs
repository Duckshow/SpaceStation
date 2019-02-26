using UnityEngine;

[CreateAssetMenu(fileName = "New InteractiveObjectAsset.asset", menuName = "New InteractiveObjectAsset")]
public class InteractiveObjectAsset : ScriptableObject {

	public bool IsWalkable;
	public float WaitTime;
	public Node.InteractiveObject.State OnCharacterIsLeft;
	public Node.InteractiveObject.State OnCharacterIsRight;
	public Node.InteractiveObject.State OnCharacterIsAbove;
	public Node.InteractiveObject.State OnCharacterIsBelow;
	public CachedAssets.RotatableAssetPos AssetPositions;


	public Int2 GetTileAssetPosForDefaultState(Sorting _sorting, Rotation _rotation, Direction _direction) {
		Int2 _assetPosBL = AssetPositions.GetAssetPos(_rotation);

		Int2 _offset = new Int2();
		switch (_direction){
			case Direction.TR:
				break;
			case Direction.TL:
				_offset.Set(1, 0);
				break;
			case Direction.BR:
				_offset.Set(0, 1);
				break;
			case Direction.BL:
				_offset.Set(1, 1);
				break;
			default:
				Debug.LogError(_direction + " hasn't been properly implemented yet!");
				break;
		}

		switch (_sorting){
			case Sorting.Back:
				break;
			case Sorting.Front:
				_offset += new Int2(0, 2);
				break;
			default:
				Debug.LogError(_sorting + " hasn't been properly implemented yet!");
				break;
		}

		// Debug.Log(_direction + ", " + _sorting + ": " + _assetPosBL + ", " + _offset);
		return _assetPosBL + _offset;
	}

	public Int2 GetTileAssetPosForOpenLeftOrBelow(Sorting _sorting, Rotation _rotation, Direction _direction){
		Int2 _assetPos = GetTileAssetPosForDefaultState(_sorting, _rotation, _direction);
		return _assetPos + new Int2(2, 0);
	}

	public Int2 GetTileAssetPosForOpenRightOrAbove(Sorting _sorting, Rotation _rotation, Direction _direction) {
		Int2 _assetPos = GetTileAssetPosForDefaultState(_sorting, _rotation, _direction);
		return _assetPos + new Int2(4, 0);
	}
}
