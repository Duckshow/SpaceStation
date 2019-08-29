using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LampManager : Singleton<LampManager> {
	private struct NodeDetails {
		public bool HasDirtyLighting { get; private set; }
		public bool HasDirtyIncandescence { get; private set; }
		
		public NodeDetails(bool? _hasDirtyLighting, bool? _hasDirtyIncandescence) {
			HasDirtyLighting = false; 
			HasDirtyIncandescence = false;
			
			SetValues(_hasDirtyLighting, _hasDirtyIncandescence);
		}

		public void SetValues(bool? _hasDirtyLighting, bool? _hasDirtyIncandescence) {
			if (_hasDirtyLighting.HasValue) {
				HasDirtyLighting = _hasDirtyLighting.Value;
			}

			if (_hasDirtyIncandescence.HasValue) {
				HasDirtyIncandescence = _hasDirtyIncandescence.HasValue;
			}
		}
	}

	private List<Lamp> allLamps = new List<Lamp>();
	private Dictionary<Int2, NodeDetails> nodesToUpdate = new Dictionary<Int2, NodeDetails>();
	private List<Int2> wallNodesToUpdate = new List<Int2>();


	public void OnLampAwake(Lamp _lamp) { 
		allLamps.Add(_lamp);
	}

	public void OnLampDestroy(Lamp _lamp) {
		allLamps.Remove(_lamp);
	}

	public void OnLampTurnOn(Lamp _lamp){
		SetNodeLightingDirty(_lamp.GetGridPosition());
	}

	public void OnLampTurnOff(Lamp _lamp) {
		SetNodeLightingDirty(_lamp.GetGridPosition());
	}

	public void SetNodeLightingDirty(Int2 _nodeGridPos) {
		Node _node = GameGrid.GetInstance().TryGetNode(_nodeGridPos);
		if (_node == null) {
			return;
		}
		
		SetNodeLightingDirty(_node);
	}
	
	public void SetNodeLightingDirty(Node _node) {
		if (_node == null) {
			return;
		}
		
		AddNodeToUpdate(_node.GridPos, _hasDirtyLighting: true, _hasDirtyIncandescence: null);

		RoomManager.Room _room = RoomManager.GetInstance().GetRoom(_node.RoomIndex);
		if (_room != null) {
			for (int i = 0; i < _room.NodeGridPositions.Count; i++) {
				AddNodeToUpdate(_room.NodeGridPositions[i], _hasDirtyLighting: true, _hasDirtyIncandescence: null);
			}
			for (int i = 0; i < _room.WallNodeGridPositions.Count; i++) {
				AddNodeToUpdate(_room.WallNodeGridPositions[i], _hasDirtyLighting: null, _hasDirtyIncandescence: null);
			}
		}
	}
	
	public void SetNodeIncandescenceDirty(Int2 _nodeGridPos) {
		AddNodeToUpdate(_nodeGridPos, true, true);
	}

	private void AddNodeToUpdate(Int2 _nodeGridPos, bool? _hasDirtyLighting, bool? _hasDirtyIncandescence) {
		Node _node = GameGrid.GetInstance().TryGetNode(_nodeGridPos);
		if (_node == null) {
			return;
		}

		if (_node.IsWall) {
			if (!wallNodesToUpdate.Contains(_nodeGridPos)) {
				wallNodesToUpdate.Add(_nodeGridPos);
			}
		}
		else {
			NodeDetails _details;
			if (nodesToUpdate.TryGetValue(_nodeGridPos, out _details)) {
				_details.SetValues(_hasDirtyLighting, _hasDirtyIncandescence);
				nodesToUpdate[_nodeGridPos] = _details;
			}
			else {
				
				_details = new NodeDetails(_hasDirtyLighting, _hasDirtyIncandescence);
				nodesToUpdate.Add(_nodeGridPos, _details);
			}
		}
	}
	
	public override bool IsUsingUpdateLate() { return true; }
	public override void UpdateLate(){
		foreach (KeyValuePair<Int2, NodeDetails> _nodeDetails in nodesToUpdate) {
			Node _node = GameGrid.GetInstance().TryGetNode(_nodeDetails.Key);
			RoomManager.Room _room = RoomManager.GetInstance().GetRoom(_node.RoomIndex);
			if (_room == null) {
				continue;
			}
			
			Color32 _lightColor = new Color32(0, 0, 0, 0);
			
			if (_nodeDetails.Value.HasDirtyLighting) {
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
			}

			if (_nodeDetails.Value.HasDirtyIncandescence && _node.ChemicalContainer.IsIncandescent()) {
				Chemical.Blob _chem_0 = _node.ChemicalContainer.Contents[0];
				Chemical.Blob _chem_1 = _node.ChemicalContainer.Contents[1];
				Chemical.Blob _chem_2 = _node.ChemicalContainer.Contents[2];
				
				Color _incandescence = _node.ChemicalContainer.GetIncandescence();

				_lightColor.r = (byte)Mathf.Max(_lightColor.r, _incandescence.r * 255.0f);
				_lightColor.g = (byte)Mathf.Max(_lightColor.g, _incandescence.g * 255.0f);
				_lightColor.b = (byte)Mathf.Max(_lightColor.b, _incandescence.b * 255.0f);
				_lightColor.a = (byte)Mathf.Max(_lightColor.a, _incandescence.a * 255.0f);
			}

			_node.SetLighting(_lightColor);
		}

		for (int i = 0; i < wallNodesToUpdate.Count; i++) {
			Node _node = GameGrid.GetInstance().TryGetNode(wallNodesToUpdate[i]);
			_node.SetLightingBasedOnNeighbors();
		}

		nodesToUpdate.Clear();
	}
}
