using UnityEngine;
using System;
using System.Collections.Generic;

public enum Priority { Lowest, Low, Medium, High, Highest}
public class MultiTask : IHeapItem<MultiTask> {

	public const Priority PRIORITY_IDLEWALK = Priority.Lowest;

	public Priority Priority { get; private set; }
	public TaskHandler TaskHandler { get; private set; }
	public int HeapIndex { get; set; }

	private Queue<Func<Task.State>> tasks = new Queue<Func<Task.State>>();
	private Func<Task.State> currentTask;
	private Task.State currentTaskState;
	private float timeStarted = 0;

	public delegate void DefaultDelegate();
	public DefaultDelegate OnAbort;

	// possible tasks
	private Task.FindPath taskFindPath;
	private Task.MoveAlongPath taskMoveAlongPath;


	public static MultiTask TryGetIdleWalk(TaskHandler _taskHandler) {
		if (_taskHandler.CurrentMultiTask != null && _taskHandler.CurrentMultiTask.Priority >= PRIORITY_IDLEWALK){
			return null;
		}

		MultiTask _multiTask = new MultiTask(_taskHandler, PRIORITY_IDLEWALK);

		_multiTask.SetTasks(new Queue<Func<Task.State>>(new Func<Task.State>[]{
			() => {
				Node _currentNode = GameGrid.GetInstance().GetNodeFromWorldPos(_taskHandler.transform.position);
				Node _randomNode = GameGrid.GetInstance().GetClosestFreeNode(GameGrid.GetInstance().GetRandomWalkableNode(_currentNode));

				_multiTask.taskFindPath.SetStartAndTarget(_currentNode, _randomNode);
				_multiTask.taskMoveAlongPath.SetSpeed(1.0f + (float)_multiTask.Priority);
				return Task.State.Done;
			},
			() => _multiTask.taskFindPath.Perform(_multiTask),
			() => _multiTask.taskMoveAlongPath.Perform(_multiTask)
		}));

		return _multiTask;
	}

	public MultiTask(TaskHandler _taskHandler, Priority _priority) {
		TaskHandler = _taskHandler;
		Priority = _priority;

		taskFindPath = new Task.FindPath(this);
		taskMoveAlongPath = new Task.MoveAlongPath(this);
	}

	public void SetTasks(Queue<Func<Task.State>> _tasks) {
		tasks = _tasks;
		timeStarted = Time.time;
	}

	public void UpdateDefault() {
		if (currentTask == null){
			return;
		}

		Task.State _state = currentTask();

		if (_state == Task.State.Abort){
			currentTask = null;
			TaskHandler.FinishedMultiTask(this, _wasSuccess: false);
		}
		else if (_state == Task.State.Done){
			currentTask = null;
			
			if (tasks.Count > 0){
				StartNextTask();
				return;
			}

			TaskHandler.FinishedMultiTask(this, _wasSuccess: true);
		}
	}

	public void StartNextTask() {
		if (tasks.Count == 0) {
			return;
		}

		currentTask = tasks.Dequeue();
    }

    public int CompareTo(MultiTask taskToCompare) {
        int compare = ((int)Priority).CompareTo((int)taskToCompare.Priority);
		if (compare == 0) { 
			compare = timeStarted.CompareTo(taskToCompare.timeStarted);
		}

        return compare;
    }

	public Vector2[] GetPathFindResult() {
		return taskFindPath.GetLatestPath();
	}
}
