using System.Collections.Generic;
using UnityEngine;

public class ColorManager : Singleton<ColorManager> {

	public const int COLOR_COUNT = 128;
	public const int COLOR_CHANNEL_COUNT = 10;

	public enum ColorName : byte {
		White = 0,
		OffWhite = 1,
		Grey = 8,
		Red = 124,
		Orange = 36
	}

	public enum ColorUsage {
		Default,
		Selected,
		New,
		AlreadyExisting,
		Blocked,
		Delete
	}

	private static Dictionary<ColorUsage, ColorName> contextColors = new Dictionary<ColorUsage, ColorName> { { ColorUsage.Default, ColorName.White },
		{ ColorUsage.Selected, ColorName.Grey },
		{ ColorUsage.New, ColorName.OffWhite },
		{ ColorUsage.AlreadyExisting, ColorName.Grey },
		{ ColorUsage.Blocked, ColorName.Orange },
		{ ColorUsage.Delete, ColorName.Red },
	};

	[SerializeField]
	private Texture2D paletteTexture;
	[SerializeField]
	private Color[] allColors;
	[SerializeField]
	private Material materialGrid;

	private static List<Vector4> allColorsForShaders = new List<Vector4>();

	void OnValidate() {
		Color[] pixels;

		pixels = paletteTexture.GetPixels();
		allColors = new Color[pixels.Length];

		int _index = 0;
		for(int _y =(paletteTexture.height - 1); _y >= 0; _y--) {
			for(int _x = 0; _x < paletteTexture.width; _x++) {
				int pixelIndex =(paletteTexture.width * _y) + _x;
				allColors[_index] = pixels[pixelIndex];
				_index++;
			}
		}
	}

	public override bool IsUsingAwakeEarly() { return true; }
	public override void AwakeEarly() {
		materialGrid.SetColorArray("allColors", allColors);
	}

	public static Color GetColor(ColorName colorName) {
		return ColorManager.GetInstance().allColors[(byte)colorName];
	}

	public static Color GetColor(int colorIndex) {
		if(Application.isPlaying) {
			return ColorManager.GetInstance().allColors[colorIndex];
		}
		else {
			return FindObjectOfType<ColorManager>().allColors[colorIndex];
		}
	}

	public static byte GetColorIndex(ColorUsage usage) {
		return(byte)contextColors[usage];
	}
}