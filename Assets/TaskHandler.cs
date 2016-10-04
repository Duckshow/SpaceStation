using System.Collections.Generic;

public class TaskHandler {

    public Unit Owner;

    public List<ResourceManager.ResourceType> ResourcesPendingFetch = new List<ResourceManager.ResourceType>();
    public Heap<MetaTask> PendingTasks = new Heap<MetaTask>(50); // arbitrary number (can't exceed)

    MetaTask currentMetaTask;
    bool isPerformingMetaTask;


    public void AssignTask(MetaTask _meta) {
        PendingTasks.Add(_meta);
        TryPerformingNext();
    }

    void TryPerformingNext() {
        if (isPerformingMetaTask && PendingTasks.Count > 0 && PendingTasks.GetFirst().CurrentPriority > currentMetaTask.CurrentPriority) {
            currentMetaTask.Stop();
            // I could reinsert a copy of the stopped task, so it's redone later, but maybe that isn't actually needed since the player doesn't control the Units?
        }

        if (!isPerformingMetaTask && PendingTasks.Count > 0) {
            currentMetaTask = PendingTasks.RemoveFirst();
            isPerformingMetaTask = true;
            currentMetaTask.TryPerformingNext();
        }
    }

    public void FinishedMeta() {
        isPerformingMetaTask = false;
        TryPerformingNext();
    }
}
