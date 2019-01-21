using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class SetToolMode : MonoBehaviour {

	private static List<SetToolMode> allSetToolModeButtons = new List<SetToolMode>();

	[SerializeField] private BuildTool.ToolMode mode;
	[SerializeField] private GameObject expandableTab;
    
	private Toggle toggle;


    void Awake() {
        toggle = GetComponent<Toggle>();
    }

    void Start() {
		if (BuildTool.GetInstance().GetCurrentToolMode() != mode && expandableTab != null) {
			expandableTab.SetActive (false);
		}
    }

    void OnEnable() {
		allSetToolModeButtons.Add(this);

		if (toggle != null) { 
			toggle.onValueChanged.AddListener(OnToggleValueChanged);
		}
    }
    
	void OnDisable() {
		allSetToolModeButtons.Remove(this);

		if (toggle != null) { 
			toggle.onValueChanged.RemoveListener(OnToggleValueChanged);
		}
    }
    
	public void OnToggleValueChanged(bool _b) {
		if (_b) {
			BuildTool.GetInstance().SetCurrentToolMode(mode);
		}

		if (expandableTab != null) { 
			expandableTab.SetActive (_b);
		}
    }

	public static void SetMode(BuildTool.ToolMode _mode) {
		for (int i = 0; i < allSetToolModeButtons.Count; i++){
			SetToolMode setToolMode = allSetToolModeButtons[i];
			if (setToolMode.mode != _mode){
				continue;
			}

			// fake-press the actual button
			setToolMode.toggle.group.SetAllTogglesOff();
			setToolMode.toggle.isOn = true;
		}
	}
}
