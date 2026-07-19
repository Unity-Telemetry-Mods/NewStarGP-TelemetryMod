using System;
using BepInEx.Logging;
using mozaAPI;

namespace com.drowhunter.NewStarGPTelemetryMod
{
    /// <summary>
    /// Single boundary around all MOZA SDK C# calls.
    ///
    /// Design notes:
    ///   - <c>installMozaSDK()</c> connects to the MOZA Pithouz background process.
    ///     If Pithouz is not running, subsequent calls will fail gracefully.
    ///   - <c>createWheelbaseET*(IntPtr hwndId, out ERRORCODE err)</c> takes the
    ///     game window handle.  Passing <see cref="IntPtr.Zero"/> works when the SDK
    ///     acquires the DirectInput device independently.
    ///   - The returned effect objects are typed as <c>dynamic</c> because the exact
    ///     C# wrapper class names cannot be confirmed without a full decompile; the
    ///     runtime types are resolved from MOZA_API_CSharp.dll at execution time.
    ///   - <c>getHIDData(out ERRORCODE err)</c> is global (not per-device); the SDK
    ///     aggregates input from all connected MOZA devices.
    ///   - Native DLLs <c>MOZA_API_C.dll</c> and <c>MOZA_SDK.dll</c> must be present
    ///     in the game root directory (on the OS DLL search path).
    /// </summary>
    internal class MozaSdkAdapter : IDisposable
    {
        private readonly ManualLogSource _log;
        private bool _initialised;
        private bool _disposed;

        // DirectInput "nominal max" constant: force values are in the range ±10000.
        public const int DI_FF_NOMINAL_MAX = 10000;

        public MozaSdkAdapter(ManualLogSource log)
        {
            _log = log;
        }

        /// <summary>
        /// Load the MOZA SDK. Returns true if successful.
        /// </summary>
        public bool Install()
        {
            try
            {
                mozaAPI.mozaAPI.installMozaSDK();
                _initialised = true;
                _log.LogInfo("[MozaSdkAdapter] installMozaSDK succeeded.");
                return true;
            }
            catch (Exception ex)
            {
                _log.LogWarning($"[MozaSdkAdapter] installMozaSDK failed: {ex.Message}");
                return false;
            }
        }

        /// <summary>
        /// Poll the global HID data from all connected MOZA devices.
        /// Returns null if the SDK is not initialised or an error occurs.
        /// </summary>
        public HIDData GetHIDData()
        {
            if (!_initialised) return null;
            try
            {
                var data = mozaAPI.mozaAPI.getHIDData(out ERRORCODE err);
                if (err != ERRORCODE.NORMAL)
                {
                    _log.LogDebug($"[MozaSdkAdapter] getHIDData error: {err}");
                    return null;
                }
                return data;
            }
            catch (Exception ex)
            {
                _log.LogDebug($"[MozaSdkAdapter] getHIDData exception: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create a continuous ETDamper effect. Returns the effect object or null on failure.
        /// </summary>
        public dynamic CreateDamper(IntPtr hwnd)
        {
            if (!_initialised) return null;
            try
            {
                var effect = mozaAPI.mozaAPI.createWheelbaseETDamper(hwnd, out ERRORCODE err);
                if (err != ERRORCODE.NORMAL)
                {
                    _log.LogWarning($"[MozaSdkAdapter] createWheelbaseETDamper error: {err}");
                    return null;
                }
                _log.LogInfo("[MozaSdkAdapter] ETDamper effect created.");
                return effect;
            }
            catch (Exception ex)
            {
                _log.LogWarning($"[MozaSdkAdapter] createWheelbaseETDamper failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Create a short-burst ETConstantForce effect for impulse feedback.
        /// Returns the effect object or null on failure.
        /// </summary>
        public dynamic CreateConstantForce(IntPtr hwnd)
        {
            if (!_initialised) return null;
            try
            {
                var effect = mozaAPI.mozaAPI.createWheelbaseETConstantForce(hwnd, out ERRORCODE err);
                if (err != ERRORCODE.NORMAL)
                {
                    _log.LogWarning($"[MozaSdkAdapter] createWheelbaseETConstantForce error: {err}");
                    return null;
                }
                _log.LogInfo("[MozaSdkAdapter] ETConstantForce effect created.");
                return effect;
            }
            catch (Exception ex)
            {
                _log.LogWarning($"[MozaSdkAdapter] createWheelbaseETConstantForce failed: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Stop all active FFB effects on the wheelbase.
        /// </summary>
        public void StopAllFfb()
        {
            if (!_initialised) return;
            try
            {
                mozaAPI.mozaAPI.stopForceFeedback();
            }
            catch (Exception ex)
            {
                _log.LogDebug($"[MozaSdkAdapter] stopForceFeedback exception: {ex.Message}");
            }
        }

        /// <summary>
        /// Center the wheel (reset to zero position).
        /// </summary>
        public void CenterWheel()
        {
            if (!_initialised) return;
            try
            {
                mozaAPI.mozaAPI.CenterWheel();
            }
            catch (Exception ex)
            {
                _log.LogDebug($"[MozaSdkAdapter] CenterWheel exception: {ex.Message}");
            }
        }

        public void Dispose()
        {
            if (_disposed) return;
            _disposed = true;

            if (_initialised)
            {
                StopAllFfb();
                try
                {
                    mozaAPI.mozaAPI.removeMozaSDK();
                    _log.LogInfo("[MozaSdkAdapter] removeMozaSDK called.");
                }
                catch (Exception ex)
                {
                    _log.LogDebug($"[MozaSdkAdapter] removeMozaSDK exception: {ex.Message}");
                }
                _initialised = false;
            }
        }
    }
}
