using UnityEngine;
using System.Collections.Generic;

public class RoomManager : Singleton<RoomManager> {

	[SerializeField]
	private bool drawDebug = false;

	public class Room {
		private static int roomCreationAttempts = 0;

		public float TimeCreated { get; private set; }
		public readonly List<Int2> NodeGridPositions = new List<Int2>(GameGrid.GetArea());
		public readonly List<Int2> WallNodeGridPositions = new List<Int2>(GameGrid.GetArea());
		public readonly List<Lamp> Lamps = new List<Lamp>(GameGrid.GetArea());

		public int RoomIndex { get; private set; }
		public bool IsInsideShip;
		private Color debugColor;

		public static Room CreateRoom(Int2 _nodeGridPosOrigin) {
			roomCreationAttempts++;
			int _roomIndex = roomCreationAttempts;
			
			Room _newRoom = new Room(_nodeGridPosOrigin, _roomIndex);

			_newRoom.RecursivelyFindRoomCoverage(_nodeGridPosOrigin, out _newRoom.IsInsideShip);

			if (RoomManager.GetInstance().drawDebug && _newRoom.IsInsideShip){
				_newRoom.DebugDrawRoom();
			}

			return _newRoom;
		}

		public Room(Int2 _nodeGridPosOrigin, int _roomIndex) {
			TimeCreated = Time.time;

			RoomIndex = _roomIndex;
			debugColor = new Color(Random.value, Random.value, Random.value, 1.0f);
		}

		void RecursivelyFindRoomCoverage(Int2 _nodeGridPos, out bool _isInsideShip, bool _shouldOnlyLookForWalls = false) { 
			Node _node = GameGrid.GetInstance().TryGetNode(_nodeGridPos);
			
			_isInsideShip = true;

			if (_node == null){
				_isInsideShip = false;
				return;
			}
			if (_node.IsWall){
				WallNodeGridPositions.Add(_nodeGridPos);
				return;
			}
			else if (_shouldOnlyLookForWalls){
				return;
			}
			if (_node.RoomIndex == RoomIndex){
				return;
			}

			NodeGridPositions.Add(_nodeGridPos);
			_node.SetRoomIndex(RoomIndex);

			Lamp _lamp = _node.GetOccupyingNodeObject() as Lamp;
			if (_lamp != null){
				Lamps.Add(_lamp);
			}

			RecursivelyFindRoomCoverage(_nodeGridPos + Int2.Up, out _isInsideShip);
			RecursivelyFindRoomCoverage(_nodeGridPos + Int2.Right, out _isInsideShip);
			RecursivelyFindRoomCoverage(_nodeGridPos + Int2.Down, out _isInsideShip);
			RecursivelyFindRoomCoverage(_nodeGridPos + Int2.Left, out _isInsideShip);

			RecursivelyFindRoomCoverage(_nodeGridPos + Int2.UpLeft, out _isInsideShip, _shouldOnlyLookForWalls: true);
			RecursivelyFindRoomCoverage(_nodeGridPos + Int2.UpRight, out _isInsideShip, _shouldOnlyLookForWalls: true);
			RecursivelyFindRoomCoverage(_nodeGridPos + Int2.DownLeft, out _isInsideShip, _shouldOnlyLookForWalls: true);
			RecursivelyFindRoomCoverage(_nodeGridPos + Int2.DownRight, out _isInsideShip, _shouldOnlyLookForWalls: true);
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


	public override bool IsUsingStartLate() { return true; }
	public override void StartLate(){
		base.StartLate();

		for (int _y = 0; _y < GameGrid.SIZE.y; _y++){
			for (int _x = 0; _x < GameGrid.SIZE.x; _x++){
				Node _node = GameGrid.GetInstance().TryGetNode(_x, _y);
				ScheduleUpdateForRoomOfNode(new Int2(_x, _y));
			}
		}

		UpdateRooms();
	}

	public override bool IsUsingUpdateLate() { return true; }
	public override void UpdateLate(){
		base.UpdateLate();
		UpdateRooms();
	}

	void UpdateRooms() { 
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
			else{
				Room _newRoom = Room.CreateRoom(_nodeGridPos);
				allRooms.Add(_newRoom.RoomIndex, _newRoom);
			}
		}
	}

	public void ScheduleUpdateForRoomOfNode(Int2 _nodeGridPos) {
		// Debug.Log((_nodeGridPos + " (" + nodeGridPositionsWhoseRoomsNeedUpdate.Count + ")").ToString().Color(Color.cyan));
		nodeGridPositionsWhoseRoomsNeedUpdate.Enqueue(_nodeGridPos);
	}

	public bool IsInsideShip(int _roomIndex) {
		Room _room;
		allRooms.TryGetValue(_roomIndex, out _room);

		return _room != null && _room.IsInsideShip;
	}

	public Room GetRoom(int _roomIndex) {
		Room _room;
		allRooms.TryGetValue(_roomIndex, out _room);
		return _room;
	}
}
