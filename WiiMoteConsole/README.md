# WiiMoteConsole

Real-time console dashboard for a Nintendo `RVL-CNT-01-TR` Wii Remote Plus.
It displays raw and normalized accelerometer values, raw MotionPlus gyro values,
calibrated angular velocities, IR sensor positions, battery state, and buttons.

Close Dolphin before launching because only one process can own the HID device.
Keep the remote still during the initial gyro calibration. Press `C` to
recalibrate and `Q`, `Esc`, or `Ctrl+C` to exit.

The IR camera only reports points when it can see suitable infrared sources,
such as a powered Wii sensor bar.
