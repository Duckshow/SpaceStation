using System.Collections.Generic;
using UnityEngine;

public class TaskHandler : EventOwner {
	private const int MAX_MULTITASK_AMOUNT = 200;

	public Character Owner { get; private set; }

    public MultiTask CurrentMultiTask { get; private set; }

	[System.NonSerialized]
	public Int2 MoveToNodeGridPos;


	public void Init(Character _owner) {
		Owner = _owner;
	}
	
	public override bool IsUsingUpdateDefault() { return true; }
	public override void UpdateDefault() {
		base.UpdateDefault();

		if (CurrentMultiTask != null){
			CurrentMultiTask.UpdateDefault();
		}
	}

	public void AssignMultiTask(MultiTask _multiTask) {
		if (_multiTask == null){
			return;
		}

		if (CurrentMultiTask != null){
			if (_multiTask.Priority <= CurrentMultiTask.Priority){
				return;
			}
			else{
				FinishedMultiTask(CurrentMultiTask, _wasSuccess: false);
			}
		}

		CurrentMultiTask = _multiTask;
		CurrentMultiTask.StartNextTask();
	}

	public void FinishedMultiTask(MultiTask _multiTask, bool _wasSuccess) {
		if (_multiTask != CurrentMultiTask){
			Debug.LogError(Owner.name + " somehow has multiple multitasks!");
		}

		if (_wasSuccess){
			// Debug.Log((Owner.name + " successfully completed multitask!").Color(Color.green));
		}
		else{
			// Debug.LogWarning((Owner.name + " failed to complete multitask!").Color(Color.red));
			if (_multiTask.OnAbort != null){
				_multiTask.OnAbort();
			}
		}

		CurrentMultiTask = null;
    }
}
