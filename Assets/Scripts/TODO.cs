/*

== Last time ==

== Next time ==

== Goal ==
Also need shader-stuff for lighting. I could have regular lights on top of the 2D-lights (ehh), but another solution might be this;
- Create new shader for the grid (with coloring-stuff)
- Each light adds its position, range, brightness and color to four arrays in the shader
- In the frag, find the four (?) lights that have the highest impact on this pixel (distance * (brightness * intensity))
- Find the direction to each light and create multipliers based on the color of the normal map
- Apply those lights' properties to the frag, using the multipliers
(update: this approach won't work entirely as the normal-maps require an angle for the light to come from. 
If it always come from the side, it's gonna look like shit. But hmm, maybe if we don't completely mimic a normal light and only factor in the light's direction
on some color of the normal-map? Either way, start with using regular point-lights I guess, then check performance.)


== Needs to be done ==
Floor in front of doors/airlocks should force the use of the DefaultPosition when walking on it, to prevent actors shortcutting through walls

Doors need a higher cost in pathfinding!

Replace Actors' SpriteRenderers with UVControllers. Not sure if there's a reason for it being like this, but they currently don't use the sorting
system like everything else which is pretty shit.


== Known bugs ==
Building-tools currently seem to skyrocket in the profiler (750 kb GC collect at one point and then memory-crash!)

The Idling-task is completely broken and can't find any paths.

Sitting down on a chair by a TV and eating, then getting bored while eating, may cause the unit to schedule a task to watch tv, but it's already sitting
on the only tv-chair and so finds no comfort. Maybe check *who's* using a chair?

Opening the color-palette window causes a huge lagspike. Might be the amount of buttons, I guess.

If a path to an activity can't be found, the task won't be cancelled and the actor will complete the task anyway, despite not being physically there

Light can sometimes pass between two diagonally-placed tiles. There is code to prevent this, but I gues it's not foolproof. Fix if it continues to look bad 
after the lightshader is completed. Also worth considering is that placing tiles like this might not be very common usage...

A Gridcast can miss its target and continue forever (will be caught, but still) but it doesn't seem to happen when the light is centered on a tile, so maaaybe
it's fine, for now at least? I don't think I need moving or offsetted lights... 


== Ideas for the future ==

__Replace TileQuads with MegaQuad!__ 

Why? : Instead of >10.000 transforms I should be able to have 5 (eh, 250 if I want depthsorting). Jesus Christ that sounds nice.

How? : If I had a plane with X quads, I should be able to set the UVs of those quads instead. However, this would mean I can't have depth sorting, unless
I instead have one megaquad, 2 units high, for each Y-unit. That could get me 48 transforms at least, going by the current gridsize.
_________________

__Replace unity-lights with shader__ 

Why? : Better control over lighting and possibly better for performance.

How? : Everytime a lamp-object is added to the grid, 3 static arrays are assigned with every lamp's position, range, color (maybe intensity?).
The arrays can be sent to a shader via MaterialPropertyBlocks. Those arrays are iterated over in the grid-shader, applying maybe the 4 strongest lights to each pixel.
_________________

__Floor-windows__ 

Why? : Could give more 3D-feeling to the ship.

How? : Should be doable - you could mark a sprite as semitransparent with a bool, then if drawing such a sprite and encountering a pixel with
Alpha < 1, combine that pixels color with the below color.
_________________

*/