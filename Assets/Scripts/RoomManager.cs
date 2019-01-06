using UnityEngine;
using System.Collections.Generic;

public class RoomManager : Singleton<RoomManager> {

	private class Room {
		private static int roomCreationAttempts = 0;

		public float TimeCreated { get; private set; }
		public List<Int2> NodeGridPositions = new List<Int2>(GameGrid.GetArea());

		public int RoomIndex { get; private set; }
		public bool IsInsideShip;
		private Color debugColor;

		public static Room CreateRoom(Int2 _nodeGridPosOrigin) {
			roomCreationAttempts++;
			int _roomIndex = roomCreationAttempts;
			
			Room _newRoom = new Room(_nodeGridPosOrigin, _roomIndex);

			_newRoom.RecursivelyFindRoomCoverage(_nodeGridPosOrigin, out _newRoom.IsInsideShip);

			if (_newRoom.IsInsideShip){
				_newRoom.DebugDrawRoom();
			}

			return _newRoom;
		}

		public Room(Int2 _nodeGridPosOrigin, int _roomIndex) {
			TimeCreated = Time.time;

			RoomIndex = _roomIndex;
			debugColor = new Color(Random.value, Random.value, Random.value, 1.0f);
			
		}

		void RecursivelyFindRoomCoverage(Int2 _nodeGridPos, out bool _isInsideShip) { 
			Node _node = GameGrid.GetInstance().TryGetNode(_nodeGridPos);
			
			if (_node == null){
				_isInsideShip = false;
				return;
			}
			if (_node.IsWall){
				_isInsideShip = true;
				return;
			}
			if (_node.RoomIndex == RoomIndex){
				_isInsideShip = true;
				return;
			}

			NodeGridPositions.Add(_nodeGridPos);
			_node.SetRoomIndex(RoomIndex);

			_isInsideShip = true;
			if(_isInsideShip) RecursivelyFindRoomCoverage(_nodeGridPos + Int2.Up, out _isInsideShip);
			if(_isInsideShip) RecursivelyFindRoomCoverage(_nodeGridPos + Int2.Right, out _isInsideShip);
			if(_isInsideShip) RecursivelyFindRoomCoverage(_nodeGridPos + Int2.Down, out _isInsideShip);
			if(_isInsideShip) RecursivelyFindRoomCoverage(_nodeGridPos + Int2.Left, out _isInsideShip);
			// Debug.Log(_isInsideShip);
		}

		void DebugDrawRoom() { 
			for (int i = 0; i < NodeGridPositions.Count; i++){
				Int2 _nodeGridPos = NodeGridPositions[i];

				GameGrid grid = GameGrid.GetInstance();
				Vector3 _worldPos = grid.GetWorldPosFromNodeGridPos(_nodeGridPos);
				Debug.DrawLine(_worldPos, grid.GetWorldPosFromNodeGridPos(_nodeGridPos + Int2.UpLeft), debugColor, Mathf.Infinity);
				Debug.DrawLine(_worldPos, grid.GetWorldPosFromNodeGridPos(_nodeGridPos + Int2.Up), debugColor, Mathf.Infinity);
				Debug.DrawLine(_worldPos, grid.GetWorldPosFromNodeGridPos(_nodeGridPos + Int2.UpRight), debugColor, Mathf.Infinity);
				Debug.DrawLine(_worldPos, grid.GetWorldPosFromNodeGridPos(_nodeGridPos + Int2.Right), debugColor, Mathf.Infinity);
				Debug.DrawLine(_worldPos, grid.GetWorldPosFromNodeGridPos(_nodeGridPos + Int2.DownRight), debugColor, Mathf.Infinity);
				Debug.DrawLine(_worldPos, grid.GetWorldPosFromNodeGridPos(_nodeGridPos + Int2.Down), debugColor, Mathf.Infinity);
				Debug.DrawLine(_worldPos, grid.GetWorldPosFromNodeGridPos(_nodeGridPos + Int2.DownLeft), debugColor, Mathf.Infinity);
				Debug.DrawLine(_worldPos, grid.GetWorldPosFromNodeGridPos(_nodeGridPos + Int2.Left), debugColor, Mathf.Infinity);
			}
		}
	}

	private Dictionary<int, Room> allRooms = new Dictionary<int, Room>();
	private Queue<Int2> nodeGridPositionsWhoseRoomsNeedUpdate = new Queue<Int2>(GameGrid.GetArea());


	public override bool IsUsingStartDefault() { return true; }
	public override void StartDefault(){
		base.StartDefault();

		for (int _y = 0; _y < GameGrid.SIZE.y; _y++){
			for (int _x = 0; _x < GameGrid.SIZE.x; _x++){
				Node _node = GameGrid.GetInstance().TryGetNode(_x, _y);
				ScheduleUpdateForRoomOfNode(new Int2(_x, _y));
			}
		}
	}

	public override bool IsUsingUpdateLate() { return true; }
	public override void UpdateLate(){
		base.UpdateLate();
		while (nodeGridPositionsWhoseRoomsNeedUpdate.Count > 0){
			Int2 _nodeGridPos = nodeGridPositionsWhoseRoomsNeedUpdate.Dequeue();
			Node _node = GameGrid.GetInstance().TryGetNode(_nodeGridPos);
			if (_node == null){
				continue;
			}


			Room _room;
			allRooms.TryGetValue(_node.RoomIndex, out _room);
			if (_room != null){
				if (_room.TimeCreated == Time.time){
					continue;
				}

				allRooms.Remove(_node.RoomIndex);
			}

			if (_node.IsWall){
				GameGrid grid = GameGrid.GetInstance();

				Node _nodeTL 	= grid.TryGetNode(_nodeGridPos + Int2.UpLeft);
				Node _nodeT 	= grid.TryGetNode(_nodeGridPos + Int2.Up);
				Node _nodeTR 	= grid.TryGetNode(_nodeGridPos + Int2.UpRight);
				Node _nodeR 	= grid.TryGetNode(_nodeGridPos + Int2.Right);
				Node _nodeBR 	= grid.TryGetNode(_nodeGridPos + Int2.DownRight);
				Node _nodeB 	= grid.TryGetNode(_nodeGridPos + Int2.Down);
				Node _nodeBL 	= grid.TryGetNode(_nodeGridPos + Int2.DownLeft);
				Node _nodeL 	= grid.TryGetNode(_nodeGridPos + Int2.Left);

				if(_nodeTL 	!= null && !_nodeTL.IsWall) ScheduleUpdateForRoomOfNode(_nodeTL.GridPos);
				if(_nodeT 	!= null && !_nodeT.IsWall) 	ScheduleUpdateForRoomOfNode(_nodeT.GridPos);
				if(_nodeTR 	!= null && !_nodeTR.IsWall) ScheduleUpdateForRoomOfNode(_nodeTR.GridPos);
				if(_nodeR 	!= null && !_nodeR.IsWall) 	ScheduleUpdateForRoomOfNode(_nodeR.GridPos);
				if(_nodeBR 	!= null && !_nodeBR.IsWall) ScheduleUpdateForRoomOfNode(_nodeBR.GridPos);
				if(_nodeB 	!= null && !_nodeB.IsWall) 	ScheduleUpdateForRoomOfNode(_nodeB.GridPos);
				if(_nodeBL 	!= null && !_nodeBL.IsWall) ScheduleUpdateForRoomOfNode(_nodeBL.GridPos);
				if(_nodeL 	!= null && !_nodeL.IsWall) 	ScheduleUpdateForRoomOfNode(_nodeL.GridPos);

				continue;
			}

			Room _newRoom = Room.CreateRoom(_nodeGridPos);
			allRooms.Add(_newRoom.RoomIndex, _newRoom);
		}
	}

	public void ScheduleUpdateForRoomOfNode(Int2 _nodeGridPos) {
		nodeGridPositionsWhoseRoomsNeedUpdate.Enqueue(_nodeGridPos);
	}

	public bool IsInsideShip(int _roomIndex) {
		Room _room;
		allRooms.TryGetValue(_roomIndex, out _room);

		return _room != null && _room.IsInsideShip;
	}
}
