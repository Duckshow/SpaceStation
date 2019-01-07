using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BuildTool : Singleton<BuildTool> {

	private class BuildToolMode { 

	}

	protected enum BuildMode { None, Room, Wall } // TODO: rename this!
	protected BuildMode Mode = BuildMode.None;

	public bool IsActive { get; private set; }
	public void SetIsActive(bool value) {
		IsActive = value;
	}

	private Int2 nodeGridPosStart;
	private Int2 nodeGridPosEnd;
	private List<Int2> affectedNodeGridPositions = new List<Int2>();


	public override bool IsUsingUpdateEarly() { return true; }
	public override void UpdateEarly() { 
		for (int i = affectedNodeGridPositions.Count - 1; i >= 0; i--){
			Node _node = GameGrid.GetInstance().TryGetNode(affectedNodeGridPositions[i]);
			affectedNodeGridPositions.RemoveAt(i);
			if (_node == null){
				continue;
			}

			_node.ClearIsWallTemporary();
		}
	}

	public override bool IsUsingUpdateDefault(){ return true; }
	public override void UpdateDefault() {

		return;

		if (!IsActive){
			return;
		}

		bool _isDeleting = false;
		bool _isTemporary = false;

		Mode = BuildMode.Room;

		Mouse.StateEnum mouseState = Mouse.GetInstance().GetStateLMB();
		if (mouseState == Mouse.StateEnum.Idle){
			mouseState = Mouse.GetInstance().GetStateRMB();

			if (mouseState == Mouse.StateEnum.Idle){
				return;
			}

			_isDeleting = true;
		}

		switch (mouseState){
			case Mouse.StateEnum.Click:
				_isTemporary = true;
				nodeGridPosStart = Mouse.GetInstance().GetGridPos();
				nodeGridPosEnd = Mouse.GetInstance().GetGridPos();
				break;
			case Mouse.StateEnum.Hold:
				_isTemporary = true;
				nodeGridPosEnd = Mouse.GetInstance().GetGridPos();
				break;
			case Mouse.StateEnum.Release:
				_isTemporary = false;
				break;
			default:
				Debug.LogError(mouseState + " hasn't been properly implemented yet!");
				break;
		}

		switch (Mode){
			case BuildMode.None:
				break;
			case BuildMode.Wall:
				TryBuildWall(nodeGridPosStart, nodeGridPosEnd, _isTemporary, _isDeleting);
				break;
			case BuildMode.Room:
				TryBuildRoom(nodeGridPosStart, nodeGridPosEnd, _isTemporary, _isDeleting);
				break;
			default:
				Debug.LogError(Mode + " hasn't been properly implemented yet!");
				break;
		}
	}

	void TryBuildWall(Int2 _nodeGridPosStart, Int2 _nodeGridPosEnd, bool _isTemporary, bool _isDeleting) {
		Int2 _nodeGridPosWallStart = new Int2(
			Mathf.Min(_nodeGridPosStart.x, _nodeGridPosEnd.x),
			Mathf.Min(_nodeGridPosStart.y, _nodeGridPosEnd.y)
		);

		int _wallLengthX = Mathf.Abs(_nodeGridPosEnd.x - _nodeGridPosStart.x);
		int _wallLengthY = Mathf.Abs(_nodeGridPosEnd.y - _nodeGridPosStart.y);

		if (_wallLengthX > _wallLengthY) { 
			_nodeGridPosWallStart.y = _nodeGridPosStart.y;
			_wallLengthY = 0;
		}
		else { 
			_nodeGridPosWallStart.x = _nodeGridPosStart.x;
			_wallLengthX = 0;
		}

		for (int _y = 0; _y <= _wallLengthY; _y++){
			for (int _x = 0; _x <= _wallLengthX; _x++){
				Int2 _nodeGridPosCurrent = _nodeGridPosWallStart + new Int2(_x, _y);

				Node _node = GameGrid.GetInstance().TryGetNode(_nodeGridPosCurrent);
				if (_node == null){
					break;
				}

				affectedNodeGridPositions.Add(_nodeGridPosCurrent);

				if (_isTemporary) {
					_node.SetIsWallTemporary(!_isDeleting);
				}
				else{
					_node.SetIsWall(!_isDeleting);
				}
			}
		}
	}

	void TryBuildRoom(Int2 _nodeGridPosStart, Int2 _nodeGridPosEnd, bool _isTemporary, bool _isDeleting) {
		Int2 _nodeGridPosBL = new Int2(
			Mathf.Min(_nodeGridPosStart.x, _nodeGridPosEnd.x),
			Mathf.Min(_nodeGridPosStart.y, _nodeGridPosEnd.y)
		);

		Int2 _nodeGridPosTR = new Int2(
			Mathf.Max(_nodeGridPosStart.x, _nodeGridPosEnd.x),
			Mathf.Max(_nodeGridPosStart.y, _nodeGridPosEnd.y)
		);

		Int2 _roomSize = new Int2(
			_nodeGridPosTR.x - _nodeGridPosBL.x,
			_nodeGridPosTR.y - _nodeGridPosBL.y
		);

		for (int _y = 0; _y <= _roomSize.y; _y++){
			for (int _x = 0; _x <= _roomSize.x; _x++){
				Int2 _nodeGridPosCurrent = _nodeGridPosBL + new Int2(_x, _y);

				Node _node = GameGrid.GetInstance().TryGetNode(_nodeGridPosCurrent);
				if (_node == null){
					break;
				}

				// TODO: roombuilding should still fill the floor!
				if (_y != 0 && _y != _roomSize.y && _x != 0 && _x != _roomSize.x){
					continue;
				}

				affectedNodeGridPositions.Add(_nodeGridPosCurrent);

				if (_isTemporary) {
					_node.SetIsWallTemporary(!_isDeleting);
				}
				else{
					_node.SetIsWall(!_isDeleting);
				}
			}
		}
	}
}
