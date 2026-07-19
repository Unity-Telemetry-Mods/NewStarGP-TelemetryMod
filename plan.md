# NewStarGP Telemetry Mod — Force Feedback (FFB) Planning

## Status
Drafting phase only. No implementation changes yet.

## Goals
- Add steering wheel integration for NewStarGP telemetry output.
- Use telemetry to drive force feedback (FFB) on supported wheels.
- Feed a virtual Xbox gamepad via ViGEm from wheel input.
- Keep architecture extensible for multiple wheel backends.

## Confirmed Constraints
- Work resides under `NewStarGP-TelemetryMod` submodule in this repository.
- Branch: `ffb`.
- **Do not modify `TelemetryExtractor`** at this stage.
  - `TelemetryExtractor` is intended to remain generic/non-game-specific.
- Consume telemetry after plugin enhancement (game-specific enrichment such as tire data).
- Runtime tuning must be exposed via **BepInEx config** (live adjustable via config UI).
- Moza-first implementation priority.

## Dependencies
- Virtual gamepad: `Nefarius.VigemClient` (NuGet)
- Wheel input fallback path (later): SharpDX / DirectInput
- Primary wheel SDK (v1): Moza C# SDK
  - Present in branch under `libs/moza/x64/MOZA_API_CSharp.dll`
- Moza examples/docs in repo:
  - `libs/moza/example`
  - `libs/moza_cpp/docsEng/index.html`

## High-Level Architecture

### Data Flow
1. `TelemetryExtractor` produces base telemetry (unchanged).
2. Plugin enriches telemetry with game-specific fields (tires, survival points, etc.).
3. Enhanced telemetry is published to a runtime-accessible state store.
4. Wheel runtime consumes enhanced telemetry each update and:
   - maps wheel inputs to virtual Xbox controller state
   - computes FFB torque/effects and sends to wheel backend

### Lifecycle
Create one long-lived runtime service as part of plugin lifecycle:
- initialize once on plugin startup
- update every plugin tick / frame
- dispose once on plugin unload

## Proposed Components (Draft)

### Runtime
- `Runtime/WheelIntegrationRuntime.cs`
  - Orchestrates provider initialization, per-frame update, and disposal.
- `Runtime/TelemetryStateStore.cs`
  - Holds latest enhanced telemetry snapshot (thread-safe access).

### Contracts
- `Contracts/IWheelInputProvider.cs`
- `Contracts/IWheelFfbProvider.cs`
- `Contracts/IVirtualGamepadOutput.cs`

### Moza Backend
- `Moza/MozaSdkAdapter.cs`
  - Single boundary around vendor SDK calls.
- `Moza/MozaWheelInputProvider.cs`
- `Moza/MozaWheelFfbProvider.cs`

### Virtual Controller
- `Output/VigemXboxGamepadOutput.cs`

### FFB Model
- `Ffb/FfbModel.cs`
  - Converts enhanced telemetry into FFB command(s)
  - Applies thresholds, cooldowns, gains, smoothing, clamping

### Models
- `Models/WheelInputState.cs`
- `Models/VirtualPadState.cs`
- `Models/FfbCommand.cs`

### Config
- `Config/WheelConfig.cs`
  - BepInEx `ConfigEntry<T>` bindings and ranges

## BepInEx Runtime Config (Draft)
All values should be editable at runtime through BepInEx config UI.

### `Wheel/General`
- `Enabled` (bool, default `true`)
- `Backend` (string, default `Moza`)
- `RequireX64` (bool, default `true`)

### `Wheel/FFB`
- `OverallStrength` (float, default `1.0`, range `0.0..2.0`)
- `MaxTorque` (float, default `1.0`)
- `BumpThreshold` (float, default `0.15`)
- `BumpScale` (float, default `0.35`)
- `BumpCooldownMs` (int, default `60`)
- `CForceDampingScale` (float, default `0.4`)
- `SmoothingAlpha` (float, default `0.25`, range `0..1`)

### `Wheel/Input`
- `SteerDeadzone` (float)
- `SteerSaturation` (float)

## FFB Behavior Plan (v1)

### 1) Tire Impact Impulses
- Generate small directional impulses from left/right tire impact signals.
- Apply a threshold to ignore minor bumps.
- Apply cooldown to avoid rapid repeated impulses.

### 2) CForce-Based Resistance
- Use CForce to increase directional resistance (damping/friction-like feel).
- Scale by configurable gain.

### 3) Global Strength
- Multiply total FFB by `Wheel/FFB/OverallStrength`.
- Clamp final torque to `MaxTorque`.

### 4) Smoothing
- Apply lightweight smoothing to reduce jitter/chatter.

## Input Mapping Plan (v1)
- Wheel steer axis -> virtual gamepad left-stick X (deadzone removal + rescale).
- Basic pedal mapping to triggers where available.
- Button/paddle mapping can be expanded later.

## Platform and Safety
- Assume game process is x64; verify at runtime.
- If required SDK/controller init fails, disable wheel integration gracefully (no plugin crash).
- Ensure clean teardown/disposal on unload.

## Deferred / Low Priority
- Tire slip behavior causing temporary wheel loosening.
- Roll/pitch angular velocity collision heuristics.
- DirectInput backend parity.

## Open Questions
1. Exact Moza SDK C# method signatures for init/poll/ffb/dispose.
2. Required call frequency/threading constraints from Moza docs.
3. Final telemetry field names for left/right tire impacts and CForce.
4. Preferred location for `plan.md` if not repository root.

## Suggested Implementation Phases
1. Skeleton runtime + config entries (no active FFB)
2. Moza input polling + ViGEm output mapping
3. Add CForce resistance effect
4. Add tire impulse effects with threshold/cooldown
5. Tune defaults and safety clamps
6. Add DirectInput backend later behind same interfaces

---

Owner note: This file is intentionally planning-only and should be updated before implementation begins.
