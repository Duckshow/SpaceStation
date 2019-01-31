﻿using UnityEngine;
using System;
using System.Collections.Generic;
public class PathRequestManager : MonoBehaviour {
   
    Queue<PathRequest> pathRequestQueue = new Queue<PathRequest>();
    PathRequest currentPathRequest;

    static PathRequestManager instance;
    Pathfinding pathfinding;

    bool isProcessingPath;

    void Awake() {
        instance = this;
        pathfinding = GetComponent<Pathfinding>();
    }

    public static void RequestPath(Vector3 _startPosWorld, Vector3 _targetPosWorld, Action<Waypoint[], Waypoint[], bool> _onPathFound) {
        PathRequest _newRequest = new PathRequest(_startPosWorld, _targetPosWorld, _onPathFound);
        instance.pathRequestQueue.Enqueue(_newRequest);
        instance.TryProcessNext();
    }

	public static bool RequestPathLength(Node _startNode, Node _targetNode, out int _pathLength) {
		return instance.pathfinding.GetPathLengthBetweenNodes(_startNode, _targetNode, out _pathLength);
	}

    void TryProcessNext() {
        if (!isProcessingPath && pathRequestQueue.Count > 0) {
            currentPathRequest = pathRequestQueue.Dequeue();
            isProcessingPath = true;
            pathfinding.StartFindPath(currentPathRequest.pathStart, currentPathRequest.pathEnd);
        }
    }

    public void FinishedProcessingPath(Waypoint[] path, Waypoint[] fullPath, bool success) {
        currentPathRequest.callback(path, fullPath, success);
        isProcessingPath = false;
        TryProcessNext();
    }

    struct PathRequest {
        public Vector3 pathStart;
        public Vector3 pathEnd;
        public Action<Waypoint[], Waypoint[], bool> callback;

        public PathRequest(Vector3 _start, Vector3 _end, Action<Waypoint[], Waypoint[], bool> _callback) {
            pathStart = _start;
            pathEnd = _end;
            callback = _callback;
        }
    }
}
