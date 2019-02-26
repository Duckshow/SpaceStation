using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class Task {
	public enum State { Start, Tick, Done, Abort }
	private State currentState = State.Start;

	protected MultiTask owner;

	public Task(MultiTask _owner){ 
		owner = _owner; 

		if (owner != null){
			owner.OnAbort += OnAbort;
		}
	}
	public abstract void OnAbort();
	public virtual void Start(out State _state){ _state = State.Done; }
	public virtual void Tick(out State _state){ _state = State.Done; }

	public State Perform(MultiTask _multiTask) { 
		switch (currentState){
			case Task.State.Done:
			case Task.State.Abort:
			case Task.State.Start:
				Start(out currentState);
				break;
			case Task.State.Tick:
				Tick(out currentState);
				break;
			default:
				Debug.LogError(currentState + " hasn't been properly implemented yet!");
				break;
		}

		return currentState;
	}

	public class FindPath : Task {

		public class PathNode : IHeapItem<PathNode> {
			public Int2 GridPos { get; private set; }
			public Vector2 WorldPos { get; private set; }

			public PathNode Parent;
			public int HeapIndex { get; set; }
			public int GCost;
			public int HCost;

			public PathNode(int _x, int _y) {
				GridPos = new Int2(_x, _y);
				WorldPos = GameGrid.GetInstance().GetWorldPosFromNodeGridPos(GridPos);
			}

			public int GetFCost() { 
				return GCost + HCost;
			}

			public int CompareTo(PathNode _otherPathNode) {
				int compare = GetFCost().CompareTo(_otherPathNode.GetFCost());
				if (compare == 0) { 
					compare = HCost.CompareTo(_otherPathNode.HCost);
				}

				return -compare;
			}
		}

		private PathNode[,] nodeGrid;

		private Int2 startNodeGridPos;
		private Int2 targetNodeGridPos;

		private Color32 debugColor;
		private Heap<PathNode> openSet;
		private HashSet<PathNode> closedSet;
		private Vector2[] path;
		private Vector2[] pathFull;
		private float pathLength;

		public void SetStartAndTarget(Node _startNode, Node _targetNode) {
			startNodeGridPos = _startNode.GridPos;
			targetNodeGridPos = _targetNode.GridPos;
		}

		public Vector2[] GetLatestPath() {
			return path;
		}

		public FindPath(MultiTask _owner) : base(_owner){
			nodeGrid = new PathNode[GameGrid.SIZE.x, GameGrid.SIZE.y];
			for (int y = 0; y < GameGrid.SIZE.y; y++){
				for (int x = 0; x < GameGrid.SIZE.x; x++){
					nodeGrid[x, y] = new PathNode(x, y);
				}
			}
		}

		public static bool TryGetPathLength(Node _from, Node _to, out float _length) { // very hax
			FindPath _pathFinder = new FindPath(null);

			_pathFinder.startNodeGridPos = _from.GridPos;
			_pathFinder.targetNodeGridPos = _to.GridPos;

			State _state;
			_pathFinder.Start(out _state);
			if (_state == State.Abort){
				_length = 0;
				return false;
			}

			while (_state == State.Tick){
				_pathFinder.Tick(out _state);
			}

			_length = _pathFinder.pathLength;
			return _state == State.Done;
		}

		public override void OnAbort() { 

		}

		public override void Start(out State _state) {
			Node _actualStartNode = GameGrid.GetInstance().TryGetNode(startNodeGridPos);
			Node _actualTargetNode = GameGrid.GetInstance().TryGetNode(targetNodeGridPos);

			if (GameGrid.GetInstance().DisplayWaypoints) {
				DrawDebugMarker(_actualStartNode.WorldPos, Color.blue);
				DrawDebugMarker(_actualTargetNode.WorldPos, Color.blue);
			}
			
			if (!_actualStartNode.GetIsWalkable() || !_actualTargetNode.GetIsWalkable()){
				Debug.Log("The start or target node was unwalkable! Skip!");
				_state = State.Abort;
				return;
			}

			debugColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1.0f);

			openSet = new Heap<PathNode>(GameGrid.GetArea());
			closedSet = new HashSet<PathNode>();

			openSet.Add(nodeGrid[startNodeGridPos.x, startNodeGridPos.y]);
			_state = State.Tick;
		}

		public override void Tick(out State _state) {
			if (openSet == null || openSet.Count == 0){
				_state = State.Abort;
				return;
			}

			PathNode _currentPathNode = openSet.RemoveFirst();
			closedSet.Add(_currentPathNode);

			if (_currentPathNode.GridPos != targetNodeGridPos) {
				Node[] _neighbors;
				NeighborFinder.GetSurroundingNodes(_currentPathNode.GridPos, out _neighbors);
				for (int i = 0; i < _neighbors.Length; i++){
					Node _neighbor = _neighbors[i];
					if (_neighbor == null){
						continue;
					}
					
					PathNode _neighborPathNode = nodeGrid[_neighbor.GridPos.x, _neighbor.GridPos.y];

					if (ShouldConsiderUsingNeighbor(_neighborPathNode, _currentPathNode)){
						Debug.DrawLine(_currentPathNode.WorldPos, _neighborPathNode.WorldPos, Color.magenta, 1.0f);

						_neighborPathNode.GCost = GetMovementCost(_currentPathNode, _neighborPathNode); 
						_neighborPathNode.HCost = GetDistance(_neighborPathNode.GridPos, targetNodeGridPos);
						_neighborPathNode.Parent = _currentPathNode;

						if (!openSet.Contains(_neighborPathNode)) { 
							openSet.Add(_neighborPathNode);
						}
						else { 
							openSet.UpdateItem(_neighborPathNode);
						}
					}
				}

				_state = State.Tick;
			}
			else{
				RetracePath(out path, out pathFull, out pathLength);
				_state = State.Done;
			}
		}

		bool ShouldConsiderUsingNeighbor(PathNode _neighborPathNode, PathNode _currentPathNode) {
			Node _neighborNode = GameGrid.GetInstance().TryGetNode(_neighborPathNode.GridPos);
			Node _currentNode = GameGrid.GetInstance().TryGetNode(_currentPathNode.GridPos);

			if (_neighborPathNode == null){
				return false;
			}
			if (!_neighborNode.GetIsWalkable()) {
				return false;
			}

			bool _isDiagonal = _neighborNode.GridPos.x - _currentNode.GridPos.x != 0 && _neighborNode.GridPos.y - _currentNode.GridPos.y != 0;
			if (_neighborNode.AttachedInteractiveObject != null && _isDiagonal){
				return false;
			}
			if (_currentNode.AttachedInteractiveObject != null && _isDiagonal){
				return false;
			}

			if (_neighborNode.GetOccupyingNodeObject() != null && _neighborPathNode.GridPos != targetNodeGridPos) {
				return false;
			}
			if (closedSet.Contains(_neighborPathNode)) {
				return false;
			}
			if (openSet.Contains(_neighborPathNode) && GetMovementCost(_currentPathNode, _neighborPathNode) >= _neighborPathNode.GCost){
				return false;
			}

			return true;
		}

		int GetMovementCost(PathNode _from, PathNode _to) {
			Node _toNode = GameGrid.GetInstance().TryGetNode(_to.GridPos);
			return _from.GCost + GetDistance(_from.GridPos, _to.GridPos) + _toNode.GetMovementPenalty();
		}

		static int GetDistance(Int2 _gridPos1, Int2 _gridPos2) {
			int distX = Mathf.Abs(_gridPos1.x - _gridPos2.x);
			int distY = Mathf.Abs(_gridPos1.y - _gridPos2.y);

			if (distX > distY) { 
				return 14 * distY + 10 * (distX - distY);
			}
			else { 
				return 14 * distX + 10 * (distY - distX);
			}
		}

		void RetracePath(out Vector2[] _newPath, out Vector2[] _fullPath, out float _pathLength) {
			_pathLength = 0.0f;

			List<PathNode> _path = new List<PathNode>();
			PathNode _currentPathNode = nodeGrid[targetNodeGridPos.x, targetNodeGridPos.y];

			while (_currentPathNode.GridPos != startNodeGridPos) {
				PathNode _parent = _currentPathNode.Parent;
				Vector2 _direction = (_parent.WorldPos - _currentPathNode.WorldPos).normalized;
				bool _isDirectionDiagonal = _direction.x != 0 && _direction.y != 0;
				_pathLength += _isDirectionDiagonal ? 1.5f : 1.0f;

				_path.Add(_currentPathNode);
				Debug.DrawLine(_currentPathNode.WorldPos, _parent.WorldPos, Color.magenta, Mathf.Infinity);
				_currentPathNode = _parent;
			}
			
			_fullPath = MakeWaypointArray(_path);
			Array.Reverse(_fullPath);

			_newPath = SimplifyPath(_path);
			Array.Reverse(_newPath);

			if (GameGrid.GetInstance().DisplayPaths || GameGrid.GetInstance().DisplayWaypoints) {
				for (int i = 1; i < _newPath.Length; i++) {
					if (GameGrid.GetInstance().DisplayPaths) { 
						Debug.DrawLine(_newPath[i - 1], _newPath[i], Color.yellow, 30);
					}
					if (GameGrid.GetInstance().DisplayWaypoints) {
						DrawDebugMarker(_newPath[i - 1], Color.red);

						if (i == _newPath.Length - 1) { 
							DrawDebugMarker(_newPath[i], Color.red);
						}
					}
				}
			}
		}

		static Vector2[] MakeWaypointArray(List<PathNode> _path){
			Vector2[] _waypoints = new Vector2[_path.Count];
			for (int i = 0; i < _path.Count; i++) {
				_waypoints[i] = _path[i].WorldPos;
			}
			return _waypoints;
		}

		static Vector2[] SimplifyPath(List<PathNode> _path) {
			List<Vector2> _waypoints = new List<Vector2>();

			for (int i = 0; i < _path.Count; i++) {
				DrawDebugMarker(_path[i].WorldPos, Color.green);

				Node _currentNode = GameGrid.GetInstance().TryGetNode(_path[i].GridPos);
				Node _nextNode = i < _path.Count - 1 ? GameGrid.GetInstance().TryGetNode(_path[i + 1].GridPos) : null;
				Node _previousNode = i > 0 ? GameGrid.GetInstance().TryGetNode(_path[i - 1].GridPos) : null;

				if (ShouldAddPointInPath(_currentNode, _previousNode, _nextNode)){
					_waypoints.Add(_currentNode.WorldPos);
				}
			}

			return _waypoints.ToArray();
		}

		static bool ShouldAddPointInPath(Node _node, Node _previousNode, Node _nextNode) { 
			if (_node.AttachedInteractiveObject != null) {
				return true;
			}

			if (_previousNode == null || _previousNode.AttachedInteractiveObject != null) {
				return true;
			}

			if (_nextNode == null || _previousNode.AttachedInteractiveObject != null) {
				return true;
			}

			Int2 _cur = _node.GridPos;
			Int2 _prev = _previousNode.GridPos;
			Int2 _next = _node.GridPos;

			if (_next.x - _cur.x != _cur.x - _prev.x || _next.y - _cur.y != _cur.y - _prev.y) {
				return true;
			}

			return false;
		}

		static void DrawDebugMarker(Vector3 _pos, Color _color){
			Debug.DrawLine(_pos + new Vector3(0, 0.1f, 0), _pos + new Vector3(0.1f, 0, 0), _color, 30);
			Debug.DrawLine(_pos + new Vector3(0.1f, 0, 0), _pos + new Vector3(0, -0.1f, 0), _color, 30);
			Debug.DrawLine(_pos + new Vector3(0, -0.1f, 0), _pos + new Vector3(-0.1f, 0, 0), _color, 30);
			Debug.DrawLine(_pos + new Vector3(-0.1f, 0, 0), _pos + new Vector3(0, 0.1f, 0), _color, 30);
		}
	}

	public class MoveAlongPath : Task {

		private Vector3 target;
		private Vector2 next;
		private Vector2 prev;
		private Node nextNode;
		private Node prevNode;
		private int waypointIndex;
		private float timeAtPrev;
		private float speed;
		private Vector2[] path;

		public void SetSpeed(float _speed) {
			speed = _speed;
		}

		public MoveAlongPath(MultiTask _owner) : base(_owner){ }

		public override void OnAbort(){
			owner.TaskHandler.Owner.OnNodeDepartCancelled();
			owner.TaskHandler.Owner.OnNodeApproachCancelled();
		}

		public override void Start(out State _state) {
			path = owner.GetPathFindResult();
			if (path == null || path.Length == 0){
				_state = State.Abort;
				return;
			}

			next = path[0];
			prev = owner.TaskHandler.transform.position;

			nextNode = GameGrid.GetInstance().GetNodeFromWorldPos(next);
			prevNode = GameGrid.GetInstance().GetNodeFromWorldPos(prev);

			timeAtPrev = Time.time;

			SetCharacterOrientation((next - (Vector2)owner.TaskHandler.transform.position).normalized);
			_state = State.Tick;
		}

		public override void Tick(out State _state) {
			if ((next - (Vector2)owner.TaskHandler.transform.position).sqrMagnitude < 0.01f) {
				if (owner.TaskHandler.Owner.GetPresentNode() == null){
					owner.TaskHandler.Owner.OnNodeDepartFinished(prevNode);
					owner.TaskHandler.Owner.OnNodeApproachFinished(nextNode);
				}

				if (!owner.TaskHandler.Owner.MayLeavePresentNode()){
					_state = State.Tick;
					return;
				}

				waypointIndex++;
				if (waypointIndex >= path.Length) {
					_state = State.Done;
					return;
				}

				prev = next;
				prevNode = nextNode;

				next = path[waypointIndex];
				nextNode = GameGrid.GetInstance().GetNodeFromWorldPos(next);

				bool _isNextNodeWithoutCloseWalls = !nextNode.IsWall && !NeighborFinder.IsCardinalNeighborWall(nextNode.GridPos);
				ForceCharacterLieDown(_isNextNodeWithoutCloseWalls);
				SetCharacterOrientation((next - prev).normalized);

				// set time so movement is kept at a good pace
				timeAtPrev = Time.time;
			}

			if (!owner.TaskHandler.Owner.MayApproachNode()){
				_state = State.Tick;
				return;
			}

			owner.TaskHandler.transform.position += GetDeltaPos();
			_state = State.Tick;

			owner.TaskHandler.Owner.OnNodeDeparting(prevNode);
			owner.TaskHandler.Owner.OnNodeApproaching(nextNode);
		}

		void SetCharacterOrientation(Vector3 _newDirection) {
			float _directionAngle = (Mathf.Rad2Deg * Mathf.Atan2(-_newDirection.x, -_newDirection.y)) + 180;
			owner.TaskHandler.Owner.GetOrienter().SetOrientation(_directionAngle);
		}

		void ForceCharacterLieDown(bool _b) {
			owner.TaskHandler.Owner.GetOrienter().ForceLieDown(_b);
		}

		Vector3 GetDeltaPos() {
			float _distance = (next - prev).magnitude;
			Vector3 _newPos = Vector3.Lerp(prev, next, Mathf.Clamp01((Time.time - timeAtPrev) / (_distance / speed)));
			Vector3 _diff = _newPos - owner.TaskHandler.transform.position;

			float _dist = Vector3.Distance(_newPos, next);
			if (_dist < GameGrid.TILE_RADIUS) { 
				_diff *= Mathf.Max(0.1f, _dist / GameGrid.TILE_RADIUS);
			}

			return _diff;
		}
	}
}