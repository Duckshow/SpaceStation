using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour {

	public static ObjectPooler Instance;

    [Serializable]
	public class Pool {

        private const int MAX_POOLED = 500;
        public List<CachedAssets.WallSet.P> IDs = new List<CachedAssets.WallSet.P>();
        public PoolerObject Prefab;
        public ObjectPooler Owner;
        private Queue<PoolerObject> available;
        private int pooledCount = 0;

		public Pool(ObjectPooler _owner, CachedAssets.WallSet.P _id, PoolerObject _prefab, CachedAssets.WallSet.P[] _additionalIDs){
			Owner = _owner;
			Prefab = _prefab;
			IDs.Add(_id);
			IDs.AddRange(_additionalIDs);
		}

		// return an available object or spawn a new one
        public GameObject GetObject() {
			if(available == null)
				available = new Queue<PoolerObject>();
            if (available.Count == 0) {
                PoolerObject newObject = Instantiate(Prefab, Vector3.zero, Quaternion.identity);
                newObject.ParentPool = this;
                pooledCount++;

				if(pooledCount > MAX_POOLED)
                    Debug.LogWarning("The " + IDs[0] + " Pool is at " + pooledCount + " objects! This is probably not normal D:");

                return newObject.gameObject;
            }

            available.Peek().gameObject.SetActive(true);
            return available.Dequeue().gameObject;
        }

		// return a pooled object to the pool
        public void ReturnObject(PoolerObject _obj) {
            _obj.gameObject.SetActive(false);
            _obj.transform.parent = Owner.transform;
            _obj.transform.localPosition = Vector3.zero;
            available.Enqueue(_obj);
        }
    }

    [SerializeField] private List<Pool> MyPools = new List<Pool>();
	public void AddPool(CachedAssets.WallSet.P _id, PoolerObject _prefab, params CachedAssets.WallSet.P[] _additionalIDs){
		MyPools.Add(new Pool(this, _id, _prefab, _additionalIDs));
	}

	// return the pool using the provided ID
    public T GetPooledObject<T>(CachedAssets.WallSet.P _id) where T : Component {
		Pool _pool = null; 
		for (int i = 0; i < MyPools.Count; i++){
			for (int j = 0; j < MyPools[i].IDs.Count; j++){
				if(MyPools[i].IDs[j] == _id)
	                _pool = MyPools[i];
			}
        }

		if(_pool == null)
	        return null;
    
		return _pool.GetObject().GetComponent<T>();
	}
	public bool HasPoolForID(CachedAssets.WallSet.P _id){
		for (int i = 0; i < MyPools.Count; i++){
			for (int j = 0; j < MyPools[i].IDs.Count; j++){
				if(MyPools[i].IDs[j] == _id)
	                return true;
			}
        }
		return false;
	}

    void Awake() {
        if (Instance != null)
            Destroy(this);
		else
            Instance = this;

        // check for duplicate ID usage (not sure how else to prevent double usage...)
        List<CachedAssets.WallSet.P> usedIDs = new List<CachedAssets.WallSet.P>();
		for (int i = 0; i < MyPools.Count; i++){
			for (int j = 0; j < MyPools[i].IDs.Count; j++){
				if(usedIDs.Contains(MyPools[i].IDs[j]))
					Debug.LogError("Two Pools are using " + MyPools[i].IDs[j] + " as ID!");
				else
					usedIDs.Add(MyPools[i].IDs[j]);
			}
        }
	}
}
