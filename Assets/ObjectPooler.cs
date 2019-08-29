using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour {
	public enum PoolableID { None, UVController, IncandescenceLamp }

    [Serializable]
	public class Pool {
		private const int MAX_POOLED = 500;
        public List<PoolableID> IDs = new List<PoolableID>();
        public PoolerObject Prefab;
        public ObjectPooler Owner;
        private Queue<PoolerObject> available;
        private int pooledCount = 0;

		public Pool(ObjectPooler _owner, PoolableID _ID, PoolerObject _prefab){
			Owner = _owner;
			Prefab = _prefab;
			IDs.Add(_ID);
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

	public static ObjectPooler Instance;
	[SerializeField] private List<Pool> MyPools = new List<Pool>();

	void Awake(){
		if (Instance != null)
			Destroy(this);
		else
			Instance = this;

		SetupPoolIDsArray();

		// check for duplicate ID usage (not sure how else to prevent double usage...)
		List<PoolableID> usedIDs = new List<PoolableID>();
		for (int i = 0; i < MyPools.Count; i++){
			for (int j = 0; j < MyPools[i].IDs.Count; j++){
				if (usedIDs.Contains(MyPools[i].IDs[j]))
					Debug.LogError("Two Pools are using " + MyPools[i].IDs[j] + " as ID!");
				else
					usedIDs.Add(MyPools[i].IDs[j]);
			}
		}
	}
	

	public void AddPool(PoolableID _id, PoolerObject _prefab){
		MyPools.Add(new Pool(this, _id, _prefab));
	}

	private PoolableID[] allPoolIDs;
	private int[] idsPerPool;
	void SetupPoolIDsArray(){
		idsPerPool = new int[MyPools.Count];
		int _total = 0;
		for (int i = 0; i < MyPools.Count; i++){
			idsPerPool[i] = MyPools[i].IDs.Count;
			_total += idsPerPool[i];
		}
		allPoolIDs = new PoolableID[_total];

		_total = 0;
		for (int i = 0; i < idsPerPool.Length; i++){
			for (int i2 = 0; i2 < idsPerPool[i]; i2++){
				allPoolIDs[_total] = MyPools[i].IDs[i2];
				_total++;
			}
		}
	}
    public T GetPooledObject<T>(PoolableID _id) where T : Component {
		Pool _pool = GetPoolForID(_id);
		if(_pool == null)
	        return null;
		return _pool.GetObject().GetComponent<T>();
	}
	public bool HasPoolForID(PoolableID _id){
		return GetPoolForID(_id) != null;
	}
	public Pool GetPoolForID(PoolableID _id){
		Pool _pool = null;
		int _idIndex = 0;
		int _poolIndex = 0;

		for (int i = 0; i < allPoolIDs.Length; i++){
			PoolableID _poolID = allPoolIDs[i];
			if (_poolID == _id)
				_pool = MyPools[_poolIndex];

			_idIndex++;
			if (_idIndex == idsPerPool[_poolIndex]) { 
				_idIndex = 0;
				_poolIndex++;
			}
		}

		return _pool;
	}

	// should only be used in editor since it's not optimized, and optimizing it could be a bit annoying.
	public bool HasPoolForID_EDITOR(PoolableID _id){
		for (int i = 0; i < MyPools.Count; i++){
			for (int j = 0; j < MyPools[i].IDs.Count; j++){
				if(MyPools[i].IDs[j] == _id)
	                return true;
			}
        }
		return false;
	}
}
