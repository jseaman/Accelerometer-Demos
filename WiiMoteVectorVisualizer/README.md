# WiiMoteVectorVisualizer

Raylib 3D visualization of the Wii Remote accelerometer vector.

The white box represents a flat Wii Remote. Sensor coordinates are mapped into
the scene as follows:

- Wii X (lateral) -> world X, red
- Wii Y (longitudinal) -> world -Z, blue
- Wii Z (normal through the buttons) -> world Y, green

The gold arrow is the resultant acceleration vector and its displayed length is
one world unit per g. The wire sphere has radius 3, representing the useful
approximately 3 g accelerometer range. Values above 3 g are shown numerically
but the arrow is clamped to the sphere.

Left-drag to orbit the camera, use the mouse wheel to zoom, and press `R` to
reconnect. Close Dolphin before launching.
