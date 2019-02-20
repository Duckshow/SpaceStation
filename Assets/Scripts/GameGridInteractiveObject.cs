using UnityEngine;

[CreateAssetMenu(fileName = "New GameGridInteractiveObject.asset", menuName = "New GameGridInteractiveObject")]
public class GameGridInteractiveObject : ScriptableObject {
	public CachedAssets.RotatableAssetPos Assets;
	public bool IsWalkable { get; private set; }
	public float WaitTime { get; private set; }

	public Int2 GetTileAssetPosForDefaultState(Sorting _sorting, Rotation _rotation, NeighborEnum _direction) {
		Int2 _assetPosBL = Assets.GetAssetPos(_rotation);

		Int2 _offset = new Int2();
		switch (_direction){
			case NeighborEnum.TR:
				break;
			case NeighborEnum.TL:
				_offset.Set(1, 0);
				break;
			case NeighborEnum.BR:
				_offset.Set(0, 1);
				break;
			case NeighborEnum.BL:
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

	public Int2 GetTileAssetPosForAlternativeState00(Sorting _sorting, Rotation _rotation, NeighborEnum _direction){
		Int2 _assetPos = GetTileAssetPosForDefaultState(_sorting, _rotation, _direction);
		return _assetPos + new Int2(2, 0);
	}

	public Int2 GetTileAssetPosForAlternativeState01(Sorting _sorting, Rotation _rotation, NeighborEnum _direction) {
		Int2 _assetPos = GetTileAssetPosForDefaultState(_sorting, _rotation, _direction);
		return _assetPos + new Int2(4, 0);
	}
}
