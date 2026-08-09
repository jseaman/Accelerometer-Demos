# WiimoteLib compatibility build

`WiimoteLib.dll` is Brian Peek's WiimoteLib 1.7 with its two
hard-coded Wii Remote product-ID checks changed from `0x0306` to `0x0330`.
That is the HID product ID used by the `Nintendo RVL-CNT-01-TR` Wii Remote
Plus (MotionPlus Inside).

Upstream source and license: <https://github.com/BrianPeek/WiimoteLib>

This compatibility build intentionally targets the `-TR` remote used by this
project. To use an original `RVL-CNT-01` instead, reference the unmodified
WiimoteLib 1.7 assembly.
