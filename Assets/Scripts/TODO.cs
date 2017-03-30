/*

== Last time ==

== Next time ==


== Goal ==


== Needs to be done ==
Doors need a higher cost in pathfinding!


== Known bugs ==
The Idling-task is completely broken and can't find any paths.

Sitting down on a chair by a TV and eating, then getting bored while eating, may cause the unit to schedule a task to watch tv, but it's already sitting
on the only tv-chair and so finds no comfort. Maybe check *who's* using a chair?




== Ideas for the future ==

__Replace TileQuads with MegaQuad!__ 

Why? : Instead of >10.000 transforms I should be able to have 5. Jesus Christ that sounds nice.

How? : If I had a plane with X quads, I should be able to set the UVs of those quads instead. However, this would mean I can't have depth sorting, unless
I instead have one megaquad, 2 units high, for each Y-unit. That could get me 48 transforms at least, going by the current gridsize.
_________________

__Instead of mouseghosts, just change the grid temporarily__ 

Why? : Instead of, what, 800~ additional transforms, I could have zero and possibly the ability to select a LOT more stuff at a time.

How? : Add a second DoubleInt to UVController that will be used unless null, then set that using BuilderBase.

_________________

__Floor-windows__ 

Why? : Could give more 3D-feeling to the ship.

How? : Should be doable - you could mark a sprite as semitransparent with a bool, then if drawing such a sprite and encountering a pixel with
Alpha < 1, combine that pixels color with the below color.
_________________

*/