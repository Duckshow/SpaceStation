using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RaycastTest : MonoBehaviour {

    [EasyButtons.Button]
    public void Pew() {
		for (int i = 0; i < 300; i++){
			RaycastHit2D ray = Physics2D.Raycast(transform.position, new Vector2(Random.value * 2 - 1, Random.value * 2 - 1), Mathf.Infinity);
			if(ray.collider != null)
                ray.collider.transform.position = ray.collider.transform.position + Vector3.up;
        }
    }

    bool _b = true;
    [EasyButtons.Button]
    public void ToggleSimulate(){
        _b = !_b;

        Rigidbody2D[] colliders = FindObjectsOfType<Rigidbody2D>();
		for (int i = 0; i < colliders.Length; i++){
            colliders[i].simulated = _b;
        }
    }
}
