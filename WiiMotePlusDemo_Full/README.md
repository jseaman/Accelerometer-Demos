# WiiMotePlusDemo_Full

Full three-axis quaternion orientation for the Nintendo Wii Remote Plus.

Unlike the earlier split implementation, this version integrates all three
device-local MotionPlus rates into one quaternion. The accelerometer supplies a
confidence-weighted gravity correction that removes pitch/roll drift without
discarding rotation around gravity. This allows a vertically held remote to
retain and display twist from the appropriate gyro axis.

Accelerometer correction is reduced during linear acceleration when the
measured magnitude differs substantially from 1 g. Yaw/heading can still drift
because neither gravity nor the gyroscope provides an absolute compass heading.

Keep the controller still during startup calibration. Press `C` to center the
orientation and recalibrate, `R` to reconnect, or `Esc` to quit. Close Dolphin
before launching.
