using UnityEngine;
using UnityEngine.UI;

public class SetToolMode : MonoBehaviour {
    public Mouse.BuildModeEnum ToggleMode;
    public int ToolIndex = -1;
	[SerializeField] private GameObject ExtraStuff;
	[SerializeField] private SetToolMode OverrideOtherButton;
	[System.NonSerialized] public bool IsOverriden;
    [System.NonSerialized] public Mouse.BuildModeEnum OverrideToggleMode;
    [System.NonSerialized] public int OverrideToolIndex;
    [System.NonSerialized] public Toggle MyToggle;


    void Awake() {
        MyToggle = GetComponent<Toggle>();
		if(ExtraStuff != null)
			ExtraStuff.SetActive (false);
    }
    void OnEnable() {
        MyToggle.onValueChanged.AddListener(OnToggleValueChanged);
    }
    void OnDisable() {
        MyToggle.onValueChanged.RemoveListener(OnToggleValueChanged);
    }
    public void OnToggleValueChanged(bool _b) {
        if (OverrideOtherButton != null) {
            if (_b) {
                OverrideOtherButton.IsOverriden = true;
                OverrideOtherButton.OverrideToggleMode = ToggleMode;
                OverrideOtherButton.OverrideToolIndex = ToolIndex;
                OverrideOtherButton.OnToggleValueChanged(true);
            }
            else {
                OverrideOtherButton.IsOverriden = false;
                OverrideOtherButton.OverrideToggleMode = Mouse.BuildModeEnum.None;
                OverrideOtherButton.OverrideToolIndex = -1;
                OverrideOtherButton.OnToggleValueChanged(true);
            }
        }

		if (_b) {
			if(IsOverriden)
				Mouse.Instance.OnBuildModeChange(OverrideToolIndex, OverrideToggleMode);
			else
				Mouse.Instance.OnBuildModeChange(ToolIndex, ToggleMode);
		} 

		if(ExtraStuff != null)
			ExtraStuff.SetActive (_b);
    }
}
