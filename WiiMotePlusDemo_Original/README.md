# WiiMotePlusDemo_Original

This raylib demo combines two existing implementations:

- Accelerometer tilt uses the robust shortest-arc gravity quaternion from
  `WiiMoteDemo_Original`, reproducing the original XNA alignment idea without
  its zero-vector and upright singularities.
- MotionPlus calibration, angular-rate conversion, yaw sign, yaw integration,
  and recenter controls are unchanged from `WiiMotePlusDemo`.

Keep the Wii Remote still during startup calibration. Press `C` to reset yaw
and recalibrate the gyro, `R` to reconnect, or `Esc` to quit. Close Dolphin
before launching because only one process can own the controller.
