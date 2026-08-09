# WiiMoteIRDemo

Raylib visualization of the Wii Remote IR camera. The viewport is the camera's
native 1024x768 coordinate space and displays the first two tracked IR blobs,
their raw coordinates, connecting baseline, midpoint, and baseline angle.

Press `C` to show or hide the midpoint cursor. The cursor is hidden by default
and rotates with the line joining the two blobs. Pointer mapping defaults to a
sensor bar below the display, using a roll-corrected bottom-bar screen box.
Press `R` to reconnect.

The controller must see two infrared sources. A powered Wii sensor bar works;
the bar does not communicate with the PC and only needs electrical power.
Close Dolphin before launching because only one process can own the controller.
