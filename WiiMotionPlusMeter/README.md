# WiiMotionPlusMeter

A modernized rewrite of WiiMeter for VS2022/.NET 9. It displays three Wii
Remote accelerometer gauges in g and, when MotionPlus is detected, three gyro
gauges for yaw, roll, and pitch angular velocity in degrees per second.

Accelerometer scale: -3 g to +3 g.

MotionPlus scales change per axis with the hardware mode:

- slow mode: -500 to +500 deg/s
- fast mode: -2000 to +2000 deg/s

Keep the controller still during the initial gyro bias calibration. The
MotionPlus row is hidden when the connected controller does not expose a
MotionPlus extension. Close Dolphin before launching.
