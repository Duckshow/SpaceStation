/*

== Last time ==


== Next time ==
Start splitting the gridgraphics into multiple textures and get that working.

== Goal ==


== Needs to be done ==
Add "empty" sprite to the wall-sprites that I can use for the delete-tool, for example. Solid 64x64 should do it.


== Known bugs ==
The first ghost tiles when drawing a room are placed wrongly

The Idling-task is completely broken and can't find any paths.

Stuff can be selected despite not being in Select-mode.

Applying the gridgraphics-texture is extremely slow. If we want to be able to... do anything regarding tiles with graphics, we're gonna have to split the grid
into multiple textures. More drawcalls, but way more performance.

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