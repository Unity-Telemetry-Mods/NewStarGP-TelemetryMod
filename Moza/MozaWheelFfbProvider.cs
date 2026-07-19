using System;
using BepInEx.Logging;

namespace com.drowhunter.NewStarGPTelemetryMod
{
    /// <summary>
    /// <see cref="IWheelFfbProvider"/> implementation using the MOZA SDK.
    ///
    /// Manages two persistent DirectInput effects:
    ///   - <c>_damper</c>  (ETDamper)        – continuous lateral resistance from cForce.
    ///   - <c>_impulse</c> (ETConstantForce)  – short bump burst from tire deltas.
    ///
    /// DirectInput force range: ±10000 (DI_FFNOMINALMAX).
    /// Our <see cref="FfbCommand"/> values are in ±1 and are scaled to ±10000 here.
    /// </summary>
    internal class MozaWheelFfbProvider : IWheelFfbProvider
    {
        private readonly MozaSdkAdapter _sdk;
        private readonly ManualLogSource _log;

        // Effect objects returned by the Moza SDK (runtime-typed via dynamic).
        private dynamic _damper;
        private dynamic _impulse;

        // Duration of bump impulse in milliseconds.
        private const int ImpulseDurationMs = 80;

        public MozaWheelFfbProvider(MozaSdkAdapter sdk, ManualLogSource log)
        {
            _sdk = sdk;
            _log = log;
        }

        public bool Initialize()
        {
            // Pass IntPtr.Zero: the SDK acquires the DirectInput device independently.
            _damper = _sdk.CreateDamper(IntPtr.Zero);
            if (_damper == null)
            {
                _log.LogWarning("[MozaWheelFfbProvider] Failed to create ETDamper effect.");
                return false;
            }

            _impulse = _sdk.CreateConstantForce(IntPtr.Zero);
            if (_impulse == null)
            {
                _log.LogWarning("[MozaWheelFfbProvider] Failed to create ETConstantForce effect.");
                return false;
            }

            // Start both effects in a "zero" state so they are acquired by DirectInput.
            try
            {
                _damper.setPositiveCoefficient(0);
                _damper.setNegativeCoefficient(0);
                _damper.start();

                _impulse.setMagnitude(0);
                _impulse.setDuration(ImpulseDurationMs);
                // Do not start impulse yet – fired on demand.
            }
            catch (Exception ex)
            {
                _log.LogWarning($"[MozaWheelFfbProvider] Effect init failed: {ex.Message}");
                return false;
            }

            _log.LogInfo("[MozaWheelFfbProvider] FFB effects initialised.");
            return true;
        }

        public void SendFfb(FfbCommand cmd)
        {
            UpdateDamper(cmd.DamperCoefficient);
            if (cmd.HasImpulse)
                FireImpulse(cmd.ImpulseMagnitude);
        }

        private void UpdateDamper(float coefficient)
        {
            if (_damper == null) return;
            try
            {
                // coefficient > 0 → resistance to right movement (positive coeff)
                // coefficient < 0 → resistance to left movement (negative coeff)
                int scaledPos = (int)(Math.Max(0f, coefficient) * MozaSdkAdapter.DI_FF_NOMINAL_MAX);
                int scaledNeg = (int)(Math.Max(0f, -coefficient) * MozaSdkAdapter.DI_FF_NOMINAL_MAX);

                _damper.setPositiveCoefficient(scaledPos);
                _damper.setNegativeCoefficient(scaledNeg);
            }
            catch (Exception ex)
            {
                _log.LogDebug($"[MozaWheelFfbProvider] UpdateDamper failed: {ex.Message}");
            }
        }

        private void FireImpulse(float magnitude)
        {
            if (_impulse == null) return;
            try
            {
                int scaled = (int)(magnitude * MozaSdkAdapter.DI_FF_NOMINAL_MAX);
                _impulse.setMagnitude(scaled);
                _impulse.start();
            }
            catch (Exception ex)
            {
                _log.LogDebug($"[MozaWheelFfbProvider] FireImpulse failed: {ex.Message}");
            }
        }

        public void Dispose()
        {
            StopEffects();
            _damper = null;
            _impulse = null;
        }

        private void StopEffects()
        {
            try { _damper?.stop(); } catch { }
            try { _impulse?.stop(); } catch { }
            _sdk.StopAllFfb();
        }
    }
}
