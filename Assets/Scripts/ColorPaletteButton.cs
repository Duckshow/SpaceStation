using UnityEngine;
using UnityEngine.UI;

[ExecuteInEditMode]
public class ColorPaletteButton : MonoBehaviour {

	[SerializeField] public Image SelectedButtonImage;
	private byte ColorIndex;
	private RectTransform myRectTransform;
	private Button myButton;
	private Image myImage;


	void Awake(){
		myRectTransform = GetComponent<RectTransform> ();
		myButton = GetComponent<Button> ();
		myImage = GetComponent<Image> ();
	}
	void OnEnable() {
		myButton.onClick.AddListener(OnButtonClick);
        Reload();
	}
	void OnDisable() {
		myButton.onClick.RemoveListener(OnButtonClick);
	}

	public void OnButtonClick() {
		ColorAssignButton.sActiveButton.AssignColor (ColorIndex);
	}

    [EasyButtons.Button]
    public void Reload() {
        if (!Application.isPlaying) {
            ColorIndex = (byte)transform.GetSiblingIndex();
            myImage.color = FindObjectOfType<Mouse>().Coloring.AllColors[ColorIndex];
        }
        else {
            ColorIndex = (byte)transform.GetSiblingIndex();
            myImage.color = Mouse.Instance.Coloring.AllColors [ColorIndex];
        }

		if (ColorAssignButton.sActiveButton != null && ColorAssignButton.sActiveButton.AssignedColorIndex == ColorIndex) {
			SelectedButtonImage.transform.SetParent (transform);
			SelectedButtonImage.transform.localPosition = myRectTransform.rect.center;
		}
    }
}
