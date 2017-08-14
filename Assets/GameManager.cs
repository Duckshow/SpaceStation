using UnityEngine;

public class GameManager : MonoBehaviour {

    public static GameManager Instance;

	[Header("How many sorting transforms can a tile contain?")]
    public byte SortingTransformsPerPosY = 10;


    void Awake() {
        Instance = this;
    }
}
