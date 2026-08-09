# WiiMoteGyroPointerDemo

A Raylib screen-pointer experiment driven by the Wii Remote Plus accelerometer
and MotionPlus gyroscope. It deliberately does not use the IR camera.

Hold the remote naturally with its buttons facing upward and point it at the
screen center. Press `C`, keep it still during calibration, then steer the gold
cursor by aiming the remote. Press `C` again whenever accumulated yaw drift
needs to be recentered. Press `R` to retry a connection and `Esc` to quit.

MotionPlus supplies relative horizontal heading. Accelerometer gravity supplies
the absolute vertical pointer angle and roll compensation. Gyro pitch is used
only to keep horizontal motion correct when the remote is rolled; it is not
integrated into screen Y. Since gravity contains no absolute heading, horizontal
drift cannot be eliminated without IR or another external reference.

Stationary testing on the target controller measured 99th-percentile noise of
approximately 0.35 deg/s yaw, 0.32 deg/s roll, and 0.45 deg/s pitch. The demo
therefore applies soft dead zones of 0.40, 0.40, and 0.50 deg/s respectively
before roll compensation. Only the resulting horizontal rate is integrated.

An 8-sample rolling average is enabled by default after the dead zones and
before roll compensation. Press `A` to compare averaged and unaveraged gyro
motion immediately. Recalibrating with `C` also clears the averaging window.

The integrated horizontal angle is clamped at the virtual screen edges. Gyro
motion continuing outward at an edge is discarded, preventing off-screen
windup and allowing reverse motion to bring the pointer back immediately.
