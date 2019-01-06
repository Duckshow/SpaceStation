using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

public class Pathfinding : MonoBehaviour {

    PathRequestManager requestManager;
    GameGrid grid;


    void Awake() {
        requestManager = GetComponent<PathRequestManager>();
        grid = GetComponent<GameGrid>();
    }

    public void StartFindPath(Vector3 startPos, Vector3 targetPos) {
        StartCoroutine(FindPath(startPos, targetPos));
    }

    IEnumerator FindPath(Vector3 _startPos, Vector3 _targetPos) {

        Stopwatch sw = new Stopwatch();
        sw.Start();

        Waypoint[] waypoints = new Waypoint[0];
        Waypoint[] waypointsFull = new Waypoint[0];
        bool pathSuccess = false;
        
        Node startNode = grid.GetNodeFromWorldPos(_startPos);
        Node targetNode = grid.GetNodeFromWorldPos(_targetPos);

        if (grid.DisplayWaypoints) {
            DrawDebugMarker(startNode.WorldPos, Color.blue);
            DrawDebugMarker(targetNode.WorldPos, Color.blue);
        }
        
        if (startNode == targetNode) {
            UnityEngine.Debug.Log("Something tried to walk to where it already was! Skip!");
            requestManager.FinishedProcessingPath(waypoints, waypoints, false);
            yield break;
        }

        if (startNode.IsWalkable() && targetNode.IsWalkable()) {
            Heap<Node> openSet = new Heap<Node>(GameGrid.GetArea());
            HashSet<Node> closedSet = new HashSet<Node>();

            openSet.Add(startNode);

            while (openSet.Count > 0) {
                Node currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == targetNode) {
                    sw.Stop();
                    print("Path found: " + sw.ElapsedMilliseconds + " ms");
                    pathSuccess = true;
                    break;
                }

                foreach (Node neighbour in grid.GetNeighbours(currentNode.GridPos.x, currentNode.GridPos.y)) {
					if (!neighbour.IsWalkable()) { 
						continue;
					}
					if (neighbour.GetOccupyingTileObject() != null && neighbour != targetNode) { 
						continue;
					}
					if (closedSet.Contains(neighbour)) { 
						continue;
					}

                    int newMovementCostToNeighbour = currentNode.GCost + GetDistance(currentNode, neighbour) + neighbour.MovementPenalty;
                    if (newMovementCostToNeighbour < neighbour.GCost || !openSet.Contains(neighbour)) {
                        neighbour.GCost = newMovementCostToNeighbour;
                        neighbour.HCost = GetDistance(neighbour, targetNode);
                        neighbour.ParentNode = currentNode;

                        if (!openSet.Contains(neighbour))
                            openSet.Add(neighbour);
                        else
                            openSet.UpdateItem(neighbour);
                    }
                }
            }
        }

        yield return null;
        if (pathSuccess)
            RetracePath(startNode, targetNode, out waypoints, out waypointsFull);

        requestManager.FinishedProcessingPath(waypoints, waypointsFull, pathSuccess);
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
