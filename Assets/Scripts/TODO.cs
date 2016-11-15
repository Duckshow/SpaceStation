/*

== Last time ==
Added two tiletypes (DoorEntrance, MiscBlocker) to be used for creating functioning doors.

== Next time ==
Get the pathfinding to detect DoorEntrance-tiles and if the path is going through a door, and add a waiting time to those waypoints.


== Goal ==


== Needs to be done ==
Doors need a higher cost in pathfinding!

Add "empty" sprite to the wall-sprites that I can use for the delete-tool, for example. Solid 64x64 should do it.


== Known bugs ==
Diagonals can't write themselves over doors because doors have a top-layer - make the extra part of diagonals top-layer as well!

The Idling-task is completely broken and can't find any paths.

Picking up a component, then switching it for another component on the ground, then cancelling, will get you two components lying on the same position.
This'll presumably be fixed later with a tile-system.

Sitting down on a chair by a TV and eating, then getting bored while eating, may cause the unit to schedule a task to watch tv, but it's already sitting
on the only tv-chair and so finds no comfort. Maybe check *who's* using a chair?




== Ideas for the future ==

__Floor-windows__ 

Why? : Could give more 3D-feeling to the ship.

How? : Should be doable - you could mark a sprite as semitransparent with a bool, then if drawing such a sprite and encountering a pixel with
Alpha < 1, combine that pixels color with the below color.
_________________

*/