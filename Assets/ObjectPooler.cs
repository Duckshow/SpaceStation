using System;
using System.Collections.Generic;
using UnityEngine;

public class ObjectPooler : MonoBehaviour {

	public static ObjectPooler Instance;

	public enum IDEnum{ 
		wall_Single_shadow,
		wall_FourWay_shadow,
		wall_Vertical_T_shadow,
		wall_Vertical_M_shadow,
		wall_Vertical_B_shadow,
		wall_Horizontal_L_shadow,
		wall_Horizontal_M_shadow,
		wall_Horizontal_R_shadow,
		wall_Corner_TopRight_shadow,
		wall_Corner_TopLeft_shadow,
		wall_Corner_BottomRight_shadow,
		wall_Corner_BottomLeft_shadow,
		wall_Tee_Right_shadow,
		wall_Tee_Left_shadow,
		wall_Tee_Top_shadow,
		wall_Tee_Bottom_shadow,
		wall_Diagonal_TopRight_shadow,
		wall_Diagonal_TopRight_T_shadow,
		wall_Diagonal_TopRight_R_shadow,
		wall_Diagonal_TopRight_TR_shadow,
		wall_Diagonal_TopLeft_shadow,
		wall_Diagonal_TopLeft_T_shadow,
		wall_Diagonal_TopLeft_L_shadow,
		wall_Diagonal_TopLeft_TL_shadow,
		wall_Diagonal_BottomRight_shadow,
		wall_Diagonal_BottomRight_B_shadow,
		wall_Diagonal_BottomRight_R_shadow,
		wall_Diagonal_BottomRight_BR_shadow,
		wall_Diagonal_BottomLeft_shadow,
		wall_Diagonal_BottomLeft_B_shadow,
		wall_Diagonal_BottomLeft_L_shadow,
		wall_Diagonal_BottomLeft_BL_shadow,
		anim_DoorVertical_Open_shadow,
		anim_DoorVertical_Close_shadow,
		anim_DoorHorizontal_Open_shadow,
		anim_DoorHorizontal_Close_shadow,
		anim_AirlockHorizontal_Open_B_shadow,
		anim_AirlockHorizontal_Close_B_shadow,
		anim_AirlockHorizontal_Open_T_shadow,
		anim_AirlockHorizontal_Close_T_shadow,
		anim_AirlockHorizontal_Wait_shadow,
		anim_AirlockVertical_Open_L_shadow,
		anim_AirlockVertical_Close_L_shadow,
		anim_AirlockVertical_Open_R_shadow,
		anim_AirlockVertical_Close_R_shadow,
		anim_AirlockVertical_Wait_shadow,
	}


    [Serializable]
	public class Pool {

        private const int MAX_POOLED = 500;
        public IDEnum ID;
        public PoolerObject Prefab;
        [NonSerialized] public ObjectPooler Owner;
        [NonSerialized] public Queue<PoolerObject> Available = new Queue<PoolerObject>();
        private int pooledCount = 0;


		// return an available object or spawn a new one
        public GameObject GetObject() {
            if (Available.Count == 0) {
                PoolerObject newObject = Instantiate(Prefab, Vector3.zero, Quaternion.identity);
                newObject.ParentPool = this;
                pooledCount++;

				if(pooledCount > MAX_POOLED)
                    Debug.LogWarning("The " + ID + " Pool is at " + pooledCount + " objects! This is probably not normal D:");

                return newObject.gameObject;
            }

            Available.Peek().gameObject.SetActive(true);
            return Available.Dequeue().gameObject;
        }

		// return a pooled object to the pool
        public void ReturnObject(PoolerObject _obj) {
            _obj.gameObject.SetActive(false);
            _obj.transform.parent = Owner.transform;
            _obj.transform.localPosition = Vector3.zero;
            Available.Enqueue(_obj);
        }
    }

    [SerializeField] private Pool[] MyPools;

	// return the pool using the provided ID
    public Pool GetPool(IDEnum _id) { 
		for (int i = 0; i < MyPools.Length; i++){
			if(MyPools[i].ID == _id)
                return MyPools[i];
        }

        Debug.LogError("Couldn't find a pool using " + _id + "!");
        return null;
    }

    void Awake() {
        if (Instance != null)
            Destroy(this);
		else
            Instance = this;

        // check for duplicate ID usage (not sure how else to prevent double usage...)
        List<IDEnum> usedIDs = new List<IDEnum>();
        for (int i = 0; i < MyPools.Length; i++){
			if(usedIDs.Contains(MyPools[i].ID))
                Debug.LogError("Two Pools are using " + MyPools[i].ID + " as ID!");
			else
                usedIDs.Add(MyPools[i].ID);
        }
	}
}
