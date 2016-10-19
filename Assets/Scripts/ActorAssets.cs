using UnityEngine;
using System.Collections;

public class ActorAssets : MonoBehaviour {

    public static ActorAssets Instance;

    [System.Serializable]
    public struct OrientedAsset {
        [SerializeField] private Sprite Back;
        [SerializeField] private Sprite Front;
        [SerializeField] private Sprite Left;
        [SerializeField] private Sprite Right;

        public Sprite GetOrientedAsset(ActorOrientation.OrientationEnum _orientation) {
            switch (_orientation) {
                case ActorOrientation.OrientationEnum.Down:
                    return Front;
                case ActorOrientation.OrientationEnum.Up:
                    return Back;
                case ActorOrientation.OrientationEnum.Left:
                    return Left;
                case ActorOrientation.OrientationEnum.Right:
                    return Right;
            }

            return null;
        }
    }
    public OrientedAsset[] HairStyles;
    public OrientedAsset[] Heads;
    public OrientedAsset[] Eyes;
    public OrientedAsset[] Beards;


    void OnEnable() {
        Instance = this;
    }
    void OnDisable() {
        Instance = null;
    }
}
