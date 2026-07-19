using BepInEx.Configuration;

namespace com.drowhunter.NewStarGPTelemetryMod
{
    /// <summary>
    /// Holds all BepInEx ConfigEntry bindings for wheel / FFB integration.
    /// All values are live-adjustable at runtime via the BepInEx config UI.
    /// </summary>
    internal class WheelConfig
    {
        // ── Wheel / General ──────────────────────────────────────────────────

        public ConfigEntry<bool> Enabled { get; }
        public ConfigEntry<string> Backend { get; }
        public ConfigEntry<bool> RequireX64 { get; }

        // ── Wheel / FFB ──────────────────────────────────────────────────────

        public ConfigEntry<float> OverallStrength { get; }
        public ConfigEntry<float> MaxTorque { get; }
        public ConfigEntry<float> BumpThreshold { get; }
        public ConfigEntry<float> BumpScale { get; }
        public ConfigEntry<int> BumpCooldownMs { get; }
        public ConfigEntry<float> CForceDampingScale { get; }
        public ConfigEntry<float> SmoothingAlpha { get; }

        // ── Wheel / Input ────────────────────────────────────────────────────

        public ConfigEntry<float> SteerDeadzone { get; }
        public ConfigEntry<float> SteerSaturation { get; }

        public WheelConfig(ConfigFile config)
        {
            // Wheel / General
            Enabled = config.Bind(
                "Wheel/General", "Enabled", true,
                "Enable wheel integration (Moza FFB + ViGEm virtual gamepad).");

            Backend = config.Bind(
                "Wheel/General", "Backend", "Moza",
                "Wheel backend to use. Supported: Moza.");

            RequireX64 = config.Bind(
                "Wheel/General", "RequireX64", true,
                "Abort wheel init if the process is not 64-bit (required for Moza native DLLs).");

            // Wheel / FFB
            OverallStrength = config.Bind(
                "Wheel/FFB", "OverallStrength", 1.0f,
                new ConfigDescription("Overall FFB strength multiplier.", new AcceptableValueRange<float>(0f, 2f)));

            MaxTorque = config.Bind(
                "Wheel/FFB", "MaxTorque", 1.0f,
                new ConfigDescription("Maximum torque clamp (0..1 maps to DirectInput 0..10000).", new AcceptableValueRange<float>(0f, 1f)));

            BumpThreshold = config.Bind(
                "Wheel/FFB", "BumpThreshold", 0.15f,
                new ConfigDescription("Minimum normalised tire delta required to trigger a bump impulse.", new AcceptableValueRange<float>(0f, 1f)));

            BumpScale = config.Bind(
                "Wheel/FFB", "BumpScale", 0.35f,
                new ConfigDescription("Scales the lateral tire delta into a bump impulse magnitude.", new AcceptableValueRange<float>(0f, 2f)));

            BumpCooldownMs = config.Bind(
                "Wheel/FFB", "BumpCooldownMs", 60,
                new ConfigDescription("Minimum milliseconds between successive bump impulses.", new AcceptableValueRange<int>(0, 500)));

            CForceDampingScale = config.Bind(
                "Wheel/FFB", "CForceDampingScale", 0.4f,
                new ConfigDescription("Scales centripetal force into damper coefficient.", new AcceptableValueRange<float>(0f, 2f)));

            SmoothingAlpha = config.Bind(
                "Wheel/FFB", "SmoothingAlpha", 0.25f,
                new ConfigDescription("Exponential smoothing factor for FFB output (0 = no update, 1 = no smoothing).", new AcceptableValueRange<float>(0f, 1f)));

            // Wheel / Input
            SteerDeadzone = config.Bind(
                "Wheel/Input", "SteerDeadzone", 0.05f,
                new ConfigDescription("Normalised steer axis deadzone (0..1).", new AcceptableValueRange<float>(0f, 0.5f)));

            SteerSaturation = config.Bind(
                "Wheel/Input", "SteerSaturation", 1.0f,
                new ConfigDescription("Normalised steer axis saturation point (0..1). Values above this map to ±1.", new AcceptableValueRange<float>(0.1f, 1f)));
        }
    }
}
