using UnityEngine;
using System;
using System.Collections;
using System.Collections.Generic;

public abstract class Task {
	public enum State { Start, Tick, Done, Abort }
	private State currentState = State.Start;

	protected MultiTask owner;

	public Task(MultiTask _owner){ owner = _owner; }
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

		private Node startNode;
		private Node targetNode;
		private Color32 debugColor;
		private Heap<Node> openSet;
		private HashSet<Node> closedSet;
		private Vector2[] path;
		private Vector2[] pathFull;
		private float pathLength;

		public void SetStartAndTarget(Node _startNode, Node _targetNode) {
			startNode = _startNode;
			targetNode = _targetNode;
		}

		public Vector2[] GetLatestPath() {
			return path;
		}

		public FindPath(MultiTask _owner) : base(_owner){ }

		public static bool TryGetPathLength(Node _from, Node _to, out float _length) { // very hax
			FindPath _pathFinder = new FindPath(null);

			_pathFinder.startNode = _from;
			_pathFinder.targetNode = _to;

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

		public override void Start(out State _state) {
			if (GameGrid.GetInstance().DisplayWaypoints) {
				DrawDebugMarker(startNode.WorldPos, Color.blue);
				DrawDebugMarker(targetNode.WorldPos, Color.blue);
			}
			
			if (!startNode.IsWalkable() || !targetNode.IsWalkable()){
				Debug.Log("The start or target node was unwalkable! Skip!");
				_state = State.Abort;
				return;
			}

			debugColor = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1.0f);

			openSet = new Heap<Node>(GameGrid.GetArea());
			closedSet = new HashSet<Node>();

			openSet.Add(startNode);
			_state = State.Tick;
		}

		public override void Tick(out State _state) {
			if (openSet == null || openSet.Count == 0){
				_state = State.Abort;
				return;
			}

			Node _currentNode = openSet.RemoveFirst();
			closedSet.Add(_currentNode);

			if (_currentNode != targetNode) {
				Node[] _neighbors;
				NeighborFinder.GetSurroundingNodes(_currentNode.GridPos, out _neighbors);
				for (int i = 0; i < _neighbors.Length; i++){
					Node _neighbor = _neighbors[i];

					if (_neighbor == null){
						continue;
					}
					if (!_neighbor.IsWalkable()) { 
						continue;
					}
					if (_neighbor.GetOccupyingNodeObject() != null && _neighbor != targetNode) { 
						continue;
					}
					if (closedSet.Contains(_neighbor)) { 
						continue;
					}

					int _newMovementCostToNeighbour = _currentNode.GCost + GetDistance(_currentNode, _neighbor) + _neighbor.MovementPenalty;
					if (_newMovementCostToNeighbour < _neighbor.GCost || !openSet.Contains(_neighbor)) {
						_neighbor.GCost = _newMovementCostToNeighbour;
						_neighbor.HCost = GetDistance(_neighbor, targetNode);
						_neighbor.ParentNode = _currentNode;

						if (!openSet.Contains(_neighbor)) { 
							openSet.Add(_neighbor);
						}
						else { 
							openSet.UpdateItem(_neighbor);
						}
					}
				}

				_state = State.Tick;
			}
			else{
				RetracePath(startNode, targetNode, out path, out pathFull, out pathLength);
				_state = State.Done;
			}
		}

		static int GetDistance(Node nodeA, Node nodeB) {
			int distX = Mathf.Abs(nodeA.GridPos.x - nodeB.GridPos.x);
			int distY = Mathf.Abs(nodeA.GridPos.y - nodeB.GridPos.y);

			if (distX > distY) { 
				return 14 * distY + 10 * (distX - distY);
			}
			else { 
				return 14 * distX + 10 * (distY - distX);
			}
		}

		static void RetracePath(Node _startNode, Node _targetNode, out Vector2[] _newPath, out Vector2[] _fullPath, out float _pathLength) {
			_pathLength = 0.0f;

			List<Node> _path = new List<Node>();
			Node _currentNode = _targetNode;
			while (_currentNode != _startNode) {
				Vector2 _direction = (_currentNode.ParentNode.WorldPos - _currentNode.WorldPos).normalized;
				bool _isDirectionDiagonal = _direction.x != 0 && _direction.y != 0;
				_pathLength += _isDirectionDiagonal ? 1.5f : 1.0f;

				_path.Add(_currentNode);
				_currentNode = _currentNode.ParentNode;
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

		static Vector2[] MakeWaypointArray(List<Node> _path){
			Vector2[] _waypoints = new Vector2[_path.Count];
			for (int i = 0; i < _path.Count; i++) {
				_waypoints[i] = _path[i].WorldPos;
			}
			return _waypoints;
		}

		static Vector2[] SimplifyPath(List<Node> _path) {
			List<Vector2> _waypoints = new List<Vector2>();
			Vector2 _dirFromLast = new Vector2();
			Vector2 _dirToNext = new Vector2();

			for (int i = 0; i < _path.Count - 1; i++) {
				DrawDebugMarker(_path[i].WorldPos, Color.green);

				Vector2 _currentPos = _path[i].WorldPos;
				Vector2 _nextPos = _path[i + 1].WorldPos;

				if (i < _path.Count - 1) {
					_dirFromLast = _dirToNext;
					_dirToNext = new Vector2(_currentPos.x - _nextPos.x, _currentPos.y - _nextPos.y).normalized;
				}

				if (_path[i].WaitTime > 0) {
				_waypoints[i] = _path[i].WorldPos;
					_waypoints.Add(_currentPos);
					continue;
				}

				if (_dirToNext != _dirFromLast) {
					_waypoints.Add(_currentPos);
					continue;
				}

				if (i == 0 || i == _path.Count - 1) {
					_waypoints.Add(_currentPos);
					continue;
				}
			}

			return _waypoints.ToArray();
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

		public override void Start(out State _state) {
			path = owner.GetPathFindResult();

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

			owner.TaskHandler.transform.position += GetDeltaPos();
			_state = State.Tick;
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