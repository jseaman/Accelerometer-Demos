# WiimoteLib compatibility build

`WiimoteLib.dll` is built from Brian Peek's WiimoteLib source with discovery
support for both Nintendo controller product IDs:

- `0x0306`: original RVL-CNT-01 Wii Remote
- `0x0330`: RVL-CNT-01-TR Wii Remote Plus (MotionPlus Inside)

When either type is connected, WiiMeter uses the first compatible device found.

Upstream source and license: <https://github.com/BrianPeek/WiimoteLib>

The source used for this build is stored under
`WiiMotePlusDemo/vendor/WiimoteLib` in this repository.
