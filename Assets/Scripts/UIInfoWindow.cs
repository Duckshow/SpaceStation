using UnityEngine;
using UnityEngine.UI;

public class UIInfoWindow : MonoBehaviour {

    [HideInInspector]
    public CanInspect.State State = CanInspect.State.Default;
    [SerializeField]
    private Sprite Background_Default;
    [SerializeField]
    private Sprite Background_PickedUp;

    public Image UI_Image;
    public Text UI_Name;
    public Text UI_Health;
    public Text UI_Desc;
    public Button[] UI_ComponentSlots;

    [HideInInspector]
    public CanInspect OwnerInspectable;

    private Image ui_BackgroundImage;
    private ComponentObject trackedComponent;
    private Canvas myCanvas;

    private Image[] images;
    [HideInInspector]
    public Button[] Buttons;
    private Text[] texts;


    void Awake() {
        ui_BackgroundImage = GetComponent<Image>();
        myCanvas = FindObjectOfType<Canvas>();

        images = GetComponentsInChildren<Image>();
        Buttons = GetComponentsInChildren<Button>();
        texts = GetComponentsInChildren<Text>();
    }

    void Update() {
        if (trackedComponent != null)
            UpdateHealth(trackedComponent._Current_, trackedComponent.StaticInfo.HP_Max);
    }

    public void UpdateHealth(float _current, float _max) {
        UI_Health.text = ((int)((_current / _max) * 100)).ToString() + "%";
    }

    public void ChangeState(CanInspect.State _state) {
        switch (_state) {
            case CanInspect.State.Default:
                ui_BackgroundImage.sprite = Background_Default;
                break;
            case CanInspect.State.PickedUp:
                ui_BackgroundImage.sprite = Background_PickedUp;
                break;
        }

        State = _state;
    }

    public void ShowWindow(CanInspect _newIO, ComponentObject _trackedComponent = null) {
        ToggleVisible(true, _newIO, _trackedComponent);
    }
    public void HideWindow() {
        ToggleVisible(false);
    }
    void ToggleVisible (bool _enable, CanInspect _newIO = null, ComponentObject _trackedComponent = null) {
        if (OwnerInspectable != null && _enable) {
            ToggleVisible(false);
            // Debug.LogError(name + " already has an OwnerIO!");
        }

        OwnerInspectable = _newIO;
        trackedComponent = _trackedComponent;

        for (int i = 0; i < images.Length; i++)
            images[i].enabled = _enable;
        for (int i = 0; i < Buttons.Length; i++)
            Buttons[i].enabled = _enable;
        for (int i = 0; i < texts.Length; i++)
            texts[i].enabled = _enable;

        transform.SetParent(_enable ? myCanvas.transform : GUIManager.Instance.transform);
        if (!_enable)
            OwnerInspectable = null;
    }
}
