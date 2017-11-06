using UnityEngine;

public class PoolerObject : MonoBehaviour {
    public ObjectPooler.Pool ParentPool;
    public void ReturnToPool() {
        ParentPool.ReturnObject(this);
    }
}
