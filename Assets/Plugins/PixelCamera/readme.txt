======================
== PixelCamera v1.0 ==
==     by Tinnus    ==
======================

Hello and thanks for buying this asset! I hope you like it.
Please send comments, suggestions and report any problems to "tinnus+assetstore@gmail.com".
I'd appreciate it if you include "[PixelCamera]" in the subject to help me filter e-mails :)


1. Introduction

This asset is intended to be used as a "low-resolution" effect of sorts in a camera inside the Unity scene.
Features included are:
- Reduce your camera's resolution to any arbitrary resolution.
- Three modes: based on width, based on height and based on both. "Based on both" lets you fully customize the target resolution, while the other two ensure square pixels by letting you set one of the two dimensions of the screen and calculating the other according to aspect ratio.
- "Color depth" option to easily reduce the output color depth to 16 or 12 bits per pixel. Note that you can also use custom shaders to do this, but this option will often have better performance.
- Support for a custom material to be applied to the screen image after it's rendered in low-res. This can be used for reducing color depth, dithering, or applying any image effects you'd like AFTER the image is rendered to the low-res buffer.


2. How to use

You can add a new PixelCamera to your scene by following the steps outlined below:
- Add a new empty GameObject to your scene.
- Attach the "PixelCamera" script to it.
- Find in your scene the camera you're currently using to render the game. Drag it to the "Target Camera" parameter in the Pixel Camera inspector.
- Choose the scale mode you want to use.
   - "Based on width" lets you set the width of the image and sets the height automatically based on aspect ratio.
   - "Based on height" lets you set the height of the image and sets the width automatically based on aspect ratio.
   - "Based on both" lets you set both the width & height of the image. Please note that this may cause non-square pixels if you don't set a resolution that matches the screen aspect ratio.
- Set the desired resolution accoring to what you chose above.


3. Custom materials

You can set a material in the "Custom material" field in the Pixel Camera inspector to add extra processing to the final low-res image after it's already drawn. This package comes with an example shader that reduces the color depth of the rendered image to an arbitrary number of shades for red, green and blue, along with gamma correction. Please refer to it as a reference.
Notes:
- You shader MUST have a main texture parameter (called "_MainTex") for the effect to work. This is the default texture parameter used in all Unity shaders and is what the script expects.


4. How to work with UI

This package is fully compatible with the new Unity UI (intruduced in Unity 4.6).
- If you want your UI to be rendered at full-res (without being affected by the effect), set your Canvas Render Mode to "Screen Space - Overlay".
- If you want your UI to be affected by the low-res effect, set your Canvas Render Mode to "Screen Space - Camera" and drag your gameplay camera (the same setup in the Pixel Camera script) to the "Render Camera" parameter in the Canvas inspector.



