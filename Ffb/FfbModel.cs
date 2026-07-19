using System;
using System.Diagnostics;
using BepInEx.Logging;

namespace com.drowhunter.NewStarGPTelemetryMod
{
    /// <summary>
    /// Converts enhanced telemetry data into an <see cref="FfbCommand"/> each frame.
    ///
    /// Implemented effects (v1):
    ///   1. CForce damper    – centripetal force → lateral damper coefficient.
    ///   2. Tire impulses    – front left/right wheel delta → short ConstantForce burst.
    ///   3. Global smoothing – exponential smoothing on damper coefficient.
    ///   4. Threshold + cooldown on impulses.
    /// </summary>
    internal class FfbModel
    {
        private readonly WheelConfig _cfg;
        private readonly ManualLogSource _log;

        // Smoothing state
        private float _smoothedDamper;

        // Impulse cooldown: tracks elapsed ms since last impulse using a Stopwatch.
        private readonly Stopwatch _impulseStopwatch = new Stopwatch();

        public FfbModel(WheelConfig cfg, ManualLogSource log)
        {
            _cfg = cfg;
            _log = log;
        }

        /// <summary>
        /// Compute the FFB command for the current physics frame.
        /// </summary>
        public FfbCommand Compute(in NewStarTelemetryData data)
        {
            // ── 1. CForce-based damper ────────────────────────────────────────
            // cForce is the centripetal acceleration (m/s² or g depending on extractor).
            // We scale and clamp it to produce a lateral resistance coefficient.
            float rawDamper = data.cForce * _cfg.CForceDampingScale.Value
                              * _cfg.OverallStrength.Value;
            rawDamper = Clamp(rawDamper, -_cfg.MaxTorque.Value, _cfg.MaxTorque.Value);

            // ── 3. Exponential smoothing ──────────────────────────────────────
            float alpha = _cfg.SmoothingAlpha.Value;
            _smoothedDamper = _smoothedDamper + alpha * (rawDamper - _smoothedDamper);

            // ── 2. Tire impulse ───────────────────────────────────────────────
            // Lateral impulse = FR delta minus FL delta → positive = right-side hit.
            float lateralDelta = data.TireFR - data.TireFL;
            float impulse = lateralDelta * _cfg.BumpScale.Value
                            * _cfg.OverallStrength.Value;
            impulse = Clamp(impulse, -_cfg.MaxTorque.Value, _cfg.MaxTorque.Value);

            bool hasImpulse = false;
            float impulseMagnitude = 0f;

            if (Math.Abs(impulse) >= _cfg.BumpThreshold.Value)
            {
                long elapsedMs = _impulseStopwatch.IsRunning
                    ? _impulseStopwatch.ElapsedMilliseconds
                    : long.MaxValue;

                if (elapsedMs >= _cfg.BumpCooldownMs.Value)
                {
                    hasImpulse = true;
                    impulseMagnitude = impulse;
                    _impulseStopwatch.Restart();
                }
            }

            return new FfbCommand
            {
                DamperCoefficient = _smoothedDamper,
                HasImpulse = hasImpulse,
                ImpulseMagnitude = impulseMagnitude,
            };
        }

        private static float Clamp(float value, float min, float max)
            => value < min ? min : value > max ? max : value;
    }
}
