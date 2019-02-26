using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampManager : Singleton<LampManager> {
	private List<Lamp> allLamps = new List<Lamp>();

	private Queue<Int2> nodeGridPositionsNeedingUpdate = new Queue<Int2>();


	public void OnLampAwake(Lamp _lamp) { 
		allLamps.Add(_lamp);
	}

	public void OnLampDestroy(Lamp _lamp) {
		allLamps.Remove(_lamp);
	}

	public void OnLampTurnOn(Lamp _lamp){
		TryAddNodeToUpdate(_lamp.GetNode());
	}

	public void OnLampTurnOff(Lamp _lamp) {
		TryAddNodeToUpdate(_lamp.GetNode());
	}

	public void TryAddNodeToUpdate(Node _node) {
		if (_node == null){
			return;
		} 
		if (nodeGridPositionsNeedingUpdate.Contains(_node.GridPos)){
			return;
		}

		nodeGridPositionsNeedingUpdate.Enqueue(_node.GridPos);
	}

	public override bool IsUsingUpdateLate() { return true; }
	public override void UpdateLate(){
		Queue<int> _roomsNeedingUpdate = new Queue<int>();
		while (nodeGridPositionsNeedingUpdate.Count > 0){
			Int2 _nodeGridPos = nodeGridPositionsNeedingUpdate.Dequeue();
			Node _node = GameGrid.GetInstance().TryGetNode(_nodeGridPos);
			if (_node == null || _roomsNeedingUpdate.Contains(_node.RoomIndex)){
				continue;
			}

			_roomsNeedingUpdate.Enqueue(_node.RoomIndex);
		}

		while (_roomsNeedingUpdate.Count > 0){
			int _roomIndex = _roomsNeedingUpdate.Dequeue();

			RoomManager.Room _room = RoomManager.GetInstance().GetRoom(_roomIndex);
			if (_room == null){
				continue;
			}

			for (int _nodeIndex = 0; _nodeIndex < _room.NodeGridPositions.Count; _nodeIndex++){
				Node _node = GameGrid.GetInstance().TryGetNode(_room.NodeGridPositions[_nodeIndex]);

				Color32 _lightColor = new Color32(0, 0, 0, 0);
				for (int _lampIndex = 0; _lampIndex < _room.Lamps.Count; _lampIndex++){
					Lamp _lamp = _room.Lamps[_lampIndex];
					if (_node.RoomIndex != _lamp.GetRoomIndex()){
						Debug.LogErrorFormat("Somehow Node({0}) found {1} despite being in different rooms ({2} and {3})!", _node.GridPos, _lamp.name, _node.RoomIndex, _lamp.GetRoomIndex());
						continue;
					}

					Int2 _distance = _node.GridPos - _lamp.GetNode().GridPos;
					if (Mathf.Abs(_distance.x) > _lamp.GetRadius() || Mathf.Abs(_distance.y) > _lamp.GetRadius()){
						continue;
					}

					float _pathLength;
					bool _hasFoundPath = Task.FindPath.TryGetPathLength(_node, _lamp.GetNode(), out _pathLength);
					if (!_hasFoundPath){
						continue;
					}
					if (_pathLength > _lamp.GetRadius()){
						continue;
					}

					float _lightFromLamp = 1.0f - _pathLength / (float)_lamp.GetRadius();

					_lightColor = new Color32(
						(byte)(_lamp.GetColor().r * _lightFromLamp),
						(byte)(_lamp.GetColor().g * _lightFromLamp),
						(byte)(_lamp.GetColor().b * _lightFromLamp),
						(byte)(255 * _lightFromLamp)
					);
				}

				_node.SetLighting(_lightColor);
			}

			for (int _wallIndex = 0; _wallIndex < _room.WallNodeGridPositions.Count; _wallIndex++){
				Int2 _nodeGridPos = _room.WallNodeGridPositions[_wallIndex];
				Node _wall = GameGrid.GetInstance().TryGetNode(_nodeGridPos);
				_wall.SetLightingBasedOnNeighbors();
			}
		}
	}
}
