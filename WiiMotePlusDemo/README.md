# WiiMotePlusDemo

Raylib 3D orientation demo for a Nintendo Wii Remote Plus (`RVL-CNT-01-TR`).

The project vendors the upstream WiimoteLib 1.8 beta MotionPlus source and adds:

- Windows/.NET 8 and Visual Studio 2022 project support
- `RVL-CNT-01-TR` USB PID (`0x0330`) detection
- built-in MotionPlus identifier (`0100A4200405`) recognition
- stationary gyro bias calibration
- MotionPlus slow/fast raw-rate conversion
- integrated yaw, with accelerometer-derived pitch and roll

Keep the remote still for about two seconds after connection. Press `C` at any
time to center yaw and recalibrate gyro drift. Press `R` to reconnect.

MotionPlus provides angular velocity, so yaw is integrated and can drift slowly.
The accelerometer corrects pitch and roll against gravity, but there is no
absolute yaw reference unless an external reference such as the IR camera is
also fused.
