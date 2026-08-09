# WiiMoteDemo_Raylib

A clean VS2022/.NET 8 rewrite of the original XNA Wii Remote orientation demo,
using raylib-cs. The original 2009 FBX 6.1 asset was migrated to a self-contained
glTF 2.0 binary (`Assets/wiimote.glb`) that raylib loads directly.

Close Dolphin before launching because only one application can own the Wii
Remote HID interface. Wake the remote, start the demo, and press `R` to retry a
connection. This variant treats the remote as a steering wheel: hold it on its
side with the buttons facing you, then rotate it in that plane. Press `C` to set
the current pose as straight ahead, or `Esc` to exit. The steering angle is
derived from gravity and does not require MotionPlus.

Press `M` to switch between steering mode and the earlier flat-on-desk mode.
Flat mode expects the remote to lie with its buttons facing upward and renders
accelerometer-derived pitch and roll.

The local WiimoteLib compatibility assembly accepts both the original
`RVL-CNT-01` and the `RVL-CNT-01-TR` Wii Remote Plus.
