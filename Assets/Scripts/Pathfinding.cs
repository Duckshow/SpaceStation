using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

public class Pathfinding : MonoBehaviour {

    private PathRequestManager requestManager;
    private GameGrid grid;


	void Awake() {
        requestManager = GetComponent<PathRequestManager>();
        grid = GetComponent<GameGrid>();
    }

    public void StartFindPath(Vector3 _startPosWorld, Vector3 _targetPosWorld) {
        StartCoroutine(IFindPath(_startPosWorld, _targetPosWorld));
    }

	public bool GetPathLengthBetweenNodes(Node _startNode, Node _targetNode, out int _stepsTaken) {
		bool _wasSuccessful = FindPath(_startNode, _targetNode, _shouldPrintLogs: false);

		_stepsTaken = 0;
		if (_wasSuccessful){
			Node _n = _targetNode;
			Color c = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1.0f);
			while (_n != _startNode){
				UnityEngine.Debug.DrawLine(_n.WorldPos, _n.ParentNode.WorldPos, c, Mathf.Infinity);
				_n = _n.ParentNode;
				_stepsTaken++;
			}
		}

		return _wasSuccessful;
	}

    IEnumerator IFindPath(Vector3 _startPosWorld, Vector3 _targetPosWorld) {
		Node _startNode = grid.GetNodeFromWorldPos(_startPosWorld);
        Node _targetNode = grid.GetNodeFromWorldPos(_targetPosWorld);

        if (grid.DisplayWaypoints) {
            DrawDebugMarker(_startNode.WorldPos, Color.blue);
            DrawDebugMarker(_targetNode.WorldPos, Color.blue);
        }
        
        if (_startNode == _targetNode) {
            UnityEngine.Debug.Log("Something tried to walk to where it already was! Skip!");
            requestManager.FinishedProcessingPath(null, null, false);
            yield break;
        }

		bool _wasSuccessful = FindPath(_startNode, _targetNode, _shouldPrintLogs: true);

        yield return null;

		Waypoint[] _waypoints = new Waypoint[0];
		Waypoint[] _waypointsFull = new Waypoint[0];
		if (_wasSuccessful) { 
			RetracePath(_startNode, _targetNode, out _waypoints, out _waypointsFull);
		}
        requestManager.FinishedProcessingPath(_waypoints, _waypointsFull, _wasSuccessful);
    }

	bool FindPath(Node _startNode, Node _targetNode, bool _shouldPrintLogs) {
		Stopwatch stopwatch = new Stopwatch();
		stopwatch.Start();

		Color c = new Color(UnityEngine.Random.value, UnityEngine.Random.value, UnityEngine.Random.value, 1.0f);

		if (_startNode.IsWalkable() && _targetNode.IsWalkable()) {
            Heap<Node> _openSet = new Heap<Node>(GameGrid.GetArea());
            HashSet<Node> _closedSet = new HashSet<Node>();

            _openSet.Add(_startNode);

            while (_openSet.Count > 0) {
				Node _currentNode = _openSet.RemoveFirst();
                _closedSet.Add(_currentNode);

                if (_currentNode == _targetNode) {
					stopwatch.Stop();
					if (_shouldPrintLogs){
	                    print("Path found: " + stopwatch.ElapsedMilliseconds + " ms");
					}
					return true;
				}

				Node[] _neighbors;
				GameGrid.NeighborFinder.GetSurroundingNodes(_currentNode.GridPos, out _neighbors);
				foreach (Node _neighbour in _neighbors) {
					if (!_neighbour.IsWalkable()) { 
						continue;
					}
					if (_neighbour.GetOccupyingNodeObject() != null && _neighbour != _targetNode) { 
						continue;
					}
					if (_closedSet.Contains(_neighbour)) { 
						continue;
					}

                    int _newMovementCostToNeighbour = _currentNode.GCost + GetDistance(_currentNode, _neighbour) + _neighbour.MovementPenalty;
                    if (_newMovementCostToNeighbour < _neighbour.GCost || !_openSet.Contains(_neighbour)) {
                        _neighbour.GCost = _newMovementCostToNeighbour;
                        _neighbour.HCost = GetDistance(_neighbour, _targetNode);
                        _neighbour.ParentNode = _currentNode;

						if (!_openSet.Contains(_neighbour)) { 
							_openSet.Add(_neighbour);
						}
						else { 
							_openSet.UpdateItem(_neighbour);
						}
                    }
                }
            }
        }

		return false;
	}

	void RetracePath(Node startNode, Node endNode, out Waypoint[] newPath, out Waypoint[] fullPath) {

        List<Node> path = new List<Node>();
        Node currentNode = endNode;
        while (currentNode != startNode) {
            path.Add(currentNode);
            currentNode = currentNode.ParentNode;
        }
        path.Add(startNode); // added 11/18/2016 (not sure if dangerous, but might prevent problems e.g. if the path starts in front of a door?)
        
        fullPath = MakeWaypointArray(path);
        Array.Reverse(fullPath);
        newPath = SimplifyPath(path);
        Array.Reverse(newPath);

        if (grid.DisplayPaths || grid.DisplayWaypoints) {
            for (int i = 1; i < newPath.Length; i++) {
                if(grid.DisplayPaths)
                    UnityEngine.Debug.DrawLine(newPath[i - 1].CharacterPosition, newPath[i].CharacterPosition, Color.yellow, 30);
                if (grid.DisplayWaypoints) {
                    DrawDebugMarker(newPath[i - 1].CharacterPosition, Color.red);

                    if(i == newPath.Length - 1)
                        DrawDebugMarker(newPath[i].CharacterPosition, Color.red);
                }
            }
        }
    }
    void DrawDebugMarker(Vector3 _pos, Color _color) {
        UnityEngine.Debug.DrawLine(_pos + new Vector3(0, 0.1f, 0), _pos + new Vector3(0.1f, 0, 0), _color, 30);
        UnityEngine.Debug.DrawLine(_pos + new Vector3(0.1f, 0, 0), _pos + new Vector3(0, -0.1f, 0), _color, 30);
        UnityEngine.Debug.DrawLine(_pos + new Vector3(0, -0.1f, 0), _pos + new Vector3(-0.1f, 0, 0), _color, 30);
        UnityEngine.Debug.DrawLine(_pos + new Vector3(-0.1f, 0, 0), _pos + new Vector3(0, 0.1f, 0), _color, 30);
    }

    Waypoint[] MakeWaypointArray(List<Node> path){
        List<Waypoint> waypoints = new List<Waypoint>();
        for (int i = 0; i < path.Count; i++) {
            waypoints.Add(new Waypoint(path[i].GetWorldPosCharacter(), path[i].WorldPosDefault));
            waypoints[i].CenterPosition = path[i].WorldPosDefault;
        }
        return waypoints.ToArray();
    }
    Waypoint[] SimplifyPath(List<Node> path) {
        List<Waypoint> waypoints = new List<Waypoint>();
        Vector2 directionFromLast = Vector2.zero;
        Vector2 directionToNext = Vector2.zero;

        for (int i = 0; i < path.Count; i++) {
            DrawDebugMarker(path[i].WorldPos, Color.green);

			Vector2 currentCharacterPos = path[i].GetWorldPosCharacter();
			Vector2 nextCharacterPos = path[i + 1].GetWorldPosCharacter();

            if (i < path.Count - 1) {
				directionFromLast = directionToNext;
                directionToNext = new Vector2(currentCharacterPos.x - nextCharacterPos.x, currentCharacterPos.y - nextCharacterPos.y).normalized;
            }

            // wait for X seconds on this tile
            if (path[i].WaitTime > 0) {
                waypoints.Add(new Waypoint(currentCharacterPos, path[i].WorldPosDefault));
                continue;
            }

            // if the direction is changing (or if at start/end), add waypoint
            if (directionToNext != directionFromLast || i == 0 || i == path.Count - 1) {
                waypoints.Add(new Waypoint(currentCharacterPos, path[i].WorldPosDefault));
                continue;
            }
        }

        return waypoints.ToArray();
    }

    int GetDistance(Node nodeA, Node nodeB) {
        int distX = Mathf.Abs(nodeA.GridPos.x - nodeB.GridPos.x);
        int distY = Mathf.Abs(nodeA.GridPos.y - nodeB.GridPos.y);

        if (distX > distY)
            return 14 * distY + 10 * (distX - distY);
        else
            return 14 * distX + 10 * (distY - distX);
    }
}
public class Waypoint {
    public Vector3 CharacterPosition;
    public Vector3 CenterPosition;
    public Waypoint(Vector3 _charPos, Vector3 _centerPos) {
        CharacterPosition = _charPos;
        CenterPosition = _centerPos;
    }
}
