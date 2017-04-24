using UnityEngine;
using UnityEngine.UI;

public class DisableColorButtonsOnDisable : MonoBehaviour {

	[SerializeField] private ColorAssignButton[] Buttons;

	void OnDisable(){
		for (int i = 0; i < Buttons.Length; i++) {
			Buttons [i].MyToggle.isOn = false;
			Buttons [i].OnToggleValueChanged (false);
		}
	}
}
