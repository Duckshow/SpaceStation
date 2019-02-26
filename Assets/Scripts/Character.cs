using UnityEngine;
using System.Collections.Generic;

[RequireComponent(typeof(TaskHandler), typeof(CharacterOrienter))]
public class Character : EventOwner {

	private static List<Character> allCharacters = new List<Character>();

	private CharacterOrienter orienter;
	private TaskHandler taskHandler;

	private float timeOfArrivalAtPresentNode;
	private Node approachingNode;
	private Node presentNode;
	private Node departingNode;


	public override bool IsUsingAwakeDefault() { return true; }
	public override void AwakeDefault(){
		allCharacters.Add(this);

		orienter = GetComponent<CharacterOrienter>();
		taskHandler = GetComponent<TaskHandler>();
		taskHandler.Init(this);
	}

	void OnDestroy() {
		allCharacters.Remove(this);
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

	public bool MayApproachNode() { 
		for (int i = 0; i < allCharacters.Count; i++){
			Character _character = allCharacters[i];
			if (_character == this){
				continue;
			}
			if (_character.presentNode == approachingNode){
				return false;
			}
			if (_character.approachingNode == approachingNode){
				return false;
			}
			if (_character.departingNode == approachingNode){
				return false;
			}
		}

		return true;
	}

	public bool MayLeavePresentNode() {
		return Time.time - timeOfArrivalAtPresentNode >= presentNode.GetWaitTime();
	}

	public void OnNodeApproaching(Node _node) {
		presentNode = null;
		approachingNode = _node;
		_node.OnCharacterApproaching(this);
	}

	public void OnNodeApproachCancelled() {
		if (approachingNode != null){
			approachingNode.OnCharacterApproachCancel(this);
		}
	
		approachingNode = null;
		presentNode = null;
		departingNode = null;
	}

	public void OnNodeApproachFinished(Node _node){
		timeOfArrivalAtPresentNode = Time.time;
		presentNode = _node;
		_node.OnCharacterApproachFinished(this);
	}

	public void OnNodeDeparting(Node _node){
		presentNode = null;
		departingNode = _node;
		_node.OnCharacterDeparting(this);
	}

	public void OnNodeDepartCancelled(){
		if (departingNode != null){
			departingNode.OnCharacterDepartCancelled(this);
		}

		approachingNode = null;
		presentNode = null;
		departingNode = null;
	}

	public void OnNodeDepartFinished(Node _node){
		_node.OnCharacterDepartFinished(this);
		departingNode = null;
	}

	public Node GetPresentNode() {
		return presentNode;
	}
}
