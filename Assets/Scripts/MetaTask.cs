using UnityEngine;
using System.Collections.Generic;

public class MetaTask : IHeapItem<MetaTask> {

    public Queue<Task> Tasks = new Queue<Task>();

    public enum Priority { Lowest = 0, Low = 1, Medium = 2, High = 3, Highest = 4 }
    public Priority CurrentPriority;
    public TaskHandler Handler;

    private float timeIssued = Mathf.Infinity;

    //Task currentTask;
    private List<Task> currentTasks = new List<Task>();
    private int tasksStillRunning = 0;
    //private bool isPerformingTask;

    private int heapIndex;
    public int HeapIndex {
        get { return heapIndex; }
        set { heapIndex = value; }
    }


    public MetaTask(TaskHandler _handler, Priority _priority, params Task[] _tasks) {
        timeIssued = Time.time; // todo: make this sustainable

        Handler = _handler;
        CurrentPriority = _priority;
        for (int i = 0; i < _tasks.Length; i++) {
            Tasks.Enqueue(_tasks[i]);
        }
    }

    public void TryPerformingNext() {
        if (tasksStillRunning == 0 && Tasks.Count > 0) { // start the next
            //currentTask = Tasks.Dequeue();
            //isPerformingTask = true;
            //currentTask.Start(this);
            
            // look ahead in the queue and start all parallel tasks
            Task _next = null;
            while (Tasks.Count > 0) {
                _next = Tasks.Peek();
                if (tasksStillRunning > 0 && !_next.ParallelToPrevious)
                    break;

                currentTasks.Add(Tasks.Dequeue());
                currentTasks[currentTasks.Count - 1].Start(this);
                tasksStillRunning++;
            }
        }
        else if (Tasks.Count == 0) {
            Handler.FinishedMeta();
        }
    }

    public void FinishedTask() {
        tasksStillRunning--;

        if(tasksStillRunning == 0)
            TryPerformingNext();
    }

    public void Stop() {
        if (tasksStillRunning == 0)
            return;

        for (int i = 0; i < currentTasks.Count; i++) {
            currentTasks[i].Stop();
        }

        //currentTask.Stop();
    }

    public int CompareTo(MetaTask taskToCompare) {
        int compare = ((int)CurrentPriority).CompareTo((int)taskToCompare.CurrentPriority);
        if (compare == 0)
            compare = timeIssued.CompareTo(taskToCompare.timeIssued);

        return compare;
    }
}
