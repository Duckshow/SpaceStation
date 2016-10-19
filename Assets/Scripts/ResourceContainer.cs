using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ResourceContainer : MonoBehaviour {
    public static List<ResourceContainer> AllResources = new List<ResourceContainer>();

    public ResourceManager.ResourceType Type;
    public bool CanBeTakenFrom = true;
    public bool DispensesHeldCaches = false;
    public bool CanBeUsedByMany = true;
    public bool Infinite = false;
    public float Max;
    [SerializeField] private float Current;
    public float _Current_ { get { return Current; } private set { Current = Mathf.Clamp(value, 0, Max); } }
    public float TransferRate = 5;
    public float _CurrentEfficiency_ { get { return (componentHolder == null ? 1 : componentHolder.CurrentEfficiency); } }
    [SerializeField] private float DecreaseRate;

    [HideInInspector]
    public Actor OwnerPawn = null;
    [HideInInspector]
    public int AmountUsingThis = 0;

    private ComponentHolder componentHolder;
    private bool decreaseRoutineIsRunning = false;


    void OnEnable() {
        AllResources.Add(this);
    }
    void OnDisable() {
        AllResources.Remove(this);
    }
    void Awake() {
        componentHolder = GetComponent<ComponentHolder>();
    }
    void Start() {
        StartCoroutine(_DecreaseOverTime());
    }

    public void SetResourceCurrent(float val) {
        _Current_ = val;

        if (!decreaseRoutineIsRunning)
            StartCoroutine(_DecreaseOverTime());
    }

    IEnumerator _DecreaseOverTime() { // TODO: this is fine for something like comfort, but physical resources like food should be converted to something else
        if (DecreaseRate == 0)
            yield break;

        decreaseRoutineIsRunning = true;
        while (true) {
            if (_Current_ == 0 || DecreaseRate == 0)
                break;

            _Current_ -= DecreaseRate;
            yield return new WaitForSeconds(0.1f);
        }
        decreaseRoutineIsRunning = false;
    }

    public bool TryTakeResource(ResourceContainer _to, float _amount = Mathf.Infinity) {
        if (DispensesHeldCaches && _to.OwnerPawn == null)
            Debug.LogError(name + " dispenses resource cache but something other than a Unit tried to use it!");

        bool result = componentHolder == null ? true : componentHolder.CanUse();
        if (!result || (!Infinite && (!DispensesHeldCaches && _Current_ == 0 || DispensesHeldCaches && _Current_ < TransferRate)) || _CurrentEfficiency_ == 0) {
            if (_to.OwnerPawn != null)
                _to.OwnerPawn.Knowledge.SetResourceContainerState(this, false);

            return false;
        }
        Debug.Log(_to.name + " took " + Type.ToString() + " from " + name);
        Transfer(_to, _amount);
        return true;
    }

    public void Transfer(ResourceContainer _to, float _amount = Mathf.Infinity) {
        float received = 0;
        if (!DispensesHeldCaches) {
            // either the specified amount or the min, which is what's left or the max transfer rate
            if (Infinite)
                received = TransferRate * _CurrentEfficiency_;
            else
                received = Mathf.Min(_amount, Mathf.Min((TransferRate * _CurrentEfficiency_), _Current_));
        }
        else {
            // if it dispenses caches, efficiency should probably affect speed, not quantity
            received = TransferRate;
        }

        if (!Infinite)
            _Current_ = Mathf.Clamp(_Current_ - received, 0, Max);
        if ((!Infinite && _Current_ == 0) || _CurrentEfficiency_ == 0) { // ran out, but got some
            if (_to.OwnerPawn != null)
                _to.OwnerPawn.Knowledge.SetResourceContainerState(this, false);

            if (received == 0)
                return;
        }
        else if (_to.OwnerPawn != null)
                _to.OwnerPawn.Knowledge.SetResourceContainerState(this, true);

        if (DispensesHeldCaches) {
            switch (Type) {
                case ResourceManager.ResourceType.Water:
                    _to.OwnerPawn.HeldWater = TransferRate;
                    break;
                case ResourceManager.ResourceType.Food:
                    _to.OwnerPawn.HeldFood = TransferRate;
                    break;
                case ResourceManager.ResourceType.Happiness:
                case ResourceManager.ResourceType.Comfort:
                    throw new System.Exception(name + " dispenses " + Type.ToString() + " as caches! We don't have support for that!");
            }
        }
        else
            _to.SetResourceCurrent(_to._Current_ + received);
    }

    public static ResourceContainer GetClosestAvailableResource(Actor unit, ResourceManager.ResourceType type) {
        ResourceContainer _closest = null;
        float _shortestDist = Mathf.Infinity;

        Dictionary<ResourceContainer, KnowledgeBase.StateInfo> dictionary = unit.Knowledge.GetResourceContainerStates(type);
        foreach (KeyValuePair<ResourceContainer, KnowledgeBase.StateInfo> _kvp in dictionary) { // optimization: maybe don't look through ALL the resources?
            if (_kvp.Value.State == false)
                continue;
            if (!_kvp.Key.CanBeUsedByMany && _kvp.Key.AmountUsingThis > 0)
                continue;

            float newDist = Vector3.Distance(_kvp.Key.transform.position, unit.transform.position);
            if (newDist < _shortestDist) {
                _shortestDist = newDist;
                _closest = _kvp.Key;
            }
        }

        return _closest;
    }

    public static ResourceContainer GetClosestSeatByTV(Actor unit, TV _tv) {
        ResourceContainer _closest = null;
        float _shortestDist = Mathf.Infinity;

        Dictionary<ResourceContainer, KnowledgeBase.StateInfo> dictionary = unit.Knowledge.GetResourceContainerStates(ResourceManager.ResourceType.Comfort);
        foreach (KeyValuePair<ResourceContainer, KnowledgeBase.StateInfo> _kvp in dictionary) {
            if (_kvp.Value.State == false)
                continue;
            if (!_kvp.Key.CanBeUsedByMany && _kvp.Key.AmountUsingThis > 0)
                continue;
            if (!_tv.ViewingArea.bounds.Contains(_kvp.Key.transform.position))
                continue;

            float newDist = Vector3.Distance(_kvp.Key.transform.position, unit.transform.position);
            if (newDist < _shortestDist) {
                _shortestDist = newDist;
                _closest = _kvp.Key;
            }
        }

        return _closest;
    }
}
