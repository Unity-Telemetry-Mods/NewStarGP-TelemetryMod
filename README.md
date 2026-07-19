# New Star GP Telemetry Mod

## Instructions

### Normal Install
1. Install BepinEx v5 [link](https://github.com/BepInEx/BepInEx/releases/tag/v5.4.23.2) into your game folder.     

2. Place plugin dll file into `BepinEx/plugins` folder

### UUVR install
1. Download RaiPal [click here](https://github.com/Raicuparta/rai-pal/releases) 
2. Scan for your games, then click on your game.
3. Install **UUVR Mono Modern** (which includes BepinEx)
4. Click the 3 dots next to **UUVR Mono Modern** and then click `Open Mod Folder` (the folder will look like `%APPDATA%\raicuparta\rai-pal\data\installed-mods\[ModId]\`)
5. Place Telemetry Mod dll into `BepinEx/plugins` folder 


## Building
1. Clone this repository

2. This project also references a project named SharedLib (located [here](https://github.com/Unity-Telemetry-Mods/SharedLib))

    - Check out SharedLib folder beside this project

    - folder structure should look like 
    ```
    [This Project]\
    SharedLib\
    ```

3. The following files will need to be copied from

    `New Star GP\release\NSGP_Data\Managed\`

    to
    
    `[Project]\lib`     
    - netstandard.dll
    - Assembly-CSharp_publicized.dll **

4. Open sln file located inside Project Folder

---
** *You will need to publicize the Assembly-CSharp file from game using a tool like [Assembly Publicizer](https://github.com/CabbageCrow/AssemblyPublicizer/releases)*

## Changelog

v 1.0 First Release
v 1.0.2 Updated for new game version as of 2026-06-18
v 1.0.3 Minor update . Refactor to use com.dowhunter.TelemetryLib

---

## Wheel / Force Feedback Integration

### Requirements

- **MOZA wheel hardware** (Moza-first implementation)
- **MOZA Pithouz** software running in the background (connects the SDK to the wheel)
- **ViGEm Bus Driver** installed — [download here](https://github.com/nefarius/ViGEmBus/releases)

### Native DLL Setup

The MOZA C++ native DLLs must be on the Windows DLL search path at game launch.
Copy both files from `libs/moza/x64/` into the **game root directory**
(the folder that contains `NewStarGP.exe`):

```
New Star GP\
  NewStarGP.exe
  MOZA_API_C.dll      ← copy from libs/moza/x64/
  MOZA_SDK.dll        ← copy from libs/moza/x64/
```

The managed C# wrapper (`MOZA_API_CSharp.dll`) is copied automatically alongside the plugin DLL into `BepInEx/plugins/NewStarGPTelemetryMod/` during build.

### Config

All wheel / FFB settings are in `BepInEx/config/com.drowhunter.NewStarGPTelemetryMod.cfg`
under the `Wheel/General`, `Wheel/FFB`, and `Wheel/Input` sections.
They are live-adjustable at runtime via the BepInEx config UI.

| Key | Default | Description |
|---|---|---|
| `Wheel/General / Enabled` | true | Master on/off switch |
| `Wheel/General / Backend` | Moza | Wheel backend (only Moza supported in v1) |
| `Wheel/General / RequireX64` | true | Abort if process is not 64-bit |
| `Wheel/FFB / OverallStrength` | 1.0 | Global FFB multiplier (0..2) |
| `Wheel/FFB / MaxTorque` | 1.0 | Torque clamp (0..1) |
| `Wheel/FFB / BumpThreshold` | 0.15 | Min tire delta to trigger bump |
| `Wheel/FFB / BumpScale` | 0.35 | Bump impulse magnitude scale |
| `Wheel/FFB / BumpCooldownMs` | 60 | Minimum ms between bumps |
| `Wheel/FFB / CForceDampingScale` | 0.4 | cForce → damper coefficient scale |
| `Wheel/FFB / SmoothingAlpha` | 0.25 | Exponential smoothing (0=frozen, 1=raw) |
| `Wheel/Input / SteerDeadzone` | 0.05 | Normalised steer deadzone |
| `Wheel/Input / SteerSaturation` | 1.0 | Normalised steer saturation |
