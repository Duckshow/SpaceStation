using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System;

public class Pathfinding : MonoBehaviour {

    PathRequestManager requestManager;
    Grid grid;


    void Awake() {
        requestManager = GetComponent<PathRequestManager>();
        grid = GetComponent<Grid>();
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
        
        Tile startNode = grid.GetTileFromWorldPoint(_startPos);
        Tile targetNode = grid.GetTileFromWorldPoint(_targetPos);

        if (grid.DisplayWaypoints) {
            DrawDebugMarker(startNode.WorldPosition, Color.blue);
            DrawDebugMarker(targetNode.WorldPosition, Color.blue);
        }
        
        if (startNode == targetNode) {
            UnityEngine.Debug.Log("Something tried to walk to where it already was! Skip!");
            requestManager.FinishedProcessingPath(waypoints, waypoints, false);
            yield break;
        }

        if (startNode.Walkable && targetNode.Walkable) {
            Heap<Tile> openSet = new Heap<Tile>(Grid.MaxSize);
            HashSet<Tile> closedSet = new HashSet<Tile>();

            openSet.Add(startNode);

            while (openSet.Count > 0) {
                Tile currentNode = openSet.RemoveFirst();
                closedSet.Add(currentNode);

                if (currentNode == targetNode) {
                    sw.Stop();
                    print("Path found: " + sw.ElapsedMilliseconds + " ms");
                    pathSuccess = true;
                    break;
                }

                foreach (Tile neighbour in grid.GetNeighbours(currentNode.GridCoord.x, currentNode.GridCoord.y)) {
                    if (!neighbour.Walkable)
                        continue;
                    if (neighbour.IsOccupiedByObject && neighbour != targetNode)
                        continue;
                    if (closedSet.Contains(neighbour))
                        continue;
                    if (Grid.Instance.IsNeighbourBlockedDiagonally(currentNode, neighbour))
                        continue;

                    int newMovementCostToNeighbour = currentNode.GCost + GetDistance(currentNode, neighbour) + neighbour.MovementPenalty;
                    if (newMovementCostToNeighbour < neighbour.GCost || !openSet.Contains(neighbour)) {
                        neighbour.GCost = newMovementCostToNeighbour;
                        neighbour.HCost = GetDistance(neighbour, targetNode);
                        neighbour.ParentTile = currentNode;

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

    void RetracePath(Tile startNode, Tile endNode, out Waypoint[] newPath, out Waypoint[] fullPath) {

        List<Tile> path = new List<Tile>();
        Tile currentNode = endNode;
        while (currentNode != startNode) {
            path.Add(currentNode);
            currentNode = currentNode.ParentTile;
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

    Waypoint[] MakeWaypointArray(List<Tile> path){
        List<Waypoint> waypoints = new List<Waypoint>();
        for (int i = 0; i < path.Count; i++) {
            waypoints.Add(new Waypoint(path[i].CharacterPositionWorld, path[i].DefaultPositionWorld));
            waypoints[i].CenterPosition = path[i].DefaultPositionWorld;
        }
        return waypoints.ToArray();
    }
    Waypoint[] SimplifyPath(List<Tile> path) {
        List<Waypoint> waypoints = new List<Waypoint>();
        Vector2 directionFromLast = Vector2.zero;
        Vector2 directionToNext = Vector2.zero;

        for (int i = 0; i < path.Count; i++) {
            DrawDebugMarker(path[i].WorldPosition, Color.green);

            //if (path[i]._Type_ == Tile.TileType.Door)
            //    continue;

            if (i < path.Count - 1) {
                directionFromLast = directionToNext;
                directionToNext = new Vector2(path[i].CharacterPositionWorld.x - path[i + 1].CharacterPositionWorld.x, path[i].CharacterPositionWorld.y - path[i + 1].CharacterPositionWorld.y).normalized;

                // stop behind previous tile
                if (path[i + 1].StopAheadAndBehindMeWhenCrossing) {
                    waypoints.Add(new Waypoint(path[i].CharacterPositionWorld, path[i].DefaultPositionWorld));
                    continue;
                }
            }

            if (i > 0) {
                // stop ahead of next tile
                if (path[i - 1].StopAheadAndBehindMeWhenCrossing) {
                    waypoints.Add(new Waypoint(path[i].CharacterPositionWorld, path[i].DefaultPositionWorld));
                    continue;
                }
            }

            // wait for X seconds on this tile
            if (path[i].ForceActorStopWhenPassingThis) {
                waypoints.Add(new Waypoint(path[i].CharacterPositionWorld, path[i].DefaultPositionWorld));
                continue;
            }

            // if the direction is changing (or if at start/end), add waypoint
            if (directionToNext != directionFromLast || i == 0 || i == path.Count - 1) {
                waypoints.Add(new Waypoint(path[i].CharacterPositionWorld, path[i].DefaultPositionWorld));
                continue;
            }
        }

        return waypoints.ToArray();
    }

    int GetDistance(Tile nodeA, Tile nodeB) {
        int distX = Mathf.Abs(nodeA.GridCoord.x - nodeB.GridCoord.x);
        int distY = Mathf.Abs(nodeA.GridCoord.y - nodeB.GridCoord.y);

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
