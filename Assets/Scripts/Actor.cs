using UnityEngine;

public class Actor : MonoBehaviour {

    public const float SPEED_STROLL = 1f;
    public const float SPEED_WALK = 2f;
    public const float SPEED_RUN = 4f;
    public const float RATE_CONSUME = 2;

    TaskHandler taskHandler = new TaskHandler();

    public int HairStyleIndex = 0;
    public int HeadIndex = 0;
    public int EyeIndex = 0;
    public int BeardIndex = 0;

    [SerializeField] private ResourceContainer thirst;
    [SerializeField] private ResourceContainer hunger;
    [SerializeField] private ResourceContainer happiness;
    [SerializeField] private ResourceContainer comfort;

    public KnowledgeBase Knowledge = new KnowledgeBase();

    public string CurrentTask;
    public float HeldWater;
    public float HeldFood;

    [HideInInspector] public ActorOrientation Orienter;


    void Awake() {
        Orienter = GetComponent<ActorOrientation>();
    }
    void Start() {
        taskHandler.Owner = this;
        //currentSector = Sector.GetSectorFromPos(transform.position);

        thirst.OwnerPawn = this;
        hunger.OwnerPawn = this;
        happiness.OwnerPawn = this;
        comfort.OwnerPawn = this;

        Knowledge.Init();
    }

    //void OnTriggerEnter(Collider coll) {
    //    if (coll.gameObject.layer == Sector.SectorLayer) {
    //        Debug.Log("New sector: " + coll.name);
    //        currentSector = coll.GetComponent<Sector>();
    //    }
    //}

    private Node currentTile;
    void Update() {

        // if actor is in a special tile, don't think about going somewhere else
        currentTile = GameGrid.GetInstance().GetNodeFromWorldPos(transform.position);

        // -- Find water --
        if ((HeldWater > 0 || Knowledge.WaterContainersFilledAmount > 0) && !taskHandler.ResourcesPendingFetch.Contains(ResourceManager.ResourceType.Water) && thirst._Current_ <= thirst.Max * 0.5f) {

            if (HeldWater > 0) {
                // take a sip of water
                taskHandler.AssignTask(new MetaTask(taskHandler, MetaTask.Priority.High,
                    new Task.ConsumeHeldResource(taskHandler, thirst, false)));
            }
            else {
                ResourceContainer nearestWater = ResourceContainer.GetClosestAvailableResource(this, ResourceManager.ResourceType.Water);
                if (nearestWater == null) {
                    throw new System.NotImplementedException("No water could be found! PANIC!");
                }
                else {

                    // hurry if emergency
                    MetaTask.Priority priority = MetaTask.Priority.Medium;
                    if (thirst._Current_ <= thirst.Max * 0.25f) {
                        // hurry
                        priority = MetaTask.Priority.High;
                        // maybe pawns should get a general speed-decrease when thirsty?
                    }

                    // go to source and drink
                    taskHandler.AssignTask(new MetaTask(taskHandler, priority,
                        new Task.Move(nearestWater.transform.position, SPEED_WALK),
                        new Task.TakeResource(taskHandler, thirst, nearestWater, thirst.Max - thirst._Current_, false)));
                }
            }
        }
        // -- Find food --
        if ((HeldFood > 0 || Knowledge.FoodContainersFilledAmount > 0) && !taskHandler.ResourcesPendingFetch.Contains(ResourceManager.ResourceType.Food) && hunger._Current_ <= hunger.Max * 0.75f) {

            ResourceContainer _availableComfort = null;
            if (HeldFood > 0) {
                _availableComfort = ResourceContainer.GetClosestAvailableResource(this, ResourceManager.ResourceType.Comfort);
                if (_availableComfort == null)
                    throw new System.NotImplementedException("Couldn't find comfort! This should be made impossible!");

                // go to seat and eat
                taskHandler.AssignTask(new MetaTask(taskHandler, MetaTask.Priority.Medium,
                    new Task.Move(_availableComfort.transform.position, SPEED_WALK),
                    new Task.TakeResource(taskHandler, comfort, _availableComfort, comfort.Max - comfort._Current_, false),
                    new Task.ConsumeHeldResource(taskHandler, hunger, true)));
            }
            else {
                ResourceContainer nearestFood = null;
                nearestFood = ResourceContainer.GetClosestAvailableResource(this, ResourceManager.ResourceType.Food);
                if (nearestFood == null) {
                    throw new System.NotImplementedException("No food could be found! PANIC!");
                }
                else if(nearestFood.DispensesHeldCaches){

                    // fetch food
                    taskHandler.AssignTask(new MetaTask(taskHandler, MetaTask.Priority.Low,
                        new Task.Move(nearestFood.transform.position, SPEED_WALK),
                        new Task.TakeResource(taskHandler, hunger, nearestFood, hunger.Max - hunger._Current_, false)));  // todo: add queue-formation
                }
                else {

                    //throw new System.Exception("Eating out of tubes is no longer permitted. All food dispensers must dispense food caches!");

                    // go to source and eat (maybe remove later, amiright?) 
                    taskHandler.AssignTask(new MetaTask(taskHandler, MetaTask.Priority.Low,
                       new Task.Move(nearestFood.transform.position, SPEED_WALK),
                       new Task.TakeResource(taskHandler, hunger, nearestFood, hunger.Max - hunger._Current_, false))); // todo: add queue-formation
                }
            }
        }
        // -- Find happiness --
        if (!taskHandler.ResourcesPendingFetch.Contains(ResourceManager.ResourceType.Happiness) && happiness._Current_ <= happiness.Max * 0.8f) {
            ResourceContainer nearestHappiness = ResourceContainer.GetClosestAvailableResource(this, ResourceManager.ResourceType.Happiness);
            if (nearestHappiness == null) {
                throw new System.NotImplementedException("No happiness could be found! PANIC!");
            }
            else {

                // IF NEARESTHAPPINESS IS A TV
                // find availableSeating before task!

                if (nearestHappiness is TV) {
                    TV _TV = nearestHappiness as TV;

                    ResourceContainer _availableComfort = ResourceContainer.GetClosestSeatByTV(this, _TV);
                    if (_availableComfort == null)
                        throw new System.NotImplementedException("Couldn't find comfort! This should be made impossible!");

                    // go to seat and watch TV
                    if (_availableComfort != null) {
                        taskHandler.AssignTask(new MetaTask(taskHandler, MetaTask.Priority.Lowest,
                            new Task.Move(_availableComfort.transform.position, SPEED_WALK),
                            new Task.TakeResource(taskHandler, comfort, _availableComfort, comfort.Max - comfort._Current_, false),
                            new Task.TakeResource(taskHandler, happiness, nearestHappiness, happiness.Max - happiness._Current_, true)));
                    }
                    // stand by TV and watch
                    else { // TODO: Comfort should always be available by sitting on the floor!
                        Vector3 _viewingPos = new Vector3();
                        _viewingPos.x = Random.Range(_TV.ViewingArea.bounds.min.x, _TV.ViewingArea.bounds.max.x);
                        _viewingPos.y = Random.Range(_TV.ViewingArea.bounds.min.y, _TV.ViewingArea.bounds.max.y);

                        taskHandler.AssignTask(new MetaTask(taskHandler, MetaTask.Priority.Lowest,
                            new Task.Move(_viewingPos, SPEED_WALK),
                            new Task.TakeResource(taskHandler, comfort, _availableComfort, comfort.Max - comfort._Current_, false),
                            new Task.TakeResource(taskHandler, happiness, nearestHappiness, happiness.Max - happiness._Current_, true)));
                    }
                }
            }
        }
        // -- Find comfort --
        if (Knowledge.ComfortContainersWorkingAmount > 0 && !taskHandler.ResourcesPendingFetch.Contains(ResourceManager.ResourceType.Comfort) && comfort._Current_ <= comfort.Max * 0.25f) {
            ResourceContainer _nearestComfort = ResourceContainer.GetClosestAvailableResource(this, ResourceManager.ResourceType.Comfort);
            if (_nearestComfort == null) {
                throw new System.NotImplementedException("No comfort could be found! PANIC!");
            }
            else {

                // go to seat and sit 
                taskHandler.AssignTask(new MetaTask(taskHandler, MetaTask.Priority.Lowest, // maybe pawns should get a general speed-decrease?
                    new Task.Move(_nearestComfort.transform.position, SPEED_STROLL),
                    new Task.TakeResource(taskHandler, comfort, _nearestComfort, comfort.Max - comfort._Current_, false))); // todo: add queue-formation
            }
        }

        // TODO: idling is broken, doesn't find any paths. Fix.

        // if pawn still has nothing to do, idle ( this should pretty much never happen )
        //if (taskHandler.PendingTasks.Count == 0) {
        //    Debug.Log("Idling...");
        //    Tile _tile = Grid.Instance.GetRandomWalkableNode(/*Grid.Instance.GetNodeFromWorldPoint(transform.position)*/);
        //    _tile = Grid.Instance.GetClosestFreeNode(_tile);
        //    taskHandler.AssignTask(new MetaTask(taskHandler, MetaTask.Priority.Lowest,
        //        new Task.Move(_tile.WorldPosition + _tile.CenterPosition, SPEED_STROLL)));
        //}
    }
}
