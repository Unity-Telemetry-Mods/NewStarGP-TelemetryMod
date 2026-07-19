using System;
using BepInEx.Logging;
using mozaAPI;

namespace com.drowhunter.NewStarGPTelemetryMod
{
    /// <summary>
    /// <see cref="IWheelInputProvider"/> implementation using the MOZA SDK.
    ///
    /// Calls <c>getHIDData</c> each frame and converts the raw HID report into
    /// a normalised <see cref="WheelInputState"/>.
    ///
    /// Pedal axes in HIDData use int16 with 0x8000 (-32768) as the rest position
    /// and 0x7FFF (32767) as fully pressed.  These are linearly mapped to 0..1.
    ///
    /// Threading: <c>getHIDData</c> is expected to be non-blocking (global read of
    /// last-received HID report).  If latency issues arise, move to a background
    /// thread and guard with a lock.
    /// </summary>
    internal class MozaWheelInputProvider : IWheelInputProvider
    {
        private readonly MozaSdkAdapter _sdk;
        private readonly WheelConfig _cfg;
        private readonly ManualLogSource _log;

        // Maximum steering angle the wheel can physically rotate (degrees).
        // Used to normalise fSteeringWheelAngle to -1..1.
        private const float MaxSteerDegrees = 540f;

        public MozaWheelInputProvider(MozaSdkAdapter sdk, WheelConfig cfg, ManualLogSource log)
        {
            _sdk = sdk;
            _cfg = cfg;
            _log = log;
        }

        public bool Initialize()
        {
            // The SDK itself is initialised by MozaSdkAdapter.Install() before this is called.
            _log.LogInfo("[MozaWheelInputProvider] Initialised.");
            return true;
        }

        public WheelInputState Poll()
        {
            var raw = _sdk.GetHIDData();
            if (raw == null)
                return default;

            float steerNorm = NormaliseSteer(raw.fSteeringWheelAngle);
            steerNorm = ApplyDeadzoneSaturation(steerNorm,
                _cfg.SteerDeadzone.Value,
                _cfg.SteerSaturation.Value);

            return new WheelInputState
            {
                SteerAngle = steerNorm,
                SteerVelocity = float.IsNaN(raw.fSteeringWheelVelocity) ? 0f : raw.fSteeringWheelVelocity / MaxSteerDegrees,
                Throttle = Int16ToAxis(raw.throttle),
                Brake = Int16ToAxis(raw.brake),
                Clutch = Int16ToAxis(raw.clutch),
                Buttons = null,  // HIDButton array not yet mapped
            };
        }

        public void Dispose()
        {
            // Resources owned by MozaSdkAdapter; nothing to release here.
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static float NormaliseSteer(float angleDegrees)
        {
            if (float.IsNaN(angleDegrees)) return 0f;
            return Math.Max(-1f, Math.Min(1f, angleDegrees / MaxSteerDegrees));
        }

        /// <summary>
        /// Applies deadzone removal and saturation rescaling to a normalised axis value.
        /// </summary>
        private static float ApplyDeadzoneSaturation(float value, float deadzone, float saturation)
        {
            float absVal = Math.Abs(value);
            int sign = value < 0 ? -1 : 1;

            if (absVal < deadzone) return 0f;

            // Rescale from [deadzone..saturation] to [0..1]
            float range = saturation - deadzone;
            if (range <= 0f) return sign;
            float scaled = Math.Min(1f, (absVal - deadzone) / range);
            return sign * scaled;
        }

        /// <summary>
        /// Converts an int16 pedal axis (rest = 0x8000 = -32768) to 0..1.
        /// </summary>
        private static float Int16ToAxis(short raw)
        {
            // 0x8000 (-32768) = rest/released; 0x7FFF (32767) = fully pressed.
            int shifted = (int)raw - short.MinValue;  // 0..65535
            return Math.Max(0f, Math.Min(1f, shifted / 65535f));
        }
    }
}
