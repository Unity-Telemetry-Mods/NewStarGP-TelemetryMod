using System;
using BepInEx.Logging;
using Nefarius.ViGEm.Client;
using Nefarius.ViGEm.Client.Targets;
using Nefarius.ViGEm.Client.Targets.Xbox360;

namespace com.drowhunter.NewStarGPTelemetryMod
{
    /// <summary>
    /// <see cref="IVirtualGamepadOutput"/> implementation backed by ViGEm Bus.
    ///
    /// Maps <see cref="VirtualPadState"/> fields to a virtual Xbox 360 controller:
    ///   - <c>LeftStickX</c>   → left thumb X (steer)
    ///   - <c>LeftTrigger</c>  → left trigger  (throttle/gas)
    ///   - <c>RightTrigger</c> → right trigger (brake)
    ///
    /// Requires ViGEm Bus driver to be installed on the host PC.
    /// </summary>
    internal class VigemXboxGamepadOutput : IVirtualGamepadOutput
    {
        private readonly ManualLogSource _log;
        private ViGEmClient _client;
        private IXbox360Controller _controller;

        public VigemXboxGamepadOutput(ManualLogSource log)
        {
            _log = log;
        }

        public bool Initialize()
        {
            try
            {
                _client = new ViGEmClient();
                _controller = _client.CreateXbox360Controller();
                _controller.Connect();
                _log.LogInfo("[VigemXboxGamepadOutput] Virtual Xbox 360 controller connected.");
                return true;
            }
            catch (Exception ex)
            {
                _log.LogWarning($"[VigemXboxGamepadOutput] Init failed: {ex.Message}");
                _client = null;
                _controller = null;
                return false;
            }
        }

        public void Submit(VirtualPadState state)
        {
            if (_controller == null) return;
            try
            {
                _controller.SetAxisValue(Xbox360Axis.LeftThumbX, NormToShort(state.LeftStickX));
                _controller.SetAxisValue(Xbox360Axis.LeftThumbY, NormToShort(state.LeftStickY));
                _controller.SetAxisValue(Xbox360Axis.RightThumbX, NormToShort(state.RightStickX));
                _controller.SetAxisValue(Xbox360Axis.RightThumbY, NormToShort(state.RightStickY));

                _controller.SetSliderValue(Xbox360Slider.LeftTrigger, NormToByte(state.LeftTrigger));
                _controller.SetSliderValue(Xbox360Slider.RightTrigger, NormToByte(state.RightTrigger));

                _controller.SubmitReport();
            }
            catch (Exception ex)
            {
                _log.LogDebug($"[VigemXboxGamepadOutput] Submit failed: {ex.Message}");
            }
        }

        public void Dispose()
        {
            try { _controller?.Disconnect(); } catch { }
            try { _client?.Dispose(); } catch { }
            _controller = null;
            _client = null;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static short NormToShort(float value)
        {
            float clamped = value < -1f ? -1f : value > 1f ? 1f : value;
            // Map -1..0 → -32768..0, 0..1 → 0..32767 to use the full short range.
            float scaled = clamped < 0f ? clamped * 32768f : clamped * 32767f;
            return (short)scaled;
        }

        private static byte NormToByte(float value)
            => (byte)Math.Max(0, Math.Min(255, (int)(value * 255f)));
    }
}
