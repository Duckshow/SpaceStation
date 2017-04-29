using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ColorPaletteButton : MonoBehaviour {

	private byte ColorIndex;
	private Button myButton;
	private Image myImage;


	void Awake(){
		myButton = GetComponent<Button> ();
		myImage = GetComponent<Image> ();
	}
	void OnEnable() {
		myButton.onClick.AddListener(OnToggleValueChanged);
        Reload();
	}
	void OnDisable() {
		myButton.onClick.RemoveListener(OnToggleValueChanged);
	}

	public void OnToggleValueChanged() {
		ColorAssignButton.sActiveButton.AssignColor (ColorIndex);
	}

    [EasyButtons.Button]
    public void Reload() {
#if UNITY_EDITOR
        ColorIndex = (byte)transform.GetSiblingIndex();
        myImage.color = FindObjectOfType<Mouse>().Coloring.AllColors [ColorIndex];
	#else
		myImage.color = Mouse.Instance.Coloring.AllColors [ColorIndex];
	#endif
    }
}
