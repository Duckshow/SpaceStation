using UnityEngine;

[RequireComponent(typeof(TaskHandler), typeof(CharacterOrienter))]
public class Character : EventOwner {

    private CharacterOrienter orienter;
	private TaskHandler taskHandler;


	public override bool IsUsingAwakeDefault() { return true; }
	public override void AwakeDefault(){
		orienter = GetComponent<CharacterOrienter>();
		taskHandler = GetComponent<TaskHandler>();
		taskHandler.Init(this);
	}

	public override bool IsUsingUpdateDefault() { return true; }
	public override void UpdateDefault(){
		taskHandler.AssignMultiTask(MultiTask.TryGetIdleWalk(taskHandler));
	}

	public CharacterOrienter GetOrienter() {
		return orienter;
	}

	public float GetAppropriateMoveSpeed() {
		float _speed = 1.0f;

		if (taskHandler.CurrentMultiTask != null){
			_speed += (float)taskHandler.CurrentMultiTask.Priority;
		}

		return _speed;
	}
}
