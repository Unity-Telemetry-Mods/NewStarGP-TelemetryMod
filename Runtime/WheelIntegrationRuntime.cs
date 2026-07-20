using System;
using BepInEx.Logging;

namespace com.drowhunter.NewStarGPTelemetryMod
{
    /// <summary>
    /// Orchestrates wheel input polling, FFB output, and virtual gamepad output
    /// for the lifetime of the plugin.
    ///
    /// Lifecycle:
    ///   1. <see cref="Initialize"/> – called once on plugin startup.
    ///   2. <see cref="Update"/>     – called every physics frame (FixedUpdate).
    ///   3. <see cref="Dispose"/>    – called on plugin unload / OnDestroy.
    ///
    /// Safety:
    ///   - Disabled automatically if init fails or if the process is not 64-bit
    ///     and <c>RequireX64</c> is true.
    ///   - All provider exceptions are caught; a failure sets <c>_disabled</c> to
    ///     prevent further calls and protect plugin stability.
    /// </summary>
    internal class WheelIntegrationRuntime : IDisposable
    {
        private readonly ManualLogSource _log;

        private WheelConfig _cfg;
        private MozaSdkAdapter _mozaSdk;
        private IWheelInputProvider _inputProvider;
        private IWheelFfbProvider _ffbProvider;
        private IVirtualGamepadOutput _padOutput;
        private FfbModel _ffbModel;

        private bool _initialised;
        private bool _disabled;

        public WheelIntegrationRuntime(ManualLogSource log)
        {
            _log = log;
        }

        /// <summary>
        /// Initialise all providers using the given config.
        /// Safe to call even if wheel hardware is absent — failures disable the
        /// runtime without throwing.
        /// </summary>
        public void Initialize(WheelConfig cfg)
        {
            _log.LogInfo("[WheelIntegrationRuntime] *** Initialize() called ***");
            _cfg = cfg;

            // ── x64 guard ────────────────────────────────────────────────────
            if (cfg.RequireX64.Value && !Environment.Is64BitProcess)
            {
                _log.LogWarning("[WheelIntegrationRuntime] Process is not x64. " +
                                "Wheel integration disabled (RequireX64 = true).");
                _disabled = true;
                return;
            }

            try
            {
                // ── MOZA SDK ─────────────────────────────────────────────────
                _mozaSdk = new MozaSdkAdapter(_log);
                if (!_mozaSdk.Install())
                {
                    _log.LogWarning("[WheelIntegrationRuntime] MOZA SDK init failed. " +
                                   "Ensure MOZA Pithouz is running and native DLLs are present.");
                    _disabled = true;
                    return;
                }

                // -- Device presence check ---------------------------------------------
                if (!_mozaSdk.IsDeviceConnected())
                {
                    _log.LogWarning("[WheelIntegrationRuntime] No MOZA device detected. " +
                                   "Wheel integration disabled. Connect your wheel and restart the game.");
                    _disabled = true;
                    return;
                }

                // ── Input provider ────────────────────────────────────────────
                _inputProvider = new MozaWheelInputProvider(_mozaSdk, cfg, _log);
                if (!_inputProvider.Initialize())
                {
                    _log.LogWarning("[WheelIntegrationRuntime] Wheel input provider failed to initialise.");
                    _disabled = true;
                    return;
                }

                // ── FFB model ─────────────────────────────────────────────────
                _ffbModel = new FfbModel(cfg, _log);

                // ── FFB provider ──────────────────────────────────────────────
                _ffbProvider = new MozaWheelFfbProvider(_mozaSdk, _log);
                if (!_ffbProvider.Initialize())
                {
                    _log.LogWarning("[WheelIntegrationRuntime] FFB provider failed to initialise. " +
                                   "Input mapping will continue without FFB.");
                    // Not fatal – pad output can still work.
                }

                // ── Virtual gamepad ───────────────────────────────────────────
                _padOutput = new VigemXboxGamepadOutput(_log);
                if (!_padOutput.Initialize())
                {
                    _log.LogWarning("[WheelIntegrationRuntime] ViGEm gamepad output failed. " +
                                   "Ensure ViGEm Bus driver is installed.");
                    // Not fatal – FFB can still work without virtual pad.
                }

                _initialised = true;
                _log.LogInfo("[WheelIntegrationRuntime] Wheel integration ready.");
            }
            catch (Exception ex)
            {
                _log.LogError($"[WheelIntegrationRuntime] Unexpected init error: {ex}");
                _disabled = true;
            }
        }

        /// <summary>
        /// Called every FixedUpdate with the latest telemetry snapshot.
        /// </summary>
        public void Update(in NewStarTelemetryData telemetry)
        {
            if (!_initialised || _disabled) return;

            try
            {
                // 1. Poll wheel input
                WheelInputState wheelInput = _inputProvider?.Poll() ?? default;

                // 2. Build virtual pad state from wheel input
                var padState = BuildPadState(in wheelInput);
                _padOutput?.Submit(padState);

                // 3. Compute FFB from telemetry
                if (_ffbModel != null && _ffbProvider != null)
                {
                    FfbCommand cmd = _ffbModel.Compute(in telemetry);
                    _ffbProvider.SendFfb(cmd);
                }
            }
            catch (Exception ex)
            {
                _log.LogError($"[WheelIntegrationRuntime] Update error (disabling): {ex}");
                _disabled = true;
            }
        }

        public void Dispose()
        {
            _initialised = false;

            try { _ffbProvider?.Dispose(); } catch (Exception ex) { _log.LogDebug($"FFB dispose: {ex.Message}"); }
            try { _padOutput?.Dispose();   } catch (Exception ex) { _log.LogDebug($"Pad dispose: {ex.Message}"); }
            try { _inputProvider?.Dispose(); } catch (Exception ex) { _log.LogDebug($"Input dispose: {ex.Message}"); }
            try { _mozaSdk?.Dispose();     } catch (Exception ex) { _log.LogDebug($"SDK dispose: {ex.Message}"); }

            _ffbProvider = null;
            _padOutput = null;
            _inputProvider = null;
            _mozaSdk = null;
            _ffbModel = null;

            _log.LogInfo("[WheelIntegrationRuntime] Disposed.");
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static VirtualPadState BuildPadState(in WheelInputState wheel)
        {
            return new VirtualPadState
            {
                LeftStickX = wheel.SteerAngle,
                LeftStickY = 0f,
                RightStickX = 0f,
                RightStickY = 0f,
                LeftTrigger = wheel.Throttle,
                RightTrigger = wheel.Brake,
                Buttons = 0,
            };
        }
    }
}
