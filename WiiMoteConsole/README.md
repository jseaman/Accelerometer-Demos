# WiiMoteConsole

Real-time console dashboard for an original `RVL-CNT-01` Wii Remote or an
`RVL-CNT-01-TR` Wii Remote Plus. It displays raw and normalized accelerometer
values, IR sensor positions, battery state, and buttons on either controller.
MotionPlus gyro values and calibrated angular velocities are shown only when a
MotionPlus-equipped controller is detected.

When a Nunchuk extension is connected, the dashboard also shows its raw and
normalized accelerometer vector, raw and normalized joystick position, and its
C and Z button states.

Close Dolphin before launching because only one process can own the HID device.
When MotionPlus is available, keep the remote still during initial gyro
calibration and press `C` to recalibrate. Press `Q`, `Esc`, or `Ctrl+C` to exit.

The IR camera only reports points when it can see suitable infrared sources,
such as a powered Wii sensor bar.
