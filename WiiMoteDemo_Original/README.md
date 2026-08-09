# WiiMoteDemo_Original

A VS2022/.NET 8 raylib reconstruction of the original XNA `WiiMoteDemo`
orientation behavior, using the corrected modern GLB model.

Instead of driving the model with Euler pitch/roll rotations, it computes the
shortest quaternion rotation from the flat reference gravity vector to the
measured accelerometer gravity vector. This reproduces the original demo's
matrix-alignment idea while fixing its zero-length cross products, unchecked
`acos()` input, and unstable normalization at parallel vectors.

The quaternion remains stable when the remote stands upright. An accelerometer
still cannot measure twist/yaw around the gravity vector; that degree of freedom
is deliberately assigned the minimum rotation rather than invented from noise.

Close Dolphin before starting, wake the remote, and press `R` to reconnect.
