using UnityEngine;
using System.Collections.Generic;

[System.Serializable]
public class KnowledgeBase {
    [System.Serializable]
    public class StateInfo {
        public bool State;
        public float Time;

        public StateInfo(bool _state, float _time) {
            State = _state;
            Time = _time;
        }
    }
    
    public ResourceContainerAndStateInfoDict WaterContainersFilledState = new ResourceContainerAndStateInfoDict(); // should probably use a standard dictionary later (but this is good for debugging, I guess)
    public int WaterContainersFilledAmount = 0;
    public ResourceContainerAndStateInfoDict FoodContainersFilledState = new ResourceContainerAndStateInfoDict();
    public int FoodContainersFilledAmount = 0;
    public ResourceContainerAndStateInfoDict HappinessContainersWorkingState = new ResourceContainerAndStateInfoDict();
    public int HappinessContainersWorkingAmount = 0;
    public ResourceContainerAndStateInfoDict ComfortContainersWorkingState = new ResourceContainerAndStateInfoDict();
    public int ComfortContainersWorkingAmount = 0;


    public void Init() {
        for (int i = 0; i < ResourceContainer.AllResources.Count; i++) {
            if (!ResourceContainer.AllResources[i].CanBeTakenFrom)
                continue;

            switch (ResourceContainer.AllResources[i].Type) {
                case ResourceManager.ResourceType.Water:
                    WaterContainersFilledState.Add(ResourceContainer.AllResources[i], new StateInfo(true, 0));
                    WaterContainersFilledAmount++;
                    break;
                case ResourceManager.ResourceType.Food:
                    FoodContainersFilledState.Add(ResourceContainer.AllResources[i], new StateInfo(true, 0));
                    FoodContainersFilledAmount++;
                    break;
                case ResourceManager.ResourceType.Happiness:
                    HappinessContainersWorkingState.Add(ResourceContainer.AllResources[i], new StateInfo(true, 0));
                    HappinessContainersWorkingAmount++;
                    break;
                case ResourceManager.ResourceType.Comfort:
                    ComfortContainersWorkingState.Add(ResourceContainer.AllResources[i], new StateInfo(true, 0));
                    ComfortContainersWorkingAmount++;
                    break;
            }
        }
    }

    public void SetResourceContainerState(ResourceContainer _rc, bool state) {
        Dictionary<ResourceContainer, StateInfo> dictionary = null;
        switch (_rc.Type) {
            case ResourceManager.ResourceType.Water:
                dictionary = WaterContainersFilledState;
                if(dictionary[_rc].State != state)
                    WaterContainersFilledAmount += state ? 1 : -1; // update amount
                break;
            case ResourceManager.ResourceType.Food:
                dictionary = FoodContainersFilledState;
                if (dictionary[_rc].State != state)
                    FoodContainersFilledAmount += state ? 1 : -1; // update amount
                break;
            case ResourceManager.ResourceType.Happiness:
                dictionary = HappinessContainersWorkingState;
                if (dictionary[_rc].State != state)
                    HappinessContainersWorkingAmount += state ? 1 : -1; // update amount
                break;
            case ResourceManager.ResourceType.Comfort:
                dictionary = ComfortContainersWorkingState;
                if (dictionary[_rc].State != state)
                    ComfortContainersWorkingAmount += state ? 1 : -1; // update amount
                break;
        }

        dictionary[_rc].State = state;
        dictionary[_rc].Time = Time.time; // this'll have to be changed at some point
    }

    public Dictionary<ResourceContainer, StateInfo> GetResourceContainerStates(ResourceManager.ResourceType type) {
        switch (type) {
            case ResourceManager.ResourceType.Water:
                return WaterContainersFilledState;
            case ResourceManager.ResourceType.Food:
                return FoodContainersFilledState;
            case ResourceManager.ResourceType.Happiness:
                return HappinessContainersWorkingState;
            case ResourceManager.ResourceType.Comfort:
                return ComfortContainersWorkingState;
            default:
                throw new System.NotImplementedException(type + " hasn't been implemented to GetResourceContainerStates()!");
        }
    }
}
