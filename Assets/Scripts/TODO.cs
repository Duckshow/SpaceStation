/*

== Last time ==

== Next time ==

== Goal ==
Get coloring working!
"1.@frag check the R of the underlying pixel of the pallette map (0-3)
 2.use the R to check the corresponding vertex color channel
 3.use that value to determine which color variable to use."


== Needs to be done ==
Doors need a higher cost in pathfinding!


== Known bugs ==
The Idling-task is completely broken and can't find any paths.

Sitting down on a chair by a TV and eating, then getting bored while eating, may cause the unit to schedule a task to watch tv, but it's already sitting
on the only tv-chair and so finds no comfort. Maybe check *who's* using a chair?

Putting two doors/airlocks next to eachother and deleting a wall connected to only one of them causes one door to be deleted but not the other one, leaving
a door without a wall. Need to check for neighboring doors' neighboring doors, etc etc!



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